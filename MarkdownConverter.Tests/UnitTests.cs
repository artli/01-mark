using System;
using MarkdownConverter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownConverter.Tests {
    [TestClass]
    public class UnitTests {
        public void TestConverter(string inputText, string expectedResult) {
            Assert.AreEqual(expectedResult, MarkdownConverter.ConvertToHTML(inputText));
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
    }
}
