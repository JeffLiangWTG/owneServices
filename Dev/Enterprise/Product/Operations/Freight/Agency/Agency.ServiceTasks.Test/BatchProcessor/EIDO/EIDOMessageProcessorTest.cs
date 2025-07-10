using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	internal class EIDOMessageProcessorTest : BaseAgencyTest
	{
		public void TestProcessSuccess_Accepted()
		{
			string messageText = "UNH+12345+APERAK:D:99A:UN:ANZ23'" + "BGM+7+001+9+AP'" + "DTM+137:20060425093000:204'" + "DOC+640+{0}'" + "DTM+137:20040425091500:204'" + "NAD+MS+1-STOP'" + "ERC+COM000'" + "FTX+AAO+++MESSAGE RECEIVED WITHOUT ERROR'" + "RFF+EQD:FAKE4100011'" + "UNT+10+12345'" + "";
			SetEmailAddresses("ack@freadnet.org", "err@freadnet.org");
			EIDOMessage message = CreateOutgoingMessage("S00000100", "BOL1", "FAKE4100011");
			EIDOMessage response = CreateIncommingMessage(string.Format(messageText, message.EM_MessageNum));
			BillOfLadingContainer container = LoadContainer("S00000100", "FAKE4100011");
			AssertNoEvents("precondition:", container, Events.MessageAccepted);
			AssertNoEvents("precondition:", container, Events.MessageRejected);
			LoggingInformation logger = new LoggingInformation();
			EIDOMessageProcessor processor = new EIDOMessageProcessor(logger);
			processor.ProcessMessage(response);
			AssertHasEvent("Should have added the event", container, Events.MessageAccepted, string.Format("E-IDO Interchange Number {0}|DEP=1-stop", message.EM_InterchangeNumber));
			AssertNoEvents("Should not have added the other event", container, Events.MessageRejected);
			AssertEquals("EM_LinkUniqueID", container.PK, response.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", JobContainerSchema.Constants.TableName, response.EM_LinkTable);
			AssertEquals("message EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("response EM_Status", EDIMessage.Status.Recognised, response.EM_Status);
			string expectedEmailDef = "FROM:\r\n" + "  Default@edi.com.au\r\n" + "SUBJECT:\r\n" + "  Accepted E-IDO for container FAKE4100011 on S00000100\r\n" + "CC:\r\n" + "  ack@freadnet.org\r\n" + "";
			string expectedEmailBody = "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			EmailDef email = FindEmail((p) => p.CCRecipients.Contains("ack@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", expectedEmailBody, email.Body);
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
		}

		public void TestProcessSuccess_Received()
		{
			string messageText = "UNH+12345+APERAK:D:99A:UN:ANZ23'" + "BGM+7+001+9+CA'" + "DTM+137:20060425093000:204'" + "DOC+640+{0}'" + "DTM+137:20040425091500:204'" + "NAD+MS+1-STOP'" + "ERC+COM019'" + "FTX+AAO+++E-IDO RECEIPT ACKNOWLEDGEMENT - NO VALIDATION OF E-IDO DETAILS'" + "RFF+EQD:KKFU1635133'" + "UNT+10+12345'" + "";
			SetEmailAddresses("ack@freadnet.org", "err@freadnet.org");
			EIDOMessage message = CreateOutgoingMessage("S00000100", "BOL1", "FAKE4100011");
			EIDOMessage response = CreateIncommingMessage(string.Format(messageText, message.EM_MessageNum));
			BillOfLadingContainer container = LoadContainer("S00000100", "FAKE4100011");
			AssertNoEvents("precondition:", container, Events.MessageAccepted);
			AssertNoEvents("precondition:", container, Events.MessageRejected);
			LoggingInformation logger = new LoggingInformation();
			EIDOMessageProcessor processor = new EIDOMessageProcessor(logger);
			processor.ProcessMessage(response);
			AssertHasEvent("Should have added the event", container, Events.InterchangeReceiptAcknowledged, string.Format("E-IDO Interchange Number {0}|DEP=1-stop", message.EM_InterchangeNumber));
			AssertNoEvents("Should not have added the other event", container, Events.MessageRejected);
			AssertEquals("EM_LinkUniqueID", container.PK, response.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", JobContainerSchema.Constants.TableName, response.EM_LinkTable);
			AssertEquals("message EM_Status", EDIMessage.Status.Acknowledged, message.EM_Status);
			AssertEquals("response EM_Status", EDIMessage.Status.Recognised, response.EM_Status);
			string expectedEmailDef = "FROM:\r\n" + "  Default@edi.com.au\r\n" + "SUBJECT:\r\n" + "  Received E-IDO for container FAKE4100011 on S00000100\r\n" + "CC:\r\n" + "  ack@freadnet.org\r\n" + "";
			string expectedEmailBody = "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			EmailDef email = FindEmail((p) => p.CCRecipients.Contains("ack@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", expectedEmailBody, email.Body);
			AssertContainsExactElementsInAnyOrder("not expecting any attachments", Array.Empty<string>(), ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
		}

		public void TestProcessSuccess_Rejected()
		{
			string messageText = "UNH+12345+APERAK:D:99A:UN:ANZ23'" + "BGM+7+001+9+RE'" + "DTM+137:20060425093000:204'" + "DOC+640+{0}'" + "DTM+137:20040425091500:204'" + "NAD+MS+1-STOP'" + "NAD+MR+EAGLE'" + "ERC+COM016'" + "FTX+AAO+++INVALID MESSAGE FUNCTION'" + "ERC+COM017'" + "FTX+AAO+++PASSWORD NOT RECOGNISED FOR LINE OPERATOR'" + "RFF+EQD:KKFU1635133'" + "UNT+12+12345'" + "";
			SetEmailAddresses("ack@freadnet.org", "err@freadnet.org");
			EIDOMessage message = CreateOutgoingMessage("S00000100", "BOL1", "FAKE4100011");
			EIDOMessage response = CreateIncommingMessage(string.Format(messageText, message.EM_MessageNum));
			BillOfLadingContainer container = LoadContainer("S00000100", "FAKE4100011");
			AssertNoEvents("precondition:", container, Events.MessageAccepted);
			AssertNoEvents("precondition:", container, Events.MessageRejected);
			LoggingInformation logger = new LoggingInformation();
			EIDOMessageProcessor processor = new EIDOMessageProcessor(logger);
			processor.ProcessMessage(response);
			AssertHasEvent("Should have added the event", container, Events.MessageRejected, string.Format("E-IDO Interchange Number {0}|DEP=1-stop", message.EM_InterchangeNumber));
			AssertNoEvents("Should not have added the other event", container, Events.MessageAccepted);
			AssertEquals("EM_LinkUniqueID", container.PK, response.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", JobContainerSchema.Constants.TableName, response.EM_LinkTable);
			AssertEquals("message EM_Status", EDIMessage.Status.Rejected, message.EM_Status);
			AssertEquals("response EM_Status", EDIMessage.Status.Recognised, response.EM_Status);
			string expectedEmailDef = "FROM:\r\n" + "  Default@edi.com.au\r\n" + "SUBJECT:\r\n" + "  Rejected E-IDO for container FAKE4100011 on S00000100\r\n" + "CC:\r\n" + "  err@freadnet.org\r\n" + "";
			string expectedEmailBody = "Received a 'Rejected' response to E-IDO message '{0}' (2004-04-25 09:15)\r\n" + "\r\n" + "COM016: INVALID MESSAGE FUNCTION\r\n" + "\r\n" + "COM017: PASSWORD NOT RECOGNISED FOR LINE OPERATOR\r\n" + "Equipment: KKFU1635133\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			EmailDef email = FindEmail((p) => p.CCRecipients.Contains("err@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", string.Format(expectedEmailBody, message.EM_MessageNum), email.Body);
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
		}

		public void TestProcessSuccess_AcceptedUpdateReceived()
		{
			string messageText = "UNH+12345+APERAK:D:99A:UN:ANZ23'" + "BGM+7+001+9+AP'" + "DTM+137:20060425093000:204'" + "DOC+640+{0}'" + "DTM+137:20040425091500:204'" + "NAD+MS+1-STOP'" + "ERC+COM000'" + "FTX+AAO+++MESSAGE RECEIVED WITHOUT ERROR'" + "RFF+EQD:FAKE4100011'" + "UNT+10+12345'" + "";
			SetEmailAddresses("ack@freadnet.org", "err@freadnet.org");
			EIDOMessage message = CreateOutgoingMessage("S00000100", "BOL1", "FAKE4100011");
			message.EM_Status = EDIMessage.Status.Acknowledged;
			EIDOMessage response = CreateIncommingMessage(string.Format(messageText, message.EM_MessageNum));
			BillOfLadingContainer container = LoadContainer("S00000100", "FAKE4100011");
			LoggingInformation logger = new LoggingInformation();
			EIDOMessageProcessor processor = new EIDOMessageProcessor(logger);
			processor.ProcessMessage(response);
			AssertEquals("EM_LinkUniqueID", container.PK, response.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", JobContainerSchema.Constants.TableName, response.EM_LinkTable);
			AssertEquals("message EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("response EM_Status", EDIMessage.Status.Recognised, response.EM_Status);
		}

		public void TestProcessSuccess_ReceivedNotUpdateAccepted()
		{
			string messageText = "UNH+12345+APERAK:D:99A:UN:ANZ23'" + "BGM+7+001+9+CA'" + "DTM+137:20060425093000:204'" + "DOC+640+{0}'" + "DTM+137:20040425091500:204'" + "NAD+MS+1-STOP'" + "ERC+COM019'" + "FTX+AAO+++E-IDO RECEIPT ACKNOWLEDGEMENT - NO VALIDATION OF E-IDO DETAILS'" + "RFF+EQD:KKFU1635133'" + "UNT+10+12345'" + "";
			SetEmailAddresses("ack@freadnet.org", "err@freadnet.org");
			EIDOMessage message = CreateOutgoingMessage("S00000100", "BOL1", "FAKE4100011");
			message.EM_Status = EDIMessage.Status.Received;
			EIDOMessage response = CreateIncommingMessage(string.Format(messageText, message.EM_MessageNum));
			BillOfLadingContainer container = LoadContainer("S00000100", "FAKE4100011");
			LoggingInformation logger = new LoggingInformation();
			EIDOMessageProcessor processor = new EIDOMessageProcessor(logger);
			processor.ProcessMessage(response);
			AssertEquals("EM_LinkUniqueID", container.PK, response.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", JobContainerSchema.Constants.TableName, response.EM_LinkTable);
			AssertEquals("message EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("response EM_Status", EDIMessage.Status.Recognised, response.EM_Status);
		}

		public void TestGarbage()
		{
			string messageText = "Do you feel inadiquite?\r\n" + "Have you tried the little blue pill and failed?\r\n" + "Well now there is a little green pill for thoes who want to colour coordinate.\r\n" + "";
			SetEmailAddresses("ack@freadnet.org", "err@freadnet.org");
			EIDOMessage response = CreateIncommingMessage(messageText);
			LoggingInformation logger = new LoggingInformation();
			EIDOMessageProcessor processor = new EIDOMessageProcessor(logger);
			processor.ProcessMessage(response);
			AssertEquals("EM_LinkUniqueID", ZGuid.Empty, response.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", ZString.Empty, response.EM_LinkTable);
			AssertEquals("EM_Status", EDIMessage.Status.Failed, response.EM_Status);
			string expectedEmailDef = "FROM:\r\n" + "  Default@edi.com.au\r\n" + "SUBJECT:\r\n" + "  Error processing an E-IDO response.\r\n" + "CC:\r\n" + "  err@freadnet.org\r\n" + "";
			string expectedEmailBody = "Corrupted or Malformed D99A APERAK Response Message. Cannot Process.\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			EmailDef email = FindEmail((p) => p.CCRecipients.Contains("err@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", expectedEmailBody, email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "response.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			AttachmentDef attachment = FindAttachment(email, "response.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		public void TestUnExpectedResponse()
		{
			string messageText = "UNH+12345+APERAK:D:99A:UN:ANZ23'" + "BGM+7+001+9+AP'" + "DTM+137:20060425093000:204'" + "DOC+640+10000'" + "DTM+137:20040425091500:204'" + "NAD+MS+1-STOP'" + "ERC+COM000'" + "FTX+AAO+++MESSAGE RECEIVED WITHOUT ERROR'" + "RFF+EQD:FAKE4100011'" + "UNT+10+12345'" + "";
			SetEmailAddresses("ack@freadnet.org", "err@freadnet.org");
			EIDOMessage response = CreateIncommingMessage(messageText);
			LoggingInformation logger = new LoggingInformation();
			EIDOMessageProcessor processor = new EIDOMessageProcessor(logger);
			processor.ProcessMessage(response);
			AssertEquals("EM_LinkUniqueID", ZGuid.Empty, response.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", ZString.Empty, response.EM_LinkTable);
			AssertEquals("EM_Status", EDIMessage.Status.Failed, response.EM_Status);
			string expectedEmailDef = "FROM:\r\n" + "  Default@edi.com.au\r\n" + "SUBJECT:\r\n" + "  Error processing an E-IDO response.\r\n" + "CC:\r\n" + "  err@freadnet.org\r\n" + "";
			string expectedEmailBody = "Received a response for a message we have no record of sending.\r\n" + "\r\n" + "Received a 'Accepted' response to E-IDO message '10000' (2004-04-25 09:15)\r\n" + "\r\n" + "COM000: MESSAGE RECEIVED WITHOUT ERROR\r\n" + "Equipment: FAKE4100011" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			EmailDef email = FindEmail((p) => p.CCRecipients.Contains("err@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", expectedEmailBody, email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "response.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			AttachmentDef attachment = FindAttachment(email, "response.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		#region Implementation
		BillOfLading LoadShipment(string shipmentNumber)
		{
			return Factory.LoadFromNaturalKey<BillOfLading>(JobShipmentSchema.JS_UniqueConsignRef, shipmentNumber);
		}

		BillOfLadingContainer LoadContainer(BillOfLading shipment, string containerNumber)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobContainerSchema.JC_JS_FCLBookingOnlyLink, shipment.PK);
			filter.AddToFilter(JobContainerSchema.JC_ContainerNum, containerNumber);
			BillOfLadingContainer[] containers = Array.ConvertAll(shipment.RealContainers.Find(new ZQuery(JobContainerSchema.JC_ContainerNum, containerNumber)), (b) => (BillOfLadingContainer)b);
			return containers.Length > 0 ? containers[0] : null;
		}

		BillOfLadingContainer LoadContainer(string shipmentNumber, string containerNumber)
		{
			BillOfLading shipment = LoadShipment(shipmentNumber);
			return shipment == null ? null : LoadContainer(shipment, containerNumber);
		}

		EIDOMessage CreateOutgoingMessage(string shipmentNumber, string bol, string containerNumber)
		{
			BillOfLading shipment = Factory.New<BillOfLading>();
			shipment.JS_UniqueConsignRef = shipmentNumber;
			shipment.JS_HouseBill = bol;
			BillOfLadingContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = containerNumber;
			EIDOMessage message = EIDOMessage.New(container, EIDOMessageFunction.Original, "BEGIN+" + EIDOMessage.MessageNumberPlaceHolder + "+END");
			message.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();
			return message;
		}

		EIDOMessage CreateIncommingMessage(string messageText)
		{
			EIDOMessage response = Factory.New<EIDOMessage>();
			response.EM_Status = EIDOMessage.Status.Queued;
			response.EM_ReceiveTransmit = EIDOMessage.Direction.Receive;
			response.EM_MessageText = messageText;
			Factory.Save();
			return response;
		}

		void SetEmailAddresses(string acknowledgementEmail, string errorEmail)
		{
			GlbGroup acknowledgementGroup = acknowledgementEmail == null ? null : NewGroupWithStaffMemberAndEmailAddress("ack", "ack", acknowledgementEmail);
			GlbGroup errorGroup = errorEmail == null ? null : NewGroupWithStaffMemberAndEmailAddress("err", "err", errorEmail);
			Factory.Save();
			AgencyRegistry.Instance.EIDOAcknowledgementEmailGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, acknowledgementGroup == null ? Guid.Empty : acknowledgementGroup.PK.ToGuid());
			AgencyRegistry.Instance.EIDOErrorEmailGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, acknowledgementGroup == null ? Guid.Empty : errorGroup.PK.ToGuid());
		}

		GlbGroup NewGroupWithStaffMemberAndEmailAddress(string staffName, string groupName, string emailAddress)
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = Left(staffName, staff.GS_CodeInfo.MaxLength);
			staff.GS_FullName = staffName;
			staff.GS_LoginName = staffName;
			staff.GS_EmailAddress = emailAddress;
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = groupName;
			group.Staff.Add(staff);
			return group;
		}

		string Left(string value, int maxLength)
		{
			if (value == null)
			{
				return null;
			}
			else if (value.Length > maxLength)
			{
				return value.Substring(0, maxLength);
			}
			else
			{
				return value;
			}
		}
		#endregion
	}
}
