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



namespace CargoWise.DPS.ContentManagement
{
    class CA_OSFIENT_Txt2Dbf : Command
    {

        private static string foxTable = "osfi_entity";
        private static string foxConnector = @"Provider=vfpoledb.1;Data Source={0}";

        public bool? Verbose { get; set; }
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

                FileHelperEngine<CA_OSFIENT_Entity> engine = new FileHelperEngine<CA_OSFIENT_Entity>();

            engine.ErrorManager.ErrorMode = ErrorMode.IgnoreAndContinue;


            CA_OSFIENT_Entity[] res = engine.ReadFile(TxtFile);
            DataTable dt = engine.ReadFileAsDT(TxtFile);

            if (engine.ErrorManager.ErrorCount > 0)
                engine.ErrorManager.SaveErrors( LogFile, "FileHelper Errors");


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
                OleDbParameter pField2 = new OleDbParameter("@name", OleDbType.BSTR);
                OleDbParameter pField3 = new OleDbParameter("@address", OleDbType.BSTR);
                OleDbParameter pField4 = new OleDbParameter("@basis", OleDbType.BSTR);
                foxcmd.CommandText = "INSERT INTO " + foxTable +
                    " ( id, name, address, basis ) " + "VALUES" +
                    " ( ?, ?, ?, ? ) ";
                foxcmd.Parameters.Add(pField1);
                foxcmd.Parameters.Add(pField2);
                foxcmd.Parameters.Add(pField3);
                foxcmd.Parameters.Add(pField4);
                int counter = 0;
                foreach (DataRow row in dt.Rows)
                {
                    pField1.Value = row["id"].ToString();
                    pField2.Value = row["name"].ToString();
                    pField3.Value = row["address"].ToString();
                    pField4.Value = row["basis"].ToString();
                    int effected = foxcmd.ExecuteNonQuery();
                    if (++counter % 10 == 0)
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
