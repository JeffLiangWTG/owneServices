using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5
{
	public class AdditionalProcedureDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AISUCC5;

		public string Code => Constants.CommonCodeTypes.AdditionalProcedure;

		public string NameInFile => "Additional procedure";

		public string TableTitleInFile => "";

		public string CodeFormattingRegularExpression => "[A-Z0-9]{3}";

		public override bool AllowCombination => true;
	}
}
