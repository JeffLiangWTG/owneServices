using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	class CMDMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessInboundXDCMessage()
		{
			const string XDCCMAMessage = @"<CMD>FNA
ACK/RECIPIENT NOT CONFIGURED TO RECEIVE THIS MESSAGE TYPE/VERSION
CMD/2
A/N/Y
MWB/081-12345675SYDSIN/T11K1001
/AS PER MANIFEST
FLT/QF410/09MAR
HWB/46546351/6/K501
/JOUMAAAAAA/76031000
SHP/YOUR AUSTRALIA COMPANY
/105 O RIORDAN STREET
CNE/CHIP FORWARDING
/1 STREET
TPT/TDB/IM0C108288R
DUI/N
SND/CargoWise Support/YOUR SINGAPORE CORP
/198801949D
/234234/23432
</CMD>
";
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "088-99898989";
			consol.JK_MasterBillNum = "08112345675";
			consol.JK_MasterBillIssueDate = ZDateTime.Now;
			var outMessage = CreateTransmitMessage("088-99898989", shipment.PK, EDIMessage.ApplicationCodes.SingaporeCMD, CMDGenerator.Constants.GHA, isActive: true);
			var responseMessage = CreateNewInboundMessage(XDCCMAMessage);
			Factory.Save();
			var processor = new BatchCMDMessageProcessor(new LoggingInformation());
			processor.ExecuteBatch();
			Factory.Save();
			AssertMessageStatus(responseMessage, outMessage, EDIMessage.Status.Received, EDIMessage.Status.Received, CMDInbound.Constants.FNA);
		}

		public void TestLicenceLog()
		{
			var outMessage = CreateBizOsForMessage7();
			var cmaMessage = CreateNewInboundMessage(CMAMessage7);
			Factory.Save();
			var logQuery = new ZQuery(StmActivityLogSchema.S7_ControllerID, LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString());
			logQuery.AddToFilter(StmActivityLogSchema.S7_MouseClicks, (int)Env.Licence.CMDReporting.LicenceType);
			var logs = Factory.Load<StmActivityLog>(logQuery);
			AssertEquals("Precondition - No existing log entry", 0, logs.Length);
			var processor = new BatchCMDMessageProcessor(new LoggingInformation());
			processor.ExecuteBatch();
			Factory.Save();
			AssertMessageStatus(cmaMessage, outMessage, EDIMessage.Status.Received, EDIMessage.Status.Received, CMDInbound.Constants.CMA);
			logs = Factory.Load<StmActivityLog>(logQuery);
			AssertEquals(1, logs.Length);
		}

		public void TestUserIsNotifiedWhenMessageIsProcessed()
		{
			SetNotificationGroupEmails();
			CreateNewInboundMessage(FNAMessage1);
			CreateBizOsForMessage1();
			Factory.Save();
			SetEventsTime();
			Factory.Save();
			var processor = new BatchCMDMessageProcessor(new LoggingInformation());
			processor.ExecuteBatch();
			Factory.Save();
			string expectedEmailSubject = "FNA (ERROR) Message Received for MasterBill: '088-12345101'";
			string expectedEmailBody = $"Message Sent: {new ZDateTime(2001, 1, 1)}\n" + "CMDFNA\r\n" + "CCN\r\n" + "FNA\r\n" + "ACK/503 INVALID CRIA NO - SND\r\n" + "CMD/2\r\n" + "A/N/N\r\n" + "MWB/088-12345101JKTJTY/T77K234.99\r\n" + "/AS PER MANIFEST\r\n" + "FLT/QF8898/01JAN\r\n" + "HWB/SERIAL123/89/L56.78\r\n" + "/LUXURY CAR A/HAR001\r\n" + "/ELEPHANT WITH TRUNKS/HAR005\r\n" + "/BIG ELEPHANTS/HAR110\r\n" + "/SMALL ELEPHANTS/HAR111\r\n" + "SHP/FREIGHT SENDER 101\r\n" + "/111 BOURKE ROAD ALEXANDRIA NSW 2000 AUSTRALIA CNE/FREIGHT RECEIVER 102\r\n" + "/101 BLABLABLA STREET YUMMMMM CAIRO XXY131Z EGYPT\r\n" + "TPT/TDB/TDB0000101\r\n" + "/TDB0000102\r\n" + "/TDB0000103\r\n" + "/TDB0000104\r\n" + "CED/Y/CED0000101/CED0000102/CED0000103\r\n" + "IMW/111-12345678\r\n" + "EXP/TS/BLABLABLA\r\n" + "SND/TEST USER/TEST COMPANY 123\r\n" + "/TEST/TEST/TEST\r\n";
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertMailItem(email, expectedEmailSubject, expectedEmailBody, "test@test.edi.com.au", null);
		}

		public void TestProcessor()
		{
			CreateBizOsForMessage2();
			CreateBizOsForMessage3();
			Factory.Save();
			foreach ((CMDEDIMessage inboundMessage, CMDEDIMessage outgoingMessage, ZString expectedMessageType, ZString expectedStatus, ZString expectedXDCStatus) in SetupInboundMessagesForTest())
			{
				Factory.Save();
				var processor = new BatchCMDMessageProcessor(new LoggingInformation());
				processor.ExecuteBatch();
				Factory.Save();
				AssertMessageStatus(inboundMessage, outgoingMessage, expectedXDCStatus, expectedStatus, expectedMessageType);
			}

			AssertEquals("Total Generated Message", 8, GetTotalIncomingMessages(false).Length);
		}

		void AssertMessageStatus(CMDEDIMessage message, CMDEDIMessage originalMessage, string originalMessageStatus, string generatedMessageStatus, string generatedMessageType)
		{
			message.Reload();
			AssertEquals("originalMessage EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals("originalMessage EM_Status", originalMessageStatus, message.EM_Status);
			var interchange = message.Interchange;
			var generatedMessages = interchange.ContainedMessages.Where(x => x.PK != message.PK);
			AssertEquals("generatedMessages.Count()", 1, generatedMessages.Count());
			var generatedMessage = generatedMessages.First();
			AssertEquals("generatedMessage EM_Status", generatedMessageStatus, generatedMessage.EM_Status);
			AssertEquals("generatedMessage EM_MessageType", generatedMessageType, generatedMessage.EM_MessageType);
			AssertEquals("generatedMessage EM_MessageNum", message.EM_MessageNum, generatedMessage.EM_MessageNum);
			if (originalMessage != null)
			{
				AssertEquals("generatedMessage EM_LinkUniqueID", originalMessage.PK, generatedMessage.EM_LinkUniqueID);
				AssertEquals("generatedMessage EM_LinkTable", EDIMessageSchema.Constants.TableName, generatedMessage.EM_LinkTable);
				AssertEquals("generatedMessage EM_GB", originalMessage.EM_GB, generatedMessage.EM_GB);
			}
			else
			{
				AssertEquals("unlinked generatedMessage EM_LinkUniqueID", ZGuid.Empty, generatedMessage.EM_LinkUniqueID);
				AssertEquals("unlinked generatedMessage EM_LinkTable", ZString.Empty, generatedMessage.EM_LinkTable);
			}
		}

		void AssertMailItem(EmailDef email, string subject, string body, string to, string cc)
		{
			var emailTo = (email.Recipients.Count > 0) ? email.Recipients[0].Email : null;
			var emailCC = (email.CCRecipients.Count > 0) ? email.CCRecipients[0].Email : null;
			AssertEquals(subject, email.Subject);
			AssertEquals(body, email.Body);
			AssertEquals(to, emailTo);
			AssertEquals(cc, emailCC);
		}

		void SetNotificationGroupEmails()
		{
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_EmailAddress = "test@test.edi.com.au";
			var postMaster = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, User.PostMasterUserName);
			postMaster.GS_EmailAddress = "postnotify@test.edi.com.au";
			Factory.Save();
			var util = new EmailGroupUtility();
			var emails = util.GetCompanyNotificationGroupEmails();
			AssertEquals("Should now be set to postnotify@test.edi.com.au", "postnotify@test.edi.com.au", emails[0]);
		}

		IEnumerable<(CMDEDIMessage inboundMessage, CMDEDIMessage outgoingMessage, ZString expectedMessageType, ZString expectedStatus, ZString expectedXDCStatus)> SetupInboundMessagesForTest()
		{
			yield return (CreateNewInboundMessage(FNAMessage1), CreateBizOsForMessage1(), CMDInbound.Constants.FNA, EDIMessage.Status.Received, EDIMessage.Status.Received);
			yield return (CreateNewInboundMessage(CMAMessage2), null, CMDInbound.Constants.CMA, EDIMessage.Status.Failed, EDIMessage.Status.Received);
			yield return (CreateNewInboundMessage(FMAMessage3), null, CMDInbound.Constants.FMA, EDIMessage.Status.Failed, EDIMessage.Status.Received);
			yield return (CreateNewInboundMessage(FNAMessage4), CreateBizOsForMessage4(), CMDInbound.Constants.FNA, EDIMessage.Status.Received, EDIMessage.Status.Received);
			yield return (CreateNewInboundMessage(FNAMessage5), null, CMDInbound.Constants.FNA, EDIMessage.Status.Failed, EDIMessage.Status.Received);
			yield return (CreateNewInboundMessage(FNAMessage6), CreateBizOsForMessage6(), CMDInbound.Constants.FNA, EDIMessage.Status.Received, EDIMessage.Status.Received);
			yield return (CreateNewInboundMessage(CMAMessage7), CreateBizOsForMessage7(), CMDInbound.Constants.CMA, EDIMessage.Status.Received, EDIMessage.Status.Received);
			yield return (CreateNewInboundMessage(OtherMessage), null, CMDInbound.Constants.UNK, EDIMessage.Status.Failed, EDIMessage.Status.Failed);
		}

		CMDEDIMessage CreateNewInboundMessage(string body)
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.SingaporeCMD;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Received;
			interchange.EI_From = "CCN";
			interchange.EI_BodyText = body;
			var inboundMessage = Factory.New<CMDEDIMessage>();
			inboundMessage.EM_EI = interchange.PK;
			inboundMessage.EM_GB = interchange.EI_GB;
			inboundMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = body;
			return inboundMessage;
		}

		CMDEDIMessage CreateBizOsForMessage1()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_MasterBillNum = "08812345101";
			consol.JK_UniqueConsignRef = "08812345101";
			var validMessage = CreateTransmitMessage("08812345101", shipment.PK, CMDGenerator.Constants.GHA);
			CreateTransmitMessage("08812345101", shipment.PK, CMDGenerator.Constants.GHA, false);
			CreateTransmitMessage("08812345101", shipment.PK, CMDGenerator.Constants.TDB);
			return validMessage;
		}

		CMDEDIMessage CreateBizOsForMessage2()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			var consol = shipment1.Consols.AddNew();
			consol.JK_UniqueConsignRef = "088-11111111";
			consol.JK_MasterBillNum = "088-11111111";
			shipment2.Consols.Add(consol);
			var validMessage = CreateTransmitMessage("088-11111111", shipment1.PK, CMDGenerator.Constants.GHA);
			CreateTransmitMessage("088-11111111", shipment1.PK, CMDGenerator.Constants.GHA, false);
			CreateTransmitMessage("088-11111111", shipment1.PK, CMDGenerator.Constants.TDB);
			CreateTransmitMessage("088-11111111", shipment2.PK, CMDGenerator.Constants.GHA);
			CreateTransmitMessage("088-11111111", shipment2.PK, CMDGenerator.Constants.TDB);
			return validMessage;
		}

		CMDEDIMessage CreateBizOsForMessage3()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "088AAAAAAAA";
			consol.JK_MasterBillNum = "088AAAAAAAA";
			var validMessage = CreateTransmitMessage("088AAAAAAAA", shipment.PK, CMDGenerator.Constants.GHA);
			CreateNonCMDTransmitMessage("088AAAAAAAA", shipment.PK, CMDGenerator.Constants.GHA);
			return validMessage;
		}

		CMDEDIMessage CreateBizOsForMessage4()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_HouseBill = "hb101";
			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_HouseBill = "hb102";
			var shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_HouseBill = "SERIAL3456";
			var shipment4 = Factory.New<ForwardingShipment>();
			shipment4.JS_HouseBill = "SERIAL345";
			var consol = shipment1.Consols.AddNew();
			consol.JK_UniqueConsignRef = "088-22222222";
			consol.JK_MasterBillNum = "088-22222222";
			shipment2.Consols.Add(consol);
			shipment3.Consols.Add(consol);
			shipment4.Consols.Add(consol);
			CreateTransmitMessage("088-22222222", shipment1.PK, CMDGenerator.Constants.GHA);
			CreateNonCMDTransmitMessage("088-22222222", shipment1.PK, CMDGenerator.Constants.GHA);
			CreateTransmitMessage("088-22222222", shipment1.PK, CMDGenerator.Constants.TDB);
			CreateTransmitMessage("088-22222222", shipment2.PK, CMDGenerator.Constants.GHA);
			CreateTransmitMessage("088-22222222", shipment3.PK, CMDGenerator.Constants.GHA);
			CreateTransmitMessage("088-22222222", shipment3.PK, CMDGenerator.Constants.TDB);
			var validMessage = CreateTransmitMessage("088-22222222", shipment4.PK, CMDGenerator.Constants.GHA);
			CreateNonCMDTransmitMessage("088-22222222", shipment4.PK, CMDGenerator.Constants.GHA);
			CreateTransmitMessage("088-22222222", shipment4.PK, CMDGenerator.Constants.TDB);
			return validMessage;
		}

		CMDEDIMessage CreateBizOsForMessage6()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol1 = shipment.Consols.AddNew();
			consol1.JK_UniqueConsignRef = "088-44444444";
			consol1.JK_MasterBillNum = "088-44444444";
			var consol2 = shipment.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "088-55555555";
			consol2.JK_MasterBillNum = "088-55555555";
			var consol3 = shipment.Consols.AddNew();
			consol3.JK_UniqueConsignRef = "088-66666666";
			consol3.JK_MasterBillNum = "088-66666666";
			var consol4 = shipment.Consols.AddNew();
			consol4.JK_UniqueConsignRef = "088-77777777";
			consol4.JK_MasterBillNum = "088-77777777";
			var validMessage = CreateTransmitMessage("088-44444444", shipment.PK, CMDGenerator.Constants.GHA);
			CreateNonCMDTransmitMessage("088-44444444", shipment.PK, CMDGenerator.Constants.GHA);
			CreateTransmitMessage("088-44444444", shipment.PK, CMDGenerator.Constants.GHA, false);
			CreateTransmitMessage("088-44444444", shipment.PK, CMDGenerator.Constants.TDB);
			CreateTransmitMessage("088-55555555", shipment.PK, CMDGenerator.Constants.TDB);
			CreateTransmitMessage("088-66666666", shipment.PK, CMDGenerator.Constants.GHA);
			CreateTransmitMessage("088-66666666", shipment.PK, CMDGenerator.Constants.TDB);
			CreateTransmitMessage("088-77777777", shipment.PK, CMDGenerator.Constants.GHA);
			CreateTransmitMessage("088-77777777", shipment.PK, CMDGenerator.Constants.TDB);
			return validMessage;
		}

		CMDEDIMessage CreateBizOsForMessage7()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "088-99898989";
			consol.JK_MasterBillNum = "088-99898989";
			return CreateTransmitMessage("088-99898989", shipment.PK, CMDGenerator.Constants.GHA);
		}

		CMDEDIMessage CreateNonCMDTransmitMessage(string applicationReference, ZGuid shipmentPK, string subType)
		{
			return CreateTransmitMessage(applicationReference, shipmentPK, EDIMessage.ApplicationCodes.CIM, subType, true);
		}

		CMDEDIMessage CreateTransmitMessage(string applicationReference, ZGuid shipmentPK, string subType, bool isActive = true)
		{
			return CreateTransmitMessage(applicationReference, shipmentPK, EDIMessage.ApplicationCodes.SingaporeCMD, subType, isActive);
		}

		CMDEDIMessage CreateTransmitMessage(string applicationReference, ZGuid shipmentPK, string messageType, string subType, bool isActive)
		{
			var message = Factory.New<CMDEDIMessage>();
			message.EM_ApplicationReference = applicationReference;
			message.EM_LinkUniqueID = shipmentPK;
			message.EM_LinkTable = shipmentPK == ZGuid.Empty ? string.Empty : JobShipmentSchema.Constants.TableName;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = subType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_IsActive = isActive;
			return message;
		}

		CMDEDIMessage[] GetTotalIncomingMessages(bool showErrorOnly)
		{
			var filter = new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.NotEqual, EDIMessageTypeList.Codes.XDC);
			if (showErrorOnly)
			{
				filter.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Failed);
			}

			filter.OrderBy = EDIMessageSchema.Constants.EM_ApplicationReference;
			return (CMDEDIMessage[])Factory.Load(typeof(CMDEDIMessage), filter);
		}

		void SetEventsTime()
		{
			var filter = new ZQuery(StmALogSchema.SL_Table, EDIMessageSchema.Constants.TableName);
			var events = (BaseStmALog[])Factory.Load(typeof(BaseStmALog), filter);
			var eventTime = new ZDateTime(2001, 1, 1);
			foreach (var @event in events)
			{
				@event.SL_EventTime = eventTime;
			}
		}

		#region Dummy and Constants
		const string FNAMessage1 = "<CMD>" + "FNA\r\n" + "ACK/503 INVALID CRIA NO - SND\r\n" + "CMD/2\r\n" + "A/N/N\r\n" + "MWB/088-12345101JKTJTY/T77K234.99\r\n" + "/AS PER MANIFEST\r\n" + "FLT/QF8898/01JAN\r\n" + "HWB/SERIAL123/89/L56.78\r\n" + "/LUXURY CAR A/HAR001\r\n" + "/ELEPHANT WITH TRUNKS/HAR005\r\n" + "/BIG ELEPHANTS/HAR110\r\n" + "/SMALL ELEPHANTS/HAR111\r\n" + "SHP/FREIGHT SENDER 101\r\n" + "/111 BOURKE ROAD ALEXANDRIA NSW 2000 AUSTRALIA CNE/FREIGHT RECEIVER 102\r\n" + "/101 BLABLABLA STREET YUMMMMM CAIRO XXY131Z EGYPT\r\n" + "TPT/TDB/TDB0000101\r\n" + "/TDB0000102\r\n" + "/TDB0000103\r\n" + "/TDB0000104\r\n" + "CED/Y/CED0000101/CED0000102/CED0000103\r\n" + "IMW/111-12345678\r\n" + "EXP/TS/BLABLABLA\r\n" + "SND/TEST USER/TEST COMPANY 123\r\n" + "/TEST/TEST/TEST\r\n" + "</CMD>";
		const string CMAMessage2 = "<CMD>" + "CMA\r\n" + "CMD/2\r\n" + "A/N/Y\r\n" + "MWB/088-11111111JKTJTY/T77K234.99\r\n" + "/AS PER MANIFEST\r\n" + "FLT/QF8898/01JAN\r\n" + "SHP/FREIGHT SENDER 101\r\n" + "/111 BOURKE ROAD ALEXANDRIA NSW 2000 AUSTRALIA CNE/FREIGHT RECEIVER 102\r\n" + "/101 BLABLABLA STREET YUMMMMM CAIRO XXY131Z EGYPT\r\n" + "IMW/111-12345678\r\n" + "EXP/TS/BLABLABLA\r\n" + "SND/TEST USER/TEST COMPANY 123\r\n" + "/TEST/TEST/TEST\r\n" + "</CMD>";
		const string FMAMessage3 = "<CMD>" + "FMA\r\n" + "CMD/2\r\n" + "A/N/Y\r\n" + "</CMD>";
		const string FNAMessage4 = "<CMD>" + "FNA\r\n" + "ACK/503 INVALID CRIA NO - SND\r\n" + "CMD/2\r\n" + "A/N/N\r\n" + "MWB/088-22222222JKTJTY/T77K234.99\r\n" + "/AS PER MANIFEST\r\n" + "FLT/QF8898/01JAN\r\n" + "HWB/SERIAL345/89/L56.78\r\n" + "/BIG ELEPHANTS/HAR110\r\n" + "/SMALL ELEPHANTS/HAR111\r\n" + "SHP/FREIGHT SENDER 101\r\n" + "/111 BOURKE ROAD ALEXANDRIA NSW 2000 AUSTRALIA CNE/FREIGHT RECEIVER 102\r\n" + "/101 BLABLABLA STREET YUMMMMM CAIRO XXY131Z EGYPT\r\n" + "TPT/TDB/TDB0000101\r\n" + "CED/Y/CED0000101/CED0000102/CED0000103\r\n" + "IMW/111-12345678\r\n" + "EXP/TS/BLABLABLA\r\n" + "SND/TEST USER/TEST COMPANY 123\r\n" + "/TEST/TEST/TEST\r\n" + "</CMD>";
		const string FNAMessage5 = "<CMD>" + "FNA\r\n" + "ACK/503 INVALID CRIA NO - SND\r\n" + "CMD/2\r\n" + "A/N/N\r\n" + "MWB/088-33333333JKTJTY/T77K234.99\r\n" + "</CMD>";
		const string FNAMessage6 = "<CMD>" + "FNA\r\n" + "ACK/503 INVALID CRIA NO - SND\r\n" + "CMD/2\r\n" + "A/N/N\r\n" + "MWB/088-44444444JKTJTY/T77K234.99\r\n" + "HWB/SERIAL101/89/L56.78\r\n" + "</CMD>";
		const string CMAMessage7 = "<CMD>" + "CMA\r\n" + "CMD/2\r\n" + "A/N/Y\r\n" + "MWB/088-99898989AUSYD/T77K12.21\r\n" + "</CMD>";
		const string OtherMessage = "<CMD>" + "BLA\r\n" + "ACK/503 INVALID CRIA NO - SND\r\n" + "CMD/2\r\n" + "A/N/N\r\n" + "MWB/088-22222222JKTJTY/T77K234.99\r\n" + "/AS PER MANIFEST\r\n" + "FLT/QF8898/01JAN\r\n" + "HWB/SERIAL345/89/L56.78\r\n" + "</CMD>";
		#endregion
	}
}
