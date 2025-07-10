using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage.Testing
{
	sealed class CMDMessageReceiverTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			var transmittedMessages = SetupMailItemsForTest1();
			var processor = new CMDMessageReceiver();
			processor.Process();
			var messages = GetIncomingMessages(transmittedMessages[0]);
			AssertEquals(1, messages.Length);
			AssertMessageAndInterchange(messages[0], FNAMessage1, "FNA", EDIMessage.Status.Received, transmittedMessages[0]);
			messages = GetIncomingMessages(transmittedMessages[1]);
			AssertEquals("Shouldn't be processed as there is more than one shipment attached to a direct consol", 0, messages.Length);
			messages = GetIncomingMessages(transmittedMessages[2]);
			AssertEquals("Shouldn't be processed as FMA reply only comes from CCN", 0, messages.Length);
			messages = GetIncomingMessages(transmittedMessages[3]);
			AssertEquals(1, messages.Length);
			AssertMessageAndInterchange(messages[0], FNAMessage4, "FNA", EDIMessage.Status.Received, transmittedMessages[3]);
			messages = GetIncomingMessages(transmittedMessages[4]);
			AssertEquals(1, messages.Length);
			AssertMessageAndInterchange(messages[0], FNAMessage6, "FNA", EDIMessage.Status.Received, transmittedMessages[4]);
			messages = GetIncomingMessages(transmittedMessages[5]);
			AssertEquals(1, messages.Length);
			AssertMessageAndInterchange(messages[0], CMAMessage7, "CMA", EDIMessage.Status.Received, transmittedMessages[5]);
			AssertEquals("Total Received Message", 6, GetTotalIncomingMessages(false).Length);
			messages = GetTotalIncomingMessages(true);
			AssertEquals("Failure Message", 2, messages.Length);
			var cMAIndex = (messages[0].EM_MessageType == "CMA") ? 0 : 1;
			var fNAIndex = (cMAIndex == 0) ? 1 : 0;
			AssertMessageAndInterchange(messages[cMAIndex], CMAMessage2, "CMA", EDIMessage.Status.Failed, null);
			AssertMessageAndInterchange(messages[fNAIndex], FNAMessage5, "FNA", EDIMessage.Status.Failed, null);
			AssertEquals(4, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void TestLicenseUsage()
		{
			SetNotificationGroupEmails(true);
			var logQuery = new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.CMDReporting.Name);
			var count = Factory.Load<StmActivityLog>(logQuery).Length;
			CreateBizOsForMessage7();
			CreateNewMailItem(CargoIMPServiceProvider.CargoWiseCargoImpEmailAddress, "CARGOIMP RESPONSE FROM EAGLE - CMD", CMAMessage7);
			Factory.Save();
			var processor = new CMDMessageReceiver();
			processor.Process();
			AssertEquals("Should be 1 activity log - licence usage", ++count, Factory.Load<StmActivityLog>(logQuery).Length);
			CreateNewMailItem(CargoIMPServiceProvider.CargoWiseCargoImpEmailAddress, "CARGOIMP RESPONSE FROM EAGLE - CMD", CMAMessage7);
			Factory.Save();
			processor.Process();
			AssertEquals("Should be 0 activity logs - licence usage", count, Factory.Load<StmActivityLog>(logQuery).Length);
		}

		public void TestFailureEmails()
		{
			TestFailureEmails(true);
		}

		public void TestFailureEmailsWhenSenderHasNoEmailAddress()
		{
			TestFailureEmails(false);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			to = (registrationKey.EnterpriseCode.Length == 0 ? (string)GlbCompany.CurrentCompany.GC_Code : registrationKey.EnterpriseCode) + registrationKey.ServerCode;
			currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
		}
		string to;
		GlbStaff currentUser;

		void TestFailureEmails(bool senderHasEmail)
		{
			SetupMailItemsForTest2(senderHasEmail);
			var processor = new CMDMessageReceiver();
			processor.Process();
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

		void AssertMessageAndInterchange(CMDEDIMessage message, string messageText, string identifier, string status, CMDEDIMessage originalMessage)
		{
			var interchange = Factory.Load<EDIInterchange>(message.EM_EI);
			AssertNotNull(interchange);
			AssertEquals(messageText, message.EM_MessageText);
			AssertEquals(identifier, message.EM_MessageType);
			AssertEquals(status, message.EM_Status);
			AssertEquals(message.EM_MessageText, interchange.EI_BodyText);
			AssertEquals(EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
			AssertEquals("EDI CCN", interchange.EI_From);
			AssertEquals(to, interchange.EI_To);
			if (originalMessage != null)
			{
				AssertEquals(EDIMessageSchema.Constants.TableName, message.EM_LinkTable);
				AssertEquals(originalMessage.PK, message.EM_LinkUniqueID);
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

		void SetEventsTime()
		{
			var filter = new ZQuery(StmALogSchema.SL_Table, EDIMessageSchema.Constants.TableName);
			var events = (BaseStmALog[])Factory.Load(typeof(BaseStmALog), filter);
			var eventTime = new ZDateTime(2001, 1, 1);
			foreach (BaseStmALog @event in events)
			{
				@event.SL_EventTime = eventTime;
			}
		}

		void CreateNewMailItem(string from, string subject, string body)
		{
			var mailItem = Factory.New<MailItem>();
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_Status = MailStatus.Queued;
			mailItem.MI_From = from;
			mailItem.MI_Subject = subject;
			mailItem.MI_Body = body;
			mailItem.MI_LastAttemptDateTime = ZDateTime.Now;
			mailItem.MI_ReceivedDateTime = ZDateTime.Now;
			mailItem.MI_SendDateTime = ZDateTime.Now;
			MailFilterLocatorTestHelper.SetApplication(mailItem, MailFilterCodes.CMDMessage, false);
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

		CMDEDIMessage CreateTransmitMessage(string applicationReference, ZGuid shipmentPK, string subType)
		{
			return CreateTransmitMessage(applicationReference, shipmentPK, EDIMessage.ApplicationCodes.SingaporeCMD, subType, true);
		}

		CMDEDIMessage CreateTransmitMessage(string applicationReference, ZGuid shipmentPK, string subType, bool isActive)
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

		CMDEDIMessage[] GetIncomingMessages(CMDEDIMessage transmittedMessage)
		{
			var filter = new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			filter.AddToFilter(EDIMessageSchema.EM_LinkTable, EDIMessageSchema.Constants.TableName);
			filter.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, transmittedMessage.PK);
			return (CMDEDIMessage[])Factory.Load(typeof(CMDEDIMessage), filter);
		}

		CMDEDIMessage[] GetTotalIncomingMessages(bool showErrorOnly)
		{
			var filter = new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			if (showErrorOnly)
			{
				filter.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Failed);
			}

			filter.OrderBy = EDIMessageSchema.Constants.EM_ApplicationReference;
			return (CMDEDIMessage[])Factory.Load(typeof(CMDEDIMessage), filter);
		}

		void SetNotificationGroupEmails(bool senderHasEmail)
		{
			var util = new EmailGroupUtility();
			var emails = util.GetCompanyNotificationGroupEmails();
			AssertEquals("Pre-condition, make sure that no NotificationGroup emails have been set", 0, emails.Count);
			var postMaster = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, User.PostMasterUserName);
			postMaster.GS_EmailAddress = "postnotify@test.edi.com.au";
			Factory.Save();
			emails = util.GetCompanyNotificationGroupEmails();
			AssertEquals("Should now be set to postnotify@test.edi.com.au", 1, emails.Count);
			AssertEquals("Should now be set to postnotify@test.edi.com.au", "postnotify@test.edi.com.au", emails[0]);
			currentUser.GS_EmailAddress = (senderHasEmail) ? "test@test.edi.com.au" : "";
		}

		CMDEDIMessage[] SetupMailItemsForTest1()
		{
			var messages = new CMDEDIMessage[6];
			SetNotificationGroupEmails(true);
			var processor = new CMDMessageReceiver();
			CreateNewMailItem("blaasdf@asdf.com", "This is the subject", "212123");
			CreateNewMailItem(CargoIMPServiceProvider.CargoWiseCargoImpEmailAddress, "Wrong subject", "body 123");
			CreateNewMailItem(CargoIMPServiceProvider.CargoWiseCargoImpEmailAddress, "CARGOIMP RESPONSE FROM EAGLE", "body 123");
			CreateNewMailItem(CargoIMPServiceProvider.CargoWiseCargoImpEmailAddress, "CARGOIMP RESPONSE FROM EAGLE - CMD", FNAMessage1);
			messages[0] = CreateBizOsForMessage1();
			CreateNewMailItem(CargoIMPServiceProvider.CargoWiseCargoImpEmailAddress, "CARGOIMP RESPONSE FROM EAGLE - CMD", CMAMessage2);
			messages[1] = CreateBizOsForMessage2();
			CreateNewMailItem(CargoIMPServiceProvider.CargoWiseCargoImpEmailAddress, "CARGOIMP RESPONSE FROM EAGLE - CMD", FMAMessage3);
			messages[2] = CreateBizOsForMessage3();
			CreateNewMailItem(CargoIMPServiceProvider.CargoWiseCargoImpEmailAddress, "CARGOIMP RESPONSE FROM EAGLE - CMD", FNAMessage4);
			messages[3] = CreateBizOsForMessage4();
			CreateNewMailItem(CargoIMPServiceProvider.CargoWiseCargoImpEmailAddress, "CARGOIMP RESPONSE FROM EAGLE - CMD", FNAMessage5);
			CreateNewMailItem(CargoIMPServiceProvider.CargoWiseCargoImpEmailAddress, "CARGOIMP RESPONSE FROM EAGLE - CMD", FNAMessage6);
			messages[4] = CreateBizOsForMessage6();
			CreateNewMailItem(CargoIMPServiceProvider.CargoWiseCargoImpEmailAddress, "CARGOIMP RESPONSE FROM EAGLE - CMD", CMAMessage7);
			messages[5] = CreateBizOsForMessage7();
			CreateNewMailItem(CargoIMPServiceProvider.CargoWiseCargoImpEmailAddress, "CARGOIMP RESPONSE FROM EAGLE - CMD", OtherMessage);
			Factory.Save();
			return messages;
		}

		void SetupMailItemsForTest2(bool senderHasEmail)
		{
			SetNotificationGroupEmails(senderHasEmail);
			CreateNewMailItem(CargoIMPServiceProvider.CargoWiseCargoImpEmailAddress, "CARGOIMP RESPONSE FROM EAGLE - CMD", FNAMessage1);
			CreateBizOsForMessage1();
			CreateNewMailItem(CargoIMPServiceProvider.CargoWiseCargoImpEmailAddress, "CARGOIMP RESPONSE FROM EAGLE - CMD", OtherMessage);
			CreateNewMailItem(CargoIMPServiceProvider.CargoWiseCargoImpEmailAddress, "CARGOIMP RESPONSE FROM EAGLE - CMD", CMAMessage7);
			Factory.Save();
			SetEventsTime();
			Factory.Save();
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

		const string FNAMessage1 =
	"CMDFNA\r\n" +
	"CCN\r\n" +
	"FNA\r\n" +
	"ACK/503 INVALID CRIA NO - SND\r\n" +
	"CMD/2\r\n" +
	"A/N/N\r\n" +
	"MWB/088-12345101JKTJTY/T77K234.99\r\n" +
	"/AS PER MANIFEST\r\n" +
	"FLT/QF8898/01JAN\r\n" +
	"HWB/SERIAL123/89/L56.78\r\n" +
	"/LUXURY CAR A/HAR001\r\n" +
	"/ELEPHANT WITH TRUNKS/HAR005\r\n" +
	"/BIG ELEPHANTS/HAR110\r\n" +
	"/SMALL ELEPHANTS/HAR111\r\n" +
	"SHP/FREIGHT SENDER 101\r\n" +
	"/111 BOURKE ROAD ALEXANDRIA NSW 2000 AUSTRALIA CNE/FREIGHT RECEIVER 102\r\n" +
	"/101 BLABLABLA STREET YUMMMMM CAIRO XXY131Z EGYPT\r\n" +
	"TPT/TDB/TDB0000101\r\n" +
	"/TDB0000102\r\n" +
	"/TDB0000103\r\n" +
	"/TDB0000104\r\n" +
	"CED/Y/CED0000101/CED0000102/CED0000103\r\n" +
	"IMW/111-12345678\r\n" +
	"EXP/TS/BLABLABLA\r\n" +
	"SND/TEST USER/TEST COMPANY 123\r\n" +
	"/TEST/TEST/TEST\r\n";

		const string CMAMessage2 =
			"CMDCMA\r\n" +
			"CIA\r\n" +
			"CMA\r\n" +
			"CMD/2\r\n" +
			"A/N/Y\r\n" +
			"MWB/088-11111111JKTJTY/T77K234.99\r\n" +
			"/AS PER MANIFEST\r\n" +
			"FLT/QF8898/01JAN\r\n" +
			"SHP/FREIGHT SENDER 101\r\n" +
			"/111 BOURKE ROAD ALEXANDRIA NSW 2000 AUSTRALIA CNE/FREIGHT RECEIVER 102\r\n" +
			"/101 BLABLABLA STREET YUMMMMM CAIRO XXY131Z EGYPT\r\n" +
			"IMW/111-12345678\r\n" +
			"EXP/TS/BLABLABLA\r\n" +
			"SND/TEST USER/TEST COMPANY 123\r\n" +
			"/TEST/TEST/TEST\r\n";

		const string FMAMessage3 =
			"CMDFMA\r\n" +
			"CCN\r\n" +
			"FMA\r\n" +
			"CMD/2\r\n" +
			"A/N/Y\r\n";

		const string FNAMessage4 =
			"CMDFNA\r\n" +
			"CCN\r\n" +
			"FNA\r\n" +
			"ACK/503 INVALID CRIA NO - SND\r\n" +
			"CMD/2\r\n" +
			"A/N/N\r\n" +
			"MWB/088-22222222JKTJTY/T77K234.99\r\n" +
			"/AS PER MANIFEST\r\n" +
			"FLT/QF8898/01JAN\r\n" +
			"HWB/SERIAL345/89/L56.78\r\n" +
			"/BIG ELEPHANTS/HAR110\r\n" +
			"/SMALL ELEPHANTS/HAR111\r\n" +
			"SHP/FREIGHT SENDER 101\r\n" +
			"/111 BOURKE ROAD ALEXANDRIA NSW 2000 AUSTRALIA CNE/FREIGHT RECEIVER 102\r\n" +
			"/101 BLABLABLA STREET YUMMMMM CAIRO XXY131Z EGYPT\r\n" +
			"TPT/TDB/TDB0000101\r\n" +
			"CED/Y/CED0000101/CED0000102/CED0000103\r\n" +
			"IMW/111-12345678\r\n" +
			"EXP/TS/BLABLABLA\r\n" +
			"SND/TEST USER/TEST COMPANY 123\r\n" +
			"/TEST/TEST/TEST\r\n";

		const string FNAMessage5 =
			"CMDFNA\r\n" +
			"CCN\r\n" +
			"FNA\r\n" +
			"ACK/503 INVALID CRIA NO - SND\r\n" +
			"CMD/2\r\n" +
			"A/N/N\r\n" +
			"MWB/088-33333333JKTJTY/T77K234.99\r\n";

		const string FNAMessage6 =
			"CMDFNA\r\n" +
			"CCN\r\n" +
			"FNA\r\n" +
			"ACK/503 INVALID CRIA NO - SND\r\n" +
			"CMD/2\r\n" +
			"A/N/N\r\n" +
			"MWB/088-44444444JKTJTY/T77K234.99\r\n" +
			"HWB/SERIAL101/89/L56.78\r\n";

		const string CMAMessage7 =
			"CMDCMA\r\n" +
			"SAT\r\n" +
			"CMA\r\n" +
			"CMD/2\r\n" +
			"A/N/Y\r\n" +
			"MWB/088-99898989AUSYD/T77K12.21\r\n";

		const string OtherMessage =
			"BLA\r\n" +
			"CCN\r\n" +
			"BLA\r\n" +
			"ACK/503 INVALID CRIA NO - SND\r\n" +
			"CMD/2\r\n" +
			"A/N/N\r\n" +
			"MWB/088-22222222JKTJTY/T77K234.99\r\n" +
			"/AS PER MANIFEST\r\n" +
			"FLT/QF8898/01JAN\r\n" +
			"HWB/SERIAL345/89/L56.78\r\n";
	}
}
