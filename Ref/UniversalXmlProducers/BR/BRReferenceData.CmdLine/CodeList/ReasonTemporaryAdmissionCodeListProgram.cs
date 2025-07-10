using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class ReasonTemporaryAdmissionCodeListProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var downloader = new ReasonTemporaryAdmissionCodeListDownloader();
				var bFile = downloader.DownloadXML(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "Reason Temporary Admission file";

		protected override string LogFileSuffix => Constants.ReasonTemporaryAdmissionLogName;

		protected override void ExportToXMLFile(byte[] bFile)
		{
			var outputFileName = GetOutputFilePath($"RefCusCodeList_BR_{Constants.RefCusCodeTypes.CustomsReasonTemporaryAdmission.Code}.xml");
			var typeOutputFileName = GetOutputFilePath($"RefCusCodeType_BR_{Constants.RefCusCodeTypes.CustomsReasonTemporaryAdmission.Code}.xml");

			using (var stream = new MemoryStream(bFile))
			{
				new ReasonTemporaryAdmissionCodeListParser("BR Reason Temporary Admission Code List").ExportToXMLFile(stream, outputFileName, typeOutputFileName);
			}
		}
	}
}
