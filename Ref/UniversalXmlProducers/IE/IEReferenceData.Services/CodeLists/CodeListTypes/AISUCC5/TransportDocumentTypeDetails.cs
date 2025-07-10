using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5
{
	public class TransportDocumentTypeDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AISUCC5;

		public string Code => Constants.AISCodeTypes.TransportDocumentType;

		public string NameInFile => "Transport Document Type";

		public string TableTitleInFile => "";

		public string CodeFormattingRegularExpression => "([A-Z][0-9]{3})";
	}
}
