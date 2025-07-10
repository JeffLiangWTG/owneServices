namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES.Tests
{
	class BusinessRejectionTypeTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AES;
		protected override string ExpectedCode => "CL570";
		protected override string ExpectedNameInFile => "CL570";
		protected override string ExpectedTableTitleInFile => "Code Name / description ";
		protected override string ExpectedCodeFormattingRegularExpression => "([0-9]{3})";
		protected override bool ExpectedAllowCombination => false;
		protected override bool ExpectedIsPublished => false;
		protected override IRevenueCodeListDetails GetCodeListDetails() => new BusinessRejectionType();
	}
}
