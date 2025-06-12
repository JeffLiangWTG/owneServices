using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using UtilityLibrary;
using Core.Strings;
using System.Data;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using Parsing.CommandLineParsing;
using System.Threading;

namespace CargoWise.DPS.ContentManagement
{
    public class US_DEAMW_Html2Text : Command
    {
        private static string foxTable = "dea_most_wanted_fugitives";
        private static string foxConnector = @"Provider=vfpoledb.1;Data Source={0}";

        public bool? Verbose { get; set; }
        public string HtmlFile { get; set; }
        public string XmlFile { get; set; }
        public string TextFile { get; set; }
        public string DbfDir { get; set; }
        public string DbfFile { get; set; }
        public string Uri { get; set; }


        public override void Execute()
        {


            DateTime start;
            DateTime end;
            TimeSpan duration;


            foxTable = DbfFile ?? foxTable;
            foxConnector = DbfDir != null ? String.Format(foxConnector, DbfDir) : String.Format(foxConnector, @"K:\Denied Global\foxtables");
            string currentUrl = ""; 
            OleDbConnection foxConn = null;
            List<DivisionHrefPair> areas = new List<DivisionHrefPair>();
            Dictionary<String, DivisionCriminalPair> fugsInArea = new Dictionary<String, DivisionCriminalPair>();

            XElement xRoot = new XElement("DpsList");



            HtmlDocument doc = new HtmlDocument();
            doc.OptionFixNestedTags = true;
            HtmlDocument fragmentDoc = new HtmlDocument();
            HtmlDocument tempDoc = new HtmlDocument();
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
            IWebProxy theProxy = client.Proxy;
            if (theProxy != null)
            {
                theProxy.Credentials = CredentialCache.DefaultCredentials;
            }

            string root = Uri.Substring(0, Uri.LastIndexOf('/'));
            string fileName = Path.GetTempFileName();

            if (HtmlFile == null)
            {
                client.DownloadFile(Uri, fileName);
                doc.LoadHtml(File.ReadAllText(fileName));
            }
            else
            {
                doc.LoadHtml(File.ReadAllText(HtmlFile));
                File.Delete(fileName);
            }
            File.WriteAllText(TextFile, "");
            File.WriteAllText(Path.ChangeExtension(TextFile, "log"), "" );


            RegexOptions options = RegexOptions.None;
            Regex regex1 = new Regex(@"\s+", options);   // all white-spaces
            Regex regex2 = new Regex(@"[ ]{2,}", options); // just spaces
            Regex regex3 = new Regex(@"^\s*$\n", RegexOptions.Multiline ); // multiple empty line


            try
            {

                start = DateTime.Now;
                foxConn = new OleDbConnection(foxConnector);
                foxConn.Open();
                OleDbCommand commandZap = foxConn.CreateCommand();
                commandZap.CommandType = CommandType.StoredProcedure;
                commandZap.CommandText = "ExecScript";
                OleDbParameter parm = commandZap.CreateParameter();
                parm.OleDbType = OleDbType.Char;
                commandZap.Parameters.Add(parm);

                parm.Value =
"use " + foxTable + @" exclusive 
ZAP
USE";
                commandZap.ExecuteScalar();

                OleDbCommand foxcmd = new OleDbCommand("Insert ", foxConn);
                OleDbParameter pField1 = new OleDbParameter("@id", OleDbType.Integer);
                OleDbParameter pField2 = new OleDbParameter("@division", OleDbType.Char);
                OleDbParameter pField3 = new OleDbParameter("@nam", OleDbType.BSTR);
                OleDbParameter pField4 = new OleDbParameter("@nam2", OleDbType.Char);
                OleDbParameter pField5 = new OleDbParameter("@dtlurl", OleDbType.Char);
                OleDbParameter pField6 = new OleDbParameter("@dtlhtm", OleDbType.BSTR);
                OleDbParameter pField7 = new OleDbParameter("@dtltxt", OleDbType.BSTR);
                OleDbParameter pField8 = new OleDbParameter("@akanam", OleDbType.BSTR);

                foxcmd.CommandText = "INSERT INTO " + foxTable +
                    " ( id, division, nam, nam2, dtlurl, dtlhtm, dtltxt, akanam ) " + " VALUES " +
                    " ( ?, ?, ?, ?, ?, ?, ?, ? )  ";

                foxcmd.Parameters.Add(pField1);
                foxcmd.Parameters.Add(pField2);
                foxcmd.Parameters.Add(pField3);
                foxcmd.Parameters.Add(pField4);
                foxcmd.Parameters.Add(pField5);
                foxcmd.Parameters.Add(pField6);
                foxcmd.Parameters.Add(pField7);
                foxcmd.Parameters.Add(pField8);

                int counter = 1;
                HtmlNode fnode =  doc.DocumentNode.SelectSingleNode("//ul[@id='dFugitiveExpandedDivisionNav']") ;
                string flist = fnode.InnerHtml ;
                flist = StringHelpers.StrExtract(flist, "<li>DOMESTIC", "<li>Most Recent", 1);
                int d_offset = 0;
                string dname = "" ; 
                string dlink = "" ;
                for (int ah = 0; ah < 1000; ah++)
                {

                    string div_href = StringHelpers.StrExtract(flist, "<a", "</a>", ref d_offset, 1);
                        if ( div_href.Equals(String.Empty) )
                        {
                            break;
                        }
                        else
                        {
                             div_href = "<a" + div_href + "</a>";
                        }


                        tempDoc.LoadHtml(div_href) ; 
                                         HtmlNode da = tempDoc.DocumentNode.SelectSingleNode("//a");
                    if (da.InnerText.Trim() != String.Empty)
                                        {
                                            dlink = root + "/" + da.GetAttributeValue("href", "NONE");
                                            dname = da.InnerText;
                                        }
                    

                        DivisionHrefPair dhp = new DivisionHrefPair();
                        areas.Clear(); // legacy when there were several areas per map
                        dhp.division = dname;
                        dhp.href = dlink;
                        if ( true   )  // division.IndexOf("Dallas") >= 0 || division == "Detroit Division" || division == "San Diego Division" || division == "Phoenix Division" || division == "Atlanta Division" 
                        {
                            Console.WriteLine(dname);
                            areas.Add(dhp);
                        }


                    fugsInArea.Clear();
                    foreach (DivisionHrefPair area in areas)
                    {

                        Console.WriteLine("Downloading data for {0} from {1}", area.division,area.href ) ;
                        Thread.Sleep(50);
                        client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
                        client.DownloadFile(area.href, fileName);
                        string area_root = area.href.Substring(0, area.href.LastIndexOf('/'));
                        string all = File.ReadAllText(fileName);
                        string fragment = StringHelpers.StrExtract(all, "<div class=\"dFugitiveList\">", "</div>");
//                      fragment = "<table" + StringHelpers.StrExtract(all, "<table") + "</table>";
                        fragmentDoc.LoadHtml(fragment);
                        HtmlNode table = fragmentDoc.DocumentNode.SelectSingleNode("//table");
                        if (table == null)
                        {

                            Console.WriteLine("problematic table: {0} | {1}", area.href, area.division);
                            continue;
                        }

                        {

                            // File.WriteAllText(@"c:\fragment.htm", table.InnerHtml);
                            foreach (HtmlNode row in table.SelectNodes("tr"))
                            {
                                foreach (HtmlNode cell in row.SelectNodes("td"))
                                {

                                    if (cell.InnerText.Trim() == String.Empty || cell.InnerText.Trim().Equals("&nbsp;"))
                                        continue;
                                    string td = cell.InnerHtml;
                                    int offset = 0;
                                    for (int i = 0; i < 1000; i++)
                                    {
                                        string href = "<a" + StringHelpers.StrExtract(td, "<a", "</a>", ref offset, 1) + "</a>";
                                        if (href.Equals("<a</a>"))
                                            break;
                                        tempDoc.LoadHtml(href);
                                        HtmlNode a = tempDoc.DocumentNode.SelectSingleNode("//a");
                                        if (a.InnerText.Trim() != String.Empty)
                                        {
                                            string details = root + "/" + a.GetAttributeValue("href", "NONE");
                                            string criminal = a.InnerText;
                                            DivisionCriminalPair dcp = new DivisionCriminalPair();
                                            dcp.criminal = regex1.Replace(criminal, " ").TrimEnd();
                                            dcp.division = area.division;
                                            if (!fugsInArea.ContainsKey(details))
                                                fugsInArea.Add(details, dcp);
                                        }


                                    }

                                }
                            }


                        }
                    }
                    foreach (KeyValuePair<string, DivisionCriminalPair> kvp in fugsInArea)
                    {

                        currentUrl = kvp.Key;
                        Boolean downloaded = true;
                        try
                        {
                            Thread.Sleep(50);
                            client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
                            client.DownloadFile(kvp.Key, fileName);
                        }
                        catch (Exception dex)
                        {
                            File.AppendAllText( Path.ChangeExtension(TextFile,"log" ), "\nUrl " + currentUrl + "  " +  DateTime.Now + "\n  Exception : \n" + dex );
                            downloaded = false; 
                        }
                        if (!downloaded) continue;
                        Console.WriteLine("Downloaded " + kvp.Key );
                        string all = File.ReadAllText(fileName);
                        string expectedTable = StringHelpers.StrExtract(all, "<table id=\"fugitiveProfile\">", "</table>", 1);
                        string fragment = "";
                        if (expectedTable != String.Empty)
                        {
                            fragment = "<table id=\"fugitiveProfile\">" + expectedTable + "</table>";
                        }
                        else
                        {
                            fragment = all;
                        }

                        fragmentDoc.LoadHtml(fragment);
                        HtmlNode table = fragmentDoc.DocumentNode.SelectSingleNode("//table");
                        if (table == null) continue;
                        StringBuilder sb = new StringBuilder();
                        HtmlNodeCollection rows = table.SelectNodes("tr");
                        if (rows == null)
                        {
                            Console.WriteLine("kvp problematic table: {0} | {1} | \n{2}", kvp.Value.criminal, kvp.Key, table.OuterHtml);
                            continue;
                        }
                        string name = "";
                        string akaName = "";
                        foreach (HtmlNode row in rows)
                        {
                            // bool nameRow = false;
                            // bool akaRow = false;
                            foreach (HtmlNode cell in row.SelectNodes("td"))
                            {
                                string td = regex1.Replace(cell.InnerText.Trim(), " " ) ;
                                /*
                                if (td.IndexOf("NAME:") >= 0)
                                {
                                    nameRow = true;
                                    continue;
                                }
                                if (td.IndexOf("AKA:") >= 0)
                                {
                                    akaRow = true;
                                    continue;
                                }
                                */
                                td = td.Replace("&quot;", "\"");
                                td = td.Replace("&nbsp;", " ");
                                // td = regex2.Replace(td, @" ");
                                /*
                                if (nameRow)
                                {
                                    name = td;
                                    nameRow = false;
                                    continue;
                                }
                                if (akaRow)
                                {
                                    akaName = td;
                                    akaRow = false;
                                    continue;
                                }
                                */
                                sb.Append(td);
                            }
                            sb.Append("\r\n");
                        }
                        string detail = "NAME:" + cleanHtml(kvp.Value.criminal) + "\r\n" + sb.ToString();
                        detail = cleanHtml(detail); 
                        File.AppendAllText(TextFile, detail + "=====================================================\r\n");
                        name = cleanHtml(StringHelpers.StrExtract(detail, "NAME:", "\n").Trim());
                        akaName = StringHelpers.StrExtract(detail, "AKA:", "\n").Trim();
                        akaName = cleanHtml(akaName);
                       
                        // detail = Regex.Replace(detail, "^NAME:.*\n", "");
                        // detail = Regex.Replace(detail, "^AKA:.*\n", "");
                        pField1.Value = counter;
                        pField2.Value = kvp.Value.division;
                        pField3.Value = name.Equals(String.Empty) ? cleanHtml(kvp.Value.criminal) : name; 
                        pField4.Value = kvp.Value.criminal;
                        pField5.Value = kvp.Key;
                        pField6.Value = all;
                        pField7.Value = detail;
                        pField8.Value = akaName;
                        int effected = foxcmd.ExecuteNonQuery();
                        if (counter++ % 100 == 0)
                            Console.WriteLine("Added {0} fox records" , counter);
                        
                    }
                }
                end = System.DateTime.Now;
                duration = end - start;
                string message = String.Format("Loading took {0} min  {1} sec ", duration.Minutes, duration.Seconds);
                XElement elapsed = new XElement("Elapsed" , message );
                xRoot.Add(elapsed); 
                XDocument xDoc = new XDocument( new XDeclaration("1.0", "", ""),xRoot);
                xDoc.Save( Path.Combine(Path.GetDirectoryName(HtmlFile), "elapsed.xml" ));
                Console.WriteLine(message);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                File.WriteAllText(TextFile, "Current Url " + currentUrl +  "  ABNORMAL TERMINATION WITH EXCEPTION : \n" + ex);   
            }
        }
        string cleanHtml(string html)
        {

            html = html.Replace("&ldquo;", "\"");
            html = html.Replace("&rdquo;", "\"");
            html = html.Replace("&rsquo;", "\"");
            html = html.Replace("&eacute;", "é");
            html = html.Replace("&ntilde;", "n");
            html = html.Replace("&#8217;", "\"");
            html = html.Replace("&#8221;", "\"");
            html = html.Replace("&amp;", "&");
            return html;

        }
        
        struct DivisionHrefPair
        {
            public string division;
            public string href;
        }
        struct DivisionCriminalPair
        {
            public string division;
            public string criminal;

        }

    }

}
