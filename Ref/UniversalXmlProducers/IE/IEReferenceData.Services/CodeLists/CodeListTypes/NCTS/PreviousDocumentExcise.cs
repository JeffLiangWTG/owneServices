using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
    public class PreviousDocumentExcise : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.NctsCodeTypes.PreviousDocumentExciseType;

		public string NameInFile => "CL234 – CL Previous Document Excise";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => "C([0-9]{3})";

        public override bool IsPublished => false;
    }
}
