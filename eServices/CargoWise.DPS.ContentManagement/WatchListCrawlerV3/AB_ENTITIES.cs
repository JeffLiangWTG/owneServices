namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;
    using System.Text.RegularExpressions;

    public class AB_ENTITIES : NameUrlGrabber
    {
        //private string tempName;
        //private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 
            int count = 0;

            var nodes = document.DocumentNode.SelectNodes("//div[@id='viewcontainer']/table//td");
            foreach (HtmlNode linkNode in nodes)
            {
                if (count % 4 == 1)
                {
                    string s = linkNode.InnerText.Trim();
                    if (!Regex.IsMatch(s, "\\d"))
                    {
                        try { List.Add(s, url); }
                        catch { }
                    }
                }
                count++;
            }
            return List;
        }
    }
}
