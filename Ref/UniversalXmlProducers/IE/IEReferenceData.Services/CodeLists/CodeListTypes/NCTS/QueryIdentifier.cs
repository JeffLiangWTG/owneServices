using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
	public class QueryIdentifier : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.NctsCodeTypes.QueryIdentifierType;

		public string NameInFile => "CL054 – CL Query Identifier";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => @"([0-9]{1})";
	}
}
