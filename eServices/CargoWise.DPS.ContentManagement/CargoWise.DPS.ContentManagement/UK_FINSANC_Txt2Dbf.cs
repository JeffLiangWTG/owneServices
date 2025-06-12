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
    public class UK_FINSANC_Txt2Dbf : Command
    {
        private static string foxTable = "hm_treasury_fin_sanction";
        private static string foxConnector = @"Provider=vfpoledb.1;Data Source={0}";
        private string _listUpdated = null;

        public bool? Verbose { get; set; }
        public string TxtFile { get; set; }
        public string XlsFile { get; set; }
        public string LogFile { get; set; }
        public string DbfDir { get; set; }
        public string DbfFile { get; set; }


        public override void Execute()
        {

            OleDbConnection foxConn = null;
            bool noException = true;
            int counter = 1;
            string pname = "";
            UK_FINSANC_Entity curElem = null;
            try
            {

                FileHelperEngine<UK_FINSANC_Entity> engine = new FileHelperEngine<UK_FINSANC_Entity>();

                engine.ErrorManager.ErrorMode = ErrorMode.IgnoreAndContinue;


                TextReader tr = new StreamReader(TxtFile);
                string updLine = tr.ReadLine();
                bool found = false; 
                for (int line = 1; line < 4; line++)
                {
                    if (updLine.IndexOf("Last Updated") >= 0)
                    {
                        found = true;
                        break;
                    }
                    updLine = tr.ReadLine();
                }

                if (found)
                {
                    if ( updLine.IndexOf(":") > 0 ) 
                    {
                        _listUpdated = StringHelpers.RightOf(updLine, ':').Trim();
                    }
                    else
                    {
                        _listUpdated = StringHelpers.RightOf(updLine, ';').Trim();
                    }

                }
                tr.Close();


                ExcelStorage provider = new ExcelStorage(typeof(UK_FINSANC_Entity));
                provider.StartColumn = 1;
                provider.StartRow = 3;
                provider.ErrorManager.ErrorMode = ErrorMode.IgnoreAndContinue;


                //           UK_FINSANC_Entity[] res = engine.ReadFile(TxtFile);
                provider.FileName = XlsFile;
                UK_FINSANC_Entity[] res = (UK_FINSANC_Entity[])provider.ExtractRecords();


                //            if (engine.ErrorManager.ErrorCount > 0)
                //               engine.ErrorManager.SaveErrors( LogFile, "DelimitedHelper CSV engine error:");
                if (provider.ErrorManager.ErrorCount > 0)
                    provider.ErrorManager.SaveErrors(LogFile, "DelimitedHelper XLS provider error:");


                foxConnector = String.Format(foxConnector, DbfDir);
                foxTable = Path.GetFileNameWithoutExtension(DbfFile);
                Console.WriteLine("Loading to " + foxTable);

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
                OleDbParameter pField1 = new OleDbParameter("@name", OleDbType.Char);
                OleDbParameter pField2 = new OleDbParameter("@name1", OleDbType.BSTR);
                OleDbParameter pField3 = new OleDbParameter("@name2", OleDbType.BSTR);
                OleDbParameter pField4 = new OleDbParameter("@name3", OleDbType.BSTR);
                OleDbParameter pField5 = new OleDbParameter("@name4", OleDbType.BSTR);
                OleDbParameter pField6 = new OleDbParameter("@name5", OleDbType.BSTR);
                OleDbParameter pField7 = new OleDbParameter("@title", OleDbType.BSTR);
                OleDbParameter pField8 = new OleDbParameter("@dob", OleDbType.Char);
                OleDbParameter pField9 = new OleDbParameter("@townOfBirth", OleDbType.BSTR);
                OleDbParameter pField10 = new OleDbParameter("@countryOfBirth", OleDbType.BSTR);
                OleDbParameter pField11 = new OleDbParameter("@nationality", OleDbType.BSTR);
                OleDbParameter pField12 = new OleDbParameter("@passport", OleDbType.BSTR);
                OleDbParameter pField13 = new OleDbParameter("@ni_number", OleDbType.BSTR);
                OleDbParameter pField14 = new OleDbParameter("@position", OleDbType.BSTR);
                OleDbParameter pField15 = new OleDbParameter("@address1", OleDbType.BSTR);
                OleDbParameter pField16 = new OleDbParameter("@address2", OleDbType.BSTR);
                OleDbParameter pField17 = new OleDbParameter("@address3", OleDbType.BSTR);
                OleDbParameter pField18 = new OleDbParameter("@address4", OleDbType.BSTR);
                OleDbParameter pField19 = new OleDbParameter("@address5", OleDbType.BSTR);
                OleDbParameter pField20 = new OleDbParameter("@address6", OleDbType.BSTR);
                OleDbParameter pField21 = new OleDbParameter("@post_zip", OleDbType.BSTR);
                OleDbParameter pField22 = new OleDbParameter("@country", OleDbType.BSTR);
                OleDbParameter pField23 = new OleDbParameter("@other", OleDbType.BSTR);
                OleDbParameter pField24 = new OleDbParameter("@groupType", OleDbType.BSTR);
                OleDbParameter pField25 = new OleDbParameter("@aliasType", OleDbType.BSTR);
                OleDbParameter pField26 = new OleDbParameter("@regime", OleDbType.BSTR);
                OleDbParameter pField27 = new OleDbParameter("@listedOn", OleDbType.DBDate);
                OleDbParameter pField28 = new OleDbParameter("@lastUpdated", OleDbType.DBDate);
                OleDbParameter pField29 = new OleDbParameter("@groupID", OleDbType.Char);
                OleDbParameter pField30 = new OleDbParameter("@groupID", OleDbType.DBDate);

                foxcmd.CommandText = "INSERT INTO " + foxTable +
                    " ( name, name1, name2, name3, name4, name5, title, dob, town_ob, country_ob, national, passport, ni_number, position, address1, address2, address3, address4, address5, address6, post_zip, country, other, group_type, alias_type, regime, listedon, updated, group_id , list_date  ) " + " VALUES " +
                    " ( ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ? ) ";

                foxcmd.Parameters.Add(pField1);
                foxcmd.Parameters.Add(pField2);
                foxcmd.Parameters.Add(pField3);
                foxcmd.Parameters.Add(pField4);
                foxcmd.Parameters.Add(pField5);
                foxcmd.Parameters.Add(pField6);
                foxcmd.Parameters.Add(pField7);
                foxcmd.Parameters.Add(pField8);
                foxcmd.Parameters.Add(pField9);
                foxcmd.Parameters.Add(pField10);
                foxcmd.Parameters.Add(pField11);
                foxcmd.Parameters.Add(pField12);
                foxcmd.Parameters.Add(pField13);
                foxcmd.Parameters.Add(pField14);
                foxcmd.Parameters.Add(pField15);
                foxcmd.Parameters.Add(pField16);
                foxcmd.Parameters.Add(pField17);
                foxcmd.Parameters.Add(pField18);
                foxcmd.Parameters.Add(pField19);
                foxcmd.Parameters.Add(pField20);
                foxcmd.Parameters.Add(pField21);
                foxcmd.Parameters.Add(pField22);
                foxcmd.Parameters.Add(pField23);
                foxcmd.Parameters.Add(pField24);
                foxcmd.Parameters.Add(pField25);
                foxcmd.Parameters.Add(pField26);
                foxcmd.Parameters.Add(pField27);
                foxcmd.Parameters.Add(pField28);
                foxcmd.Parameters.Add(pField29);
                foxcmd.Parameters.Add(pField30);

                CultureInfo ci = new CultureInfo("en-GB");
                foreach (UK_FINSANC_Entity elem in res)
                {
                    curElem = elem;
                    pField1.Value = elem.name;
                    pname = pField1.Value.ToString();
                    pField2.Value = elem.name1;
                    pField3.Value = elem.name2;
                    pField4.Value = elem.name3;
                    pField5.Value = elem.name4;
                    pField6.Value = elem.name5;
                    pField7.Value = elem.title;
                    pField8.Value = elem.dob;
                    pField9.Value = elem.townOfBirth;
                    pField10.Value = elem.countryOfBirth;
                    pField11.Value = elem.nationality;
                    pField12.Value = elem.passport;
                    pField13.Value = elem.ni_number;
                    pField14.Value = elem.position;
                    pField15.Value = elem.address1;
                    pField16.Value = elem.address2;
                    pField17.Value = elem.address3;
                    pField18.Value = elem.address4;
                    pField19.Value = elem.address5;
                    pField20.Value = elem.address6;
                    pField21.Value = elem.post_zip;
                    pField22.Value = elem.country;
                    pField23.Value = elem.other;
                    pField24.Value = elem.groupType;
                    pField25.Value = elem.aliasType;
                    pField26.Value = elem.regime;
                    DateTime trial = new DateTime();
                    pField27.Value = elem.listedOn; // DateTime.TryParse(elem.listedOn, out trial) ? Convert.ToDateTime(elem.listedOn, ci) : DateTime.MinValue; 
                    pField28.Value = elem.lastUpdated; // DateTime.TryParse(elem.lastUpdated, out trial) ? elem.lastUpdated : DateTime.MinValue; 
                    pField29.Value = elem.groupID;
                    if (!String.IsNullOrEmpty(_listUpdated))
                    {
                        pField30.Value = Convert.ToDateTime(_listUpdated, ci);
                    }
                    else
                    {
                        pField30.Value = null;
                    }


                    int effected = foxcmd.ExecuteNonQuery();
                    if (counter++ % 100 == 0)
                        Console.WriteLine("Written {0} records", counter);
                }

                Console.WriteLine("Finished with {0} records written", counter);
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex);
                File.WriteAllText(LogFile, "Current line " + counter + "  Name " + pname + "\n" + ex.ToString());
                noException = false;
            }
            finally
            {
                if (foxConn != null) foxConn.Close();
            }

            if (noException)
                File.WriteAllText(LogFile, "SUCCESS");
        }

    }
}
