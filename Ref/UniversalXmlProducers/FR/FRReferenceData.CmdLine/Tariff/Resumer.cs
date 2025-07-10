using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions;

namespace CargoWise.RefDbRepo.FRReferenceData.CmdLine
{
	public static class Resumer
	{
		public static string ParseStatus()
		{
			if (!File.Exists(resumeTextFilePath))
			{
				WriteOutStatus(DoNotResume);
			}
			return	File.ReadLines(resumeTextFilePath).Where(x => !x.StartsWith(ProcessStartDate, System.StringComparison.CurrentCulture)).FirstOrDefault();
		}

		public static string GetTariffFromStatus(string status)
		{
			var result = "";

			if (status.Contains(Resumer.ResumeDownload))
			{
				var splittedResumeStatus = status.Split(new char[] { ' ' });
				if (splittedResumeStatus.Length > 2)
				{
					result = splittedResumeStatus[2];
				}
			}

			return result;
		}
		public static void WriteOutStatus(string status)
		{
			try
			{
				File.WriteAllText(resumeTextFilePath, status);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				throw new ResumerException(e.Message, e);
			}
		}

		public static DateTime GetProcessStartDate()
		{
			var result = DateTime.Today.AddDays(-1);

			if (File.Exists(lastRunTextFilePath))
			{
				var resumerProcessStartDate = File.ReadLines(lastRunTextFilePath).FirstOrDefault();
				if (!string.IsNullOrEmpty(resumerProcessStartDate))
				{
					if (!DateTime.TryParseExact(resumerProcessStartDate.Split(' ').Last(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
					{
						result = DateTime.Today;
					}
				}
			}

			return result;
		}

		public static void StoreProcessStartDate(DateTime date)
		{
			var newProcessStartDate = $"{ProcessStartDate} {date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
			try
			{
				File.WriteAllText(lastRunTextFilePath, newProcessStartDate);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				throw new ResumerException(e.Message, e);
			}
		}

		static string resumeTextFilePath = Path.Combine(ApplicationConfig.Instance.DownloadDirectory, "Resume.txt");
		static string lastRunTextFilePath = Path.Combine(ApplicationConfig.Instance.DownloadDirectory, "LastRun.txt");

		public const string ResumeDownload = "Resume download";
		public const string ResumeGeneration = "ResumeGeneration";
		public const string DoNotResume = "DoNotResume";
		public const string Unknown = "Unknown";
		public const string ProcessStartDate = "ProcessStartDate";
	}
}
