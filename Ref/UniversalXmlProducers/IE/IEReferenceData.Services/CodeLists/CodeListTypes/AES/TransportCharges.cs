using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class TransportCharges : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.AESCodeTypes.TransportChargesType;

		public string NameInFile => "CL116 – CL Transport Charges – Method of Payment";

		public string CodeFormattingRegularExpression => "([A-Z]{1})";

		public string TableTitleInFile => "Code Name / description ";
	}
}
