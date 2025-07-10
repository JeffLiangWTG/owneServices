using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class QueryImporterBondProcessorTest : ABIProcessorTest<QueryImporterBondProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			var image1 = new System.Drawing.Bitmap(1, 2);
			Registry.Business.SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var outgoing = CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", organisation);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K191-0131990001ATLAS INTERNATIONAL             891A0000500003001063001300107150 ",
"K2DBATIC UNITED CORP                                                            ",
"K3C/O JOHN LITZLER, TRUSTEE       1412 MAIN ST FL 24                            ",
"K4ATLAS INTERNATIONAL             DALLAS               TX752024018              ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "~150000";
			processor.Message = responseMessage;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals(organisation, responseMessage.EM_LinkedObject);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Query Importer Bond"); }));
			Assert(sentMail.Body.Contains("Query Importer Bond Response"));
			Assert(sentMail.Body.Contains("Importer Name: ATLAS INTERNATIONAL"));
			Assert(sentMail.Body.Contains("Importer Number: 91-013199000"));
			Assert(sentMail.Body.Contains("Name Qualifier: DBA TIC UNITED CORP"));

			var banner = sentMail.Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(1, image.Width);

			AssertHasBondDetail(organisation.PK, "1", 50000m, new ZDateTime(2001, 06, 30), "3001", "300107150", "8", "891");
		}

		public void TestEINflagForDifferentQueryResultsCodeInAQIBK1()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var outgoing = CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", organisation);
			Factory.Save();
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "~150000";
			QueryImporterBondProcessor processor;

			var eINflags = new bool[] { false, true, true, false, false };
			for (var i = 0; i < eINflags.Length; i++)
			{
				processor = new QueryImporterBondProcessor() { Message = responseMessage };
				AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
					$"K174-264880600{i}MAR BRAN USA LTD                036A00005000099001110119911GC271 ");
				processor.Process();
				AssertEquals(organisation, responseMessage.EM_LinkedObject);
				AssertEquals(eINflags[i] ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No, OrgHeaderWrapper.New(organisation).ZO_IsEINNumberVerifiedIndicator);
			}

			processor = new QueryImporterBondProcessor() { Message = responseMessage };
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
				"K174-2648806000MAR BRAN USA LTD                036A00005000099001110119911GC271 ",
				"K174-2648806001MAR BRAN USA LTD                036A00005000099001110119911GC271 ");
			processor.Process();
			AssertEquals(organisation, responseMessage.EM_LinkedObject);
			AssertEquals(YesNoDefaultList.Codes.Yes, OrgHeaderWrapper.New(organisation).ZO_IsEINNumberVerifiedIndicator);

			processor = new QueryImporterBondProcessor() { Message = responseMessage };
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
				"K174-2648806003MAR BRAN USA LTD                036A00005000099001110119911GC271 ",
				"K174-2648806004MAR BRAN USA LTD                036A00005000099001110119911GC271 ");
			processor.Process();
			AssertEquals(organisation, responseMessage.EM_LinkedObject);
			AssertEquals(YesNoDefaultList.Codes.No, OrgHeaderWrapper.New(organisation).ZO_IsEINNumberVerifiedIndicator);

			processor = new QueryImporterBondProcessor() { Message = responseMessage };
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
				"K174-2648806001MAR BRAN USA LTD                036A00005000099001110119911GC271 ",
				"K174-2648806002MAR BRAN USA LTD                036A00005000099001110119911GC271 ");
			processor.Process();
			AssertEquals(organisation, responseMessage.EM_LinkedObject);
			AssertEquals(YesNoDefaultList.Codes.Yes, OrgHeaderWrapper.New(organisation).ZO_IsEINNumberVerifiedIndicator);
		}

		public void TestSameBondDetailsForDifferentOrganisation()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "POLO RALP LAUREN  CORP";
			org1.MainAddress.OA_Address1 = "9 POLITO AVE";
			org1.MainAddress.OA_City = "LYNDHURST";
			org1.MainAddress.OA_PostCode = "07071";
			org1.OH_RL_NKClosestPort = "USLDT";
			org1.MainAddress.OA_State = "NJ";
			org1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "13-262203600");

			MQEDIMessage message1 = CreateMessage(EDIMessage.Direction.Transmit, MQEDIMessage.Status.Sent, ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", org1);
			message1.EM_MessageText = "B018888XJ5KI                                               ~150000              " +
									"K 13-2622036001                                                                 " +
									"Y  8888XJ5KI00001";

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "POLO SOMEONE CORP";
			org2.MainAddress.OA_Address1 = "SOME ADDRESS";
			org2.MainAddress.OA_City = "CITY";
			org2.MainAddress.OA_PostCode = "070713498";
			org2.OH_RL_NKClosestPort = "USNJ2";
			org2.MainAddress.OA_State = "OH";
			org2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "13-2622036LA");

			MQEDIMessage message2 = CreateMessage(EDIMessage.Direction.Transmit, MQEDIMessage.Status.Sent, ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150001", org2);
			message2.EM_MessageText = "B018888XJ5KI                                               ~150001              " +
									"K 13-2622036LA1                                                                 " +
									"Y  8888XJ5KI00001";

			MQEDIMessage responseMessage1 = Factory.New<MQEDIMessage>();
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse;
			responseMessage1.EM_MessageNum = "~150000";
			responseMessage1.EM_MessageText = "B008888XJ5KR                                               ~150000              " +
									"K113-2622036001POLO RALPH LAUREN CORPORATION   891A01000000099001203089908AU930 " +
									"K39 POLITO AVE                                                                  " +
									"K4POLO RALPH LAUREN CORPORATION   LYNDHURST            NJ07071349813-262203600  " +
									"Y  8888XJ5KR00003";

			MQEDIMessage responseMessage2 = Factory.New<MQEDIMessage>();
			responseMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse;
			responseMessage2.EM_MessageNum = "~150001";
			responseMessage2.EM_MessageText = "B008888XJ5KR                                               ~150001              " +
									"K113-2622036LA1POLO RALPH LAUREN CORPORATION   891A01000000099001203089908AU930 " +
									"K2DIVLAUREN BY RALPH LAUREN                                                     " +
									"K39 POLITO AVE                                                                  " +
									"K4POLO RALPH LAUREN CORPORATION   LYNDHURST            NJ07071349813-2622036LA  " +
									"Y  8888XJ5KR00004";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			responseMessage1.Reload();
			AssertEquals(org1, responseMessage1.EM_LinkedObject);
			responseMessage2.Reload();
			AssertEquals(org2, responseMessage2.EM_LinkedObject);
			AssertHasBondDetail(org1.PK, ActivityCodeList.Codes._1, 10000000m, new ZDateTime(2008, 12, 3), "9900", "9908AU930", ImporterBondTypeList.Codes.ContinuousBond, "891");
			AssertHasBondDetail(org2.PK, ActivityCodeList.Codes._1, 10000000m, new ZDateTime(2008, 12, 3), "9900", "9908AU930", ImporterBondTypeList.Codes.ContinuousBond, "891");
		}

		void AssertHasBondDetail(ZGuid orgPK, ZString activityCode, ZDecimal bondAmount, ZDateTime bondEffectiveDate, ZString bondFiledPort, ZString bondNumber, ZString bondType, ZString suretyCode)
		{
			var query = new ZQuery(CusBondDetailSchema.PW_ParentID, orgPK);
			query.AddToFilter(CusBondDetailSchema.PW_ParentTableCode, OrgHeaderSchema.Constants.Prefix);
			query.AddToFilter(CusBondDetailSchema.PW_BondNumber, bondNumber);
			var bondDetails = Factory.Load<CusBondDetail>(query);
			AssertEquals("Bond Details", 1, bondDetails.Length);
			AssertEquals("Activity Code", activityCode, bondDetails[0].PW_ActivityCode);
			AssertEquals("Bond Amount", bondAmount, bondDetails[0].PW_BondAmount);
			AssertEquals("Effective Date", bondEffectiveDate, bondDetails[0].PW_BondEffectiveDate);
			AssertEquals("Filed Port", bondFiledPort, bondDetails[0].PW_BondFiledPort);
			AssertEquals("Bond Type", bondType, bondDetails[0].PW_BondType);
			AssertEquals("Surety Code", suretyCode, bondDetails[0].PW_SuretyCode);
		}

		public void TestMaskImporterNumberWhenItIsSSNFormat()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var outgoing = CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", organisation);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K1123-12-1234 1ATLAS INTERNATIONAL             891A0000500003001063001300107150 ",
