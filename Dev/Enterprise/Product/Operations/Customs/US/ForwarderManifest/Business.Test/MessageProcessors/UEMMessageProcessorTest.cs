using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class UEMMessageProcessorTest : ApplicationTypeMessageProcessorTest
	{
		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", string.Empty, uemMessageProcessor.MessageFriendlyName);
		}

		public void TestApplicationCode()
		{
			AssertEquals("ApplicationCode", ApplicationCodeList.Codes.USExportManifest, uemMessageProcessor.ApplicationCode);
		}

		public void TestProcessMessage()
		{
			var message = Factory.New<UEMEDIMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			uemMessageProcessor.ProcessMessage(message);
			AssertEquals("No valid processor is available yet.", EDIMessage.Status.Failed, message.EM_Status);
		}

		public void TestExportManifestProcessMessage_MessageReferenceNumber()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "TEST1";
			staff.GS_Code = "TS1";
			staff.GS_IsSystemAccount = false;
			staff.GS_IsActive = true;
			staff.GS_EmailAddress = "test1@wisetechglobal.com";

			var manifest = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, USExportManifestTypes.Codes.EFM);
			manifest.AMA_ManifestType = USExportManifestTypes.Codes.EFM;
			manifest.AMA_JobReference = "MAN123456789";
			var bill = manifest.Bills.AddNew();
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ABL_BillNumber = "111";

			var sendMessage = Factory.New<UEMEDIMessage>();
			sendMessage.EM_MessageType = MessageTypeList.Codes.ExportManifestSubmission;
			sendMessage.EM_Status = EDIMessage.Status.Sent;
			sendMessage.EM_ApplicationCode = ApplicationCodeList.Codes.USExportManifest;
			sendMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			sendMessage.EM_LinkedObject = bill;
			sendMessage.EM_SystemCreateUser = "TS1";
			Factory.Save();

			var receivedMessage = Factory.New<UEMEDIMessage>();
			receivedMessage.EM_MessageType = MessageTypeList.Codes.ExportManifestResponse;
			receivedMessage.EM_Status = EDIMessage.Status.Queued;
			receivedMessage.EM_ApplicationCode = ApplicationCodeList.Codes.USExportManifest;
			receivedMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			receivedMessage.EM_MessageData = new ZBlob(MessageEncoding.UTF8WithoutBOM.GetBytes($"<?xml version=\"1.0\" encoding=\"utf-16\"?><CBPManifestMessage xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://manifest.cbp.dhs.gov/shared/model\"><Version /><Filing><MessageControlNumber><Value></Value></MessageControlNumber><MessageReferenceNumber><Value>{sendMessage.EM_MessageNum}</Value></MessageReferenceNumber></Filing></CBPManifestMessage>"));
			Factory.Save();
			uemMessageProcessor.ProcessMessage(receivedMessage);

			AssertEquals(AsycudaBill.Schema.TableName, receivedMessage.EM_LinkTable);
			AssertEquals(bill.PK, receivedMessage.EM_LinkUniqueID);
			AssertEquals("EDIEDIDAT_1", sendMessage.EM_MessageNum);
			AssertEquals("EDIEDIDAT_1", receivedMessage.EM_MessageNum);
			AssertEquals(EDIMessage.Status.Received, receivedMessage.EM_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("Export Manifest")));
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(staff.GS_EmailAddress, email.Recipients[0].Email);
			AssertEquals("Export Manifest Response for MAN123456789 - EDIEDIDAT_1", email.Subject);
			AssertContains(receivedMessage.EM_MessageInterpretation, email.Body);
		}

		public void TestExportManifestProcessMessage_MessageControlNumber()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "TEST1";
			staff.GS_Code = "TS1";
			staff.GS_IsSystemAccount = false;
			staff.GS_IsActive = true;
			staff.GS_EmailAddress = "test1@wisetechglobal.com";

			var manifest = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, USExportManifestTypes.Codes.EFM);
			manifest.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.Consolidator;
			manifest.AMA_ManifestType = USExportManifestTypes.Codes.EFM;
			manifest.AMA_JobReference = "MAN123456789";
			manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			var bill = manifest.Bills.AddNew();
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ABL_BillNumber = "111";

			var sendMessage = Factory.New<UEMEDIMessage>();
			sendMessage.EM_MessageType = MessageTypeList.Codes.ExportManifestSubmission;
			sendMessage.EM_Status = EDIMessage.Status.Sent;
			sendMessage.EM_ApplicationCode = ApplicationCodeList.Codes.USExportManifest;
			sendMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			sendMessage.EM_LinkedObject = bill;
			sendMessage.EM_SystemCreateUser = "TS1";
			Factory.Save();

			var receivedMessage = Factory.New<UEMEDIMessage>();
			receivedMessage.EM_MessageType = MessageTypeList.Codes.ExportManifestResponse;
			receivedMessage.EM_Status = EDIMessage.Status.Queued;
			receivedMessage.EM_ApplicationCode = ApplicationCodeList.Codes.USExportManifest;
			receivedMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			receivedMessage.EM_MessageData = new ZBlob(MessageEncoding.UTF8WithoutBOM.GetBytes($"<?xml version=\"1.0\" encoding=\"utf-16\"?><CBPManifestMessage xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://manifest.cbp.dhs.gov/shared/model\"><Version /><Filing><MessageControlNumber><Value>{manifest.AMA_JobReference}</Value></MessageControlNumber><MessageReferenceNumber><Value></Value></MessageReferenceNumber></Filing></CBPManifestMessage>"));
			Factory.Save();
			uemMessageProcessor.ProcessMessage(receivedMessage);

			AssertEquals(AsycudaManifestHeader.Schema.TableName, receivedMessage.EM_LinkTable);
			AssertEquals(manifest.PK, receivedMessage.EM_LinkUniqueID);
			AssertEquals(ZString.Empty, receivedMessage.EM_MessageNum);
			AssertEquals(EDIMessage.Status.Received, receivedMessage.EM_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("Export Manifest")));
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(staff.GS_EmailAddress, email.Recipients[0].Email);
			AssertEquals("Export Manifest Response for MAN123456789 ", email.Subject);
			AssertContains(receivedMessage.EM_MessageInterpretation, email.Body);
		}

		public void TestGenerateHtmlEmailAndSendToUser()
		{
			var user1 = Factory.New<GlbStaff>();
			user1.GS_EmailAddress = "dummy1@where.com";
			user1.GS_IsSystemAccount = false;
			user1.GS_Code = "TS1";
			user1.GS_LoginName = "TS1 Name";

			var user2 = Factory.New<GlbStaff>();
			user2.GS_EmailAddress = "dummy2@where.com";
			user2.GS_IsSystemAccount = false;
			user2.GS_Code = "TS2";
			user2.GS_LoginName  = "TS2 Name";

			Factory.Save();

			var manifest = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, USExportManifestTypes.Codes.EFM);
			manifest.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.Consolidator;
			manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
			manifest.AMA_JobReference = "MANPHL4X0001";

			var bill = manifest.Bills.AddNew();
			var sendMessage = Factory.New<UEMEDIMessage>();
			sendMessage.EM_MessageType = MessageTypeList.Codes.ExportManifestSubmission;
			sendMessage.EM_Status = EDIMessage.Status.Sent;
			sendMessage.EM_ApplicationCode = ApplicationCodeList.Codes.USExportManifest;
			sendMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			sendMessage.EM_LinkedObject = bill;
			sendMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-3);
			sendMessage.EM_SystemCreateUser = user1.GS_Code;
			sendMessage.EM_MessageNum = "EDIEDIDAT_1";
			bill.Messages.Add(sendMessage);

			var bill2 = manifest.Bills.AddNew();
			var sendMessage2 = Factory.New<UEMEDIMessage>();
			sendMessage2.EM_MessageType = MessageTypeList.Codes.ExportManifestSubmission;
			sendMessage2.EM_Status = EDIMessage.Status.Sent;
			sendMessage2.EM_ApplicationCode = ApplicationCodeList.Codes.USExportManifest;
			sendMessage2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			sendMessage2.EM_LinkedObject = bill2;
			sendMessage2.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-2);
			sendMessage2.EM_SystemCreateUser = user2.GS_Code;
			sendMessage2.EM_MessageNum = "EDIEDIDAT_2";
			bill2.Messages.Add(sendMessage2);
			Factory.Save();

			var receivedMessage = CreateManifestResponseMessage(manifest.AMA_JobReference, "", "970", "000", "000");
			uemMessageProcessor.ProcessMessage(receivedMessage);
			AssertEquals(MessageStatusCodeList.Codes.Error, manifest.AMA_MessageStatus);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("Export Manifest")));
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("dummy2@where.com", email.Recipients[0].Email);
			AssertEquals("Export Manifest Response (Failure) for MANPHL4X0001 ", email.Subject);
			AssertContains(receivedMessage.EM_MessageInterpretation, email.Body);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			receivedMessage = CreateManifestResponseMessage(manifest.AMA_JobReference, "", "AAA", "BBB", "000");
			uemMessageProcessor.ProcessMessage(receivedMessage);
			AssertEquals(MessageStatusCodeList.Codes.Registered, manifest.AMA_MessageStatus);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("Export Manifest")));
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("dummy2@where.com", email.Recipients[0].Email);
			AssertEquals("Export Manifest Response for MANPHL4X0001 ", email.Subject);
			AssertContains(receivedMessage.EM_MessageInterpretation, email.Body);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var messageNum = sendMessage.EM_MessageNum;
			receivedMessage = CreateManifestResponseMessage("", messageNum, "970", "000", "000");
			uemMessageProcessor.ProcessMessage(receivedMessage);
			AssertEquals(MessageStatusCodeList.Codes.Error, manifest.AMA_MessageStatus);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("Export Manifest")));
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("dummy1@where.com", email.Recipients[0].Email);
			AssertEquals("Export Manifest Response (Failure) for MANPHL4X0001 - " + messageNum, email.Subject);
			AssertContains(receivedMessage.EM_MessageInterpretation, email.Body);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			receivedMessage = CreateManifestResponseMessage("", messageNum, "AAA", "BBB", "000");
			uemMessageProcessor.ProcessMessage(receivedMessage);
			AssertEquals(MessageStatusCodeList.Codes.Registered, manifest.AMA_MessageStatus);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("Export Manifest")));
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("dummy1@where.com", email.Recipients[0].Email);
			AssertEquals("Export Manifest Response for MANPHL4X0001 - " + messageNum, email.Subject);
			AssertContains(receivedMessage.EM_MessageInterpretation, email.Body);
		}

		public void TestUpdateManifestHeaderMessageStatus()
		{
			var manifest = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, USExportManifestTypes.Codes.EFM);
			manifest.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.Consolidator;
			manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
			manifest.AMA_JobReference = "MANPHL4X0001";
			AssertEquals(string.Empty, manifest.AMA_MessageStatus);

			var manifestMessageWithUnknownResponseCode = CreateManifestResponseMessage(manifest.AMA_JobReference, "", "AAA", "BBB", "CCC");
			uemMessageProcessor.ProcessMessage(manifestMessageWithUnknownResponseCode);
			AssertEquals(string.Empty, manifest.AMA_MessageStatus);

			manifest.AMA_MessageStatus = "";
			var manifestRejectMessage = CreateManifestResponseMessage(manifest.AMA_JobReference, "", "970", "000", "000");
			uemMessageProcessor.ProcessMessage(manifestRejectMessage);
			AssertEquals(MessageStatusCodeList.Codes.Error, manifest.AMA_MessageStatus);

			manifest.AMA_MessageStatus = "";
			manifestRejectMessage = CreateManifestResponseMessage(manifest.AMA_JobReference, "", "000", "970", "000");
			uemMessageProcessor.ProcessMessage(manifestRejectMessage);
			AssertEquals(MessageStatusCodeList.Codes.Error, manifest.AMA_MessageStatus);

			manifest.AMA_MessageStatus = "";
			manifestRejectMessage = CreateManifestResponseMessage(manifest.AMA_JobReference, "", "000", "000", "970");
			uemMessageProcessor.ProcessMessage(manifestRejectMessage);
			AssertEquals(MessageStatusCodeList.Codes.Error, manifest.AMA_MessageStatus);

			manifest.AMA_MessageStatus = "";
			var manifestAcceptMessage = CreateManifestResponseMessage(manifest.AMA_JobReference, "", "000", "AAA", "BBB");
			uemMessageProcessor.ProcessMessage(manifestAcceptMessage);
			AssertEquals(MessageStatusCodeList.Codes.Registered, manifest.AMA_MessageStatus);

			manifest.AMA_MessageStatus = "";
			manifestAcceptMessage = CreateManifestResponseMessage(manifest.AMA_JobReference, "", "AAA", "000", "BBB");
			uemMessageProcessor.ProcessMessage(manifestAcceptMessage);
			AssertEquals(MessageStatusCodeList.Codes.Registered, manifest.AMA_MessageStatus);

			manifest.AMA_MessageStatus = "";
			manifestAcceptMessage = CreateManifestResponseMessage(manifest.AMA_JobReference, "", "AAA", "BBB", "000");
			uemMessageProcessor.ProcessMessage(manifestAcceptMessage);
			AssertEquals(MessageStatusCodeList.Codes.Registered, manifest.AMA_MessageStatus);

			var bill = manifest.Bills.AddNew();
			var sentMessage = Factory.New<UEMEDIMessage>();
			sentMessage.EM_ApplicationCode = ApplicationCodeList.Codes.USExportManifest;
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_LinkTable = AsycudaBillSchema.Constants.TableName;
			sentMessage.EM_MessageType = MessageTypeList.Codes.ExportManifestSubmission;
			sentMessage.EM_Status = EDIMessage.Status.Sent;
			sentMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			bill.Messages.Add(sentMessage);
			Factory.Save();

			var messageNum = sentMessage.EM_MessageNum;
			manifest.AMA_MessageStatus = "";
			AssertEquals(string.Empty, manifest.AMA_MessageStatus);
			var receivedMessage = CreateManifestResponseMessage("", messageNum, "AAA", "BBB", "CCC");
			uemMessageProcessor.ProcessMessage(receivedMessage);
			AssertEquals(string.Empty, manifest.AMA_MessageStatus);

			manifest.AMA_MessageStatus = "";
			receivedMessage = CreateManifestResponseMessage("", messageNum, "970", "000", "000");
			uemMessageProcessor.ProcessMessage(receivedMessage);
			AssertEquals(MessageStatusCodeList.Codes.Error, manifest.AMA_MessageStatus);

			manifest.AMA_MessageStatus = "";
			receivedMessage = CreateManifestResponseMessage("", messageNum, "000", "970", "000");
			uemMessageProcessor.ProcessMessage(receivedMessage);
			AssertEquals(MessageStatusCodeList.Codes.Error, manifest.AMA_MessageStatus);

			manifest.AMA_MessageStatus = "";
			receivedMessage = CreateManifestResponseMessage("", messageNum, "000", "000", "970");
			uemMessageProcessor.ProcessMessage(receivedMessage);
			AssertEquals(MessageStatusCodeList.Codes.Error, manifest.AMA_MessageStatus);

			manifest.AMA_MessageStatus = "";
			receivedMessage = CreateManifestResponseMessage("", messageNum, "000", "AAA", "BBB");
			uemMessageProcessor.ProcessMessage(receivedMessage);
			AssertEquals(MessageStatusCodeList.Codes.Registered, manifest.AMA_MessageStatus);

			manifest.AMA_MessageStatus = "";
			receivedMessage = CreateManifestResponseMessage("", messageNum, "AAA", "000", "BBB");
			uemMessageProcessor.ProcessMessage(receivedMessage);
			AssertEquals(MessageStatusCodeList.Codes.Registered, manifest.AMA_MessageStatus);

			manifest.AMA_MessageStatus = "";
			receivedMessage = CreateManifestResponseMessage("", messageNum, "AAA", "BBB", "000");
			uemMessageProcessor.ProcessMessage(receivedMessage);
			AssertEquals(MessageStatusCodeList.Codes.Registered, manifest.AMA_MessageStatus);
		}

		UEMEDIMessage CreateManifestResponseMessage(string messageControlNumber, string messageReferenceNumber, string responseCode1, string responseCode2, string responseCode3)
		{
			var template =
$@"<?xml version=""1.0"" encoding=""utf-16""?>
<CBPManifestMessage xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://manifest.cbp.dhs.gov/shared/model"">
	<Version />
	<Filing>
		<MessageControlNumber>
		<Value>{messageControlNumber}</Value>
		</MessageControlNumber>
		<MessageReferenceNumber>
		<Value>{messageReferenceNumber}</Value>
		</MessageReferenceNumber>
	</Filing>
	<Conveyance>
		<BOLInfoList>
			<ResponseMessage>
				<ResponseCode>{responseCode3}</ResponseCode>
			</ResponseMessage>
		</BOLInfoList>
		<ResponseMessage>
			<ResponseCode>{responseCode2}</ResponseCode>
		</ResponseMessage>
	</Conveyance>
	<ResponseMessage>
		<ResponseCode>{responseCode1}</ResponseCode>
	</ResponseMessage>
</CBPManifestMessage>";

			var message = Factory.New<UEMEDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.ExportManifestResponse;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.USExportManifest;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageData = new ZBlob(MessageEncoding.UTF8WithoutBOM.GetBytes(template));
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();
			uemMessageProcessor = new UEMMessageProcessor(new LoggingInformation());
		}

		UEMMessageProcessor uemMessageProcessor;
	}
}
