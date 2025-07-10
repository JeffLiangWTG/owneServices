using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
	public class SupportingDocumentType : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.NctsCodeTypes.SupportingDocumentType;

		public string NameInFile => "CL213 – CL Supporting Document Type";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => @"([A-Z0-9]{4})";
	}
}
