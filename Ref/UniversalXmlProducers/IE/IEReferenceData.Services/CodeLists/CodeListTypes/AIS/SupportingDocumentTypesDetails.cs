using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS
{
	public class SupportingDocumentTypesDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AIS;

		public string Code => Constants.AISCodeTypes.SupportingDocumentType;

		public string NameInFile => "CL213 - Supporting Document Type";

		public string TableTitleInFile => "Code Name / description";

		public string CodeFormattingRegularExpression => "([A-Z][0-9]{3})";
	}
}