"K2DBATIC UNITED CORP                                                            ",
"K3C/O JOHN LITZLER, TRUSTEE       1412 MAIN ST FL 24                            ",
"K4ATLAS INTERNATIONAL             DALLAS               TX752024018              ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "~150000";
			processor.Message = responseMessage;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals(organisation, responseMessage.EM_LinkedObject);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Query Importer Bond"); }));
			Assert(!sentMail.Body.Contains("Importer Number: "));
		}

		public void TestOrganisationBondDataIsNotUpdatedWithDuplicateRequests()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var outgoing = CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", organisation);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K191-0131990001ATLAS INTERNATIONAL             891A0000500003001063001300107150 ",
"K2DBATIC UNITED CORP                                                            ",
"K3C/O JOHN LITZLER, TRUSTEE       1412 MAIN ST FL 24                            ",
"K4ATLAS INTERNATIONAL             DALLAS               TX752024018              ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "~150000";
			processor.Message = responseMessage;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals(organisation, responseMessage.EM_LinkedObject);
			AssertHasBondDetail(organisation.PK, "1", 50000m, new ZDateTime(2001, 06, 30), "3001", "300107150", "8", "891");

			var outgoing2 = CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150110", organisation);
			Factory.Save();
			processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K191-0131990001ATLAS INTERNATIONAL             891A0000500003001063001300107150 ",
