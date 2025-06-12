namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_AIRFUG : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 
            List<string> final = new List<string>();

            var nodes = document.DocumentNode.SelectNodes("//td/center/a[(following::br)]");
            foreach (HtmlNode linkNode in nodes)
            {
                tempName = unEscape(linkNode.InnerText.Trim());
                tempUrl = "http://www.osi.andrews.af.mil" + linkNode.GetAttributeValue("href", "ERROR");
                try { List.Add(tempName, tempUrl); }
                catch { }
            }
            return List;
        }
    }
}
