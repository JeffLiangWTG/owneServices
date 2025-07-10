using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class ExitControlResultCode : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.AESCodeTypes.ExitControlResultCode;

		public string NameInFile => "CL393 - CL Exit Control Result Code";

		public string CodeFormattingRegularExpression => "([A-Z]{1}[0-9]{1})";

		public string TableTitleInFile => "Code Name / description ";
	}
}
