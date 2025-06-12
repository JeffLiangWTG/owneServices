namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_MTGSDDEBAR : NameUrlGrabber
    {
        public List<string> GrabNames(String url, HtmlDocument document)
        {
            List<string> final = new List<string>();
            int count = 0;

            var nodes = document.DocumentNode.SelectNodes("//table[@border='1']//td");
            foreach (HtmlNode linkNode in nodes)
            {
                if (count % 3 == 0 && !linkNode.HasAttributes)
                {
                    final.Add(linkNode.InnerText.Replace("\n", "").Trim());
                }
                    count++;
            }
            return final;
        }
    }
}
