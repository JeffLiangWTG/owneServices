using System;
using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class LEBITTariffProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var bFile = LEBITTariffDownloader.Download(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "LEBIT file";

		protected override string LogFileSuffix => Constants.LebitLogName;

		protected override void ExportToXMLFile(byte[] bFile)
		{
			var outputFileName = GetOutputFilePath($"RefCusTariffList_BR_{Constants.TariffTypes.Codes.LEBIT}.xml");

			using (var stream = new MemoryStream(bFile))
			{
				new LEBITTariffParser("BR LEBIT Tariff").ExportToXMLFile(stream, outputFileName, DateTime.Now);
			}
		}
	}
}
