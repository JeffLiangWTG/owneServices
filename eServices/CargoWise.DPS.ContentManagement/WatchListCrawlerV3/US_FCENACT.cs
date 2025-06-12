namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_FCENACT : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 

            var nodes = document.DocumentNode.SelectNodes("//table[@class='enforcement_action']//a");
            foreach (HtmlNode linkNode in nodes)
            {
                tempName = linkNode.InnerText.Trim();
                tempUrl = linkNode.GetAttributeValue("href", "ERROR");
                try { List.Add(tempName, tempUrl); }
                catch { }
            }
            return List;
        }
    }
}
