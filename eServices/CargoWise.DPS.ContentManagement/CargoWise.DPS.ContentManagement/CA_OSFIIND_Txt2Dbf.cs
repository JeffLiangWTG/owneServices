using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Data.OleDb;
using FileHelpers;
using Core.Strings;
using UtilityLibrary;
using Parsing.CommandLineParsing;
using FileHelpers.DataLink;



namespace CargoWise.DPS.ContentManagement
{
    class CA_OSFIIND_Txt2Dbf : Command
    {

        private static string foxTable = "osfi_Individual";
        private static string foxConnector = @"Provider=vfpoledb.1;Data Source={0}";

        public bool? Verbose { get; set; }
        public string XlsFile { get; set; }
        public string TxtFile { get; set; }
        public string LogFile { get; set; }
        public string DbfDir { get; set; }
        public string DbfFile { get; set; }



        public override void Execute()
        {
            OleDbConnection foxConn = null;
            bool noException = true;

            try
            {
                foxConnector = String.Format(foxConnector, DbfDir);
                foxTable = Path.GetFileNameWithoutExtension(DbfFile);
                Console.WriteLine("Loading to " + foxTable);

                FileHelperEngine<CA_OSFIIND_Entity> engine = new FileHelperEngine<CA_OSFIIND_Entity>();

                engine.ErrorManager.ErrorMode = ErrorMode.IgnoreAndContinue;

                ExcelStorage provider = new ExcelStorage(typeof(CA_OSFIIND_Entity));
                provider.StartColumn = 1;
                provider.StartRow = 7;
                provider.ErrorManager.ErrorMode = ErrorMode.IgnoreAndContinue;


                //          CA_OSFIIND_Entity[] res = engine.ReadFile(TxtFile);
                
                               
                //DataTable dt = engine.ReadFileAsDT(TxtFile);

                //if (engine.ErrorManager.ErrorCount > 0)
                //    engine.ErrorManager.SaveErrors(LogFile, "Errors.txt");


                provider.FileName = XlsFile;
                DataTable dt = provider.ExtractRecordsAsDT();
                if (provider.ErrorManager.ErrorCount > 0)
                    provider.ErrorManager.SaveErrors(LogFile, "DelimitedHelper XLS provider error:");


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
                OleDbParameter pField1 = new OleDbParameter("@id", OleDbType.Char);
                OleDbParameter pField2 = new OleDbParameter("@lastname", OleDbType.BSTR);
                OleDbParameter pField3 = new OleDbParameter("@name1", OleDbType.BSTR);
                OleDbParameter pField4 = new OleDbParameter("@name2", OleDbType.BSTR);
                OleDbParameter pField5 = new OleDbParameter("@name3", OleDbType.BSTR);
                OleDbParameter pField6 = new OleDbParameter("@name4", OleDbType.BSTR);
                OleDbParameter pField7 = new OleDbParameter("@pob", OleDbType.BSTR);
                OleDbParameter pField8 = new OleDbParameter("@altpob", OleDbType.BSTR);
                OleDbParameter pField9 = new OleDbParameter("@dob", OleDbType.BSTR);
                OleDbParameter pField10 = new OleDbParameter("@altdob1", OleDbType.BSTR);
                OleDbParameter pField11 = new OleDbParameter("@altdob2", OleDbType.BSTR);
                OleDbParameter pField12 = new OleDbParameter("@altdob3", OleDbType.BSTR);
                OleDbParameter pField13 = new OleDbParameter("@nation", OleDbType.BSTR);
                OleDbParameter pField14 = new OleDbParameter("@altnation1", OleDbType.BSTR);
                OleDbParameter pField15 = new OleDbParameter("@altnation2", OleDbType.BSTR);
                OleDbParameter pField16 = new OleDbParameter("@misc", OleDbType.BSTR);
                OleDbParameter pField17 = new OleDbParameter("@basis", OleDbType.BSTR);
                OleDbParameter pField18 = new OleDbParameter("@title", OleDbType.BSTR);
                OleDbParameter pField19 = new OleDbParameter("@passport", OleDbType.BSTR);
                OleDbParameter pField20 = new OleDbParameter("@designatio", OleDbType.BSTR);
                OleDbParameter pField21 = new OleDbParameter("@address", OleDbType.BSTR);
                OleDbParameter pField22 = new OleDbParameter("@nat_id", OleDbType.BSTR);
                OleDbParameter pField23 = new OleDbParameter("@other", OleDbType.BSTR);

                foxcmd.CommandText = "INSERT INTO " + foxTable +
                    " ( id, lastname, name1, name2, name3, name4, pob, altpob, dob, altdob1, altdob2, altdob3, nation, altnation1, altnation2, misc, basis, title, designatio, passport, address, nat_id, other ) " + " VALUES " +
                    " ( ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ? ) ";
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

                int counter = 0;
                foreach (DataRow row in dt.Rows)
                {
                    pField1.Value = row["id"].ToString();
                    //if (pField1.Value.Equals("422.06"))
                    //    Console.WriteLine("422.06");

                    pField2.Value = row["lastname"].ToString();
                    pField3.Value = row["name1"].ToString();
                    pField4.Value = row["name2"].ToString();
                    pField5.Value = row["name3"].ToString();
                    pField6.Value = row["name4"].ToString();
                    pField7.Value = row["pob"].ToString();
                    pField8.Value = row["altpob"].ToString();
                    pField9.Value = row["dob"].ToString();
                    pField10.Value = row["altdob1"].ToString();
                    pField11.Value = row["altdob2"].ToString();
                    pField12.Value = row["altdob3"].ToString();
                    pField13.Value = row["nationality"].ToString();
                    pField14.Value = row["nationality1"].ToString();
                    pField15.Value = row["nationality2"].ToString();

                    string misc = row["misc"].ToString();
                    pField16.Value = misc;

                    pField17.Value = row["basis"].ToString();
                    if (misc != string.Empty)
                    {
                        pField18.Value = StringHelpers.StrExtract(misc, "Title:", ";", 1).Trim();
                        pField19.Value = StringHelpers.StrExtract(misc, "Designation:", ";", 1).Trim();
                        if (pField19.Value.ToString() == String.Empty)
                        {
                            pField19.Value = StringHelpers.StrExtract(misc, "Designation:", 1).Trim();
                        }
                        pField20.Value = StringHelpers.StrExtract(misc, "Passport no.:", ";", 1).Trim();
                        if (pField20.Value.ToString() == String.Empty)
                        {
                            pField20.Value = StringHelpers.StrExtract(misc, "Passport no.:").Trim();
                        }
                        pField21.Value = StringHelpers.StrExtract(misc, "Address:", ";", 1).Trim(); ;
                        if (pField21.Value.ToString() == String.Empty)
                        {
                            pField21.Value = StringHelpers.StrExtract(misc, "Address:", 1).Trim();
                        }
                        pField22.Value = StringHelpers.StrExtract(misc, "National identification no.:", ";", 1).Trim();
                        if (pField22.Value.ToString() == String.Empty)
                        {
                            pField22.Value = StringHelpers.StrExtract(misc, "National identification no.:", 1).Trim();
                        }
                        pField23.Value = StringHelpers.StrExtract(misc, "Other information:", 1).Trim();

                    }
                    else
                    {

                        pField19.Value = "";
                        pField18.Value = "";
                        pField20.Value = "";
                        pField21.Value = "";
                        pField22.Value = "";
                        pField23.Value = "";
                    }
                    int effected = foxcmd.ExecuteNonQuery();
                    if (++counter % 100 == 0)
                        Console.WriteLine("Written {0} records", counter);

                }
                Console.WriteLine("Finished with {0} records written", counter);


            }

            catch (Exception ex)
            {
                Console.WriteLine(ex);
                File.WriteAllText(LogFile, ex.ToString());
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
