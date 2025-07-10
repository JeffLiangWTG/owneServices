using System;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class TariffTecProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var bFile = TariffTecDownloader.Download(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "Tec file";

		protected override string LogFileSuffix => Constants.TecLogName;

		protected override void ExportToXMLFile(byte[] bFile)
		{
			throw new NotImplementedException();
		}
	}
}
