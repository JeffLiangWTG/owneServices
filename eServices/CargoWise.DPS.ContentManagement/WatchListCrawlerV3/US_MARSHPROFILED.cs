namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;

    public class US_MARSHPROFILED : NameUrlGrabber
    {
        public List<string> GrabNames(String url, HtmlDocument document)
        {
            List<string> final = new List<string>(); 
            
            var nodes = document.DocumentNode.SelectNodes("//td/a");
            foreach (HtmlNode linkNode in nodes)
            {
                if (linkNode.InnerText.IndexOf(",") > -1)
                {
                    String s = linkNode.InnerText.Trim();
                    string[] derp = s.Split(new char[] { ',' });
                    String firstName = derp[1].Trim();
                    String lastName = derp[0].Trim();
                    final.Add(firstName + " " + lastName);
                }
            }
            final.Sort();
            return final;
        }
    }
}
