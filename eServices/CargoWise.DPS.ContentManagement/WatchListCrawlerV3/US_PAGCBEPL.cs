namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;
    using System.Text.RegularExpressions;

    public class US_PAGCBEPL : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 
            List<string> final = new List<string>();
            List<string> temp = new List<string>();
            string tempFName = "";
            string tempLName = "";
            int count = 0;

            var nodes = document.DocumentNode.SelectNodes("//table[@border='0']//td");
            foreach (HtmlNode linkNode in nodes)
            {
                switch (count % 6)
                {
                    case 3:
                        tempFName = linkNode.InnerText.Trim();
                        break;
                    case 4:
                        tempLName = linkNode.InnerText.Trim();
                        break;
                    case 5:
                        tempUrl = "http://gamingcontrolboard.pa.gov" + linkNode.ChildNodes[0].GetAttributeValue("href", "ERROR");
                        tempName = tempFName + " " + tempLName;
                        try { List.Add(tempName, tempUrl); }
                        catch { }
                        break;
                }
                count++;
            }
            return List;
        }
    }
}
