using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class DutyLegalBasisCodeListProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var downloader = new DutyLegalBasisCodeListDownloader();
				var bFile = downloader.Download(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "LRTII - Legal Basis Taxation Regime - Duty file";

		protected override string LogFileSuffix => Constants.LRTIILogName;

		protected override void ExportToXMLFile(byte[] bFile)
		{
			var outputFileName = GetOutputFilePath($"RefCusCodeList_BR_{Constants.RefCusCodeTypes.DutyLegalBasis.Code}.xml");
			var typeOutputFileName = GetOutputFilePath($"RefCusCodeType_BR_{Constants.RefCusCodeTypes.DutyLegalBasis.Code}.xml");
			var attributeOutputFileName = GetOutputFilePath($"RefCusCodeListAttributeName_BR_{Constants.RefCusCodeTypes.DutyLegalBasis.Code}.xml");

			using (var stream = new MemoryStream(bFile))
			{
				new DutyLegalBasisCodeListParser("BR Duty Legal Basis Code List").ExportToXMLFile(stream, outputFileName, typeOutputFileName, attributeOutputFileName);
			}
		}
	}
}
