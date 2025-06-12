namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Reflection;
    using HtmlAgilityPack;
    using iTextSharp;
    using System.Xml;

    public class NameUrlGrabber
    {
        public static String states = "|AL|AK|AS|AZ|AR|CA|CO|CT|DE|DC|FM|FL|GA|GU|HI|ID|IL|IN|IA|KS|KY|LA|ME|MH|MD|MA|MI|MN|MS|MO|MT|NE|NV|NH|NJ|NM|NY|NC|ND|MP|OH|OK|OR|PW|PA|PR|RI|SC|SD|TN|TX|UT|VT|VI|VA|WA|WV|WI|WY|";
        public static char[] alpha = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

        public string ListCode;
        public string ListUrl;
        public string OutputFilename;
        public List<string> NameList;
        public List<string> UrlList;
        public Dictionary<string, string> NamesUrlList;
        public SortedDictionary<string, string> SortedNamesUrlList;
        HtmlDocument document;
        HtmlWeb remoteDocument;
        public bool sort = true;

        public NameUrlGrabber() { }

        public NameUrlGrabber(string list_code, string list_url, string output_filename)
        {
            this.ListCode = list_code.ToUpper();
            this.ListUrl = list_url;
            this.OutputFilename = output_filename;
            this.NameList = new List<string>();
            this.UrlList = new List<string>();
            this.NamesUrlList = new Dictionary<string, string>();
            this.document = new HtmlDocument();
            this.remoteDocument = new HtmlWeb();
        }

        public void DoWork()
        {
            Console.WriteLine("Beginning operations.");
            PreProcess();
            GrabNames();
            PostProcess();
//            Console.ReadKey();

        }

        public void PreProcess()
        {
            Console.WriteLine("Loading page.");
            document = remoteDocument.Load(ListUrl);
            if(ListCode.Equals("UK_METPLC"))
                sort = true;
        }

        public void GrabNames()
        {
            NamesUrlList = getList();
        }

        public void PostProcess()
        {
            if (sort)
            {
                NameList.Sort();
                SortedNamesUrlList = new SortedDictionary<string, string>(NamesUrlList);
                Console.WriteLine("List sorted.");
            }
            else
                Console.WriteLine("List not sorted.");

            //WriteToFile();
            WriteToXml();
            if (File.Exists(OutputFilename))
                Console.WriteLine("Writing successful.");
            else
                Console.WriteLine("Writing unsuccessful.");
            Console.WriteLine();
        }

        private void WriteToFile()
        {
            if (File.Exists(OutputFilename))
                File.Delete(OutputFilename);
            foreach (String name in NameList)
            {
                StreamWriter log;

                if (!File.Exists(OutputFilename))
                    log = new StreamWriter(OutputFilename);
                else
                    log = File.AppendText(OutputFilename);

                log.WriteLine(name);
                log.Close();
            }
        }

        private void WriteToXml()
        {
            using (XmlWriter writer = XmlWriter.Create(OutputFilename))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("DPSList");

                //foreach (KeyValuePair<string, string> kvp in NamesUrlList)
                foreach (KeyValuePair<string, string> kvp in SortedNamesUrlList)
                {
                    writer.WriteStartElement("DPSRecord");

                    writer.WriteStartElement("Name");
                    writer.WriteAttributeString("Type", "Primary");
                    writer.WriteValue(kvp.Key);
                    writer.WriteEndElement();

                    writer.WriteComment(kvp.Value);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private Dictionary<string, string> getList()
        {
            string baseClassName = typeof(Program).Name;
            string baseQualifiedName = typeof(Program).AssemblyQualifiedName;
            string instanceName = baseQualifiedName.Replace(baseClassName, ListCode);

            Type instanceType = Type.GetType(instanceName);
            object listObject = Activator.CreateInstance(instanceType);

            MethodInfo methodInfo = instanceType.GetMethod("GrabNames", new Type[] { typeof(string), typeof(HtmlDocument) });

            object listValues = methodInfo.Invoke(listObject, new object[] { ListUrl, document });
            return listValues as Dictionary<string, string>;
        }

        public bool isStateAbbreviation(String state)
        {
            return state.Length == 2 && states.IndexOf(state) > 0;
        }

        public string unEscape(string temp)
        {
            temp = temp.Replace("&amp;", "&");
            temp = temp.Replace("#38;", "&");
            temp = temp.Replace("&nbsp;", "");
            temp = temp.Replace("&#8217;", "'");
            temp = temp.Replace("&#8211;", "-");
            temp = temp.Replace("&#160;", "");
            temp = temp.Replace("&rsquo;", "'");
            temp = temp.Replace("&quot;", "\"");
            return temp;
        }
    }
}
