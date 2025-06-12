namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_DEAMIF : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 
            
            var nodes = document.DocumentNode.SelectNodes("//font/a");
            foreach (HtmlNode linkNode in nodes)
            {
                if (linkNode.InnerText.IndexOf(",") > -1)
                {
                    tempName = unEscape(linkNode.InnerText.Trim());
                    string[] derp = tempName.Split(new char[] { ',' });
                    String firstName = derp[1].Trim();
                    String lastName = derp[0].Trim();
                    tempName = firstName + " " + lastName;
                    tempUrl = "http://www.justice.gov/dea/fugitives/internl/" + linkNode.GetAttributeValue("href", "ERROR");
                    try { List.Add(tempName, tempUrl); }
                    catch { }
                }
            }
            return List;
        }
    }
}
