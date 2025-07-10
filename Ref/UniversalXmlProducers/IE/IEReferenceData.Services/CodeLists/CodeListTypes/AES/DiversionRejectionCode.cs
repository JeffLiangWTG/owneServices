using System;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class DiversionRejectionCode : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.AESCodeTypes.DiversionRejectionCodeType;

		public string NameInFile => "CL046 - CL Diversion Rejection Code";

		public string CodeFormattingRegularExpression => "([0-9]{1,2})";

		public string TableTitleInFile => "Code Name/description ";
	}
}
