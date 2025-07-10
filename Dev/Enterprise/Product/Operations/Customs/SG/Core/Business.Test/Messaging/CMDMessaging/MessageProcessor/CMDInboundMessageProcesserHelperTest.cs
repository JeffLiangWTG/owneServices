using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	public class CMDInboundMessageProcesserHelperTest : TestCaseWithFactory
	{
		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestFindWithMultipleConsolsGetOnlyOne()
		{
			var outgoingMessage = SetupForTestFindWithMultipleConsols();
			var fnaMessage = new CMDInbound(FNAMessage1);
			var interchange = CreateNewEDIInterchange(FNAMessage1);
			Factory.Save();
			outgoingMessage.EM_LinkTable = JobShipmentSchema.Constants.TableName;
			var logger = new LoggingInformation();
			var helper = new CMDInboundMessageProcesserHelper(logger);
			helper.CreateMessageThrouthInterchangeAndSendEmail(interchange, fnaMessage);
			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, outgoingMessage.PK));
			AssertEquals(1, messages.Length);
			AssertCollectionContains("\tSuccessfully processed inbound FNA message for MAWB '088-12345101' and HAWB 'SERIAL123'", logger.UserLogStrings);
		}

		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestFindWithMultipleConsolsGetTwo()
		{
			var outgoingMessage = SetupForTestFindWithMultipleConsols(false);
			var fnaMessage = new CMDInbound(FNAMessage1);
			var interchange = CreateNewEDIInterchange(FNAMessage1);
			Factory.Save();
			var logger = new LoggingInformation();
			var helper = new CMDInboundMessageProcesserHelper(logger);
			helper.CreateMessageThrouthInterchangeAndSendEmail(interchange, fnaMessage);
			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, outgoingMessage.PK));
			AssertEquals(0, messages.Length);
			AssertCollectionContains("\tDid not find exactly one consol dated within the MAWB recycle period. Found 2. MAWB# was 088-12345101.", logger.UserLogStrings);
		}

		public void TestInterchangeHasNoOriginatingMessage()
		{
			var interchange = CreateNewEDIInterchange(FNAMessage1);
			var inboundMessage = new CMDInbound(interchange.EI_BodyText);
			Factory.Save();
			var helper = new CMDInboundMessageProcesserHelper(new LoggingInformation());
			Assert(!helper.CreateMessageThrouthInterchangeAndSendEmail(interchange, inboundMessage));
			Factory.Save();
			var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			AssertNotNull(message);
			AssertEquals(EDIMessage.ApplicationCodes.SingaporeCMD, message.EM_ApplicationCode);
			AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.Status.Failed, message.EM_Status);
			AssertEquals(CMDInbound.Constants.FNA, message.EM_MessageType);
			AssertEquals(interchange.EI_BodyText, message.EM_MessageText);
		}

		public void TestInterchangeHasOriginatingMessage()
		{
			var interchange = CreateNewEDIInterchange(FNAMessage1);
			var originatingMessage = CreateBizOsForMessage1();
			var inboundMessage = new CMDInbound(interchange.EI_BodyText);
			Factory.Save();
			var helper = new CMDInboundMessageProcesserHelper(new LoggingInformation());
			Assert(helper.CreateMessageThrouthInterchangeAndSendEmail(interchange, inboundMessage));
			Factory.Save();
			var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			AssertNotNull(message);
			AssertEquals(EDIMessage.ApplicationCodes.SingaporeCMD, message.EM_ApplicationCode);
			AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
			AssertEquals(CMDInbound.Constants.FNA, message.EM_MessageType);
			AssertEquals(interchange.EI_BodyText, message.EM_MessageText);
			AssertEquals(originatingMessage.PK, message.EM_LinkUniqueID);
			AssertEquals(EDIMessageSchema.Constants.TableName, message.EM_LinkTable);
		}

		public void TestInterchangeHasOriginatingMessageIfHouseBillHasSpecialCharacter()
		{
			var interchange = CreateNewEDIInterchange(FNAMessage1);
			var originatingMessage = CreateBizOsForMessage1IfHouseBillHasSpecialCharacter();
			var inboundMessage = new CMDInbound(interchange.EI_BodyText);
			Factory.Save();
			var helper = new CMDInboundMessageProcesserHelper(new LoggingInformation());
			Assert(helper.CreateMessageThrouthInterchangeAndSendEmail(interchange, inboundMessage));
			Factory.Save();
			var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			AssertNotNull(message);
			AssertEquals(EDIMessage.ApplicationCodes.SingaporeCMD, message.EM_ApplicationCode);
			AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
			AssertEquals(CMDInbound.Constants.FNA, message.EM_MessageType);
			AssertEquals(interchange.EI_BodyText, message.EM_MessageText);
			AssertEquals(originatingMessage.PK, message.EM_LinkUniqueID);
			AssertEquals(EDIMessageSchema.Constants.TableName, message.EM_LinkTable);
		}

		public void TestFailureEmails()
		{
			TestFailureEmails(true);
		}

		public void TestFailureEmailsWhenSenderHasNoEmailAddress()
		{
			TestFailureEmails(false);
		}

		void TestFailureEmails(bool senderHasEmail)
		{
			SetNotificationGroupEmails(senderHasEmail);
			var interchangeFNA = CreateNewEDIInterchange(FNAMessage1);
			var inboundMessageFNA = new CMDInbound(interchangeFNA.EI_BodyText);
			CreateBizOsForMessage1();
			var interchangeOther = CreateNewEDIInterchange(OtherMessage);
			var inboundMessageOther = new CMDInbound(interchangeOther.EI_BodyText);
			var interchangeFMA = CreateNewEDIInterchange(CMAMessage7);
			var inboundMessageFMA = new CMDInbound(interchangeFMA.EI_BodyText);
			Factory.Save();
			SetEventsTime();
			Factory.Save();
			var helper = new CMDInboundMessageProcesserHelper(new LoggingInformation());
			helper.CreateMessageThrouthInterchangeAndSendEmail(interchangeFNA, inboundMessageFNA);
			helper.CreateMessageThrouthInterchangeAndSendEmail(interchangeOther, inboundMessageOther);
			helper.CreateMessageThrouthInterchangeAndSendEmail(interchangeFMA, inboundMessageFMA);
			Factory.Save();
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			Env.OutgoingCustomsMailManager.EmailsCreated.Sort(new Comparison<EmailDef>(CompareEmailDef));
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var subject = "FNA (ERROR) Message Received for MasterBill: '088-12345101'";
			var body = "Message Sent: " + new ZDateTime(2001, 1, 1).ToString() + "\n" + FNAMessage1;
			if (senderHasEmail)
			{
				AssertMailItem(email, subject, body, "test@test.edi.com.au", null);
			}
			else
			{
				AssertMailItem(email, subject, body, "postnotify@test.edi.com.au", null);
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

		int CompareEmailDef(EmailDef emailX, EmailDef emailY)
		{
			var result = emailX.Subject.CompareTo(emailY.Subject);
			if (result == 0)
			{
				result = emailX.Body.CompareTo(emailY.Body);
			}

			return result;
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

		void SetNotificationGroupEmails(bool senderHasEmail)
		{
			var util = new EmailGroupUtility();
			var emails = util.GetCompanyNotificationGroupEmails();
			AssertEquals("Pre-condition, make sure that no NotificationGroup emails have been set", 0, emails.Count);
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_EmailAddress = senderHasEmail ? "test@test.edi.com.au" : "";
			var postMaster = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, User.PostMasterUserName);
			postMaster.GS_EmailAddress = "postnotify@test.edi.com.au";
			Factory.Save();
			emails = util.GetCompanyNotificationGroupEmails();
			AssertEquals("Should now be set to postnotify@test.edi.com.au", 1, emails.Count);
			AssertEquals("Should now be set to postnotify@test.edi.com.au", "postnotify@test.edi.com.au", emails[0]);
		}

		EDIInterchange CreateNewEDIInterchange(string body)
		{
			var eDIInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			eDIInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.SingaporeCMD;
			eDIInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			eDIInterchange.EI_Status = EDIInterchange.Status.Queued;
			eDIInterchange.EI_BodyText = body;
			return eDIInterchange;
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

		CMDEDIMessage CreateBizOsForMessage1IfHouseBillHasSpecialCharacter()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "08812345101";
			consol.JK_UniqueConsignRef = "08812345101";
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "SERI-AL 123";
			var validMessage = CreateTransmitMessage("08812345101", shipment1.PK, CMDGenerator.Constants.GHA);
			CreateTransmitMessage("08812345101", shipment1.PK, CMDGenerator.Constants.GHA, false);
			CreateTransmitMessage("08812345101", shipment1.PK, CMDGenerator.Constants.TDB);
			CreateTransmitMessage("08812345101", shipment2.PK, CMDGenerator.Constants.GHA);
			return validMessage;
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

		CMDEDIMessage SetupForTestFindWithMultipleConsols(bool getOnlyOne = true)
		{
			var consolOne = Factory.New<ForwardingConsol>();
			var consolTwo = Factory.New<ForwardingConsol>();
			consolOne.JK_UniqueConsignRef = "C000100";
			consolOne.JK_MasterBillNum = "08812345101";
			consolTwo.JK_MasterBillNum = consolOne.JK_MasterBillNum;
			var mawbRecycleMonths = FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
			consolOne.JK_MasterBillIssueDate = ZDateTime.Now.AddMonths(1 - mawbRecycleMonths); // 11 months old - fresh
			consolTwo.JK_MasterBillIssueDate = ZDateTime.Now.AddMonths(-1 - mawbRecycleMonths); //13 months old - ignored
			if (!getOnlyOne)
			{
				consolTwo.JK_MasterBillIssueDate = ZDateTime.Now.AddDays(-6);
			}

			var outgoingMessage = Factory.New<CMDEDIMessage>();
			outgoingMessage.EM_ApplicationReference = consolOne.JK_UniqueConsignRef;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeCMD;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_IsActive = ZBool.True;
			return outgoingMessage;
		}

		#region Dummy and Constants
		const string FNAMessage1 = "CMDFNA\r\n" + "CCN\r\n" + "FNA\r\n" + "ACK/503 INVALID CRIA NO - SND\r\n" + "CMD/2\r\n" + "A/N/N\r\n" + "MWB/088-12345101JKTJTY/T77K234.99\r\n" + "/AS PER MANIFEST\r\n" + "FLT/QF8898/01JAN\r\n" + "HWB/SERIAL123/89/L56.78\r\n" + "/LUXURY CAR A/HAR001\r\n" + "/ELEPHANT WITH TRUNKS/HAR005\r\n" + "/BIG ELEPHANTS/HAR110\r\n" + "/SMALL ELEPHANTS/HAR111\r\n" + "SHP/FREIGHT SENDER 101\r\n" + "/111 BOURKE ROAD ALEXANDRIA NSW 2000 AUSTRALIA CNE/FREIGHT RECEIVER 102\r\n" + "/101 BLABLABLA STREET YUMMMMM CAIRO XXY131Z EGYPT\r\n" + "TPT/TDB/TDB0000101\r\n" + "/TDB0000102\r\n" + "/TDB0000103\r\n" + "/TDB0000104\r\n" + "CED/Y/CED0000101/CED0000102/CED0000103\r\n" + "IMW/111-12345678\r\n" + "EXP/TS/BLABLABLA\r\n" + "SND/TEST USER/TEST COMPANY 123\r\n" + "/TEST/TEST/TEST\r\n";
		const string CMAMessage7 = "CMDCMA\r\n" + "SAT\r\n" + "CMA\r\n" + "CMD/2\r\n" + "A/N/Y\r\n" + "MWB/088-99898989AUSYD/T77K12.21\r\n";
		const string OtherMessage = "BLA\r\n" + "CCN\r\n" + "BLA\r\n" + "ACK/503 INVALID CRIA NO - SND\r\n" + "CMD/2\r\n" + "A/N/N\r\n" + "MWB/088-22222222JKTJTY/T77K234.99\r\n" + "/AS PER MANIFEST\r\n" + "FLT/QF8898/01JAN\r\n" + "HWB/SERIAL345/89/L56.78\r\n";
		#endregion
	}
}
