using System;
using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class TariffCharacteristicNVEProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var bFile = TariffCharacteristicNVEDownloader.Download(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "Customs Tariff NVE Attribute file";

		protected override string LogFileSuffix => Constants.TariffBRCharacteristicNveLogName;

		protected override void ExportToXMLFile(byte[] bFile)
		{
			using (var stream = new MemoryStream(bFile))
			{
				var outputFileName = GetOutputFilePath($"RefCusTariffBRCharacteristic_BR_{Constants.RefCusTariffBRCharacteristicCodes.CharacteristicTypes.NVE}.xml");
				var outputFileNameNomenclature = GetOutputFilePath($"RefCusTariffBRCharacteristic_Nomenclature_BR_{Constants.RefCusTariffBRCharacteristicCodes.CharacteristicTypes.NVE}.xml");

				var parser = new TariffCharacteristicNVEParser("BR Tariff Characteristic NVE", "BR Nomenclature Characteristic NVE");
				parser.ExportToXMLFile(stream, outputFileName, outputFileNameNomenclature, DateTime.Now);
			}
		}
	}
}
