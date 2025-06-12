namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_NCISNAVY : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();
            List<string> names = new List<string>();
            List<string> urls = new List<string>();

            var nodes = document.DocumentNode.SelectNodes("//font/strong/text()[(preceding::br)]");
            foreach (HtmlNode linkNode in nodes)
            {
                tempName = linkNode.InnerText.Trim();
                int pos = tempName.IndexOf("Wanted For:");
                if (pos > -1)
                    tempName = tempName.Remove(pos, 11).Trim();
                if (!(tempName.Contains("People?") | tempName.Equals("")))
                    names.Add(tempName);
            }

            var nodel = document.DocumentNode.SelectNodes("//table//font[@size='2']/a[(*)]");
            foreach (HtmlNode linkNode in nodel)
            {
                tempUrl = linkNode.GetAttributeValue("href", "ERROR");
                urls.Add("http://www.ncis.navy.mil" + tempUrl);
            }

            for (int i = 0; i < names.Count; i++)
            {
                try { List.Add(names[i], urls[i+1]); }
                catch { }
            }
            return List;
        }
    }
}
