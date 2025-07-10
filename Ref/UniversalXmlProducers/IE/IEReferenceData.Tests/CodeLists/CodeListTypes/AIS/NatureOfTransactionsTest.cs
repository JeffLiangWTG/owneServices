namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS.Tests
{
	class NatureOfTransactionsTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AIS;

		protected override string ExpectedCode => "TRNAT";

		protected override string ExpectedNameInFile => "CL091 - Nature of Transaction Code";

		protected override string ExpectedTableTitleInFile => "Code Description ";

		protected override string ExpectedCodeFormattingRegularExpression => "([0-9]{1,2})";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new NatureOfTransactionsDetails();
	}
}
