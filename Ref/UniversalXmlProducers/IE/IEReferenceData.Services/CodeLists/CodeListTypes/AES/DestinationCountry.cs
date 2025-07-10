using System;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class DestinationCountry : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.AESCodeTypes.DestinationCountry;

		public string NameInFile => "CL008 – CL Country";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => "([A-Z]{2})";
	}
}
