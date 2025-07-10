using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5
{
	public class ErrorTypesDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AISUCC5;

		public string Code => Constants.AISCodeTypes.ErrorType;

		public string NameInFile => "Error types";

		public string CodeFormattingRegularExpression => "([0-9]{1,2})";

		public string TableTitleInFile => "";
	}
}