"K2DBATIC UNITED CORP                                                            ",
"K3C/O JOHN LITZLER, TRUSTEE       1412 MAIN ST FL 24                            ",
"K4ATLAS INTERNATIONAL             DALLAS               TX752024018              ");

			var responseMessage2 = Factory.New<MQEDIMessage>();
			responseMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2.EM_MessageNum = "~150110";
			processor.Message = responseMessage2;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals(organisation, responseMessage.EM_LinkedObject);
			AssertHasBondDetail(organisation.PK, "1", 50000m, new ZDateTime(2001, 06, 30), "3001", "300107150", "8", "891");

			var outgoing3 = CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150527", organisation);
			Factory.Save();

			processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K191-0131990001ATLAS INTERNATIONAL             756C0000750005640070107300152601 ",
"K2DBATIC UNITED CORP                                                            ",
"K3C/O JOHN LITZLER, TRUSTEE       1412 MAIN ST FL 24                            ",
"K4ATLAS INTERNATIONAL             DALLAS               TX752024018              ");

			var responseMessage3 = Factory.New<MQEDIMessage>();
			responseMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage3.EM_MessageNum = "~150527";
			processor.Message = responseMessage3;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals(organisation, responseMessage.EM_LinkedObject);

			var oneQuery = new ZQuery(CusBondDetailSchema.PW_ParentID, organisation.PK);
			oneQuery.AddToFilter(CusBondDetailSchema.PW_ActivityCode, ActivityCodeList.Codes._1);
			AssertNotNull("new data should be added", Factory.LoadTop1<CusBondDetail>(oneQuery));
			AssertHasBondDetail(organisation.PK, "1a1", 75000m, new ZDateTime(2007, 07, 01), "5640", "300152601", "8", "756");
		}

		public void TestMultipleOrganisationBondDataIsUpdated()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var outgoing = CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", organisation);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K191-1325671001STARBUCKS CORPORATION           891A00080000099001220089908BS103 ",
"K191-1325671001STARBUCKS CORPORATION           741E0001000003001010303300222694 ",
"K2DBASTARBUCKS COFFEE CO                                                        ",
"K32401 UTAH AVE S                                                               ",
"K4STARBUCKS CORPORATION           SEATTLE              WA98134143691-132567100  ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "~150000";
			processor.Message = responseMessage;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals(organisation, responseMessage.EM_LinkedObject);

			var bondDetails = Factory.Load<CusBondDetail>(new ZQuery(CusBondDetailSchema.PW_ParentID, organisation.PK));
			AssertEquals("Should have been 2 sets of bond data loaded for this organisation", 2, bondDetails.Length);
			AssertHasBondDetail(organisation.PK, "1", 800000m, new ZDateTime(2008, 12, 20), "9900", "9908BS103", "8", "891");
			AssertHasBondDetail(organisation.PK, "3", 100000m, new ZDateTime(2003, 01, 03), "3001", "300222694", "8", "741");

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Query Importer Bond"); }));
			Assert(sentMail.Body.Contains("Query Importer Bond Response for " + organisation.OH_Code));
			Assert(sentMail.Body.Contains("Importer Name: STARBUCKS CORPORATION"));
			Assert(sentMail.Body.Contains("Name Qualifier: DBA STARBUCKS COFFEE CO"));
		}

		public void TestValidResponseSetsOrganisationAsBeingOnFile()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", organisation);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K191-0131990001ATLAS INTERNATIONAL             891A0000500003001063001300107150 ",
"K2DBATIC UNITED CORP                                                            ",
"K3C/O JOHN LITZLER, TRUSTEE       1412 MAIN ST FL 24                            ",
"K4ATLAS INTERNATIONAL             DALLAS               TX752024018              ");

			processor.Message = CreateMessage(EDIMessage.Direction.Receive, "QUE", ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse, "~150000", null);
			processor.Process();
			Factory.Save();
			organisation.Reload();
			var orgWrapped = OrgHeaderWrapper.New(organisation);
			AssertEquals(YesNoDefaultList.Codes.Yes, orgWrapped.ZO_IsEINNumberVerifiedIndicator);
		}

		public void TestValidResponseSetsOrganisationAsBeingOnFile_1()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", organisation);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K193-0970344002SAT-PAK COMMUNICATIONS INC          000000000                    ",
