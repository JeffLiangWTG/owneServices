namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES.Tests
{
	class TransportDocumentTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AES;
		protected override string ExpectedCode => "TD44E";
		protected override string ExpectedNameInFile => "CL754 - CL Transport Document Type";
		protected override string ExpectedTableTitleInFile => "Code Name / description ";
		protected override string ExpectedCodeFormattingRegularExpression => "([A-Z0-9]{2}[0-9]{2})";
		protected override bool ExpectedAllowCombination => false;
		protected override bool ExpectedIsPublished => true;
		protected override IRevenueCodeListDetails GetCodeListDetails() => new TransportDocument();
	}
}
