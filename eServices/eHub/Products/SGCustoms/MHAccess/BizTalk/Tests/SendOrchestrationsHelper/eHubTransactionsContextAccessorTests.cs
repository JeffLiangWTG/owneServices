using System;
using System.Collections;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Xml;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.SGCustoms.MHAccess.Orchestrations.Helpers;
using Common.Logging.Simple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
	[TestClass]
	public class eHubTransactionsContextAccessorTests
	{
		const string filePath = "SendOrchestrationsHelper.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUpdateSubscriptionValueWithBatchNumberForAIRAED()
		{
			var mockDataModelAccessor = MockRepository.GenerateMock<DataModelAccessor>();
			eHubTransactionsContextAccessor.NewDataModelAccessor = () => mockDataModelAccessor;

			#region message
			var message = @"<ns0:EFACT_31_AIRAED xmlns:ns0=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
	<UNH>
		<UNH1>MAN0000091</UNH1>
		<UNH2>
			<UNH2.1>AIRAED</UNH2.1>
			<UNH2.2>3</UNH2.2>
			<UNH2.3>1</UNH2.3>
		</UNH2>
	</UNH>
	<ns0:IDT>
		<ns0:IDT1>
			<ns0:IDT1.1>201612334A</ns0:IDT1.1>
			<ns0:IDT1.2>20170823</ns0:IDT1.2>
			<ns0:IDT1.3>0001</ns0:IDT1.3>
		</ns0:IDT1>
		<ns0:IDT2>VWGT.VWGT001</ns0:IDT2>
	</ns0:IDT>
	<ns0:PUR>
		<ns0:PUR1>E</ns0:PUR1>
	</ns0:PUR>
	<ns0:DTM>
		<ns0:DTM1>20170823</ns0:DTM1>
		<ns0:DTM2>0001</ns0:DTM2>
	</ns0:DTM>
	<ns0:SG1Loop>
		<ns0:CST>
			<ns0:CST1>00001</ns0:CST1>
		</ns0:CST>
		<ns0:RFF>
			<ns0:RFF1>HAWB1</ns0:RFF1>
		</ns0:RFF>
		<ns0:SG2Loop>
			<ns0:REF>
				<ns0:REF1>012-12334123</ns0:REF1>
			</ns0:REF>
			<ns0:FLI>
				<ns0:FLI1>UPS100</ns0:FLI1>
			</ns0:FLI>
			<ns0:DTM_2>
				<ns0:DTM1>201708230101</ns0:DTM1>
			</ns0:DTM_2>
			<ns0:PAR>
				<ns0:PAR1>
					<ns0:PAR1.1>AALEE INDIA EXPORTS</ns0:PAR1.1>
					<ns0:PAR1.2>AALBORG INDUSTRIES PTE LTD</ns0:PAR1.2>
				</ns0:PAR1>
				<ns0:PAR2>uen123</ns0:PAR2>
			</ns0:PAR>
			<ns0:LOC>
				<ns0:LOC1>SGSIN</ns0:LOC1>
			</ns0:LOC>
			<ns0:EQN>
				<ns0:EQN1>10</ns0:EQN1>
				<ns0:EQN2>10</ns0:EQN2>
			</ns0:EQN>
			<ns0:MOA>
				<ns0:MOA1>1</ns0:MOA1>
			</ns0:MOA>
			<ns0:SG3Loop>
				<ns0:SER>
					<ns0:SER1>00001</ns0:SER1>
					<ns0:SER2>NT</ns0:SER2>
					<ns0:SER3>Games</ns0:SER3>
					<ns0:SER4/>
				</ns0:SER>
				<ns0:CTY>
					<ns0:CTY1>AU</ns0:CTY1>
					<ns0:CTY2>SG</ns0:CTY2>
				</ns0:CTY>
				<ns0:MEA>
					<ns0:MEA1>NO</ns0:MEA1>
					<ns0:MEA2>10</ns0:MEA2>
				</ns0:MEA>
				<ns0:MOA_2>
					<ns0:MOA1>12</ns0:MOA1>
				</ns0:MOA_2>
				<ns0:DOC>
					<ns0:DOC1/>
				</ns0:DOC>
			</ns0:SG3Loop>
		</ns0:SG2Loop>
	</ns0:SG1Loop>
</ns0:EFACT_31_AIRAED>";
			#endregion message

			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(message);

			eHubTransactionsContextAccessor.UpdateSubscriptionValueWithBatchNumberForAIRAED("HYEDAUAYA", "SGCustoms", "0001", xmlDocument);

			mockDataModelAccessor.AssertWasCalled(x => x.InsertSubscriptionValue("SGCMSG", "HYEDAUAYA", "SGCustoms", "201708230001", "MAN0000091", "BatchNo-JobNo"));
			mockDataModelAccessor.AssertWasCalled(x => x.InsertSubscriptionValue("SGCMSG", "HYEDAUAYA", "SGCustoms", "201708230001", "201612334A201708230001", "BatchNo-MsgUniqueReference"));
		}

	    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGetSubscriptiopnReferenceValueForAEP()
	    {
	        #region message
	        var message = @"<ns0:EFACT_31_AIRAEP xmlns:ns0=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
  <UNH>
    <UNH1>1</UNH1>
    <UNH2>
      <UNH2.1>AIRAEP</UNH2.1>
      <UNH2.2>3</UNH2.2>
      <UNH2.3>1</UNH2.3>
    </UNH2>
  </UNH>
  <ns0:IDT>
    <ns0:IDT1>
      <IDT1.1>198801949D</IDT1.1>
      <IDT1.2>20170707</IDT1.2>
      <IDT1.3>8106</IDT1.3>
    </ns0:IDT1>
    <IDT2>VWGT.VWGT001</IDT2>
  </ns0:IDT>
  <ns0:PUR>
    <PUR1>E</PUR1>
  </ns0:PUR>
  <ns0:DTM>
    <DTM1>20170707</DTM1>
    <DTM2>0001</DTM2>
  </ns0:DTM>
  <GIR>
    <GIR1>01</GIR1>
    <GIR2>20170707132757</GIR2>
  </GIR>
  <ns0:SG1Loop>
    <ns0:CST>
      <CST1>00005</CST1>
    </ns0:CST>
    <ns0:RFF>
      <RFF1>HAWB0707001</RFF1>
    </ns0:RFF>
    <ns0:REF>
      <REF1>MAWB0707003</REF1>
    </ns0:REF>
    <IND>
      <IND1>CR</IND1>
    </IND>
  </ns0:SG1Loop>
  <ns0:SG1Loop>
    <ns0:CST>
      <CST1>00006</CST1>
    </ns0:CST>
    <ns0:RFF>
      <RFF1>HAWB0707001</RFF1>
    </ns0:RFF>
    <ns0:REF>
      <REF1>MAWB0707003</REF1>
    </ns0:REF>
    <IND>
      <IND1>IP</IND1>
    </IND>
  </ns0:SG1Loop>
  <ns0:SG1Loop>
    <ns0:CST>
      <CST1>00007</CST1>
    </ns0:CST>
    <ns0:RFF>
      <RFF1>HAWB0707002</RFF1>
    </ns0:RFF>
    <ns0:REF>
      <REF1>MAWB0707003</REF1>
    </ns0:REF>
    <IND>
      <IND1>CR</IND1>
    </IND>
  </ns0:SG1Loop>
  <UNT>
    <UNT1>18</UNT1>
    <UNT2>1</UNT2>
  </UNT>
</ns0:EFACT_31_AIRAEP>";
	        #endregion message

	        var xmlDocument = new XmlDocument();
	        xmlDocument.LoadXml(message);
	        var IDTKey = string.Empty;

            var ecpextedResult = "198801949D201707078106E";
            var hasIDTKey = eHubTransactionsContextAccessor.TryGetSubscriptionReferenceValueForAEP(xmlDocument, out IDTKey);
            Assert.AreEqual(ecpextedResult, IDTKey);
            Assert.IsTrue(hasIDTKey);
	    }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestSucceedMessages()
		{
			var mHAccessMessageTrackingID = new Guid("B36040D1-0610-43E9-B764-A71498124199");
			var outboxMessagePK1 = new Guid("0bfd6d60-ba17-4206-a96d-5eada5f45744");
			var outboxMessagePK2 = new Guid("540e3d9b-d3fe-4653-a4ab-82875c5cb373");
			var messageTrackingID1 = "C2FA1C1C-39DC-4397-8318-EFD258A3819C";
			var messageTrackingID2 = "13F470ED-1F30-445C-AE0A-F154D61EEF47";
			var messageTrackingIDArrayList = new ArrayList();
			var outboxMessageContent1 = "message content 1";
			var outboxMessageContent2 = "message content 2";
			var senderPK = new Guid("1B29B863-A8A4-4AEB-9CDA-D9E9C2C035B8");
			var recipientPK = new Guid("5E1929AC-9FAB-4B8F-8686-B811CB15CBE4");
			var outboxMessageTypePK1 = new Guid("67749D07-1A8C-4C2A-BE1A-32333A6F6DA3");
			var outboxMessageTypePK2 = new Guid("1E4285DE-4019-4198-9C7C-F7F183B9EAA7");
			var outboxMessageTypeCode1 = "EFACT_31_AIRAED";
			var outboxMessageTypeCode2 = "EFACT_31_AIRAEU";

			messageTrackingIDArrayList.Add(messageTrackingID1);
			messageTrackingIDArrayList.Add(messageTrackingID2);

			var mockeHubTransactionsContext = MockRepository.GenerateMock<eHubTransactionsContext>();

			var eHubOutboxMessages = new TestDbSet<eHubOutboxMessage> {
				new eHubOutboxMessage { OI_PK = outboxMessagePK1, OI_MessageTrackingID = messageTrackingID1, OI_DT_Target = outboxMessageTypePK1, OI_Content = outboxMessageContent1 },
				new eHubOutboxMessage { OI_PK = outboxMessagePK2, OI_MessageTrackingID = messageTrackingID2, OI_DT_Target = outboxMessageTypePK2, OI_Content = outboxMessageContent2 }
				};
			var eHubInboxMessages = new TestDbSet<eHubInboxMessage>();

			var eHubClients = new TestDbSet<eHubClient> {
				new eHubClient {CC_PK = senderPK, CC_ID = "HYEDAUAYA"},
				new eHubClient {CC_PK = recipientPK, CC_ID = "SGCustomsTest"}
			};

			var eHubMessageTypes = new TestDbSet<eHubMessageType> {
				new eHubMessageType {DT_PK = outboxMessageTypePK1, DT_Code = outboxMessageTypeCode1},
				new eHubMessageType {DT_PK = outboxMessageTypePK2, DT_Code = outboxMessageTypeCode2},
			};

			mockeHubTransactionsContext.Expect(_ => _.BeginTransaction());
			mockeHubTransactionsContext.Stub(_ => _.eHubClients).Return(eHubClients).Repeat.Any();
			mockeHubTransactionsContext.Stub(_ => _.eHubMessageTypes).Return(eHubMessageTypes).Repeat.Any();
			mockeHubTransactionsContext.Stub(_ => _.eHubOutboxMessages).Return(eHubOutboxMessages).Repeat.Any();
			mockeHubTransactionsContext.Stub(_ => _.eHubInboxMessages).Return(eHubInboxMessages).Repeat.Any();
		    mockeHubTransactionsContext.Expect(_ => _.ExecuteSqlCommand(Arg.Is("exec [dbo].[InsertInbox] @InboxPK, @MessageTrackingID, @EnvelopeTrackingID, @SenderID, @RecipientID, @MessageType, @IsFlatFile, @EmailSubject, @FileName, @ApplicationCode, @Status, @CurrentDateTimeUTC, @AssignSN, @Content"), Arg<SqlParameter>.Is.Anything)).Return(0).Repeat.Once();
			mockeHubTransactionsContext.Expect(_ => _.ExecuteSqlCommand(Arg.Is("exec [dbo].[InsertInboxXMLContent] @InboxPK, @XMLContent, @MessageType, @UncompressedLength"), Arg<SqlParameter>.Is.Anything)).Return(0).Repeat.Once();
			mockeHubTransactionsContext.Expect(_ => _.ExecuteSqlCommand(Arg.Is("exec [dbo].[WriteInboxContent] @InboxPK, @Offset, @ContentChunk"), Arg<SqlParameter>.Is.Anything)).Return(0).Repeat.Once();
			mockeHubTransactionsContext.Expect(_ => _.ExecuteSqlCommand(Arg.Text.StartsWith("exec [dbo].[InsertOutboxMessage]"), Arg<SqlParameter>.Is.Anything)).Return(0).Repeat.Once().WhenCalled(call =>
			{
				SqlParameter parameter1 = (SqlParameter)(((object[])(call.Arguments[1]))[8]);
				Assert.AreEqual("Status", parameter1.ParameterName);
				Assert.AreEqual(MessageStatus.Processing, parameter1.Value);
			});
			mockeHubTransactionsContext.Expect(_ => _.ExecuteSqlCommand(Arg.Is("exec [dbo].[WriteOutboxContent] @OutboxPK, @Offset, @ContentChunk"), Arg<SqlParameter>.Is.Anything)).Return(0).Repeat.Once();
			mockeHubTransactionsContext.Expect(_ => _.ExecuteSqlCommand(Arg.Is("update eHubOutboxMessage set OI_BatchEnvelopeTrackingID = @batchEnvelopeTrackingID where OI_PK = @pk"), Arg<SqlParameter>.Is.Anything)).Return(0).Repeat.Once().WhenCalled(call =>
			{
				SqlParameter parameter1 = (SqlParameter)(((object[])(call.Arguments[1]))[0]);
				Assert.AreEqual("batchEnvelopeTrackingID", parameter1.ParameterName);
				Assert.AreEqual(mHAccessMessageTrackingID, parameter1.Value);

				SqlParameter parameter2 = (SqlParameter)(((object[])(call.Arguments[1]))[1]);
				Assert.AreEqual("pk", parameter2.ParameterName);
				Assert.AreEqual(outboxMessagePK1, parameter2.Value);
			}).Repeat.Once();
			mockeHubTransactionsContext.Expect(_ => _.ExecuteSqlCommand(Arg.Is("update eHubOutboxMessage set OI_BatchEnvelopeTrackingID = @batchEnvelopeTrackingID where OI_PK = @pk"), Arg<SqlParameter>.Is.Anything)).Return(0).Repeat.Once().WhenCalled(call =>
			{
				SqlParameter parameter1 = (SqlParameter)(((object[])(call.Arguments[1]))[0]);
				Assert.AreEqual("batchEnvelopeTrackingID", parameter1.ParameterName);
				Assert.AreEqual(mHAccessMessageTrackingID, parameter1.Value);

				SqlParameter parameter2 = (SqlParameter)(((object[])(call.Arguments[1]))[1]);
				Assert.AreEqual("pk", parameter2.ParameterName);
				Assert.AreEqual(outboxMessagePK2, parameter2.Value);
			}).Repeat.Once();
			mockeHubTransactionsContext.Expect(_ => _.ExecuteSqlCommand(Arg.Is("exec [dbo].[UpdateMessageDistributionStatus] @MessageTrackingID, @SenderID, @RecipientID, @CurrentDateTimeUTC"), Arg<SqlParameter>.Is.Anything)).Return(0).Repeat.Once();
			mockeHubTransactionsContext.Expect(_ => _.SaveChanges()).Return(0);
			((IDisposable)mockeHubTransactionsContext).Expect(x => x.Dispose());

			eHubTransactionsContextAccessor.NewEHubTransactionContext = () => mockeHubTransactionsContext;

			using (var inputStream = GetEmbeddedResource(filePath + "Test1_input.xml"))
			using (var outputStream = GetEmbeddedResource(filePath + "Test1_input.xml"))
			{
				eHubTransactionsContextAccessor.SucceedMessages("HYEDAUAYA", "SGCustomsTest", mHAccessMessageTrackingID, inputStream.ReadToEnd(), outputStream.ReadToEnd(), messageTrackingIDArrayList);
			}

			mockeHubTransactionsContext.VerifyAllExpectations();

			var messages = new ArrayList();
			foreach (var message in eHubInboxMessages)
			{
				messages.Add(message);
			}

			Assert.AreEqual(messages.Count, 2);
			Assert.AreEqual(((eHubInboxMessage)messages[0]).EI_Content, outboxMessageContent1);
			Assert.AreEqual(((eHubInboxMessage)messages[1]).EI_Content, outboxMessageContent2);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFailMessages()
		{
			var outboxMessagePK1 = new Guid("0bfd6d60-ba17-4206-a96d-5eada5f45744");
			var outboxMessagePK2 = new Guid("540e3d9b-d3fe-4653-a4ab-82875c5cb373");
			var messageTrackingID1 = "C2FA1C1C-39DC-4397-8318-EFD258A3819C";
			var messageTrackingID2 = "13F470ED-1F30-445C-AE0A-F154D61EEF47";
			var outboxMessageTypePK1 = new Guid("67749D07-1A8C-4C2A-BE1A-32333A6F6DA3");
			var outboxMessageTypePK2 = new Guid("1E4285DE-4019-4198-9C7C-F7F183B9EAA7");
			var outboxMessageTypeCode1 = "EFACT_31_AIRAED";
			var outboxMessageTypeCode2 = "EFACT_31_AIRAEU";
			var outboxMessageContent1 = "content 1";
			var outboxMessageContent2 = "content 2";
			var senderPK1 = new Guid("1B29B863-A8A4-4AEB-9CDA-D9E9C2C035B8");
            var senderPK2 = new Guid("863F0B68-B966-4060-8242-E58F930683A3");
			var recipientPK = new Guid("5E1929AC-9FAB-4B8F-8686-B811CB15CBE4");

			var messageTrackingIDArrayList = new ArrayList();
			messageTrackingIDArrayList.Add(messageTrackingID1);
			messageTrackingIDArrayList.Add(messageTrackingID2);

			var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
			eHubTransactionsContextAccessor.NewEHubTransactionContext = () => mockeHubTransactionsContext;

			var eHubOutboxMessages = new TestDbSet<eHubOutboxMessage> {
				new eHubOutboxMessage { OI_PK = outboxMessagePK1, OI_MessageTrackingID = messageTrackingID1, OI_DT_Target = outboxMessageTypePK1, OI_Content = outboxMessageContent1, OI_CC_Sender = senderPK1 },
				new eHubOutboxMessage { OI_PK = outboxMessagePK2, OI_MessageTrackingID = messageTrackingID2, OI_DT_Target = outboxMessageTypePK2, OI_Content = outboxMessageContent2, OI_CC_Sender = senderPK2 }
			};

			var eHubClients = new TestDbSet<eHubClient> {
				new eHubClient {CC_PK = senderPK1, CC_ID = "HYEDAUAYA"},
                new eHubClient {CC_PK = senderPK2, CC_ID = "HYEDAUIVS"},
				new eHubClient {CC_PK = recipientPK, CC_ID = "SGCustomsTest"}
			};

			var eHubMessageTypes = new TestDbSet<eHubMessageType> {
				new eHubMessageType {DT_PK = outboxMessageTypePK1, DT_Code = outboxMessageTypeCode1},
				new eHubMessageType {DT_PK = outboxMessageTypePK2, DT_Code = outboxMessageTypeCode2},
			};

			var eHubInboxMessages = new TestDbSet<eHubInboxMessage>();

			mockeHubTransactionsContext.Expect(_ => _.BeginTransaction());
			mockeHubTransactionsContext.Stub(_ => _.eHubOutboxMessages).Return(eHubOutboxMessages).Repeat.Any();
			mockeHubTransactionsContext.Stub(_ => _.eHubClients).Return(eHubClients).Repeat.Any();
			mockeHubTransactionsContext.Stub(_ => _.eHubMessageTypes).Return(eHubMessageTypes).Repeat.Any();
			mockeHubTransactionsContext.Stub(_ => _.eHubInboxMessages).Return(eHubInboxMessages).Repeat.Any();
			mockeHubTransactionsContext.Expect(_ => _.ExecuteSqlCommand("exec [dbo].[InsertError] @ErrorPK, @Source, @ErrorType, @Description, @ErrorDetail, @InboxPK, @OutboxPK, @CurrentDateTimeUTC, @InboxMessageTrackingID, @OutboxMessageTrackingID, @Alerted", Arg<SqlParameter>.Is.Anything, Arg<SqlParameter>.Is.Anything)).Return(0);
			mockeHubTransactionsContext.Expect(_ => _.SaveChanges()).Return(0);
			((IDisposable)mockeHubTransactionsContext).Expect(x => x.Dispose());

			var failReason = "Account 'AccountID1' is invalid";
			eHubTransactionsContextAccessor.FailMessages("HYEDAUAYA", "SGCustomsTest", messageTrackingIDArrayList, failReason);

			mockeHubTransactionsContext.VerifyAllExpectations();
			Assert.AreEqual(2, eHubInboxMessages.Local.Count);

			AssertEHubInboxMessageCreatedByFailingMessage(outboxMessageTypeCode1, messageTrackingID1, outboxMessageContent1, senderPK1, recipientPK, eHubInboxMessages.Local[0], failReason);
			AssertEHubInboxMessageCreatedByFailingMessage(outboxMessageTypeCode2, messageTrackingID2, outboxMessageContent2, senderPK2, recipientPK, eHubInboxMessages.Local[1], failReason);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFailMessages_MultipleExceptions()
		{
			var outboxMessagePK1 = new Guid("0bfd6d60-ba17-4206-a96d-5eada5f45744");
			var outboxMessagePK2 = new Guid("540e3d9b-d3fe-4653-a4ab-82875c5cb373");
			var messageTrackingID1 = "C2FA1C1C-39DC-4397-8318-EFD258A3819C";
			var messageTrackingID2 = "13F470ED-1F30-445C-AE0A-F154D61EEF47";
			var outboxMessageTypePK1 = new Guid("67749D07-1A8C-4C2A-BE1A-32333A6F6DA3");
			var outboxMessageTypePK2 = new Guid("1E4285DE-4019-4198-9C7C-F7F183B9EAA7");
			var outboxMessageTypeCode1 = "EFACT_31_AIRAED";
			var outboxMessageTypeCode2 = "EFACT_31_AIRAEU";
			var outboxMessageContent1 = "content 1";
			var outboxMessageContent2 = "content 2";
			var senderPK1 = new Guid("1B29B863-A8A4-4AEB-9CDA-D9E9C2C035B8");
			var senderPK2 = new Guid("863F0B68-B966-4060-8242-E58F930683A3");
			var recipientPK = new Guid("5E1929AC-9FAB-4B8F-8686-B811CB15CBE4");

			var messageTrackingIDArrayList = new ArrayList();
			messageTrackingIDArrayList.Add(messageTrackingID1);
			messageTrackingIDArrayList.Add(messageTrackingID2);

			var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
			eHubTransactionsContextAccessor.NewEHubTransactionContext = () => mockeHubTransactionsContext;

			var eHubOutboxMessages = new TestDbSet<eHubOutboxMessage> {
				new eHubOutboxMessage { OI_PK = outboxMessagePK1, OI_MessageTrackingID = messageTrackingID1, OI_DT_Target = outboxMessageTypePK1, OI_Content = outboxMessageContent1, OI_CC_Sender = senderPK1 },
				new eHubOutboxMessage { OI_PK = outboxMessagePK2, OI_MessageTrackingID = messageTrackingID2, OI_DT_Target = outboxMessageTypePK2, OI_Content = outboxMessageContent2, OI_CC_Sender = senderPK2 }
			};

			var eHubClients = new TestDbSet<eHubClient> {
				new eHubClient {CC_PK = senderPK1, CC_ID = "HYEDAUAYA"},
				new eHubClient {CC_PK = senderPK2, CC_ID = "HYEDAUIVS"},
				new eHubClient {CC_PK = recipientPK, CC_ID = "SGCustomsTest"}
			};

			var eHubMessageTypes = new TestDbSet<eHubMessageType> {
				new eHubMessageType {DT_PK = outboxMessageTypePK1, DT_Code = outboxMessageTypeCode1},
				new eHubMessageType {DT_PK = outboxMessageTypePK2, DT_Code = outboxMessageTypeCode2},
			};

			var eHubInboxMessages = new TestDbSet<eHubInboxMessage>();

			mockeHubTransactionsContext.Expect(_ => _.BeginTransaction());
			mockeHubTransactionsContext.Stub(_ => _.eHubOutboxMessages).Return(eHubOutboxMessages).Repeat.Any();
			mockeHubTransactionsContext.Stub(_ => _.eHubClients).Return(eHubClients).Repeat.Any();
			mockeHubTransactionsContext.Stub(_ => _.eHubMessageTypes).Return(eHubMessageTypes).Repeat.Any();
			mockeHubTransactionsContext.Stub(_ => _.eHubInboxMessages).Return(eHubInboxMessages).Repeat.Any();
			mockeHubTransactionsContext.Expect(_ => _.ExecuteSqlCommand("exec [dbo].[InsertError] @ErrorPK, @Source, @ErrorType, @Description, @ErrorDetail, @InboxPK, @OutboxPK, @CurrentDateTimeUTC, @InboxMessageTrackingID, @OutboxMessageTrackingID, @Alerted", Arg<SqlParameter>.Is.Anything, Arg<SqlParameter>.Is.Anything)).Return(0);
			mockeHubTransactionsContext.Expect(_ => _.SaveChanges()).Return(0);
			((IDisposable)mockeHubTransactionsContext).Expect(x => x.Dispose());

			var exceptions = new ArrayList();
			exceptions.Add(new Exception("Message1 is invalid"));
			exceptions.Add(new Exception("Message2 is invalid"));
			eHubTransactionsContextAccessor.FailMessages("HYEDAUAYA", "SGCustomsTest", messageTrackingIDArrayList, exceptions);

			mockeHubTransactionsContext.VerifyAllExpectations();
			Assert.AreEqual(2, eHubInboxMessages.Local.Count);

			AssertEHubInboxMessageCreatedByFailingMessage(outboxMessageTypeCode1, messageTrackingID1, outboxMessageContent1, senderPK1, recipientPK, eHubInboxMessages.Local[0], "Message1 is invalid");
			AssertEHubInboxMessageCreatedByFailingMessage(outboxMessageTypeCode2, messageTrackingID2, outboxMessageContent2, senderPK2, recipientPK, eHubInboxMessages.Local[1], "Message2 is invalid");
		}

		void AssertEHubInboxMessageCreatedByFailingMessage(string messageType, string outboxMessageTrackingId, string messageContent, Guid recipientPK, Guid senderPK, eHubInboxMessage inboxMessage, string expectedFileNameOverride)
		{
			Assert.AreEqual(senderPK, inboxMessage.EI_CC_Sender);
			Assert.AreEqual(recipientPK, inboxMessage.EI_CC_Recipient);
			Assert.AreEqual(messageType, inboxMessage.EI_MessageType);
			Assert.AreEqual(false, inboxMessage.EI_IsFlatFile);
			Assert.AreEqual(string.Empty, inboxMessage.EI_EmailSubjectOverride);
			Assert.AreEqual($"ErrorCode: 99999999;ErrorMessage: {expectedFileNameOverride};LinkedOutboxMessageTrackingID: {outboxMessageTrackingId}.", inboxMessage.EI_FileNameOverride);
			Assert.AreEqual("SGC", inboxMessage.EI_ApplicationCode);
			Assert.AreEqual(0, inboxMessage.EI_Status);
			Assert.AreEqual(messageContent, inboxMessage.EI_Content);
			Assert.IsNull(inboxMessage.EI_SN);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFormatError()
		{
			var expected = "ErrorCode: 1325;ErrorMessage: Invalid User ID / Password;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB.";
			var actual = eHubTransactionsContextAccessor.FormatError("Status: fail; State: login; ErrorCode: 1325; ErrorMessage: Invalid User ID / Password", "3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB");
			Assert.AreEqual(expected, actual);

			expected = "ErrorCode: 99999999;ErrorMessage: Account 'AccountID1' is invalid;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB.";
			actual = eHubTransactionsContextAccessor.FormatError("Account 'AccountID1' is invalid", "3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB");
			Assert.AreEqual(expected, actual);

			expected = "ErrorCode: 99999999;ErrorMessage: CargoWise.eHub.Core.Orchestrations.Helper.FatalMessageProcessingException: CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Common.Orchestrations.MHAccessGateway - SG Customs MHAccess account 'VWGT002' is invalid at Carg";
			var longError = @"CargoWise.eHub.Core.Orchestrations.Helper.FatalMessageProcessingException: CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Common.Orchestrations.MHAccessGateway - SG Customs MHAccess account 'VWGT002' is invalid at CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Orchestrations.eHub2MHAccess
							blahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblah";
			actual = eHubTransactionsContextAccessor.FormatError(longError, "3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB");
			Assert.AreEqual(expected, actual);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUpdateOutboxMessageBatchEnvelopeTrackingID()
		{
			var batchEnvelopeTrackingID = "B36040D1-0610-43E9-B764-A71498124199";
			var outboxMessagePK1 = new Guid("0bfd6d60-ba17-4206-a96d-5eada5f45744");
			var outboxMessagePK2 = new Guid("540e3d9b-d3fe-4653-a4ab-82875c5cb373");
			var messageTrackingID1 = "C2FA1C1C-39DC-4397-8318-EFD258A3819C";
			var messageTrackingID2 = "13F470ED-1F30-445C-AE0A-F154D61EEF47";
			var messageTrackingIDArrayList = new ArrayList();
			messageTrackingIDArrayList.Add(messageTrackingID1);
			messageTrackingIDArrayList.Add(messageTrackingID2);

			var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
			eHubTransactionsContextAccessor.NewEHubTransactionContext = () => mockeHubTransactionsContext;

			var eHubOutboxMessages = new TestDbSet<eHubOutboxMessage> {
				new eHubOutboxMessage { OI_PK = outboxMessagePK1, OI_MessageTrackingID = messageTrackingID1 },
				new eHubOutboxMessage { OI_PK = outboxMessagePK2, OI_MessageTrackingID = messageTrackingID2 }
			};

			mockeHubTransactionsContext.Expect(_ => _.BeginTransaction());
			mockeHubTransactionsContext.Stub(_ => _.eHubOutboxMessages).Return(eHubOutboxMessages).Repeat.Any();
			mockeHubTransactionsContext.Expect(_ => _.ExecuteSqlCommand("update eHubOutboxMessage set OI_BatchEnvelopeTrackingID = @batchEnvelopeTrackingID where OI_PK = @pk", Arg<SqlParameter>.Is.Anything, Arg<SqlParameter>.Is.Anything)).Return(0).Repeat.Once().WhenCalled(call =>
			{
				SqlParameter parameter1 = (SqlParameter)(((object[])(call.Arguments[1]))[0]);
				Assert.AreEqual("batchEnvelopeTrackingID", parameter1.ParameterName);
				Assert.AreEqual(batchEnvelopeTrackingID, parameter1.Value);

				SqlParameter parameter2 = (SqlParameter)(((object[])(call.Arguments[1]))[1]);
				Assert.AreEqual("pk", parameter2.ParameterName);
				Assert.AreEqual(outboxMessagePK1, parameter2.Value);
			});
			mockeHubTransactionsContext.Expect(_ => _.ExecuteSqlCommand("update eHubOutboxMessage set OI_BatchEnvelopeTrackingID = @batchEnvelopeTrackingID where OI_PK = @pk", Arg<SqlParameter>.Is.Anything, Arg<SqlParameter>.Is.Anything)).Return(0).Repeat.Once().WhenCalled(call =>
			{
				SqlParameter parameter1 = (SqlParameter)(((object[])(call.Arguments[1]))[0]);
				Assert.AreEqual("batchEnvelopeTrackingID", parameter1.ParameterName);
				Assert.AreEqual(batchEnvelopeTrackingID, parameter1.Value);

				SqlParameter parameter2 = (SqlParameter)(((object[])(call.Arguments[1]))[1]);
				Assert.AreEqual("pk", parameter2.ParameterName);
				Assert.AreEqual(outboxMessagePK2, parameter2.Value);
			});
			((IDisposable)mockeHubTransactionsContext).Expect(x => x.Dispose());

			eHubTransactionsContextAccessor.UpdateOutboxMessageBatchEnvelopeTrackingID(batchEnvelopeTrackingID, messageTrackingIDArrayList);

			mockeHubTransactionsContext.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetAccountFlag()
		{
			var registrationPK = new Guid("D4A90731-9866-433A-B78C-E8B94FBBA85F");
			var registrationTypePK = new Guid("F632D41A-D1ED-4480-A711-16EE24E4098D");
			var clientPK = new Guid("1BCB4F38-9696-45B1-A884-2487340B9166");

			var eHubClientRegistrations = new TestDbSet<eHubClientRegistration> {
				new eHubClientRegistration { CX_PK = registrationPK, CX_RT = registrationTypePK, CX_CC = clientPK, CX_Flag1 = 1, CX_Code = "AccountID", CX_Password1 = "Password" }
			};
			var eHubRegistrationTypes = new TestDbSet<eHubRegistrationType> {
				new eHubRegistrationType { RT_PK = registrationTypePK, RT_ID = "SGCustomsAccount" }
			};
			var eHubClients = new TestDbSet<eHubClient> {
				new eHubClient { CC_PK = clientPK, CC_ID = "HYEDAUAYA" }
			};

			var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
			eHubTransactionsContextAccessor.NewEHubTransactionContext = () => mockeHubTransactionsContext;
			mockeHubTransactionsContext.Stub(_ => _.eHubClientRegistrations).Return(eHubClientRegistrations).Repeat.Any();
			mockeHubTransactionsContext.Stub(_ => _.eHubRegistrationTypes).Return(eHubRegistrationTypes).Repeat.Any();
			mockeHubTransactionsContext.Stub(_ => _.eHubClients).Return(eHubClients).Repeat.Any();
			((IDisposable)mockeHubTransactionsContext).Expect(x => x.Dispose());

			var flag = eHubTransactionsContextAccessor.GetAccountFlag("HYEDAUAYA", "AccountID");
			Assert.AreEqual(1, flag);

			mockeHubTransactionsContext.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetAccountPassword()
		{
			var registrationPK = new Guid("D4A90731-9866-433A-B78C-E8B94FBBA85F");
			var registrationTypePK = new Guid("F632D41A-D1ED-4480-A711-16EE24E4098D");
			var clientPK = new Guid("1BCB4F38-9696-45B1-A884-2487340B9166");

			var eHubClientRegistrations = new TestDbSet<eHubClientRegistration> {
				new eHubClientRegistration { CX_PK = registrationPK, CX_RT = registrationTypePK, CX_CC = clientPK, CX_Flag1 = 1, CX_Code = "AccountID", CX_Password1 = "Password" }
			};
			var eHubRegistrationTypes = new TestDbSet<eHubRegistrationType> {
				new eHubRegistrationType { RT_PK = registrationTypePK, RT_ID = "SGCustomsAccount" }
			};
            var eHubClients = new TestDbSet<eHubClient> {
				new eHubClient { CC_PK = clientPK, CC_ID = "HYEDAUAYA" }
            };

			var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
			eHubTransactionsContextAccessor.NewEHubTransactionContext = () => mockeHubTransactionsContext;
			mockeHubTransactionsContext.Stub(_ => _.eHubClientRegistrations).Return(eHubClientRegistrations).Repeat.Any();
			mockeHubTransactionsContext.Stub(_ => _.eHubRegistrationTypes).Return(eHubRegistrationTypes).Repeat.Any();
			mockeHubTransactionsContext.Stub(_ => _.eHubClients).Return(eHubClients).Repeat.Any();
			((IDisposable)mockeHubTransactionsContext).Expect(x => x.Dispose());

			var password = eHubTransactionsContextAccessor.GetAccountPassword("HYEDAUAYA", "AccountID", new NoOpLogger());
			Assert.AreEqual("Password", password);

			mockeHubTransactionsContext.VerifyAllExpectations();
		}

		Stream GetEmbeddedResource(string resourceName)
		{
			string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
			var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
			if (resource == null)
			{
				throw new Exception(String.Format("Could not locate embedded resource '{0}'", fullResourceName));
			}
			return resource;
		}
	}
}
