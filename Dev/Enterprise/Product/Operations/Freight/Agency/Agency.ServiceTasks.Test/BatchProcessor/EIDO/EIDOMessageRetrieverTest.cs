using System;
using CargoWise.EntityFramework;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	internal class EIDOMessageRetrieverTest : ShippingManagerMessageRetrieverTest
	{
		public void TestGarbage()
		{
			EDIMessage[] messages;
			MailItem item = CreateIncomingMailItem("stop20@test.1stop.biz", "1-Stop E-IDO Response Rejected 6", "Random Crap", MailFilterCodes.EIDOMessageRetriever);
			item.AddRecipientForUserCommunication("bob@freadnet.org", MailRecipient.RecipientTypes.TO);
			Factory.Save();
			AssertEquals("precondition: no messages", 0, LoadMessages().Length);
			NotificationBuffer buffer = new NotificationBuffer();
			IProcessor processor = new EIDOMessageRetriever();
			processor.Process(buffer);
			AssertEmailProcessed("Mail item should be processed.", item);
			messages = LoadMessages();
			AssertEquals("No message to extract from the email.", 0, LoadMessages().Length);
		}

		public void TestExtractFromMailItem_EIDO_APERAK()
		{
			#region message texts
			const string messageText1 = "UNH+1+APERAK:D:99A:UN:ANZ23'" + "BGM+7+RESP42265+9+RE'" + "DTM+137:20080220141119:204'" + "DOC+640+6'" + "DTM+137:20080220124400:204'" + "NAD+MS+1STOP'" + "NAD+MR+GSL'" + "ERC+COM025'" + "FTX+AAO+++RECEIVER CODE UNKNOWN'" + "UNT+10+1'" + "";
			const string interchangeText1 = "UNA:+.? '" + "UNB+UNOC:3+1STOP+GSL:ZZ+080220:1411+2051769'" + messageText1 + "UNZ+1+2051769'" + "";
			const string messageText2 = "UNH+1+APERAK:D:99A:UN:ANZ23'" + "BGM+7+12097+9+AP'" + "DTM+137:20080221093111:204'" + "DOC+640+7'" + "DTM+137:20080220181000:204'" + "NAD+MS+ASLPB'" + "NAD+MR+GSL'" + "ERC+COM0000'" + "FTX+AAO+++Message received without error'" + "RFF+EQD:FOOU4100012'" + "UNT+11+1'" + "";
			const string interchangeText2 = "UNA:+.? '" + "UNB+UNOC:3+1STOP+GSL:ZZ+080221:0931+2051834'" + messageText2 + "UNZ+1+2051834'" + "";
			const string messageText3 = "UNH+1+APERAK:D:99A:UN:ANZ23'" + "BGM+7+13485+9+AP'" + "DTM+137:20080526172522:204'" + "DOC+640+2'" + "DTM+137:20080521141000:204'" + "NAD+MS+ASLPB'" + "NAD+MR+SNL'" + "ERC+COM0000'" + "FTX+AAO+++Message received without error'" + "RFF+EQD:SNBU2000009'" + "UNT+11+1'" + "";
			const string attachment3 = "VU5BOisuPyAnVU5CK1VOT0M6MytBQUEzNDlLOjpGRk0zNjdLK1NOTDpaWiswODA1MjY6MTcy" + "NSsyMDgwNTYwJ1VOSCsxK0FQRVJBSzpEOjk5QTpVTjpBTloyMydCR00rNysxMzQ4NSs5K0FQ" + "J0RUTSsxMzc6MjAwODA1MjYxNzI1MjI6MjA0J0RPQys2NDArMidEVE0rMTM3OjIwMDgwNTIx" + "MTQxMDAwOjIwNCdOQUQrTVMrQVNMUEInTkFEK01SK1NOTCdFUkMrQ09NMDAwMCdGVFgrQUFP" + "KysrTWVzc2FnZSByZWNlaXZlZCB3aXRob3V0IGVycm9yJ1JGRitFUUQ6U05CVTIwMDAwMDkn" + "VU5UKzExKzEnVU5aKzErMjA4MDU2MCc=";
			const string EIDOCodes = "Application Code: EDO\r\n" + "Message Type: EDO\r\n" + "Message Sub Type: XXX\r\n" + "";
			#endregion
			MailItem item;
			EDIMessage[] messages;
			NotificationBuffer buffer = new NotificationBuffer();
			IProcessor processor = new EIDOMessageRetriever();
			// Message 1
			item = CreateIncomingMailItem("stop20@test.1stop.biz", "1-Stop EIDO Response Rejected 6", interchangeText1, MailFilterCodes.EIDOMessageRetriever);
			item.AddRecipientForUserCommunication("bob@freadnet.org", MailRecipient.RecipientTypes.TO);
			Factory.Save();
			messages = LoadMessages();
			AssertMessages("precondition: (1)", Array.Empty<string>(), LoadMessages());
			processor.Process(buffer);
			AssertEmailProcessed("Mail item should be processed. (1)", item);
			messages = LoadMessages();
			AssertMessages("Expecting a single message (1)", new string[] { EIDOCodes }, messages);
			AssertMultilineASCIIEquals("Expected message content (1)", messageText1.Replace("'", "'\n"), messages[0].EM_MessageText.Replace("'", "'\n"));
			messages[0].DeleteFromTest();
			// Message 2
			item = CreateIncomingMailItem("stop20@test.1stop.biz", "1-Stop E-IDO Response Accepted 7/FOOU4100012", interchangeText2, MailFilterCodes.EIDOMessageRetriever);
			item.AddRecipientForUserCommunication("bob@freadnet.org", MailRecipient.RecipientTypes.TO);
			Factory.Save();
			messages = LoadMessages();
			AssertMessages("precondition: (2)", Array.Empty<string>(), LoadMessages());
			processor.Process(buffer);
			AssertEmailProcessed("Mail item should be processed. (2)", item);
			messages = LoadMessages();
			AssertMessages("Expecting a single message (2)", new string[] { EIDOCodes }, messages);
			AssertMultilineASCIIEquals("Expected message content (2)", messageText2.Replace("'", "'\n"), messages[0].EM_MessageText.Replace("'", "'\n"));
			messages[0].DeleteFromTest();
			// Message 3
			item = CreateIncomingMailItem("stop20@test.1stop.biz", "E-IDO APERAK", "Random Text", MailFilterCodes.EIDOMessageRetriever);
			CreateAttachment(item, "cmrr080526172522315.dat", Convert.FromBase64String(attachment3));
			item.AddRecipientForUserCommunication("bob@freadnet.org", MailRecipient.RecipientTypes.TO);
			Factory.Save();
			messages = LoadMessages();
			AssertMessages("precondition: (3)", Array.Empty<string>(), LoadMessages());
			processor.Process(buffer);
			AssertEmailProcessed("Mail item should be processed. (3)", item);
			messages = LoadMessages();
			AssertMessages("Expecting a single message (3)", new string[] { EIDOCodes }, messages);
			AssertMultilineASCIIEquals("Expected message content (3)", messageText3.Replace("'", "'\n"), messages[0].EM_MessageText.Replace("'", "'\n"));
			messages[0].DeleteFromTest();
			Factory.Save();
			// No Messages
			processor.Process(buffer);
			AssertMessages("No new emails to process.", Array.Empty<string>(), LoadMessages());
		}
	}
}
