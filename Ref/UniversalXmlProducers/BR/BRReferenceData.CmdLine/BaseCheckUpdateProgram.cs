using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public abstract class GenericCheckUpdateProgram<T> : BaseProgram
	{
		protected sealed override void RunCore()
		{
			var downloadContent = DowloadContent();
			CheckDowloadContent(downloadContent);

			if (DataSourceHasUpdate(downloadContent))
			{
				ExportToXMLFile(downloadContent);
				UpdateLogFile(downloadContent);
				Console.WriteLine($"File {DataSourceFriendlyName}, exported with success!");
			}
			else
			{
				Console.WriteLine($"No update found on {DataSourceFriendlyName}!");
			}
		}

		protected virtual HttpClient GetHttpClient(HttpClientHandler handler = null) => HttpClientUtils.New();

		protected abstract string DataSourceFriendlyName { get; }

		protected abstract T DowloadContent();

		protected abstract void CheckDowloadContent(T downloadedContent);

		protected abstract string LogFileSuffix { get; }

		protected abstract void ExportToXMLFile(T bFile);

		public string LogFilePath => logFilePath ?? (logFilePath = GetLogFilePath());
		string logFilePath;

		string GetLogFilePath()
		{
			var executePath = Assembly.GetExecutingAssembly().Location;
			return Path.Combine(Path.GetDirectoryName(executePath), Path.GetFileNameWithoutExtension(executePath) + LogFileSuffix + ".log");
		}

		public bool DataSourceHasUpdate(T downloadedContent)
		{
			return downloadedContent != null && (!File.Exists(LogFilePath) || CompareDownloadedContentWithLog(downloadedContent));
		}

		protected abstract bool CompareDownloadedContentWithLog(T downloadedContent);

		public abstract void UpdateLogFile(T downloadedContent);

		public void DeleteLogFile()
		{
			if (File.Exists(LogFilePath))
			{
				File.Delete(LogFilePath);
			}
		}
	}

	public abstract class BaseCheckUpdateProgram : GenericCheckUpdateProgram<byte[]>
	{
		protected override void CheckDowloadContent(byte[] downloadedContent)
		{
			if (downloadedContent == null || downloadedContent.Length == 0)
			{
				throw new InvalidOperationException($"The application was unable to download {DataSourceFriendlyName}.");
			}
		}

		protected override bool CompareDownloadedContentWithLog(byte[] downloadedContent)
		{
			return !CompareUtils.AreEquals(File.ReadAllBytes(LogFilePath), downloadedContent);
		}

		public override void UpdateLogFile(byte[] downloadedContent)
		{
			File.WriteAllBytes(LogFilePath, downloadedContent);
		}
	}
}
