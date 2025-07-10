using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5
{
	public class KindOfPackagesDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AISUCC5;

		public string Code => Constants.AISCodeTypes.KindOfPackages;

		public string NameInFile => "Kind of packages";

		public string CodeFormattingRegularExpression => "([A-Z0-9]{1,2})";

		public string TableTitleInFile => "";
	}
}
