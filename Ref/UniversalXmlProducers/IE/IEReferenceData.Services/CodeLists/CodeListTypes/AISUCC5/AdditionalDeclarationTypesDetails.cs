using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5
{
	public class AdditionalDeclarationTypesDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AISUCC5;

		public string Code => Constants.AISCodeTypes.AdditionalDeclarationType;

		public string NameInFile => "Additional declaration type";

		public string TableTitleInFile => "";

		public string CodeFormattingRegularExpression => "[A-Z]";
	}
}
