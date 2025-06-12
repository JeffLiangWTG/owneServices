namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class WB_FIRMS : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 
            int count = 0;

            var nodes = document.DocumentNode.SelectNodes("//table[@class='tableBorderGrey']//td/font[@size='1']");
            foreach (HtmlNode linkNode in nodes)
            {
                if (count % 6 == 0)
                {
                    tempName = linkNode.InnerText.Trim();
                    tempUrl = url;
                    try { List.Add(tempName, tempUrl); }
                    catch { }
                }
                count++;
            }

            var nodesMore = document.DocumentNode.SelectNodes("//table[@border='1']//strong");
            foreach (HtmlNode linkNode in nodesMore)
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
