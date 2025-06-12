using System;
using System.IO;
using CargoWise.eHub.Products.NZCustoms.PullService;

namespace CargoWise.eHub.Products.NZCustoms.Tests
{
	class TestConfigurationProvider : IConfigurationProvider
	{
		public string PartnerID
		{
			get { return "CargoWise"; }
		}

		public int PullInterval
		{
			get { return 10000; }
		}

		public System.IO.FileInfo LogFilePath
		{
			get { throw new NotImplementedException(); }
		}

		public DirectoryInfo FailedToDeliverMessageFolder
		{
			get
			{
				return new DirectoryInfo(Path.GetTempPath());
			}
		}

		public string[] ExceptionMessagePatterns
		{
			get { return new[] { "Message specific error" }; }
		}

		public int RetryCount
		{
			get { return 1; }
		}
	}
}
