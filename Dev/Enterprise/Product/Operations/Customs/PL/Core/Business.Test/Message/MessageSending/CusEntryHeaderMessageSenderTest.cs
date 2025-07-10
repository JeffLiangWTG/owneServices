using System;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Moq;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CusEntryHeaderMessageSenderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null BaseMessageSendingObjectParent", "Value cannot be null.\r\nParameter name: sendingObjectParent", () => new CusEntryHeaderMessageSenderForTest(testFactory, null, null));
			AssertExceptionThrown<ArgumentNullException>("Null BaseMessageSendingObject", "Value cannot be null.\r\nParameter name: sendingObject", () => new CusEntryHeaderMessageSenderForTest(testFactory, null, sendingObjectParent));
		});
	}

	public void TestSendChangesMessage()
	{
		var messageSender = new CusEntryHeaderMessageSenderForTest(testFactory, sendingObject, sendingObjectParent);
		var message = messageSender.Send();

		var testEntryHeader = testFactory.Load<CusEntryHeader>(entryHeader.PK);
		CombineAssertions(() =>
		{
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.PLCustoms, message.EM_ApplicationCode);
			AssertEquals("EM_ApplicationReference", ZString.Empty, message.EM_ApplicationReference);
			AssertEquals("EM_MessageType", declaration.JE_MessageType, message.EM_MessageType);

			AssertSame("EM_LinkedObject", testEntryHeader, message.EM_LinkedObject);
			AssertEquals("EM_LinkUniqueID", testEntryHeader.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_IsActive", false, message.EM_IsActive);
		});
	}

	public void TestSendChangesEntryHeader()
	{
		var messageSender = new CusEntryHeaderMessageSenderForTest(testFactory, sendingObject, sendingObjectParent);
		var message = messageSender.Send();

		var testEntryHeader = testFactory.Load<CusEntryHeader>(entryHeader.PK);
		CombineAssertions(() =>
		{
			AssertSame("Entry header message", message, testEntryHeader.Messages[0]);
			AssertEquals("Entry header status", Common.Shared.MessageStatusList.Codes.Sent, testEntryHeader.CH_Status);
		});
	}

	public void TestGetXmlMessage()
	{
		var expectedEM_MessageText = "<Something>123</Something>";
		var expectedEM_MessageDataLength = expectedEM_MessageText.Length;

		var messageSender = new CusEntryHeaderMessageSenderForTest(testFactory, sendingObject, sendingObjectParent);
		messageSender.XmlMessage_ForTest = $"<messageRoot>{expectedEM_MessageText}</messageRoot>";

		messageSender.Send();

		var testEntryHeader = testFactory.Load<CusEntryHeader>(entryHeader.PK);
		var message = testEntryHeader.Messages[0];
		CombineAssertions(() =>
		{
			AssertContains("EM_MessageText", expectedEM_MessageText, message.EM_MessageText);
			AssertNotEquals("EM_MessageData", 0, message.EM_MessageData.Length);
		});
	}

	public void TestSendChangesFieldsOnSave()
	{
		var messageSender = new CusEntryHeaderMessageSenderForTest(testFactory, sendingObject, sendingObjectParent);
		messageSender.XmlMessage_ForTest = $"<message>Test message with placeholder {EDIMessage.PLMessageNumberPlaceHolder}</message>";

		var message = messageSender.Send();

		CombineAssertions(() =>
		{
			AssertEquals("EM_IsActive before save", false, message.EM_IsActive);
			AssertContains("EM_MessageText should contain placeholder before save", EDIMessage.PLMessageNumberPlaceHolder, message.EM_MessageText);

			testFactory.Save();

			AssertEquals("EM_IsActive after save", true, message.EM_IsActive);
			AssertNotContains("EM_MessageText shouldn't contain placeholder after save", EDIMessage.PLMessageNumberPlaceHolder, message.EM_MessageText);
		});
	}

	protected override void SetUp()
	{
		var glbStaffCertificate = GlbStaff.CurrentUser.GetPLWrapper().PLBPassword;
		glbStaffCertificate.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		glbStaffCertificate.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
		glbStaffCertificate.Factory.Save();

		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		Factory.Save();
		testFactory = new BusinessObjectFactory();

		sendingObjectParent = new BaseMessageSendingObjectParent(declaration);
		sendingObject = (BaseMessageSendingObject)sendingObjectParent.SendingObjectsCollection.First();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	BusinessObjectFactory testFactory;
	BaseMessageSendingObjectParent sendingObjectParent;
	BaseMessageSendingObject sendingObject;

	class CusEntryHeaderMessageSenderForTest : CusEntryHeaderMessageSender
	{
		public CusEntryHeaderMessageSenderForTest(BusinessObjectFactory factory, BaseMessageSendingObject sendingObject, BaseMessageSendingObjectParent sendingObjectParent)
			: base(factory, sendingObject, sendingObjectParent)
		{
		}
		public string XmlMessage_ForTest { get; set; }

		protected override ZString GetMessageSubType() => ZString.Empty;

		protected override IXmlMessageBuilder GetXmlMessageBuilder() => Mock.Of<IXmlMessageBuilder>(b
			=> b.GenerateXmlMessage() == Mock.Of<IXmlMessage>(m => m.GetSerializedStream() == (string.IsNullOrEmpty(XmlMessage_ForTest) ? null : new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(XmlMessage_ForTest)))));
	}
}
