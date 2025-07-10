namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES.Tests
{
	class AddtionalInformationTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AES;
		protected override string ExpectedCode => "AI44E";
		protected override string ExpectedNameInFile => "CL239 - CL Additional Information Code";
		protected override string ExpectedTableTitleInFile => "Code Name / description ";
		protected override string ExpectedCodeFormattingRegularExpression => "([0-9]{3}00)";
		protected override bool ExpectedAllowCombination => false;
		protected override bool ExpectedIsPublished => true;
		protected override IRevenueCodeListDetails GetCodeListDetails() => new AddtionalInformation();
	}
}
