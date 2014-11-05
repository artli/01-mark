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
        public void BlankTest() {
            var inputText = "";
            var expectedResult = "";
            TestConverter(inputText, expectedResult);
        }
    }
}
