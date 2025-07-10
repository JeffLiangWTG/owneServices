using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS
{
	public class AdditionalReferenceNational : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AIS;

		public string Code => Constants.AISCodeTypes.AdditionalReferenceType;

		public string NameInFile => "CL380 - Additional reference type (National)";

		public string TableTitleInFile => "Code Name / description";

		public string CodeFormattingRegularExpression => "([0-9][A-Z0-9]{3})";

		public override bool AllowCombination => true;
	}
}
