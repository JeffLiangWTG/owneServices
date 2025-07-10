using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
	public class UNDangerousGoodsCode : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.NctsCodeTypes.UNDangerousGoodsCodeType;

		public string NameInFile => "CL101 – CL UN Dangerous Goods Code";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => @"([0-9]{4})";
	}
}
