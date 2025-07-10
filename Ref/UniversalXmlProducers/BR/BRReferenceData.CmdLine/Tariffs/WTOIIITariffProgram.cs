using System;
using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class WTOIIITariffProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var bFile = RateOMCTecDownloader.DownloadOMCTecFile(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "OMC Tec file for WTO";

		protected override string LogFileSuffix => Constants.OMCTecWTOLogName;

		protected override void ExportToXMLFile(byte[] bFile)
		{
			using (var stream = new MemoryStream(bFile))
			{
				var outputFile = GetOutputFilePath($"RefCusTariff_BR_{Constants.TariffTypes.Codes.WTOIII}.xml");

				new WTOIIITariffParser("BR WTOIII Tariff").ExportToXMLFile(stream, outputFile, DateTime.Now);
			}
		}
	}
}
