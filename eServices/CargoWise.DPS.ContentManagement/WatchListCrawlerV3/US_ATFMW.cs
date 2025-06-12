namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_ATFMW : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();

            var nodes = document.DocumentNode.SelectNodes("//p/a");
            foreach (HtmlNode linkNode in nodes)
            {
                tempName = linkNode.InnerText.Trim();
                if (tempName.Contains("Download"))
                {
                    tempName = tempName.Substring(9);
                    tempName = tempName.Substring(0, tempName.Length - 22);
                    tempUrl = "http://www.atf.gov/most-wanted/" + linkNode.GetAttributeValue("href", "ERROR");
                    List.Add(tempName, tempUrl);
                }
            }
            return List;
        }
    }
}
