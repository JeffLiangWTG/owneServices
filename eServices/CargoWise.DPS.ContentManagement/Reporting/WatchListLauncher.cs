using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using Parsing.CommandLineParsing;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using WatchListCrawlerV3;


namespace CargoWise.DPS.Reporting
{
    public class WatchListLauncher : Command
    {

        public override void Execute()
        {

            Configuration config = ConfigurationManager.OpenExeConfiguration(
                                     ConfigurationUserLevel.None);

            // Get the AppSettings section.
            AppSettingsSection section =
              (AppSettingsSection)config.GetSection("appSettings");
            string xml = section.SectionInformation.GetRawXml(); // +"<Root>" + "</Root>";

            XDocument xdoc = XDocument.Parse(xml) ;

            var pairs = from pair in xdoc.Descendants("add")  //Elements("appSettings").Elements("add")
                       select new
                       {
                           key = pair.Attribute("key").Value,
                           value = pair.Attribute("value").Value
                       };
            foreach (var pair in pairs)
           {Console.WriteLine("Key={0} / Value={1}", pair.key, pair.value);   
           }

            XElement xelem = XElement.Parse(xml);

            pairs = from pair in xelem.Descendants("add")
                        select new
                        {
                            key = pair.Attribute("key").Value,
                            value = pair.Attribute("value").Value
                        };
            foreach (var pair in pairs)
            {
                Console.WriteLine("Key={0} / Value={1}", pair.key, pair.value);
                string arg0 = pair.key; 
                string[] args12 = pair.value.Replace('#','&').Split(',') ;
                NameUrlGrabber temp = new NameUrlGrabber(arg0, args12[0],  args12[1] ) ;
                temp.DoWork();

                
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = @".\";
                startInfo.FileName = @"C:\eServices\CargoWise.DPS.ContentManagement\WatchListCrawlerV3\bin\Debug\WatchListCrawlerV3.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                startInfo.Arguments = arg0 + " \"" + args12[0] + "\" \"" + args12[1] + "\"" ;  

                try
                {
                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        exeProcess.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            string listParam = ConfigurationManager.AppSettings["US_FBI"];
        }
    }
}
