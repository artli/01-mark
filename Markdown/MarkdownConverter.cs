using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownConverter {
    public class MarkdownConverter {
        public static string ConvertToHTML(string text) {
            var allParagraphs = text.Split(new string[] { "\n\n" }, StringSplitOptions.None)
                .Select(paragraph => "<p>" + paragraph.Replace("\n", "<br/>") + "</p>");

            return String.Join("", allParagraphs);
        }
    }
}
