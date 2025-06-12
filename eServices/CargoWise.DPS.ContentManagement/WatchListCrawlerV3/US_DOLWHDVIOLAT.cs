namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_DOLWHDVIOLAT : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();
            List<string> temp = new List<string>();
            int count = 0;
            tempUrl = url;

            var nodes = document.DocumentNode.SelectNodes("//table//td/p");
            foreach (HtmlNode linkNode in nodes)
            {
                string s = linkNode.InnerText.Trim();
                if (count % 4 == 0)
                {
                    tempName = s.Replace("\n", "");
                    try { List.Add(tempName, tempUrl); }
                    catch { }
                }       
                count++;
            }
            return List;
        }
    }
}