"K31492 NW 6TH STREET                                                            ",
"K4SAT-PAK COMMUNICATIONS INC      REDMOND              OR97756    93-097034400  ");

			processor.Message = CreateMessage(EDIMessage.Direction.Receive, "QUE", ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse, "~150000", null);
			processor.Process();
			Factory.Save();
			organisation.Reload();
			var orgWrapped = OrgHeaderWrapper.New(organisation);
			AssertEquals(YesNoDefaultList.Codes.Yes, orgWrapped.ZO_IsEINNumberVerifiedIndicator);
		}

		public void TestSendStandAloneInBondMessage()
		{
			CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", null);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K191-1325671001STARBUCKS CORPORATION           891A00080000099001220089908BS103 ",
"K191-1325671001STARBUCKS CORPORATION           741E0001000003001010303300222694 ",
"K2DBASTARBUCKS COFFEE CO                                                        ",
"K32401 UTAH AVE S                                                               ",
"K4STARBUCKS CORPORATION           SEATTLE              WA98134143691-132567100  ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "~150000";
			processor.Message = responseMessage;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Query Importer Bond"); }));
			Assert(sentMail.Body.Contains("Query Importer Bond Response for Multiple Importer Numbers"));
		}

		public void TestSendStandAloneWithMultipleNumbersInBondMessage()
		{
			CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", null);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K112-1234567003STARBUCKS CORPORATION           891A00080000099001220089908BS103 ",
"K112-1234567893IMPORTER # VOIDED               891A00080000099001220089908BS103 ",
"K112-1234567AA2TEST1                           891A00080000099001220089908BS103 ",
"K191-1325671001STARBUCKS CORPORATION           741E0001000003001010303300222694 ",
"K2DBASTARBUCKS COFFEE CO                                                        ",
"K32401 UTAH AVE S                                                               ",
"K4STARBUCKS CORPORATION           SEATTLE              WA98134143612-1234567**  ",
"K112-1234567AB1TEST 10                         741E0001000003001010303300222694 ",
"K2DBASTARBUCKS COFFEE CO                                                        ",
"K32401 UTAH AVE S                                                               ",
"K4STARBUCKS CORPORATION           SEATTLE              WA98134143612-1234567**  ",
"K112-1234567AC1TEST 12                         741E0001000003001010303300222694 ",
"K2DBASTARBUCKS COFFEE CO                                                        ",
"K32401 UTAH AVE S                                                               ",
"K4STARBUCKS CORPORATION           SEATTLE              WA98134143612-1234567**  ",
"K112-1234567AD1TEST 13                         741E0001000003001010303300222694 ",
"K2DBASTARBUCKS COFFEE CO                                                        ",
"K32401 UTAH AVE S                                                               ",
"K4STARBUCKS CORPORATION           SEATTLE              WA98134143612-1234567**  ",
"K112-1234567B11TESTB1                          741E0001000003001010303300222694 ",
"K2DBASTARBUCKS COFFEE CO                                                        ",
"K32401 UTAH AVE S                                                               ",
"K4STARBUCKS CORPORATION           SEATTLE              WA98134143612-1234567**  ",
"K112-1234567BB1TEST  2                         741E0001000003001010303300222694 ",
"K2DBASTARBUCKS COFFEE CO                                                        ",
"K32401 UTAH AVE S                                                               ",
"K4STARBUCKS CORPORATION           SEATTLE              WA98134143612-1234567**  ",
"K112-1234567C22TESTC2                                                           ",
"K2DBA SOMETHING                                                                 ",
"K32401 UTAH AVE S                                                               ",
"K4STARBUCKS CORPORATION           SEATTLE              WA98134143612-1234567**  ",
"K112-1234567C11TEST  2                         741E0001000003001010303300222694 ",
"K2DBASTARBUCKS COFFEE CO                                                        ",
"K32401 UTAH AVE S                                                               ",
"K4STARBUCKS CORPORATION           SEATTLE              WA98134143612-1234567**  ",
"K112-1234567C21TESTC2                          741E0001000003001010303300222694 ",
"K2DBA SOMETHING                                                                 ",
"K32401 UTAH AVE S                                                               ",
"K4STARBUCKS CORPORATION           SEATTLE              WA98134143612-1234567**  ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "~150000";
			processor.Message = responseMessage;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Query Importer Bond"); }));
			Assert(sentMail.Body.Contains("Query Importer Bond Response for Multiple Importer Numbers"));
		}

		public void TestBondNotOnFileUpdateExpiryDate()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var collection = new CusBondDetailCollection(organisation);
			var bondData = collection.AddNew();
			bondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-1);
			bondData.PW_SuretyCode = "555";

			var outgoing = CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", organisation);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K132-0036930002INTERNATIONAL MACHINE SERVICES I    000000000                    ",
