using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;
using FileHelpers.DataLink;
using System.Data;
using System.IO;
using System.Data.OleDb;
using System.Net;
using Core.Strings;
using UtilityLibrary;
using System.Globalization;
using Parsing.CommandLineParsing;


namespace CargoWise.DPS.ContentManagement
{
    public class Russian_Xls2Dbf : Command
    {

        public bool? Verbose { get; set; }
        public string XlsFile { get; set; }


        public override void Execute () 
        {

            FileHelperEngine<Russian_Entity> engine = new FileHelperEngine<Russian_Entity>();

            engine.ErrorManager.ErrorMode = ErrorMode.IgnoreAndContinue; ;

            ExcelStorage provider = new ExcelStorage(typeof(Russian_Entity));
//            provider.StartColumn = 1;
//            provider.StartRow = 1;
            provider.ErrorManager.ErrorMode = ErrorMode.IgnoreAndContinue;
            provider.FileName = XlsFile;
            Russian_Entity[] res  = (Russian_Entity[])provider.ExtractRecords();
            if (provider.ErrorManager.ErrorCount > 0)
                provider.ErrorManager.SaveErrors(@"c:\1.log" , "DelimitedHelper XLS provider error:");

            string luka = res[0].ruName.Replace('П' , 'Л' ) ;

        }
        
    }
}
