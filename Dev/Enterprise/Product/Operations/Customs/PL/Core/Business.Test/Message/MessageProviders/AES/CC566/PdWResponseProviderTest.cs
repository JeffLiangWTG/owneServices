using System;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class PdWResponseProviderTest : Customs.Business.Testing.DataProviderTestCase<PdWResponseProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(
		"Null BaseMessageSendingObject", "Value cannot be null.\r\nParameter name: sendingObject", () => new CC514CExportOperationProvider(null));

	[TestDate(2023, 12, 22)]
	public void TestNotificationDate() => AssertEquals(new DateTime(2023, 12, 22), Provider.NotificationDate);

	public void TestCorrectionAcceptance() => CombineAssertions(() =>
	{
		messageSendingObject.CorrectionAcceptance = ZString.Empty;
		AssertEquals("Is not defined.", expected: false, actual: Provider.CorrectionAcceptance);

		messageSendingObject.CorrectionAcceptance = MessageSendingObjectCorrectionAcceptanceList.Codes._1;
		AssertEquals("Defined as 1.", expected: true, actual: GetProvider().CorrectionAcceptance);

		messageSendingObject.CorrectionAcceptance = MessageSendingObjectCorrectionAcceptanceList.Codes._0;
		AssertEquals("Defined as 0.", expected: false, actual: GetProvider().CorrectionAcceptance);
	});

	public void TestAcceptanceComment()
	{
		messageSendingObject.AcceptanceComment = "TestComment";
		AssertEquals("TestComment", Provider.AcceptanceComment);
	}

	protected override PdWResponseProvider GetProvider() => new(messageSendingObject);

	protected override void SetUp()
	{
		base.SetUp();

		var jobDeclaration = Factory.New<JobDeclaration>();
		var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		messageSendingObject = new BaseMessageSendingObject(cusEntryHeader);
	}

	BaseMessageSendingObject messageSendingObject;
}
