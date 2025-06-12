namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_NJDGEEPL : NameUrlGrabber
    {
        private List<string> sites = new List<string>();
        private HtmlWeb docRemote = new HtmlWeb();
        private HtmlDocument doc = new HtmlDocument();

        //private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 
            genUrls();
            foreach (string urls in sites)
            {
                doc = docRemote.Load(urls);
                getNames(doc, List);
            }
            return List;
        }

        private void genUrls()
        {
            for (int i = 0; i < alpha.Length; i++)
                sites.Add("http://www.nj.gov/oag/ge/exclusion/exclusion_list/exclude_" + alpha[i] + ".htm");
        }

        private void getNames(HtmlDocument document, Dictionary<string, string> final)
        {
            var nodes = document.DocumentNode.SelectNodes("//table[@align='center']//td/a[not(@class='footerLink') and not(@class='footerLinksmall')]");
            int count = 0;
            string tempLName = "";
            string tempFName = "";

            foreach (HtmlNode linkNode in nodes)
            {
                string tempName = linkNode.InnerText.Trim();
                if (!tempName.Equals(""))
                {
                    if (count % 2 == 0)
                        tempLName = tempName;
                    else
                    {
                        tempFName = tempName;
                        tempUrl = "http://www.nj.gov/oag/ge/exclusion" + linkNode.GetAttributeValue("href", "ERROR").Substring(2);
                        final.Add(tempFName + " " + tempLName, tempUrl);
                    }
                }
                count++;
            }
        }
    }
}
