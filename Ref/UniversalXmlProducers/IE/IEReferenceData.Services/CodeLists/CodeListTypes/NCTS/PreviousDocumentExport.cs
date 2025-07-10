using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
	public class PreviousDocumentExport : RevenueCodeListDetails, IRevenueCodeListDetails, ICodeListAttributeDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.NctsCodeTypes.PreviousDocumentType;

		public string NameInFile => "CL228 – CL Previous Document Export Type";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => @"([A-Z0-9]{4})";

		public override bool AllowCombination => true;

		public bool IsCodeListAttributeNecessary => true;
	}
}
