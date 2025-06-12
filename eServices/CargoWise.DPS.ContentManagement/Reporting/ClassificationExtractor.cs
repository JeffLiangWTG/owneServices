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
using System.Data;
using System.Data.SqlClient;


namespace CargoWise.DPS.Reporting
{
    public class ClassificationExtractor : Command
    {
        public bool? Verbose { get; set; }
        public string CmdName { get; set; }
        string root = "c:\\erouter\\";

        public override void Execute()
        {

            int yearFrom = 2013;
            int yearTo = 2013;
            int startDay = 1;
            int duration = -1;
            File.WriteAllText(Path.Combine(root, "log.txt"), "");
            for (int year = yearFrom; year <= yearTo; year++)
            {
                if (!Directory.Exists(root + year))
                {
                    Directory.CreateDirectory(root + year);
                }
                int days = (duration < 0) ? (DateTime.IsLeapYear(year) ? 366 : 365) : duration;
                for (int day = startDay; day <= days; day++)
                {
                    string date1 = new DateTime(year, 1, 1).AddDays(day-1).ToString("yyyyMMdd");
                    string month = date1.Substring(4, 2);
                    string mday = date1.Substring(6, 2);
                    string date2 = new DateTime(year, 1, 1).AddDays(day).ToString("yyyyMMdd");
                    if (!Directory.Exists(Path.Combine(root, year.ToString(), month)))
                    {
                        Directory.CreateDirectory(Path.Combine(root, year.ToString(), month));
                    }
                    string folder = Path.Combine(root, year.ToString(), month, mday);
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    GetOneDayInfo(date1, date2, folder);
                }
            }
        }

        private void GetOneDayInfo(string today, string tomorrow, string folder)
        {

            string connStr = "Data Source=syddps.db.wtg.zone;Initial Catalog=eRouter;Integrated Security=False;User Id=odysseyadmin;Password=tlu$i*ci$brla2ie;";
            List<string> description = new List<string>();
            List<string> classification = new List<string>();
            using (SqlConnection connection = new SqlConnection(connStr))
            {

                string query = "SELECT  EC_Data,ec_pk from eRouterEdiEnterpriseCommunication with (nolock) where EC_ApplicationCode = 'CMR' and EC_Direction = 'TRX' and EC_TransmitDate between '{0}' and '{1}' and  EC_data like '%ABD:%'"; //  and EC_data like '%ABD:%' and EC_Data like '%IPAD%';";
                const int BufferSize = 65536;  // 64 Kilobytes
                int lineCount = 0;
                int totCount = 0;
                int fileCount = 1;
                StreamWriter sw = new StreamWriter(Path.Combine(folder, "classification00001.txt"), false, Encoding.UTF8, BufferSize);
                query = String.Format(query,today,tomorrow);
                SqlCommand command = new SqlCommand(query, connection);
                command.CommandTimeout = 600;
                connection.Open();
                DateTime start = DateTime.Now;
                File.AppendAllText(Path.Combine(root, "log.txt"), "Day " + today + "\n\n");
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                {

                    while (reader.Read())
                    {
                        string ec_data = reader.GetString(0);
                        Guid ec_pk = reader.GetGuid(1);
                        if (totCount % 100 == 0)
                        {
                            Console.WriteLine("Read " + totCount );
                        }
                        totCount++;
                        if (ec_data.IndexOf("ABD:") > 0)
                        {

                            description = ExtractFromString(ec_data, "FTX+AAA+++", "'");
                            classification = ExtractFromString(ec_data, "ABD:", "'");
                            if (classification.Count == 0 || description.Count != classification.Count)
                            {
                                File.AppendAllText(Path.Combine(root, "log.txt"), "mismatch for " + ec_pk + "\n");
                                continue;
                            }

                            try
                            {
                                for (int i = 0; i < classification.Count; i++)
                                {
                                    sw.WriteLine(String.Format("{0}~{1}", classification[i], description[i]));
                                }
                            }
                            catch (Exception ex)
                            {
                                File.AppendAllText(Path.Combine(root, "log.txt"), "FAILED to extract for " + ec_pk + "\n");

                            }
                            lineCount += classification.Count;
                            if (lineCount > 10000)
                            {
                                sw.Close();
                                fileCount++;
                                sw = new StreamWriter(Path.Combine(folder, String.Format("classification{0}.txt", fileCount.ToString("D5"))), false, Encoding.UTF8, BufferSize);
                                lineCount = 0;

                            }
                        }
                        else
                        {

                            Console.WriteLine("Skipped " + ec_pk + ". No Tariff classification...");
                        }

                    }
                    sw.Close();
                    connection.Close();
                    DateTime end = DateTime.Now;
                    Console.WriteLine("Finished...");
                    File.AppendAllText(Path.Combine(root, "log.txt"), String.Format("Query took {0} \n\n", end.Subtract(start)));
                    //Console.ReadLine();
                }

            }

        }

        private static List<string> ExtractFromString(
          string text, string startString, string endString)
        {
            List<string> matched = new List<string>();
            int indexStart = 0, indexEnd = 0, indexRel = 0;
            bool exit = false;
            while (!exit)
            {
                indexStart = text.IndexOf(startString);
                if (indexStart == -1)
                { break; }
                indexRel = text.Substring(indexStart).IndexOf(endString);
                if (indexRel == -1)
                { break; }
                indexEnd = indexStart + indexRel;

                matched.Add(text.Substring(indexStart + startString.Length,
                    indexEnd - indexStart - startString.Length));
                text = text.Substring(indexEnd + endString.Length);
            }
            return matched;
        }

    }
}
