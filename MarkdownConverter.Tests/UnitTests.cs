using System;
using MarkdownConverter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownConverter.Tests {
    [TestClass]
    public class UnitTests {
        public void TestConverter(string inputText, string expectedResult) {
            var result = MarkdownConverter.ConvertToHTML(inputText);
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void BlankLine() {
            TestConverter("", "<p></p>");
        }

        [TestMethod]
        public void OneParagraphWithASingleLine() {
            TestConverter(
                "Single line should be enclosed in the paragraph tags.",
                "<p>Single line should be enclosed in the paragraph tags.</p>");
        }

        [TestMethod]
        public void OneParagraphWithTwoLines() {
            TestConverter(
                "Lines separated by a linebreak\n" +
                "should become separated by the linebreak tag.",

                "<p>Lines separated by a linebreak<br/>" +
                "should become separated by the linebreak tag.</p>");
        }

        [TestMethod]
        public void OneParagraphWithMultipleLines() {
            TestConverter(
                "There may be many lines\n" +
                "in a single paragraph.\n" +
                "It shouldn't break our converter.",

                "<p>There may be many lines<br/>" +
                "in a single paragraph.<br/>" +
                "It shouldn't break our converter.</p>");
        }

        [TestMethod]
        public void MultipleParagraphs() {
            TestConverter(
                "A sequence of two linebreaks, one after another,\n" +
                "means that a new paragraph starts.\n" +
                "\n" +
                "Each paragraph should be enclosed in the paragraph tags.",

                "<p>A sequence of two linebreaks, one after another,<br/>" +
                "means that a new paragraph starts.</p>" +
                "<p>Each paragraph should be enclosed in the paragraph tags.</p>");
        }

        [TestMethod]
        public void ManyLinebreaksInARow() {
            TestConverter(
                "A sequence of 2n+1 linebreaks\n" +
                "\n" + 
                "\n" +
                "\n" +
                "\n" +
                "results in n new paragraphs and a linebreak in the beginning of the last one.\n" +
                "If there's an even number of linebreaks in a row,\n" +
                "\n" +
                "only new paragraphs are created.",

                
                "<p>A sequence of 2n+1 linebreaks</p>" +
                "<p></p>" +
                "<p><br/>" +
                "results in n new paragraphs and a linebreak in the beginning of the last one.<br/>" +
                "If there's an even number of linebreaks in a row,</p>" +
                "<p>only new paragraphs are created.</p>");
        }

        [TestMethod]
        public void UnderscoresInTheSameLine() {
            TestConverter(
                "_Text enclosed in underscores should become enclosed in em tags._",

                "<p><em>Text enclosed in underscores should become enclosed in em tags.</em></p>");
        }

        [TestMethod]
        public void UnderscoresInDifferentLinesAndParagraphs() {
            TestConverter(
                "_That should work\n" +
                "even if underscores are in different lines_ _or\n" +
                "\n" +
                "even paragraphs._\n" +
                "In _that_ case additional em tags may be added to ensure that tags don't intersect.",

                "<p><em>That should work<br/>" +
                "even if underscores are in different lines</em> <em>or</em></p>" +
                "<p><em>even paragraphs.</em><br/>" +
                "In <em>that</em> case additional em tags may be added to ensure that tags don't intersect.</p>");
        }

        [TestMethod]
        public void UnpairedUnderscores() {
            TestConverter(
                "Unpaired underscores _should not count as proper formatting.\n" +
                "They should be left as is.",

                "<p>Unpaired underscores _should not count as proper formatting.<br/>" +
                "They should be left as is.</p>");
        }

        [TestMethod]
        public void UnderscoresInsideWords() {
            TestConverter(
                "Underscores _i_nside words_ or_ connecting_words _also shouldn't count as _forma_tting.",

                "<p>Underscores <em>i_nside words</em> or<em> connecting_words </em>also shouldn't count as _forma_tting.</p>");
        }

        [TestMethod]
        public void SimpleEscaping() {
            TestConverter(
                "A slash before a symbol escapes that symbol.\n" +
                "An escaped symbol should not be counted as a formatting symbol.\n" +
                "For example, escaped \\_underscores\\_ should remain underscores\n" +
                "Any other escaped \\symb\\ol should also remain the same",

                "<p>A slash before a symbol escapes that symbol.<br/>" +
                "An escaped symbol should not be counted as a formatting symbol.<br/>" +
                "For example, escaped _underscores_ should remain underscores<br/>" +
                "Any other escaped symbol should also remain the same</p>");
        }

        [TestMethod]
        public void TwoSlashesInARow() {
            TestConverter(
                "Two slashes in a row (\\\\) should become one slash in the result.",

                "<p>Two slashes in a row (\\) should become one slash in the result.</p>");
        }

        [TestMethod]
        public void EndOfLineSlash() {
            TestConverter(
                "A slash in the end of a line should be ignored\\\n" +
                "This includes the case of a slash in the end of all text.\\",

                "<p>A slash in the end of a line should be ignored<br/>" +
                "This includes the case of a slash in the end of all text.</p>");
        }
        
        [TestMethod]
        public void ComplexEscapingExample() {
            TestConverter(
                "This is \\_an example\\\\\\_ of a text\n" +
                "\\With symbols \\'escaped _in_ multiple\\' places\\.\\",

                "<p>This is _an example\\_ of a text<br/>" +
                "With symbols 'escaped <em>in</em> multiple' places.</p>");
        }

        [TestMethod]
        public void UnderscoresAfterEscaping() {
            TestConverter(
                "Symbols in a word after an escaped symbol should be treated\n" +
                "as symbols inside a word: _\\__asdf_\\__",

                "<p>Symbols in a word after an escaped symbol should be treated<br/>" +
                "as symbols inside a word: <em>__asdf__</em></p>");
        }
    }
}
