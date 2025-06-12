using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using UtilityLibrary;
using Core.Strings;
using System.Text.RegularExpressions;
using Parsing.CommandLineParsing;
using System.Net.Mail;

namespace CargoWise.DPS.Reporting
{
    public class MX_PGRMW_Report : Command
    {
        const string root = "http://www.pgr.gob.mx/Servicios/fugitivos/";
        public override void Execute()
        {

            XElement main = XElement.Load(@"C:\DPS\MX_PGRmw\PRG\0 MANUAL XML\MX_PGRmw.xml");
            string url = "http://www.pgr.gob.mx/Servicios/fugitivos/opera_consulta.asp?pagina=1&TamPagina=72&orden=nombre&estatus=A&sexo=A&tipo=img";
            string filename = @"C:\temp\scraped.htm";
            StringBuilder html = new StringBuilder();
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            List<PersonInfo> pi = new List<PersonInfo>();
            /*
            IWebProxy theProxy = client.Proxy;
            if (theProxy != null)
            {
                theProxy.Credentials = CredentialCache.DefaultCredentials;
            }
            */
            string body = client.DownloadString(url);
            List<PersonInfo> onpage = PopulateNames(body);
            pi.AddRange(onpage);



            //main.Save(@"c:\1.xml");

            var primaryNames =
                from c in main.Elements("DpsRecord").Elements("Name")
                where (string)c.Attribute("Type") == "Primary"
                select c;

            Lookup<string, string> nameXmlLookup = (Lookup<string, string>)primaryNames.ToLookup(p => StripNonAlpha(p.Value).ToUpper(),
                                                    p => p.Value.ToUpper());


            //        Array.ForEach(primaryNames.ToArray(), p => Console.WriteLine(p.Value));

            //           Console.WriteLine("============================================");


            int pageNum = StringHelpers.Occurs("class=\"ligas\"", body);
            string linkFragment = StringHelpers.ExtractString(body, "<p class=\"actualizacion\">", "class=\"breadcrumb\">Siguiente</a>");
            int offset = 0;
            for (int i = 1; i <= pageNum; i++)
            {
                string nextUrl = StringHelpers.StrExtract(linkFragment, "<a href=\"", "\" class=\"ligas\"", ref offset, 1);
                if (nextUrl == String.Empty)
                    break;
                body = client.DownloadString(root + nextUrl);
                if (body == String.Empty)
                    break;
                onpage = PopulateNames(body);
                pi.AddRange(onpage);
            }

            html.Append("<table border='1'><tr><td colspan='2'><h3>Found on Html page , but missing in XML</h3>( <b>to be added manually</b> )</td></tr>");

            foreach (var p in pi)
            {

                if (!nameXmlLookup.Contains(StripNonAlpha(p.Name).ToUpper()))
                {
                    html.Append(String.Format("<tr><td>{0}</td><td>{1}</td></tr>", p.Name , p.Url ));
                    Console.WriteLine("NOT in XML -" + p.Name);
                }
                else
                   Console.WriteLine("YES in XML " + p.Name);

            }
            html.Append("</table>");


            //          Console.WriteLine("============================================");
//            Console.ReadLine();
            Lookup<string, string> nameHtmlLookup = (Lookup<string, string>)pi.ToLookup(p => StripNonAlpha(p.Name).ToUpper(),
                                        p => p.Name.ToUpper());

            List<XElement> removed = new List<XElement>();

            html.Append("<hr/><table border='1'><tr><td><h3>Still in Xml, but missing on Html pages</h3>( <b>to be deleted programmatically</b> )</td></tr>");

            Dictionary<string, string> duplicates = new Dictionary<string, string>();
            foreach (var p in primaryNames)
            {

                if (!nameHtmlLookup.Contains(StripNonAlpha(p.Value).ToUpper()))
                {
                    IEnumerable<XElement> dpsRecords = from elem in
                                                           main.Elements("DpsRecord")
                                                       where (((string)elem.Element("Name")).Equals(p.Value) && (string)elem.Element("Name").Attribute("Type") == "Primary")

                                                       select elem;
                    if (dpsRecords.Count() == 0)
                    {
                        Console.WriteLine("Linq to XML unexpected empty result for " + " - " + p);
                    }
                    else
                    {
                        if (dpsRecords.Count() == 1)
                        {
                            html.Append(String.Format("<tr><td>{0}</td></tr>", p.Value));
                            removed.Add(dpsRecords.ToArray()[0]);
                        }
                        else
                        {
                            if (!duplicates.ContainsKey(p.Value))
                            {
                                duplicates.Add(p.Value, p.Value);
                                Console.WriteLine("NOT in HTML, but duplicate in XML " + dpsRecords.Count() + p.Value);

                                foreach (XElement elem in dpsRecords)
                                {
                                    removed.Add(elem);
                                }
                            }


                        }
                    }

                }

            }
            html.Append("</table>");
            if (removed.Count() > 0)
            {
                removed.Remove();
                html.Append(@"<h3>Saved as </h3><a href='\\syd-scon-1\dps\MX_PGRmw\PRG\0 MANUAL XML\MX_PGRMW_cleaned.xml'>\\syd-scon-1\dps\MX_PGRmw\PRG\0 MANUAL XML\MX_PGRMW_cleaned.xml</a><br/>");
                main.Save(@"\\syd-scon-1\dps\MX_PGRmw\PRG\0 MANUAL XML\MX_PGRMW_cleaned.xml");
            }
            else
            {
                html.Append(@"<h3>No Deletions Found, file was not generated</h3>");
            }
            //  sendEmail("nataliia.mishchenko@cargowise.com;raj.naidu@cargowise.com;michael.mitiaguin@cargowise.com", html.ToString(), true);   
            File.WriteAllText(@"c:\1.htm", html.ToString());
            sendEmail("michael.mitiaguin@cargowise.com", html.ToString(), true);

            //foreach (var p in primaryNames)
            //{
            //    if (p.Value.IndexOf("Luis Corona", StringComparison.InvariantCultureIgnoreCase) > 0)
            //    {
            //        Console.WriteLine(p.Value);
            //        Console.WriteLine(p.Value.ToUpper());
            //        Console.WriteLine("Name In Lookup " + nameLookup.Contains(p.Value));

            //    }
            //}

        }
        static List<PersonInfo> PopulateNames(string body)
        {

            List<PersonInfo> names = new List<PersonInfo>();
            string table90 = StringHelpers.ExtractString(body, "<table width='90%' align=\"center\"  class=\"bodercomp\">", "<hr width='90%' align=\"center\" size=\"1\">");
            int offset = 0;
            for (int i = 1; i < 100; i++)
            {
                string box = StringHelpers.StrExtract(table90, "<table border ='0'>", "</table>", ref offset, 1);
                if (box == String.Empty)
                    break;
                int cell2 = box.LastIndexOf("<td>");
                string person = StringHelpers.StrExtract(box, "<td>", "</td>", ref cell2, 1);
                string pname = StringHelpers.ExtractString(person, "<b>", "</b>").Trim();
                if (pname != String.Empty)
                {
                    person = pname;
                }
                else
                {
                    person = person.Replace("<p>", "").Replace("</p>", "").Replace("<br>", "").Replace("<b>", "").Replace("</b>", "").Trim();
                    if (person == String.Empty)
                        continue;
                }
                Console.WriteLine(person);
                PersonInfo p = new PersonInfo();
                p.Name = StripNonAlpha(person);
                p.Url = root + StringHelpers.ExtractString(box, "go2url(2,'", "'", false);
                names.Add(p);

            }
            return names;

        }
        static string StripNonAlpha(string name)
        {

            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            return rgx.Replace(name, "");

            //return Regex.Replace(name, @"[\W]", "");

        }

