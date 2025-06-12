namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;
    using System.Text.RegularExpressions;

    public class US_IRANSYRIA : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();
            int count = 0;

            var nodes = document.DocumentNode.SelectNodes("//td");
            foreach (HtmlNode linkNode in nodes)
            {
                switch (count % 5)
                {
                    case 1:
                        tempName = linkNode.InnerText.Trim();
                        break;
                    case 4:
                        tempUrl = getLink(linkNode.ChildNodes, url);
                        try { List.Add(tempName, tempUrl); }
                        catch { }
                        break;
                }
                count++;
            }
            return List;
        }

        private string getLink(HtmlNodeCollection collection, string url)
        {
            string temp = url;
            foreach (HtmlNode derp in collection)
            {
                if (derp.InnerText == "HTML" | derp.InnerText == "PDF")
                {
                    temp = derp.GetAttributeValue("href", url);
                    Console.WriteLine(temp);
                }

            }
            return temp;
        }
    }
}
