namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_GADASDEBAR : NameUrlGrabber
    {
        public List<string> GrabNames(String url, HtmlDocument document)
        {
            List<string> final = new List<string>();
            int count = 0;

            var nodes = document.DocumentNode.SelectNodes("//table[@border='0']//td[@class='jobdesc']");
            foreach (HtmlNode linkNode in nodes)
            {
                if (count % 5 == 2)
                    final.Add(linkNode.InnerText.Trim());
                count++;
            }
            return final;
        }
    }
}
