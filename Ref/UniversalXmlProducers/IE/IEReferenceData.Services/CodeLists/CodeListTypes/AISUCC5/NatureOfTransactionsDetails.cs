using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5
{
	public class NatureOfTransactionsDetails : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AISUCC5;

		public string Code => Constants.AISCodeTypes.NatureTransaction;

		public string NameInFile => "Nature of transaction";

		public string CodeFormattingRegularExpression => "([0-9]{1,2})";

		public string TableTitleInFile => "";
	}
}
