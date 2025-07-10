using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS
{
	public class PreviousDocumentTypesDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AIS;

		public string Code => Constants.AISCodeTypes.PreviousDocumentType;

		public string NameInFile => "CL214 - Previous document type";

		public string CodeFormattingRegularExpression => "([A-Z][0-9]{3})|([A-Z]{4})";

		public string TableTitleInFile => "Code Name / description ";
	}
}
