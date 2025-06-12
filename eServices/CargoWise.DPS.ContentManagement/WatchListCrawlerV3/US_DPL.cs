namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_DPL : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();
            int count = 0;

            var nodes = document.DocumentNode.SelectNodes("//table[@border='0']//td[@scope='row']/text()");
            foreach (HtmlNode linkNode in nodes)
            {
                tempName = linkNode.InnerText.Trim();
                tempUrl = "url";
                if (count % 2 == 0)
                {
                    try { List.Add(tempName, tempUrl); }
                    catch { }
                }
                count++;
            }
            return List;
        }
    }
}
