using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS
{
	public class AdditionalDeclarationTypesDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AIS;

		public string Code => Constants.AISCodeTypes.AdditionalDeclarationType;

		public string NameInFile => "CL042 - Additional Declaration Type";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => "[A-Z]";
	}
}
