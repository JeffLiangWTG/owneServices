using System;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class TariffGMCProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var downloader = new TariffGMCDownloader();
				var bFile = downloader.Download(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "GMC file";

		protected override string LogFileSuffix => Constants.GMCLogName;

		protected override void ExportToXMLFile(byte[] bFile)
		{
			throw new NotImplementedException();
		}
	}
}