"K35965 WALL ST                                                                  ",
"K4INTERNATIONAL MACHINE SERVICES ISTERLING HEIGHTS     MI48312107432-003693000  ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "~150000";
			processor.Message = responseMessage;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals(organisation, responseMessage.EM_LinkedObject);

			var wrapper = OrgHeaderWrapper.New(organisation);
			AssertEquals("Bond Expiry Date should = bond Effective Date when the customs query yields no matching bond", ZDateTime.Today.AddDays(-1), wrapper.BondDetails[0].PW_BondExpiryDate);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Query Importer Bond"); }));
			Assert(sentMail.Body.Contains("Query Importer Bond Response"));
			Assert(sentMail.Body.Contains("Importer Name: INTERNATIONAL MACHINE SERVICES I"));
			Assert(sentMail.Body.Contains("Importer Number: 32-003693000"));
			Assert(sentMail.Body.Contains("Query Result Code: Name and address information is on file with no bond"));
			Assert(sentMail.Body.Contains(QueryImporterBondProcessor.BondExpiredAdvice));
		}

		public void TestBondOnFile_AndUpdateOtherExpiriedBonds()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			var collection = new CusBondDetailCollection(organisation);
			var bondData = collection.AddNew();
			bondData.PW_BondNumber = "303256";
			bondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-1);
			bondData.PW_SuretyCode = "555";

			bondData = collection.AddNew();
			bondData.PW_BondNumber = "399901596";
			bondData.PW_BondAmount = 50000m;
			bondData.PW_BondFiledPort = "9900";
			bondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData.PW_BondEffectiveDate = new ZDateTime(1999, 02, 17);
			bondData.PW_SuretyCode = "891";

			bondData = collection.AddNew();
			bondData.PW_BondNumber = "500455";
			bondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondData.PW_ActivityCode = ActivityCodeList.Codes._4;
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			bondData.PW_SuretyCode = "892";

			var outgoing = CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", organisation);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K136-3050396001LINDGREN RF ENCLOSURES          891A0000500009900021799399901596 ",
"K3400 HIGH GROVE BLVD                                                           ",
"K4LINDGREN RF ENCLOSURES          GLENDALE HEIGHTS     IL60139221836-305039600  ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "~150000";
			processor.Message = responseMessage;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals(organisation, responseMessage.EM_LinkedObject);

			var wrapper = OrgHeaderWrapper.New(organisation);
			foreach (CusBondDetail bond in wrapper.BondDetails)
			{
				if (bond.PW_BondNumber == "303256")
				{
					AssertEquals("missed in the message and should be expired", bond.PW_BondExpiryDate, ZDateTime.Today.AddDays(-1));
				}
				else if (bond.PW_BondNumber == "399901596")
				{
					AssertEquals("not expired", bond.PW_BondExpiryDate, ZDateTime.Empty);
				}
				else if (bond.PW_BondNumber == "500455")
				{
					AssertEquals("missed in the message and should be expired", bond.PW_BondExpiryDate, ZDateTime.Today.AddDays(-10));
				}
			}

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Query Importer Bond"); }));
			Assert(sentMail.Body.Contains("Query Importer Bond Response"));
			Assert(sentMail.Body.Contains("Importer Name: LINDGREN RF ENCLOSURES"));
			Assert(sentMail.Body.Contains("Importer Number: 36-305039600"));
			Assert(sentMail.Body.Contains(QueryImporterBondProcessor.BondExpiredAdvice));
		}

		public void TestImportBondTerminateDate()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			var collection = new CusBondDetailCollection(organisation);
			var bondData = collection.AddNew();
			bondData.PW_BondNumber = "990900083";
			bondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData.PW_BondEffectiveDate = new ZDateTime(2009, 03, 10);
			bondData.PW_SuretyCode = "891";
			bondData.PW_BondAmount = 50000;
			bondData.PW_BondFiledPort = "9900";
			bondData.PW_BondExpiryDate = new ZDateTime(2012, 12, 21);

			var bondData2 = collection.AddNew();
			bondData2.PW_BondNumber = "89890";
			bondData2.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondData2.PW_ActivityCode = ActivityCodeList.Codes._1a;
			bondData2.PW_SuretyCode = "891";
			bondData2.PW_BondAmount = 100000;
			bondData2.PW_BondEffectiveDate = new ZDateTime(2012, 09, 17);
			bondData2.PW_BondFiledPort = "9900";
			bondData2.PW_BondExpiryDate = new ZDateTime(2012, 09, 17);

			var outgoing = CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", organisation);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K158-1234567891NWD ENT 12 B                    891A0000500009900031009990900083 ",
