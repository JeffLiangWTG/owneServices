namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;
    using System.Text.RegularExpressions;

    public class EB_ENTITIES : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 
            int count = 0;

            var nodes = document.DocumentNode.SelectNodes("//table[@class='table-b']//td[not(b)]");
            foreach (HtmlNode linkNode in nodes)
            {
                tempName = linkNode.InnerText.Trim();
                tempUrl = "http://www.ebrd.com/pages/about/integrity/list.shtml";
                tempName = unEscape(tempName);
                if(!tempName.Equals("\n"))
                    if(!Regex.IsMatch(tempName, "\\d"))
                        if (IsAllUpper(tempName))
                        {
                            try { List.Add(tempName, tempUrl); }
                            catch { }
                        }
                count++;
            }
            return List;
        }

        private bool IsAllUpper(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (Char.IsLetter(input[i]) && !Char.IsUpper(input[i]))
                    return false;
            }
            return true;
        }
    }
}
