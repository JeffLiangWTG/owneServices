namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_PADLIDEBAR : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 
            List<string> final = new List<string>();
            List<string> temp = new List<string>();
            int count = 0;
            tempUrl = url;

            var nodes = document.DocumentNode.SelectNodes("//table[@name='debarments']//td[not(div)]");
            foreach (HtmlNode linkNode in nodes)
            {
                if (count % 3 == 0)
                    temp.Add(linkNode.InnerText.Trim());
                count++;
            }
            foreach (string pre in temp)
            {
                string[] split = pre.Split(new Char[] { '\n' });
                tempName = split[0].Trim();
                try { List.Add(tempName, tempUrl); }
                catch { }
            }
            return List;
        }
    }
}
