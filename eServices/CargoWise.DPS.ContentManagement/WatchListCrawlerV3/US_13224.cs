namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;
    using System.Text.RegularExpressions;

    public class US_13224 : NameUrlGrabber
    {
        //private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();
            tempUrl = url;

            var nodes = document.DocumentNode.SelectNodes("//div[@id='centerblock']/p/text()[(preceding::br)]");
            foreach (HtmlNode linkNode in nodes)
            {
                string s = linkNode.InnerText.Trim();
                if(!Regex.IsMatch(s, "\\d"))
                    if(!(s.StartsWith("(") || s.StartsWith(".")))
                        if(!(s.Equals("") || s.Equals("ANNEX")))
                            if (!(s.Contains("OFAC") || s.Contains("U.S") || s.Contains("Executive") || s.Contains("GEORGE W.") || s.Contains(":")))
                            {
                                try { List.Add(s, tempUrl);}
                                catch { }
                            }
            }
            return List;
        }
    }
}
