using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class WarehousingSectorsCodeListProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var downloader = new WarehousingSectorsCodeListDownloader();
				var bFile = downloader.Download(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "Warehousing Sectors file";

		protected override string LogFileSuffix => Constants.WarehousingSectorsLogName;

		protected override void ExportToXMLFile(byte[] bFile)
		{
			var outputFileName = GetOutputFilePath($"RefCusCodeList_BR_{Constants.RefCusCodeTypes.WarehousingSectorsCodes.Code}.xml");
			var typeOutputFileName = GetOutputFilePath($"RefCusCodeType_BR_{Constants.RefCusCodeTypes.WarehousingSectorsCodes.Code}.xml");

			using (var stream = new MemoryStream(bFile))
			{
				new WarehousingSectorsCodeListParser($"BR {Constants.RefCusCodeTypes.WarehousingSectorsCodes.Description} Code List").ExportToXMLFile(stream, outputFileName, typeOutputFileName);
			}
		}
	}
}
