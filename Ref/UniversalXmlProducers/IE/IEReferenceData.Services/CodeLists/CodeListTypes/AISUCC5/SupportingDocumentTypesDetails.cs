using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5
{
	public class SupportingDocumentTypesDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AISUCC5;

		public string Code => Constants.AISCodeTypes.SupportingDocumentType;

		public string NameInFile => "Common documents type (TARIC)";

		public string TableTitleInFile => "";

		public string CodeFormattingRegularExpression => "([0-9][A-Z][0-9]{2})|([0-9][A-Z]{3})|([A-Z][0-9]{3})";
	}
}
