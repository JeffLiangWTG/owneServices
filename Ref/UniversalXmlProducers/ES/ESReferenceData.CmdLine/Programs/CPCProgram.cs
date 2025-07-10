using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using static CargoWise.RefDbRepo.ESReferenceData.Services.CPCProvider;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine
{
	public static class CPCProgram
	{
		public static void Run(string outputPath)
		{
			var dateTimeProvider = new DateTimeProvider();

			var cpcExport = RequestCodes(CPCType.EXREG371);
			var concessionsExport = RequestCodes(CPCType.EXREG372);
			var cpcImport = RequestCodes(CPCType.IMREG371);
			var concessionsImport = RequestCodes(CPCType.IMREG372);
			Program.PrintErrorMessage(new ExportCPCParser(dateTimeProvider).ConvertRecordsToXMLFile<ExportCPCItem, ExportCPCConcessionItem>(cpcExport, concessionsExport, Path.Combine(outputPath, "Ref_CPCExport_ZZ_ES.xml")));
			Program.PrintErrorMessage(new ImportCPCParser(dateTimeProvider).ConvertRecordsToXMLFile<ImportCPCItem, ImportCPCConcessionItem>(cpcImport, concessionsImport, Path.Combine(outputPath, "Ref_CPCImport_ZZ_ES.xml")));

			string RequestCodes(CPCType type)
				=> new CPCProvider().RequestCodes(ApplicationConfig.ElementoQueryAeatURL, Helper.GetDateWithEsFormat(dateTimeProvider.CurrentLocalDate), type);
		}
	}
}
