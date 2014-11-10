using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownConverter {

    public class MarkdownConverter {
        public enum TokenType {
            Text,
            Linebreak,
            ParagraphBreak,
            EmFormatting,
        }

        public class Token : IEquatable<Token> {
            public TokenType Type;
            public string Text = "";

            public Token(TokenType type) {
                Type = type;
                switch (type) {
                    case TokenType.EmFormatting:
                        Text = "_";
                        break;
                    case TokenType.Linebreak:
                        Text = "\n";
                        break;
                    case TokenType.ParagraphBreak:
                        Text = "\n\n";
                        break;
                }
            }

            public Token(TokenType type, string text) {
                Type = type;
                Text = text;
            }

            public string GetTagName() {
                switch (Type) {
                    case TokenType.EmFormatting:
                        return "em";
                    case TokenType.Linebreak:
                        return "br";
                    case TokenType.ParagraphBreak:
                        return "p";
                    default:
                        return "";
                }
            }

            public Token GetTagText(bool isOpening) {
                if (Type == TokenType.Linebreak)
                    return new Token(TokenType.Text, "<br/>");

                var result = "<";
                if (!isOpening)
                    result += "/";
                result += GetTagName();
                result += ">";
                return new Token(TokenType.Text, result);
            }

            public bool Equals(Token second) {
                return this.Type == second.Type && this.Text == second.Text;
            }

            public string ToString() {
                return Text;
            }
        }

        public static int FirstMatchingIndex<T>(IEnumerable<T> list, Func<T, bool> predicate) {
            int index = 0;
            foreach (var elem in list) {
                if (predicate(elem))
                    break;
                index++;
            }
            return index;
        }

        public static string[] DivideWord(string word) {
            if (word == "")
                return new[] { "", "", "" };

            var startIndex = FirstMatchingIndex(word, sym => (sym != '_'));
            var firstPart = word.Substring(0, startIndex);
            word = word.Remove(0, startIndex);

            var endIndex = word.Length - FirstMatchingIndex(word.Reverse(), sym => (sym != '_'));
            var secondPart = word.Substring(0, endIndex);
            var thirdPart = word.Substring(endIndex);

            if (secondPart.Last() == '\\') {
                if (thirdPart.Length > 0) {
                    secondPart += thirdPart[0];
                    thirdPart = thirdPart.Remove(0, 1);
                }
            }

            return new[] {
                firstPart,
                secondPart,
                thirdPart
            };
        }

        public static IEnumerable<Token> GetFormattingTokens(string formattingSymbols) {
            if (formattingSymbols.Any(sym => sym != '_'))
                throw new ArgumentOutOfRangeException();

            return formattingSymbols.Select(sym => new Token(TokenType.EmFormatting));
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

        public static Token[] Tokenize(string text) {
            var tokens = new List<Token>();

            var paragraphs = text.Split(new[] { "\n\n" }, StringSplitOptions.None);
            for (var paragraph = 0; paragraph < paragraphs.Length; paragraph++) {
                var lines = paragraphs[paragraph].Split('\n');
                for (var line = 0; line < lines.Length; line++) {
                    var words = lines[line].Split(' ');
                    for (var word = 0; word < words.Length; word++) {
                        var divided = DivideWord(words[word]);

                        tokens.AddRange(GetFormattingTokens(divided[0]));
                        tokens.Add(new Token(TokenType.Text, Unescape(divided[1])));
                        tokens.AddRange(GetFormattingTokens(divided[2]));

                        if (word != words.Length - 1)
                            tokens.Add(new Token(TokenType.Text, " "));
                    }
                    if (line != lines.Length - 1)
                        tokens.Add(new Token(TokenType.Linebreak));
                }
                if (paragraph != paragraphs.Length - 1)
                    tokens.Add(new Token(TokenType.ParagraphBreak));
            }

            return tokens.ToArray();
        }

        public static string ConvertToHTML(string text) {
            var tokens = Tokenize(text);
            var result = new StringBuilder();

            var tagsWithIndices = new List<Tuple<Token, int>>();
            var paragraph = new List<Token>();
            foreach (var next in tokens) {
                switch (next.Type) {
                    case TokenType.Text:
                        paragraph.Add(next);
                        break;
                    case TokenType.Linebreak:
                        paragraph.Add(next.GetTagText(true));
                        break;
                    case TokenType.EmFormatting:
                        var sameTagIndex = tagsWithIndices.FindLastIndex(elem => elem.Item1.Equals(next));
                        if (sameTagIndex == -1) {
                            tagsWithIndices.Add(Tuple.Create(next, paragraph.Count));
                            paragraph.Add(next);
                        } else {
                            tagsWithIndices.RemoveRange(sameTagIndex + 1, tagsWithIndices.Count - sameTagIndex - 1);
                            var sameTag = tagsWithIndices.Last();
                            tagsWithIndices.RemoveAt(tagsWithIndices.Count - 1);
                            paragraph[sameTag.Item2] = next.GetTagText(true);
                            paragraph.Add(next.GetTagText(false));
                        }
                        break;
                    case TokenType.ParagraphBreak:
                        foreach (var tagIndexPair in Enumerable.Reverse(tagsWithIndices)) {
                            paragraph[tagIndexPair.Item2] = tagIndexPair.Item1.GetTagText(true);
                            paragraph.Add(tagIndexPair.Item1.GetTagText(false));
                        }
                        result.Append("<p>");
                        foreach (var token in paragraph)
                            result.Append(token.Text);
                        result.Append("</p>");

                        paragraph = new List<Token>();
                        for (int i = 0; i < tagsWithIndices.Count; i++) {
                            paragraph.Add(tagsWithIndices[i].Item1);
                            tagsWithIndices[i] = Tuple.Create(tagsWithIndices[i].Item1, i);
                        }
                        break;
                }
            }
            result.Append("<p>");
            foreach (var token in paragraph)
                result.Append(token.Text);
            result.Append("</p>");

            return result.ToString();
        }
    }
}
