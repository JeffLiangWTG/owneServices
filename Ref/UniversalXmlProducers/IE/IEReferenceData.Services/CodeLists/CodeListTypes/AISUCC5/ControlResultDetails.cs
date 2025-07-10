using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5
{
	public class ControlResultDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AISUCC5;

		public string Code => Constants.CommonCodeTypes.TypeOfControls;

		public string NameInFile => "Control result";

		public string CodeFormattingRegularExpression => "([A-Z0-9]{2})";

		public string TableTitleInFile => "";
	}
}
