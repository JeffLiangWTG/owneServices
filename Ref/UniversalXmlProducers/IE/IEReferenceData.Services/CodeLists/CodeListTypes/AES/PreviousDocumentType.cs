using System;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class PreviousDocumentType : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.AESCodeTypes.PreviousDocumentType;

		public string NameInFile => "CL214 - CL Previous Document Type";

		public string CodeFormattingRegularExpression => "([A-Z0-9]{4})";

		public string TableTitleInFile => "Code Name / description ";
	}
}
