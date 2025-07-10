using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.AWB.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	[TestedType(typeof(DummyCMDEDIMessage))]
	sealed class CMDEDIMessageTest : CIMEDIMessageTest
	{
		protected override string ExpectedApplicationCode
		{
			get
			{
				return EDIMessage.ApplicationCodes.SingaporeCMD;
			}
		}

		public void TestIsActiveAndHasFailureReply()
		{
			MessageForTest.EM_IsActive = false;
			MessageForTest.Reply = null;
			AssertEquals(false, MessageForTest.IsActiveAndHasFailureReply);
			MessageForTest.Reply = new CMDInbound("CMA/2\r\nA/N/N\r\nTesting123");
			AssertEquals(false, MessageForTest.IsActiveAndHasFailureReply);
			MessageForTest.Reply = new CMDInbound("FNA\r\nA/N/N\r\nTesting123");
			AssertEquals(false, MessageForTest.IsActiveAndHasFailureReply);
			MessageForTest.EM_IsActive = true;
			AssertEquals(true, MessageForTest.IsActiveAndHasFailureReply);
		}

		public void TestNumberFountainType()
		{
			using (Db.Connection.BeginTransactionWithManager())
			{
				AssertEquals(Env.NumberFountains.CMDNumber.PeekPreliminaryFormatted(Db.Connection), MessageForTest.ExposedNumberFountain.PeekPreliminaryFormatted(Db.Connection));
				MessageForTest.ExposedNumberFountain.GetNextFormatted(Db.Connection);
				AssertEquals(Env.NumberFountains.CMDNumber.PeekPreliminaryFormatted(Db.Connection), MessageForTest.ExposedNumberFountain.PeekPreliminaryFormatted(Db.Connection));
				Env.NumberFountains.CMDNumber.GetNextFormatted(Db.Connection);
				AssertEquals(Env.NumberFountains.CMDNumber.PeekPreliminaryFormatted(Db.Connection), MessageForTest.ExposedNumberFountain.PeekPreliminaryFormatted(Db.Connection));
			}
		}

		public void TestDefaultValues()
		{
			AssertEquals(ExpectedApplicationCode, MessageForTest.EM_MessageType);
		}

		public void TestEM_MessageSummary()
		{
			MessageForTest.EM_MessageSubType = "001";
			MessageForTest.EM_MessageText = "CMD/2\r\nA/Y/N\r\nMWB/Testing12345\r\nHWB/Blah";
			string expected = "Action: Add\r\nLate Submission: N\r\nSent To: 001\r\nMAWB: Testing12345\r\nHAWB: Blah\r\n";
			AssertEquals(expected, MessageForTest.EM_MessageSummary);
			MessageForTest.EM_MessageSubType = "FOO";
			MessageForTest.EM_MessageText = "CMD/2\r\nM/Y/Y\r\nMWB/asdlfkj\r\n";
			expected = "Action: Modify\r\nLate Submission: Y\r\nSent To: FOO\r\nMAWB: asdlfkj\r\nHAWB: \r\n";
			AssertEquals(expected, MessageForTest.EM_MessageSummary);
			MessageForTest.EM_MessageText = "-109-2q0394-2934";
			expected = "Unable to parse CMD Message Text. Invalid or unexpected format / structure. Message starts: ";
			AssertEquals(expected + "-109-2q0394-2934", MessageForTest.EM_MessageSummary);
			CMDEDIMessage message = Factory.New<CMDEDIMessage>();
			AssertEquals(expected, message.EM_MessageSummary);
		}

		public void TestMessageReply()
		{
			var replyMessage = Factory.New<CMDEDIMessage>();
			replyMessage.EM_IsActive = ZBool.False;
			replyMessage.EM_MessageNum = "1";
			replyMessage.EM_MessageText = "test1";
			replyMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeCMD;
			AssertNull("Should not be included by sql filter", MessageForTest.Reply);
			replyMessage.EM_LinkTable = EDIMessageSchema.Constants.TableName;
			AssertNull("Should not be included by sql filter", MessageForTest.Reply);
			replyMessage.EM_LinkUniqueID = MessageForTest.PK;
			AssertNull("Should not be included by sql filter", MessageForTest.Reply);
			replyMessage.EM_IsActive = ZBool.True;
			AssertEquals("test1", MessageForTest.Reply.MessageText);
			var replyMessage2 = Factory.New<CMDEDIMessage>();
			replyMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeCMD;
			replyMessage2.EM_LinkTable = EDIMessageSchema.Constants.TableName;
			replyMessage2.EM_LinkUniqueID = MessageForTest.PK;
			replyMessage2.EM_IsActive = ZBool.True;
			replyMessage2.EM_MessageNum = "2";
			replyMessage2.EM_MessageText = "test2";
			MessageForTest.Reply = null;
			AssertEquals("test2", MessageForTest.Reply.MessageText);
			replyMessage.EM_MessageNum = "3";
			MessageForTest.Reply = null;
			AssertEquals("test1", MessageForTest.Reply.MessageText);
		}

		public void TestEM_MessageReplySummary()
		{
			AssertEquals("REPLY: None", MessageForTest.EM_ReplySummary);
			var replyMessage = Factory.New<CMDEDIMessage>();
			replyMessage.EM_IsActive = ZBool.True;
			replyMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeCMD;
			replyMessage.EM_LinkTable = EDIMessageSchema.Constants.TableName;
			replyMessage.EM_LinkUniqueID = MessageForTest.PK;
			replyMessage.EM_MessageText = FNAMessage;
			MessageForTest.Reply = null;
			AssertEquals("Sender: CIAS\r\nError: Y\r\n\r\nREPLY:\r\n" + FNAMessageWithoutRoutingInfo, MessageForTest.EM_ReplySummary);
			replyMessage.EM_MessageText = CMAMessage;
			MessageForTest.Reply = null;
			AssertEquals("Sender: SATS\r\nError: N\r\n\r\nREPLY:\r\n" + CMAMessageWithoutRoutingInfo, MessageForTest.EM_ReplySummary);
		}

		public void TestOnUpdatedByDataRefresh()
		{
			MessageForTest.EM_MessageText = "test";
			Factory.Save();
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			ForwardingShipment shipment = factory1.New<ForwardingShipment>();
			CMDShipmentWrapper shipmentWrapper1 = new CMDShipmentWrapper(shipment);
			CMDEDIMessageCollection collection1 = new CMDEDIMessageCollection(shipmentWrapper1);
			collection1.Add(factory1.Load(typeof(CMDEDIMessage), MessageForTest.PK));
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			ForwardingShipment shipment2 = factory2.New<ForwardingShipment>();
			CMDShipmentWrapper shipmentWrapper2 = new CMDShipmentWrapper(shipment2);
			CMDEDIMessageCollection collection2 = new CMDEDIMessageCollection(shipmentWrapper2);
			collection2.Add(factory2.Load(typeof(CMDEDIMessage), MessageForTest.PK));
			MessageForTest.EM_MessageText = "test2";
			Factory.Save();
			AssertEquals("test2", collection1[0].EM_MessageText);
			AssertEquals("test2", collection2[0].EM_MessageText);
		}

		#region Dummy
		DummyCMDEDIMessage MessageForTest => (DummyCMDEDIMessage)CachedBusinessObject;

		sealed class DummyCMDEDIMessage : CMDEDIMessage
		{
			public DummyCMDEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public INumberFountainProxy ExposedNumberFountain => NumberFountain;
		}

		#endregion
		#region FNAMessage
		const string FNAMessage = "FNA\r\n" + "CIAS\r\n" + "FNA\r\n" + "ACK/503 INVALID CRIA NO - SND\r\n" + "CMD/2\r\n" + "A/N/N\r\n" + "Test Content 1";
		const string FNAMessageWithoutRoutingInfo = "FNA\r\n" + "ACK/503 INVALID CRIA NO - SND\r\n" + "CMD/2\r\n" + "A/N/N\r\n" + "Test Content 1";
		#endregion
		#region CMAMessage
		const string CMAMessage = "CMA\r\n" + "SATS\r\n" + "CMA/2\r\n" + "A/N/N\r\n" + "Test Content 2";
		const string CMAMessageWithoutRoutingInfo = "CMA/2\r\n" + "A/N/N\r\n" + "Test Content 2";
		#endregion
	}
}
