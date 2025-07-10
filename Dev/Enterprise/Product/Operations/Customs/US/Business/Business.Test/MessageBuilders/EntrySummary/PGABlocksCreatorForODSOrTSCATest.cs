using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForODSOrTSCATest : PGABlocksCreatorTest
	{
		public void TestEAPPGALineNumber()
		{
			SetUpData();
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			invoiceLine.JI_Description = "MONOCHLORODIFLUOROMETHANE";
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCAODSCertIndividual = "CB";
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;

			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			var pstLine = invoiceLine.PSTLines.AddNew();
			pstLine.US_ProductType = PSTProductTypeList.Codes.PS1;

			var pstLine2 = invoiceLine.PSTLines.AddNew();
			pstLine2.US_ProductType = PSTProductTypeList.Codes.PS2;

			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			var vneLine = invoiceLine.VehicleLines.AddNew();
			vneLine.US_BodyDescription = "TST";
			var vneLine2 = invoiceLine.VehicleLines.AddNew();
			vneLine2.US_BodyDescription = "TST2";
			var vneLine3 = invoiceLine.VehicleLines.AddNew();
			vneLine3.US_BodyDescription = "TST3";

			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var messageText = message.EM_FormattedMessageText;

			AssertContains("PG01001EPAODS", messageText);
			AssertContains("PG01002EPATS1", messageText);
			AssertContains("PG01003EPAVNE", messageText);
			AssertContains("PG01004EPAVNE", messageText);
			AssertContains("PG01005EPAVNE", messageText);
			AssertContains("PG01006EPAPS1", messageText);
			AssertContains("PG01007EPAPS2", messageText);
		}

		public void TestAMSLineNumberCounter()
		{
			SetUpData();
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			invoiceLine.JI_Description = "MONOCHLORODIFLUOROMETHANE";
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;

			var ams1 = invoiceLine.AMSLines.AddNew();
			ams1.US_Program = AMSProgramList.Codes.MO1;
			var line1 = ams1.AMSLines.AddNew();
			line1.US_Packages = 1m;

			var ams2 = invoiceLine.AMSLines.AddNew();
			ams2.US_Program = AMSProgramList.Codes.MO2;
			var line2 = ams2.AMSLines.AddNew();
			line2.US_CertNumber = "123456";
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var messageText = message.EM_FormattedMessageText;
			AssertContains("PG01001AMSMO 1", messageText);
			AssertContains("PG01002AMSMO 2", messageText);
		}

		public void TestAPHLineNumberCounter()
		{
			SetUpData();
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			invoiceLine.JI_Description = "MONOCHLORODIFLUOROMETHANE";
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			var line1 = invoiceLine.LaceyActLines.AddNew();
			line1.US_PGACommercialDescription = "BAD";

			var line2 = invoiceLine.APHISHeaders.AddNew();
			line2.US_CategoryCode = "1";
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var messageText = message.EM_FormattedMessageText;
			AssertContains("PG01001APH", messageText);
			AssertContains("PG01002APH", messageText);
		}

		public void TestVNEComponentLevelReportingWhenPG10V05ENG()
		{
			SetUpData();
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;

			var vneLine = invoiceLine.VehicleLines.AddNew();
			vneLine.US_CertOfConformity = "ENGINE FAMILY";

			var vehicleDetails1 = vneLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails1.US_EngineNumber = "MHO-874200";
			vehicleDetails1.US_IdentityNumber = "1337";
			vehicleDetails1.US_MfrDateType = ManufactureDateTypeList.Codes.ENG;

			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			AssertContains("The PG10 V05 ENG should be reported at the component level (PG02C)", @"
PG02P                                                                           
PG07                                                           1337             
PG14 EP4ENGINE FAMILY                                                           
PG02C                                                                           
PG07                                                        ENNMHO-874200       
PG14 EP4ENGINE FAMILY                                                           
PG10           V05 ENG                                                          ",
			message.EM_FormattedMessageText);
		}

		public void TestDisclaimersLineCount()
		{
			SetUpData();
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			invoiceLine.JI_Description = "MONOCHLORODIFLUOROMETHANE";
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_ODSDisclaimReason = "A";
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_PSTDisclaimReason = "B";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_VNEDisclaimReason = "C";

			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var messageText = message.EM_FormattedMessageText;
			AssertContains("PG01001EPA", messageText);
			AssertContains("PG01002EPA", messageText);
			AssertContains("PG01003EPA", messageText);
		}

		public void TestTSCASendWithDisclaimed()
		{
			SetUpData();
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.JI_Description = "TEST FOR ENTRYSUMMERY";
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;
			invoiceLine.US_TSCADisclaimReason = PGADisclaimReasonList.Codes.D;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains("ACE Entry Summary message including EPA TSCA data",
@"OI        TEST FOR ENTRYSUMMERY                                                 
PG01001EPATS1                                                                  D", message.EM_FormattedMessageText);
		}

		public void TestTSCASample()
		{
			/*A shipment of Aluminum Chloride is imported into the United States. It is regulated by TSCA and is listed in the TSCA Chemical Inventory, 
			 * so the filer submits a positive TSCA certification PGA Message Set.*/
			SetUpData();
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			invoiceLine.JI_Description = "ALUMINUM CHLORIDE";
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCAODSCertIndividual = "CB";
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var expectedPattern = @"OI        ALUMINUM CHLORIDE                                                     
PG01001EPATS1                                                                   
PG02P                                                                           
PG22             CI EP4 Y{0}                                               
PG21CI                                                                          ";
			AssertContains(
				message.EM_FormattedMessageText,
				ZString.Format(expectedPattern, ZDate.Today.ToString("MMddyyyy")),
				message.EM_FormattedMessageText);

			invoiceLine.US_TSCAODSCertIndividual = "IM";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add);
			message = builder.PopulateMessage();
			expectedPattern = @"OI        ALUMINUM CHLORIDE                                                     
PG01001EPATS1                                                                   
PG02P                                                                           
PG22             CI EP4 Y{0}                                               
PG21CI IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          ";
			AssertContains(
				message.EM_FormattedMessageText,
				ZString.Format(expectedPattern, ZDate.Today.ToString("MMddyyyy")),
				message.EM_FormattedMessageText);
		}

		public void TestODSSample()
		{
			SetUpData();
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.DefaultFDAContact();
			invoiceLine.JI_Description = "MONOCHLORODIFLUOROMETHANE";
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCAODSCertIndividual = "CB";
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var expectedPattern = @"OI        MONOCHLORODIFLUOROMETHANE                                             
PG01001EPAODS                                                                   
PG02P                                                                           
PG22             CI EP4 Y{0}                                               
PG21CI BROKER                 04010101       BROKER EMAIL                       ";
			AssertContains(
				message.EM_FormattedMessageText,
				ZString.Format(expectedPattern, ZDate.Today.ToString("MMddyyyy")),
				message.EM_FormattedMessageText);

			invoiceLine.US_TSCAODSCertIndividual = "IM";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add);
			message = builder.PopulateMessage();
			expectedPattern = @"OI        MONOCHLORODIFLUOROMETHANE                                             
PG01001EPAODS                                                                   
PG02P                                                                           
PG22             CI EP4 Y{0}                                               
PG21CI IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          ";
			AssertContains(
				message.EM_FormattedMessageText,
				ZString.Format(expectedPattern, ZDate.Today.ToString("MMddyyyy")),
				message.EM_FormattedMessageText);
		}

		public void TestBothDeclaredODSAndTSCA()
		{
			SetUpData();
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			invoiceLine.JI_Description = "MONOCHLORODIFLUOROMETHANE";
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			invoiceLine.US_TSCAODSCertIndividual = "CB";
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var expectedPattern = @"OI        MONOCHLORODIFLUOROMETHANE                                             
PG01001EPAODS                                                                   
PG01002EPATS1                                                                   
PG02P                                                                           
PG22             CI EP4 Y{0}                                               
PG21CI                                                                          ";
			AssertContains(message.EM_FormattedMessageText, ZString.Format(expectedPattern, ZDate.Today.ToString("MMddyyyy")), message.EM_FormattedMessageText);
		}

		public void TestDisclaimODSAndDeclaredTSCA()
		{
			SetUpData();
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			invoiceLine.JI_Description = "MONOCHLORODIFLUOROMETHANE";
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_ODSDisclaimReason = "A";

			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;
			invoiceLine.US_TSCAODSCertIndividual = "IM";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var expectedPattern = @"OI        MONOCHLORODIFLUOROMETHANE                                             
PG01001EPAODS                                                                  A
PG01002EPATS1                                                                   
PG02P                                                                           
PG22             CI EP4 Y{0}                                               
PG21CI IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          ";
			AssertContains(
				message.EM_FormattedMessageText,
				ZString.Format(expectedPattern, ZDate.Today.ToString("MMddyyyy")),
				message.EM_FormattedMessageText);
		}

		public void TestDisclaimTSCAAndDeclaredODS()
		{
			SetUpData();
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			invoiceLine.JI_Description = "MONOCHLORODIFLUOROMETHANE";
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_TSCADisclaimReason = "A";

			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;
			invoiceLine.US_TSCAODSCertIndividual = "IM";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var expectedPattern = @"OI        MONOCHLORODIFLUOROMETHANE                                             
PG01001EPAODS                                                                   
PG01002EPATS1                                                                  A
PG02P                                                                           
PG22             CI EP4 Y{0}                                               
PG21CI IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          ";
			AssertContains(
				message.EM_FormattedMessageText,
				ZString.Format(expectedPattern, ZDate.Today.ToString("MMddyyyy")),
				message.EM_FormattedMessageText);
		}

		public void TestBothDisclaimTSCAAndODS()
		{
			SetUpData();
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			invoiceLine.JI_Description = "MONOCHLORODIFLUOROMETHANE";
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_TSCADisclaimReason = "A";

			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_ODSDisclaimReason = "A";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(message.EM_FormattedMessageText,
@"OI        MONOCHLORODIFLUOROMETHANE                                             
PG01001EPAODS                                                                  A
PG01002EPATS1                                                                  A", message.EM_FormattedMessageText);
		}

		public void TestDisclaimODS()
		{
			SetUpData();
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			invoiceLine.JI_Description = "MONOCHLORODIFLUOROMETHANE";
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_ODSDisclaimReason = "A";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(message.EM_FormattedMessageText,
@"OI        MONOCHLORODIFLUOROMETHANE                                             
PG01001EPAODS                                                                  A", message.EM_FormattedMessageText);
		}

		public void TestEntityNameAndEmailOverflow()
		{
			SetUpData();
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var iorWrapper = declaration.IORWrapper;
			DeclarationTestHelper.AddPGAContact(iorWrapper.organisation, "IOR", "ALEXANDER THE GREATEST OF ALL", null, "THIS.IS.A.VERY.LONG.EMAIL@ABCDEFG.COM", null);

			invoiceLine.JI_Description = "MONOCHLORODIFLUOROMETHANE";
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;
			invoiceLine.US_TSCAODSCertIndividual = "IM";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var expectedPattern = @"OI        MONOCHLORODIFLUOROMETHANE                                             
PG01001EPAODS                                                                   
PG02P                                                                           
PG22             CI EP4 Y{0}                                               
PG21CI IOR ALEXANDER THE GREAT04123456       THIS.IS.A.VERY.LONG.EMAIL@ABCDEFG.C
PG60INAEST OF ALL                                                               
PG60EMAOM                                                                       ";
			Assert("PG60 blocks for Individual name and email should be generated",
				message.EM_FormattedMessageText.Contains(ZString.Format(expectedPattern, ZDate.Today.ToString("MMddyyyy"))
));
		}

		[TestDate(2016, 11, 14)]
		public void TestTSCANotIncludedInPGADataCorrectionWhenAdded()
		{
			SetUpData();
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			invoiceLine.JI_Description = "TEST TSCA AGAINST WITH FDA";
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;
			invoiceLine.US_TSCAODSCertIndividual = "IM";
			invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Added;

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_BLO;
			fda.US_ProductCode = "57UH-12";
			fda.US_ProdCountry = "ES";
			fda.US_Description = "FLEBOGAMMA 5% 400ML";
			fda.US_TotalValue = 43466769m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new PGACorrectionMessageBuilder(entry, ACEEntrySummaryMessageSendingOption.New(), false);
			builder.GenerateMessage();
			AssertEquals(1, entry.Messages.Count);
			Assert(entry.Messages[0].EM_FormattedMessageText.Contains(
@"OI        TEST TSCA AGAINST WITH FDA                                            
PG01001FDABIOBLO                                                                
PG02PFDP 57UH-12                                                                
PG0639 ES                                                                       
PG10                   FLEBOGAMMA 5% 400ML                                      
PG19FD1                  IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21FD1IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG25                                                    000000010000            "));
		}

		public void TestCertifyingDateIsSetToCurrentDateIfEmpty()
		{
			SetUpData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "ACE";
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = "SE";

			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCAODSCertIndividual = "CB";
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;

			invoiceLine.InvoiceHeader.US_TSCASignDate = ZDateTime.Empty;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var today = ZDate.Today;
			AssertContains("PG22             CI EP4 Y" + today.ToString("MMddyyyy"), message.EM_FormattedMessageText);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_TSCASignDate = new ZDate(2016, 11, 29);
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			AssertContains("PG22             CI EP4 Y11292016", message.EM_FormattedMessageText);
		}

		[TestDate(2017, 8, 9)]
		public void TestNoExceptionThrownWhenGenerateEPABlocks()
		{
			SetUpData();
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.DefaultFDAContact();

			invoiceLine.JI_Description = "TEST EPA DOES NOT THROW EXCEPTION";
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;
			invoiceLine.US_TSCAODSCertIndividual = "IM";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new PGACorrectionMessageBuilder(entry, ACEEntrySummaryMessageSendingOption.New(), false);
			builder.GenerateMessage();
			AssertEquals(1, entry.Messages.Count);
			Assert(entry.Messages[0].EM_FormattedMessageText.Contains(
@"OI        TEST EPA DOES NOT THROW EXCEPTION                                     
PG01001EPATS1                                                                   
PG02P                                                                           
PG22             CI EP4 Y08092017                                               
PG21CI IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          "));

			invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Added;
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_PSTDisclaimReason = PGADisclaimReasonList.Codes.B;

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_BLO;
			fda.US_ProductCode = "57UH-12";
			fda.US_ProdCountry = "ES";
			fda.US_Description = "FLEBOGAMMA 5% 400ML";
			fda.US_TotalValue = 43466769m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry.Messages.RemoveAndDeleteAll();
			builder = new PGACorrectionMessageBuilder(entry, ACEEntrySummaryMessageSendingOption.New(), false);
			builder.GenerateMessage();
			AssertEquals(1, entry.Messages.Count);
			Assert(entry.Messages[0].EM_FormattedMessageText.Contains(
@"OI        TEST EPA DOES NOT THROW EXCEPTION                                     
PG01001FDABIOBLO                                                                
PG02PFDP 57UH-12                                                                
PG0639 ES                                                                       
PG10                   FLEBOGAMMA 5% 400ML                                      
PG19FD1                  IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21FD1IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21PK BROKER                 04010101       BROKER EMAIL                       
PG25                                                    000000010000            "));
		}

		protected override void SetUpData()
		{
			base.SetUpData();
			declaration.IOROrgPK = GetIOROrgHeader().PK;
			declaration.JE_GS_NKCusAgent = GetBroker().GS_Code;
		}

		OrgHeader GetIOROrgHeader()
		{
			var ior = Factory.New<OrgHeader>();
			declaration.IOROrgPK = ior.PK;
			ior.OH_FullName = "IMPORTER OF RECORD";
			var iorAddress = ior.MainAddress;
			iorAddress.OA_RL_NKRelatedPortCode = "US2CW";
			iorAddress.OA_Address1 = "IOR ADDRESS 1";
			iorAddress.OA_Address2 = "IOR ADDRESS 2";
			iorAddress.OA_City = "SYDNEY";
			iorAddress.OA_State = "NSW";
			iorAddress.OA_PostCode = "2017";
			DeclarationTestHelper.AddPGAContact(ior, "IOR", "ALEXANDER THE GREAT", "04 123456", "IOR EMAIL", "IOR FAX");

			return ior;
		}

		GlbStaff GetBroker()
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "KNZ";
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			broker.GS_FullName = "BROKER";
			broker.GS_WorkPhone = "04 010101";
			broker.GS_EmailAddress = "BROKER EMAIL";
			broker.GS_FaxNum = "BROKER FAX";
			return broker;
		}
	}
}
