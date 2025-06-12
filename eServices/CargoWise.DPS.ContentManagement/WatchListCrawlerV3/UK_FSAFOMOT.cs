namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class UK_FSAFOMOT : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();
            List<string> names = new List<string>();
            List<string> urls = new List<string>();
            int count = 0;

            var nodes = document.DocumentNode.SelectNodes("//table[@class='table-horizontal-line']//td");
            foreach (HtmlNode linkNode in nodes)
            {
                switch (count % 3)
                {
                    case 0:
                        try { List.Add(tempName, tempUrl); }
                        catch { }
                        break;                    
                    case 1:
                        tempName = unEscape(linkNode.InnerText).Trim();
                        break;
                    case 2:
                        tempUrl = linkNode.FirstChild.GetAttributeValue("href", "ERROR");
                        break;
                }
                count++;
            }
            return List;
        }
    }
}
