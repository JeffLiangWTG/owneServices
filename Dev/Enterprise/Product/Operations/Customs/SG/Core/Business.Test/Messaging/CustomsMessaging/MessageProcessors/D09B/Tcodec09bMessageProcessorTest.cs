using System.IO;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	sealed class Tcodec09bMessageProcessorTest : SGMessageProcessorTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTCODEC()
		{
			var messageText = LoadFromFile("EDI_COODCI_NH19.txt");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationSent, "XXXXXXXXE01T     201105240149", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, EntryHeader.CH_Status);
			AssertEquals("647838", Declaration.CertificateNumber);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCertificateOfOriginResponseEmail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001013";
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = Core.SGConstants.DeclarationStatus.CancellationSent;
			var trxMessage = Factory.New<EDIMessage>();
			trxMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			trxMessage.EM_ApplicationReference = "XXXXXXXXE01T     201105240149";
			trxMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			trxMessage.EM_LinkedObject = entry;
			var message = Factory.New<SGEDIMessage>();
			message.EM_MessageText = LoadFromFile("EDI_COODCI_NH19.txt");
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new Tcodec09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals("Response Email Subject", "Certificate of Origin for B00001013", processor.responseEmail.Subject);
			AssertMultilineASCIIEquals("Response Email Body Equals", string.Format(CertOriginHtmlBody, SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value), processor.responseEmail.Body);
		}

		public void TestInvalidMessage()
		{
			var message = Factory.New<SGEDIMessage>();
			message.EM_MessageText = "THIS MESSAGE TEXT IS SO WRONG IT IS NOT FUNNY";
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new Tcodec09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals("Response Email Subject", "ERROR PROCESSING", processor.responseEmail.Subject);
			AssertMultilineASCIIEquals("Response Email Body Equals", InvalidMessageHtmlBody, processor.responseEmail.Body);
		}

		protected override SGMessageProcessor GetMessageProcessor() => new Tcodec09bMessageProcessor(new LoggingInformation());

		string LoadFromFile(string fileName) => File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\Business.Test\Messaging\CustomsMessaging\MessageProcessors\TestMessages\D09B\" + fileName);

		const string CertOriginHtmlBody = "<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\">\r\n" + "<head>\r\n  <title>Certificate of Origin for B00001013</title>\r\n  <style type=\"text/css\">\r\n  <!--\r\n  {0}\r\n  -->\r\n</style>\r\n</head>\r\n" + "<body>\r\n  <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#FFFFFF\">\r\n    <tr>\r\n      <td class=\"banner\"><img src=\"cid:Banner.jpg\" alt=\"Banner Image\" /></td>\r\n    </tr>\r\n    <tr>\r\n      <td class=\"content\">\r\n<br />\r\n" + "<strong>Job Number : B00001013<br />\r\nCertificate Approval Date : 20110524<br />\r\nCertificate Number : 647838<br />\r\nEntry Status : DOK<br />\r\nUnique Reference Number (URN) : XXXXXXXXE01T     201105240149<br />\r\n<br />\r\n</strong>" + "A response message has been received from Singapore Customs.<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n" + "    The message sent for the above mentioned job had the following certificate of origin information.</p>\r\n<br />\r\n<strong>AAZ : Additional Export Information<br />\r\nCCI : Customs Clearance Instructions<br />\r\nDCL : Declaration<br />\r\nTDT : Transport Details Remarks<br />\r\n</strong>\r\n<br />\r\n" + "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" + "<thead><tr class=\"tableheadings\"><th>Type</th><th>Condition Code</th><th>Description</th></tr></thead>" + "<tr><td>CCI</td><td>Z99</td><td>AT ALL TIMES, REFERENCES TO IESGP / CED IN THIS DOCUMENT SHALL MEAN SINGAPORE CUSTOMS.</td></tr>" + "<tr><td>CCI</td><td>A51</td><td>APPROVED BY SINGAPORE CUSTOMS.</td></tr>" + "<tr><td>CCI</td><td>EEE</td><td>END OF CARGO CLEARANCE PERMIT.</td></tr></table>\r\n" + "<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender<br />\r\n<br />\r\n<br />\r\n<br />      </td>\r\n    </tr>\r\n    <tr>\r\n" + "      <td><img src=\"cid:Footer.jpg\" alt=\"Footer Image\" /></td>\r\n    </tr>\r\n  </table>\r\n</body>\r\n</html>\r\n";

		const string InvalidMessageHtmlBody = "FATAL PROCESSING ERROR IN MESSAGE : \r\nTHIS MESSAGE TEXT IS SO WRONG IT IS NOT FUNNY";
	}
}
