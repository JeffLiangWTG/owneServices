using System;
using System.Drawing;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageProcessors.Testing;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFProcessorTest : ABIProcessorTest<ISFProcessor, APLA, APLB, APLY>
	{
		public void TestBannerAndFooterComeFromJobBranch()
		{
			var notificationForPM = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "E"));
			notificationForPM.GS_EmailAddress = "test@cargowise.com";
			var newBranch = Factory.New<GlbBranch>();
			newBranch.FillWithValidTestData();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var image1 = new Bitmap(1, 2);
			var image2 = new Bitmap(2, 1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty, image2);

			var header = Factory.New<CusISFHeader>();
			header.FillWithValidTestData();
			header.BF_GB = newBranch.PK;
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock
				.Protected()
				.Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			MQEDIMessage outgoingMessage = mock.Object;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "~15000";
			outgoingMessage.EM_MessageText = "B018888XJ5SF                                                                     Y         SF00001000000000000000000000000";
			outgoingMessage.EM_LinkedObject = header;

			MQEDIMessage incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			incomingMessage.EM_MessageNum = "~15000";
			incomingMessage.EM_MessageText =
				"B018888XJ5SN                                               ~15000               " +
				"SF10101RCTEI 13-2559853NY           10OHL-20093668970NGTL13-2559853NY   018  N  " +
				"SF15BMNGTL5461002(A)                                                            " +
				"SF9001156INVALID BILL                                                           " +
				"SF9001   ISF REJECTED                                                           " +
				"Y 8888XJ5SN00004";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ISFIncomingMessageProcessor().ExecuteBatch();

			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var banner = email.Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new Bitmap(new MemoryStream(banner.Data));
			AssertEquals(2, image.Width);
			mock.VerifyAll();
		}

		protected override void EndToEndCore()
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock
				.Protected()
				.Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			MQEDIMessage outgoingMessage = mock.Object;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "~150000";
			outgoingMessage.EM_MessageText = "B018888XJ5SF                                                                     Y         SF00001000000000000000000000000";

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			message.EM_MessageNum = "~150000";

			message.EM_MessageText =
				"B018888XJ5SN                                               ~150000              " +
				"SF10103A  DUN4684446465             11SV9-12345678901SCAS                    Y  " +
				"SF90  308CONSOLIDATOR NAME/ADDRESS REQUIRED                                     " +
				"SF90  303CONSIGNEE NUMBER REQUIRED                                              " +
				"SF15BMBM123456789                                                               " +
				"SF15OBOB123456789                                                               " +
				"SF20SBNSB123456789                                                              " +
				"SF20MB MB123456789                                                              " +
				"SF20V1 V1123456789                                                              " +
				"SF206B 6B123456789                                                              " +
				"SF2543TURE234323         14050                                                  " +
				"SF90  252INVALID EQUIPMENT DESCRIPTION CODE                                     " +
				"SF30BY IMPORTER AUSTRALIAN COMPANY        EI                                    " +
				"SF90  315INVALID ENTITY IDENTIFIER                                              " +
				"SF30MF MANACCOM PTY LTD                   EI                                    " +
				"SF90  315INVALID ENTITY IDENTIFIER                                              " +
				"SF401234568844AU                                                                " +
				"SF90  404INVALID HTS CODE                                                       " +
				"SF30MF MAINFREIGHT INTERNATIONAL             65007252333                        " +
				"SF401042334668US                                                                " +
				"SF90  404INVALID HTS CODE                                                       " +
				"SF9001   SECURITY FILING REJECTED                                               " +
				"Y  8888XJ5SN00021";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ISFIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_ApplicationReference should be updated from message", "SV9-12345678901", message.EM_ApplicationReference);

			outgoingMessage.Reload();
			AssertEquals("EM_ApplicationReference should be updated from incoming message", "SV9-12345678901", outgoingMessage.EM_ApplicationReference);

			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("Importer Security Filing")));
			Assert(email.Recipients.Contains(staffZ1.GS_EmailAddress));
			Assert(email.Recipients.Contains(staffZ2.GS_EmailAddress));
			string expectedBodyMessage = @"<br />
