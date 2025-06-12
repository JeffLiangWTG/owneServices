namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class HK_SFCWANTED : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();

            var nodes = document.DocumentNode.SelectNodes("//table[@class='noborders']//text()");
            foreach (HtmlNode linkNode in nodes)
            {
                string s = linkNode.InnerText.Trim();
                tempUrl = url;
                if (s.StartsWith("Name:"))
                {
                    tempName = s.Substring(6);
                    try { List.Add(tempName, tempUrl); }
                    catch { }
                }
            }
            return List;
        }
    }
}
