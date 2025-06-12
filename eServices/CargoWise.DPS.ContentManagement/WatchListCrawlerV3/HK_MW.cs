namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class HK_MW : NameUrlGrabber
    {
        //private string tempName;
        //private string tempUrl;
        private List<string> names = new List<string>();
        private List<string> urls = new List<string>();

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();

            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//font/text()[(preceding::br)]");
            HtmlNodeCollection nodes_link = document.DocumentNode.SelectNodes("//a[@class='smalltext']");

            getNames(nodes);
            getUrls(nodes_link);

            for (int i = 0; i < urls.Count - 1; i++)
            {
                try { List.Add(names[i], urls[i]); }
                catch { }
            }
            return List;
        }

        private void getNames(HtmlNodeCollection nodes)
        {
            string lastName = "";
            string firstName = "";
            int count = 0;

            foreach (HtmlNode linkNode in nodes)
            {
                if (linkNode.InnerText.IndexOf(",") == -1)
                {
                    if (count % 2 == 0)
                        lastName = linkNode.InnerText.Trim();
                    else
                    {
                        firstName = linkNode.InnerText.Trim();
                        names.Add(firstName.Trim() + " " + lastName.Trim());
                    }
                    count++;
                }
            }
        }

        private void getUrls(HtmlNodeCollection nodes)
        {
            foreach (HtmlNode linkNode in nodes)
                urls.Add("http://www.icac.org.hk" + linkNode.GetAttributeValue("href", "ERROR"));
        }
    }
}
