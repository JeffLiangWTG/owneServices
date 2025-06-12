namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_DOLWHDDEBAR : NameUrlGrabber
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
                if (count % 3 == 0)
                        temp.Add(s);
                count++;
            }

            foreach (string pre in temp)
            {
                string[] split = pre.Split(new Char[] { '\n' });
                string s = split[0];
                tempName = s.Replace("&amp;", "&").Trim();
                try { List.Add(tempName, tempUrl); }
                catch { }
            }
            return List;
        }
    }
}
