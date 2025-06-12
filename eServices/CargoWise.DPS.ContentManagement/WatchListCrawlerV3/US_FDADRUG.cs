namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_FDADRUG : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();

            var nodes = document.DocumentNode.SelectNodes("//table[@border='1']//td");
            foreach (HtmlNode linkNode in nodes)
            {
                if (linkNode.InnerText.IndexOf(",") > -1)
                {
                    String s = linkNode.InnerText.Trim();
                    string[] derp = s.Split(new char[] { ',' });
                    String firstName = derp[1].Trim();
                    String lastName = derp[0].Trim();
                    tempName = firstName + " " + lastName;
                    tempUrl = url;
                    try { List.Add(tempName, tempUrl); }
                    catch { }
                }

            }
            return List;
        }
    }
}
