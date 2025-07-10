using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
	public class XmlErrorCodesCode : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.NctsCodeTypes.XmlErrorCodesCodeType;

		public string NameInFile => "CL030 – CL Xml Error Codes Code";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => @"([0-9]{2})";
	}
}
