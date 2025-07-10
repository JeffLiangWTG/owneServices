using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS
{
	public class AdditionalReference : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AIS;

		public string Code => Constants.AISCodeTypes.AdditionalReferenceType;

		public string NameInFile => "CL380 - Additional reference type";

		public string TableTitleInFile => "Code Name / description";

		public string CodeFormattingRegularExpression => "([A-Z][0-9]{3})";

		public override bool AllowCombination => true;
	}
}
