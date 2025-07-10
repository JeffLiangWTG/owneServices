using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5
{
	public class DutyAndTaxType : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AISUCC5;

		public string Code => Constants.AISCodeTypes.DutyAndTaxType;

		public string NameInFile => "Tax Type";

		public string CodeFormattingRegularExpression => "[A-Z0-9]{3}";

		public string TableTitleInFile => "Code Description";
	}
}
