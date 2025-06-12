namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;
    using System.Text.RegularExpressions;

    public class US_CHEMSANC : NameUrlGrabber
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
                    case 2:
                        tempName = linkNode.InnerText.Trim();
                        break;
                    case 0:
                        tempUrl = getLink(linkNode.ChildNodes, url);
                        try { List.Add(tempName, tempUrl); }
                        catch { }
                        break;
                    default:
                        Console.WriteLine("DERP");
                        break;
                }
                count++;
            }
            return List;
        }

        private bool IsAllUpper(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (Char.IsLetter(input[i]) && !Char.IsUpper(input[i]))
                    return false;
            }
            return true;
        }

        private string getLink(HtmlNodeCollection collection, string url)
        {
            string temp = url;
            foreach (HtmlNode derp in collection)
            {
                if (derp.InnerText == "HTML" | derp.InnerText == "PDF")
                {
                    temp = derp.GetAttributeValue("href", "ERROR");
                    Console.WriteLine(temp);
                }

            }
            return temp;
        }
    }
}
