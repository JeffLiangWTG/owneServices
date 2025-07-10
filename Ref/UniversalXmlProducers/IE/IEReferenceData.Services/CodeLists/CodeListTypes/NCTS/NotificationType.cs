using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
	public class NotificationType : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.AESCodeTypes.NotificationType;

		public string NameInFile => "CL384 – CL Notification Type";

		public string CodeFormattingRegularExpression => "([0-9]{1})";

		public string TableTitleInFile => "Code Name / description ";

		public override bool AllowCombination => true;
	}
}
