using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MarkdownConverter;

namespace ConsoleApp {
    class Program {
        private const string consoleHelp = "Help:\nYou should provide exactly one argument: the path to the input file.\n" +
            "'.html' is appended to your filename and the output file with the resulting name is created in the working directory.";
        private const string fileNameError = "First argument should be a valid file path.";

        private static string[] ReadData(string dataFilePath) {
            return File.ReadAllLines(dataFilePath, Encoding.GetEncoding(1251));
        }
        private static void WriteData(string dataFilePath, string data) {
            File.WriteAllText(dataFilePath, data, Encoding.GetEncoding(1251));
        }

        static void Main(string[] args) {
            if (args.Length != 1 || args[0] == "/?") {
                Console.WriteLine(consoleHelp);
                return;
            }

            string[] lines;
            try {
                lines = ReadData(args[0]);
            } catch (System.IO.IOException e) {
                Console.WriteLine(fileNameError);
                Console.WriteLine(e.Message);
                return;
            }

            var html = MarkdownConverter.MarkdownConverter.ConvertToHTML(string.Join("\n", lines));
            var outputFileName = args[0].Split('\\').Last() + ".html";
            WriteData(outputFileName, html);
        }
    }
}
