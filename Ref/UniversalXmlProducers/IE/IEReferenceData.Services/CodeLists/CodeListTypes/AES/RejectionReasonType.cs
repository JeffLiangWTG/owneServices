using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class RejectionReasonType : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.AESCodeTypes.RejectionReasonType;

		public string NameInFile => "CL560 - CL Rejection Reason Type";

		public string CodeFormattingRegularExpression => "([0-9]{3})";

		public string TableTitleInFile => "Code Name / description ";
	}
}
