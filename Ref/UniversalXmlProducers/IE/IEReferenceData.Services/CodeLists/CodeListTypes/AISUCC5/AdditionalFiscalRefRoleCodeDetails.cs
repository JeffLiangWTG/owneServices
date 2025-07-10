using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5
{
	public class AdditionalFiscalRefRoleCodeDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AISUCC5;

		public string Code => Constants.AISCodeTypes.AdditionalFiscalRefRoleCode;

		public string NameInFile => "Additional fiscal ref role code";

		public string TableTitleInFile => "";

		public string CodeFormattingRegularExpression => @"FR\d";
	}
}
