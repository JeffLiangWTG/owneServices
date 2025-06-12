namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class CY_CYSECWARN : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 

            var nodes = document.DocumentNode.SelectNodes("//table//td/a");
            foreach (HtmlNode linkNode in nodes)
            {
                string s = linkNode.InnerText.Trim();
                if (s.Contains("Commission"))
                {
                    s = s.Substring(53);
                    if (s.StartsWith("\'"))
                        s = s.Substring(1);
                    tempName = s.Trim();
                    tempUrl = "http://www.cysec.gov.cy" + linkNode.GetAttributeValue("href", "ERROR");
                    try { List.Add(tempName, tempUrl); }
                    catch { }
                }
            }
            return List;
        }
    }
}