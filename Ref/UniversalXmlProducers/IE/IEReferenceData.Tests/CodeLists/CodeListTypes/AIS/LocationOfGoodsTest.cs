namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS.Tests
{
	class GoodsLocationTypeTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AIS;

		protected override string ExpectedCode => "FAC";

		protected override string ExpectedNameInFile => "RL020 - Location of goods code";

		protected override string ExpectedTableTitleInFile => "Code Description";

		protected override string ExpectedCodeFormattingRegularExpression => "([A-Z]{2}000000)|(IE[A-Z]{3}[0-9]{3})";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new LocationOfGoodsDetails();
	}
}
