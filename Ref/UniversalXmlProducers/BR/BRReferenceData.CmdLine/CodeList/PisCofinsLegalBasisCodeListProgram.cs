using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class PisCofinsLegalBasisCodeListProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var downloader = new PisCofinsLegalBasisCodeListDownloader();
				var bFile = downloader.Download(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "BR Pis/Cofins Legal Basis Code List";

		protected override string LogFileSuffix => Constants.PisCofinsLegalBasisLogName;

		protected override void ExportToXMLFile(byte[] bFile)
		{
			var outputFileName = GetOutputFilePath($"RefCusCodeList_BR_{Constants.RefCusCodeTypes.PisCofinsLegalBasis.Code}.xml");
			var typeOutputFileName = GetOutputFilePath($"RefCusCodeType_BR_{Constants.RefCusCodeTypes.PisCofinsLegalBasis.Code}.xml");

			using (var stream = new MemoryStream(bFile))
			{
				new PisCofinsLegalBasisCodeListParser("BR Pis/Cofins Legal Basis Code List").ExportToXMLFile(stream, outputFileName, typeOutputFileName);
			}
		}
	}
}
