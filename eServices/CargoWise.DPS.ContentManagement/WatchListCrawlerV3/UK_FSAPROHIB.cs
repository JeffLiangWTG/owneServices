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

    public class UK_FSAPROHIB : NameUrlGrabber
    {
        private List<string> final = new List<string>();
        private List<string> raw = new List<string>();

        //private string tempName;
        //private string tempUrl;

        string list_url;
        HtmlDocument document;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 
            this.list_url = url;
            this.document = document;
            if (list_url.Contains(".pdf"))
                fsaLocal();
            else
                fsaRemote();
            return List;
        }

        private void fsaLocal()
        {
            raw = ReadPdfFile(list_url);
            foreach (string s in raw)
            {
                Match m = Regex.Match(s, " [0-9][0-9]/[0-9][0-9]/[0-9][0-9][0-9][0-9]");
                if (m.Success)
                {
                    string temp = s.Substring(0, m.Index);
                    if(!temp.ToUpper().Contains("HTTP"))
                        if(!temp.Contains(":"))
                            final.Add(temp);
                }
            }
        }

        private void fsaRemote()
        {
            var nodes = document.DocumentNode.SelectNodes("//td/a");
            foreach (HtmlNode linkNode in nodes)
            {
                final.Add(linkNode.InnerText.Trim());
            }
        }

        public List<string> ReadPdfFile(string fileName)
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
            string temp = text.ToString();
            string[] split = temp.Split(new Char[] { '\n' });
            return new List<string>(split);
        }
    }
}
