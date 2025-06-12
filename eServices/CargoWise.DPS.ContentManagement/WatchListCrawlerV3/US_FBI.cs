namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_FBI : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 
            var nodes = document.DocumentNode.SelectNodes("//dt[@class='contenttype-FBIPerson']/a");
            foreach (HtmlNode linkNode in nodes)
            {
                tempName = linkNode.InnerText.Trim();
                tempUrl = linkNode.GetAttributeValue("href", "ERROR"); ;
                List.Add(tempName, tempUrl);
            }
            return List;
        }
    }
}
