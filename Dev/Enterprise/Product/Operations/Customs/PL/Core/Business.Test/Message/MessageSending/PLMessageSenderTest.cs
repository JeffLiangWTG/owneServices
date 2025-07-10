using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class PLMessageSenderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null Factory", "Value cannot be null.\r\nParameter name: factory", () => new PLMessageSenderForTest(factory: null, sendingObjectParent: sendingObjectParent));
			AssertExceptionThrown<ArgumentNullException>("Null BaseMessageSendingObjectParent", "Value cannot be null.\r\nParameter name: sendingObjectParent", () => new PLMessageSenderForTest(testFactory, sendingObjectParent: null));
		});
	}

	public void TestSendFillsMessage()
	{
		const bool testSendWithErrors = true;
		sendingObjectParent.AllowSendWithError = true;
		var messageSender = new PLMessageSenderForTest(testFactory, sendingObjectParent);
		var message = messageSender.Send();

		var plCustomsPassword = GlbStaff.CurrentUser.GetPLWrapper()?.PLBPassword;

		CombineAssertions(() =>
		{
			AssertEquals("EM_SendWithMessageErrors", testSendWithErrors, message.EM_SendWithMessageErrors);
			AssertEquals("EM_ApplicationCode", "APP", message.EM_ApplicationCode);
			AssertEquals("EM_ApplicationReference", "REF", message.EM_ApplicationReference);
			AssertEquals("EM_MessageOwner", ZString.Empty, message.EM_MessageOwner);

			AssertEquals("EM_ReceiveTransmit", EDIInterchange.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals("EM_SystemLastEditUser", GlbStaff.CurrentUser.GS_Code, message.EM_SystemLastEditUser);
			AssertEquals("EM_SystemCreateUser", GlbStaff.CurrentUser.GS_Code, message.EM_SystemCreateUser);
			AssertEquals("EM_GB", GlbBranch.CurrentBranch.PK, message.EM_GB);
			AssertEquals("EM_GE", GlbDepartment.CurrentDepartment.PK, message.EM_GE);
			AssertEquals("EM_GP", plCustomsPassword.PK, message.EM_GP);

			AssertEquals("EM_IsTestMessage", true, message.EM_IsTestMessage);
		});
	}

	protected override void SetUp()
	{
		testFactory = new BusinessObjectFactory();
		var declaration = Factory.New<JobDeclaration>();
		sendingObjectParent = new BaseMessageSendingObjectParent(declaration);
	}

	BusinessObjectFactory testFactory;
	BaseMessageSendingObjectParent sendingObjectParent;

	class PLMessageSenderForTest : PLMessageSender
	{
		public PLMessageSenderForTest(BusinessObjectFactory factory, BaseMessageSendingObjectParent sendingObjectParent)
			: base(factory, sendingObjectParent)
		{
		}

		protected override ZString MessageType => "MSG";

		protected override ZString ApplicationCode => "APP";

		protected override ZString ApplicationReference => "REF";
	}
}
