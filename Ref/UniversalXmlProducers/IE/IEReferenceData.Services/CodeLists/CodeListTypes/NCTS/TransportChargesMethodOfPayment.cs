using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
	public class TransportChargesMethodOfPayment : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.NctsCodeTypes.TransportChargesMethodOfPaymentType;

		public string NameInFile => "CL116 – CL Transport Charges Method οf Payment";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => @"([A-Z]{1})";
	}
}
