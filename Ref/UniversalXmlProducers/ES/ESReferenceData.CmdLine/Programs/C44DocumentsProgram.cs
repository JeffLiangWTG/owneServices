using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using static CargoWise.RefDbRepo.ESReferenceData.Services.C44DocumentsProvider;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine
{
	public static class C44DocumentsProgram
	{
		public static void Run(string outputPath)
		{
			var dateTimeProvider = new DateTimeProvider();

			var c44DocumentsProvider = new C44DocumentsProvider();
			var documents = c44DocumentsProvider.RequestCodes(ApplicationConfig.C44DocumentsURL, Helper.GetDateWithEsFormat(dateTimeProvider.CurrentLocalDate));

			var csvCSRDT213 = RequestCodesNCTS(NctsC44Type.CSRDT213);
			var csvTRSUPNAC = RequestCodesNCTS(NctsC44Type.TRSUPNAC);
			var csvTRSUPNCA = RequestCodesNCTS(NctsC44Type.TRSUPNCA);
			var csvTRSUPNHO = RequestCodesNCTS(NctsC44Type.TRSUPNHO);
			var csvTRSUPNPA = RequestCodesNCTS(NctsC44Type.TRSUPNPA);
			var nctsDocuments = (csvCSRDT213, csvTRSUPNAC, csvTRSUPNCA, csvTRSUPNHO, csvTRSUPNPA);

			Program.PrintErrorMessage(new C44DocumentsParser(dateTimeProvider).ConvertRecordsToXMLFile(documents, nctsDocuments, Path.Combine(outputPath, "Ref_C44Documents_ZZ_ES.xml")));

			string RequestCodesNCTS(NctsC44Type type)
				=> c44DocumentsProvider.RequestCodesNCTS(ApplicationConfig.ElementoQueryAeatURL, Helper.GetDateWithEsFormat(dateTimeProvider.CurrentLocalDate), type);
		}
	}
}
