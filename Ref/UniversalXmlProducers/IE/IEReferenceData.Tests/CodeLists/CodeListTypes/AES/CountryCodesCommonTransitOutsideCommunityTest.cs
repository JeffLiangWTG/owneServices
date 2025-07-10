namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES.Tests
{
	class CountryCodesCommonTransitOutsideCommunityTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AES;
		protected override string ExpectedCode => "CL063";
		protected override string ExpectedNameInFile => "CL063";
		protected override string ExpectedTableTitleInFile => "Code Name / description ";
		protected override string ExpectedCodeFormattingRegularExpression => "([A-Z]{2})";
		protected override bool ExpectedAllowCombination => false;
		protected override bool ExpectedIsPublished => false;
		protected override IRevenueCodeListDetails GetCodeListDetails() => new CountryCodesCommonTransitOutsideCommunity();
	}
}
