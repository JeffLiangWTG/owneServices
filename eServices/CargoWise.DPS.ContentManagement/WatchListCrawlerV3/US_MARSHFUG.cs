namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_MARSHFUG : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 

            var nodes = document.DocumentNode.SelectNodes("//table//table[@border='0']//td[@valign='top']//a");
            foreach (HtmlNode linkNode in nodes)
            {
                tempName = linkNode.InnerText.Trim();
                tempUrl = "http://www.usmarshals.gov/investigations/major_cases/" + linkNode.GetAttributeValue("href", "ERROR");
                if(!(tempName.Contains("USMS") | tempName.Contains ("usms") | tempName.Equals("")))
                try { List.Add(tempName, tempUrl); }
                catch { }
            }
            return List;
        }
    }
}
