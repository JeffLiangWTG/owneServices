namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_ILDOLDEBAR : NameUrlGrabber
    {
        private List<string> final = new List<string>();
        private List<string> temp = new List<string>();
        
        public List<string> GrabNames(string list_url, HtmlDocument document)
        {
            var nodes = document.DocumentNode.SelectNodes("//pre");
            foreach (HtmlNode linkNode in nodes)
            {
                temp.Add(linkNode.InnerText.Trim());
            }

            foreach (string pre in temp)
            {
                string[] split = pre.Split(new Char[] { '\n' });
                final.Add(split[0].Trim());
            }
            return final;
        }
    }
}