"K2                                    020313 Y N                                ",
"K35000 TOWERS CRESCENT DR                                                       ",
"K4                                VIENNA               VA22182                  ",
"K5670 YOUNG ST                                                                  ",
"K6                                TONAWANDA            NY141504103              ",
"K123-1306344003FISHMAN & TOBIN INC             891A00300000099000330079907D5092 ",
"K2                                    020313 N Y                                ",
"K3625 W RIDGE PIKE STE E320                                                     ",
"K4FISHMAN & TOBIN INC             CONSHOHOCKEN         PA19428321923-130634400  ",
"K5670 YOUNG ST                                                                  ",
"K6                                TONAWANDA            NY141504103              ",
"K136-3050396004LINDGREN RF ENCLOSURES          891A0000500009900021799399901596 ",
"K2                                    020313 Y N                                ",
"K3400 HIGH GROVE BLVD                                                           ",
"K4LINDGREN RF ENCLOSURES          GLENDALE HEIGHTS     IL60139221836-305039600  ",
"K5670 YOUNG ST                                                                  ",
"K6BOB THE BUILDER                 TONAWANDA            NY141504103              ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "~150000";
			processor.Message = responseMessage;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals(organisation, responseMessage.EM_LinkedObject);

			var wrapper = OrgHeaderWrapper.New(organisation);
			AssertEquals("wrapper.BondDetails.Count", 2, wrapper.BondDetails.Count);
			var bondDetail1 = wrapper.BondDetails[0];
			var bondDetail2 = wrapper.BondDetails[1];
			if (bondDetail2.PW_BondNumber == "990900083")
			{
				bondDetail1 = wrapper.BondDetails[1];
				bondDetail2 = wrapper.BondDetails[0];
			}
			AssertEquals("bondDetail1.PW_BondNumber", "990900083", bondDetail1.PW_BondNumber);
			AssertEquals("Bond Expire date should be set", new ZDateTime(2013, 2, 3), bondDetail1.PW_BondExpiryDate);
			AssertEquals("Bond has sufficient Fund", ZBool.False, bondDetail1.HasSufficientFund);
			AssertEquals("bondDetail2.PW_BondNumber", "89890", bondDetail2.PW_BondNumber);
			AssertEquals("Bond not on file and Expire Date should not be changed", new ZDateTime(2012, 09, 17), bondDetail2.PW_BondExpiryDate);
			AssertEquals("Bond has sufficient Fund", ZBool.True, bondDetail2.HasSufficientFund);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.First(x => x.Subject.Contains("Query Importer Bond"));
			AssertContains(@"Importer Name: NWD ENT 12 B<br />Importer Number: 58-123456789<br />Query Result Code: Name and address information is on file with a continuous bond<br />Address Line 1: 5000 TOWERS CRESCENT DR<br />City: VIENNA<br />State: VA<br />Zip: 22182<br />Periodic Monthly Statement Status: Y<br /><br />Physical Address Details:<br />Address Line 1: 670 YOUNG ST<br />City: TONAWANDA<br />State: NY<br />Zip: 141504103<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Result</th><th>Surety Code</th><th>Type</th><th>Amount</th><th>Port</th><th>Effective</th><th>Termination</th><th>Number</th><th>Bond is Sufficient?</th><th>Bond User Status</th><th>Bond User Termination Date</th></tr></thead><tr><td>Continuous</td><td>891</td><td>Importer or Broker</td><td>50000</td><td>9900</td><td>10-Mar-09</td><td>03-Feb-13</td><td>990900083</td><td>N</td><td>&nbsp;</td><td>&nbsp;</td></tr></table><br />Importer Name: FISHMAN & TOBIN INC<br />Importer Number: 23-130634400<br />Query Result Code: Importer number voided: if further assistance is required, contact your CBP Client Representative<br />Address Line 1: 625 W RIDGE PIKE STE E320<br />City: CONSHOHOCKEN<br />State: PA<br />Zip: 194283219<br />Periodic Monthly Statement Status: N<br /><br />Physical Address Details:<br />Address Line 1: 670 YOUNG ST<br />City: TONAWANDA<br />State: NY<br />Zip: 141504103<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Result</th><th>Surety Code</th><th>Type</th><th>Amount</th><th>Port</th><th>Effective</th><th>Termination</th><th>Number</th><th>Bond is Sufficient?</th><th>Bond User Status</th><th>Bond User Termination Date</th></tr></thead><tr><td>Voided</td><td>891</td><td>Importer or Broker</td><td>3000000</td><td>9900</td><td>30-Mar-07</td><td>03-Feb-13</td><td>9907D5092</td><td>Y</td><td>&nbsp;</td><td>&nbsp;</td></tr></table><br />Importer Name: LINDGREN RF ENCLOSURES<br />Importer Number: 36-305039600<br />Query Result Code: Importer number is in inactive status due to no cargo release, entry summary, or electronic invoice transactions were received within the last 18 months.<BR />To reactivate the importer number, provide CBP with the complete importer number, name, and address information through an Importer/Consignee Create/Update transaction.<br />Address Line 1: 400 HIGH GROVE BLVD<br />City: GLENDALE HEIGHTS<br />State: IL<br />Zip: 601392218<br />Periodic Monthly Statement Status: Y<br /><br />Physical Address Details:<br />Address Line 1: 670 YOUNG ST<br />City: TONAWANDA<br />State: NY<br />Zip: 141504103<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Result</th><th>Surety Code</th><th>Type</th><th>Amount</th><th>Port</th><th>Effective</th><th>Termination</th><th>Number</th><th>Bond is Sufficient?</th><th>Bond User Status</th><th>Bond User Termination Date</th></tr></thead><tr><td>Inactive</td><td>891</td><td>Importer or Broker</td><td>50000</td><td>9900</td><td>17-Feb-99</td><td>03-Feb-13</td><td>399901596</td><td>N</td><td>&nbsp;</td><td>&nbsp;</td></tr></table><br /><b>Some Continuous Bonds for this importer have expired and Bond Expiry Date has been set.</ b><br />", email.Body);
		}

		public void TestImportBondUserStatusAndTerminateDate()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var outgoing = CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", organisation);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K191-0131990001ATLAS INTERNATIONAL             891A0000500003001063001300107150 ",
