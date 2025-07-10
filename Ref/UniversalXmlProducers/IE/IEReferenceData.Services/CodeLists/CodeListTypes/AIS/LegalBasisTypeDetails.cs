using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS
{
	public class LegalBasisTypeDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AIS;

		public string Code => Constants.AISCodeTypes.LegalBasisCode;

		public string NameInFile => "RL140 - Legal basis";

		public string CodeFormattingRegularExpression => @"([A-Z][0-9]{2})";

		public string TableTitleInFile => "Code Description";
	}
}
