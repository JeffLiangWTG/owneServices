using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.MHUB;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor.Testing
{
	public class BatchSGCInterchangeRetriever30Test : TestCaseWithFactory
	{
		public void TestExecute()
		{
			GlbStaff broker = Factory.New<GlbStaff>();
			broker.GS_IsActive = true;
			broker.GS_Code = "TS1";
			broker.GS_LoginName = "Test 1";
			var wrapper = SGGlbStaffWrapper.Get(broker);
			wrapper.Tradenetv4Password.GP_UserID = "v13t000";
			wrapper.Tradenetv4Password.CurrentDecryptedPassword = "Current";
			wrapper.Tradenetv4Password.GP_PasswordStatus = Core.Constants.PasswordOK;
			GlbStaff broker2 = Factory.New<GlbStaff>();
			broker2.GS_IsActive = true;
			broker2.GS_Code = "TS2";
			broker2.GS_LoginName = "Test 2";
			var wrapper2 = SGGlbStaffWrapper.Get(broker2);
			wrapper2.Tradenetv4Password.GP_UserID = "cwise";
			wrapper2.Tradenetv4Password.CurrentDecryptedPassword = "Current";
			wrapper2.Tradenetv4Password.GP_PasswordStatus = Core.Constants.PasswordOK;
			Factory.Save();
			var mock = new Mock<BatchSGCInterchangeRetrieverTestClass>();
			mock.CallBase = true;
			mock.Protected().Setup<bool>("IsEnvironmentDataValid").Returns(true);
			mock.Protected().Setup<bool>("TestConnection").Returns(true);
			BatchSGCInterchangeRetrieverTestClass retriever = mock.Object;
			retriever.ExecuteBatch();
			AssertEquals(true, retriever.Logger.DebugLogStrings.Count > 0);
			var logDetails = ZString.Empty;
			foreach (var log in retriever.Logger.DebugLogStrings)
			{
				logDetails += log;
			}

			AssertEquals(true, logDetails.Contains("Retrieving..."));
			AssertEquals(true, logDetails.Contains("Testing Connection..."));
			AssertEquals(true, logDetails.Contains("Retrieving Complete"));
			mock.VerifyAll();
		}

		public void TestMultipleReferenceInterchangesWithSameNumberProcess()
		{
			CombineAssertions(() =>
			{
				var broker = Factory.New<GlbStaff>();
				broker.GS_IsActive = true;
				broker.GS_Code = "TS1";
				broker.GS_LoginName = "Test 1";
				var wrapper = SGGlbStaffWrapper.Get(broker);
				wrapper.Tradenetv4Password.GP_UserID = "v13t000";
				wrapper.Tradenetv4Password.CurrentDecryptedPassword = "Current";
				wrapper.Tradenetv4Password.GP_PasswordStatus = Core.Constants.PasswordOK;
				var interchange1 = CreateInterchange("UNA:+.? 'UNB+UNOA:4+DCST.DCST401:ZZ+V13T.V13T002:ZZ+20180330:0826+1++CLASET'", clasetTxt1, "UNZ+1+1'");
				var interchange2 = CreateInterchange("UNA:+.? 'UNB+UNOA:4+DCST.DCST401:ZZ+V13T.V13T002:ZZ+20180330:1231+1++CLASET'", clasetTxt2, "UNZ+1+1'");
				var interchange3 = CreateInterchange("UNA:+.? 'UNB+UNOA:4+DCST.DCST401:ZZ+V13T.V13T002:ZZ+20180330:1240+1++CLASET'", clasetTxt3, "UNZ+1+1'");
				Factory.Save();
				var mock = new Mock<BatchSGCInterchangeRetrieverTestClass>();
				mock.CallBase = true;
				mock.Protected().Setup<bool>("IsEnvironmentDataValid").Returns(true);
				mock.Protected().Setup<bool>("TestConnection").Returns(true);
				BatchSGCInterchangeRetrieverTestClass retriever = mock.Object;
				retriever.ExecuteBatch();
				AssertEquals(true, retriever.Logger.DebugLogStrings.Count > 0);
				var logDetails = ZString.Empty;
				foreach (var log in retriever.Logger.DebugLogStrings)
				{
					logDetails += log;
				}

				AssertEquals(true, logDetails.Contains("Retrieving..."));
				var interchangeCount = Factory.GetDatabaseCount(typeof(EDIInterchange));
				//process data first time
				var logger = new LoggingInformation();
				AssertInterchangeIsCreated("UNA:+.? 'UNB+UNOA:4+DCST.DCST401:ZZ+V13T.V13T002:ZZ+20180330:0826+1++CLASET'" + clasetTxt1 + "UNZ+1+1'", logger);
				AssertInterchangeIsCreated("UNA:+.? 'UNB+UNOA:4+DCST.DCST401:ZZ+V13T.V13T002:ZZ+20180330:1231+1++CLASET'" + clasetTxt2 + "UNZ+1+1'", logger);
				AssertInterchangeIsCreated("UNA:+.? 'UNB+UNOA:4+DCST.DCST401:ZZ+V13T.V13T002:ZZ+20180330:1240+1++CLASET'" + clasetTxt3 + "UNZ+1+1'", logger);
				Factory.Save();
				AssertEquals("3 new interchanges added.", interchangeCount + 3, Factory.GetDatabaseCount(typeof(EDIInterchange)));
				//assert data processed again will generate new interchanges
				AssertInterchangeIsCreated("UNA:+.? 'UNB+UNOA:4+DCST.DCST401:ZZ+V13T.V13T002:ZZ+20180330:0826+1++CLASET'" + clasetTxt1 + "UNZ+1+1'", logger);
				AssertInterchangeIsCreated("UNA:+.? 'UNB+UNOA:4+DCST.DCST401:ZZ+V13T.V13T002:ZZ+20180330:1231+1++CLASET'" + clasetTxt2 + "UNZ+1+1'", logger);
				AssertInterchangeIsCreated("UNA:+.? 'UNB+UNOA:4+DCST.DCST401:ZZ+V13T.V13T002:ZZ+20180330:1240+1++CLASET'" + clasetTxt3 + "UNZ+1+1'", logger);
				AssertEquals("3 new interchanges should be added again.", interchangeCount + 6, Factory.GetDatabaseCount(typeof(EDIInterchange)));
				mock.VerifyAll();
			}

			);
		}

		public void TestProcessInterchangeWithXmlMessage()
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_IsActive = true;
			broker.GS_Code = "TS1";
			broker.GS_LoginName = "Test 1";
			var wrapper = SGGlbStaffWrapper.Get(broker);
			wrapper.Tradenetv4Password.GP_UserID = "v13t000";
			wrapper.Tradenetv4Password.CurrentDecryptedPassword = "Current";
			wrapper.Tradenetv4Password.GP_PasswordStatus = Core.Constants.PasswordOK;
			CreateInterchange("", xmlErrorResponse1, "", true);
			Factory.Save();
			var mock = new Mock<BatchSGCInterchangeRetrieverTestClass>();
			mock.CallBase = true;
			mock.Protected().Setup<bool>("IsEnvironmentDataValid").Returns(true);
			mock.Protected().Setup<bool>("TestConnection").Returns(true);
			BatchSGCInterchangeRetrieverTestClass retriever = mock.Object;
			retriever.ExecuteBatch();
			AssertEquals(true, retriever.Logger.DebugLogStrings.Count > 0);
			var logDetails = ZString.Empty;
			foreach (var log in retriever.Logger.DebugLogStrings)
			{
				logDetails += log;
			}

			AssertEquals(true, logDetails.Contains("Retrieving..."));
			var interchangeCount = Factory.GetDatabaseCount(typeof(EDIInterchange));
			var logger = new LoggingInformation();
			AssertInterchangeIsCreated(xmlErrorResponse1, logger);
			Factory.Save();
			AssertEquals("1 new interchanges added.", interchangeCount + 1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			mock.VerifyAll();
		}

		[TestDate(2020, 08, 01)]
		[TestDateIncremental(milliseconds: 1)]
		public void TestProcessMultipleResponsesToSameURN()
		{
			var expectedDateToUse = ZDateTime.UtcNow.Date.ToString("yyyyMMdd");
			var broker = Factory.New<GlbStaff>();
			broker.GS_IsActive = true;
			broker.GS_Code = "TS1";
			broker.GS_LoginName = "Test 1";
			var wrapper = SGGlbStaffWrapper.Get(broker);
			wrapper.Tradenetv4Password.GP_UserID = "v13t000";
			wrapper.Tradenetv4Password.CurrentDecryptedPassword = "Current";
			wrapper.Tradenetv4Password.GP_PasswordStatus = Core.Constants.PasswordOK;
			CreateInterchange("", sameURNResponse1, "", true);
			CreateInterchange("", sameURNResponse2, "", true);
			Factory.Save();
			var mock = new Mock<BatchSGCInterchangeRetrieverTestClass>();
			mock.CallBase = true;
			mock.Protected().Setup<bool>("IsEnvironmentDataValid").Returns(true);
			mock.Protected().Setup<bool>("TestConnection").Returns(true);
			BatchSGCInterchangeRetrieverTestClass retriever = mock.Object;
			retriever.ExecuteBatch();
			AssertEquals(true, retriever.Logger.DebugLogStrings.Count > 0);
			var logDetails = ZString.Empty;
			foreach (var log in retriever.Logger.DebugLogStrings)
			{
				logDetails += log;
			}

			AssertEquals(true, logDetails.Contains("Retrieving..."));
			var interchangeCount = Factory.GetDatabaseCount(typeof(EDIInterchange));
			var logger = new LoggingInformation();
			AssertInterchangeIsCreated(sameURNResponse1, logger);
			AssertInterchangeIsCreated(sameURNResponse2, logger);
			Factory.Save();
			AssertEquals("2 new interchanges for same URN added.", interchangeCount + 2, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchangesGenerated = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.SGCustomsTradenetXML));
			var interchangeNumPreffix = expectedDateToUse + "5326-" + expectedDateToUse;
			var response1IntNum = interchangesGenerated[0].EI_InterchangeNum;
			var response2IntNum = interchangesGenerated[1].EI_InterchangeNum;
			AssertEquals("Interchange1 Num starts with date & seq no", true, response1IntNum.StartsWith(interchangeNumPreffix));
			AssertEquals("Interchange2 Num starts with date & seq no", true, response2IntNum.StartsWith(interchangeNumPreffix));
			Assert("Interchanges should have been created with unique Interchange numbers", response1IntNum != response2IntNum);
			mock.VerifyAll();
		}

		#region BatchSGCInterchangeRetrieverTestClass
		public class BatchSGCInterchangeRetrieverTestClass : BatchSGCInterchangeRetriever30
		{
			protected override LoginCommand CheckBrokerMailbox(GlbStaff broker)
			{
				var loginCommand = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
				loginCommand.LoginState = LoginCommand.LoginStateType.LoggedIn;
				return loginCommand;
			}

			protected override void Logout(LoginCommand loginCommand)
			{
			}

			protected override void LoginAndRetrieveAndLogout(GlbStaff broker, CancellationToken token)
			{
			}
		}

		#endregion
		EDIInterchange CreateInterchange(string headerText, string bodyText, string footerText, bool hasXmlMessage = false)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "DCST.DCST401";
			interchange.EI_To = "V13T.V13T001";
			interchange.EI_HeaderText = hasXmlMessage ? "" : headerText;
			interchange.EI_BodyText = bodyText;
			interchange.EI_FooterText = hasXmlMessage ? "" : footerText;
			return interchange;
		}

		void AssertInterchangeIsCreated(string interchangeData, LoggingInformation logger)
		{
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(interchangeData);
					sw.Flush();
				}

				var savedOk = BatchSGCInterchangeRetriever30.TryAddInterchange(testFileName.Filename, logger);
				var message = "Assert saved OK, but not, the following messages may be helpful: " + System.Environment.NewLine + "ErrorReporter.LastMessageReported: " + ErrorReporter.LastMessageReported + System.Environment.NewLine + "logger.DebugLogStrings: " + string.Join(System.Environment.NewLine, logger.DebugLogStrings.ToList<string>());
				AssertEquals(message, true, savedOk);
				Thread.Sleep(10); // to force the NumberFountain updated.
			}
		}

		readonly string clasetTxt1 = @"UNH+F1|01000+CLASET:D:09B:UN:041+CUSCOM'BGM+273++9'RFF+MS:DCST.DCST401'RFF+RE:01000'DTM+771:20180329:102'RFF+ACW:01000'DTM+771:20180329:102'VLI+CHSCODE:AK'SCD+2+HC201803261757350000'STS++23'DTM+7:20180326175924SST:304'DTM+36:99991231235959SST:304'ATT+ZZZ++01012100'CAV+::SC'FTX+ADF+++PURE BRED BREEDING HORSES (NMB)'IDE+1'ATT+ZZZ'CAV+::SC'FTX+ADE+++IM:AGRI-FOOD & VETERINARY AUTHORITY (AVA) QUARANTINE & INSPECTION DEPARTMENT - AVA (ANIMAL) - AVA (ANI)'FTX+ADE+++EX:AGRI-FOOD & VETERINARY AUTHORITY (AVA) QUARANTINE & INSPECTION DEPARTMENT - AVA (ANIMAL) - AVA (ANI)'FTX+ADE+++TP:AGRI-FOOD & VETERINARY AUTHORITY (AVA) QUARANTINE & INSPECTION DEPARTMENT - AVA (ANIMAL) - AVA (ANI)'FTX+ADE+++UOM:NMB'SCD+2+HC201803261757350000'STS++23'DTM+7:20180326175924SST:304'DTM+36:99991231235959SST:304'ATT+ZZZ++01012900'CAV+::SC'FTX+ADF+++OTHER LIVE HORSES (NMB)'IDE+1'ATT+ZZZ'CAV+::SC'FTX+ADE+++IM:AGRI-FOOD & VETERINARY AUTHORITY (AVA) QUARANTINE & INSPECTION DEPARTMENT - AVA (ANIMAL) - AVA (ANI)'FTX+ADE+++EX:AGRI-FOOD & VETERINARY AUTHORITY (AVA) QUARANTINE & INSPECTION DEPARTMENT - AVA (ANIMAL) - AVA (ANI)'FTX+ADE+++TP:AGRI-FOOD & VETERINARY AUTHORITY (AVA) QUARANTINE & INSPECTION DEPARTMENT - AVA (ANIMAL) - AVA (ANI)'FTX+ADE+++UOM:NMB'UNT+20052+F1|01000'";
		readonly string clasetTxt2 = @"UNH+F2|01000+CLASET:D:09B:UN:041+CUSCOM'BGM+273++9'RFF+MS:DCST.DCST401'RFF+RE:01000'DTM+771:20180329:102'RFF+ACW:01000'DTM+771:20180329:102'VLI+CHSCODE:AK'SCD+2+HC201803261757490000'STS++23'DTM+7:20180326180026SST:304'DTM+36:99991231235959SST:304'ATT+ZZZ++29329200'CAV+::SC'FTX+ADF+++1-(1-3-BENZODIOXOL-5-YL) PROPAN-2-ONE (KGM)'IDE+1'ATT+ZZZ'CAV+::SC'FTX+ADE+++IM:CENTRAL NARCOTICS BUREAU (CNB)'FTX+ADE+++EX:CENTRAL NARCOTICS BUREAU (CNB)'FTX+ADE+++TP:CENTRAL NARCOTICS BUREAU (CNB)'FTX+ADE+++UOM:KGM'SCD+2+HC201803261757490000'STS++23'DTM+7:20180326180026SST:304'DTM+36:99991231235959SST:304'ATT+ZZZ++29329300'CAV+::SC'FTX+ADF+++PIPERONAL (KGM)'IDE+1'ATT+ZZZ'CAV+::SC'FTX+ADE+++IM:CENTRAL NARCOTICS BUREAU (CNB)'FTX+ADE+++EX:CENTRAL NARCOTICS BUREAU (CNB)'FTX+ADE+++TP:CENTRAL NARCOTICS BUREAU (CNB)'FTX+ADE+++UOM:KGM'SCD+2+HC201803261757490000'STS++23'DTM+7:20180326180026SST:304'DTM+36:99991231235959SST:304'ATT+ZZZ++29329400'CAV+::SC'FTX+ADF+++SAFROLE (KGM)'IDE+1'ATT+ZZZ'CAV+::SC'FTX+ADE+++IM:CENTRAL NARCOTICS BUREAU (CNB)'FTX+ADE+++EX:CENTRAL NARCOTICS BUREAU (CNB)'FTX+ADE+++TP:CENTRAL NARCOTICS BUREAU (CNB)'FTX+ADE+++UOM:KGM'UNT+10044+F2|01000'";
		readonly string clasetTxt3 = @"UNH+F1|01000+CLASET:D:09B:UN:041+CUSCOM'BGM+273++9'RFF+MS:DCST.DCST401'RFF+RE:01000'DTM+771:20180329:102'RFF+ACW:01000'DTM+771:20180329:102'VLI+CPDCODE:AK'SCD+2+PC201803291459390000'STS++23'DTM+7:20180329145940SST:304'DTM+36:99991231235959SST:304'ATT+ZZZ++ANEPCS003'CAV+::SC'FTX+ADF+++BARIUM NITRATE'ATT+ZZZ++28342990'IDE+1'ATT+ZZZ'CAV+::SC'FTX+ADE+++CD:AED/AED/AED'FTX+ADE+++DES:POLICE LICENSING AND REGULATORY DEPARTMENT(PLRD) ARMS & EXPLOSIVES (A&E)/POLICE LICENSING AND REGULATORY DEPARTMENT(PLRD) ARMS & EXPLOSIVES (A&E)/POLICE LICENSING AND REGULATORY DEPARTMENT(PLRD) ARMS & EXPLOSIVES (A&E)'FTX+ADE+++CTL:IM/EX/TP'FTX+ADE+++UOM:KGM'SCD+2+PC201803291459400000'STS++23'DTM+7:20180329145940SST:304'DTM+36:99991231235959SST:304'ATT+ZZZ++ANEXPL012'CAV+::SC'FTX+ADF+++STEERING WHEELS WITH AIRBAG ASSEMBLIES CONTAINING EXPLOSIVES, FOR VEHICLES OF HEADING 87.01'ATT+ZZZ++87089411'IDE+1'ATT+ZZZ'CAV+::SC'FTX+ADE+++CD:AED/AED/AED'FTX+ADE+++DES:POLICE LICENSING AND REGULATORY DEPARTMENT(PLRD) ARMS & EXPLOSIVES (A&E)/POLICE LICENSING AND REGULATORY DEPARTMENT(PLRD) ARMS & EXPLOSIVES (A&E)/POLICE LICENSING AND REGULATORY DEPARTMENT(PLRD) ARMS & EXPLOSIVES (A&E)'FTX+ADE+++CTL:IM/EX/TP'FTX+ADE+++UOM:-'SCD+2+PC201803291459390000'STS++23'DTM+7:20180329145940SST:304'DTM+36:99991231235959SST:304'ATT+ZZZ++ANEPCS010'CAV+::SC'FTX+ADF+++SODIUM CHLORATE'ATT+ZZZ++28291100'IDE+1'ATT+ZZZ'CAV+::SC'FTX+ADE+++CD:AED/AED/AED'FTX+ADE+++DES:POLICE LICENSING AND REGULATORY DEPARTMENT(PLRD) ARMS & EXPLOSIVES (A&E)/POLICE LICENSING AND REGULATORY DEPARTMENT(PLRD) ARMS & EXPLOSIVES (A&E)/POLICE LICENSING AND REGULATORY DEPARTMENT(PLRD) ARMS & EXPLOSIVES (A&E)'FTX+ADE+++CTL:IM/EX/TP'FTX+ADE+++UOM:KGM'UNT+1614+F1|01000'";
		readonly string xmlErrorResponse1 = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<TradenetResponse instanceIdentifier=""91424946"" dateTime=""201102140001"" xmlns:cux=""urn:crimsonlogic:tn:schema:xsd:CustomsExchangeRate"" xmlns:fee=""urn:crimsonlogic:tn:schema:xsd:FeeMessage"" xmlns:err=""urn:crimsonlogic:tn:schema:xsd:ErrorMessage"" xmlns:rej=""urn:crimsonlogic:tn:schema:xsd:RejectionMessage"" xmlns:app=""urn:crimsonlogic:tn:schema:xsd:ApprovalMessage"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetResponse"">
	<cbc:MessageVersion>041</cbc:MessageVersion>
	<cbc:SenderID>DCST.DCST401</cbc:SenderID>
	<cbc:RecipientID>E01T.E01T001</cbc:RecipientID>
	<cbc:TotalNumberOfResponse>1</cbc:TotalNumberOfResponse>
	<OutboundMessage>
		<err:ErrorMessage>
			<cbc:MessageReference>1</cbc:MessageReference>
			<cac:UniqueReferenceNumber>
				<cbc:ID>XXXXXXXXE01T</cbc:ID>
				<cbc:Date>20110214</cbc:Date>
				<cbc:SequenceNumeric>1101</cbc:SequenceNumeric>
			</cac:UniqueReferenceNumber>
			<cbc:CommonAccessReference>ERRORM</cbc:CommonAccessReference>
			<cac:ErrorDetail>
				<cbc:ErrorCode>E17007</cbc:ErrorCode>
				<cbc:ErrorDescription>PORT OF LOADING DOES NOT EXIST IN THE DATABASE</cbc:ErrorDescription>
				<cbc:ErrorTrace>TradenetDeclaration:1</cbc:ErrorTrace>
				<cbc:ErrorTrace>InboundMessage:1</cbc:ErrorTrace>
				<cbc:ErrorTrace>inp:InNonPaymentUpdate:1</cbc:ErrorTrace>
				<cbc:ErrorTrace>inp:Declaration:1</cbc:ErrorTrace>
				<cbc:ErrorTrace>inp:Transport:1</cbc:ErrorTrace>
				<cbc:ErrorTrace>cac:InwardTransport:1</cbc:ErrorTrace>
				<cbc:ErrorTrace>cbc:LoadingPort:1</cbc:ErrorTrace>
			</cac:ErrorDetail>
		</err:ErrorMessage>
	</OutboundMessage>
