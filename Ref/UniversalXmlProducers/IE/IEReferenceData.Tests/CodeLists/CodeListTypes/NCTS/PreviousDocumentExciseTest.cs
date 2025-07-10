using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
    class PreviousDocumentExciseTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.NCTS;

		protected override string ExpectedCode => "CL234";

		protected override string ExpectedNameInFile => "CL234 – CL Previous Document Excise";

		protected override string ExpectedTableTitleInFile => "Code Name / description ";

		protected override string ExpectedCodeFormattingRegularExpression => "C([0-9]{3})";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => false;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new PreviousDocumentExcise();
	}
}
