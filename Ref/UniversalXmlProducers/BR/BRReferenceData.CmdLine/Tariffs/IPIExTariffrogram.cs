using System;
using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class IPIExTariffrogram : BaseCheckUpdateProgram
	{
		protected override string DataSourceFriendlyName => "IPI EX Tariff files";

		protected override string LogFileSuffix => Constants.ExTariffIPILogName;

		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				return IPIExTariffDownloader.Download(client);
			}
		}

		protected override void ExportToXMLFile(byte[] bFile)
		{
			using (var stream = new MemoryStream(bFile))
			{
				var outputFileName = GetOutputFilePath($"RefCusTariff_{Constants.DataGroupingCodes.Brazil}_{Constants.TariffTypes.Codes.ExTariffIPI}.xml");

				new IPIExTariffParser("BR IPI Ex Tariff").ExportToXMLFile(stream, outputFileName, DateTime.Now);
				Console.WriteLine($"RefCusTariffExTariffIPI records generated to {outputFileName}");
			}
		}
	}
}
