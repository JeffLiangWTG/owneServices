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
    public class TestWebClient : Command
    {
        public bool? Verbose { get; set; }
        public string Uri { get; set; }
        public override void Execute()
        {

            try
            {
                Console.WriteLine("Downloading to string from " + Uri); 
                WebClient client = new WebClient();
                IWebProxy theProxy = client.Proxy;
                if (theProxy != null)
                {
                    theProxy.Credentials = CredentialCache.DefaultCredentials;
                }
                client.DownloadFile(Uri, "test.html");
//              string html = client.DownloadString(Uri);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }

        }
    }
}

