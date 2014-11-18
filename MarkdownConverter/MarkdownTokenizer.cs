using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownConverter {
    public class MarkdownTokenizer {
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

        public static List<List<string>> GetParagraphs(string text) {
            var lines = text.Split('\n');
            var paragraphs = new List<List<string>>();

            var currentParagraph = new List<string>();
            bool newParagraphStarts = true;
            foreach (var line in lines) {
                if (newParagraphStarts) {
                    currentParagraph.Add(line);
                    newParagraphStarts = false;
                    continue;
                }

                if (line.All(char.IsWhiteSpace)) {
                    paragraphs.Add(currentParagraph);
                    currentParagraph = new List<string>();
                    newParagraphStarts = true;
                } else
                    currentParagraph.Add(line);
            }
            if (currentParagraph.Count != 0)
                paragraphs.Add(currentParagraph);

            return paragraphs;
        }

        public static IEnumerable<Token> TokenizeWord(string word) {
            if (word == "")
                return new List<Token>();

            var symbolsNotStartingAWord = new[] { '_', '`', '.', ',', ':', ';', '\'' };

            var index = 0;
            while (index < word.Length) {
                var sym = word[index++];
                if (!symbolsNotStartingAWord.Contains(sym)) {
                    index--;
                    break;
                }
            }

            var wordStart = index;
            var wordEnd = index;
            while (index < word.Length) {
                var sym = word[index++];
                if (!symbolsNotStartingAWord.Contains(sym))
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

        public static IEnumerable<Token> GetFormattingTokens(string formattingString) {
            var tokens = new List<Token>();
            var underscoresCount = 0;
            foreach (var sym in formattingString) {
                if (sym == '_')
                    underscoresCount++;
                else {
                    if (underscoresCount != 0) {
                        tokens.Add(GetUnderscoreToken(underscoresCount));
                        underscoresCount = 0;
                    }

                    if (sym == '`')
                        tokens.Add(new Token(TokenType.CodeFormatting));
                    else
                        tokens.Add(new Token(TokenType.Text, new String(sym, 1)));
                }
            }
            if (underscoresCount != 0)
                tokens.Add(GetUnderscoreToken(underscoresCount));
            return tokens;
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
    }
}
