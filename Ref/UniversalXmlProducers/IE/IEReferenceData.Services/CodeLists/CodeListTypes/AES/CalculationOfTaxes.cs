using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class CalculationOfTaxes : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.AESCodeTypes.CalculationOfTaxesType;

		public string NameInFile => "CL104 - CL Calculation of taxes - Method of payment";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => "([A-Z]{1})";
	}
}
