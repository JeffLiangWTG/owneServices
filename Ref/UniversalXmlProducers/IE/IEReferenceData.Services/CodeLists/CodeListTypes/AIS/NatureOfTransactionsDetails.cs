using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS
{
	public class NatureOfTransactionsDetails : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AIS;

		public string Code => Constants.AISCodeTypes.NatureTransaction;

		public string NameInFile => "CL091 - Nature of Transaction Code";

		public string CodeFormattingRegularExpression => "([0-9]{1,2})";

		public string TableTitleInFile => "Code Description ";
	}
}