</TradenetResponse>";
		readonly string sameURNResponse1 = @"<TradenetResponse xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:app=""urn:crimsonlogic:tn:schema:xsd:ApprovalMessage"" dateTime=""202009210051"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:rej=""urn:crimsonlogic:tn:schema:xsd:RejectionMessage"" xmlns:fee=""urn:crimsonlogic:tn:schema:xsd:FeeMessage"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetResponse"" xmlns:err=""urn:crimsonlogic:tn:schema:xsd:ErrorMessage"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:com=""urn:crimsonlogic:tn:schema:xsd:CustomsCommonCode"" xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" xmlns:cux=""urn:crimsonlogic:tn:schema:xsd:CustomsExchangeRate"" instanceIdentifier=""91280679""><cbc:MessageVersion>041</cbc:MessageVersion><cbc:SenderID>DCST.DCST401</cbc:SenderID><cbc:RecipientID>F01T.F01T007</cbc:RecipientID><cbc:TotalNumberOfResponse>1</cbc:TotalNumberOfResponse><OutboundMessage><rej:RejectionMessage><cbc:MessageReference>1</cbc:MessageReference><cac:UniqueReferenceNumber><cbc:ID>198800784N</cbc:ID><cbc:Date>20200921</cbc:Date><cbc:SequenceNumeric>5326</cbc:SequenceNumeric></cac:UniqueReferenceNumber><cbc:IssuingAuthorityID>DCST.DCST401</cbc:IssuingAuthorityID><cbc:DeclarantID>F01T.F01T007</cbc:DeclarantID><cbc:CommonAccessReference>STATUS</cbc:CommonAccessReference><cbc:StatusType>AQR</cbc:StatusType><cbc:PermitNumber>DP0I105282X</cbc:PermitNumber><cac:RejectionDetail><cbc:ErrorID>CAHEADER</cbc:ErrorID><cbc:ErrorDescription>PLEASE SUBMIT SUPPORTING DOCUMENTS BY EMAIL TO CUSTOMS_REFUND@CUSTOMS.GOV.SG OR FAX TO 63552156.</cbc:ErrorDescription></cac:RejectionDetail></rej:RejectionMessage></OutboundMessage></TradenetResponse>";
		readonly string sameURNResponse2 = @"<TradenetResponse xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:app=""urn:crimsonlogic:tn:schema:xsd:ApprovalMessage"" dateTime=""202009211158"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:rej=""urn:crimsonlogic:tn:schema:xsd:RejectionMessage"" xmlns:fee=""urn:crimsonlogic:tn:schema:xsd:FeeMessage"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetResponse"" xmlns:err=""urn:crimsonlogic:tn:schema:xsd:ErrorMessage"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:com=""urn:crimsonlogic:tn:schema:xsd:CustomsCommonCode"" xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" xmlns:cux=""urn:crimsonlogic:tn:schema:xsd:CustomsExchangeRate"" instanceIdentifier=""91280694""><cbc:MessageVersion>041</cbc:MessageVersion><cbc:SenderID>DCST.DCST401</cbc:SenderID><cbc:RecipientID>F01T.F01T007</cbc:RecipientID><cbc:TotalNumberOfResponse>1</cbc:TotalNumberOfResponse><OutboundMessage><ipt:InPaymentUpdatePermit><cac:Update><cbc:UpdateIndicatorCode>PRS</cbc:UpdateIndicatorCode><cbc:UpdateRequestNumber>1</cbc:UpdateRequestNumber><cac:Refund><cbc:RefundReferenceNumber>RFM2020090895</cbc:RefundReferenceNumber><cbc:ReasonCode>RF35</cbc:ReasonCode><cbc:Reason>TEST</cbc:Reason></cac:Refund></cac:Update><cac:RefundOnly><cac:RefundHeader><cbc:MessageReference>WTGB00008810</cbc:MessageReference><cac:UniqueReferenceNumber><cbc:ID>198800784N</cbc:ID><cbc:Date>20200921</cbc:Date><cbc:SequenceNumeric>5326</cbc:SequenceNumeric></cac:UniqueReferenceNumber><cbc:CommonAccessReference>IPTUPT</cbc:CommonAccessReference><cbc:DeclarationIndicator>true</cbc:DeclarationIndicator></cac:RefundHeader><cac:DeclarantParty><cac:PersonInformation><cbc:CodeValue>P0111116</cbc:CodeValue><cbc:Name>QA TEST USER 1</cbc:Name></cac:PersonInformation><cbc:Telephone>+6588889999</cbc:Telephone></cac:DeclarantParty><cac:RefundItem><cbc:ItemSequenceNumeric>1</cbc:ItemSequenceNumeric><cbc:ItemHarmonizedSystemCode>24013090</cbc:ItemHarmonizedSystemCode><cac:TariffRefund><cbc:ExciseDutyRefundAmount>1000.00</cbc:ExciseDutyRefundAmount></cac:TariffRefund></cac:RefundItem><cac:RefundSummary><cac:TotalTariffRefund><cbc:TotalExciseDutyRefundAmount>1000.00</cbc:TotalExciseDutyRefundAmount></cac:TotalTariffRefund></cac:RefundSummary></cac:RefundOnly><cac:Permit><cbc:PermitNumber>DP0I105282X</cbc:PermitNumber><cbc:PermitApprovalDatetime>20200921115818SST</cbc:PermitApprovalDatetime><cac:RefundApprovalCondition><cbc:ConditionCode>A00</cbc:ConditionCode><cbc:ConditionDescription>APPROVED BY SINGAPORE CUSTOMS.</cbc:ConditionDescription></cac:RefundApprovalCondition><cac:RefundApprovalCondition><cbc:ConditionCode>C02</cbc:ConditionCode><cbc:ConditionDescription>REFUND APPROVED BY SINGAPORE CUSTOMS.</cbc:ConditionDescription></cac:RefundApprovalCondition></cac:Permit></ipt:InPaymentUpdatePermit></OutboundMessage></TradenetResponse>";
	}
}
