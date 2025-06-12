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
    public class US_ECR_Csv2Xml : Command
    {


        public bool? Verbose { get; set; }
        public string CsvFile { get; set; }
        public string LogFile { get; set; }
        public string XmlDir { get; set; }
        public string XmlFile { get; set; }


        public override void Execute()
        {

            bool noException = true;
            try
            {

                FileHelperEngine<US_ECR_Entity> engine = new  FileHelperEngine<US_ECR_Entity>();
                DelimitedFileEngine engine1 = new DelimitedFileEngine(typeof(US_ECR_Entity));

                engine.ErrorManager.ErrorMode = ErrorMode.IgnoreAndContinue;

                US_ECR_Entity[] res = (US_ECR_Entity[])engine1.ReadFile(CsvFile);
                List<US_ECR_Entity> sorted =  res.ToList<US_ECR_Entity>().OrderBy(en => en.entNum ).ThenBy( en => en.name ).ToList() ;
                engine1.Options.Delimiter = ",";
                engine1.WriteFile(@"C:\dps\us_ecr\cplf.txt", sorted.ToArray());

                res = (US_ECR_Entity[])engine1.ReadFile(Path.ChangeExtension(CsvFile , "csv"));
                sorted = res.ToList<US_ECR_Entity>().OrderBy(en => en.name).ThenBy( en => en.name ).ToList();
                engine1.WriteFile(@"C:\dps\us_ecr\cplf.csv", sorted.ToArray());



                if (engine.ErrorManager.ErrorCount > 0)
                    engine.ErrorManager.SaveErrors(LogFile, "DelimitedHelper CSV engine error:");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                File.WriteAllText(LogFile, "\n\n" + ex.ToString());
                noException = false;
            }
            finally
            {
            }


        }
    }
}