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

namespace CargoWise.DPS.ContentManagement
{
    public class US_SSMW_Html2Text : Command
    {
        private static string foxTable = "dea_most_wanted_fugitives";
        private static string foxConnector = @"Provider=vfpoledb.1;Data Source={0}";

        public bool? Verbose { get; set; }
        public string HtmlFile { get; set; }
        public string TextFile { get; set; }
        public string DbfDir { get; set; }
        public string DbfFile { get; set; }
        public string Uri { get; set; }

        OleDbConnection foxConn;
        OleDbCommand foxcmd;
        //OleDbParameter pField1;
        //OleDbParameter pField2;
        //OleDbParameter pField3;
        //OleDbParameter pField4;
        //OleDbParameter pField5;
        //OleDbParameter pField6;
        //OleDbParameter pField7;

        private string separator = new string('=', 20) + "\n"; 

        public override void Execute()
        {


            DateTime start;
            DateTime end;
            TimeSpan duration;

            try
            {

            foxTable = DbfFile ?? foxTable;
            foxConnector = DbfDir != null ? String.Format(foxConnector, DbfDir) : String.Format(foxConnector, @"K:\Denied Global\foxtables");


            HtmlDocument doc = new HtmlDocument();
            List<UrlNamePair> pairs = new List<UrlNamePair>();
            doc.OptionFixNestedTags = true;
            HtmlDocument fragmentDoc = new HtmlDocument();
            HtmlDocument tempDoc = new HtmlDocument();
            WebClient client = new WebClient();
            IWebProxy theProxy = client.Proxy;
            if (theProxy != null)
            {
                theProxy.Credentials = CredentialCache.DefaultCredentials;
            }

            string root = Uri.Substring(0, Uri.LastIndexOf('/')) + "/";
            string index = "";
            string fileName = Path.GetTempFileName();
            RegexOptions options = RegexOptions.None;
//          Regex regex = new Regex(@"[ ]{2,}", options);
            Regex regex = new Regex(@"\s+",options) ;


            index = client.DownloadString(Uri);

            if (HtmlFile != null)
            {
                File.WriteAllText(HtmlFile, index);
            }
            File.WriteAllText(TextFile, separator );

            string fragment = StringHelpers.StrExtract(index, "<br clear=\"all\">", "</table>", 1);
            if (fragment.Equals(String.Empty))
            {
                throw new Exception("Expected fragment is empty for delimiter <br clear");
            }
            fragment += "</table>";
            doc.LoadHtml(fragment);


                start = DateTime.Now;
                //           PrepareInsertStatement();

                HtmlNode table = doc.DocumentNode.SelectSingleNode("//table");
                HtmlNodeCollection rows = table.SelectNodes("tr");
                foreach (HtmlNode row in rows)
                {
                    if (row == null) continue;
                    HtmlNodeCollection cells = row.SelectNodes("td");
                    if (cells == null) continue;
                    foreach (HtmlNode cell in cells)
                    {
                        if (cell == null)
                            break;
                        string attr = cell.GetAttributeValue("class", "");
                        if (attr.Equals("size8"))
                        {
                            tempDoc.LoadHtml(cell.InnerHtml);
                            HtmlNode a = tempDoc.DocumentNode.SelectSingleNode("//a");
                            if (a != null)
                            {
                                UrlNamePair pair = new UrlNamePair()
                                {
                                    url = Path.Combine(root, a.GetAttributeValue("href", "")),
                                    name = a.InnerText
                                };
                                pairs.Add(pair);
                                Console.WriteLine(pair.url + "  " + pair.name);

                            }
                        }
                        /*
                    string link = root + "/" + a.GetAttributeValue("href", "");
                    <td class="size8"><a href="wanted_amthor.shtml"*/
                    }
                }

                int counter = 1;
                foreach (UrlNamePair pair in pairs)
                {
                    string all = client.DownloadString(pair.url);
                    string expectedTable = StringHelpers.StrExtract(all, "<table width=\"450\"", "</table>", 1);

                    if (expectedTable != String.Empty)
                    {
                        fragment = "<table width=\"450\"" + expectedTable + "</table>";
                    }
                    else
                    {
                        fragment = all;
                    }

                    fragmentDoc.LoadHtml(fragment);
                    table = fragmentDoc.DocumentNode.SelectSingleNode("//table");
                    StringBuilder sb = new StringBuilder();
                    rows = table.SelectNodes("tr");
                    if (rows == null)
                    {
                        Console.WriteLine("problematic table: ");
                        continue;
                    }

                    bool startMark = false;
                    foreach (HtmlNode row in rows)
                    {
                        if (!startMark)
                        {
                            string inner = row.InnerHtml;

                            if (inner.IndexOf("<td colspan=\"2\">") >= 0)
                            {
                                startMark = true;
                                continue;
                            }
                            else if (inner.IndexOf("<h3") >= 0)
                            {
                                string name = StringHelpers.StrExtract(inner, "<h3>", "</h3>", 1);
                                sb.Append("Name: " + name + "\n");
                                continue;

                            }

                            else
                                continue;
                        }
                        if ( row.InnerHtml.IndexOf("<td colspan=\"2\">") >= 0 )  
                           break;

                        foreach (HtmlNode cell in row.SelectNodes("td"))
                        {
                            string td = cell.InnerText.Replace("\n", " ").Trim();
                            td = regex.Replace(td, " ");
                            sb.Append(td);
                        }
                        sb.Append("\n");
                    }
                    string detail = "Url:"  + pair.url + "\n" + sb.ToString();
                    File.AppendAllText(TextFile, detail + separator );
                    /*
                    name = StringHelpers.StrExtract(detail, "Name:", "\n").Trim();
                    akaName = StringHelpers.StrExtract(detail, "Aliases:", "\n").Trim();
                    pField1.Value = counter;
                    pField2.Value = "";
                    pField3.Value = "";
                    pField4.Value = "";
                    pField5.Value = "";
                    pField6.Value = all;
                    pField7.Value = detail;
                    int effected = foxcmd.ExecuteNonQuery();
                    */
                    if (counter++ % 100 == 0)
                        Console.WriteLine("Added {0} records", counter);

                }
                end = System.DateTime.Now;
                duration = end - start;
                string message = String.Format("Loading took {0} min  {1} sec ", duration.Minutes, duration.Seconds);
                Console.WriteLine(message);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                File.WriteAllText(TextFile, "ABNORMAL TERMINATION WITH EXCEPTION : \n" + ex);   

            }

        }
        void PrepareInsertStatement()
        {
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

            foxcmd = new OleDbCommand("Insert ", foxConn);
            OleDbParameter pField1 = new OleDbParameter("@id", OleDbType.Integer);
            OleDbParameter pField2 = new OleDbParameter("@nam", OleDbType.BSTR);
            OleDbParameter pField3 = new OleDbParameter("@nam2", OleDbType.Char);
            OleDbParameter pField4 = new OleDbParameter("@dtlurl", OleDbType.Char);
            OleDbParameter pField5 = new OleDbParameter("@dtlhtm", OleDbType.BSTR);
            OleDbParameter pField6 = new OleDbParameter("@dtltxt", OleDbType.BSTR);
            OleDbParameter pField7 = new OleDbParameter("@akanam", OleDbType.BSTR);

            foxcmd.CommandText = "INSERT INTO " + foxTable +
                " ( id, nam, nam2, dtlurl, dtlhtm, dtltxt, akanam ) " + " VALUES " +
                " ( ?, ?, ?, ?, ?, ?, ? )  ";

            foxcmd.Parameters.Add(pField1);
            foxcmd.Parameters.Add(pField2);
            foxcmd.Parameters.Add(pField3);
            foxcmd.Parameters.Add(pField4);
            foxcmd.Parameters.Add(pField5);
            foxcmd.Parameters.Add(pField6);
            foxcmd.Parameters.Add(pField7);



        }


        struct UrlNamePair
        {
            public string url;
            public string name;
        }

    }


}
