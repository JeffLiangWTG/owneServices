using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class TransportDocument : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.AESCodeTypes.TransportDocumentType;

		public string NameInFile => "CL754 - CL Transport Document Type";

		public string CodeFormattingRegularExpression => "([A-Z0-9]{2}[0-9]{2})";

		public string TableTitleInFile => "Code Name / description ";
	}
}
