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

    public class UK_TERGRP : NameUrlGrabber
    {
        private List<string> final = new List<string>();
        private List<string> raw = new List<string>();

        public List<string> GrabNames(string list_url, HtmlDocument document)
        {
            bool flag = false;

            raw = ReadPdfFile(list_url);
            foreach (string s in raw)
            {
                if (flag)
                {
                    if (!(s.Contains("Note") || s.Contains(".") || s.Contains(",") || s.Trim().Equals("")))
                    {
                        final.Add(s.Trim());
                        flag = false;
                    }
                }
                else
                {
                    if (s.Trim().Equals(""))
                        flag = true;
                }
            }
            return final.Skip(8).Take(final.Count - 8).ToList();
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
