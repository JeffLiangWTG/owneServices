using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
    public class CountryCodeFullList : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.NctsCodeTypes.CountryCodeType;

		public string NameInFile => "CL008 – CL Country Codes Full List";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => "([A-Z]{2})";
	}
}
