using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	sealed class Debadv09bMessageProcessorTest : SGMessageProcessorTest
	{
		public void TestDEBADV()
		{
			string messageText = "UNH+1+DEBADV:D:09B:UN:041+FEEMSG'BGM+80+XXXXXXXXE01T     201102148892+9'BUS+1:LIF'DTM+137:201102141025:203'RFF+ABT:TT6I400104P'MOA+9:40.00:SGD'DTM+205:20110214:102'FII+AO'NAD+MS+RPIT.RPIT101'NAD+MR+E01T.E01T001'UNT+11+1'";
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, "XXXXXXXXE01T     201102148892", messageText);
		}

		public void TestFeeMessageInformationEmail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001013";
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			var trxMessage = Factory.New<EDIMessage>();
			trxMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			trxMessage.EM_ApplicationReference = "XXXXXXXXE01T     201102148889";
			trxMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			trxMessage.EM_LinkedObject = entry;
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = "UNH+1+DEBADV:D:09B:UN:041+FEEMSG'BGM+80+XXXXXXXXE01T     201102148889+9'BUS+1:LIF'DTM+137:201102140924:203'RFF+DM:AE/001688/2005/T'MOA+9:20.00:SGD'DTM+205:20110214:102'FTX+PMD+++ | | | | | | | | |301|JLN AHMAD IBRAHIM| | | |639526| | | | | | |'FII+AO'NAD+MS+AEBT.AEBT101'NAD+MR+E01T.E01T001'UNT+12+1'";
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new Debadv09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals("Response Email Subject", "Licence fee payment advice for B00001013", processor.responseEmail.Subject);
			AssertMultilineASCIIEquals("Response Email Body Equals", debitAdviceInformationHtmlBody, processor.responseEmail.Body);
		}

		readonly string debitAdviceInformationHtmlBody = string.Format("<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\">\r\n<head>\r\n  <title>Licence fee payment advice for B00001013</title>\r\n  <style type=\"text/css\">\r\n  <!--\r\n  {0}\r\n  -->\r\n</style>\r\n</head>\r\n<body>\r\n  <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#FFFFFF\">\r\n    <tr>\r\n      <td class=\"banner\"><img src=\"cid:Banner.jpg\" alt=\"Banner Image\" /></td>\r\n    </tr>\r\n    <tr>\r\n      <td class=\"content\">\r\n<br />\r\n<strong>Job Number : B00001013<br />\r\nUnique Reference Number (URN) : XXXXXXXXE01T     201102148889<br />\r\n<br />\r\n</strong>A fee message has been received from Issuing Authorities of CA licences or permits.<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The message sent for the above mentioned job has now received the following licence fees payment information.</p>\r\n<br />\r\n<br />\r\nPermit Number : <br />\r\nLicence Number : AE/001688/2005/T<br />\r\nFee Amount : 20.00<br />\r\nDate of Debit : 14-FEB-2011<br />\r\nPayment detail/remittance information :  | | | | | | | | |301|JLN AHMAD IBRAHIM| | | |639526| | | | | | |<br />\r\n<br />\r\n<!--DynamicHtml-->\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender<br />\r\n<br />\r\n<br />\r\n<br />      </td>\r\n    </tr>\r\n    <tr>\r\n      <td><img src=\"cid:Footer.jpg\" alt=\"Footer Image\" /></td>\r\n    </tr>\r\n  </table>\r\n</body>\r\n</html>\r\n", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
		public void TestFeeMessageInformationEmail_Permit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001015";
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			var trxMessage = Factory.New<EDIMessage>();
			trxMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			trxMessage.EM_ApplicationReference = "XXXXXXXXE01T     201102148888";
			trxMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			trxMessage.EM_LinkedObject = entry;
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = "UNH+1+DEBADV:D:09B:UN:041+FEEMSG'BGM+80+XXXXXXXXE01T     201102148888+9'BUS+1:LIF'DTM+137:201102140902:203'RFF+ABT:IG6B000002W'MOA+9:40.00:SGD'DTM+205:20110214:102'FII+AO'NAD+MS+RPIT.RPIT101'NAD+MR+E01T.E01T001'UNT+11+1'";
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new Debadv09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals("Response Email Subject", "Licence fee payment advice for B00001015", processor.responseEmail.Subject);
			AssertMultilineASCIIEquals("Response Email Body Equals", debitAdviceWithPermitInformationHtmlBody, processor.responseEmail.Body);
		}

		readonly string debitAdviceWithPermitInformationHtmlBody = string.Format("<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\">\r\n<head>\r\n  <title>Licence fee payment advice for B00001015</title>\r\n  <style type=\"text/css\">\r\n  <!--\r\n  {0}\r\n  -->\r\n</style>\r\n</head>\r\n<body>\r\n  <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#FFFFFF\">\r\n    <tr>\r\n      <td class=\"banner\"><img src=\"cid:Banner.jpg\" alt=\"Banner Image\" /></td>\r\n    </tr>\r\n    <tr>\r\n      <td class=\"content\">\r\n<br />\r\n<strong>Job Number : B00001015<br />\r\nUnique Reference Number (URN) : XXXXXXXXE01T     201102148888<br />\r\n<br />\r\n</strong>A fee message has been received from Issuing Authorities of CA licences or permits.<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The message sent for the above mentioned job has now received the following licence fees payment information.</p>\r\n<br />\r\n<br />\r\nPermit Number : IG6B000002W<br />\r\nLicence Number : <br />\r\nFee Amount : 40.00<br />\r\nDate of Debit : 14-FEB-2011<br />\r\nPayment detail/remittance information : <br />\r\n<br />\r\n<!--DynamicHtml-->\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender<br />\r\n<br />\r\n<br />\r\n<br />      </td>\r\n    </tr>\r\n    <tr>\r\n      <td><img src=\"cid:Footer.jpg\" alt=\"Footer Image\" /></td>\r\n    </tr>\r\n  </table>\r\n</body>\r\n</html>\r\n", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
		protected override SGMessageProcessor GetMessageProcessor()
		{
			return new Debadv09bMessageProcessor(new LoggingInformation());
		}
	}
}
