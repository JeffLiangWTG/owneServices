using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.NOReferenceData.Business;
using CargoWise.RefDbRepo.NOReferenceData.Business.ReferenceCodes;
using CargoWise.RefDbRepo.NOReferenceData.Services;
using CargoWise.RefDbRepo.NOReferenceData.Services.ErrorCodes;
using CargoWise.RefDbRepo.NOReferenceData.Services.ReferenceCodes;

namespace CargoWise.RefDbRepo.NOReferenceData.CmdLine.ReferenceCodes
{
	static class ReferenceCodesProgram
	{
		internal static void Run()
		{
			Parallel.Invoke(
				() =>
				{
					var downloadUrl = new Uri(ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceImportReferenceFilename);
					ProcessAndDownloadReferenceCodes(downloadUrl, Constants.CodeTypes.DC44CodeImport, "RefCusCodeListZZ_NO_DC44I.xml");
				},
				() =>
				{
					var downloadUrl = new Uri(ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceExportReferenceFilename);
					ProcessAndDownloadReferenceCodes(downloadUrl, Constants.CodeTypes.DC44CodeExport, "RefCusCodeListZZ_NO_DC44E.xml");
				},
				() =>
				{
					var downloadUrl = new Uri(ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceErrorCodesFilename);
					ProcessAndDownloadErrorCodes(downloadUrl, Constants.CodeTypes.ErrorMsgCodes, "RefCusCodeListZZ_NO_ERRCD.xml");
				});
		}

		static void ProcessAndDownloadReferenceCodes(Uri resourceSearchUrl, string refCuscodeType, string filename)
		{
			var client = new HttpClient();
			var (downloadErrors, xmlData, resourceData) = DownloadContent.Download<referanserListe>(client, resourceSearchUrl, "CargoWise.RefDbRepo.NOReferenceData.Services.ReferenceCodes.referanse.xsd");
			if (!Program.PrintErrorMessage("Error fetching ReferenceCodes: ", downloadErrors))
			{
				var filenameWithPath = Path.Combine(ApplicationConfig.OutputDirectory, filename);
				Program.PrintErrorMessage(ReferenceCodesParser.ConvertToXmlFile(xmlData, resourceData.LastModified, refCuscodeType, filenameWithPath));
			}
		}

		static void ProcessAndDownloadErrorCodes(Uri resourceSearchUrl, string refCuscodeType, string filename)
		{
			var client = new HttpClient();
			var (downloadErrors, xmlData, resourceData) = DownloadContent.Download<FeilmeldingListe>(client, resourceSearchUrl, "CargoWise.RefDbRepo.NOReferenceData.Services.ReferenceCodes.feilmelding.xsd");
			if (!Program.PrintErrorMessage("Error fetching ErrorMsgCodes: ", downloadErrors))
			{
				var filenameWithPath = Path.Combine(ApplicationConfig.OutputDirectory, filename);
				Program.PrintErrorMessage(ErrorCodesParser.ConvertToXmlFile(xmlData, resourceData.LastModified, refCuscodeType, filenameWithPath));
			}
		}
	}
}
