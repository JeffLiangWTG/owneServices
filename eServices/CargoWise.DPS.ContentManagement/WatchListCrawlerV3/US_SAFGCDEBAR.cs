namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_SAFGCDEBAR : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 
            var nodes = document.DocumentNode.SelectNodes("//table[@cellpadding='5']//td/a");
            
            foreach (HtmlNode linkNode in nodes)
            {
                string s = linkNode.InnerText.Trim();
                string[] temp;
                if (s.IndexOf('(') > -1)
                {
                    temp = s.Split('&');
                    tempName = temp[0].Trim();
                }
                tempUrl = "http://www.safgc.hq.af.mil" + linkNode.GetAttributeValue("href", "ERROR");
                try { List.Add(tempName, tempUrl); }
                catch { }
            }
            return List;
        }
    }
}
