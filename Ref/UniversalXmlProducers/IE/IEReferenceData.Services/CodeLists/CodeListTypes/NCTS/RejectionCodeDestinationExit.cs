using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
	public class RejectionCodeDestinationExit : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.NctsCodeTypes.RejectionCodeDestinationExitType;

		public string NameInFile => "CL227 – CL Rejection Code Destination Exit";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => "([0-9]{1,2})";
	}
}
