namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_HSMW : NameUrlGrabber
    {
        public List<string> GrabNames(String url, HtmlDocument document)
        {
            List<string> final = new List<string>();

            var nodes = document.DocumentNode.SelectNodes("//a/h3");
            foreach (HtmlNode linkNode in nodes)
            {
                final.Add(linkNode.InnerText.Trim());
            }
            final.Sort();
            return final;
        }
    }
}
