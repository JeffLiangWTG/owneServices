namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS.Tests
{
	class LocationIdentificationQualifiersTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AIS;

		protected override string ExpectedCode => "QUA";

		protected override string ExpectedNameInFile => "CL326 - Qualifier of the Location Identification";

		protected override string ExpectedTableTitleInFile => "Code Name / description ";

		protected override string ExpectedCodeFormattingRegularExpression => "([A-Z]{1})";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new LocationIdentificationQualifiersDetails();
	}
}
