using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS
{
	public class LocationOfGoodsDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AIS;

		public string Code => Constants.AISCodeTypes.GoodsLocation;

		public string NameInFile => "RL020 - Location of goods code";

		public string CodeFormattingRegularExpression => @"([A-Z]{2}000000)|(IE[A-Z]{3}[0-9]{3})";

		public string TableTitleInFile => "Code Description";
	}
}
