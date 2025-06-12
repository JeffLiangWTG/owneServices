namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using HtmlAgilityPack;
    using iTextSharp;
    using iTextSharp.text.pdf;
    using iTextSharp.text.pdf.parser;
    using System.Text.RegularExpressions;

    public class US_MOOASUSPEN : NameUrlGrabber
    {
        private List<string> final = new List<string>();
        private List<string> raw = new List<string>();

        public List<string> GrabNames(string list_url, HtmlDocument document)
        {
            var derp = ReadPdfFile(list_url);

            Match m = Regex.Match(derp, "[0-9][0-9-][0-9A-Z] [0-9A-Z]");
            while(m.Success)
            {
                string temp = derp.Substring(m.Index);
                string extratemp = temp.Substring(4, 60);
                raw.Add(extratemp);
                m = m.NextMatch();
            }

            foreach (string s in raw)
            {
                Match m2 = Regex.Match(s, ", [A-Z][A-Z]");
                if (m2.Success)
                {
                    Console.WriteLine("Pass");
                    string temp = s.Substring(0, m2.Index);
                    final.Add(temp);
                }
                else
                    Console.WriteLine("Fail");
            }
            return final;
        }

        public string ReadPdfFile(string fileName)
        {
            StringBuilder text = new StringBuilder();

            if (File.Exists(fileName))
            {
                PdfReader pdfReader = new PdfReader(fileName);

                for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                    currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                    text.Append(currentText);
                    pdfReader.Close();
                }
            }
            return text.ToString();
        }
    }
}
