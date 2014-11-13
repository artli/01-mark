using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownConverter {
    public static class InterfaceExtensions {
        public static void Swap<T>(this IList<T> enumerable, int i1, int i2) {
            var temp = enumerable[i1];
            enumerable[i1] = enumerable[i2];
            enumerable[i2] = temp;
        }
    }


    public class MarkdownConverter {
        public enum TokenType {
            Text,
            Linebreak,
            ParagraphBreak,
            EmFormatting,
            StrongFormatting,
            EmAndStrong,
            CodeFormatting
        }

        public class Token : IEquatable<Token> {
            public TokenType Type;
            public string Text = "";

            public Token(TokenType type) {
                Type = type;
                switch (type) {
                    case TokenType.Linebreak:
                        Text = "\n";
                        break;
                    case TokenType.ParagraphBreak:
                        Text = "\n\n";
                        break;
                    case TokenType.EmFormatting:
                        Text = "_";
                        break;
                    case TokenType.StrongFormatting:
                        Text = "__";
                        break;
                    case TokenType.EmAndStrong:
                        Text = "___";
                        break;
                    case TokenType.CodeFormatting:
                        Text = "`";
                        break;
                }
            }

            public Token(TokenType type, string text) {
                Type = type;
                Text = text;
            }

            public string GetTagName() {
                switch (Type) {
                    case TokenType.Linebreak:
                        return "br";
                    case TokenType.ParagraphBreak:
                        return "p";
                    case TokenType.EmFormatting:
                        return "em";
                    case TokenType.StrongFormatting:
                        return "strong";
                    case TokenType.CodeFormatting:
                        return "code";
                    default:
                        return "";
                }
            }

            public Token TagAsText(bool isOpeningTag) {
                if (Type == TokenType.Linebreak)
                    return new Token(TokenType.Text, "<br/>");

                var result = "<";
                if (!isOpeningTag)
                    result += "/";
                result += GetTagName();
                result += ">";
                return new Token(TokenType.Text, result);
            }

            public bool Equals(Token second) {
                return this.Type == second.Type && this.Text == second.Text;
            }

            public override string ToString() {
                return Text;
            }
        }

        public static Token GetUnderscoreToken(int underscoresCount) {
            if (underscoresCount > 3)
                underscoresCount %= 2;

            if (underscoresCount == 0)
                return new Token(TokenType.Text, "");
            if (underscoresCount == 1)
                return new Token(TokenType.EmFormatting);
            else if (underscoresCount == 2)
                return new Token(TokenType.StrongFormatting);
            else
                return new Token(TokenType.EmAndStrong);
        }

        public static IEnumerable<Token> GetFormattingTokens(string formattingString) {
            var tokens = new List<Token>();
            var underscoresCount = 0;
            foreach (var sym in formattingString) {
                if (sym == '`') {
                    if (underscoresCount != 0)
                        tokens.Add(GetUnderscoreToken(underscoresCount));
                    underscoresCount = 0;
                    tokens.Add(new Token(TokenType.CodeFormatting));
                } else
                    underscoresCount++;
            }
            if (underscoresCount != 0)
                tokens.Add(GetUnderscoreToken(underscoresCount));
            return tokens;
        }

        public static string Unescape(string text) {
            var result = new List<char>();
            for (int i = 0; i < text.Length; i++) {
                if (text[i] == '\\') {
                    if (i != text.Length - 1 && text[i + 1] == '\\') {
                        result.Add('\\');
                        i++;
                    }
                } else
                    result.Add(text[i]);
            }
            return string.Join("", result);
        }

        public static IEnumerable<Token> TokenizeWord(string word) {
            if (word == "")
                return new List<Token>();

            var index = 0;
            while (index < word.Length) {
                var sym = word[index++];
                if (sym != '_' && sym != '`') {
                    index--;
                    break;
                }
            }

            var wordStart = index;
            var wordEnd = index;
            while (index < word.Length) {
                var sym = word[index++];
                if (sym != '_' && sym != '`')
                    wordEnd = index;
                if (sym == '\\')
                    wordEnd = Math.Min(++index, word.Length);
            }

            var tokens = new List<Token>();
            tokens.AddRange(GetFormattingTokens(word.Substring(0, wordStart)));
            tokens.Add(new Token(TokenType.Text, Unescape(word.Substring(wordStart, wordEnd - wordStart))));
            tokens.AddRange(GetFormattingTokens(word.Substring(wordEnd)));
            return tokens;
        }

        public static List<List<string>> GetParagraphs(string text) {
            var lines = text.Split('\n');
            var paragraphs = new List<List<string>>();

            var paragraph = new List<string>();
            bool newParagraphStarts = true;
            foreach (var line in lines) {
                if (newParagraphStarts) {
                    paragraph.Add(line);
                    newParagraphStarts = false;
                    continue;
                }

                if (line.All(char.IsWhiteSpace)) {
                    paragraphs.Add(paragraph);
                    paragraph = new List<string>();
                    newParagraphStarts = true;
                } else {
                    paragraph.Add(line);
                }
            }
            if (paragraph.Count != 0)
                paragraphs.Add(paragraph);

            return paragraphs;
        }

        public static Token[] Tokenize(string text) {
            var tokens = new List<Token>();

            var paragraphs = GetParagraphs(text);
            for (var paragraph = 0; paragraph < paragraphs.Count; paragraph++) {
                var codeTagsCount = 0;
                var lines = paragraphs[paragraph];
                for (var line = 0; line < lines.Count; line++) {
                    var words = lines[line].Split(' ');
                    for (var word = 0; word < words.Length; word++) {
                        var tokenizedWord = TokenizeWord(words[word]);
                        codeTagsCount += tokenizedWord.Count(token => token.Type == TokenType.CodeFormatting);
                        tokens.AddRange(tokenizedWord);

                        if (word != words.Length - 1)
                            tokens.Add(new Token(TokenType.Text, " "));
                    }
                    if (line != lines.Count - 1)
                        tokens.Add(new Token(TokenType.Linebreak));
                }
                if (paragraph != paragraphs.Count - 1)
                    tokens.Add(new Token(TokenType.ParagraphBreak));
                if (codeTagsCount % 2 == 1) {
                    var lastCodeTagIndex = tokens.FindLastIndex(token => token.Type == TokenType.CodeFormatting);
                    tokens[lastCodeTagIndex].Type = TokenType.Text;
                }
            }

            return tokens.ToArray();
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

        public static void FlushParagraph(List<Token> paragraph, StringBuilder html) {
            html.Append("<p>");
            foreach (var token in paragraph)
                html.Append(token.Text);
            html.Append("</p>");
        }

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
            var tokens = Tokenize(text);
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
    }
}
