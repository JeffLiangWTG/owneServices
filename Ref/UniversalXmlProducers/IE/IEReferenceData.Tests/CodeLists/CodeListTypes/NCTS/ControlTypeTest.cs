using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
    class ControlTypeTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.NCTS;

		protected override string ExpectedCode => "CL716";

		protected override string ExpectedNameInFile => "CL716 – CL Control Type";

		protected override string ExpectedTableTitleInFile => "Code Name / description ";

		protected override string ExpectedCodeFormattingRegularExpression => "([0-9]{2})";

		protected override bool ExpectedAllowCombination => true;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new ControlType();
	}
}
