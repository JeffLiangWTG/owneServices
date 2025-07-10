using System;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class TariffRateIPIProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var bFile = TariffRateIPIDownloader.Download(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "IPI Home";

		protected override string LogFileSuffix => Constants.TIPILogName;

		protected override void ExportToXMLFile(byte[] bFile)
		{
			throw new NotImplementedException();
		}
	}
}
