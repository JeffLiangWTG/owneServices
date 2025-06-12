namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class SE_FINSALERTS : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();
            int count = 0;

            var nodes = document.DocumentNode.SelectNodes("//table[@id='alert-table']//td");
            foreach (HtmlNode linkNode in nodes)
            {
                switch (count % 3)
                {
                    case 0:
                        tempName = linkNode.InnerText.Trim();
                        break;
                    case 1:
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
            temp = collection[0].GetAttributeValue("href", "ERROR");
            return temp;
        }
    }
}
