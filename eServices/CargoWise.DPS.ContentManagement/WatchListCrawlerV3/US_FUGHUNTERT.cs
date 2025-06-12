namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;
    using System.Text.RegularExpressions;

    public class US_FUGHUNTERT : NameUrlGrabber
    {
        //private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 

            var nodes = document.DocumentNode.SelectNodes("//p/strong/text()[(preceding::br)]");
            foreach (HtmlNode linkNode in nodes)
            {
                string s = linkNode.InnerText.Trim();
                tempUrl = url;

                if (!s.Contains("Conspiracy") && !s.Contains("conspiracy") && !s.Contains("Murder") && !s.Contains("murder") && !s.Contains("Wanted") && !s.Contains("wanted") && !s.Contains("Terrorist") && !s.Contains("terrorist"))
                {
                    try { List.Add(s, tempUrl); }
                    catch { }
                }
            }
            return List;
        }
    }
}
