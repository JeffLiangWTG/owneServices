using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
	public class DeclarationTypeAdditional : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.NctsCodeTypes.DeclarationTypeAdditionalType;

		public string NameInFile => "CL042 – CL Declaration Type Additional";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => @"([A-Z]{1})";
	}
}