"K2                                    020313 Y N A 063015                       ",
"K3C/O JOHN LITZLER, TRUSTEE       1412 MAIN ST FL 24                            ",
"K4ATLAS INTERNATIONAL             DALLAS               TX752024018              ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "~150000";
			processor.Message = responseMessage;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals(organisation, responseMessage.EM_LinkedObject);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Query Importer Bond"); }));
			Assert(sentMail.Body.Contains("<th>Result</th><th>Surety Code</th><th>Type</th><th>Amount</th><th>Port</th><th>Effective</th><th>Termination</th><th>Number</th><th>Bond is Sufficient?</th><th>Bond User Status</th><th>Bond User Termination Date</th></tr></thead><tr><td>Continuous</td><td>891</td><td>Importer or Broker</td><td>50000</td><td>3001</td><td>30-Jun-01</td><td>03-Feb-13</td><td>300107150</td><td>N</td><td>Active</td><td>30-Jun-15</td>"));

			outgoing = CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150001", organisation);
			Factory.Save();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K191-0131990001ATLAS INTERNATIONAL             891A0000500003001063001300107150 ",
"K2                                    020313 Y N                                ",
"K3C/O JOHN LITZLER, TRUSTEE       1412 MAIN ST FL 24                            ",
"K4ATLAS INTERNATIONAL             DALLAS               TX752024018              ");

			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "~150001";
			processor.Message = responseMessage;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals(organisation, responseMessage.EM_LinkedObject);

			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Query Importer Bond"); }));
			Assert(sentMail.Body.Contains("<th>Result</th><th>Surety Code</th><th>Type</th><th>Amount</th><th>Port</th><th>Effective</th><th>Termination</th><th>Number</th><th>Bond is Sufficient?</th><th>Bond User Status</th><th>Bond User Termination Date</th></tr></thead><tr><td>Continuous</td><td>891</td><td>Importer or Broker</td><td>50000</td><td>3001</td><td>30-Jun-01</td><td>03-Feb-13</td><td>300107150</td><td>N</td><td>&nbsp;</td><td>&nbsp;</td>"));
		}

		public void TestBondOnFile_ClearExpiryDate()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			var collection = new CusBondDetailCollection(organisation);
			var bondData = collection.AddNew();
			bondData.PW_BondNumber = "9907D5092";
			bondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData.PW_BondEffectiveDate = new ZDateTime(2007, 03, 30);
			bondData.PW_SuretyCode = "891";
			bondData.PW_BondAmount = 3000000;
			bondData.PW_BondFiledPort = "9900";
			bondData.PW_BondExpiryDate = new ZDateTime(2012, 12, 21);

			var bondData2 = collection.AddNew();
			bondData2.PW_BondNumber = "89890";
			bondData2.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondData2.PW_ActivityCode = ActivityCodeList.Codes._1a;
			bondData2.PW_SuretyCode = "891";
			bondData2.PW_BondAmount = 100000;
			bondData2.PW_BondEffectiveDate = new ZDateTime(2012, 09, 17);
			bondData2.PW_BondFiledPort = "9900";
			bondData2.PW_BondExpiryDate = new ZDateTime(2012, 09, 17);

			var outgoing = CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", organisation);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K123-1306344001FISHMAN & TOBIN INC             891A00300000099000330079907D5092 ",
"K3625 W RIDGE PIKE STE E320                                                     ",
"K4FISHMAN & TOBIN INC             CONSHOHOCKEN         PA19428321923-130634400  ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "~150000";
			processor.Message = responseMessage;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals(organisation, responseMessage.EM_LinkedObject);

			var wrapper = OrgHeaderWrapper.New(organisation);
			foreach (CusBondDetail bond in wrapper.BondDetails)
			{
				if (bond.PW_BondNumber == "9907D5092")
				{
					AssertEquals("Bond Expire date should be cleared, because bond is active and on Customs file", bond.PW_BondExpiryDate, ZDateTime.Empty);
				}
				else if (bond.PW_BondNumber == "89890")
				{
					AssertEquals("Bond not on file and Expire Date should not be changed", bond.PW_BondExpiryDate, new ZDateTime(2012, 09, 17));
				}
			}
		}

		public void TestBondChangesFromInsufficientFundToSufficientFund()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			var collection = new CusBondDetailCollection(organisation);
			var bondData = collection.AddNew();
			bondData.PW_BondNumber = "990900083";
			bondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData.PW_BondEffectiveDate = new ZDateTime(2009, 03, 10);
			bondData.PW_SuretyCode = "891";
			bondData.PW_BondAmount = 50000;
			bondData.PW_BondFiledPort = "9900";
			bondData.PW_BondExpiryDate = new ZDateTime(2012, 12, 21);

			var outgoing = CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", organisation);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K158-1234567891NWD ENT 12 B                    891A0000500009900031009990900083 ",
