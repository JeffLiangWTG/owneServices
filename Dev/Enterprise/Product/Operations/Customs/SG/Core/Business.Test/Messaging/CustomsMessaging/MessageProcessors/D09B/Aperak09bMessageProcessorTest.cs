using System.IO;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	sealed class Aperak09bMessageProcessorTest : SGMessageProcessorTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportErrorM_1()
		{
			var messageText = LoadFromFile("EDI_ERRORM_1.txt");
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationSent, "XXXXXXXXE01T     200609131101", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors, EntryHeader.CH_Status);
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.AmendmentSent, "XXXXXXXXE01T     200609131101", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors, EntryHeader.CH_Status);
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.RefundSent, "XXXXXXXXE01T     200609131101", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.RefundHadSyntaxErrors, EntryHeader.CH_Status);
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.CancellationSent, "XXXXXXXXE01T     200609131101", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors, EntryHeader.CH_Status);
			ErrorReporter.Clear();
		}

		public void TestImportErrorM_2()
		{
			var messageText = "UNH+1+APERAK:D:09B:UN:041+ERRORM'BGM+963+18311960000C        200711098729'ERC+E17001'FTX+AAO+++DUPLICATE DECLARATION NUMBER:0:BGM:0#0'UNT+5+1";
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, "18311960000C        200711098729", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, EntryHeader.CH_Status);
			AssertEquals("", ((Aperak09bMessageProcessor)MessageProcessor).responseEmail.Subject);
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationSent, "18311960000C        200711098729", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors, EntryHeader.CH_Status);
			AssertEquals("Error for " + EntryHeader.Declaration.JE_DeclarationReference, ((Aperak09bMessageProcessor)MessageProcessor).responseEmail.Subject);
			ErrorReporter.Clear();
		}

		public void TestImportErrorStatusQuery()
		{
			var messageText = "UNH+1+APERAK:D:09B:UN:041+STATUS'BGM+963:::AQR+197700341D          201002057824'RFF+MS:DCS4.DCS4001'RFF+MR:SCL1.SCL1004'ERC+CADETAIL'FTX+AAO+++P01 - PLEASE FAX INVOICE WITH THE     UNIQUE REFERENCE NO. TO        ON-LINE PROCESSING UNIT, SINGAPORE CUSTOMS AT 6337 2061 WITHIN 48 HOURS.. FOR ALL ITEMS.'RFF+LI:1'UNT+8+1'";
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationSent, "197700341D          201002057824", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationQuery, EntryHeader.CH_Status);
		}

		public void TestImportErrorStatusQuery1()
		{
			var messageText = "UNH+1+APERAK:D:09B:UN:041+STATUS'BGM+963:::AQR+200303721R          201002036452'RFF+MS:DCS4.DCS4001'RFF+MR:BJA1.BJA1004'ERC+CADETAIL'FTX+AAO+++P01 - PLEASE EXPLAIN WHY INVOICE TO ABU DHABI??. FOR ALL ITEMS.'RFF+LI:1'UNT+8+1'";
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationSent, "200303721R          201002036452", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationQuery, EntryHeader.CH_Status);
		}

		public void TestImportErrorStatusQuery2()
		{
			var messageText = "UNH+1+APERAK:D:09B:UN:041+STATUS'BGM+963:::AQR+199000558G          201002010015'RFF+MS:DCS4.DCS4001'RFF+MR:ROH1.ROH1008'ERC+CADETAIL'FTX+AAO+++P01 - QUERIED BY CONTROLLER OF UNDESIRABLE PUBLICATIONS.  PLEASE FAX INVOICES TO CUP.FAX NO 65773602.. FOR ALL ITEMS.'RFF+LI:1'UNT+8+1'";
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationSent, "199000558G          201002010015", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationQuery, EntryHeader.CH_Status);
		}

		public void TestImportErrorStatusQuery3()
		{
			var messageText = "UNH+1+APERAK:D:09B:UN:041+STATUS'BGM+963:::AQR+199504458R          201003164848'RFF+MS:DCS4.DCS4001'RFF+MR:PHJ1.PHJ1004'ERC+CADETAIL'FTX+AAO+++P02 - PLEASE FAX TO HSA TRADENET (FAX NO. 64789035) PRODUCT   FORMULA/SPECIFIC CHEMICAL COMPOSITION. INDICATE THE IMPORT   DECLARATION UNIQUE REFERENCE NUMBER CLEARLY ON THE   DOCUMENT.. FOR ALL ITEMS.'RFF+LI:1'UNT+8+1'";
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationSent, "199504458R          201003164848", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationQuery, EntryHeader.CH_Status);
		}

		public void TestAmendmentStatusQuery()
		{
			var messageText = "UNH+1+APERAK:D:09B:UN:041+STATUS'BGM+963:::AQR+197700341D          201003180277'RFF+MS:DCS4.DCS4001'RFF+MR:SCL1.SCL1004'RFF+ACW:CA0C210646X'ERC+CRHEADER'FTX+AAO+++PERMIT APPLICATION NOT APPROVED'ERC+CAHEADER'FTX+AAO+++PLEASE CALL - AMENDMENT IS NOT ALLOWED - GOODS DESCRIPTION'ERC+CRHEADER'FTX+AAO+++END - END OF MESSAGES'UNT+12+1'";
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.AmendmentSent, "197700341D          201003180277", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.AmendmentQuery, EntryHeader.CH_Status);
		}

		public void TestCancelStatusQuery()
		{
			var messageText = "UNH+1+APERAK:D:09B:UN:041+STATUS'BGM+963:::AQR+198205400E          201003088677'RFF+MS:DCS4.DCS4001'RFF+MR:BFP1.BFP1007'RFF+ACW:IR0C267743V'ERC+CRHEADER'FTX+AAO+++PERMIT APPLICATION NOT APPROVED'ERC+CAHEADER'FTX+AAO+++PLEASE CALL - CANCELLATION IS NOT ALLOWED - CA/SC CODE 1'ERC+CRHEADER'FTX+AAO+++END - END OF MESSAGES'UNT+12+1'";
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.CancellationSent, "198205400E          201003088677", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.CancellationQuery, EntryHeader.CH_Status);
		}

		public void TestRefundStatusQuery()
		{
			var messageText = "UNH+1+APERAK:D:09B:UN:041+STATUS'BGM+963:::AQR+198205400E          201003088677'RFF+MS:DCS4.DCS4001'RFF+MR:BFP1.BFP1007'RFF+ACW:IR0C267743V'ERC+CRHEADER'FTX+AAO+++PERMIT APPLICATION NOT APPROVED'ERC+CAHEADER'FTX+AAO+++PLEASE CALL - REFUND IS NOT ALLOWED - CA/SC CODE 1'ERC+CRHEADER'FTX+AAO+++END - END OF MESSAGES'UNT+12+1'";
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.RefundSent, "198205400E          201003088677", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.RefundQuery, EntryHeader.CH_Status);
		}

		public void TestStatusAfterQuery()
		{
			var queryMessageText = "UNH+1+APERAK:D:09B:UN:041+STATUS'BGM+963:::AQR+200303721R          201009237825'RFF+MS:DCS4.DCS4001'RFF+MR:BJA1.BJA1004'ERC+CADETAIL'FTX+AAO+++P01 - PLEASE FAX COMMERCIAL INVOICE AND THE END-USER LETTER     WITH THE UNIQUE REFERENCE NO. TO SINGAPORE CUSTOMS AT 6337 2061.. FOR ALL ITEMS.'RFF+LI:1'UNT+8+1'";
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationSent, "200303721R          201009237825", queryMessageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationQuery, EntryHeader.CH_Status);
		}

		public void TestStatusWhenRejectMessageReceivedAfterQueryMessage()
		{
			var rejectionMessageText = "UNH+1+APERAK:D:09B:UN:041+STATUS'BGM+963:::ARD+200303721R          201009237825'RFF+MS:DCS4.DCS4001'RFF+MR:BJA1.BJA1004'ERC+CAREJ'FTX+AAO+++R01 - PERMIT APPLICATION NOT APPROVED. PLEASE VERIFY HS CODES FOR ITEM2 (STAINLESS STEEL FLANGES?:73072100), ITEM 4 (SURE PIPE FITTINGS??, VERIFY HS CODE ACCORDINGLY), ITEM 5 (73079100).. FOR ALL ITEMS.'RFF+LI:1'UNT+8+1'";
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationQuery, "200303721R          201009237825", rejectionMessageText);
			AssertEquals("Reject message received must set status to reject", Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms, EntryHeader.CH_Status);
		}

		public void TestImportStatusR_1()
		{
			var messageText = "UNH+1+APERAK:D:09B:UN:041+STATUS'BGM+963:::ARD+XXXXXXXXE01T     200609139010'RFF+MR:E01T.E01T001'RFF+MS:DCST.DCST201'ERC+CAREJ'FTX+AAO+++PERMIT APPLICATION NOT APPROVED PLS CHECK LICENCE NO LICENCE 1:END OF MESSAGES'UNT+7+1'";
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.DeclarationSent, "XXXXXXXXE01T     200609139010", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms, EntryHeader.CH_Status);
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.AmendmentSent, "XXXXXXXXE01T     200609139010", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms, EntryHeader.CH_Status);
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.RefundSent, "XXXXXXXXE01T     200609139010", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.RefundRejectedByCustoms, EntryHeader.CH_Status);
			AssertProcessValidMessage(Core.SGConstants.DeclarationStatus.CancellationSent, "XXXXXXXXE01T     200609139010", messageText);
			AssertEquals(Core.SGConstants.DeclarationStatus.CancellationRejectedByCustoms, EntryHeader.CH_Status);
		}

		public void TestQueryImpedimentEmail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001001";
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			var trxMessage = Factory.New<EDIMessage>();
			trxMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			trxMessage.EM_ApplicationReference = "199002561R          201001047472";
			trxMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			trxMessage.EM_LinkedObject = entry;
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = "UNH+1+APERAK:D:09B:UN:041+STATUS'BGM+963:::AQR+199002561R          201001047472'RFF+MS:DCS4.DCS4001'RFF+MR:JBK1.JBK1017'ERC+CADETAIL'FTX+AAO+++P07 - PLEASE INDICATE THE CPM IMPORTER?'S LICENCE NUMBER AND   CPM PRODUCT REFERENCE NUMBER TOGETHER WITH THE LOT/BATCH   NUMBER AGAINST EACH PRODUCT CLEARLY ON THE SUPPLIER?'S   INVOICE.. FOR ALL ITEMS.'RFF+LI:1'UNT+8+1'";
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new Aperak09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals("Query Response Email Subject", "Impediment Query from Customs for B00001001", processor.responseEmail.Subject);
			AssertMultilineASCIIEquals("Query Response Email Body Equals", queryEmailHtmlBody, processor.responseEmail.Body);
			ErrorReporter.Clear();
		}

		readonly string queryEmailHtmlBody = string.Format("<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\">\r\n<head>\r\n  <title>Impediment Query from Customs for B00001001</title>\r\n  <style type=\"text/css\">\r\n  <!--\r\n  {0}\r\n  -->\r\n</style>\r\n</head>\r\n<body>\r\n  <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#FFFFFF\">\r\n    <tr>\r\n      <td class=\"banner\"><img src=\"cid:Banner.jpg\" alt=\"Banner Image\" /></td>\r\n    </tr>\r\n    <tr>\r\n      <td class=\"content\">\r\n<br />\r\n<strong>Job Number : B00001001<br />\r\nEntry Status : DQY<br />\r\nUnique Reference Number (URN) : 199002561R          201001047472<br />\r\n<br />\r\n</strong>A response message has been received from Singapore Customs.<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The message sent for the above job has received the following Query Response from Customs.</p>\r\n<br />\r\n<br />\r\n<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Query Detail</th><th>Query Description</th></tr></thead><tr><td>CADETAIL</td><td>P07 - PLEASE INDICATE THE CPM IMPORTER&#39;S LICENCE NUMBER AND   CPM PRODUCT REFERENCE NUMBER TOGETHER WITH THE LOT/BATCH   NUMBER AGAINST EACH PRODUCT CLEARLY ON THE SUPPLIER&#39;S   INVOICE.. FOR ALL ITEMS. </td></tr></table>\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender<br />\r\n<br />\r\n<br />\r\n<br />      </td>\r\n    </tr>\r\n    <tr>\r\n      <td><img src=\"cid:Footer.jpg\" alt=\"Footer Image\" /></td>\r\n    </tr>\r\n  </table>\r\n</body>\r\n</html>\r\n", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
		public void TestInvalidMessage()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = "THIS MESSAGE TEXT IS SO WRONG IT IS NOT FUNNY";
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new Aperak09bMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals("Response Email Subject", "ERROR PROCESSING", processor.responseEmail.Subject);
			AssertMultilineASCIIEquals("Response Email Body Equals", InvalidMessageHtmlBody, processor.responseEmail.Body);
		}

		const string InvalidMessageHtmlBody = "FATAL PROCESSING ERROR IN MESSAGE : \r\nTHIS MESSAGE TEXT IS SO WRONG IT IS NOT FUNNY";

		string LoadFromFile(string fileName)
		{
			return File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\Business.Test\Messaging\CustomsMessaging\MessageProcessors\TestMessages\D09B\" + fileName);
		}

		#region Message Processor
		protected override SGMessageProcessor GetMessageProcessor()
		{
			return new Aperak09bMessageProcessor(new LoggingInformation());
		}
		#endregion
	}
}
