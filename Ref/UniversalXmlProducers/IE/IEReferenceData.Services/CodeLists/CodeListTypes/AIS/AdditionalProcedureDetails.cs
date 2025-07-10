using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS
{
	public class AdditionalProcedureDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AIS;

		public string Code => Constants.CommonCodeTypes.AdditionalProcedure;

		public string NameInFile => "CL457 - Additional Procedure";

		public string TableTitleInFile => "Code Name / description";

		public string CodeFormattingRegularExpression => "([A-Z][0-9]{2})|([0]{3})|([0-9][A-Z][0-9])";

		public override bool AllowCombination => true;
	}
}
