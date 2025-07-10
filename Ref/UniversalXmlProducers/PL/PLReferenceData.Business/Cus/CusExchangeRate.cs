using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using CargoWise.RefDbRepo.PLReferenceData.Services;
using CargoWise.RefDbRepo.PLReferenceData.Services.Cus;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Cus
{
	class CusExchangeRate
	{
		public static tabela_kursow GetExchangeRateXmlData()
		{
			try
			{
				var exchangeRateUrlTuple = GetLatestExchangeRateUrl();

				if (null == exchangeRateUrlTuple)
				{
					return null;
				}

				var dataTableA = GetDataTableContent(GetFullXmlUrl(exchangeRateUrlTuple.Item1));
				var dataTableB = GetDataTableContent(GetFullXmlUrl(exchangeRateUrlTuple.Item2));

				if ((dataTableA == null) || (dataTableB == null))
				{
					return null;
				}

				var dataTable = new tabela_kursow
				{
					pozycja = new List<tabela_kursowPozycja>(),

					// does not matter which one table we will use to pass base informations so we can use either table A or table B
					data_publikacji = dataTableA.data_publikacji
				};

				// add positions from tables 'a' & 'b'
				dataTable.pozycja.AddRange(dataTableA.pozycja);
				dataTable.pozycja.AddRange(dataTableB.pozycja);

				return dataTable;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex);
			}

			return null;
		}

		static tabela_kursow GetDataTableContent(Uri xmlUrl)
		{
			var xmlContent = DownloadDataTableContent.GetDataTableContent(xmlUrl);

			var dataTable = XmlParser.DeserializeFromString<tabela_kursow>(xmlContent.ReadToEnd());

			return dataTable;
		}

		protected static Uri GetFullXmlUrl(string xmlFilename) => new Uri($"{CusConstants.NbpXmlUrl}{xmlFilename}.xml");

		static Tuple<string, string> GetLatestExchangeRateUrl()
		{
			int retry = 0;
			WebException webException = null;
			var searchedDate = GetPenultimateWednesday(DateTime.UtcNow);

			if (searchedDate > DateTime.UtcNow)
			{
				searchedDate = GetPenultimateWednesday(DateTime.UtcNow.AddMonths(-1));
			}
			var fileUrl = GetDirTxtFileUrl(searchedDate);

			while (retry < ApplicationConfig.Instance.MaxRetry)
			{
				try
				{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
					using (var webClient = new WebClient())
					{
						var dirTxtFileContent = new StringReader(webClient.DownloadString(fileUrl))
							?? throw new Exception("Failed to download dir.txt content.");

						var avaliableFiles = GetListOfFilenamesFromReadDirTxt(dirTxtFileContent);

						while ((searchedDate <= DateTime.UtcNow))
						{
							var retv = GetSpecifiedFilenameWithDateX(avaliableFiles, searchedDate.ToString("yyMMdd", CultureInfo.InvariantCulture));
							if (retv != null)
							{
								return retv;
							}

							searchedDate = searchedDate.AddDays(1);
						}
					}
#pragma warning restore SYSLIB0014 // Type or member is obsolete
				}
				catch (WebException ex)
				{
					webException = ex;
				}

				retry++;
			}

			if (webException != null)
			{
				Console.Error.WriteLine("Web Exception status : {0}", webException.Status);
				Console.Error.WriteLine($"Web Exception message as follows:");
				Console.Error.WriteLine(webException);
			}
			else
			{
				Console.Error.WriteLine("Could not find dir.txt exchange rate file");
			}

			return null;
		}

		protected static Uri GetDirTxtFileUrl(DateTime date) => new Uri(date.Year == DateTime.UtcNow.Year ? $"{CusConstants.DirTxtFileUrl}.txt" : $"{CusConstants.DirTxtFileUrl}{date.Year}.txt");

		protected static List<string> GetListOfFilenamesFromReadDirTxt(StringReader dirTxtFileContent)
		{
			var retv = new List<string>();
			string readLine;

			while (null != (readLine = dirTxtFileContent.ReadLine()))
			{
				retv.Add(readLine);
			}

			return retv;
		}

		public static DateTime GetPenultimateWednesday(DateTime date)
		{
			DateTime penultimateWednesdayDate;

			var tempDate = new DateTime(
				date.Year,
				date.Month,
				DateTime.DaysInMonth(date.Year, date.Month) // get last day of the month
			);

			while (tempDate.DayOfWeek != DayOfWeek.Wednesday)
			{
				tempDate = tempDate.AddDays(-1);
			}

			penultimateWednesdayDate = tempDate.AddDays(-7);

			return penultimateWednesdayDate;
		}

		protected static Tuple<string, string> GetSpecifiedFilenameWithDateX(List<string> avaliableFilesList, string searchedFilenameDate)
		{
			var avaliableFilenamesListWithSpecifiedDate = avaliableFilesList.Where(x => x.Contains(searchedFilenameDate)).ToList();

			if (avaliableFilenamesListWithSpecifiedDate.Count < 2)
			{
				return null;
			}

			string tableA = avaliableFilenamesListWithSpecifiedDate.Where(x => x[0] == 'a').First();
			string tableB = avaliableFilenamesListWithSpecifiedDate.Where(x => x[0] == 'b').First();

			if (string.IsNullOrEmpty(tableA) || string.IsNullOrEmpty(tableB))
			{
				return null;
			}

			return new Tuple<string, string>(tableA, tableB);
		}
	}
}
