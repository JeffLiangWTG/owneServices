using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class ImporterAddMessageProcessorTest : ABIProcessorTest<ImporterAddMessageProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			var image1 = new System.Drawing.Bitmap(1, 2);
			Registry.Business.SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);

			BlockControlGenerator block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ACEApplicationIdentifierCodeList.Codes.Add5106toImporterFileProcessingResults,
				"T2                                            CHICAGO              IL60010    US",
				"T385-2574189AB60LCOUNTRY CODE(S) NOT ALLOWED   test   SHIPPING AGENCIES P/L     "
				);
			foreach (MessageBlock messageBlock in block.MessageBlocks)
			{
				processor.AddMessageBlock(messageBlock);
			}

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.Add5106toImporterFileProcessingResults;
			processor.Process();
			Factory.Save();

			ZQuery query = new ZQuery(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			query.AddToFilter(OrgCountryDataSchema.OV_OH_OrgHeader, organisation.PK);
			OrgCountryData countryData = Factory.LoadTop1<OrgCountryData>(query);
			OrgImpAddInfo addInfo = new OrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);

			AssertEquals(organisation, message.EM_LinkedObject);
			AssertEquals(YesNoDefaultList.Codes.No, OrgHeaderWrapper.New(organisation).ZO_IsEINNumberVerifiedIndicator);

			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Importer Add"); }));
			AssertEquals("attachment", 2, email.Attachments.Count);

			var banner = email.Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(1, image.Width);
		}

		public void TestPrintErrorDescriptionInEmail()
		{
			BlockControlGenerator block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ACEApplicationIdentifierCodeList.Codes.Add5106toImporterFileProcessingResults,
				"T2                                            CHICAGO              IL60010    US",
				"T385-2574189AB60LCOUNTRY CODE(S) NOT ALLOWED   test   SHIPPING AGENCIES P/L     "
				);
			foreach (MessageBlock messageBlock in block.MessageBlocks)
			{
				processor.AddMessageBlock(messageBlock);
			}
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.Add5106toImporterFileProcessingResults;
			processor.Process();
			Factory.Save();

			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Importer Add"); }));
			AssertNotNull(email);
			AssertContains("COUNTRY CODES DO NOT APPLY TO COUNTRY RESPONSIBILITY CODE = 0.", email.Body);
		}

		public void TestProcessTRMessage()
		{
			AssertNoExceptionThrown(
				"Generating message blocks from TR Message throws exception",
				() => OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(
							ACEApplicationIdentifierCodeList.Codes.Add5106toImporterFileProcessingResults,
							"B002704EL6TR                                               TTCLAXLAX_288        ",
							"TADBA HI DESIG                                                                  ",
							"T345-430199200AB0LEADING SPACE/SPEC CHAR IN NAMMEROOJAN MIRIJANIAN              ",
							"Y  2704EL6TR00002"
				));
		}

		public void TestWhenImporterAlreadyOnFile_SetZO_IsEINNumberVerifiedIndicator()
		{
			BlockControlGenerator block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ACEApplicationIdentifierCodeList.Codes.Add5106toImporterFileProcessingResults,
				"T1A85-2574189ABMAXS IMPORTS                    4160 E 15 MILE RD               C",
				"T385-2574189ABACYIMPORTER NUMBER ALREADY ON FILMAXS IMPORTS                     "
				);

			foreach (MessageBlock messageBlock in block.MessageBlocks)
			{
				processor.AddMessageBlock(messageBlock);
			}
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.Add5106toImporterFileProcessingResults;

			var orgWrapper = OrgHeaderWrapper.New(organisation);
			orgWrapper.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.No;
			AssertEquals("pre-condition", YesNoDefaultList.Codes.No, orgWrapper.ZO_IsEINNumberVerifiedIndicator);

			processor.Process();

			Factory.Save();

			ZQuery query = new ZQuery(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			query.AddToFilter(OrgCountryDataSchema.OV_OH_OrgHeader, organisation.PK);
			OrgCountryData countryData = Factory.LoadTop1<OrgCountryData>(query);
			OrgImpAddInfo addInfo = new OrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);

			AssertEquals(organisation, message.EM_LinkedObject);
			AssertEquals(YesNoDefaultList.Codes.Yes, OrgHeaderWrapper.New(organisation).ZO_IsEINNumberVerifiedIndicator);
		}

		public void TestProcessSuccessfulResponse()
		{
			BlockControlGenerator block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ACEApplicationIdentifierCodeList.Codes.Add5106toImporterFileProcessingResults,
				"T4YYDDPP-NNNNN2EEDATABASE NOT UPDATED TEST MODEtest   SHIPPING AGENCIES P/L"
				);
			foreach (MessageBlock messageBlock in block.MessageBlocks)
			{
				processor.AddMessageBlock(messageBlock);
			}
			AssertEquals("", organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.CBPAssignedNumber));
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.Add5106toImporterFileProcessingResults;

			processor.Process();
			Factory.Save();

			ZQuery query = new ZQuery(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			query.AddToFilter(OrgCountryDataSchema.OV_OH_OrgHeader, organisation.PK);
			OrgCountryData countryData = Factory.LoadTop1<OrgCountryData>(query);
			OrgImpAddInfo addInfo = new OrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);

			AssertEquals(organisation, message.EM_LinkedObject);
			AssertEquals(YesNoDefaultList.Codes.Yes, OrgHeaderWrapper.New(organisation).ZO_IsEINNumberVerifiedIndicator);
			AssertEquals("YYDDPP-NNNNN", organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.CBPAssignedNumber));

			AssertNotNull("An email with subject containing 'Importer Add' should have been created.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Importer Add"); })));
		}

		public void TestProcessErrorResponseForIsEINNumberVerifiedIndicator()
		{
			BlockControlGenerator block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ACEApplicationIdentifierCodeList.Codes.Add5106toImporterFileProcessingResults,
				"T1A98-123412343DAANET PTY LTD                  1444                            C",
				"T398-12341234360LCOUNTRY CODE(S) NOT ALLOWED   DAANET PTY LTD                   "
				);
			foreach (MessageBlock messageBlock in block.MessageBlocks)
			{
				processor.AddMessageBlock(messageBlock);
			}
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.Add5106toImporterFileProcessingResults;

			processor.Process();
			Factory.Save();

			ZQuery query = new ZQuery(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			query.AddToFilter(OrgCountryDataSchema.OV_OH_OrgHeader, organisation.PK);
			OrgCountryData countryData = Factory.LoadTop1<OrgCountryData>(query);
			OrgImpAddInfo addInfo = new OrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);

			AssertEquals(organisation, message.EM_LinkedObject);
			AssertEquals(YesNoDefaultList.Codes.No, OrgHeaderWrapper.New(organisation).ZO_IsEINNumberVerifiedIndicator);
		}

		public void TestProcessErrorResponseForT345RecordType5()
		{
			BlockControlGenerator block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ACEApplicationIdentifierCodeList.Codes.Add5106toImporterFileProcessingResults,
				"B005201DM4TR                                               CRWMIAJAX_54685      ",
				"T513-406366700K32DATA UPDATED ON DATABASE      ASB PRODUCE, INC                 ",
				"Y  5201DM4TR00001"
				);
			foreach (MessageBlock messageBlock in block.MessageBlocks)
			{
				processor.AddMessageBlock(messageBlock);
			}
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.Add5106toImporterFileProcessingResults;

			processor.Process();
			Factory.Save();

			Assert("Message body should contain narrative", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("K32DATA UPDATED ON DATABASE"));
		}

		public void TestObjectNullReferenceException_CS00153863()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = MQEDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageNum = "37468";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.Add5106toImporterFileProcessingResults;
			Factory.Save();

			message.EM_MessageNum = "37468";
			message.EM_MessageText = "B002720267TR                                               37468                T1A95-411689300HENSEL VISIT USA                543 COUNTRY CLUB DRIVE          CT395-411689300ACYIMPORTER NUMBER ALREADY ON FILHENSEL VISIT USA                 Y  2720267TR00002";

			processor.Message = message;
			AssertNoExceptionThrown(delegate
			{ processor.Process(); });
		}

		public void TestProcessingError2EEDoesNotReportToUs()
		{
			MessageError error2EE = ABIError.Instance.GetErrorInfoByCode("2EE");
			AssertNotNull(error2EE);

			string responseData =
				"T1A22-222222200JIM Q ENTERPRISES               1515 WOODFIELD ROAD             S" +
				"T322-2222222002EEDATABASE NOT UPDATED TEST MODEJIM Q ENTERPRISES                ";
			AssertErrorCodeNotReportedToUs(responseData, true, "DATABASE NOT UPDATED TEST MODE");
		}

		void AssertErrorCodeNotReportedToUs(string responseData, bool isFailure, params string[] expectedBodyMessages)
		{
			message.EM_MessageText =
				"B018888XJ5TR                                               ~15000".PadRight(80) +
				responseData +
				"Y 8888XJ5SN00002";
			message.EM_SystemCreateUser = staffZ2.GS_Code;
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);

			string subject = string.Format("Importer Add Response {0}for {1}", isFailure ? "(Failure) " : "", "Organisation " + organisation.OH_Code);
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == subject; }));
			Assert("Email Send to Notify Group", email.Recipients.Contains("dong2@pretend.email.com"));
			foreach (string expectedBodyMessage in expectedBodyMessages)
			{
				AssertContains(expectedBodyMessage, email.Body);
			}
			AssertEquals("", ErrorReporter.LastKeyReported);
		}

		OrgHeader organisation;
		MQEDIMessage message;
		MQEDIMessage outgoing;
		ImporterAddMessageProcessor processor;

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();

			organisation.OH_FullName = "test   SHIPPING AGENCIES P/L";
			OrgCusCode cusCode = organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "85-2574189AB");

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			outgoing = mock.Object;
			outgoing.EM_ApplicationCode = MQEDIMessage.ApplicationCodes.USCustomsImport;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_MessageNum = "~15000";
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.AddCBPFormCBPF5106DatatotheImporterFile;
			outgoing.EM_MessageText = "B01       TI                                                                    Y         TI00001000000000000000000000000";
			outgoing.EM_LinkedObject = organisation;

			message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = MQEDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.Add5106toImporterFileProcessingResults;
			Factory.Save();

			processor = new ImporterAddMessageProcessor();
			processor.Message = message;
			AssertEquals(outgoing.PK, message.OriginalMessage.PK);
		}
	}
}
