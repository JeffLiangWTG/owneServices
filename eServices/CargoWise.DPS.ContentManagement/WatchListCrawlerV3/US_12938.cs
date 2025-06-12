namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_12938 : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 
            int count = 0;

            var nodes = document.DocumentNode.SelectNodes("//table[@border='1']//td");
            foreach (HtmlNode linkNode in nodes)
            {
                switch (count % 5)
                {
                    case 0:
                        try { List.Add(tempName, tempUrl); }
                        catch { }
                        break;
                    case 1:
                        tempName = linkNode.InnerText.Trim();
                        break;
                    case 4:
                        Console.WriteLine(linkNode.ChildNodes);
                        tempUrl = getLink(linkNode.ChildNodes, url);
                        break;
                }
                count++;
                
            }
            try { List.Add(tempName, tempUrl); }
            catch { }
            return List;
        }

        private string getLink(HtmlNodeCollection collection, string url)
        {
            string temp = url;
            foreach (HtmlNode derp in collection)
            {
                if (derp.InnerText == "HTML")
                    temp = derp.GetAttributeValue("href", "ERROR");

            }
            return temp;
        }
    }
}