"K2                                    020313 Y N                                ");

			var responseMessageInsufficient = Factory.New<MQEDIMessage>();
			responseMessageInsufficient.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessageInsufficient.EM_MessageNum = "~150000";
			processor.Message = responseMessageInsufficient;

			processor.Process();
			AssertEquals(organisation, responseMessageInsufficient.EM_LinkedObject);

			var wrapper = OrgHeaderWrapper.New(organisation);
			var bondDetail = wrapper.BondDetails[0];

			AssertEquals("Bond has sufficient Fund", ZBool.False, bondDetail.HasSufficientFund);

			processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K158-1234567891NWD ENT 12 B                    891A0000500009900031009990900083 ",
"K2                                    020313 N Y                                ");

			var responseMessageSufficient = Factory.New<MQEDIMessage>();
			responseMessageSufficient.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessageSufficient.EM_MessageNum = "~150000";
			processor.Message = responseMessageSufficient;

			processor.Process();
			AssertEquals(organisation, responseMessageSufficient.EM_LinkedObject);

			AssertEquals("Bond has sufficient Fund", ZBool.True, bondDetail.HasSufficientFund);
		}

		public void TestSendK7Message()
		{
			CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", null);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K191-1325671001STARBUCKS CORPORATION           891A00080000099001220089908BS103 ",
"K7Test Test Test 1234            PNDING Pending Center Assignment               ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "~150000";
			processor.Message = responseMessage;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Query Importer Bond"); }));
			Assert(sentMail.Body.Contains("Full Legal Importer Name: Test Test Test 1234"));
			Assert(sentMail.Body.Contains("Center Identifier: PNDING"));
			Assert(sentMail.Body.Contains("Center ID Description: Pending Center Assignment"));
		}

		public void TestSendK7K8Message()
		{
			CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", null);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K191-1325671001STARBUCKS CORPORATION           891A00080000099001220089908BS103 ",
"K7Sunday is comeing i wanna driv CEE009 Industrial and Manufacturing M          ",
"K8IN1e my car                                                                   ",
"K8IN2aterials                                                                   ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "~150000";
			processor.Message = responseMessage;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Query Importer Bond"); }));
			Assert(sentMail.Body.Contains("Full Legal Importer Name: Sunday is comeing i wanna drive my car"));
			Assert(sentMail.Body.Contains("Center Identifier: CEE009"));
			Assert(sentMail.Body.Contains("Center ID Description: Industrial and Manufacturing Materials"));
		}

		public void TestSendK7K8MessageWhenK7FullNameEndWithSpace()
		{
			CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", null);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K191-1325671001STARBUCKS CORPORATION           891A00080000099001220089908BS103 ",
"K7I love working I love working  CEE009 Industrial and Manufacturing M          ",
"K8IN1Just a joke                                                                ",
"K8IN2aterials                                                                   ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "~150000";
			processor.Message = responseMessage;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Query Importer Bond"); }));
			Assert(sentMail.Body.Contains("Full Legal Importer Name: I love working I love working Just a joke"));
			Assert(sentMail.Body.Contains("Center Identifier: CEE009"));
			Assert(sentMail.Body.Contains("Center ID Description: Industrial and Manufacturing Materials"));
		}

		public void TestSendK7K8MessageWhenK8InformationEndWithSpaces()
		{
			CreateMessage(EDIMessage.Direction.Transmit, "SNT", ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "~150000", null);
			Factory.Save();

			var processor = new QueryImporterBondProcessor();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse,
"K191-1325671001STARBUCKS CORPORATION           891A00080000099001220089908BS103 ",
"K7Sunday is comeing i wanna driv CEE009 Industrial and Manufacturing M          ",
"K8IN1e my car                                                                   ",
"K8IN1To your apartment with a present like a start                              ",
"K8IN2aterials                                                                   ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "~150000";
			processor.Message = responseMessage;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Query Importer Bond"); }));
			Assert(sentMail.Body.Contains("Full Legal Importer Name: Sunday is comeing i wanna drive my car                                                              To your apartment with a present like a start"));
			Assert(sentMail.Body.Contains("Center Identifier: CEE009"));
			Assert(sentMail.Body.Contains("Center ID Description: Industrial and Manufacturing Materials"));
		}

		MQEDIMessage CreateMessage(ZString direction, ZString status, ZString messageType, ZString messageNumber, BusinessObject linkedObject)
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var result = mock.Object;
			result.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			result.EM_ReceiveTransmit = direction;
			result.EM_Status = status;
			result.EM_MessageType = messageType;
			result.EM_MessageNum = messageNumber;
			result.EM_LinkedObject = linkedObject;

			return result;
		}
	}
}
