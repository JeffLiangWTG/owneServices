using System;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class TariffRateCovidProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var downloader = new TariffRateCovidDownloader();
				var bFile = downloader.Download(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "Rate Covid file";

		protected override string LogFileSuffix => Constants.COVIDLogName;

		protected override void ExportToXMLFile(byte[] bFile)
		{
			throw new NotImplementedException();
		}
	}
}
