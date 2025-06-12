namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_IWENTITIES : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();

            var nodes = document.DocumentNode.SelectNodes("//td//a");
            foreach (HtmlNode linkNode in nodes)
            {
                tempName = linkNode.InnerText.Trim();
                tempUrl = linkNode.GetAttributeValue("href", "ERROR");
                if (tempUrl.StartsWith("records"))
                    tempUrl = "http://www.iranwatch.org/suspect/" + tempUrl;
                else
                    tempUrl = "http://www.iranwatch.org" + tempUrl;
                List.Add(tempName, tempUrl);
            }
            return List;

        }
    }
}
