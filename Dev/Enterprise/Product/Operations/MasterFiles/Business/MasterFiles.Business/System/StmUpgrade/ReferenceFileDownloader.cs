using System;
using System.Net;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.AU.CMR
{
	public class ReferenceFileDownloader
	{
		public ReferenceFileDownloader(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly BusinessObjectFactory factory;

		#region Constants

		public string MainFile => ReferenceFileUrl + (NoResString)"/production/main/P1-MAIN.tar.gz";
		public string ChangeFile => ReferenceFileUrl + (NoResString)"/production/change/today/P1-CHNG.tar.gz";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard-coded URL")]
		public string NumberedChangeFile => ReferenceFileUrl + "/industry_test/change/today-{0}/P1-CHNG.tar.gz";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard-coded URL")]
		public string TestingMainFile => ReferenceFileUrl + "/industry_test/main/Q1-MAIN.tar.gz";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard-coded URL")]
		public string TestingChangeFile => ReferenceFileUrl + "/industry_test/change/today/Q1-CHNG.tar.gz";

		#endregion

		#region Production

		public ReferenceFile DownloadMainFile()
		{
			return new ReferenceFile(ReferenceFile.Constants.MainFileName, Download(MainFile));
		}

		public ReferenceFile DownloadChangeFile(int number)
		{
			if (number < 1 || number > 5)
			{
				throw new ReferenceFileDownloaderException("Invalid change file number was passed to DownloadChangeFile method. Acceptable values are between 1 and 5.");
			}

			ZBlob downloadedData;

#if DEBUG
			if (Globals.IsTest)
			{
				ZString timeStamp = ZDateTime.Now.AddDays(-number).ToString(ReferenceFile.Constants.TimestampFormat);
				downloadedData = ZBlob.FromAscii("P1-CHNG.tar.gz-" + timeStamp);
			}
			else
			{
#endif
				downloadedData = Download(ZString.Format(NumberedChangeFile, number));
#if DEBUG
			}
#endif
			return new ReferenceFile(ReferenceFile.Constants.ChangeFileName, downloadedData);
		}

		public ReferenceFile DownloadLatestChangeFile()
		{
			return new ReferenceFile(ReferenceFile.Constants.ChangeFileName, Download(ChangeFile));
		}

		#endregion

		#region Testing

		public ReferenceFile DownloadTestingMainFile()
		{
			return new ReferenceFile(ReferenceFile.Constants.TestingMainFileName, Download(TestingMainFile));
		}

		public ReferenceFile DownloadTestingLatestChangeFile()
		{
			return new ReferenceFile(ReferenceFile.Constants.TestingChangeFileName, Download(TestingChangeFile));
		}

		#endregion

		#region Download

		protected ZBlob Download(string uRL)
		{
			ZBlob result = ZBlob.Empty;
			try
			{
#if DEBUG
				if (Globals.IsTest)
				{
					string[] bits = uRL.Split('/');
					string lastBit = bits[bits.Length - 1];
					ZString timeStamp = ZDateTime.Now.ToString(ReferenceFile.Constants.TimestampFormat);
					result = ZBlob.FromAscii(lastBit + "-" + timeStamp);
				}
				else
				{
#endif
#pragma warning disable SYSLIB0014 // WebClient.WebClient()' is obsolete: 'WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.'
					WebClient client = new WebClient();
#pragma warning restore SYSLIB0014
					client.Proxy = WebRequest.DefaultWebProxy;
					result = client.DownloadData(uRL);
#if DEBUG
				}
#endif
			}
			catch (WebException e)
			{
				throw new ReferenceFileDownloaderException(GetErrorNotification(), e);
			}
			return result;
		}
		#endregion

		#region Url

		ZString ReferenceFileUrl
		{
			get
			{
				if (string.IsNullOrWhiteSpace(referenceFileUrl))
				{
					var urlProvider = new ReferenceFileUrlProvider(factory);
					referenceFileUrl = urlProvider.GetCustomsReferenceFileURL();
				}

				return referenceFileUrl;
			}
		}
		string referenceFileUrl;

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		protected string GetErrorNotification()
		{
			return Res.GetString("0E867629-F93B-435F-B15C-23A42030D2F3", @"Error encountered while attempting to download Reference File data from Customs. 

Please ensure that the machine running the update has an Internet connection, as {0} will be downloading files from the Customs website. 

If you continue to experience problems downloading reference files, you can manually download them from the following links and upload them to the system from the downloaded file:

Full Production Update:" + "\r\n\r\n {1}/production/main/P1-MAIN.tar.gz\r\n\r\nChanges Only Update:\r\n\r\n {1}/production/change/today/P1-CHNG.tar.gz", BrandingFactory.Instance.ProductName, ReferenceFileUrl);
		}
	}

	#region Exception

	[Serializable]
	public class ReferenceFileDownloaderException : ApplicationException
	{
		public ReferenceFileDownloaderException(string message) : base(message)
		{
		}

		public ReferenceFileDownloaderException(string message, WebException innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected ReferenceFileDownloaderException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	#endregion
}
