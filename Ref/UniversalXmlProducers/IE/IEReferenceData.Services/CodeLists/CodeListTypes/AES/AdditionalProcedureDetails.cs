using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class AdditionalProcedureDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.CommonCodeTypes.AdditionalProcedure;

		public string NameInFile => "CL102 – CL Additional Procedure";

		public string TableTitleInFile => "Code Name / description";

		public string CodeFormattingRegularExpression => "[A-Z0-9]{3}";

		public override bool AllowCombination => true;
	}
}
