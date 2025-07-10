namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES.Tests
{
	class NotificationTypeTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AES;
		protected override string ExpectedCode => "CL384";
		protected override string ExpectedNameInFile => "CL384 - CL Notification Type";
		protected override string ExpectedTableTitleInFile => "Code Name / description ";
		protected override string ExpectedCodeFormattingRegularExpression => "([0-9]{1})";
		protected override bool ExpectedAllowCombination => true;
		protected override bool ExpectedIsPublished => true;
		protected override IRevenueCodeListDetails GetCodeListDetails() => new NotificationType();
	}
}
