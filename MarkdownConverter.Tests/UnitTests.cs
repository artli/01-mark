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
        public void UnpairedUnderscores() {
            TestConverter(
                "Unpaired underscores _should not count as proper formatting.\n" +
                "They should be left untouched.",

                "<p>Unpaired underscores _should not count as proper formatting.<br/>" +
                "They should be left untouched.</p>");
        }

        [TestMethod]
        public void UnderscoresInDifferentLinesAndParagraphs() {
            TestConverter(
                "_Underscores formatting should work\n" +
                "even if underscores are in different lines_\n" +
                "If they are in different paragraphs _though,\n" +
                "\n" +
                "they should not_ be treated as a pair. Instead_\n" +
                "the_ second underscore may be paired with another one in its paragraph.",

                "<p><em>Underscores formatting should work<br/>" +
                "even if underscores are in different lines</em><br/>" +
                "If they are in different paragraphs _though,</p>" +
                "<p>they should not<em> be treated as a pair. Instead</em><br/>" +
                "the_ second underscore may be paired with another one in its paragraph.</p>");
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

        [TestMethod]
        public void SimpleStrongFormatting() {
            TestConverter(
                "Text __enclosed in double underscores__ should __become __enclosed in__ strong __tags.",

                "<p>Text <strong>enclosed in double underscores</strong> should <strong>become </strong>enclosed in<strong> strong </strong>tags.</p>");
        }

        [TestMethod]
        public void StrongAndEmCombined() {
            TestConverter(
                "Strong _formatting defined __by __double_ underscores should work inside __single _underscores' _scope__ and vice versa.",

                "<p>Strong <em>formatting defined <strong>by </strong>double</em> underscores should work inside <strong>single <em>underscores' </em>scope</strong> and vice versa.</p>");
        }

        [TestMethod]
        public void ThreeUnderscoresInARow() {
            TestConverter(
                "Three ___underscores in a row\n" +
                "should mean both ___strong tag and em tag.\n" +
                "\n" +
                "The order ___in which__ the _tags appear depends on\n" +
                "which _tag __was opened first___ or ___will later _be__ closed first.",

                "<p>Three <em><strong>underscores in a row<br/>" +
                "should mean both </strong></em>strong tag and em tag.</p>" +
                "<p>The order <em><strong>in which</strong> the </em>tags appear depends on<br/>" +
                "which <em>tag <strong>was opened first</strong></em> or <strong><em>will later </em>be</strong> closed first.</p>");
        }

        [TestMethod]
        public void PartlyUnpairedThreeUnderscores() {
            TestConverter(
                "If one of the tags hidden behind three underscores is unpaired,\n" +
                "the underscore __left untouched ___is chosen so that\n" +
                "\n" +
                "it's not ___influenced by _other underscores' tags.",

                "<p>If one of the tags hidden behind three underscores is unpaired,<br/>" +
                "the underscore <strong>left untouched </strong>_is chosen so that</p>" +
                "<p>it's not __<em>influenced by </em>other underscores' tags.</p>");
        }

        [TestMethod]
        public void ManyUndersoresInARow() {
            TestConverter(
                "If there is an even number of underscores in a row (more than two),\n" +
                "the ____underscores____ should be ignored (they close themselves).\n" +
                "\n" +
                "If there's an odd number of underscores in a row (more than three),\n" +
                "all _____underscores but_ one _____should be ignored.",

                "<p>If there is an even number of underscores in a row (more than two),<br/>" +
                "the underscores should be ignored (they close themselves).</p>" +
                "<p>If there's an odd number of underscores in a row (more than three),<br/>" +
                "all <em>underscores but</em> one _should be ignored.</p>");
        }

        [TestMethod]
        public void ComplexUnderscoresUsage() {
            TestConverter(
                "All the rules described for single underscores usage\n" +
                "and it's interaction with escaping\n" +
                "should also work for multiple underscores.\n" +
                "\n" +
                "This is ___\\_an\\______ example\\\\_ of a__ text\n" +
                "With complex usage ___of_ u_nderscores.",

                "<p>All the rules described for single underscores usage<br/>" +
                "and it's interaction with escaping<br/>" +
                "should also work for multiple underscores.</p>" +
                "<p>This is <strong><em>_an_</em> example\\_ of a</strong> text<br/>" +
                "With complex usage __<em>of</em> u_nderscores.</p>");
        }
    }
}
