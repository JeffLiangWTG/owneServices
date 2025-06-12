namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_FCEN311 : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();
            int count = 0;

            var nodes = document.DocumentNode.SelectNodes("//table[@class='RuleMaking311']//td");
            foreach (HtmlNode linkNode in nodes)
            {
                tempName = linkNode.InnerText.Trim();
                tempUrl = url;
                if (count % 5 == 0 && !linkNode.HasAttributes)
                {
                    try { List.Add(tempName, tempUrl); }
                    catch { }
                }
                count++;
            }
            return List;
        }
    }
}
