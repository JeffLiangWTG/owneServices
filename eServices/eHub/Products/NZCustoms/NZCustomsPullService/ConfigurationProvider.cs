using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;

namespace CargoWise.eHub.Products.NZCustoms.PullService
{
	public class ConfigurationProvider : IConfigurationProvider
	{
		public string PartnerID
		{
			get
			{
				if (partnerID == null)
				{
					partnerID = GetPartnerID();
				}

				return partnerID;
			}
		}

		public int PullInterval
		{
			get
			{
				if (pullIntervalInMs == 0)
				{
					pullIntervalInMs = GetPullInterval();
				}

				return pullIntervalInMs;
			}
		}

		static string GetPartnerID()
		{
			var partnerIDString = ConfigurationManager.AppSettings["CargowisePartnerID"];
			if (string.IsNullOrEmpty(partnerIDString)) throw new InvalidOperationException("CargowisePartnerID is empty");
			return partnerIDString;
		}

		string partnerID;

		int GetPullInterval()
		{
			int pullIntervalInMs;
			var pullIntervalString = ConfigurationManager.AppSettings["PullInterval"];
			if (!int.TryParse(pullIntervalString, out pullIntervalInMs))
			{
				throw new InvalidOperationException("PullInterval is not integer");
			}

			return pullIntervalInMs;
		}

		int pullIntervalInMs = 0;

		public DirectoryInfo FailedToDeliverMessageFolder
		{
			get
			{
				if (failedToDeliverMessageFolder == null)
				{
					var failedToDeliverMessageFolderString = ConfigurationManager.AppSettings["FailedToDeliverMessageFolder"];
					failedToDeliverMessageFolder = new DirectoryInfo(failedToDeliverMessageFolderString);
					if (!failedToDeliverMessageFolder.Exists) failedToDeliverMessageFolder.Create();
					if (!failedToDeliverMessageFolder.Exists) throw new InvalidOperationException(String.Format("Can't create Folder {0}", failedToDeliverMessageFolder.FullName));
				}

				return failedToDeliverMessageFolder;
			}
		}

		DirectoryInfo failedToDeliverMessageFolder;

		public string[] ExceptionMessagePatterns
		{
			get
			{
				return ConvertToArray((NameValueCollection)ConfigurationManager.GetSection("ExceptionMessagePatterns"));
			}
		}

		string[] ConvertToArray(NameValueCollection source)
		{
			var values = new List<string>();
			for (int i = 0; i < source.Count; i++)
			{
				values.Add(source[i]);
			}

			return values.ToArray();
		}

		public int RetryCount
		{
			get
			{
				if (retryCount == 0)
				{
					retryCount = GetRetryCount();
				}

				return retryCount;
			}
		}

		int GetRetryCount()
		{
			int retryCount;
			var retryCountString = ConfigurationManager.AppSettings["RetryCount"];
			if (!int.TryParse(retryCountString, out retryCount))
			{
				throw new InvalidOperationException("RetryCount is not integer");
			}

			return retryCount;
		}

		int retryCount = 0;
	}
}