Importer Security Filing Message Result<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>308</td><td>CONSOLIDATOR NAME/ADDRESS REQUIRED (An Add/Replace transaction is submitted with Submission Type 1, Action Reason Code other than FT or FX, and no Consolidators)</td></tr><tr><td>303</td><td>CONSIGNEE NUMBER REQUIRED (An Add/Replace transaction is submitted with Submission Type 1, and no Consignee Number)</td></tr><tr><td>252</td><td>INVALID EQUIPMENT DESCRIPTION CODE (A transaction is submitted with an SF25 equipment record and invalid Equipment Description Code)</td></tr><tr><td>315</td><td>INVALID ENTITY IDENTIFIER (A transaction is submitted with an IOR/Consignee Entity Code and a Entity Identifier Qualifier, but the corresponding Entity Identifier is missing)</td></tr><tr><td>315</td><td>INVALID ENTITY IDENTIFIER (A transaction is submitted with an IOR/Consignee Entity Code and a Entity Identifier Qualifier, but the corresponding Entity Identifier is missing)</td></tr><tr><td>404</td><td>INVALID HTS CODE (A transaction is submitted with an HTS Code that is not on file)</td></tr><tr><td>404</td><td>INVALID HTS CODE (A transaction is submitted with an HTS Code that is not on file)</td></tr><tr><td>&nbsp;</td><td>SECURITY FILING REJECTED</td></tr></table>
<br />";
			AssertContains(expectedBodyMessage, email.Body);
			mock.VerifyAll();
		}

		public void TestUpdateAcceptedDate()
		{
			var header = Factory.New<CusISFHeader>();
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock
				.Protected()
				.Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			MQEDIMessage outgoingMessage = mock.Object;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "~15000";
			outgoingMessage.EM_MessageText = "B018888XJ5SF                                                                     Y         SF00001000000000000000000000000";
			outgoingMessage.EM_LinkedObject = header;

			var incomingInterchange = Factory.New<EDIInterchange>();
			incomingInterchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingInterchange.EI_HeaderText = "A3901SV9      07160901   071609021226                                00004006830";
			incomingInterchange.EI_BodyText = "B018888XJ5SN                                               ~15000               SF10107ACTEI 91-013199123           10XJ5-20094480586AACT               019  Y  SF9003   ISF ACCEPTED WITH WARNINGS                                             Y  8888XJ5SN00003";
			incomingInterchange.EI_FooterText = "Z3901SV9      07160901   071609021227                                00004006830";
			incomingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingInterchange.EI_From = "USC";
			incomingInterchange.EI_To = "SV93902";
			incomingInterchange.EI_InterchangeNum = "~15000";

			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			incomingMessage.EM_MessageNum = "~15000";
			incomingMessage.EM_MessageText = incomingInterchange.EI_BodyText;
			incomingMessage.EM_EI = incomingInterchange.PK;

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ISFIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			header.Reload();
			AssertEquals(new ZDateTime(2009, 07, 16), header.BF_FirstAcceptedDate);
			AssertEquals(new ZDateTime(2009, 07, 16), header.BF_LastAcceptedDate);
			mock.VerifyAll();
		}

		public void TestProcessingError108DoesNotReportToUs()
		{
			string responseData =
				"SF10101R  EI 13-2559853NY           10OHL-20098287317JSCP13-2559853NY        N  " +
				"SF9001108INVALID ISF TRANSACTION NUMBER                                         " +
				"SF15OBJSCPJBTKE0900265                                                          " +
				"SF9001   ISF REJECTED                                                           ";
			string expectedBodyMessage = @"<br />
Importer Security Filing Message Result<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th></tr></thead></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>108</td><td>INVALID ISF TRANSACTION NUMBER (A Replace/Delete transaction is submitted with an ISF Transaction Number that is not associated with any active transactions in that environment)</td></tr><tr><td>&nbsp;</td><td>ISF REJECTED</td></tr></table>
<br />";
			AssertErrorCodeNotReportedToUs(responseData, true, expectedBodyMessage, false);
		}

		public void TestProcessingError113DoesNotReportToUs()
		{
			string responseData =
				"SF10101R  EI 13-2559853NY           10OHL-20098287317JSCP13-2559853NY        N  " +
				"SF9001113INVALID ISF TRANSACTION NUMBER                                         " +
				"SF15OBJSCPJBTKE0900265                                                          " +
				"SF9001   ISF REJECTED                                                           ";
			string expectedBodyMessage = @"<br />
Importer Security Filing Message Result<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th></tr></thead></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>113</td><td>INVALID ISF TRANSACTION NUMBER</td></tr><tr><td>&nbsp;</td><td>ISF REJECTED</td></tr></table>
<br />";
			AssertErrorCodeNotReportedToUs(responseData, true, expectedBodyMessage, false);
		}

		public void TestProcessingError117DoesNotReportToUs()
		{
			string responseData =
				"SF10101A  EI 13-2559853SL           10               APLU13-2559853SL        N  " +
				"SF9001117DUPLICATE ISF TRANSACTION                                              " +
				"SF15OBAPLU058025645                                                             " +
				"SF9001   ISF REJECTED                                                           ";
			string expectedBodyMessage = @"<br />
Importer Security Filing Message Result<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th></tr></thead></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>117</td><td>DUPLICATE ISF TRANSACTION (A stand-alone Add transaction is submitted using an ISF Importer and Bill that was previously submitted in another stand-alone transaction)</td></tr><tr><td>&nbsp;</td><td>ISF REJECTED</td></tr></table>
<br />";
			AssertErrorCodeNotReportedToUs(responseData, true, expectedBodyMessage, false);
		}

		public void TestProcessingError125DoesNotReportToUs()
		{
			string responseData =
				"SF10101ACTEI 25-57-995              10                   25-57-995      018  N  " +
				"SF9001125INVALID ISF IMPORTER NUMBER                                            " +
				"SF9001   ISF REJECTED                                                           ";
			string expectedBodyMessage = @"<br />
Importer Security Filing Message Result<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th></tr></thead></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>125</td><td>INVALID ISF IMPORTER NUMBER (A stand-alone transaction is submitted with an invalid ISF Importer Number format)</td></tr><tr><td>&nbsp;</td><td>ISF REJECTED</td></tr></table>
<br />";
			AssertErrorCodeNotReportedToUs(responseData, true, expectedBodyMessage, false);
		}

		public void TestProcessingError127DoesNotReportToUs()
		{
			string responseData =
				"SF10101ACTEI 25-57-995              10                   25-57-995      018  N  " +
				"SF9003127INVALID ISF BOND HOLDER                                                " +
				"SF9001   ISF REJECTED                                                           ";
			string expectedBodyMessage = @"<br />
Importer Security Filing Message Result<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th></tr></thead></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>127</td><td>INVALID ISF BOND HOLDER (An Add/Replace transaction is submitted with an invalid Bond Holder Number format)</td></tr><tr><td>&nbsp;</td><td>ISF REJECTED</td></tr></table>
<br />";
			AssertErrorCodeNotReportedToUs(responseData, true, expectedBodyMessage, false);
		}

		public void TestProcessingError128DoesNotReportToUs()
		{
			string responseData =
				"SF10101ACTEI 56-2402667-00          11               HJSC56-240266700   019  Y  " +
				"SF9003128INVALID SURETY CODE                                                    " +
				"SF9001   ISF REJECTED                                                           ";
			string expectedBodyMessage = @"<br />
Importer Security Filing Message Result<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th></tr></thead></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>128</td><td>INVALID SURETY CODE (An Add/Replace transaction is submitted with an invalid Surety Code format)</td></tr><tr><td>&nbsp;</td><td>ISF REJECTED</td></tr></table>
<br />";
			AssertErrorCodeNotReportedToUs(responseData, true, expectedBodyMessage, false);
		}

		public void TestProcessingError133DoesNotReportToUs()
		{
			string responseData =
				"SF10101ACTEI 25-57-995              10                   25-57-995      018  N  " +
				"SF9003133INVALID ISF IMPORTER NUMBER                                            " +
				"SF9001   ISF REJECTED                                                           ";
			string expectedBodyMessage = @"<br />
Importer Security Filing Message Result<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th></tr></thead></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>133</td><td>INVALID ISF IMPORTER NUMBER (A stand-alone transaction is submitted with an ISF Importer Number that is not on file)</td></tr><tr><td>&nbsp;</td><td>ISF REJECTED</td></tr></table>
<br />";
			AssertErrorCodeNotReportedToUs(responseData, true, expectedBodyMessage, false);
		}

		public void TestProcessingError154DoesNotReportToUs()
		{
			string responseData =
				"SF10101ACTEI 22-285509300           10               EGLV22-285509300   018  N  " +
				"SF15OBEGLV0500900374116                                                         " +
				"SF9001154INVALID HOUSE OR REGULAR BOL NUMBER                                    " +
				"SF9001   ISF REJECTED                                                           ";
			string expectedBodyMessage = @"<br />
Importer Security Filing Message Result<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th></tr></thead></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>154</td><td>INVALID HOUSE OR REGULAR BOL NUMBER (A transaction is submitted with a House or Regular Bill Number that is an invalid format)</td></tr><tr><td>&nbsp;</td><td>ISF REJECTED</td></tr></table>
<br />";
			AssertErrorCodeNotReportedToUs(responseData, true, expectedBodyMessage, false);
		}

		public void TestProcessingError155DoesNotReportToUs()
		{
			string responseData =
				"SF10101RCTEI 13-496883700           10OHL-20097060237    13-496883700   01   N  " +
				"SF15BMKOSR209ULA18                                                              " +
				"SF20MB  MSCUKR418027                                                            " +
				"SF9001155INVALID SCAC                                                           " +
				"SF9001   ISF REJECTED                                                           ";
			string expectedBodyMessage = @"<br />
Importer Security Filing Message Result<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th></tr></thead></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>155</td><td>INVALID SCAC</td></tr><tr><td>&nbsp;</td><td>ISF REJECTED</td></tr></table>
<br />";
			AssertErrorCodeNotReportedToUs(responseData, true, expectedBodyMessage, false);
		}

		public void TestProcessingError156DoesNotReportToUs()
		{
			string responseData =
				"SF10101RCTEI 13-2559853NY           10OHL-20093668970NGTL13-2559853NY   018  N  " +
				"SF15BMNGTL5461002(A)                                                            " +
				"SF9001156INVALID BILL                                                           " +
				"SF9001   ISF REJECTED                                                           ";
			string expectedBodyMessage = @"<br />
Importer Security Filing Message Result<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th></tr></thead></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>156</td><td>INVALID BILL</td></tr><tr><td>&nbsp;</td><td>ISF REJECTED</td></tr></table>
<br />";
			AssertErrorCodeNotReportedToUs(responseData, true, expectedBodyMessage, false);
		}

		public void TestProcessingError316DoesNotReportToUs()
		{
			string responseData =
				"SF30CN                                    EI 25-57-995                          " +
				"SF9001316INVALID IRS FORMAT IN ENTITY IDENTIFIER                                " +
				"SF9001   ISF REJECTED                                                           ";
			string expectedBodyMessage = @"<br />
Importer Security Filing Message Result<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th></tr></thead></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>316</td><td>INVALID IRS FORMAT IN ENTITY IDENTIFIER (A transaction is submitted with an Entity Identifier Qualifier for IRS #, but the corresponding Entity Identifier is an invalid IRS # format)</td></tr><tr><td>&nbsp;</td><td>ISF REJECTED</td></tr></table>
<br />";
			AssertErrorCodeNotReportedToUs(responseData, true, expectedBodyMessage, false);
		}

		public void TestProcessingError317DoesNotReportToUs()
		{
			string responseData =
				"SF10101ACTEI 76-044778000           10               EGLV76-044778000   018  N  " +
				"SF15OBEGLV003901340574                                                          " +
				"SF30IM                                    ANI75-317523800                       " +
				"SF9001317INVALID ANI FORMAT IN ENTITY IDENTIFIER                                " +
				"SF9001   ISF REJECTED                                                           ";
			string expectedBodyMessage = @"<br />
Importer Security Filing Message Result<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th></tr></thead></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>317</td><td>INVALID ANI FORMAT IN ENTITY IDENTIFIER (A transaction is submitted with an Entity Identifier Qualifier for CBP Assigned Number, but the corresponding Entity Identifier is an invalid CBP Assigned Number format)</td></tr><tr><td>&nbsp;</td><td>ISF REJECTED</td></tr></table>
<br />";
			AssertErrorCodeNotReportedToUs(responseData, true, expectedBodyMessage, false);
		}

		public void TestProcessingError321DoesNotReportToUs()
		{
			string responseData =
				"SF10101ACTEI 76-044778000           10               EGLV76-044778000   018  N  " +
				"SF15OBEGLV003901340574                                                          " +
				"SF30BY GEOSPACE TECHNOLOGIES, LP          DUN165958245                          " +
				"SF9001321INVALID DUNS NUMBER IN ENTITY IDENTIFIER                               " +
				"SF9001   ISF REJECTED                                                           ";
			string expectedBodyMessage = @"<br />
Importer Security Filing Message Result<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th></tr></thead></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>321</td><td>INVALID DUNS NUMBER IN ENTITY IDENTIFIER</td></tr><tr><td>&nbsp;</td><td>ISF REJECTED</td></tr></table>
<br />";
			AssertErrorCodeNotReportedToUs(responseData, true, expectedBodyMessage, false);
		}

		public void TestProcessingError329DoesNotReportToUs()
		{
			string responseData =
				"SF30CN                                    EI 14-834967800                       " +
				"SF9003329INVALID ENTITY IDENTIFIER                                              " +
				"SF9003   ISF ACCEPTED WITH WARNINGS                                             ";
			string expectedBodyMessage = @"<br />
Importer Security Filing Message Result<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th></tr></thead></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>329</td><td>INVALID ENTITY IDENTIFIER (A transaction is submitted with an Entity Identifier Qualifier for IRS#, CBP Assigned Number or SSN; but the corresponding Entity Identifier is not on file)</td></tr><tr><td>&nbsp;</td><td>ISF ACCEPTED WITH WARNINGS</td></tr></table>
<br />";
			AssertErrorCodeNotReportedToUs(responseData, false, expectedBodyMessage, false);
		}

		public void TestProcessingAcceptWithWarningsIsTreatedAsError()
		{
			string responseData =
				"SF30CN                                    EI 14-834967800                       " +
				"SF9003329INVALID ENTITY IDENTIFIER                                              " +
				"SF9003   ISF ACCEPTED WITH WARNINGS                                             ";
			string expectedBodyMessage = @"<br />
Importer Security Filing Message Result<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th></tr></thead></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>329</td><td>INVALID ENTITY IDENTIFIER (A transaction is submitted with an Entity Identifier Qualifier for IRS#, CBP Assigned Number or SSN; but the corresponding Entity Identifier is not on file)</td></tr><tr><td>&nbsp;</td><td>ISF ACCEPTED WITH WARNINGS</td></tr></table>
<br />";
			AssertErrorCodeNotReportedToUs(responseData, false, expectedBodyMessage, true);
		}

		void AssertErrorCodeNotReportedToUs(string responseData, bool isFailure, string expectedBodyMessage, bool originalSendWithErrors)
		{
			var header = Factory.New<CusISFHeader>();
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock
				.Protected()
				.Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			MQEDIMessage outgoingMessage = mock.Object;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "~15000";
			outgoingMessage.EM_MessageText = "B018888XJ5SF                                                                     Y         SF00001000000000000000000000000";
			outgoingMessage.EM_LinkedObject = header;
			outgoingMessage.EM_SendWithMessageErrors = originalSendWithErrors;

			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			incomingMessage.EM_MessageNum = "~15000";
			incomingMessage.EM_MessageText =
				"B018888XJ5SN                                               ~15000".PadRight(80) +
				responseData +
				"Y 8888XJ5SN00004";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ISFIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);

			string subject = string.Format("Importer Security Filing Response {0}for {1}", isFailure ? "(Failure) " : "", header.HumanReadableName);
			EmailDef email1 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == subject));
			Assert(email1.Recipients.Contains(staffZ1.GS_EmailAddress));
			Assert(email1.Recipients.Contains(staffZ2.GS_EmailAddress));
			AssertContains(expectedBodyMessage, email1.Body);
			AssertEquals("", ErrorReporter.LastKeyReported);
			mock.VerifyAll();
		}

		protected override void SetUp()
		{
			base.SetUp();
			ISFRegistry.Instance.ImporterSecurityFilingMessagesGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupZZ1.PK.ToGuid());
		}

		public void TestProcessingEmails_WhenParentHasHVLTRFLog_UseRegistryHVLVImporterSecurityFilingMessagesGroup()
		{
			var responseData =
	"SF30CN                                    EI 14-834967800                       " +
	"SF9003329INVALID ENTITY IDENTIFIER                                              " +
	"SF9003   ISF ACCEPTED WITH WARNINGS                                             ";

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "KNZ";

			var hvlvRecipient = group.Staff.AddNew();
			hvlvRecipient.GS_Code = "K1";
			hvlvRecipient.GS_LoginName = "K1";
			hvlvRecipient.GS_EmailAddress = "k1@k1.email.com";
			Factory.Save();

			ISFRegistry.Instance.HVLVImporterSecurityFilingMessagesGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, group.PK, false));

			var header = Factory.New<CusISFHeader>();
			((ZArchitecture.Business.IStmALogProvider)header).Logs.AddNew(ZArchitecture.Business.AutoEvents.Transferred, "|TYP=HVL");
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock
				.Protected()
				.Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoingMessage = mock.Object;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "~15000";
			outgoingMessage.EM_MessageText = "B018888XJ5SF                                                                     Y         SF00001000000000000000000000000";
			outgoingMessage.EM_LinkedObject = header;

			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			incomingMessage.EM_MessageNum = "~15000";
			incomingMessage.EM_MessageText =
				"B018888XJ5SN                                               ~15000".PadRight(80) +
				responseData +
				"Y 8888XJ5SN00004";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ISFIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Contains recipient from the HVLVImporterSecurityFilingMessagesGroup", true, email.Recipients.Contains("k1@k1.email.com"));
				AssertEquals("Does not contain staffZ1's email", true, !email.Recipients.Contains("dong@pretend.email.com"));
				AssertEquals("Does not contain staffZ2's email", true, !email.Recipients.Contains("dong2@pretend.email.com"));
			});
			mock.VerifyAll();
		}
	}
}
