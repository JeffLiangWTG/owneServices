namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class AU_ASICUNLIC : NameUrlGrabber
    {
        private List<string> urls = new List<string>();
        private HtmlWeb docRemote = new HtmlWeb();
        private HtmlDocument doc = new HtmlDocument();
        private Dictionary<string, string> List = new Dictionary<string, string>();
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            genUrls();
            foreach (string s in urls)
            {
                doc = docRemote.Load(s);
                getNames(doc);
            }
            return List;
        }

        private void genUrls()
        {
            for (int i = 0; i < alpha.Length; i++)
                urls.Add("https://www.moneysmart.gov.au/scams/companies-you-should-not-deal-with/unlicensed-companies-list/" + alpha[i]);
        }

        private void getNames(HtmlDocument document)
        {
            var nodes = document.DocumentNode.SelectNodes("//h2/a");
            try
            {
                foreach (HtmlNode linkNode in nodes)
                {
                    tempName = linkNode.InnerText.Trim();
                    tempUrl = linkNode.GetAttributeValue("href", "ERROR");
                    tempUrl = "https://www.moneysmart.gov.au" + tempUrl;
                    try { List.Add(tempName, tempUrl); }
                    catch { }
                }
            }
            catch (NullReferenceException)
            { }
        }
    }
}
