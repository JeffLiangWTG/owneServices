using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class AddtionalInformation : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.AESCodeTypes.AdditionalInformationType;

		public string NameInFile => "CL239 - CL Additional Information Code";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => "([0-9]{3}00)";
	}
}
