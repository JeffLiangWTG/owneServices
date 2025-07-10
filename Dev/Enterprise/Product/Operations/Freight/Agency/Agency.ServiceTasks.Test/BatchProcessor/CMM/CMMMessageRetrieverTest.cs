using System;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	sealed class CMMMessageRetrieverTest : ShippingManagerMessageRetrieverTest
	{
		public void TestGarbage()
		{
			EDIMessage[] messages;
			MailItem item = CreateIncomingMailItem("EDIProd@csxwt.com.au", "EDI CODECO Interchange", "Random Crap", MailFilterCodes.CMMMessage);
			item.AddRecipientForUserCommunication("bob@freadnet.org", MailRecipient.RecipientTypes.TO);
			Factory.Save();
			AssertEquals("precondition: no messages", 0, LoadMessages().Length);
			NotificationBuffer buffer = new NotificationBuffer();
			IProcessor processor = new CMMMessageRetriever();
			processor.Process(buffer);
			AssertEmailProcessed("Mail item should be processed.", item);
			messages = LoadMessages();
			AssertEquals("No message to extract from the email.", 0, LoadMessages().Length);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExtractFromMailItem_CODECO()
		{
			#region Message Texts
			const string messageText = "UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA30043048-1+9'" + "TDT+20+0012E+1++Qua:172:184+++:146'" + "NAD+MS+CCPFI:160:184'" + "NAD+CF+Qua:160:184'" + "GID+1'" + "EQD+CN+TRLU3924710+2210:102:5+++4'" + "DTM+7:200902020909:203'" + "LOC+165+AUBNE:139:6+CCPFI'" + "LOC+8+AUBNE'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + "CNT+16:1'" + "UNT+14+1'" + "";
			const string CODECOCodes = "Application Code: CMG\r\n" + "Message Type: CMG\r\n" + "Message Sub Type: XXX\r\n" + "";
			#endregion
			var buffer = new NotificationBuffer();
			var processor = new CMMMessageRetriever();
			// Message 1
			var item = CreateIncomingMailItem(TestFileHelper.CODECO.GetInboundEmail(), MailFilterCodes.CMMMessage);
			Factory.Save();
			AssertMessages("precondition:\r\n" + buffer.AsString, Array.Empty<string>(), LoadMessages());
			processor.Process(buffer);
			AssertEmailProcessed(item);
			var messages = LoadMessages();
			AssertMessages("Expecting a single message\r\n" + buffer.AsString, new string[] { CODECOCodes }, messages);
			AssertMultilineASCIIEquals("Expected message content", messageText.Replace("'", "'\n"), messages[0].EM_MessageText.Replace("'", "'\n"));
			messages[0].DeleteFromTest();
			// No Messages
			processor.Process(buffer);
			AssertMessages("No new emails to process.", Array.Empty<string>(), LoadMessages());
		}

		public void TestExtractFromMailItem_COARRI()
		{
			#region Message Texts
			const string messageText1 = "UNH+02632053-001+COARRI:D:95B:UN:ITG12'" + "BGM+270+02632053-001+9'" + "TDT+20+026N+1++APL:172:87+++8802909:146:11:APL ZIRCON'" + "LOC+9+AUADL:139:6+DPIADL'" + "DTM+132:200804040000:203'" + "DTM+133:200804040000:203'" + "NAD+CA+CSC:160:184'" + "NAD+MS+CSXWTADL:ZZZ'" + "EQD+CN+GESU3541003+22G0:102:5++2+5'" + "RFF+BN:CSG121223'" + "TMD+3'" + "DTM+203:200804042224:203'" + "LOC+11+SGSIN:139:6'" + "LOC+7+MYPEN:139:6'" + "LOC+147+410502::5'" + "MEA+AAE+G+KGM:25700'" + "SEL+307856+SH'" + "FTX+AAA+++GRAIN'" + "TDT+10++3+31+MAK:172:87+++UWK676:146:ZZZ'" + "CNT+16:1'" + "UNT+21+02632053-001'" + "";
			const string interchangeText1 = "UNA:+.? '" + "UNB+UNOA:1+CSXWTADL:ZZ+CSC:ZZ+080404:2315+02632053++COARRI'" + messageText1 + "UNZ+1+02632053'" + "";
			const string COARRICodes = "Application Code: CMG\r\n" + "Message Type: CMG\r\n" + "Message Sub Type: XXX\r\n" + "";
			#endregion
			MailItem item;
			EDIMessage[] messages;
			NotificationBuffer buffer = new NotificationBuffer();
			IProcessor processor = new CMMMessageRetriever();
			// Message 1
			item = CreateIncomingMailItem("EDIProd@csxwt.com.au", "COMPANY EDI Interchange (MSG ID: 02950998 / Customer: CSC / Type: COARRI)", interchangeText1, MailFilterCodes.CMMMessage);
			item.AddRecipientForUserCommunication("bob@freadnet.org", MailRecipient.RecipientTypes.TO);
			Factory.Save();
			AssertMessages("precondition:", Array.Empty<string>(), LoadMessages());
			processor.Process(buffer);
			AssertEmailProcessed(item);
			messages = LoadMessages();
			AssertMessages("Expecting a single message", new string[] { COARRICodes }, messages);
			AssertMultilineASCIIEquals("Expected message content", messageText1.Replace("'", "'\n"), messages[0].EM_MessageText.Replace("'", "'\n"));
			messages[0].DeleteFromTest();
			// No Messages
			processor.Process(buffer);
			AssertMessages("No new emails to process.", Array.Empty<string>(), LoadMessages());
		}

		[TestDate(2010, 10, 07, 08, 27, 00)]
		public void TestExtractFromMailItem_NoSenderOrInterchangeNum()
		{
			#region Message Texts
			const string interchangeText = "UNB+UNOA:1+:ZZ+MSCU:ZZ+      :    +'" + "UNZ+0+'" + "";
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Error extracting interchange from email.\r\n" + "TO:\r\n" + "  err@freadnet.org\r\n" + "";
			const string expectedEmailBody = "Interchange text appears malformed.\r\n" + "\r\n" + "Subject: COMPANY EDI Interchange (MSG ID: 02950998 / Customer: CSC / Type: COARRI)\r\n" + "From: EDIProd@csxwt.com.au\r\n" + "Date: 07-Oct-10 08:27\r\n" + "\r\n" + "Detail: no sender or interchange number\r\n" + "\r\n" + "\r\n" + "You have received this email because you are a member of the staff group defined at System Registry: Notification -> Liner & Agency -> Container Management Messaging -> Group To Send Error Emails To.\r\n" + "";
			#endregion
			MailItem item;
			EDIMessage[] messages;
			NotificationBuffer buffer = new NotificationBuffer();
			IProcessor processor = new CMMMessageRetriever();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			// Message 1
			item = CreateIncomingMailItem("EDIProd@csxwt.com.au", "COMPANY EDI Interchange (MSG ID: 02950998 / Customer: CSC / Type: COARRI)", interchangeText, MailFilterCodes.CMMMessage);
			item.AddRecipientForUserCommunication("bob@freadnet.org", MailRecipient.RecipientTypes.TO);
			Factory.Save();
			AssertMessages("precondition:", Array.Empty<string>(), LoadMessages());
			processor.Process(buffer);
			AssertEmailProcessed(item);
			messages = LoadMessages();
			AssertMessages("Expecting no messages", Array.Empty<string>(), messages);
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			EmailDef email = FindEmail("err@freadnet.org", "Error extracting interchange from email.");
			AssertMultilineASCIIEquals("expected body", expectedEmailBody, email.Body);
		}

		[TestDate(2010, 10, 07, 08, 27, 00)]
		public void TestExtractFromMailItem_Corrupt()
		{
			#region Message Texts
			const string interchangeText = "UNA:+.? '" + "UNB+UNOA:1:X+CSXWTADL:ZZ+CSC:ZZ+080404:2315+02632053++COARRI'" + "UNH+02632053-001+COARRI:D:95B:UN:ITG12'" + "BGM+270+02632053-001+9'" + "LOC+9+AUADL:139:6+DPIADL'" + "DTM+132:200804040000:203'" + "EQD+CN+GESU3541003+22G0:102:5++2+5'" + "CNT+16:1'" + "UNT+7+02632053-001'" + "UNZ+1+02632053'" + "";
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Error extracting interchange from email.\r\n" + "TO:\r\n" + "  err@freadnet.org\r\n" + "";
			const string expectedEmailBody = "Interchange text appears malformed.\r\n" + "\r\n" + "Subject: COMPANY EDI Interchange (MSG ID: 02950998 / Customer: CSC / Type: COARRI)\r\n" + "From: EDIProd@csxwt.com.au\r\n" + "Date: 07-Oct-10 08:27\r\n" + "\r\n" + "Detail: Expected no more than 2 values, but was 3 in SyntaxIdentifierElements\r\n" + "\r\n" + "\r\n" + "You have received this email because you are a member of the staff group defined at System Registry: Notification -> Liner & Agency -> Container Management Messaging -> Group To Send Error Emails To.\r\n" + "";
			#endregion
			MailItem item;
			EDIMessage[] messages;
			NotificationBuffer buffer = new NotificationBuffer();
			IProcessor processor = new CMMMessageRetriever();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			// Message 1
			item = CreateIncomingMailItem("EDIProd@csxwt.com.au", "COMPANY EDI Interchange (MSG ID: 02950998 / Customer: CSC / Type: COARRI)", interchangeText, MailFilterCodes.CMMMessage);
			item.AddRecipientForUserCommunication("bob@freadnet.org", MailRecipient.RecipientTypes.TO);
			Factory.Save();
			AssertMessages("precondition:", Array.Empty<string>(), LoadMessages());
			processor.Process(buffer);
			AssertEmailProcessed(item);
			messages = LoadMessages();
			AssertMessages("Expecting no messages", Array.Empty<string>(), messages);
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			EmailDef email = FindEmail("err@freadnet.org", "Error extracting interchange from email.");
			AssertMultilineASCIIEquals("expected body", expectedEmailBody, email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "interchange.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			AttachmentDef attachment = FindAttachment(email, "interchange.edi");
			AssertMultilineASCIIEquals("attachment content", interchangeText, Encoding.UTF8.GetString(attachment.Data));
			// No Messages
			processor.Process(buffer);
			AssertMessages("No new emails to process.", Array.Empty<string>(), LoadMessages());
		}

		public void TestExtractMultipleMessages()
		{
			#region Message Texts
			const string messageText1 = "UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA30043048-1+9'" + "TDT+20+0012E+1++Qua:172:184+++:146'" + "NAD+MS+CCPFI:160:184'" + "NAD+CF+Qua:160:184'" + "GID+1'" + "EQD+CN+TRLU3924710+2210:102:5+++4'" + "DTM+7:200902020909:203'" + "LOC+165+AUBNE:139:6+CCPFI'" + "LOC+8+AUBNE'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + "CNT+16:1'" + "UNT+14+1'" + "";
			const string interchangeText1 = "UNA:+.? '" + "UNB+UNOA:1+CSXWTADL:ZZ+CSC:ZZ+080404:2315+02632053++CODECO'" + messageText1 + "UNZ+1+02632053'" + "";
			const string messageText2 = "UNH+02632053-001+COARRI:D:95B:UN:ITG12'" + "BGM+270+02632053-001+9'" + "TDT+20+026N+1++APL:172:87+++8802909:146:11:APL ZIRCON'" + "LOC+9+AUADL:139:6+DPIADL'" + "DTM+132:200804040000:203'" + "DTM+133:200804040000:203'" + "NAD+CA+CSC:160:184'" + "NAD+MS+CSXWTADL:ZZZ'" + "EQD+CN+GESU3541003+22G0:102:5++2+5'" + "RFF+BN:CSG121223'" + "TMD+3'" + "DTM+203:200804042224:203'" + "LOC+11+SGSIN:139:6'" + "LOC+7+MYPEN:139:6'" + "LOC+147+410502::5'" + "MEA+AAE+G+KGM:25700'" + "SEL+307856+SH'" + "FTX+AAA+++GRAIN'" + "TDT+10++3+31+MAK:172:87+++UWK676:146:ZZZ'" + "CNT+16:1'" + "UNT+21+02632053-001'" + "";
			const string interchangeText2 = "UNA:+.? '" + "UNB+UNOA:1+CSXWTADL:ZZ+CSC:ZZ+080404:2315+02632054++COARRI'" + messageText2 + "UNZ+1+02632054'" + "";
			const string Codes = "Application Code: CMG\r\n" + "Message Type: CMG\r\n" + "Message Sub Type: XXX\r\n" + "";
			#endregion
			MailItem item;
			EDIMessage[] messages;
			NotificationBuffer buffer = new NotificationBuffer();
			IProcessor processor = new CMMMessageRetriever();
			// Message 1
			item = CreateIncomingMailItem("EDIProd@csxwt.com.au", "CODECO COARRI)", "", MailFilterCodes.CMMMessage);
			item.AddRecipientForUserCommunication("bob@freadnet.org", MailRecipient.RecipientTypes.TO);
			CreateAttachment(item, "bob1.edi", Encoding.UTF8.GetBytes(interchangeText1));
			CreateAttachment(item, "bob2.edi", Encoding.UTF8.GetBytes(interchangeText2));
			Factory.Save();
			AssertMessages("precondition:", Array.Empty<string>(), LoadMessages());
			processor.Process(buffer);
			AssertEmailProcessed(item);
			messages = LoadMessages();
			AssertMessages("Expecting two messages", new string[] { Codes, Codes }, messages);
			EDIMessage codecoMessage;
			EDIMessage coarriMessage;
			if (messages[0].EM_MessageText.StartsWith("UNH+1+CODECO"))
			{
				codecoMessage = messages[0];
				coarriMessage = messages[1];
			}
			else
			{
				codecoMessage = messages[1];
				coarriMessage = messages[0];
			}

			AssertMultilineASCIIEquals("Expected message content", messageText1.Replace("'", "'\n"), codecoMessage.EM_MessageText.Replace("'", "'\n"));
			AssertMultilineASCIIEquals("Expected message content", messageText2.Replace("'", "'\n"), coarriMessage.EM_MessageText.Replace("'", "'\n"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExtractFromMailItemDependsOnRegistry()
		{
			AgencyRegistry.Instance.AllowDirectCODECOCOARRICMMMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var buffer = new NotificationBuffer();
			var processor = new CMMMessageRetriever();
			var mailItem = CreateIncomingMailItem(TestFileHelper.CODECO.GetInboundEmail(), MailFilterCodes.CMMMessage);
			Factory.Save();
			processor.Process(buffer);
			var loadedMessages = LoadMessages();
			AssertEmailFailed(mailItem);
			AssertEquals("No message to extract from the email.", 0, loadedMessages.Length);
		}

		protected override void SetUp()
		{
			base.SetUp();
			AgencyRegistry.Instance.AllowDirectCODECOCOARRICMMMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();
		}
	}
}
