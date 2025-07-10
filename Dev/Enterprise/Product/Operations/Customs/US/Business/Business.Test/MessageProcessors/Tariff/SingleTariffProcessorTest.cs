using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class SingleTariffProcessorTest : ABIProcessorTest<ACSSingleTariffProcessor, APLA, APLB, APLY>
	{
		public void TestWLPGACodes()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageText = "B018888XJ5WI                                               33481                W 0299999999080709                                                              Y  8888XJ5WI00001";
			message.EM_MessageNum = "33482";
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var processor = new ACSSingleTariffProcessor();
			AddMessageBlocksToProcessor(processor, ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem,
"W10299999999 0101959999991KG       7FEATHERS, STUFFING, GSA STND  000000000000",
"W20299999999000000000000000000000000000000000000000020000000000000000000",
"W30299999999                       ",
"WL0299999999DF1FS4DT2DT1           ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_MessageNum = "33482";
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			processor.Message = responseMessage;
			processor.Process();
			Factory.Save();

			var queryFeathers = new ZQuery(USCTariffSchema.UE_Tariff, "0299999999");
			var tariffFeathers = Factory.LoadTop1<USCTariff>(queryFeathers);
			AssertNotNull(tariffFeathers);
			AssertEquals("DF1FS4DT2DT1", tariffFeathers.UE_PGACodes);
		}

		public void TestV3OGACodes_NotPopulated()
		{
			TestCaseHelper.ClearTable(USCTariff.Schema.TableName);
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageText = "B018888XJ5WI                                               33481                W 0208909000080709                                                              Y  8888XJ5WI00001";
			message.EM_MessageNum = "33482";
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var processor = new ACSSingleTariffProcessor();
			AddMessageBlocksToProcessor(processor, ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse,
				"V10208909000A0101109999991KG       7OTHER MEAT AND EDIBLE OFFA    000000000000  ",
				"V20208909000000006400000000000000000000000000000000020000000000000000000        ",
				"V30208909000                                        D E J P A+AUBHCACLILJOMAMXOM");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_MessageNum = "33482";
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse;
			processor.Message = responseMessage;
			processor.Process();
			Factory.Save();

			var queryFeathers = new ZQuery(USCTariffSchema.UE_Tariff, "0208909000");
			var tariffFeathers = Factory.LoadTop1<USCTariff>(queryFeathers);
			AssertNotNull(tariffFeathers);
			AssertEquals("", tariffFeathers.UE_OGACodes);
		}

		public void TestV1ShouldRemoveOGAAndPGACodes()
		{
			var tariff = Factory.NewWithValidTestData<USCTariff>();
			tariff.UE_Tariff = "0208909000";
			tariff.UE_PGACodes = "EP1EP2";
			tariff.UE_OGACodes = "FD1";
			tariff.UE_DateFrom = new ZDateTime(2010, 1, 1);
			tariff.UE_DateTo = new ZDateTime(2099, 12, 31);
			Factory.Save();

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageText = "B018888XJ5WI                                               33481                W 0208909000080709                                                              Y  8888XJ5WI00001";
			message.EM_MessageNum = "33482";
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var processor = new ACSSingleTariffProcessor();
			AddMessageBlocksToProcessor(processor, ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse,
				"V10208909000A0101109999991KG       7OTHER MEAT AND EDIBLE OFFA    000000000000  ",
				"V20208909000000006400000000000000000000000000000000020000000000000000000        ",
				"V30208909000                                        D E J P A+AUBHCACLILJOMAMXOM");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_MessageNum = "33482";
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse;
			processor.Message = responseMessage;
			processor.Process();
			Factory.Save();

			AssertEquals("", tariff.UE_OGACodes);
			AssertEquals("If message does not have WL record. After processing the message, a value in UE_PGACodes should have been gone", string.Empty, tariff.UE_PGACodes);
		}

		protected override void EndToEndCore()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageText = "B018888XJ5WI                                               33481                W 0299999999080709                                                              Y  8888XJ5WI00001";
			message.EM_MessageNum = "33481";
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var processor = new ACSSingleTariffProcessor();
			AddMessageBlocksToProcessor(processor, ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem,
"W10299999999 0101959999991KG       7FEATHERS, STUFFING, GSA STND  000000000000",
"W20299999999000000000000000000000000000000000000000020000000000000000000",
"W30299999999                       ",
"WL0299999999FS4                    ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_MessageNum = "33481";
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			processor.Message = responseMessage;
			processor.Process();
			Factory.Save();

			var queryFeathers = new ZQuery(USCTariffSchema.UE_Tariff, "0299999999");
			var tariffFeathers = Factory.LoadTop1<USCTariff>(queryFeathers);
			AssertNotNull(tariffFeathers);
			AssertEquals("FS4", tariffFeathers.UE_PGACodes);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == "Tariff Query Request"; }));
			Assert(email.Body.Contains("FEATHERS, STUFFING, GSA ST"));
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}

		public void TestLinkBackToDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();

			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = declaration;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageText = "B018888XJ5WI                                               ~002874              W 98176101  022808                                                              Y  8888XJ5WI00001";
			message.EM_MessageNum = "~002874";
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_MessageText = "B018888XJ5WR                                               ~002874              W198176101   0801069999990         7SKI RCNG AP,PRTCT,6101-620343 000000000000  W298176101  000005500000000000000000000000000000000000000000000000000000 R      W398176101                                          A E J P AUBHCACLILJOMAMXSG  W498176101                                           01                         W098176101  022808           RANGE INCLUDES 0001 RECORDS                        Y  8888XJ5WR00005";
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			response.EM_Status = "QUE";
			response.EM_MessageNum = "~002874";
			Factory.Save();

			new USRIncomingMessageProcessor().ExecuteBatch();

			response.Reload();
			AssertEquals("PreCondition", "RCV", response.EM_Status);
			AssertEquals("response is attached to Declaration", declaration, response.EM_LinkedObject);

			AssertNotNull("An email with subject 'Tariff Query Request' should have been sent.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == "Tariff Query Request"; })));
		}

		public void TestAllTariffUpdatesShowIn1Email()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00012345";

			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = declaration;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageText = "B018888XJ5WI                                               ~29433               W 198130050  022808                                                              Y  8888XJ5WI00001";
			message.EM_MessageNum = "~29433";
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_MessageText = "B018888XJ5WR                                               ~29433               W198130050   0801069999990         0NONRES EQUIP TEMP USE;UNDRBOND000000000000  W298130050  000000000000000000000000000000000000000000000000000000000000 R      W398130050                                          P AUBHCACLILJOMAMXSG        W098130050  020309           RANGE INCLUDES 0001 RECORDS                        W14421909760 0701089999991X        7OTHER ARTICLES OF WOOD NSPF   000000000000  W24421909760000003300000000000000000000000000000000033333333000000000000  11    W34421909760                                        A B E J P AUBHCACLILJOMAMXSGW04421909760020309           RANGE INCLUDES 0001 RECORDS                        Y  8888XJ5WR00008		";
			response.EM_MessageNum = "~29433";
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			response.EM_Status = "QUE";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new USRIncomingMessageProcessor().ExecuteBatch();

			response.Reload();
			AssertEquals("PreCondition", "RCV", response.EM_Status);
			AssertEquals("response is attached to Declaration", declaration, response.EM_LinkedObject);

			AssertEquals("one email only is generated", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == "Tariff Query Request"; }));
			AssertNotNull("Tariff Query Request email should have been generated", email);
			AssertEquals("banner & footer", 2, email.Attachments.Count);
			Assert("Advise user of related Job No", email.Body.Contains("This tariff update request was made on Job B00012345"));

			Assert("email has Tariff 9813.00.50", email.Body.Contains("<br />Tariff Number: <b>9813.00.50</b><br /><br />"));
			Assert("details for this tariff", email.Body.Contains("NONRES EQUIP TEMP USE;UNDR"));
			Assert("details for this tariff", email.Body.Contains("Effective Date From: </td><td>08/01/2006"));
			Assert("details for this tariff", email.Body.Contains("Effective Date To: </td><td>12/31/2099"));
			Assert("details for this tariff", email.Body.Contains("Duty Computation Code: </td><td>"));
			Assert("details for this tariff", email.Body.Contains("Column 1 Specific Rate : </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Column 1 Ad Valorem Rate: </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Column 1 Other Rate: </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Column 2 Specific Rate: </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Column 2 Ad Valorem Rate: </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Column 2 Other Rate: </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Additional Tariff Number Ind.: </td><td>R"));
			Assert("details for this tariff", email.Body.Contains("Special Program Indicator (SPI) Code: </td><td>P , AU, BH, CA, CL, IL, JO, MA, MX, SG"));

			Assert("email has Tariff 4421.90.9760", email.Body.Contains("Tariff Number: <b>4421.90.9760"));
			Assert("details for this tariff", email.Body.Contains("OTHER ARTICLES OF WOOD NSP"));
			Assert("details for this tariff", email.Body.Contains("Effective Date From: </td><td>07/01/2008"));
			Assert("details for this tariff", email.Body.Contains("Effective Date To: </td><td>12/31/2099"));
			Assert("details for this tariff", email.Body.Contains("Reporting Units: </td><td>1 - Unit 1: X"));
			Assert("details for this tariff", email.Body.Contains("Duty Computation Code: </td><td>7"));
			Assert("details for this tariff", email.Body.Contains("Column 1 Specific Rate : </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Column 1 Ad Valorem Rate: </td><td>0.03300000"));
			Assert("details for this tariff", email.Body.Contains("Column 1 Other Rate: </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Column 2 Specific Rate: </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Column 2 Ad Valorem Rate: </td><td>0.33333333"));
			Assert("details for this tariff", email.Body.Contains("Column 2 Other Rate: </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Miscellaneous Permit License Ind.: </td><td>11"));
			Assert("details for this tariff", email.Body.Contains("Special Program Indicator (SPI) Code: </td><td>A , B , E , J , P , AU, BH, CA, CL, IL, JO, MA, MX, SG"));
		}

		public void TestTariffUpdatesWithValidAndInvalidTariffs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00012347";

			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = declaration;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageText = "B018888XJ5WI                                               ~29488               W 6212109020011509                                                              W 98211119  011509                                                              W 7700109010011509                                                              Y  8888XJ5WI00003";
			message.EM_MessageNum = "~29488";
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_MessageText = "B018888XJ5WR                                               ~29488               " +
				"W16212109020 0101091231092DOZKG    7BRASSIERES,NT LACE/NET,MM,KN/C000000000000  " +
				"W26212109020000016900000000000000000000000000000000075000000000000000000        " +
				"W36212109020                                    1649E*P AUBHCACLILJOMAMXSG      " +
				"W56212109020AU00000000000000001550000000000000000005621000000238200000000000000 " +
				"W66212109020MA000000000000999999999999000000000000                              " +
				"W06212109020011509           RANGE INCLUDES 0001 RECORDS                        " +
				"W198211119   0201011231090         0ATPDEA,BRASSIERES UNDER 621210000000000000  " +
				"W298211119  000000000000000000000000000000000000000000000000000000000000 R07    " +
				"W098211119  011509           RANGE INCLUDES 0001 RECORDS                        " +
				"W07700109010011509          NOT ON FILE OR EXPIRED                              " +
				"W087089970601116098708997060NONE IN FILE OR EXPIRED                             " +
				"Y  8888XJ5WR00010";
			response.EM_MessageNum = "~29488";
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			response.EM_Status = "QUE";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new USRIncomingMessageProcessor().ExecuteBatch();

			response.Reload();
			AssertEquals("PreCondition", "RCV", response.EM_Status);
			AssertEquals("response is attached to Declaration", declaration, response.EM_LinkedObject);

			AssertEquals("one email only is generated", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == "Tariff Query Request"; }));
			AssertNotNull("Tariff Query Request email should have been generated", email);
			AssertEquals("banner & footer", 2, email.Attachments.Count);
			Assert("Advise user of related Job No", email.Body.Contains("This tariff update request was made on Job B00012347"));

			Assert("email has Tariff 6212109020", email.Body.Contains("Tariff Number: <b>6212.10.9020"));
			Assert("details for this tariff", email.Body.Contains("BRASSIERES,NT LACE/NET,MM,"));
			Assert("details for this tariff", email.Body.Contains("Effective Date From: </td><td>01/01/2009"));
			Assert("details for this tariff", email.Body.Contains("Effective Date To: </td><td>12/31/2009"));
			Assert("details for this tariff", email.Body.Contains("Reporting Units: </td><td>2 - Unit 1: DOZ; Unit 2: K"));
			Assert("details for this tariff", email.Body.Contains("Duty Computation Code: </td><td>7"));
			Assert("details for this tariff", email.Body.Contains("Column 1 Specific Rate : </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Column 1 Ad Valorem Rate: </td><td>0.16900000"));
			Assert("details for this tariff", email.Body.Contains("Column 1 Other Rate: </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Column 2 Specific Rate: </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Column 2 Ad Valorem Rate: </td><td>0.75000000"));
			Assert("details for this tariff", email.Body.Contains("Column 2 Other Rate: </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Quota Ind.: </td><td>1"));
			Assert("details for this tariff", email.Body.Contains("Category No.: </td><td>649"));
			Assert("details for this tariff", email.Body.Contains("Special Program Indicator (SPI) Code: </td><td>E*, P , AU, BH, CA, CL, IL, JO, MA, MX, SG"));
			Assert("details for this tariff", email.Body.Contains("ISO Country Code 1: </td><td>AU"));
			Assert("details for this tariff", email.Body.Contains("Specific Special Rate 1: </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Ad Valorem Special Rate 1: </td><td>0.15500000"));
			Assert("details for this tariff", email.Body.Contains("Other Special Rate 1: </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Tax/Fee Ad Valorem Rate 1: </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Fee Class Code 1: </td><td>056"));
			Assert("details for this tariff", email.Body.Contains("Tax/Fee Computation Code 1: </td><td>2"));
			Assert("details for this tariff", email.Body.Contains("Tax/Fee Flag 1: </td><td>1"));
			Assert("details for this tariff", email.Body.Contains("ISO Country Code 1: </td><td>AU"));
			Assert("details for this tariff", email.Body.Contains("Tax/Fee Specific Rate 1: </td><td>0.00238200"));
			Assert("details for this tariff", email.Body.Contains("ISO Country Code 2: </td><td>MA"));
			Assert("details for this tariff", email.Body.Contains("Specific Special Rate 2: </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Ad Valorem Special Rate 2: </td><td>9999.99999999"));
			Assert("details for this tariff", email.Body.Contains("Other Special Rate 2: </td><td>0.00000000"));
			Assert("details for this tariff", email.Body.Contains("Tax/Fee Ad Valorem Rate 2: </td><td>0.00000000"));

			Assert("email has Tariff 98211119", email.Body.Contains("Tariff Number: <b>9821.11.19"));
			Assert("details for this tariff", email.Body.Contains("ATPDEA,BRASSIERES UNDER 62"));
			Assert("details for this tariff", email.Body.Contains("Effective Date From: </td><td>02/01/2001"));
			Assert("details for this tariff", email.Body.Contains("Effective Date To: </td><td>12/31/2009"));

			Assert("email has Tariff 7700109010", email.Body.Contains("7700.10.9010"));
			Assert("Tariff 7700109010 is invalid", email.Body.Contains("NOT ON FILE OR EXPIRED"));
			Assert("Tariff 8708997060 is invalid", email.Body.Contains("NONE IN FILE OR EXPIRED"));
		}

		public void TestTariffResponseForRandomQuery()
		{
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_FullName = "Test User";
			testUser.GS_Code = "TU";
			testUser.GS_EmailAddress = "testuser@test.com";
			Factory.Save();

			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = testUser;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageText = "B01    GAZWI                                               ~191                 W 4010216000021809                                                              Y      GAZWI00001";
			message.EM_MessageNum = "~191";
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_SystemCreateUser = testUser.GS_Code;
			message.EM_SystemLastEditUser = testUser.GS_Code;
			Factory.Save();

			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_MessageText = "B018888XJ5WR                                               ~191                 W14010216000 0801069999990KG       0TRNS BLT:CMB W/OTH FBR,60-80CM000000000000  Y  8888XJ5WR00008		";
			response.EM_MessageNum = "~191";
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			response.EM_Status = "QUE";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new USRIncomingMessageProcessor().ExecuteBatch();

			response.Reload();
			AssertEquals("PreCondition", "RCV", response.EM_Status);

			AssertEquals("An email should be generated to the user who sent query", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals("Email address", "testuser@test.com", Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients[0].Email);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == "Tariff Query Request"; }));
			AssertNotNull("Tariff Query Request email should have been generated", email);
			AssertEquals("banner & footer", 2, email.Attachments.Count);

			Assert("response to random query", email.Body.Contains("This tariff update is sent in response to your tariff query on " + message.EM_SystemCreateTimeUtc.ToLongTimeString()));
			Assert("email has Tariff 4010.21.6000", email.Body.Contains("Tariff Number: <b>4010.21.6000"));
			Assert("details for this tariff", email.Body.Contains("TRNS BLT:CMB W/OTH FBR,60-"));
			Assert("details for this tariff", email.Body.Contains("Effective Date From: </td><td>08/01/2006"));
			Assert("details for this tariff", email.Body.Contains("Effective Date To: </td><td>12/31/2099"));
			Assert("details for this tariff", email.Body.Contains("Reporting Units: </td><td>0 - Unit 1: KG"));
		}

		public void TestWhenTarrifIsInvalidOrExpiredStillSendsResponseEmail()
		{
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_FullName = "Test User";
			testUser.GS_Code = "TU";
			testUser.GS_EmailAddress = "testuser@test.com";
			Factory.Save();

			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = testUser;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageText = "B01    GAZWI                                               ~191                 W 4010216000021809                                                              Y      GAZWI00001";
			message.EM_MessageNum = "~191";
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_SystemCreateUser = testUser.GS_Code;
			message.EM_SystemLastEditUser = testUser.GS_Code;
			Factory.Save();

			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_MessageText = "B018888XJ5WR                                               ~191                 W03926905560010110          NOT ON FILE OR EXPIRED                              Y  8888XJ5WR00001";
			response.EM_MessageNum = "~191";
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			response.EM_Status = "QUE";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new USRIncomingMessageProcessor().ExecuteBatch();

			response.Reload();
			AssertEquals("PreCondition", "RCV", response.EM_Status);

			AssertEquals("An email should still be generated to the user who sent query", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals("Email address", "testuser@test.com", Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients[0].Email);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == "Tariff Query Request"; }));
			AssertNotNull("Tariff Query Request email should have been generated", email);
			AssertEquals("banner & footer", 2, email.Attachments.Count);

			Assert("response to random query", email.Body.Contains("This tariff update is sent in response to your tariff query on " + message.EM_SystemCreateTimeUtc.ToLongTimeString()));
			Assert("email has Tariff 3926.90.5560", email.Body.Contains("Tariff Queried: <b>3926.90.5560"));
			Assert("details for this tariff", email.Body.Contains("NOT ON FILE OR EXPIRED"));
		}

		public void TestResponseForQueryRange()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageText = "B018888XJ5WI                                               33484                W 6006      0807096104                                                          Y  8888XJ5WI00001";
			message.EM_MessageNum = "33484";
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var processor = new ACSSingleTariffProcessor();

			#region Test Message

			AddMessageBlocksToProcessor(processor, ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem,
"W16006100000 0201091231091KG       7OTH KNIT FAB NSPF OF WOOL/HIAR000000000000  ",
"W26006100000000010000000000000000000000000000000000065500000000000000000        ",
"W36006100000                                    1414P AUBHCACLILJOMAMXOMPESG    ",
"W56006100000AU000000000000000008000000000000000000                              ",
"W66006100000MA000000000000000002100000000000000000                              ",
"W76006100000OM000000000000000008000000000000000000                              ",
"W16006211000 0807091231091KG       7OTH KNIT/CRO FAB,COTN,CIRC NIT000000000000  ",
"W26006211000000010000000000000000000000000000000000045000000000000000000        ",
"W36006211000                                    1222P AUBHCACLILJOMAMXOMPESG    ",
"W56006211000AU00000000000000000800000000000000000005611000001259300000000000000 ",
"W66006211000MA000000000000000002000000000000000000                              ",
"W76006211000OM000000000000000008000000000000000000                              ",
"W16006219020 0201091231091KG       7OTH KNIT/CRO FAB,COTN,CIRC NIT000000000000  ",
"W26006219020000010000000000000000000000000000000000045000000000000000000        ",
"W36006219020                                    1222P AUBHCACLILJOMAMXOMPESG    ",
"W56006219020AU000000000000000008000000000000000000                              ",
"W66006219020MA000000000000000002000000000000000000                              ",
"W76006219020OM000000000000000008000000000000000000                              ",
"W16006219080 0201091231091KG       7OTH KNIT/CRO FAB,COTN, OTHER  000000000000  ",
"W26006219080000010000000000000000000000000000000000045000000000000000000        ",
"W36006219080                                    1222P AUBHCACLILJOMAMXOMPESG    ",
"W56006219080AU000000000000000008000000000000000000                              ",
"W66006219080MA000000000000000002000000000000000000                              ",
"W76006219080OM000000000000000008000000000000000000                              ",
"W16103230075 0201099999992DOZKG    9SHIRTS,M/B,OTH,SYN FIB,NIT/CRO000000000000  ",
"W26103230075000100000000000000000000000000000000000100000000000000000000 R      ",
"W36103230075                                    1638P AUBHCACLILJOMAMXOMPESG    ",
"W56103230075AU000000000000000100000000000000000000                              ",
"W66103230075JO000000000000000100000000000000000000                              ",
"W76103230075MA000000000000000100000000000000000000                              ",
"W86103230075OM000000000000000100000000000000000000                              ",
"W16103230080 0201099999992DOZKG    9OTHER,M/B,OTH,SYN FIB,KNIT/CRO000000000000  ",
"W26103230080000100000000000000000000000000000000000100000000000000000000 R      ",
"W36103230080                                    1659P AUBHCACLILJOMAMXOMPESG    ",
"W56103230080AU000000000000000100000000000000000000                              ",
"W66103230080JO000000000000000100000000000000000000                              ",
"W76103230080MA000000000000000100000000000000000000                              ",
"W86103230080OM000000000000000100000000000000000000                              ",
"W16103290510 0201099999992DOZKG    9MEN/BY ENS WL/FIN ANML HR,6101000000000000  ",
"W26103290510000100000000000000000000000000000000000100000000000000000000 R      ",
"W36103290510                                    1434P AUBHCACLILJOMAMXOMPESG    ",
"W56103290510AU000000000000000100000000000000000000                              ",
"W66103290510MA000000000000000100000000000000000000                              ",
"W76103290510OM000000000000000100000000000000000000                              ",
"W16103230010 0201099999992DOZKG    9TROUSRS..M/B,SYN FIB,=>23%WOOL000000000000  ",
"W26103230010000100000000000000000000000000000000000100000000000000000000 R      ",
"W36103230010                                    1447P AUBHCACLILJOMAMXOMPESG    ",
"W56103230010AU000000000000000100000000000000000000                              ",
"W66103230010JO000000000000000100000000000000000000                              ",
"W76103230010MA000000000000000100000000000000000000                              ",
"W86103230010OM000000000000000100000000000000000000                              ",
"W16103230025 0201099999992DOZKG    9SHIRTS(M/B)SYN FBR, =>23% WOOL000000000000  ",
"W26103230025000100000000000000000000000000000000000100000000000000000000 R      ",
"W36103230025                                    1438P AUBHCACLILJOMAMXOMPESG    ",
"W56103230025AU000000000000000100000000000000000000                              ",
"W66103230025JO000000000000000100000000000000000000                              ",
"W76103230025MA000000000000000100000000000000000000                              ",
"W86103230025OM000000000000000100000000000000000000                              ",
"W16103230030 0201099999992DOZKG    9SWTRS(M/B) SYTH FBR,=>23% WOOL000000000000  ",
"W26103230030000100000000000000000000000000000000000100000000000000000000 R      ",
"W36103230030                                    1445P AUBHCACLILJOMAMXOMPESG    ",
"W56103230030AU000000000000000100000000000000000000                              ",
"W66103230030JO000000000000000100000000000000000000                              ",
"W76103230030MA000000000000000100000000000000000000                              ",
"W86103230030OM000000000000000100000000000000000000                              ",
"W16103230035 0201099999992DOZKG    9MEN/BY,SYTH FBR,=>23% WOOL,OTH000000000000  ",
"W26103230035000100000000000000000000000000000000000100000000000000000000 R      ",
"W36103230035                                    1459P AUBHCACLILJOMAMXOMPESG    ",
"W56103230035AU000000000000000100000000000000000000                              ",
"W66103230035JO000000000000000100000000000000000000                              ",
"W76103230035MA000000000000000100000000000000000000                              ",
"W86103230035OM000000000000000100000000000000000000                              ",
"W16103230036 0201099999992DOZKG    9OTHR M/B,SYN FIBR,NIT/CRO-6101000000000000  ",
"W26103230036000100000000000000000000000000000000000100000000000000000000 R      ",
"W36103230036                                    1634P AUBHCACLILJOMAMXOMPESG    ",
"W56103230036AU000000000000000100000000000000000000                              ",
"W66103230036JO000000000000000100000000000000000000                              ",
"W76103230036MA000000000000000100000000000000000000                              ",
"W86103230036OM000000000000000100000000000000000000                              ",
"W16103230037 0201099999992DOZKG    9OTHR M/B,JACKET/BLAZER (6103) 000000000000  ",
"W26103230037000100000000000000000000000000000000000100000000000000000000 R      ",
"W36103230037                                    1633P AUBHCACLILJOMAMXOMPESG    ",
"W56103230037AU000000000000000100000000000000000000                              ",
"W66103230037JO000000000000000100000000000000000000                              ",
"W76103230037MA000000000000000100000000000000000000                              ",
"W86103230037OM000000000000000100000000000000000000                              ",
"W06006      0807096104       RANGE EXCEEDS 100 RECORDS                          ");

			#endregion

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_MessageNum = "33484";
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			processor.Message = responseMessage;
			processor.Process();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == "Tariff Query Request"; }));
			AssertNotNull("Tariff Query Request email should have been generated", email);
			AssertEquals("banner & footer", 2, email.Attachments.Count);
			Assert(email.Body.Contains("RANGE EXCEEDS 100 RECORDS"));
			Assert(email.Body.Contains("Response Tariff Range: From"));
		}
	}
}
