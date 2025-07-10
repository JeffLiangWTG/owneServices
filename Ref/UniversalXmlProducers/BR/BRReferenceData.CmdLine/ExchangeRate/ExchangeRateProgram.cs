using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class ExchangeRateProgram : BaseProgram
	{
		protected override void RunCore()
		{
			var parser = new ExchangeRateParser("BR Customs Exchange Rate");

			var lastWorkingDate = CalendarUtils.GetBRLastButOneWorkDay(GetToday());

			if (File.Exists(LogFilePath) && DateTime.TryParseExact(File.ReadAllText(LogFilePath), datePattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exchangeRateDate))
			{
				exchangeRateDate = exchangeRateDate.AddDays(1);
			}
			else
			{
				exchangeRateDate = lastWorkingDate.AddDays(-5);
			}

			while (exchangeRateDate <= lastWorkingDate)
			{
				if (CalendarUtils.IsWorkDay(exchangeRateDate))
				{
					Console.WriteLine($"Download Exchange Rate for {exchangeRateDate.ToString(datePattern, CultureInfo.InvariantCulture)}");

					var bXml = DownloadDataWithRetries(exchangeRateDate);
					if (bXml != null)
					{
						foreach (var rateType in new[] { Constants.ExchangeRateTypes.Customs, Constants.ExchangeRateTypes.CustomsExport })
						{
							using (var inputStream = new MemoryStream(bXml))
							{
								var outputFileName = GetOutputFilePath($"RefExchangeRateZZ_BR_{rateType}_{exchangeRateDate:yyyyMMdd}.xml");
								parser.ExportToXMLFile(inputStream, outputFileName, rateType, exchangeRateDate);
							}
						}
						File.WriteAllText(LogFilePath, $"{exchangeRateDate.ToString(datePattern, CultureInfo.InvariantCulture)}");
					}
				}
				exchangeRateDate = exchangeRateDate.AddDays(1);
			}
		}

		byte[] DownloadDataWithRetries(DateTime date)
		{
			int retries = 0;
			while (true)
			{
				using (var client = GetHttpClient())
				{
					byte[] bXml = null;
					Exception exception = null;

					try
					{
						bXml = ExchangeRateDownloader.Download(client, date);
					}
					catch (ArgumentException ex)
					{
						exception = ex;
					}
					if (bXml != null && bXml.Length > 0)
					{
						return bXml;
					}
					else if (retries++ < 3)
					{
						Console.WriteLine($"Failed to download exchange rate data for {date.ToString(datePattern, CultureInfo.InvariantCulture)}...{retries}");
						Thread.Sleep(SleepInterval);
					}
					else
					{
						if (exception != null)
						{
							throw exception;
						}
						return null;
					}
				}
			}
		}

		public string LogFilePath => logFilePath ?? (logFilePath = GetLogFilePath());
		string logFilePath;

		static string GetLogFilePath() => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
			Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location) + Constants.CustomsExchangeRateLogName + ".log");

		public void DeleteLogFile()
		{
			if (File.Exists(LogFilePath))
			{
				File.Delete(LogFilePath);
			}
		}

		protected virtual DateTime GetToday() => DateTime.Today;

		protected virtual HttpClient GetHttpClient() => HttpClientUtils.New();

		protected virtual int SleepInterval => 5 * 60 * 1000;

		readonly string datePattern = "yyyy-MM-dd";
	}
}
