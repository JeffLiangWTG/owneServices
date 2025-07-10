using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5
{
	public class LocationOfGoodsDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AISUCC5;

		public string Code => Constants.AISCodeTypes.GoodsLocation;

		public string NameInFile => "Location of goods code";

		public string CodeFormattingRegularExpression => @"([A-Z]{2}000000)|(IE[A-Z]{3}[0-9]{3})";

		public string TableTitleInFile => "";
	}
}
