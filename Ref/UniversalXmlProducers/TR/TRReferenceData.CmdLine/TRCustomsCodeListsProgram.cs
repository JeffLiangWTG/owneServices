using CargoWise.RefDbRepo.TRReferenceData.Business;
using CargoWise.RefDbRepo.TRReferenceData.Services;

namespace CargoWise.RefDbRepo.TRReferenceData.CmdLine
{
    public static class TRCustomsCodeListsProgram
    {
        public static void Run() => RunWithOutputPath(ApplicationConfig.OutputPath);

        static void RunWithOutputPath(string outputPath)
        {
            var parser = new TRCustomsCodeListParser();
            var codeTypes = new[]
            {
                Business.Constants.RefCusCodeType.TariffAdditionalCodeListCode,
                Business.Constants.RefCusCodeType.WarehouseCodes,
				Business.Constants.RefCusCodeType.SupportingDocumentsCodes,
				Business.Constants.RefCusCodeType.ExportUnionCountryCodes
            };

            foreach (var codeType in codeTypes)
            {
                parser.GenerateUniversalReferenceData(outputPath, codeType);
                Program.PrintErrorMessage(parser.ErrorMessage);
            }
        }
    }
}
