namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;
    using System.Text.RegularExpressions;

    public class US_WVDOADEBAR : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();
            List<string> temp = new List<string>();

            var nodes = document.DocumentNode.SelectNodes("//td[@class='style2']/blockquote/p");
            foreach (HtmlNode linkNode in nodes)
            {
                temp.Add(linkNode.InnerText.Trim());
                tempUrl = url;
            }

            foreach (string pre in temp)
            {
                string[] split = pre.Split(new Char[] { '\n' });
                string s = split[0];
                if (!Regex.IsMatch(s, "\\d"))
                {
                    tempName = s.Trim();
                    try { List.Add(tempName, tempUrl); }
                    catch { }
                }
            }
            return List;
        }
    }
}
