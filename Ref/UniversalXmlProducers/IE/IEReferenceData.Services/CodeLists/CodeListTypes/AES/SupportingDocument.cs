using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class SupportingDocument : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.AESCodeTypes.SupportingDocumentType;

		public string NameInFile => "CL213 - CL Supporting Document Type";

		public string CodeFormattingRegularExpression => "([A-Z][0-9]{3})";

		public string TableTitleInFile => "Code Name / description ";
	}
}
