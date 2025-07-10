using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class BusinessRejectionType : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.AESCodeTypes.BusinessRejectionType;

		public string NameInFile => "CL570";

		public string CodeFormattingRegularExpression => "([0-9]{3})";

		public string TableTitleInFile => "Code Name / description ";

		public override bool IsPublished => false;
	}
}
