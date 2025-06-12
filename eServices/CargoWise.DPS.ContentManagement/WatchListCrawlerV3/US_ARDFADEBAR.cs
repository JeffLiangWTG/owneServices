namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_ARDFADEBAR : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;
        int count = 0;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 
            var nodes = document.DocumentNode.SelectNodes("//table[@border='0']//td[@class='tableCell']");
            
            foreach (HtmlNode linkNode in nodes)
            {
                tempName = linkNode.InnerText.Trim();
                tempUrl = url;
                if (count % 3 == 0)
                    List.Add(tempName, tempUrl);
                count++;
            }
            return List;
        }
    }
}
