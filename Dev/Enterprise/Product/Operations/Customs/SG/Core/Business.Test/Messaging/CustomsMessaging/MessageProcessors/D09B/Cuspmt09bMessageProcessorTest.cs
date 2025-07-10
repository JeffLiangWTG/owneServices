using System;
using System.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Registry;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	sealed class Cuspmt09bMessageProcessorTest : SGMessageProcessorTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSetEntryAndEntryAuthorisationDate()
		{
			var messageText = LoadFromFile("EDI_OUTPMT_BKT.txt");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationSent, "XXXXXXXXE47T     201102140001", messageText);
			AssertEquals("OX6I000689A", EntryHeader.EntryNumber);
			AssertEquals(new ZDateTime(2011, 02, 14, 00, 15, 50), Declaration.JE_EntryAuthorisationDate);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCUSPMT_INPPMT()
		{
			var messageText = LoadFromFile("EDI_INPPMT_APS.txt");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationSent, "XXXXXXXXE01T     201102140001", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, EntryHeader.CH_Status);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCUSPMT_INPUPT()
		{
			var messageText = LoadFromFile("EDI_INPUPT_DES.txt");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.AmendmentSent, "XXXXXXXXE01T     201102146512", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.AmendmentPermitReceived, EntryHeader.CH_Status);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCUSPMT_IPTPMT()
		{
			var messageText = LoadFromFile("EDI_IPTPMT_DNG.txt");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationSent, "XXXXXXXXE47T     201102140204", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, EntryHeader.CH_Status);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCUSPMT_IPTUPT_Amendment()
		{
			var messageText = LoadFromFile("EDI_IPTUPT_DUT.txt");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.AmendmentSent, "XXXXXXXXE47T     201102140028", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.AmendmentPermitReceived, EntryHeader.CH_Status);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCUSPMT_IPTUPT_Refund()
		{
			var messageText = LoadFromFile("EDI_IPTUPT_DUT.txt");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.RefundSent, "XXXXXXXXE47T     201102140028", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.RefundPermitReceived, EntryHeader.CH_Status);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCUSPMT_OUTPMT()
		{
			var messageText = LoadFromFile("EDI_OUTPMT_BKT.txt");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationSent, "XXXXXXXXE47T     201102140001", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, EntryHeader.CH_Status);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCUSPMT_OUTUPT()
		{
			var messageText = LoadFromFile("EDI_OUTUPT_DRT.txt");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.AmendmentSent, "XXXXXXXXE47T     201102140004", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.AmendmentPermitReceived, EntryHeader.CH_Status);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCUSPMT_TNPPMT()
		{
			string messageText = LoadFromFile("EDI_TNPPMT_REM.txt");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationSent, "XXXXXXXXE01T     201102141002", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, EntryHeader.CH_Status);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCUSPMT_TNPUPT()
		{
			var messageText = LoadFromFile("EDI_TNPUPT_TTI.txt");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.AmendmentSent, "XXXXXXXXE01T     201102140013", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.AmendmentPermitReceived, EntryHeader.CH_Status);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCustomsClearanceInformationEmail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001013";
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentSent;
			var trxMessage = Factory.New<EDIMessage>();
			trxMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			trxMessage.EM_ApplicationReference = "XXXXXXXXE01T     201102140013";
			trxMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			trxMessage.EM_LinkedObject = entry;
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = LoadFromFile("EDI_TNPUPT_TTI.txt");
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new Cuspmt09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals("Response Email Subject", "Customs Permit for B00001013", processor.responseEmail.Subject);
			AssertMultilineASCIIEquals("Response Email Body Equals", string.Format(CustomsClearanceInformationHtmlBody, SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value), processor.responseEmail.Body);
		}

		const string CustomsClearanceInformationHtmlBody = "<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\">\r\n" + "<head>\r\n  <title>Customs Permit for B00001013</title>\r\n  <style type=\"text/css\">\r\n  <!--\r\n  {0}\r\n  -->\r\n</style>\r\n</head>\r\n" + "<body>\r\n  <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#FFFFFF\">\r\n    <tr>\r\n      <td class=\"banner\"><img src=\"cid:Banner.jpg\" alt=\"Banner Image\" /></td>\r\n    </tr>\r\n    <tr>\r\n      <td class=\"content\">\r\n<br />\r\n" + "<strong>Job Number : B00001013<br />\r\nPermit Number : TT6I400104P<br />\r\nCertificate Number : <br />\r\nEntry Status : AOK<br />\r\nUnique Reference Number (URN) : XXXXXXXXE01T     201102140013<br />\r\n<br />\r\n</strong>" + "A response message has been received from Singapore Customs.<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n" + "    The message sent for the above mentioned job has received the following customs permit information.</p>\r\n<br />\r\n<br />\r\n<strong>CCI : Customs Clearance Instructions<br />\r\nREG : Regulatory Information<br />\r\n</strong>\r\n<br />\r\n" + "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" + "<thead><tr class=\"tableheadings\"><th>Type</th><th>Condition Code</th><th>Description</th></tr></thead>" + "<tr><td>CUS</td><td>&nbsp;</td><td>010000000000MF</td></tr>" + "<tr><td>CCI</td><td>Z01</td><td>APPROVED BY SINGAPORE CUSTOMS.</td></tr>" + "<tr><td>CCI</td><td>GA</td><td>APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING WITH THE FOLLOWING CONDITION(S) FOR THE PERMIT TO BE VALID. FAILURE TO COMPLY WITH THE CONDITION(S) IS AN OFFENCE.</td></tr>" + "<tr><td>CCI</td><td>A3</td><td>THE GOODS AND THIS PERMIT MUST BE PRODUCED FOR CUSTOMS CLEARANCE/ENDORSEMENT AT AN AIRPORT CUSTOMS CHECKPOINT.</td></tr>" + "<tr><td>CCI</td><td>A1</td><td>THE GOODS AND THIS PERMIT MUST BE PRODUCED FOR CUSTOMS CLEARANCE/ENDORSEMENT AT A FREE TRADE &quot;IN&quot; GATE.</td></tr>" + "<tr><td>CCI</td><td>AX</td><td>GOODS RELEASED FROM THE 1ST CUSTOMS CHECKPOINT MUST BE PRODUCED AT THE 2ND CHECKPOINT WITHIN 24 HOURS. OTHERWISE, THEY MUST BE STORED AT A PLACE APPROVED BY A PROPER OFFICER OF CUSTOMS.</td></tr>" + "<tr><td>CCI</td><td>E3</td><td>GOODS REMOVED MUST BE SENT DIRECTLY TO THE PLACE OF RECEIPT AS DECLARED AFTER CUSTOMS EXAMINATIONS. NO OPERATION OF ANY KIND (EG: UNSTUFFING/STUFFING/REPACKING/STORAGE) IS ALLOWED ON THE WAY.</td></tr>" + "<tr><td>CCI</td><td>A6</td><td>IF THE PERMIT IS NOT USED, IT MUST BE CANCELLED OR RE-VALIDATED NOT LATER THAN 24 HOURS OF ITS EXPIRY.</td></tr>" + "<tr><td>CCI</td><td>EEE</td><td>END OF CARGO CLEARANCE PERMIT.</td></tr></table>\r\n" + "<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender<br />\r\n<br />\r\n<br />\r\n<br />      </td>\r\n    </tr>\r\n    <tr>\r\n" + "      <td><img src=\"cid:Footer.jpg\" alt=\"Footer Image\" /></td>\r\n    </tr>\r\n  </table>\r\n</body>\r\n</html>\r\n";

		[TestDate(2021, 05, 01)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRegulatoryInformationEmail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001012";
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = Core.SGConstants.DeclarationStatus.CancellationSent;
			var trxMessage = Factory.New<EDIMessage>();
			trxMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			trxMessage.EM_ApplicationReference = "XXXXXXXXE01T     201102141002";
			trxMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			trxMessage.EM_LinkedObject = entry;
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = LoadFromFile("EDI_TNPPMT_REM.txt");
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			SGCustomsDataRegistry.Instance.AttachPermitToAcknowledgementEmail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var processor = new Cuspmt09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals("Response Email Subject", "Customs Permit for B00001012", processor.responseEmail.Subject);
			AssertMultilineASCIIEquals("Response Email Body Equals", string.Format(RegulatoryInformationHtmlBody, SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value), processor.responseEmail.Body);
			AssertEquals("Attachment Count", 3, processor.responseEmail.Attachments.Count);
			AssertEquals("Attachment Name", "Permit.pdf", processor.responseEmail.Attachments[2].DisplayName);
		}

		const string RegulatoryInformationHtmlBody = "<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\">\r\n" + "<head>\r\n  <title>Customs Permit for B00001012</title>\r\n  <style type=\"text/css\">\r\n  <!--\r\n  {0}\r\n  -->\r\n</style>\r\n</head>\r\n" + "<body>\r\n  <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#FFFFFF\">\r\n    <tr>\r\n      <td class=\"banner\"><img src=\"cid:Banner.jpg\" alt=\"Banner Image\" /></td>\r\n    </tr>\r\n    <tr>\r\n      <td class=\"content\">\r\n<br />\r\n" + "<strong>Job Number : B00001012<br />\r\nPermit Number : RM6I000323S<br />\r\nCertificate Number : <br />\r\nEntry Status : DOK<br />\r\nUnique Reference Number (URN) : XXXXXXXXE01T     201102141002<br />\r\n<br />\r\n</strong>" + "A response message has been received from Singapore Customs.<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n" + "    The message sent for the above mentioned job has received the following customs permit information.</p>\r\n<br />\r\n<br />\r\n<strong>CCI : Customs Clearance Instructions<br />\r\nREG : Regulatory Information<br />\r\n</strong>\r\n<br />\r\n" + "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" + "<thead><tr class=\"tableheadings\"><th>Type</th><th>Condition Code</th><th>Description</th></tr></thead>" + "<tr><td>CCI</td><td>GA</td><td>APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING WITH THE FOLLOWING CONDITION(S) FOR THE PERMIT TO BE VALID. FAILURE TO COMPLY WITH THE CONDITION(S) IS AN OFFENCE.</td></tr>" + "<tr><td>CCI</td><td>BW</td><td>THE GOODS DECLARED IN THIS PERMIT ARE IMPORTED/EXPORTED UNDER THE BONDED WAREHOUSE SCHEME</td></tr>" + "<tr><td>CCI</td><td>A6</td><td>IF THE PERMIT IS NOT USED, IT MUST BE CANCELLED OR RE-VALIDATED NOT LATER THAN 24 HOURS OF ITS EXPIRY.</td></tr>" + "<tr><td>CCI</td><td>EEE</td><td>END OF CARGO CLEARANCE PERMIT.</td></tr></table>\r\n" + "<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender<br />\r\n<br />\r\n<br />\r\n<br />      </td>\r\n    </tr>\r\n    <tr>\r\n" + "      <td><img src=\"cid:Footer.jpg\" alt=\"Footer Image\" /></td>\r\n    </tr>\r\n  </table>\r\n</body>\r\n</html>\r\n";

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRefundPermitMessage()
		{
			Declaration.JE_DeclarationReference = "S15SSIN0000706";
			string messageText = LoadFromFile("EDI_RefundMessage.txt");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.RefundSent, "200406875K       201506166008", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.RefundPermitReceived, EntryHeader.CH_Status);
			SGCustomsDataRegistry.Instance.AttachPermitToAcknowledgementEmail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = LoadFromFile("EDI_RefundMessage.txt");
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new Cuspmt09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals("Response Email Subject", "Customs Permit for S15SSIN0000706", processor.responseEmail.Subject);
			AssertMultilineASCIIEquals("Response Email Body Equals", RefundInformationHtmlBody, processor.responseEmail.Body);
			AssertEquals("Attachment Count should only be email banner & footer - ie no permit generated for Refund approval message", 2, processor.responseEmail.Attachments.Count);
		}

		const string RefundInformationHtmlBody = "<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\">\r\n" + "<head>\r\n  <title>Customs Permit for S15SSIN0000706</title>\r\n  <style type=\"text/css\">\r\n  <!--\r\n  P {\r\n\t\tFONT-SIZE: 12px;\r\n\t\tCOLOR: #666666;\r\n\t\tFONT-FAMILY: Arial, sans-serif;\r\n\t\tline-height: 17px;\r\n}\r\n\r\nbody {\r\n\t\tbackground-color: #FFFFFF;\r\n\tfont-family: Arial, sans-serif;\r\n\tfont-size: 12px;\r\n\tcolor: #666666;\r\n\t\tmargin: auto;\r\n\t\twidth: 600px;\r\n}\r\n\r\nth {\r\n\tfont-family: Arial, sans-serif;\r\n\tfont-size: 12px;\r\n\tcolor: #FFFFFF;\r\n\tbackground-color: #005596;\r\n}\r\n\r\ntd {\r\n\tfont-family: Arial, sans-serif;\r\n\tfont-size: 12px;\r\n\tcolor: #666666;\r\n}\r\n\r\na, a:visited {\r\n\tcolor: #666666;\r\n\ttext-decoration: underline;\r\n}\r\n\r\na:hover {\r\n\tcolor: #00a4e4;\r\n\ttext-decoration: underline;\r\n}\r\n\r\n.heading1 {\r\n\t\tfont-family: Arial, sans-serif;\r\n\t\tfont-size: 20px;\r\n\t\tfont-weight: normal;\r\n\t\tcolor: #000000;\r\n\t\tline-height: 36px;\r\n}\r\n\r\n.subheading2 {\r\n\t\tfont-family: Arial, sans-serif;\r\n\t\tfont-size: 16px;\r\n\t\tline-height: 18px;\r\n\t\tfont-weight: bold;\r\n\tcolor: #00a4e4;\r\n}\r\n\r\n.table {\r\n\t\tborder-right: #666666 solid thin;\r\n\t\tborder-top: #666666 solid thin;\r\n\t\tborder-left: #666666 solid thin;\r\n\t\tborder-bottom: #666666 solid thin;\r\n}\r\n\r\n.tableheadings td, th {\r\n\t\tborder-bottom: #666666 solid thin;\r\n\t\tbackground-color: #005596;\r\n}\r\n\r\n.content {\r\n\t\tpadding: 0em 1em;\r\n}\r\n\r\n.banner {\r\n\t\tpadding-top: 1em;\r\n}\r\n  -->\r\n</style>\r\n</head>\r\n" + "<body>\r\n  <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#FFFFFF\">\r\n    <tr>\r\n      <td class=\"banner\"><img src=\"cid:Banner.jpg\" alt=\"Banner Image\" /></td>\r\n    </tr>\r\n    <tr>\r\n      <td class=\"content\">\r\n<br />\r\n" + "<strong>Job Number : S15SSIN0000706<br />\r\nPermit Number : IG5F014121R<br />\r\nCertificate Number : <br />\r\nEntry Status : ROK<br />\r\nUnique Reference Number (URN) : 200406875K       201506166008<br />\r\n<br />\r\n</strong>" + "A response message has been received from Singapore Customs.<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n" + "    The message sent for the above mentioned job has received the following customs permit information.</p>\r\n<br />\r\n<br />\r\n<strong>CCI : Customs Clearance Instructions<br />\r\nREG : Regulatory Information<br />\r\n</strong>\r\n<br />\r\n" + "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" + "<thead><tr class=\"tableheadings\"><th>Type</th><th>Condition Code</th><th>Description</th></tr></thead>" + "<tr><td>AER</td><td>A00</td><td>APPROVED BY SINGAPORE CUSTOMS.</td></tr>" + "<tr><td>AER</td><td>C02</td><td>REFUND APPROVED BY SINGAPORE CUSTOMS.</td></tr></table>\r\n" + "<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender<br />\r\n<br />\r\n<br />\r\n<br />      </td>\r\n    </tr>\r\n    <tr>\r\n" + "      <td><img src=\"cid:Footer.jpg\" alt=\"Footer Image\" /></td>\r\n    </tr>\r\n  </table>\r\n</body>\r\n</html>\r\n";

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRefundPermitResponseDoesNotTryToAttachPermit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001012";
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = Core.SGConstants.DeclarationStatus.RefundSent;
			var trxMessage = Factory.New<EDIMessage>();
			trxMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			trxMessage.EM_ApplicationReference = "XXXXXXXXE47T     201102140028";
			trxMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			trxMessage.EM_LinkedObject = entry;
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = LoadFromFile("EDI_IPTUPT_DUT.txt");
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			SGCustomsDataRegistry.Instance.AttachPermitToAcknowledgementEmail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var processor = new Cuspmt09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals("Response Email Subject", "Customs Permit for B00001012", processor.responseEmail.Subject);
			AssertEquals("Attachment Count should only be email banner & footer - ie no permit", 2, processor.responseEmail.Attachments.Count);
		}

		public void TestInvalidMessage()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = "THIS MESSAGE TEXT IS SO WRONG IT IS NOT FUNNY";
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new Cuspmt09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals("Response Email Subject", "ERROR PROCESSING", processor.responseEmail.Subject);
			AssertMultilineASCIIEquals("Response Email Body Equals", InvalidMessageHtmlBody, processor.responseEmail.Body);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPaymentDetailsAreCaptured()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001013";
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			var trxMessage = Factory.New<EDIMessage>();
			trxMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			trxMessage.EM_ApplicationReference = "XXXXXXXXE01T     201102140001";
			trxMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			trxMessage.EM_LinkedObject = entry;
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = LoadFromFile("EDI_INPPMT_APS.txt");
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new Cuspmt09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals("EntryPayInfo should have been created", 1, entry.EntryPayInfos.Count);
			var paymentInfo = entry.EntryPayInfos[0];
			AssertEquals("C9_IncomingPayResponseNo", "1", paymentInfo.C9_IncomingPayResponseNo);
			AssertEquals("C9_PaymentAmount", 20037.33m, paymentInfo.C9_PaymentAmount);
			AssertEquals("C9_TransactionType", "APS", paymentInfo.C9_TransactionType);
			AssertEquals("C9_PaymentDate", new ZDateTime(2011, 02, 14, 00, 15, 50), paymentInfo.C9_PaymentDate);
			AssertEquals("C9_PaymentReference", "ME6I309961C", paymentInfo.C9_PaymentReference);
			AssertEquals("C9_PaymentParty should indicate payment by Broker", "DEF", paymentInfo.C9_PaymentParty);
		}

		public void TestBrokerPaymentDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00004520";
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			var trxMessage = Factory.New<EDIMessage>();
			trxMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			trxMessage.EM_ApplicationReference = "199702247W       201710101019";
			trxMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			trxMessage.EM_LinkedObject = entry;
			var responseMessage = Factory.New<EDIMessage>();
			responseMessage.EM_MessageText = "UNH+CWISEB00004520+CUSPMT:0:1:RT:041+IPTPMT'BGM+962:::DNG+199702247W       201710101019+11'CST++9'LOC+11+KZ'LOC+88+RCNOSTK'LOC+9+GRLIX'DTM+178:20171010:102'DTM+416:20171010084019SST:304'GEI+5+:Y'MEA+ABK++CTN:25'MEA+AAH++TNE:120.000'EQD+CN+PWSU7366558:1+003:::FCL20'SEL+NA'FTX+AAI+++SHIPPER SEAL ?:- L356612/ES11393'RFF+ABT:DP7K004961T'DTM+148:20171110084354SST:304'DTM+273:2017111020171123:718'RFF+AEA:SC'FTX+CCI++GQ+IF THE DUTY/GST IS NOT PAID WITHIN THE VALIDITY PERIOD OF THE PERMIT,    THIS PERMIT MUST BE CANCELLED BEFORE ITS EXPIRY DATE IF IT IS NOT USED FOR CARGOCLEARANCE'FTX+CCI++P1+THE CONTAINER(S) MUST BE PRODUCED AT CUSTOMS CHECKPOINT(S) FOR CLEARANCE UNLESS DIRECTED TO ?'GREEN?' LANE. FOR CONTAINER(S) TO BE SCANNED, PLEASE PRODUCE FOR SCANNING BY ICA AT SCANNING STATION AS DIRECTED.'FTX+CCI++GA+APPROVED BY SINGAPORE CUSTOMS SUBJECT TO THE IMPORTER, EXPORTER,         DECLARING AGENT OR/AND THE DECLARANT COMPLYING WITH THE FOLLOWING CONDITION(S)  FOR THE PERMIT TO BE VALID. FAILURE TO COMPLY WITH CONDITION(S) IS AN OFFENCE.'FTX+CCI++GX+THE DUTY/GST MUST BE PAID SHOULD THE GIRO DEDUCTION FAIL. SINGAPORE      CUSTOMS MAY INVOKE THE IMPORTER/DECLARING AGENT?'S SECURITY FOR RECOVERY OF THE  DUTY/GST. A PENALTY CHARGE MAY BE IMPOSED BY SINGAPORE CUSTOMS FOR AN           UNSUCCESSFUL GIRO DEDUCTION.'FTX+CCI++G7+SUCCESSFUL GIRO DEDUCTION OF THE AMOUNT TO BE PAID FROM THE DECLARING    AGENT?'S ACCOUNT. YOU MUST HAVE ENOUGH FUNDS IN YOUR BANK ACCOUNT TO MEET PAYMENTBEFORE MAKING THE DECLARATION.'FTX+CCI++EEE+END OF CARGO CLEARANCE PERMIT.'TDT+3+12323+1+++++:::ADMIRALENGRACHT'DOC+704+GJFSDJHKJHK'NAD+DT'CTA+IC+P213111:GARY O?'DEA'COM+?+61280012206:TE'NAD+AE+199702247W++YOUR SINGAPORE CORP:::::1'NAD+IM+123456ABC++TN41PARTYNAME:::::2'NAD+CG+197801536N++BASFSOUTHEASTASIAPTELTD:::::1'NAD+FW+197801536N++BASFSOUTHEASTASIAPTELTD:::::1'UNS+D'DMS+INVOICE DETAILS'MOA+39:12000.00:SGD'TOD+++FOB'NAD+SE'DOC+380+INV123'DTM+3:20171010:102'ALC+C'MOA+64:100.00:SGD'ALC+C'MOA+70:10.00:SGD'CST+1+87112051'FTX+AAA+++NON CKD MOTORCYCLES INCL MOTOR SCOOTERS RECIPROCATING INTERNAL COMBUSTION ENGINE OVER 150 CC BUT NOT OVER 200 CC (NMB)'FTX+PRD+++UNBRANDED'LOC+27+ID'MEA+AAF++NMB:1.0000'MEA+AAE++NMB:1.0000'MEA+AAI++NMB:1.0000'MOA+63:12110.00'RFF+IV:INV123'RFF+SE'IMD++8+:::197.00:CC'RFF+AVM'GIN+AV+91HN01950002K+MC22E1081387+N'DOC+703+HJGKJ'TAX+7++++:::7'MOA+369:949.42'TAX+5++++PER:::12.0000'MOA+161:1453.20'UNS+S'CNT+5:1'TAX+1'MOA+63:12110.00'TAX+7'MOA+369:949.42'TAX+5'MOA+161:1453.20'TAX+2'MOA+9:2402.62'UNT+73+CWISEB00004520'";
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new Cuspmt09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(responseMessage);
			AssertEquals("EntryPayInfo should have been created", 1, entry.EntryPayInfos.Count);
			var paymentInfo = entry.EntryPayInfos[0];
			AssertEquals("C9_IncomingPayResponseNo", "CWISEB00004520", paymentInfo.C9_IncomingPayResponseNo);
			AssertEquals("C9_PaymentAmount", 2402.62m, paymentInfo.C9_PaymentAmount);
			AssertEquals("C9_TransactionType", "DNG", paymentInfo.C9_TransactionType);
			AssertEquals("C9_PaymentDate", new ZDateTime(2017, 11, 10, 08, 43, 54), paymentInfo.C9_PaymentDate);
			AssertEquals("C9_PaymentReference", "DP7K004961T", paymentInfo.C9_PaymentReference);
			AssertEquals("C9_PaymentParty should indicate payment by Broker", "BRK", paymentInfo.C9_PaymentParty);
		}

		public void TestImporterPaymentDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "S00001169";
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			var trxMessage = Factory.New<EDIMessage>();
			trxMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			trxMessage.EM_ApplicationReference = "199702247W       201706200802";
			trxMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			trxMessage.EM_LinkedObject = entry;
			var responseMessage = Factory.New<EDIMessage>();
			responseMessage.EM_MessageText = "UNH+CWISES00001169+CUSPMT:0:1:RT:041+IPTPMT'BGM+962:::GST+199702247W       201706200802+11'CST++5'LOC+11+CZ'LOC+88+O:::FORESPAND FOOD ENTERPRISE PTE LTD  21 TOH GUAN ROAD EAST  08-17  SINGAPORE 608609'LOC+9+JPTYO'DTM+178:20170620:102'DTM+416:20170620053126SST:304'GEI+5+:Y'MEA+ABK++PAT:10'MEA+AAH++KGM:160.000'RFF+ABT:IG7F192188X'DTM+148:20170620053127SST:304'DTM+273:2017062020170704:718'RFF+AEA:SC'FTX+CCI++Y99+SPECIMEN PERMIT ONLY'FTX+CCI++Z01+APPROVED BY SINGAPORE CUSTOMS.'FTX+CCI++GA+APPROVED BY CUSTOMS SUBJECT TO THE IMPORTER, EXPORTER, DECLARING AGENT   OR/AND THE DECLARANT COMPLYING WITH THE FOLLOWING CONDITION(S) FOR THE PERMIT   TO BE VALID. FAILURE TO COMPLY WITH CONDITION(S) IS AN OFFENCE.'FTX+CCI++TX+THE GOODS DECLARED IN THIS PERMIT ARE IMPORTED/EXPORTED BY A TAXABLE     PERSON'FTX+CCI++GQ+IF THE DUTY/GST IS NOT PAID WITHIN THE VALIDITY PERIOD OF THE PERMIT,    THIS PERMIT MUST BE CANCELLED BEFORE ITS EXPIRY DATE IF IT IS NOT USED FOR CARGOCLEARANCE'FTX+CCI++MA+THE GOODS AND THIS PERMIT WITH INVOICES, BL/AWB, ETC MUST BE PRODUCED    FOR CUSTOMS CLEARANCE AT A FREE TRADE ZONE ?'OUT?' GATE, WOODLANDS                TRAIN/WOODLANDS/TUAS CHECKPOINT UNLESS IT IS DIRECTED TO THE ?'GREEN LANE?' AT    THE TIME OF CLEARANCE, OR A DESIGNATED CUSTOMS OFFICE OR STATION AS INSTRUCTED.'FTX+CCI++GF+SUCCESSFUL GIRO DEDUCTION OF THE AMOUNT TO BE PAID FROM THE IMPORTER?'S   ACCOUNT. IMPORTER MUST HAVE ENOUGH FUNDS IN BANK ACCOUNT TO MEET PAYMENT BEFORE INSTRUCTING DECLARING AGENT TO MAKE THIS DECLARATION.'FTX+CCI++GX+THE DUTY/GST MUST BE PAID SHOULD THE GIRO DEDUCTION FAIL. CUSTOMS MAY    INVOKE THE IMPORTER/DECLARING AGENT?'S BG FOR RECOVERY OF THE DUTY/GST. A        PENALTY CHARGE MAY BE IMPOSED BY CUSTOMS FOR AN UNSUCCESSFUL GIRO DEDUCTION.'FTX+CCI++EEE+END OF CARGO CLEARANCE PERMIT.'TDT+3+QF89+4'DOC+704+88999999999'NAD+DT'CTA+IC+P213111:GARY O?'DEA'COM+?+61280012206:TE'NAD+AE+199702247W++YOUR SINGAPORE CORP:::::1'NAD+IM+198700002E++FORESPAND FOOD ENTERPRISE PTE LTD:::::2'NAD+CG+197201770G++SATS CARGO SERVICES PTE LTD:::::1'NAD+FW+199702247W++YOUR SINGAPORE COMPANY:::::1'UNS+D'DMS+INVOICE DETAILS'MOA+39:3500.00:SGD'TOD+++CIF'NAD+SU+++JP 1 IMPORTER/EXPORTER CORPORATION'DOC+380+H320999'DTM+3:20160714:102'CST+1+42050010'FTX+AAA+++BOOT LACES & MATS OF LEATHER OR COMPOSITION LEATHER'FTX+PRD+++KIWI:SIZE 5'LOC+27+CN'MEA+AAF++NMB:100.0000'MOA+63:3500.00'RFF+IV:H320999'DOC+703+HOUSE1'TAX+7++++:::7'MOA+369:245.00'UNS+S'CNT+5:1'TAX+1'MOA+63:3500.00'TAX+7'MOA+369:245.00'TAX+2'MOA+9:245.00'UNT+59+CWISES00001169'";
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new Cuspmt09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(responseMessage);
			AssertEquals("EntryPayInfo should have been created", 1, entry.EntryPayInfos.Count);
			var paymentInfo = entry.EntryPayInfos[0];
			AssertEquals("C9_IncomingPayResponseNo", "CWISES00001169", paymentInfo.C9_IncomingPayResponseNo);
			AssertEquals("C9_PaymentAmount", 245.00m, paymentInfo.C9_PaymentAmount);
			AssertEquals("C9_TransactionType", "GST", paymentInfo.C9_TransactionType);
			AssertEquals("C9_PaymentDate", new ZDateTime(2017, 06, 20, 05, 31, 27), paymentInfo.C9_PaymentDate);
			AssertEquals("C9_PaymentReference", "IG7F192188X", paymentInfo.C9_PaymentReference);
			AssertEquals("C9_PaymentParty should indicate payment by Importer", "IMP", paymentInfo.C9_PaymentParty);
		}

		public void TestBrokerPaymentDetailsWhenUpdated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00004538";
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			var trxMessage = Factory.New<EDIMessage>();
			trxMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			trxMessage.EM_ApplicationReference = "199702247W       201712111035";
			trxMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			trxMessage.EM_LinkedObject = entry;
			var responseMessage = Factory.New<EDIMessage>();
			responseMessage.EM_MessageText = "UNH+WTGB00004538+CUSPMT:0:1:RT:041+IPTPMT'BGM+962:::GST+199702247W       201712111035+11'CST++5'LOC+11+KZ'LOC+88+O:::NO. 48, JURONG WEST INDUSTRIAL PARK  STREET. 24  118048 SINGAPORE'LOC+9+GBCHE'DTM+178:20171211:102'DTM+416:20171211154947SST:304'GEI+5+:Y'MEA+ABK++CTN:1'MEA+AAH++KGM:2000.000'FTX+AAI+++VENDOR TESTING'RFF+ABT:IG7L197116L'DTM+148:20171211154949SST:304'DTM+273:2017121120171222:718'RFF+MR:F01T.F01T009'RFF+MR:F01T.F01T008'RFF+MR:F01T.F01T010'RFF+DM:IP09A9999'RFF+AEA:SC'FTX+CCI++Y99+SPECIMEN PERMIT ONLY'FTX+CCI++Z01+APPROVED BY SINGAPORE CUSTOMS.'FTX+CCI++Y94+YOUR REQUEST FOR REMOTE PRINTING OF THIS CCP IS GRANTED SUBJECT TO THE   TERMS & CONDITIONS FOR REMOTE PRINTING. YOUR SIGNATURE ON REMOTE CCP IS NOT     REQUIRED.'FTX+CCI++GA+APPROVED BY SINGAPORE CUSTOMS SUBJECT TO THE IMPORTER, EXPORTER,         DECLARING AGENT OR/AND THE DECLARANT COMPLYING WITH THE FOLLOWING CONDITION(S)  FOR THE PERMIT TO BE VALID. FAILURE TO COMPLY WITH CONDITION(S) IS AN OFFENCE.'FTX+CCI++TX+THE GOODS DECLARED IN THIS PERMIT ARE IMPORTED/EXPORTED BY A TAXABLE     PERSON'FTX+CCI++GQ+IF THE DUTY/GST IS NOT PAID WITHIN THE VALIDITY PERIOD OF THE PERMIT,    THIS PERMIT MUST BE CANCELLED BEFORE ITS EXPIRY DATE IF IT IS NOT USED FOR CARGOCLEARANCE'FTX+CCI++MA+THE GOODS AND THIS PERMIT WITH INVOICES, BL/AWB, ETC MUST BE PRODUCED    FOR CUSTOMS CLEARANCE AT A FREE TRADE ZONE ?'OUT?' GATE, WOODLANDS                TRAIN/WOODLANDS/TUAS CHECKPOINT UNLESS IT IS DIRECTED TO THE ?'GREEN LANE?' AT    THE TIME OF CLEARANCE, OR A DESIGNATED CUSTOMS OFFICE OR STATION AS INSTRUCTED.'FTX+CCI++G7+SUCCESSFUL GIRO DEDUCTION OF THE AMOUNT TO BE PAID FROM THE DECLARING    AGENT?'S ACCOUNT. YOU MUST HAVE ENOUGH FUNDS IN YOUR BANK ACCOUNT TO MEET PAYMENTBEFORE MAKING THE DECLARATION.'FTX+CCI++GX+THE DUTY/GST MUST BE PAID SHOULD THE GIRO DEDUCTION FAIL. SINGAPORE      CUSTOMS MAY INVOKE THE IMPORTER/DECLARING AGENT?'S SECURITY FOR RECOVERY OF THE  DUTY/GST. A PENALTY CHARGE MAY BE IMPOSED BY SINGAPORE CUSTOMS FOR AN           UNSUCCESSFUL GIRO DEDUCTION.'FTX+CCI++EEE+END OF CARGO CLEARANCE PERMIT.'TDT+3+SQ410+4'DOC+704+61800230249'NAD+DT'CTA+IC+P213111:GARY O?'DEA'COM+?+61280012206:TE'NAD+AE+199702247W++YOUR SINGAPORE CORP:::::1'NAD+IM+198800784N++CRIMSONLOGIC:::::2'NAD+CG+123456ABC++TN41PARTYNAME:::::1'NAD+FW+197801536N++BASFSOUTHEASTASIAPTELTD:::::1'NAD+BB'RFF+DAN:D'UNS+D'DMS+INVOICE DETAILS'MOA+39:6534.00:SGD'TOD+++CIF'NAD+SU++198803498R+HAMWORTHY KSE PTE LTD'DOC+380+INV1579'DTM+3:20171205:102'CST+1+76012000'FTX+AAA+++UNWROUGHT ALUMINIUM ALLOYS (TNE)'FTX+PRD+++EAGLE BRAND'LOC+27+GL'MEA+AAF++TNE:2.0000'MOA+63:6534.00'RFF+IV:INV1579'DOC+703+G42488'TAX+7++++:::7'MOA+369:457.38'UNS+S'CNT+5:1'TAX+1'MOA+63:6534.00'TAX+7'MOA+369:457.38'TAX+2'MOA+9:457.38'UNT+67+WTGB00004538'";
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new Cuspmt09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(responseMessage);
			AssertEquals("EntryPayInfo should have been created", 1, entry.EntryPayInfos.Count);
			var paymentInfo = entry.EntryPayInfos[0];
			AssertEquals("C9_IncomingPayResponseNo", "B00004538", paymentInfo.C9_IncomingPayResponseNo);
			AssertEquals("C9_PaymentAmount", 457.38m, paymentInfo.C9_PaymentAmount);
			AssertEquals("C9_TransactionType", "GST", paymentInfo.C9_TransactionType);
			AssertEquals("C9_PaymentDate", new ZDateTime(2017, 12, 11, 15, 49, 49), paymentInfo.C9_PaymentDate);
			AssertEquals("C9_PaymentReference", "IG7L197116L", paymentInfo.C9_PaymentReference);
			AssertEquals("C9_PaymentParty should indicate payment by Broker", "BRK", paymentInfo.C9_PaymentParty);
			// Entry updated
			var amendmentMessage = Factory.New<EDIMessage>();
			amendmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			amendmentMessage.EM_ApplicationReference = "199702247W       201712111037";
			amendmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			amendmentMessage.EM_LinkedObject = entry;
			var updRespsoneMessage = Factory.New<EDIMessage>();
			updRespsoneMessage.EM_MessageText = "UNH+WTGB00004538+CUSPMT:0:1:RT:041+IPTUPT'BGM+962:::GST+199702247W       201712111037+32'CST++5+AME'LOC+11+KZ'LOC+88+O:::NO. 48, JURONG WEST INDUSTRIAL PARK  STREET. 24  118048 SINGAPORE'LOC+9+GBCHE'DTM+178:20171211:102'DTM+416:20171211155709SST:304'GEI+5+:Y'MEA+ABK++CTN:1'MEA+AAH++KGM:200.000'FTX+AAI+++VENDOR TESTING'FTX+ACF+++FIXING ERRORS IN WEIGHT AND VALUE'RFF+ABT:IG7L197116L'DTM+148:20171211155756SST:304'DTM+273:2017121120171222:718'DTM+56:20171211154949SST:304'FTX+CUS+++167900000000MF:108101000000MF:080501000000MF:168900000000MF'FTX+CUS+++134101000000MF:163900000000MF:018000000000MF:153101000000MF'RFF+MR:F01T.F01T009'RFF+MR:F01T.F01T008'RFF+MR:F01T.F01T010'RFF+DM:IP09A9999'RFF+AEA:SC'FTX+CCI++Y99+SPECIMEN PERMIT ONLY'FTX+CCI++Z01+APPROVED BY SINGAPORE CUSTOMS.'FTX+CCI++Y95+PLS CHECK AGAIN THE DECLARED 1) HS CODES/DESCRIPTION, 2) ITEM QUANTITY   OR VALUE, OR 3) ITEM VALUE WHICH EXCEEDED $1 MILLION. IF WRONG, PLEASE AMEND OR CANCEL THIS UNUSED PERMIT WITHIN 48 HOURS. IN PARTICULAR, FOR UNUSED GST        PAYMENT PERMITS, CANCELLATION OF PERMITS OR AMENDMENTS TO FIELDS AFFECTING GST  SHOULD BE SUBMITTED WITHIN 23?:59?:59 HOURS OF THE DATE OF PERMIT APPROVAL.'FTX+CCI++Y94+YOUR REQUEST FOR REMOTE PRINTING OF THIS CCP IS GRANTED SUBJECT TO THE   TERMS & CONDITIONS FOR REMOTE PRINTING. YOUR SIGNATURE ON REMOTE CCP IS NOT     REQUIRED.'FTX+CCI++Z20+AMENDMENT APPROVED BY SINGAPORE CUSTOMS. THIS AMENDED PERMIT WILL        SUPERCEDE THE PREVIOUS APPROVED PERMIT AND SHOULD BE USED FOR ANY SUBSEQUENT    FOLLOW UP RELATING TO THIS PERMIT.'FTX+CCI++GA+APPROVED BY SINGAPORE CUSTOMS SUBJECT TO THE IMPORTER, EXPORTER,         DECLARING AGENT OR/AND THE DECLARANT COMPLYING WITH THE FOLLOWING CONDITION(S)  FOR THE PERMIT TO BE VALID. FAILURE TO COMPLY WITH CONDITION(S) IS AN OFFENCE.'FTX+CCI++TX+THE GOODS DECLARED IN THIS PERMIT ARE IMPORTED/EXPORTED BY A TAXABLE     PERSON'FTX+CCI++GQ+IF THE DUTY/GST IS NOT PAID WITHIN THE VALIDITY PERIOD OF THE PERMIT,    THIS PERMIT MUST BE CANCELLED BEFORE ITS EXPIRY DATE IF IT IS NOT USED FOR CARGOCLEARANCE'FTX+CCI++MA+THE GOODS AND THIS PERMIT WITH INVOICES, BL/AWB, ETC MUST BE PRODUCED    FOR CUSTOMS CLEARANCE AT A FREE TRADE ZONE ?'OUT?' GATE, WOODLANDS                TRAIN/WOODLANDS/TUAS CHECKPOINT UNLESS IT IS DIRECTED TO THE ?'GREEN LANE?' AT    THE TIME OF CLEARANCE, OR A DESIGNATED CUSTOMS OFFICE OR STATION AS INSTRUCTED.'FTX+CCI++G7+SUCCESSFUL GIRO DEDUCTION OF THE AMOUNT TO BE PAID FROM THE DECLARING    AGENT?'S ACCOUNT. YOU MUST HAVE ENOUGH FUNDS IN YOUR BANK ACCOUNT TO MEET PAYMENTBEFORE MAKING THE DECLARATION.'FTX+CCI++GX+THE DUTY/GST MUST BE PAID SHOULD THE GIRO DEDUCTION FAIL. SINGAPORE      CUSTOMS MAY INVOKE THE IMPORTER/DECLARING AGENT?'S SECURITY FOR RECOVERY OF THE  DUTY/GST. A PENALTY CHARGE MAY BE IMPOSED BY SINGAPORE CUSTOMS FOR AN           UNSUCCESSFUL GIRO DEDUCTION.'FTX+CCI++EEE+END OF CARGO CLEARANCE PERMIT.'TDT+3+SQ410+4'DOC+704+61800230249'NAD+DT'CTA+IC+P213111:GARY O?'DEA'COM+?+61280012206:TE'NAD+AE+199702247W++YOUR SINGAPORE CORP:::::1'NAD+IM+198800784N++CRIMSONLOGIC:::::2'NAD+CG+123456ABC++TN41PARTYNAME:::::1'NAD+FW+197801536N++BASFSOUTHEASTASIAPTELTD:::::1'NAD+BB'RFF+DAN:D'UNS+D'DMS+INVOICE DETAILS'MOA+39:16534.00:SGD'TOD+++CIF'NAD+SU++198803498R+HAMWORTHY KSE PTE LTD'DOC+380+INV1579'DTM+3:20171205:102'CST+1+76012000'FTX+AAA+++UNWROUGHT ALUMINIUM ALLOYS (TNE)'FTX+PRD+++EAGLE BRAND'LOC+27+GL'MEA+AAF++TNE:0.2000'MOA+63:16534.00'RFF+IV:INV1579'DOC+703+G42488'TAX+7++++:::7'MOA+369:1157.38'UNS+S'CNT+5:1'CNT+6:1'CNT+22:8'TAX+1'MOA+63:16534.00'TAX+7'MOA+369:1157.38'TAX+2'MOA+9:1157.38'UNT+75+WTGB00004538'";
			updRespsoneMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			updRespsoneMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor = new Cuspmt09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(updRespsoneMessage);
			AssertEquals("EntryPayInfo should have been updated - should still only be 1 record", 1, entry.EntryPayInfos.Count);
			var amendedPaymentInfo = entry.EntryPayInfos[0];
			AssertEquals("C9_IncomingPayResponseNo", "B00004538", amendedPaymentInfo.C9_IncomingPayResponseNo);
			AssertEquals("C9_PaymentAmount should have been updated", 1157.38m, amendedPaymentInfo.C9_PaymentAmount);
			AssertEquals("C9_TransactionType", "GST", amendedPaymentInfo.C9_TransactionType);
			AssertEquals("C9_PaymentDate", new ZDateTime(2017, 12, 11, 15, 57, 56), amendedPaymentInfo.C9_PaymentDate);
			AssertEquals("C9_PaymentReference", "IG7L197116L", amendedPaymentInfo.C9_PaymentReference);
			AssertEquals("C9_PaymentParty should indicate payment by Broker", "BRK", amendedPaymentInfo.C9_PaymentParty);
		}

		const string InvalidMessageHtmlBody = "FATAL PROCESSING ERROR IN MESSAGE : \r\nTHIS MESSAGE TEXT IS SO WRONG IT IS NOT FUNNY";

		string LoadFromFile(string fileName)
		{
			return File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\Business.Test\Messaging\CustomsMessaging\MessageProcessors\TestMessages\D09B\" + fileName);
		}

		protected override SGMessageProcessor GetMessageProcessor()
		{
			return new Cuspmt09bMessageProcessor(new LoggingInformation());
		}
	}
}
