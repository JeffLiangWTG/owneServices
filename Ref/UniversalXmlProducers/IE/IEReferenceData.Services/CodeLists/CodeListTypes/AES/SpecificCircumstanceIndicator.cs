using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class SpecificCircumstanceIndicator : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.AESCodeTypes.SpecificCircumstanceIndicatorType;

		public string NameInFile => "CL296 - CL Specific circumstance indicator";

		public string CodeFormattingRegularExpression => "([A-Z][0-9]{2})";

		public string TableTitleInFile => "Code Name / description ";
	}
}
