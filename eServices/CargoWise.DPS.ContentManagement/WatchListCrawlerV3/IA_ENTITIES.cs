namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class IA_ENTITIES : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();
            int count = 0;

            var nodes = document.DocumentNode.SelectNodes("//table//td/span[@class='smaller']");
            foreach (HtmlNode linkNode in nodes)
            {
                if (count % 7 == 0)
                {
                    tempName = linkNode.InnerText.Trim();
                    tempUrl = "http://www.iadb.org/en/topics/transparency/integrity-at-the-idb-group/sanctioned-firms-and-individuals,1293.html";
                    try { List.Add(tempName, tempUrl); }
                    catch { }
                }
                count++;
            }
            return List;
        }
    }
}
