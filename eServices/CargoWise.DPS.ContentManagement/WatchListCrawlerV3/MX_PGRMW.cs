namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;
    using System.Text.RegularExpressions;

    public class MX_PGRMW : NameUrlGrabber
    {
        //private string tempName;
        //private string tempUrl;
        private List<string> categ = new List<string>();
        //private List<string> final = new List<string>();
        private HtmlWeb tempRemote = new HtmlWeb();
        private HtmlDocument temp = new HtmlDocument();

        public Dictionary<string, string> GrabNames(String url, HtmlDocument document)
        {
            Dictionary<string, string> List = new Dictionary<string, string>();
            //List<string> final = new List<string>();

            //Get number of pages
            int pages = getPages(url);
            
            //Get URLs for each page
            for (int i = 1; i <= pages; i++)
                categ.Add("http://www.pgr.gob.mx/Servicios/fugitivos/opera_consulta.asp?pagina=" + i + "&TamPagina=72&orden=nombre&estatus=A&sexo=A&tipo=img");

            //Grab names from each page and add to dictionary
            foreach(string urls in categ)
            {
                List<string> final = new List<string>();
                final.Clear();
                Console.WriteLine(url);
                //final = final.Union(getNames(urls, document));
                final = getNames(urls);

                foreach(string name in final)
                {
                    Console.WriteLine(name);
                    try { List.Add(name, urls); }
                    catch { }
                }
            }
            return List;            
        }

        private int getPages(string tempLink)
        {
            List<string> num = new List<string>();
            temp = tempRemote.Load(tempLink);
            var linkNodes = temp.DocumentNode.SelectNodes("//table//p");
            try
            {
                foreach (HtmlNode node in linkNodes)
                {
                    string s = node.InnerText;
                    if (s.Contains("Página"))
                        if (Regex.IsMatch(s, "\\d"))
                            num.Add(s.Substring(s.Length-1));
                }
            }
            catch (NullReferenceException)
            {
                num.Add("1");
            }
            return Convert.ToInt32(num[num.Count - 1]);
        }

        private List<string> getNames(string url)
        {
            List<string> names = new List<string>();
            temp = tempRemote.Load(url);
            var nodes = temp.DocumentNode.SelectNodes("//b/text()[(preceding::br)]");
            foreach (HtmlNode linkNode in nodes)
            {
                if (!Regex.IsMatch(linkNode.InnerText, "\\d"))
                        names.Add(linkNode.InnerText.Trim());
            }
            return names;
        }
    }
}
