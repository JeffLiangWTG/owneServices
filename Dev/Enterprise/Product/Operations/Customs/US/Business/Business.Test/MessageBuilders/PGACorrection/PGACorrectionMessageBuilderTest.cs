using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PGACorrectionMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2017, 06, 20)]
		public void TestSendEndToEndWithNMFSCOAData()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.PGADataCorrection2ndPhase, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EntryFilerCode = "XJ5";
				declaration.US_EnableENS = true;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();

				invoiceLine.US_NMFSCOAInd = OGAIndicatorList.Codes.Declared;
				var nmfs = invoiceLine.NMFSLines.AddNew();
				nmfs.US_ProgramType = NMFSProgramCodeList.Codes.COA;
				DeclarationTestHelper.SetupNMFSCOAData(nmfs, invoiceLine, Factory);
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				var ensEntry = declaration.FormalEntry;
				new PGACorrectionMessageBuilder(ensEntry, ACEEntrySummaryMessageSendingOption.New(), false).GenerateMessage();
				AssertEquals(1, ensEntry.Messages.Count);
				AssertContains(@"OI                                                                              
PG01001NMFCOA    Y                                                              
PG02P                                                                           
PG05                                                              ADD           
PG06HCFAUCAR                 02152023        GIL                                
PG142NM4123456789                                                               
PG22Y894            COA1                                                        ", ensEntry.Messages[0].EM_FormattedMessageText);
			}
		}

		[TestDate(2016, 8, 1)]
		public void TestGenerate()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.PGADataCorrection2ndPhase, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				dec.US_EntryFilerCode = "XJ5";
				dec.US_EnableENS = true;
				dec.US_SchDArrival = "1101";
				var invoice = dec.Invoices.AddNew();
				var invoiceLine0 = invoice.InvoiceLines.AddNew();
				invoiceLine0.JI_Tariff = "9801001000";
				invoiceLine0.JI_Description = "Line 0";
				invoiceLine0.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine0.US_APHISDisclaimReason = PGADisclaimReasonList.Codes.A;
				var aphisLine0 = invoiceLine0.APHISHeaders.AddNew();
				aphisLine0.US_LineNo = 1;
				aphisLine0.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;

				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				invoiceLine1.JI_Tariff = "9801001001";
				invoiceLine1.JI_Description = "Line 1";
				invoiceLine1.US_APHISInd = OGAIndicatorList.Codes.Declared;
				var aphisLine1 = invoiceLine1.APHISHeaders.AddNew();
				aphisLine1.US_LineNo = 1;
				aphisLine1.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;

				var secondaryLine = invoiceLine1.AddSecondaryInvoiceLine();
				secondaryLine.JI_Tariff = "9801001002";
				secondaryLine.JI_Description = "Line 2";
				secondaryLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
				var aphisLine2 = secondaryLine.APHISHeaders.AddNew();
				aphisLine2.US_LineNo = 1;
				aphisLine2.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
				var aphisLine3 = secondaryLine.APHISHeaders.AddNew();
				aphisLine3.US_LineNo = 2;
				aphisLine3.US_TrackingStatus = PGATrackingStatusList.Codes.Added;

				var secondaryLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				secondaryLine2.JI_Tariff = "9801001003";
				secondaryLine2.JI_Description = "Line 2 - 1";
				secondaryLine2.US_APHISInd = OGAIndicatorList.Codes.Declared;
				var aphisLine4 = secondaryLine2.APHISHeaders.AddNew();
				aphisLine4.US_LineNo = 1;
				aphisLine4.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
				var aphisLine5 = secondaryLine2.APHISHeaders.AddNew();
				aphisLine5.US_LineNo = 2;
				aphisLine5.US_TrackingStatus = PGATrackingStatusList.Codes.Added;

				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "9801001004";
				invoiceLine2.JI_Description = "Line 3";
				invoiceLine2.US_TSCAInd = OGAIndicatorList.Codes.Declared;
				invoiceLine2.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;

				var invoiceLine3 = invoice.InvoiceLines.AddNew();
				invoiceLine3.JI_Tariff = "9801001005";
				invoiceLine3.JI_Description = "Line 4";
				invoiceLine3.US_DDTCInd = OGAIndicatorList.Codes.Declared;
				invoiceLine3.US_DDTCTrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;

				var invoiceLine4 = invoice.InvoiceLines.AddNew();
				invoiceLine4.JI_Tariff = "9801001006";
				invoiceLine4.JI_Description = "Line 5";
				invoiceLine4.US_ATFInd = OGAIndicatorList.Codes.Declared;
				var atfLine1 = invoiceLine4.ATFLines.AddNew();
				atfLine1.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
				var atfLine2 = invoiceLine4.ATFLines.AddNew();
				atfLine2.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;

				dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				Factory.Save();

				var seEntry = dec.ActiveEntryHeaders.SimplifiedEntry;
				invoice.US_TSCASignDate = ZDateTime.Today;

				new PGACorrectionMessageBuilder(seEntry, ACEEntrySummaryMessageSendingOption.New(), false).GenerateMessage();
				AssertEquals(1, seEntry.Messages.Count);
				AssertMultilineASCIIEquals("PGA Correction",
	@"B         CA                                               <<MSGNO PLACEHOLDER>>
CA10RXJ500000014                                                                
CA4000001                                                                       
CA609801001000                                                                  
OI        LINE 0                                                                
PG01001APHAVS                                                                  A
CA4000002                                                                       
CA609801001001                                                                  
CA609801001002                                                                  
CA609801001003                                                                  
OI        LINE 2                                                                
PG01001APH       Y                                                              
PG02P                                                                           
PG10                                                                            
PG19CB 336XJ5            EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG30A            2   1101                                                       
PG01002APH       Y                                                              
PG02P                                                                           
PG10                                                                            
PG19CB 336XJ5            EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG30A            2   1101                                                       
PG01003APH       Y                                                              
PG02P                                                                           
PG10                                                                            
PG19CB 336XJ5            EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG30A            2   1101                                                       
CA4000003                                                                       
CA609801001004                                                                  
OI        LINE 3                                                                
PG01001EPATS1                                                                   
PG02P                                                                           
PG22             CI EP4 Y08012016                                               
PG21CI                                                                          
CA4000004                                                                       
CA609801001005                                                                  
OI        LINE 4                                                                
PG01000DTCCOR                                                                 D 
CA4000005                                                                       
CA609801001006                                                                  
OI        LINE 5                                                                
PG01000ATFCOR                                                                 D 
Y         CA", seEntry.Messages[0].EM_FormattedMessageText);
			}
		}

		public void TestGenerateMesageForRLF()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.PGADataCorrection2ndPhase, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				dec.US_EntryFilerCode = "XJ5";
				dec.US_EnableENS = true;
				dec.US_SchDEntry = "3901";
				dec.US_SchDArrival = "3901";
				dec.US_EntryMode = EntryModeList.Codes.RLF;
				dec.US_PreparerDistrictPort = "2809";

				var invoice = dec.Invoices.AddNew();
				var invoiceLine0 = invoice.InvoiceLines.AddNew();
				invoiceLine0.JI_Tariff = "0";
				invoiceLine0.JI_Description = "Line 0";
				invoiceLine0.US_APHISInd = OGAIndicatorList.Codes.Declared;
				var aphisLine0 = invoiceLine0.APHISHeaders.AddNew();
				aphisLine0.US_LineNo = 1;
				aphisLine0.US_ProgramType = APHISProgramCodeList.Codes.AVS;

				dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				Factory.Save();

				var seEntry = dec.ActiveEntryHeaders.SimplifiedEntry;
				new PGACorrectionMessageBuilder(seEntry, ACEEntrySummaryMessageSendingOption.New(), false).GenerateMessage();
				AssertEquals(1, seEntry.Messages.Count);
				AssertMultilineASCIIEquals("PGA Correction",
	@"B  2809   CA                                               <<MSGNO PLACEHOLDER>>
CA10RXJ500000014                                                                
CA4000001                                                                       
CA600                                                                           
OI        LINE 0                                                                
PG01001APHAVS    Y                                                              
PG02P                                                                           
PG10                                                                            
PG19CB 336XJ5            EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG30A            2   3901                                                       
Y  2809   CA", seEntry.Messages[0].EM_FormattedMessageText);
			}
		}

		public void TestBuildPGAMessageBlocksForSecondaryTariffLine()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.PGADataCorrection2ndPhase, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				var tariff0 = Factory.New<USCTariff>();
				tariff0.UE_Tariff = "9608500000";
				tariff0.UE_PGACodes = "EP8";
				tariff0.UE_DateFrom = ZDate.Today.AddMonths(-1);
				tariff0.UE_DateTo = ZDate.Today.AddMonths(1);

				var tariff1 = Factory.New<USCTariff>();
				tariff1.UE_Tariff = "9608100000";
				tariff1.UE_PGACodes = "EP8";
				tariff1.UE_DateFrom = ZDate.Today.AddMonths(-1);
				tariff1.UE_DateTo = ZDate.Today.AddMonths(1);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EntryFilerCode = "XJ5";
				declaration.US_EnableCRL = true;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine0 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine0.JI_Tariff = tariff0.UE_Tariff;
				invoiceLine0.JI_Description = "TEST PARENT TSCA";
				invoiceLine0.US_TSCAInd = OGAIndicatorList.Codes.Declared;
				invoiceLine0.US_TSCAIndicator = TSCAIndicatorList.Codes.TSCAPositive;
				invoiceLine0.US_TSCAODSCertIndividual = PartyTypeList.Codes.Importer;
				invoiceLine0.US_FDAContactName = "PARENT IMPORTER";

				var invoiceLine1 = declaration.InvoiceLines.AddNew();
				invoiceLine1.JI_Tariff = tariff1.UE_Tariff;
				invoiceLine1.JI_ParentID = invoiceLine0.PK;
				invoiceLine1.JI_Description = "TEST SECONDARY TSCA";
				invoiceLine1.US_TSCAInd = OGAIndicatorList.Codes.Declared;
				invoiceLine1.US_TSCAIndicator = TSCAIndicatorList.Codes.TSCANegative;
				invoiceLine1.US_TSCAODSCertIndividual = PartyTypeList.Codes.CustomsBroker;
				invoiceLine1.US_FDAContactName = "SECONDARY BROKER";

				invoice.US_TSCASignDate = new ZDateTime(2016, 11, 30);
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;

				new PGACorrectionMessageBuilder(seEntry, ACEEntrySummaryMessageSendingOption.New(), false).GenerateMessage();
				AssertEquals(1, seEntry.Messages.Count);
				AssertMultilineASCIIEquals("PGA Correction, only 1 OI block generated",
	@"B         CA                                               <<MSGNO PLACEHOLDER>>
CA10RXJ5                                                                        
CA4000001                                                                       
CA609608500000                                                                  
CA609608100000                                                                  
OI        TEST PARENT TSCA                                                      
PG01001EPATS1                                                                   
PG02P                                                                           
PG22             CI     Y11302016                                               
PG21CI PARENT IMPORTER                                                          
PG01002EPATS1                                                                   
PG02P                                                                           
PG22             CI     Y11302016                                               
PG21CI SECONDARY BROKER                                                         
Y         CA", seEntry.Messages[0].EM_FormattedMessageText);
			}
		}

		public void TestSendOIBlockWhenPGAOnlyExistsInSecondaryTariff()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.PGADataCorrection2ndPhase, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			using (USCustomsDataRegistry.Instance.EntryDeclarant.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EntryFilerCode = "XJ5";
				declaration.US_EnableENS = true;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine0 = invoice.InvoiceLines.AddNew();
				invoiceLine0.JI_Tariff = "9801001000";
				invoiceLine0.JI_Description = "Line 0";
				var secondaryLine = invoiceLine0.AddSecondaryInvoiceLine();
				secondaryLine.JI_Tariff = "9801001002";
				secondaryLine.JI_Description = "Line 1";
				secondaryLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
				var secondaryFDALine1 = secondaryLine.ACE_FDALines.AddNew();
				secondaryFDALine1.US_LineNo = 1;
				secondaryFDALine1.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
				secondaryFDALine1.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_NED;
				secondaryFDALine1.US_ProductCode = "80K--YZ";
				secondaryFDALine1.US_IntendedUseCode = IntendedUseCodesList.Codes.ForResearchDevelopmentNonFoodProduct;
				secondaryFDALine1.US_BrandName = "PARTS 9817";

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				Factory.Save();

				var ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
				new PGACorrectionMessageBuilder(ensEntry, ACEEntrySummaryMessageSendingOption.New(), false).GenerateMessage();
				AssertEquals(1, ensEntry.Messages.Count);
				AssertMultilineASCIIEquals("PGA Correction",
	@"B         CA                                               <<MSGNO PLACEHOLDER>>
CA10RXJ500000014                                                                
CA4000001                                                                       
CA609801001000                                                                  
CA609801001002                                                                  
OI        LINE 1                                                                
PG01001FDADEVNED                         180.000                                
PG02PFDP 80K--YZ                                                                
PG07PARTS 9817                                                                  
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
Y         CA", ensEntry.Messages[0].EM_FormattedMessageText);

				invoiceLine0.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
				var fdaLine1 = invoiceLine0.ACE_FDALines.AddNew();
				fdaLine1.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
				fdaLine1.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_FEE;
				fdaLine1.US_ProductCode = "7654321";
				fdaLine1.US_ProdCountry = "FR";
				fdaLine1.US_Description = "FRUIT JUICE";
				fdaLine1.US_BrandName = "NATURAL";
				fdaLine1.US_ProducerType = ProducerFirmTypeList.Codes.M;
				fdaLine1.US_InvCurrValue = 10000m;
				ensEntry.Messages.RemoveAndDeleteAll();

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				Factory.Save();

				new PGACorrectionMessageBuilder(ensEntry, ACEEntrySummaryMessageSendingOption.New(), false).GenerateMessage();
				AssertEquals(1, ensEntry.Messages.Count);
				AssertMultilineASCIIEquals("PGA Correction",
	@"B         CA                                               <<MSGNO PLACEHOLDER>>
CA10RXJ500000014                                                                
CA4000001                                                                       
CA609801001000                                                                  
CA609801001002                                                                  
OI        LINE 0                                                                
PG01001FDAFOOFEE                                                                
PG02PFDP 7654321                                                                
PG0639 FR                                                                       
PG07NATURAL                                                                     
PG10                   FRUIT JUICE                                              
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG25                                                    000000010000            
PG01002FDADEVNED                         180.000                                
PG02PFDP 80K--YZ                                                                
PG07PARTS 9817                                                                  
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
Y         CA", ensEntry.Messages[0].EM_FormattedMessageText);
			}
		}

		[TestDate(2017, 05, 25)]
		public void TestSendOIBlockForSecondaryTariff()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.PGADataCorrection2ndPhase, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				var tariff0 = Factory.New<USCTariff>();
				tariff0.UE_Tariff = "85123000##";
				tariff0.UE_PGACodes = "EP3";
				tariff0.UE_DateFrom = ZDate.Today.AddMonths(-1);
				tariff0.UE_DateTo = ZDate.Today.AddMonths(1);

				var tariff1 = Factory.New<USCTariff>();
				tariff1.UE_Tariff = "98010080##";
				tariff1.UE_DateFrom = ZDate.Today.AddMonths(-1);
				tariff1.UE_DateTo = ZDate.Today.AddMonths(1);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EntryFilerCode = "XJ5";
				declaration.US_EnableENS = true;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = tariff0.UE_Tariff;
				invoiceLine.US_SupTariff = tariff1.UE_Tariff;
				invoiceLine.JI_Description = "TEST MISSING OI FOR 98/99 AND ITS SECONDARY TARIFF";
				invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;

				var vehicleLine = invoiceLine.VehicleLines.AddNew();
				vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
				vehicleLine.US_ModelYear = "2014";
				vehicleLine.US_VehicleModel = "PRIUS";
				vehicleLine.US_ImportCode = ImportCodesForm3520_1List.Codes.A;
				vehicleLine.US_CertOfConformity = "9EPAV01.0ABC";
				vehicleLine.US_VNEElectronicImage = true;

				var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
				vehicleDetails.US_BuildMonth = MonthList.Codes._12;
				vehicleDetails.US_BuildYear = "2013";
				vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
				vehicleDetails.US_IdentityNumber = "JTDBBADU5B0865DK0";
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				var ensEntry = declaration.FormalEntry;
				new PGACorrectionMessageBuilder(ensEntry, ACEEntrySummaryMessageSendingOption.New(), false).GenerateMessage();
				AssertEquals(1, ensEntry.Messages.Count);
				AssertMultilineASCIIEquals("PGA Correction, only 1 OI block generated",
	@"B         CA                                               <<MSGNO PLACEHOLDER>>
CA10RXJ5                                                                        
CA4000001                                                                       
CA6098010080                                                                    
CA6085123000                                                                    
OI        TEST MISSING OI FOR 98/99 AND ITS SECONDARY TARIFF                    
PG01001EPAVNE   Y                                                               
PG02P                                                                           
PG24EP2A                                                                        
PG07                                   PRIUS          122013AKGJTDBBADU5B0865DK0
PG10           V06     2014                                                     
PG14 EP49EPAV01.0ABC                                                            
PG22 942         CI EP2 Y05252017                                               
Y         CA", ensEntry.Messages[0].EM_FormattedMessageText);
			}
		}

		[TestDate(2017, 06, 20)]
		public void TestSendMessageWhenPSTIsDeclaredAndTSCAIsDisclaimed()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.PGADataCorrection2ndPhase, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EntryFilerCode = "XJ5";
				declaration.US_EnableENS = true;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "9801001000";
				invoiceLine.JI_Description = "TEST TSCA INCLUDED IN PGA CORRECTION";
				invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_TSCADisclaimReason = PGADisclaimReasonList.Codes.A;

				invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
				var pst = invoiceLine.PSTLines.AddNew();
				pst.US_ProductType = PSTProductTypeList.Codes.PS1;
				pst.US_BrandName = "TEST PST";
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				var ensEntry = declaration.FormalEntry;
				new PGACorrectionMessageBuilder(ensEntry, ACEEntrySummaryMessageSendingOption.New(), false).GenerateMessage();
				AssertEquals(1, ensEntry.Messages.Count);
				AssertMultilineASCIIEquals("TSCA is included in PGA Correction even disclaimed",
	@"B         CA                                               <<MSGNO PLACEHOLDER>>
CA10RXJ5                                                                        
CA4000001                                                                       
CA609801001000                                                                  
OI        TEST TSCA INCLUDED IN PGA CORRECTION                                  
PG01001EPATS1                                                                  A
PG01002EPAPS1                                                                   
PG02P                                                                           
PG07TEST PST                                                                    
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG55NP                                                                          
PG22 944         CI EP3 Y06202017                                               
PG261                                                                           
PG262                                                                           
PG29                                                                            
Y         CA", ensEntry.Messages[0].EM_FormattedMessageText);
			}
		}

		[TestDate(2017, 06, 20)]
		public void TestSendMessageWhenNMFSAMRIsDeclaredAndNMFS370IsDisclaimed()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.PGADataCorrection2ndPhase, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EntryFilerCode = "XJ5";
				declaration.US_EnableENS = true;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "9801001000";
				invoiceLine.JI_Description = "TEST NMFS 370 INCLUDED IN PGA CORRECTION";
				invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_NMFS370DisclaimReason = PGADisclaimReasonList.Codes.A;

				invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
				var nmfs = invoiceLine.NMFSLines.AddNew();
				nmfs.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
				nmfs.US_Commodity = FishStateList.Codes.FrozenToothfish;
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				var ensEntry = declaration.FormalEntry;
				new PGACorrectionMessageBuilder(ensEntry, ACEEntrySummaryMessageSendingOption.New(), false).GenerateMessage();
				AssertEquals(1, ensEntry.Messages.Count);
				AssertMultilineASCIIEquals("TSCA is included in PGA Correction even disclaimed",
	@"B         CA                                               <<MSGNO PLACEHOLDER>>
CA10RXJ5                                                                        
CA4000001                                                                       
CA609801001000                                                                  
OI        TEST NMFS 370 INCLUDED IN PGA CORRECTION                              
PG01001NMF370                                                                  A
PG01002NMFAMRFRZ Y                                                              
PG02P                                                                           
PG10           FRZ                                                              
PG142NM4                                                                        
PG141NM2                                                          KG            
Y         CA", ensEntry.Messages[0].EM_FormattedMessageText);
			}
		}

		[TestDate(2017, 07, 07)]
		public void TestDisclaimPSTAndVNEAndTSCAAndDeleteAllLines()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.PGADataCorrection2ndPhase, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EntryFilerCode = "XJ5";
				declaration.US_EnableENS = true;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "9801001000";
				invoiceLine.JI_Description = "TEST DISCLAIM PST AND VNE AND TSCA";

				invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
				var pstLine = invoiceLine.PSTLines.AddNew();
				pstLine.US_ProductType = PSTProductTypeList.Codes.PS1;
				invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
				var vneLine = invoiceLine.VehicleLines.AddNew();
				vneLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
				invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
				invoiceLine.US_TSCAIndicator = TSCAIndicatorList.Codes.TSCAPositive;
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				var ensEntry = declaration.FormalEntry;
				new PGACorrectionMessageBuilder(ensEntry, ACEEntrySummaryMessageSendingOption.New(), false).GenerateMessage();
				AssertEquals(1, ensEntry.Messages.Count);
				AssertMultilineASCIIEquals("Both PST and VNE and TSCA are included",
	@"B         CA                                               <<MSGNO PLACEHOLDER>>
CA10RXJ5                                                                        
CA4000001                                                                       
CA609801001000                                                                  
OI        TEST DISCLAIM PST AND VNE AND TSCA                                    
PG01001EPATS1                                                                   
PG02P                                                                           
PG22             CI     Y07072017                                               
PG21CI                                                                          
PG01002EPAVNE                                                                   
PG02P                                                                           
PG22 943         CI EP1 Y07072017                                               
PG01003EPAPS1                                                                   
PG02P                                                                           
PG07                                                                            
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG55NP                                                                          
PG22 944         CI EP3 Y07072017                                               
PG261                                                                           
PG262                                                                           
PG29                                                                            
Y         CA", ensEntry.Messages[0].EM_FormattedMessageText);

				pstLine.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
				vneLine.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
				invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Added;
				invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_PSTDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_PSTDisclaimProgram = PSTProductTypeList.Codes.PS2;

				ensEntry.Messages.RemoveAndDeleteAll();
				new PGACorrectionMessageBuilder(ensEntry, ACEEntrySummaryMessageSendingOption.New(), false).GenerateMessage();
				AssertEquals(1, ensEntry.Messages.Count);
				AssertMultilineASCIIEquals("Both PST and VNE and TSCA are included",
	@"B         CA                                               <<MSGNO PLACEHOLDER>>
CA10RXJ5                                                                        
CA4000001                                                                       
CA609801001000                                                                  
OI        TEST DISCLAIM PST AND VNE AND TSCA                                    
PG01001EPATS1                                                                   
PG02P                                                                           
PG22             CI     Y07072017                                               
PG21CI                                                                          
PG01002EPAVNE                                                                   
PG02P                                                                           
PG22 943         CI EP1 Y07072017                                               
PG01003EPAPS2                                                                  A
Y         CA", ensEntry.Messages[0].EM_FormattedMessageText);

				pstLine.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
				vneLine.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
				invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
				invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_TSCADisclaimReason = PGADisclaimReasonList.Codes.A;

				ensEntry.Messages.RemoveAndDeleteAll();
				new PGACorrectionMessageBuilder(ensEntry, ACEEntrySummaryMessageSendingOption.New(), false).GenerateMessage();
				AssertEquals(1, ensEntry.Messages.Count);
				AssertMultilineASCIIEquals("Both PST and VNE and TSCA are included",
	@"B         CA                                               <<MSGNO PLACEHOLDER>>
CA10RXJ5                                                                        
CA4000001                                                                       
CA609801001000                                                                  
OI        TEST DISCLAIM PST AND VNE AND TSCA                                    
PG01001EPATS1                                                                  A
PG01002EPAVNE                                                                   
PG02P                                                                           
PG22 943         CI EP1 Y07072017                                               
PG01003EPAPS2                                                                  A
Y         CA", ensEntry.Messages[0].EM_FormattedMessageText);

				pstLine.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
				vneLine.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
				invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Deleted;
				invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_VNEDisclaimReason = PGADisclaimReasonList.Codes.A;

				ensEntry.Messages.RemoveAndDeleteAll();
				new PGACorrectionMessageBuilder(ensEntry, ACEEntrySummaryMessageSendingOption.New(), false).GenerateMessage();
				AssertEquals(1, ensEntry.Messages.Count);
				AssertMultilineASCIIEquals("Both PST and VNE and TSCA are included",
	@"B         CA                                               <<MSGNO PLACEHOLDER>>
CA10RXJ5                                                                        
CA4000001                                                                       
CA609801001000                                                                  
OI        TEST DISCLAIM PST AND VNE AND TSCA                                    
PG01001EPATS1                                                                  A
PG01002EPAVNE                                                                  A
PG01003EPAPS2                                                                  A
Y         CA", ensEntry.Messages[0].EM_FormattedMessageText);
			}
		}
	}
}
