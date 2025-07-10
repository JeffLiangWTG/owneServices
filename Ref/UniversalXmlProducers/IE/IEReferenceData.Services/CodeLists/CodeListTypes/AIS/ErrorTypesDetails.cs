using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS
{
	public class ErrorTypesDetails : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AIS;

		public string Code => Constants.AISCodeTypes.ErrorType;

		public string NameInFile => "RL102 - Error types";

		public string CodeFormattingRegularExpression => "([0-9]{1,2})";

		public string TableTitleInFile => "Code Name / description ";
	}
}
