using System.Diagnostics.Contracts;
using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	class CustomsOfficeCodeListProgram : BaseProgram
	{
		protected override void RunCore()
		{
			var outputFileName = GetOutputFilePath($"RefCusCodeList_BR_{Constants.RefCusCodeTypes.CustomsOffice.Code}.xml");
			var outputTypeFileName = GetOutputFilePath($"RefCusCodeType_BR_{Constants.RefCusCodeTypes.CustomsOffice.Code}.xml");

			using (var xml = new MemoryStream(DownloadURFXml()))
			{
				var parser = new CustomsOfficeCodeListParser("BR Customs Office Code List");
				parser.ExportToXMLFile(xml, outputFileName, outputTypeFileName);
			}
		}

		static byte[] DownloadURFXml()
		{
			using (var client = HttpClientUtils.New())
			{
				var downloader = new CustomsOfficeCodeListDownloader();
				var bXml = downloader.Download(client);

				Contract.Assume(bXml != null);

				return bXml;
			}
		}
	}
}
