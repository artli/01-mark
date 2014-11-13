using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownConverter {
    public static class IEnumerableExtension {
        public static int FirstMatchingIndex<T>(this IEnumerable<T> list, Func<T, bool> predicate) {
            int index = 0;
            foreach (var elem in list) {
                if (predicate(elem))
                    break;
                index++;
            }
            return index;
        }
    }


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

        public static IEnumerable<Token> GetFormattingTokens(string formattingString) {
            if (formattingString.Any(sym => sym != '_'))
                throw new ArgumentOutOfRangeException();

            return formattingString.Select(sym => new Token(TokenType.EmFormatting));
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
                if (sym != '_') {
                    index--;
                    break;
                }
            }

            var wordStart = index;
            var wordEnd = index;
            while (index < word.Length) {
                var sym = word[index++];
                if (sym != '_')
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

        public static Token[] Tokenize(string text) {
            var tokens = new List<Token>();

            var paragraphs = text.Split(new[] { "\n\n" }, StringSplitOptions.None);
            for (var paragraph = 0; paragraph < paragraphs.Length; paragraph++) {
                var lines = paragraphs[paragraph].Split('\n');
                for (var line = 0; line < lines.Length; line++) {
                    var words = lines[line].Split(' ');
                    for (var word = 0; word < words.Length; word++) {
                        tokens.AddRange(TokenizeWord(words[word]));

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
            var html = new StringBuilder();

            var paragraph = new List<Token>();
            var tagsWithParagraphPositions = new List<Tuple<Token, int>>();
            foreach (var next in tokens) {
                switch (next.Type) {
                    case TokenType.Text:
                        paragraph.Add(next);
                        break;
                    case TokenType.Linebreak:
                        paragraph.Add(next.TagAsText(true));
                        break;
                    case TokenType.EmFormatting:
                        var sameTagIndex = tagsWithParagraphPositions.FindLastIndex(elem => elem.Item1.Equals(next));
                        if (sameTagIndex == -1) {
                            tagsWithParagraphPositions.Add(Tuple.Create(next, paragraph.Count));
                            paragraph.Add(next);
                        } else {
                            tagsWithParagraphPositions.RemoveRange(sameTagIndex + 1, tagsWithParagraphPositions.Count - (sameTagIndex + 1));
                            var sameTag = tagsWithParagraphPositions.Last();
                            tagsWithParagraphPositions.RemoveAt(tagsWithParagraphPositions.Count - 1);
                            paragraph[sameTag.Item2] = next.TagAsText(true);
                            paragraph.Add(next.TagAsText(false));
                        }
                        break;
                    case TokenType.ParagraphBreak:
                        foreach (var tagWithParagraphPosition in Enumerable.Reverse(tagsWithParagraphPositions)) {
                            paragraph[tagWithParagraphPosition.Item2] = tagWithParagraphPosition.Item1.TagAsText(true);
                            paragraph.Add(tagWithParagraphPosition.Item1.TagAsText(false));
                        }
                        html.Append("<p>");
                        foreach (var token in paragraph)
                            html.Append(token.Text);
                        html.Append("</p>");

                        paragraph = new List<Token>();
                        for (int i = 0; i < tagsWithParagraphPositions.Count; i++) {
                            paragraph.Add(tagsWithParagraphPositions[i].Item1);
                            tagsWithParagraphPositions[i] = Tuple.Create(tagsWithParagraphPositions[i].Item1, i);
                        }
                        break;
                }
            }
            html.Append("<p>");
            foreach (var token in paragraph)
                html.Append(token.Text);
            html.Append("</p>");

            return html.ToString();
        }
    }
}
