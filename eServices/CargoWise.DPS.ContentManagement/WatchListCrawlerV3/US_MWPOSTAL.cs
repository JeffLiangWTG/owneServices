namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;
    using System.Text.RegularExpressions;

    public class US_MWPOSTAL : NameUrlGrabber
    {
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 

            var nodes = document.DocumentNode.SelectNodes("//font/text()[(preceding::br)]");
            foreach (HtmlNode linkNode in nodes)
            {
                String s = linkNode.InnerText;
                s = s.Trim();
                if (s.IndexOf("Reward") == -1)
                {
                    if (s.IndexOf(",") != -1)
                    {
                        if (!IsAllUpper(s))
                        {
                            String code = (s.Remove(0, s.Length - 2));
                            if (!isStateAbbreviation(code))
                            {
                                if (!Regex.IsMatch(s, "\\d"))
                                {
                                    if (!Regex.IsMatch(s, "&nbsp"))
                                    {
                                        s = s.Trim();
                                        string[] derp = s.Split(new char[] { ',' });
                                        String firstName = derp[1].Trim();
                                        String lastName = derp[0].Trim();
                                        tempName = firstName + " " + lastName;
                                        tempUrl = linkNode.ParentNode.ParentNode.GetAttributeValue("href", "ERROR");
                                        if(tempUrl.StartsWith("/radDocs"))
                                            tempUrl = "https://postalinspectors.uspis.gov" + tempUrl;
                                        try { List.Add(tempName, tempUrl); }
                                        catch { }
                                    }
                                }
                            }
                        }
                    }
                }
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
