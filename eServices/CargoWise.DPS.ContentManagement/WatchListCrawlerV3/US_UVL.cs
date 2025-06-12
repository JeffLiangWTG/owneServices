namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_UVL : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 
            var nodes = document.DocumentNode.SelectNodes("//td/strong");
            foreach (HtmlNode linkNode in nodes)
            {
                tempName = linkNode.InnerText.Trim();
                tempUrl = url;
                try { List.Add(tempName, tempUrl); }
                catch { }
            }
            return List;
        }
    }
}
