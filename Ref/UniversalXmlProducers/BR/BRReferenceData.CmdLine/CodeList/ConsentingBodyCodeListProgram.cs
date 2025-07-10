using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class ConsentingBodyCodeListProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var bFile = ConsentingBodyCodeListDownloader.Download(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "BR Consenting Body Code List";

		protected override string LogFileSuffix => Constants.ConsentingBodyLogName;

		protected override void ExportToXMLFile(byte[] bFile)
		{
			var outputFileName = GetOutputFilePath($"RefCusCodeList_BR_{Constants.RefCusCodeTypes.CustomsConsentingBody.Code}.xml");
			var typeOutputFileName = GetOutputFilePath($"RefCusCodeType_BR_{Constants.RefCusCodeTypes.CustomsConsentingBody.Code}.xml");

			using (var stream = new MemoryStream(bFile))
			{
				new ConsentingBodyCodeListParser("BR Consenting Body Code List").ExportToXMLFile(stream, outputFileName, typeOutputFileName);
			}
		}
	}
}
