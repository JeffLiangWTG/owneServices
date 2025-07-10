using System;
using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class AutoPartsTariffProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var downloader = new AutoPartsTariffDownloader();
				var bFile = downloader.Download(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "Auto Parts List file";

		protected override string LogFileSuffix => Constants.AutoPartsLogName;

		protected override void ExportToXMLFile(byte[] bFile)
		{
			using (var stream = new MemoryStream(bFile))
			{
				var outputFileName = GetOutputFilePath($"RefCusTariffList_BR_{Constants.TariffTypes.Codes.AutoPartList}.xml");
				new AutoPartsTariffParser("BR Auto Parts Tariff").ExportToXMLFile(stream, outputFileName, DateTime.Now);
			}
		}
	}
}
