using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
    public class TypeOfControls : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.CommonCodeTypes.TypeOfControls;

		public string NameInFile => "CL716 - CL Type Of Controls";

		public string CodeFormattingRegularExpression => "([0-9]{2})";

		public string TableTitleInFile => "Code Name / description ";

		public override bool AllowCombination => true;
    }
}
