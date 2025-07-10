namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5.Tests
{
	class LocationIdentificationQualifiersTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AISUCC5;

		protected override string ExpectedCode => "QUA";

		protected override string ExpectedNameInFile => "Location identification qualifier";

		protected override string ExpectedTableTitleInFile => "";

		protected override string ExpectedCodeFormattingRegularExpression => "([A-Z]{1})";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new LocationIdentificationQualifiersDetails();
	}
}
