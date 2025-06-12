namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class AF_ENTITIES : NameUrlGrabber
    {
        //private bool details = false;
        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();
            int count = 0;

            var nodes = document.DocumentNode.SelectNodes("//div[@class='debarredList']/table//td");
            foreach (HtmlNode linkNode in nodes)
            {
                string s = linkNode.InnerText.Trim();
                if (count % 6 == 1)
                {
                    try
                    {
                        List.Add(s, url);
                    }
                    catch
                    { }
                }
                count++;
            }
            return List;
        }
    }
}
