namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_MODOLDEBAR : NameUrlGrabber
    {
        private List<string> final = new List<string>();
        private List<string> temp = new List<string>();
        int count = 0;

        public List<string> GrabNames(string list_url, HtmlDocument document)
        {
            var nodes = document.DocumentNode.SelectNodes("//table//td");
            foreach (HtmlNode linkNode in nodes)
            {
                string s = linkNode.InnerText.Trim();
                if (count % 4 == 0)
                        temp.Add(s);
                count++;
            }

            foreach (string pre in temp)
            {
                string[] split = pre.Split(new Char[] { '\n' });
                string s = split[0];
                s = s.Replace("&amp;", "&");
                final.Add(s.Trim());
            }
            return final;
        }
    }
}
