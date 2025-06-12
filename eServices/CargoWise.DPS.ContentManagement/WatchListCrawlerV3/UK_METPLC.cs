namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;
    using System.Text.RegularExpressions;

    public class UK_METPLC : NameUrlGrabber
    {
        private List<string> final = new List<string>();
        private List<string> category = new List<string>();
        private List<string> profiles = new List<string>();
        private List<string> moreProf = new List<string>();
        private List<string> names = new List<string>();
        private HtmlWeb tempRemote = new HtmlWeb();
        private HtmlDocument temp = new HtmlDocument();
        private string tempName;
        private string tempUrl;

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>(); 
            //Get categories
            category = getCateg(document);

            for (int i = 0; i < category.Count; i++)
            {
                //Get number of pages and then grab profile links
                int page = getPages("http://londonmostwanted.crimestoppers-uk.org/search/1/" + category[i] + "/87");
                for (int j = 1; j <= page; j++)
                {
                    moreProf = getProfiles("http://londonmostwanted.crimestoppers-uk.org/search/" + j + "/" + category[i] + "/87");
                    profiles = profiles.Union(moreProf).ToList();
                }

                //Get names from profiles for current category
                foreach (string fool in profiles)
                {
                    tempName = getName(fool);
                    tempUrl = "http://londonmostwanted.crimestoppers-uk.org" + fool;
                    try { List.Add(tempName, tempUrl); }
                    catch { }
                }

                //Write to final list with category headings
                final.Add("******************** " + category[i].ToUpper());

                foreach (string temp in names)
                    if (!temp.Equals(""))
                        final.Add(temp);

                //Clear lists
                profiles.Clear();
                names.Clear();
            }        
            return List;
        }

        private List<string> getCateg(HtmlDocument document)
        {
            List<string> temp = new List<string>();
            var nodes = document.DocumentNode.SelectNodes("//a[@class='blueschemelink']");
            foreach (HtmlNode linkNode in nodes)
            {
                if (!linkNode.InnerHtml.Trim().Equals("Home"))
                {
                    string s = linkNode.GetAttributeValue("href", "ERROR");
                    string [] split = s.Split(new Char [] {'/'});
                    temp.Add(split[5]);
                }
            }
            return temp;
        }

        private List<string> getProfiles(string cat)
        {
            List<string> prof = new List<string>();
            temp = tempRemote.Load(cat);
            var subNodes = temp.DocumentNode.SelectNodes("//a[@class='mw_captionlink']");
            foreach (HtmlNode sublinkNodes in subNodes)
            {
                String s = sublinkNodes.GetAttributeValue("href", "ERROR");
                if (s.StartsWith("/"))
                    profiles.Add(s);
            }
            return prof;
        }

        private string getName(string prof)
        {
            string name = "";
            string tempLink = "http://londonmostwanted.crimestoppers-uk.org" + prof;
            temp = tempRemote.Load(tempLink);
            var subsubNodes = temp.DocumentNode.SelectNodes("//td/div[@class='mw_suspectlayoutbold']");
            foreach (HtmlNode subsublinkNodes in subsubNodes)
            {
                if (!Regex.IsMatch(subsublinkNodes.InnerText, "\\d"))
                    if (subsublinkNodes.InnerText.Trim().IndexOf(" ") > -1)
                        if (subsublinkNodes.InnerText.Trim().IndexOf(",") == -1)
                            if(!subsublinkNodes.InnerText.Trim().Contains("Metropolitan"))
                                if(!subsublinkNodes.InnerText.Trim().Contains("Centre"))
                                    if(!subsublinkNodes.InnerText.Trim().Contains("Other"))
                                        name = subsublinkNodes.InnerText;
            }
            return name;
        }

        private int getPages(string tempLink)
        {
            List<string> num = new List<string>();
            temp = tempRemote.Load(tempLink);
            var subsubNodes = temp.DocumentNode.SelectNodes("//a/em");
            try
            {
                foreach (HtmlNode subsublinkNodes in subsubNodes)
                {
                    if (Regex.IsMatch(subsublinkNodes.InnerText, "\\d"))
                        num.Add(subsublinkNodes.InnerText);
                }
            }
            catch (NullReferenceException)
            {
                num.Add("1");
            }
            return Convert.ToInt32(num[num.Count - 1]);
        }
    }
}
