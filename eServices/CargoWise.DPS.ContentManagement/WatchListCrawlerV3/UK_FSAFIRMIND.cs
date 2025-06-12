namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class UK_FSAFIRMIND : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;
        private string knull = null;

        private List<string> names = new List<string>();
        private List<string> links = new List<string>();

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();
            var nodes = document.DocumentNode.SelectNodes("//div[not(@*)]/p[(preceding::br)]/text() | //div[not(@*)]/p/a");
            foreach (HtmlNode linkNode in nodes)
            {
                tempName = linkNode.InnerText.Trim();
                tempUrl = url;
                if(linkNode.OuterHtml.Contains("target=\""))
                    tempUrl = "http://www.fsa.gov.uk" + linkNode.GetAttributeValue("href", "ERROR");
                if (!(tempName.Equals("") || tempName.StartsWith("(") || tempName.StartsWith(")") || tempName.StartsWith(".") || tempName.StartsWith("/") || tempName.StartsWith("- ")))
                {
                    try { List.Add(unEscape(tempName), tempUrl); }
                    catch { }
                }
            }
            return List;
        }
    }
}
