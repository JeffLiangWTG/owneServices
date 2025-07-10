using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class DutyTaxationRegimeCodeListProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var downloader = new DutyTaxationRegimeCodeListDownloader();
				var bFile = downloader.Download(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "Taxation Regime Duty XML";

		protected override string LogFileSuffix => Constants.TaxationRegimeDuty;

		protected override void ExportToXMLFile(byte[] bFile)
		{
			var outputFileName = GetOutputFilePath($"RefCusCodeList_BR_{Constants.RefCusCodeTypes.CustomsTaxationRegimeDuty.Code}.xml");
			var typeOutputFileName = GetOutputFilePath($"RefCusCodeType_BR_{Constants.RefCusCodeTypes.CustomsTaxationRegimeDuty.Code}.xml");

			using (var stream = new MemoryStream(bFile))
			{
				new DutyTaxationRegimeCodeListParser("BR Duty Taxation Regime Code List").ExportToXMLFile(stream, outputFileName, typeOutputFileName);
			}
		}
	}
}
