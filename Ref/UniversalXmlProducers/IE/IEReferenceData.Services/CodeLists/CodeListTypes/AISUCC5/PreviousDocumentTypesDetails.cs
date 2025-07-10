using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5
{
	public class PreviousDocumentTypesDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AISUCC5;

		public string Code => Constants.AISCodeTypes.PreviousDocumentType;

		public string NameInFile => "Previous document type";

		public string CodeFormattingRegularExpression => "([0-9]{3})|([A-Z]{3})|([A-Z]{2}[0-9])|([A-Z][0-9][A-Z])";

		public string TableTitleInFile => "";
	}
}
