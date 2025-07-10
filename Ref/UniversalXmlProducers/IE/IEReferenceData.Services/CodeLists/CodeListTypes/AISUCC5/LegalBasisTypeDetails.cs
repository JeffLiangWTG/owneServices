using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5
{
	public class LegalBasisTypeDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AISUCC5;

		public string Code => Constants.AISCodeTypes.LegalBasisCode;

		public string NameInFile => "AIS;LegalBasis";

		public string CodeFormattingRegularExpression => @"([A-Z][0-9]{2})";

		public string TableTitleInFile => "";
	}
}
