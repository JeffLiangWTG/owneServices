using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	sealed class Cusres09bMessageProcessorTest : SGMessageProcessorTest
	{
		public void TestCUSRES()
		{
			var messageText = "UNH+1+CUSRES:D:09B:UN:041+STATUS'BGM+961:::AAC+XXXXXXXXE01T     201105240112'FTX+ACD+++APPROVED ON 20110524'RFF+ACW:IM6I011522Y'RFF+MR:E01T.E01T001'RFF+MS:DCST.DCST201'";
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.CancellationPending, "XXXXXXXXE01T     201105240112", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.CancellationAccepted, EntryHeader.CH_Status);
		}

		public void TestCancellationEmail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001013";
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = Core.SGConstants.DeclarationStatus.CancellationSent;
			var trxMessage = Factory.New<EDIMessage>();
			trxMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			trxMessage.EM_ApplicationReference = "XXXXXXXXE01T     201105240112";
			trxMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			trxMessage.EM_LinkedObject = entry;
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = "UNH+1+CUSRES:D:09B:UN:041+STATUS'BGM+961:::AAC+XXXXXXXXE01T     201105240112'FTX+ACD+++APPROVED ON 20110524'RFF+ACW:IM6I011522Y'RFF+MR:E01T.E01T001'RFF+MS:DCST.DCST201'";
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new Cusres09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals("Response Email Subject", "Cancellation for B00001013", processor.responseEmail.Subject);
			AssertMultilineASCIIEquals("Response Email Body Equals", cancellationHtmlBody, processor.responseEmail.Body);
		}

		readonly string cancellationHtmlBody = string.Format("<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\">\r\n<head>\r\n  <title>Cancellation for B00001013</title>\r\n  <style type=\"text/css\">\r\n  <!--\r\n  {0}\r\n  -->\r\n</style>\r\n</head>\r\n<body>\r\n  <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#FFFFFF\">\r\n    <tr>\r\n      <td class=\"banner\"><img src=\"cid:Banner.jpg\" alt=\"Banner Image\" /></td>\r\n    </tr>\r\n    <tr>\r\n      <td class=\"content\">\r\n<br />\r\n<strong>Job Number : B00001013<br />\r\nEntry Status : COK<br />\r\nUnique Reference Number (URN) : XXXXXXXXE01T     201105240112<br />\r\n<br />\r\n</strong>A response message has been received from Singapore Customs.<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The message sent for the above mentioned job had the following cancellation information.</p>\r\n<br />\r\n<br />\r\n<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Reason</th></tr></thead><tr><td>APPROVED ON 20110524 </td></tr></table>\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender<br />\r\n<br />\r\n<br />\r\n<br />      </td>\r\n    </tr>\r\n    <tr>\r\n      <td><img src=\"cid:Footer.jpg\" alt=\"Footer Image\" /></td>\r\n    </tr>\r\n  </table>\r\n</body>\r\n</html>\r\n", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
		public void TestInvalidMessage()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = "THIS MESSAGE TEXT IS SO WRONG IT IS NOT FUNNY";
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new Cusres09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals("Response Email Subject", "ERROR PROCESSING", processor.responseEmail.Subject);
			AssertMultilineASCIIEquals("Response Email Body Equals", InvalidMessageHtmlBody, processor.responseEmail.Body);
		}

		const string InvalidMessageHtmlBody = "FATAL PROCESSING ERROR IN MESSAGE : \r\nTHIS MESSAGE TEXT IS SO WRONG IT IS NOT FUNNY";
		protected override SGMessageProcessor GetMessageProcessor()
		{
			return new Cusres09bMessageProcessor(new LoggingInformation());
		}
	}
}
