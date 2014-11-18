using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownConverter {
    public class MarkdownConverter {
        public static string ReplaceSpecialSymbols(string text) {
            var specialSymbols = new Dictionary<char, string>()
            {
                {'<', "&lt;"},
                {'>', "&gt;"},
                {'&', "&amp;"}
            };

            return string.Join("", text.Select(
                sym => specialSymbols.ContainsKey(sym) ?
                        specialSymbols[sym] :
                        new string(sym, 1)));
        }

        public static string ConvertToHTML(string text) {
            text = ReplaceSpecialSymbols(text);
            var tokens = MarkdownTokenizer.Tokenize(text);
            var html = new StringBuilder();

            var currentParagraph = new List<Token>();
            var openingTagsWithPositions = new List<Tuple<Token, int>>();
            bool enclosedInCodeTags = false;
            foreach (var token in tokens) {
                switch (token.Type) {
                    case TokenType.Text:
                        currentParagraph.Add(token);
                        break;
                    case TokenType.Linebreak:
                        currentParagraph.Add(token.TagAsText(true));
                        break;
                    case TokenType.ParagraphBreak:
                        FlushParagraphIntoHTML(currentParagraph, html);
                        currentParagraph = new List<Token>();
                        openingTagsWithPositions = new List<Tuple<Token, int>>();
                        break;
                    case TokenType.CodeFormatting:
                        enclosedInCodeTags = !enclosedInCodeTags;
                        currentParagraph.Add(token.TagAsText(enclosedInCodeTags));
                        break;
                    default:
                        if (!enclosedInCodeTags)
                            ProcessTag(currentParagraph, openingTagsWithPositions, token);
                        else {
                            token.Type = TokenType.Text;
                            currentParagraph.Add(token);
                        }
                        break;
                }
            }
            FlushParagraphIntoHTML(currentParagraph, html);

            return html.ToString();
        }

        public static void FlushParagraphIntoHTML(List<Token> paragraph, StringBuilder html) {
            html.Append("<p>");
            foreach (var token in paragraph)
                html.Append(token.Text);
            html.Append("</p>");
        }

        public static void ProcessTag(List<Token> currentParagraph, List<Tuple<Token, int>> openingTagsWithPositions, Token token) {
            var openingTagIndex = FindOpeningTag(openingTagsWithPositions, token);
            if (openingTagIndex == -1)
                AddOpeningTag(currentParagraph, openingTagsWithPositions, token);
            else {
                if (token.Type == TokenType.EmAndStrong) {
                    Token tagToClose, tagToProcess;
                    if (openingTagsWithPositions[openingTagIndex].Item1.Type == TokenType.StrongFormatting) {
                        tagToClose = new Token(TokenType.StrongFormatting);
                        tagToProcess = new Token(TokenType.EmFormatting);
                    } else {
                        tagToClose = new Token(TokenType.EmFormatting);
                        tagToProcess = new Token(TokenType.StrongFormatting);
                    }
                    CloseTag(currentParagraph, openingTagsWithPositions, openingTagIndex, tagToClose);
                    ProcessTag(currentParagraph, openingTagsWithPositions, tagToProcess);
                } else
                    CloseTag(currentParagraph, openingTagsWithPositions, openingTagIndex, token);
            }
        }

        public static int FindOpeningTag(List<Tuple<Token, int>> openingTagsWithPositions, Token token) {
            var tokensToFind = new List<Token>();
            if (token.Type == TokenType.EmAndStrong) {
                tokensToFind.Add(new Token(TokenType.EmFormatting));
                tokensToFind.Add(new Token(TokenType.StrongFormatting));
            } else
                tokensToFind.Add(token);

            return openingTagsWithPositions.FindLastIndex(elem => tokensToFind.Contains(elem.Item1));
        }

        public static void AddOpeningTag(List<Token> currentParagraph, List<Tuple<Token, int>> openingTagsWithPositions, Token token) {
            var tokensToAdd = new List<Token>();
            if (token.Type == TokenType.EmAndStrong) {
                tokensToAdd.Add(new Token(TokenType.EmFormatting));
                tokensToAdd.Add(new Token(TokenType.StrongFormatting));
            } else
                tokensToAdd.Add(token);

            foreach (var tokenToAdd in tokensToAdd) {
                openingTagsWithPositions.Add(Tuple.Create(tokenToAdd, currentParagraph.Count));
                currentParagraph.Add(tokenToAdd);
            }
        }

        public static void CloseTag(List<Token> currentParagraph, List<Tuple<Token, int>> openingTagsWithPositions, int openingTagIndex, Token token) {
            var openingTagIndexInParagraph = openingTagsWithPositions[openingTagIndex].Item2;
            if (token.Type == TokenType.EmFormatting) {
                if (openingTagIndexInParagraph != currentParagraph.Count - 1 && currentParagraph[openingTagIndexInParagraph + 1].Type == TokenType.StrongFormatting) {
                    currentParagraph.Swap(openingTagIndexInParagraph, openingTagIndexInParagraph + 1);
                    var first = openingTagsWithPositions[openingTagIndex];
                    var second = openingTagsWithPositions[openingTagIndex + 1];
                    openingTagsWithPositions[openingTagIndex] = Tuple.Create(second.Item1, second.Item2 - 1);
                    openingTagsWithPositions[openingTagIndex + 1] = Tuple.Create(first.Item1, first.Item2 + 1);
                    openingTagIndex++;
                    openingTagIndexInParagraph++;
                }
            }
            var openingTag = openingTagsWithPositions[openingTagIndex];
            openingTagsWithPositions.RemoveRange(openingTagIndex, openingTagsWithPositions.Count - openingTagIndex);
            currentParagraph[openingTagIndexInParagraph] = token.TagAsText(true);
            currentParagraph.Add(token.TagAsText(false));
        }
    }
}
