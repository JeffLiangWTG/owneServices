namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_ILIGBEPL : NameUrlGrabber
    {
        private List<string> final = new List<string>();
        
        public List<string> GrabNames(string list_url, HtmlDocument document)
        {
            var nodes = document.DocumentNode.SelectNodes("//table[@border='1']//td//a");
            foreach (HtmlNode linkNode in nodes)
            {
                string s = linkNode.InnerText.Trim();
                s = s.Replace("\n", "");
                final.Add(s);
            }
            return final;
        }
    }
}
