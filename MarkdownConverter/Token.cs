using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownConverter {
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

            var openingFormat = "<{0}>";
            var closingFormat = "</{0}>";

            if (isOpeningTag)
                return new Token(TokenType.Text, string.Format(openingFormat, GetTagName()));
            else
                return new Token(TokenType.Text, string.Format(closingFormat, GetTagName()));
        }

        public bool Equals(Token second) {
            return this.Type == second.Type && this.Text == second.Text;
        }

        public override string ToString() {
            return Text;
        }
    }
}
