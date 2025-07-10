using System;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class TariffIPITableProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var bFile = TariffIPITableDownloader.Download(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "IPI Table File";

		protected override string LogFileSuffix => Constants.IPITableLogName;

		protected override void ExportToXMLFile(byte[] bFile)
		{
			throw new NotImplementedException();
		}
	}
}
