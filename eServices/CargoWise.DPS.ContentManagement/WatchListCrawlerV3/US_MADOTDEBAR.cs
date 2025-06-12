namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;
    using System.Text.RegularExpressions;

    public class US_MADOTDEBAR : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();
            int count = 0;

            var nodes = document.DocumentNode.SelectNodes("//table[@class='dataTable']//td");
            foreach (HtmlNode linkNode in nodes)
            {
                tempName = linkNode.InnerText.Trim();
                tempUrl = url;
                if (count % 4 == 0)
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
