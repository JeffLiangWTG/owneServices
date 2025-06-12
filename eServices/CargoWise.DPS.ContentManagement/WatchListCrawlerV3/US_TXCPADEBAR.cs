namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;
    using System.Text.RegularExpressions;

    public class US_TXCPADEBAR : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();

            var nodes = document.DocumentNode.SelectNodes("//table[@class='debar']//td/text()");
            foreach (HtmlNode linkNode in nodes)
            {
                tempName = linkNode.InnerText.Trim();
                tempUrl = url;
                if (!Regex.IsMatch(tempName, "\\d"))
                {
                    try { List.Add(tempName, tempUrl); }
                    catch { }
                }
            }
            return List;
        }
    }
}
