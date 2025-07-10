using System;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CC566ExportOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<CC566ExportOperationProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null BaseMessageSendingObject", "Value cannot be null.\r\nParameter name: sendingObject", () => new CC566ExportOperationProvider(null));
	}

	public void TestLrn()
	{
		cusEntryHeader.CH_BGMReference = "TestLRN";
		AssertEquals("TestLRN", Provider.Lrn);
	}

	public void TestMrn()
	{
		cusEntryHeader.MovementReferenceNumberSetter("TestMRN", ZDateTime.Now);
		var sendingObject = new BaseMessageSendingObject(cusEntryHeader);
		var provider = new CC566ExportOperationProvider(sendingObject);
		AssertEquals("TestMRN", provider.Mrn);
	}

	public void TestControlNotificationDateAndTime() => AssertEquals(default(DateTime), Provider.ControlNotificationDateAndTime);

	public void TestNotificationType() => AssertNull(Provider.NotificationType);

	public void TestAnticipatedControlDate() => AssertNull(Provider.AnticipatedControlDate);

	public void TestText() => AssertNull(Provider.Text);

	protected override CC566ExportOperationProvider GetProvider() => new(sendingObject);

	protected override void SetUp()
	{
		base.SetUp();

		var jobDeclaration = Factory.New<JobDeclaration>();
		cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		sendingObject = new BaseMessageSendingObject(cusEntryHeader);
	}

	CusEntryHeader cusEntryHeader;
	BaseMessageSendingObject sendingObject;
}
