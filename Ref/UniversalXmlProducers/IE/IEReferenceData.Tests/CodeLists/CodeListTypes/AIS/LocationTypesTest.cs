namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS.Tests
{
	class LocationTypesTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AIS;

		protected override string ExpectedCode => "LOC";

		protected override string ExpectedNameInFile => "CL347 - Type of Location";

		protected override string ExpectedTableTitleInFile => "Code Name / description ";

		protected override string ExpectedCodeFormattingRegularExpression => "([A-Z]{1})";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new LocationTypesDetails();
	}
}
