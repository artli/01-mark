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

            var result = new StringBuilder();
            var index = -1;
            while (++index < text.Length) {
                var c = text[index];
                if (specialSymbols.ContainsKey(c))
                    result.Append(specialSymbols[c]);
                else
                    result.Append(c);
            }
            return result.ToString();
        }

        public static string ConvertToHTML(string text) {
            text = ReplaceSpecialSymbols(text);
            var tokens = MarkdownTokenizer.Tokenize(text);
            var html = new StringBuilder();

            var paragraph = new List<Token>();
            var openingTagsWithPositions = new List<Tuple<Token, int>>();
            bool enclosedInCodeTags = false;
            foreach (var next in tokens) {
                switch (next.Type) {
                    case TokenType.Text:
                        paragraph.Add(next);
                        break;
                    case TokenType.Linebreak:
                        paragraph.Add(next.TagAsText(true));
                        break;
                    case TokenType.ParagraphBreak:
                        FlushParagraph(paragraph, html);
                        paragraph = new List<Token>();
                        openingTagsWithPositions = new List<Tuple<Token, int>>();
                        break;
                    case TokenType.CodeFormatting:
                        enclosedInCodeTags = !enclosedInCodeTags;
                        paragraph.Add(next.TagAsText(enclosedInCodeTags));
                        break;
                    default:
                        if (!enclosedInCodeTags)
                            ProcessTag(paragraph, openingTagsWithPositions, next);
                        else {
                            next.Type = TokenType.Text;
                            paragraph.Add(next);
                        }
                        break;
                }
            }
            FlushParagraph(paragraph, html);

            return html.ToString();
        }

        public static void FlushParagraph(List<Token> paragraph, StringBuilder html) {
            html.Append("<p>");
            foreach (var token in paragraph)
                html.Append(token.Text);
            html.Append("</p>");
        }

        public static int FindOpeningTag(List<Tuple<Token, int>> openingTagsWithPositions, Token next) {
            var tokensToFind = new List<Token>();
            if (next.Type == TokenType.EmAndStrong) {
                tokensToFind.Add(new Token(TokenType.EmFormatting));
                tokensToFind.Add(new Token(TokenType.StrongFormatting));
            } else
                tokensToFind.Add(next);

            return openingTagsWithPositions.FindLastIndex(elem => tokensToFind.Contains(elem.Item1));
        }

        public static void AddOpeningTag(List<Token> paragraph, List<Tuple<Token, int>> openingTagsWithPositions, Token next) {
            var tokensToAdd = new List<Token>();
            if (next.Type == TokenType.EmAndStrong) {
                tokensToAdd.Add(new Token(TokenType.EmFormatting));
                tokensToAdd.Add(new Token(TokenType.StrongFormatting));
            } else
                tokensToAdd.Add(next);

            foreach (var token in tokensToAdd) {
                openingTagsWithPositions.Add(Tuple.Create(token, paragraph.Count));
                paragraph.Add(token);
            }
        }

        public static void CloseTag(List<Token> paragraph, List<Tuple<Token, int>> openingTagsWithPositions, int openingTagIndex, Token next) {
            var openingTagParagraphIndex = openingTagsWithPositions[openingTagIndex].Item2;
            if (next.Type == TokenType.EmFormatting) {
                if (openingTagParagraphIndex != paragraph.Count - 1 && paragraph[openingTagParagraphIndex + 1].Type == TokenType.StrongFormatting) {
                    paragraph.Swap(openingTagParagraphIndex, openingTagParagraphIndex + 1);
                    var first = openingTagsWithPositions[openingTagIndex];
                    var second = openingTagsWithPositions[openingTagIndex + 1];
                    openingTagsWithPositions[openingTagIndex] = Tuple.Create(second.Item1, second.Item2 - 1);
                    openingTagsWithPositions[openingTagIndex + 1] = Tuple.Create(first.Item1, first.Item2 + 1);
                    openingTagIndex++;
                    openingTagParagraphIndex++;
                }
            }
            var openingTag = openingTagsWithPositions[openingTagIndex];
            openingTagsWithPositions.RemoveRange(openingTagIndex, openingTagsWithPositions.Count - openingTagIndex);
            paragraph[openingTagParagraphIndex] = next.TagAsText(true);
            paragraph.Add(next.TagAsText(false));
        }

        public static void ProcessTag(List<Token> paragraph, List<Tuple<Token, int>> openingTagsWithPositions, Token next) {
            var openingTagIndex = FindOpeningTag(openingTagsWithPositions, next);
            if (openingTagIndex == -1)
                AddOpeningTag(paragraph, openingTagsWithPositions, next);
            else {
                if (next.Type == TokenType.EmAndStrong) {
                    Token tagToClose, tagToProcess;
                    if (openingTagsWithPositions[openingTagIndex].Item1.Type == TokenType.StrongFormatting) {
                        tagToClose = new Token(TokenType.StrongFormatting);
                        tagToProcess = new Token(TokenType.EmFormatting);
                    } else {
                        tagToClose = new Token(TokenType.EmFormatting);
                        tagToProcess = new Token(TokenType.StrongFormatting);
                    }
                    CloseTag(paragraph, openingTagsWithPositions, openingTagIndex, tagToClose);
                    ProcessTag(paragraph, openingTagsWithPositions, tagToProcess);
                } else
                    CloseTag(paragraph, openingTagsWithPositions, openingTagIndex, next);
            }
        }
    }
}
