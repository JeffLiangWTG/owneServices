namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_IWSHIP : NameUrlGrabber
    {
        public List<string> GrabNames(String url, HtmlDocument document)
        {
            List<string> final = new List<string>();
            int count = 0;

            var nodes = document.DocumentNode.SelectNodes("//table[@border='1']//td");
            foreach (HtmlNode linkNode in nodes)
            {
                if (count % 8 == 7)
                    final.Add(linkNode.InnerText.Trim());
                count++;
            }
            return final;
        }
    }
}
