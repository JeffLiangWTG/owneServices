namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_MOOADEBAR : NameUrlGrabber
    {
        private List<string> final = new List<string>();
        private List<string> temp = new List<string>();
        int count = 0;

        public List<string> GrabNames(string list_url, HtmlDocument document)
        {
            var nodes = document.DocumentNode.SelectNodes("//table[@border='1']//td/p");
            foreach (HtmlNode linkNode in nodes)
            {
                string s = linkNode.InnerText.Trim();
                if (count % 2 == 0)
                        temp.Add(s);
                count++;
            }

            foreach (string pre in temp)
            {
                string[] split = pre.Split(new Char[] { '\n' });
                final.Add(split[0].Trim());
            }
            return final;
        }
    }
}
