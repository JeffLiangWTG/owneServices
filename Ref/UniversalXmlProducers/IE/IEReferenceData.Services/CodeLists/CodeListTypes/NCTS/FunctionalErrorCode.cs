using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
	public class FunctionalErrorCode : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.AESCodeTypes.FunctionalErrorCode;

		public string NameInFile => "CL180 – CL Functional Error Code";

		public string CodeFormattingRegularExpression => "([0-9]{2})";

		public string TableTitleInFile => "Code Name / description ";

		public override bool AllowCombination => true;
	}
}
