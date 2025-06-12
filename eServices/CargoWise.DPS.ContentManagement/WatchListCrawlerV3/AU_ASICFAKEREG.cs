namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class AU_ASICFAKEREG : NameUrlGrabber
    {
        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();
            var nodes = document.DocumentNode.SelectNodes("//div[@id='content']//li");
            foreach (HtmlNode linkNode in nodes)
            {
                string tempName = linkNode.InnerText.Trim();
                if(!tempName.Contains("&gt;"))
                {
                        tempName = unEscape(tempName);
                        string tempUrl = linkNode.FirstChild.GetAttributeValue("href", "ERROR");
                        if (tempUrl.StartsWith("/scams/"))
                        {
                            if (tempUrl.Equals("ERROR"))
                            tempUrl = "";
                        try { List.Add(tempName, "https://www.moneysmart.gov.au" + tempUrl); }
                        catch { }
                    }
                }
            }
            return List;
        }
    }
}
