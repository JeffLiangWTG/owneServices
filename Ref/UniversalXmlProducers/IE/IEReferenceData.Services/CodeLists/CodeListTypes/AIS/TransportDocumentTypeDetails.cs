using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS
{
	public class TransportDocumentTypeDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AIS;

		public string Code => Constants.AISCodeTypes.TransportDocumentType;

		public string NameInFile => "CL754 - Transport document type";

		public string TableTitleInFile => "Code Name / description";

		public string CodeFormattingRegularExpression => "([A-Z][0-9]{3})|([0-9][A-Z][0-9]{2})";
	}
}
