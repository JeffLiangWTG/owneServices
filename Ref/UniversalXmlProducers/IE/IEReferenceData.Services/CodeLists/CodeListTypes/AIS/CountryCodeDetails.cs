using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS
{
	public class CountryCodeDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AIS;

		public string Code => Constants.AISCodeTypes.CountryCode;

		public string NameInFile => "CL008 - Country Codes (Full List)";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => "([A-Z]{2})";
	}
}
