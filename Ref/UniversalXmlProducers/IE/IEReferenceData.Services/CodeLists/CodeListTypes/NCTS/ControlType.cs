using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
    public class ControlType : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.CommonCodeTypes.TypeOfControls;

		public string NameInFile => "CL716 – CL Control Type";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => "([0-9]{2})";

        public override bool AllowCombination => true;
    }
}
