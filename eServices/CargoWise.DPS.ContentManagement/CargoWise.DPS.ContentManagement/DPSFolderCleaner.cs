using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Data.Common;
using System.Data.ProviderBase;
using System.Configuration;
using System.Text.RegularExpressions;
using Parsing.CommandLineParsing;
using System.Globalization;

namespace CargoWise.DPS.ContentManagement
{
    public class DPSFolderCleaner : Command
    {

        public string AppZipDir { get; set; }
        public string ExportSqlBkpDir { get; set; }
        public string IncSqlBkpDir { get; set; }
        public string SourceDataDir { get; set; }

        public override void Execute()
        {

            if (!String.IsNullOrEmpty(SourceDataDir))
            {
                Console.WriteLine("#### Deleting in " + SourceDataDir);
                CleanSourceDataDir();
            }
            if (!String.IsNullOrEmpty(ExportSqlBkpDir))
            {
                Console.WriteLine("#### Deleting in " + ExportSqlBkpDir);
                CleanSqlBkpDir(ExportSqlBkpDir);
            }

            if (!String.IsNullOrEmpty(IncSqlBkpDir))
            {
                Console.WriteLine("#### Deleting in " + IncSqlBkpDir);
                CleanSqlBkpDir(IncSqlBkpDir);
            }
            if (!String.IsNullOrEmpty(AppZipDir))
            {
                Console.WriteLine("#### Deleting in " + AppZipDir);
                CleanAppZipDir();
            }
            Console.WriteLine( "Press Enter to exit..." );
            Console.ReadLine();


        }

