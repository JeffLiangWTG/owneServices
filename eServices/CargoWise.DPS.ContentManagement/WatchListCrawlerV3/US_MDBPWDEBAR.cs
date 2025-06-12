namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_MDBPWDEBAR : NameUrlGrabber
    {
        private List<string> final = new List<string>();
        int count = 0;

        public List<string> GrabNames(string list_url, HtmlDocument document)
        {
            var nodes = document.DocumentNode.SelectNodes("//table[@class='data']//td[@class='data']");
            foreach (HtmlNode linkNode in nodes)
            {
                string s = linkNode.InnerText.Trim();
                if (count % 2 == 0)
                        final.Add(s);
                count++;
            }
            return final;
        }
    }
}
