namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5.Tests
{
	class NatureOfTransactionsTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AISUCC5;

		protected override string ExpectedCode => "TRNAT";

		protected override string ExpectedNameInFile => "Nature of transaction";

		protected override string ExpectedTableTitleInFile => "";

		protected override string ExpectedCodeFormattingRegularExpression => "([0-9]{1,2})";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new NatureOfTransactionsDetails();
	}
}