        void CleanAppZipDir()
        {
            DateTime today = DateTime.Today;
            int weekDay = (int)today.DayOfWeek;
            int tday = today.Day;
            int tmonth = today.Month;
            int tyear = today.Year;

            //The filesystem
            var q = from directory in new DirectoryInfo(AppZipDir).FlattenHierarchy(x => x.GetDirectories())
                    orderby directory.Name ascending
                    select directory;
            List<FileInfo> files = new List<FileInfo>();

            foreach (DirectoryInfo dir in q)
            {

                var f = from file in dir.GetFiles()
                        orderby file.Name descending
                        select file;

                foreach (FileInfo file in f)
                {
                    files.Add(file);
                }
            }

            int nToKeep = 3;
            int counter = 0;
            int iniMonth = 0;
            int iniYear = 0;
            int prevMonth = 0;
            int prevYear = 0;
            DateTime iniDate = DateTime.Today;
            DateTime backDate = DateTime.Today;

            foreach (FileInfo file in files)
            {


                string yymmdd = Path.GetFileNameWithoutExtension(file.Name).Substring(3);
                int dollar_pos = yymmdd.IndexOf("$");
                if ( dollar_pos > 0 )
                {
                    yymmdd = yymmdd.Substring(0, dollar_pos).TrimEnd();
                }

                try
                {
                    backDate = DateTime.ParseExact(yymmdd, "yyMMdd", CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    Console.WriteLine("{0} is not in the correct format. File {1}", yymmdd, file.FullName);
                    return;
                }
                string mockFile = file.FullName.ToUpper().Replace(@"K:", @"C:");
//                                if ( !File.Exists(mockFile) && File.Create( mockFile ) != null )
//                                    Console.WriteLine( "Created " + mockFile );
                Console.WriteLine(yymmdd + "  " + file.CreationTime);
                int fday = Convert.ToInt32(yymmdd.Substring(4, 2));
                int fmonth = Convert.ToInt32(yymmdd.Substring(2, 2));
                int fyear = Convert.ToInt32(yymmdd.Substring(0, 2));
                if (counter == 0)
                {
                    iniMonth = fmonth;
                    iniYear = fyear;
                    try
                    {
                        iniDate = DateTime.ParseExact(yymmdd, "yyMMdd", CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("{0} is not in the correct format. File {1}", yymmdd, file.FullName);
                        return;
                    }
                }
                if (counter++ < nToKeep) continue; // keep at least 3 
                if (fyear == iniYear && fmonth == iniMonth)
                {
                    if (counter <  31 ) continue;  // will keep  all for current  month
                    else
                    {
                        try
                        {
                            Console.WriteLine("Deleting " + file.FullName);
                            File.Delete(file.FullName);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to delete " + file.FullName + "\n" + ex);
                        }

                    }
                    continue;
                }
                if ( (iniDate - backDate).TotalDays < 120 && fmonth != prevMonth)
                {

                }
                else
                {
                    if (prevMonth != 0)
                    {
                        try
                        {
                            Console.WriteLine("Deleting " + file.FullName);
                            File.Delete(file.FullName);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to delete " + file.FullName + "\n" + ex);
                        }
                    }
                }

                prevMonth = fmonth;
                prevYear = fyear;
            }
//            Console.WriteLine(today.DayOfWeek);
//            Console.ReadLine();
        }


        void CleanSqlBkpDir(string backupDir)
        {
            
            DateTime today = DateTime.Today;
            DateTime backDate = DateTime.Today;
            int weekDay = (int)today.DayOfWeek;
            int tday = today.Day;
            int tmonth = today.Month;
            int tyear = today.Year;
            

            //The filesystem
            var q = from directory in new DirectoryInfo(backupDir).FlattenHierarchy(x => x.GetDirectories())
                    orderby directory.Name descending
                    select directory;
            List<FileInfo> files = new List<FileInfo>();

            foreach (DirectoryInfo dir in q)
            {

                var f = from file in dir.GetFiles()
                        orderby file.Name descending
                        select file;

                foreach (FileInfo file in f)
                {
                    files.Add(file);
                }
            }

            int nToKeep = 3;
            int counter = 0;
            int iniMonth = 0;
            int iniYear = 0;
            int prevMonth = 0;
            int prevYear = 0;
            DateTime iniDate = DateTime.Today;

            foreach (FileInfo file in files)
            {
                string yymmdd = Path.GetFileNameWithoutExtension(file.Name).ToUpper().Replace("DENIEDPARTYCONTENT_" , "" ) ;
                yymmdd = yymmdd.Replace("_", "").Replace("-","").Replace("INC","") ;
                try
                {
                    backDate = DateTime.ParseExact(yymmdd, "yyyyMMdd", CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    Console.WriteLine("{0} is not in the correct format. File {1}", yymmdd, file.FullName);
                    return;
                }
                string mockFile = file.FullName.ToUpper().Replace(@"K:", @"C:"); 
//                if ( !File.Exists(mockFile) && File.Create( mockFile ) != null )
//                    Console.WriteLine( "Created " + mockFile );
                Console.WriteLine( file.Name + "  " + yymmdd + "  " + file.CreationTime);
                int fday = Convert.ToInt32(yymmdd.Substring(6, 2));
                int fmonth = Convert.ToInt32(yymmdd.Substring(4, 2));
                int fyear = Convert.ToInt32(yymmdd.Substring(0, 4));
                if (counter == 0)
                {
                    iniMonth = fmonth;
                    iniYear = fyear;
                    try
                    {
                        iniDate = DateTime.ParseExact(yymmdd, "yyyyMMdd", CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("{0} is not in the correct format. File {1}", yymmdd, file.FullName);
                        return;
                    }
                }
                if (counter++ < nToKeep) continue;
                if (fyear == iniYear && fmonth == iniMonth)
                {
                    if (counter < 31 ) continue;
                    else
                    {
                        try
                        {
                            Console.WriteLine("Deleting " + file.FullName);
                            File.Delete(file.FullName);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to delete " + file.FullName + "\n" + ex);
                        }

                    }
                    continue;
                }
                if ((iniDate - backDate).TotalDays > 120 || fmonth == prevMonth )
                {
                    if (prevMonth != 0)
                    {
                        try
                        {
                            Console.WriteLine("Deleting " + file.FullName);
                            File.Delete(file.FullName);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to delete " + file.FullName + "\n" + ex);
                        }
                    }
                }

                prevMonth = fmonth;
                prevYear = fyear;
            }
//            Console.WriteLine(today.DayOfWeek);
//            Console.ReadLine();
        }


        void CleanSourceDataDir()
        {

            DateTime today = DateTime.Today;
            int weekDay = (int)today.DayOfWeek;
            int tday = today.Day;
            int tmonth = today.Month;
            int tyear = today.Year;

            DataTable listInfo = new DataTable();  
           using (SqlConnection conn = new SqlConnection(ConfigurationManager.AppSettings["DPS_DB"]))
           {

               conn.Open();
               IDbCommand selectCmd = conn.CreateCommand();
               selectCmd.CommandType = System.Data.CommandType.Text;
               string sql = "select a4_ListCode,a4_SourceDataRootDir from dpssourcelist where a4_SourceDataRootDir != '' order by a4_ListCode"; 
               selectCmd.CommandText = sql;
               using (IDataReader dataReader = selectCmd.ExecuteReader(CommandBehavior.CloseConnection))
               {
                   listInfo.Load(dataReader);
               }

//               File.WriteAllText( @"c:\1.txt" , "" ) ;
               foreach (DataRow elem in listInfo.Rows)
               {
                   
                   string fullPath =  Path.Combine(SourceDataDir, elem["a4_SourceDataRootDir"].ToString().Trim()) ; 
                   string line = elem["a4_ListCode"].ToString() + "  " + fullPath;
                   Console.WriteLine(line);
                   //                 File.AppendAllText( @"c:\1.txt" , line + "#\r\n" ) ; 
                   Regex dataDir = new Regex(@"\d\d\d\d_\d\d_\d\d");

                   //The filesystem
                   var q = from directory in new DirectoryInfo(fullPath).FlattenHierarchy(x => x.GetDirectories())
                           where dataDir.IsMatch(directory.Name)
                           orderby directory.Name descending
                           select directory;


                   int nToKeep = 3;
                   int counter = 0;
                   int iniMonth = 0;
                   int iniYear = 0;
                   int prevMonth = 0;
                   int prevYear = 0;
                   DateTime iniDate = DateTime.Today;
                   DateTime backDate = DateTime.Today;

                    foreach (DirectoryInfo dir in q)
                   {


                        string yymmdd = Path.GetFileNameWithoutExtension(dir.Name).Replace("_","").Replace("-", "");
                        try
                        {
                            backDate = DateTime.ParseExact(yymmdd, "yyyyMMdd", CultureInfo.InvariantCulture);
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("{0} is not in the correct format. File {1}", yymmdd, dir.FullName);
                            return;
                        }
                        int fday = Convert.ToInt32(yymmdd.Substring(6, 2));
                       int fmonth = Convert.ToInt32(yymmdd.Substring(4, 2));
                       int fyear = Convert.ToInt32(yymmdd.Substring(0, 4));
                       string mockDir = @"C:\TEST_RD" + dir.FullName.ToUpper().Replace(@"\\SYDCO-SDPS-1", "");
//                     Directory.CreateDirectory(mockDir); 

                       if (counter == 0)
                       {
                           iniMonth = fmonth;
                           iniYear = fyear;
                            try
                            {
                                iniDate = DateTime.ParseExact(yymmdd, "yyyyMMdd", CultureInfo.InvariantCulture);
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine("{0} is not in the correct format. File {1}", yymmdd, dir.FullName);
                                return;
                            }
                        }
                        if (counter++ <= nToKeep) continue;

                       if (fyear == iniYear && fmonth == iniMonth)
                       {
                           if ( counter < 31  ) continue;
                           else
                           {
                               try
                               {
                                     Directory.Delete(dir.FullName, true); 
//                                   Directory.CreateDirectory(outDir); 

                                   Console.WriteLine(dir.Name + "  " + dir.FullName);
                               }
                               catch (Exception ex)
                               {
                                   Console.WriteLine("Failed to delete " + dir.FullName + "\n" + ex);
                               }

                           }
                           continue;
                       }
                       if ((iniDate - backDate).TotalDays < 120 && fmonth != prevMonth)
                       {

                       }
                       else
                       {
                           if (prevMonth != 0)
                           {
                               try
                               {
                                   Console.WriteLine("Deleting " + dir.FullName);
                                     Directory.Delete(dir.FullName,true);
//                                   Directory.CreateDirectory(outDir);
                               }
                               catch (Exception ex)
                               {
                                   Console.WriteLine("Failed to delete " + dir.FullName + "\n" + ex);
                               }
                           }
                       }

                       prevMonth = fmonth;
                       prevYear = fyear;


                   }
               }
//               Console.ReadLine(); 

           }


  //          DataTable 
        }

    }

}

