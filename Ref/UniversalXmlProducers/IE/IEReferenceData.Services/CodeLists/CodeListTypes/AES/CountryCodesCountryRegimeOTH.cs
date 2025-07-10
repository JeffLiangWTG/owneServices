using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class CountryCodesCountryRegimeOTH : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.AESCodeTypes.CountryCodesCountryRegimeOTH;

		public string NameInFile => "CL140";

		public string CodeFormattingRegularExpression => "([A-Z]{2})";

		public string TableTitleInFile => "Code Name / description ";

		public override bool IsPublished => false;
	}
}
