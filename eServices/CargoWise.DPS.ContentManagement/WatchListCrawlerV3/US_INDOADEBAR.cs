namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_INDOADEBAR : NameUrlGrabber
    {
        public List<string> GrabNames(String url, HtmlDocument document)
        {
            List<string> final = new List<string>();
            int count = 0;

            var nodes = document.DocumentNode.SelectNodes("//table//td[not(*)]");
            foreach (HtmlNode linkNode in nodes)
            {
                string s = linkNode.InnerText.Trim();
                s = s.Replace("&amp;", "&");
                if (count % 2 == 0)
                    final.Add(s);
                count++;
            }
            return final;
        }
    }
}
