namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class HK_SFCALERT : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 
            var nodes = document.DocumentNode.SelectNodes("//div[@class='fullMargin']//a");
            foreach (HtmlNode linkNode in nodes)
            {
                tempName = linkNode.InnerText.Trim();
                tempUrl = linkNode.GetAttributeValue("href", "ERROR");
                tempUrl = tempUrl.Substring(2);
                tempUrl = "http://www.invested.hk/InvestEdAlertList" + tempUrl;
                try { List.Add(tempName, tempUrl); }
                catch { }
            }
            return List;
        }
    }
}