        public static void sendEmail(string to, string body, Boolean isBodyHtml)
        {

            // Create and configure the SmtpClient that will send the mail. 
            // Specify the host name of the SMTP server and the port used 
            // to send mail. 
            SmtpClient client = new SmtpClient("SYD-SMAI-1.corporate.cargowise.com", 25);

            // Configure the SmtpClient with the credentials used to connect 
            // to the SMTP server. 
            client.Credentials =
                 new NetworkCredential("michael.mitiaguin", "foxpro90");

            // Create the MailMessage to represent the e-mail being sent. 
            using (MailMessage msg = new MailMessage())
            {
                // Configure the e-mail sender and subject. 
                msg.From = new MailAddress("dps-report@cargowise.com");
                msg.Subject = System.Environment.MachineName + " # " + System.Environment.UserName + "| Dps Report for MX_PGRMW";

                // Configure the e-mail body. 
                msg.Body = body;
                msg.IsBodyHtml = isBodyHtml;

                // Attach the files to the e-mail message and set their MIME type. 
                //                   msg.Attachments.Add( 
                //                       new Attachment(@".\1.txt","text/plain")); 
                //                   msg.Attachments.Add( 
                //                       new Attachment(@".\1.exe", 
                //                       "application/octet-stream")); 

                // Iterate through the set of recipients specified as a parameter 
                // command line. Add all addresses with the correct structure as 
                //  recipients. 
                foreach (string str in to.Split(';'))
                {
                    // Create a MailAddress from each value on the command line 
                    // and add it to the set of recipients. 
                    try
                    {
                        msg.To.Add(new MailAddress(str));
                    }
                    catch (FormatException ex)
                    {
                        // Proceed to the next specified recipient. 
                        Console.WriteLine("{0}: Error -- {1}", str, ex.Message);
                        continue;
                    }
                }

                // Send the message. 
                client.Send(msg);
            }


        }

    }
    struct PersonInfo
    {
        public string Name;
        public string Url;
    }

}
