using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForFDATest : PGABlocksCreatorTest
	{
		public void TestNoScientificDetail_CloneFromProduct()
		{
			SetUpData();

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";
			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_ACEFDAIndicator = OGAIndicatorList.Codes.Declared;

			var fdaOnProduct = pivot.ACEFDAs.AddNew();
			fdaOnProduct.US_BrandName = "BRD NAME";
			fdaOnProduct.US_ProgramCode = FDAProgramCodeList.Codes.COS;

			var scientificData = Factory.NewWithValidTestData<ScientificData>();
			scientificData.B7_Type = CusAddInfoTypeAttribute.Codes.USSCI;
			scientificData.B7_ParentID = fdaOnProduct.PK;
			scientificData.US_PGAScientificGenusName = "Test GenusName";
			scientificData.US_PGAScientificSpeciesName = "Test SpeciesName";
			scientificData.US_PGACountryCode = "HK";

			declaration.JE_OH_Importer = importer.PK;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_PartNo = "Test";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			AssertNotContains("PG05", message.EM_FormattedMessageText);
			AssertNotContains("TEST GENUSNAME", message.EM_FormattedMessageText);
			AssertNotContains("TEST SPECIESNAME", message.EM_FormattedMessageText);
			AssertNotContains("PG0630 HK", message.EM_FormattedMessageText);
		}

		public void TestNoScientificDetail_CloneJob()
		{
			SetUpData();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "COSMETICS PRODUCT";

			var fda = invoiceLine.ACE_FDALines.AddNew();
			var scientificData = Factory.NewWithValidTestData<ScientificData>();
			scientificData.B7_Type = CusAddInfoTypeAttribute.Codes.USSCI;
			scientificData.B7_ParentID = fda.PK;
			scientificData.US_PGAScientificGenusName = "Test GenusName";
			scientificData.US_PGAScientificSpeciesName = "Test SpeciesName";
			scientificData.US_PGACountryCode = "HK";

			var clonedDeclaration = (JobDeclaration)new USJobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.DeepTemplateCopy).Clone();
			clonedDeclaration.US_EntryFilerCode = "XJ5";
			clonedDeclaration.US_EnableCRL = true;
			clonedDeclaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			clonedDeclaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			clonedDeclaration.US_EnableENS = false;
			clonedDeclaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var activeEntryHeader = clonedDeclaration.ActiveEntryHeaders.AddNew();
			activeEntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var action = GetAction(clonedDeclaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			AssertNotContains("PG05", message.EM_FormattedMessageText);
			AssertNotContains("TEST GENUSNAME", message.EM_FormattedMessageText);
			AssertNotContains("TEST SPECIESNAME", message.EM_FormattedMessageText);
			AssertNotContains("PG0630 HK", message.EM_FormattedMessageText);
		}

		public void TestNoScientificDetail_CloneFromInvoice()
		{
			SetUpData();

			var invoice = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.FillWithValidTestData();
			line.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			line.JI_Description = "COSMETICS PRODUCT";

			var fda = line.ACE_FDALines.AddNew();
			var scientificData = Factory.NewWithValidTestData<ScientificData>();
			scientificData.B7_Type = CusAddInfoTypeAttribute.Codes.USSCI;
			scientificData.B7_ParentID = fda.PK;
			scientificData.US_PGAScientificGenusName = "Test GenusName";
			scientificData.US_PGAScientificSpeciesName = "Test SpeciesName";
			scientificData.US_PGACountryCode = "HK";

			invoice.JZ_JE = declaration.PK;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertNotContains("PG05", message.EM_FormattedMessageText);
			AssertNotContains("TEST GENUSNAME", message.EM_FormattedMessageText);
			AssertNotContains("TEST SPECIESNAME", message.EM_FormattedMessageText);
			AssertNotContains("PG0630 HK", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 07, 29)]
		public void TestCosmeticsProduct()
		{
			SetUpData();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "COSMETICS PRODUCT";
			declaration.US_FDAADTA = new ZDateTime(2015, 07, 14, 12, 04, 05);
			declaration.US_FDAContactName = "Joo Youm";
			declaration.US_FDAContactPhoneNo = "5555555555";
			declaration.US_FDAContactEmail = "joo.youm@test.com";
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.COS;
			fda.US_ProductCode = "203AB05";
			fda.US_ProdCountry = "CA";
			fda.US_Description = "TEST";
			fda.US_Qty1 = 1000M;
			fda.US_UQ1 = "LB";
			fda.US_Qty2 = 100m;
			fda.US_UQ2 = "CT";
			fda.US_TotalValue = 2000m;
			fda.US_UnitValue = 100m;

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_RL_NKClosestPort = "DEBRE";
			DeclarationTestHelper.AddPGAContact(manufacturer, "Test", "Contact For FDA For Testing", null, "TestContact@test.com", null);
			var address = manufacturer.Addresses.AddNew();
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "456378259");
			address.OA_PostCode = "1234567890";//Test for PG20
			fda.US_ManufacturerAddress = address.PK;
			fda.US_OA_ShipperAddress = address.PK;
			fda.US_Remarks = "REMARKS SHOULD BE UNDER PG02";

			var fdaImporter = Factory.New<OrgHeader>();
			fdaImporter.OH_RL_NKClosestPort = "DEBRE";
			DeclarationTestHelper.AddPGAContact(fdaImporter, "John", "Smith", null, "JohnTestContact@test.com", null);
			var impAddress = fdaImporter.Addresses.AddNew();
			impAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "4568214563");
			fda.US_FDAImporterAddress = impAddress.PK;
			fda.US_Remarks = "REMARKS SHOULD BE UNDER PG02";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(@"OI        COSMETICS PRODUCT                                                     
PG01001FDACOS                                                                   
PG02PFDP 203AB05                                                                
PG24GEN     REMARKS SHOULD BE UNDER PG02                                        
PG0639 CA                                                                       
PG10                   TEST                                                     
PG19MF 16 456378259                                                             
PG20                                                             US123456789    
PG19DEQ16 456378259                                                             
PG20                                                             US123456789    
PG19FD147 4568214563                                                            
PG20                                                             US             
PG21FD1JOHN SMITH                            JOHNTESTCONTACT@TEST.COM           
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21PK JOO YOUM               5555555555     JOO.YOUM@TEST.COM                  
PG25                                                    000000002000000000010000
PG261000000010000CT                                                             
PG262000000100000LB                                                             
PG30A071420151204                                                               ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 07, 29)]
		public void TestTobaccoProduct()
		{
			SetUpData();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "TOBACCO PRODUCT";
			declaration.US_FDAADTA = new ZDateTime(2015, 07, 14, 12, 04, 05);
			declaration.US_FDAContactName = "Joo Youm";
			declaration.US_FDAContactPhoneNo = "5555555555";
			declaration.US_FDAContactEmail = "joo.youm@test.com";

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.TOB_CSU;
			fda.US_ProductCode = "12AAB01";
			fda.US_ProdCountry = "CA";
			fda.US_Description = "TEST";
			fda.US_IntendedUseCode = "150.000";
			fda.US_Qty1 = 2323m;
			fda.US_UQ1 = FDABaseUQList.Codes.PCS;
			fda.US_Qty2 = 100m;
			fda.US_UQ2 = "CT";
			fda.US_BrandName = "BRAND NAME";
			fda.US_TotalValue = 2000m;

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_RL_NKClosestPort = "DEBRE";
			DeclarationTestHelper.AddPGAContact(manufacturer, "Test", "Contact For FDA For Testing", null, "TestContact@test.com", null);
			var address = manufacturer.Addresses.AddNew();
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "456378259");
			fda.US_ManufacturerAddress = address.PK;
			fda.US_OA_ShipperAddress = address.PK;
			fda.US_Remarks = "REMARKS SHOULD BE UNDER PG02";

			var fdaImporter = Factory.New<OrgHeader>();
			fdaImporter.OH_RL_NKClosestPort = "DEBRE";
			DeclarationTestHelper.AddPGAContact(fdaImporter, "John", "Smith", null, "JohnTestContact@test.com", null);
			var impAddress = fdaImporter.Addresses.AddNew();
			impAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "4568214563");
			fda.US_FDAImporterAddress = impAddress.PK;
			fda.US_Remarks = "REMARKS SHOULD BE UNDER PG02";

			var fdaSubmitter = Factory.New<OrgHeader>();
			fdaSubmitter.OH_Code = "ZXCVCXZV";
			fdaSubmitter.MainAddress.OA_Address1 = "HOLLAND VILLAGE";
			fdaSubmitter.MainAddress.OA_Address2 = "HOLLAND VILLAGE2";
			fdaSubmitter.MainAddress.OA_Phone = "+ 1 (234) 5678901";
			fdaSubmitter.MainAddress.OA_City = "CHICAGO";
			fdaSubmitter.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			fdaSubmitter.MainAddress.OA_State = "IL";
			fdaSubmitter.MainAddress.OA_PostCode = "987654";
			fdaSubmitter.OH_FullName = "CARGOWISE";

			DeclarationTestHelper.AddPGAContact(fdaSubmitter, "BRENDON", "PAINE", "+ 1 (234) 5678317", "IAN.TEST.VERY.LONG.EMALADDRESS@ABCDEFG.COM", null);
			invoiceLine.Declaration.JE_OH_FDASubmitter = fdaSubmitter.PK;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(@"OI        TOBACCO PRODUCT                                                       
PG01001FDATOBCSU                         150.000                                
PG02PFDP 12AAB01                                                                
PG24GEN     REMARKS SHOULD BE UNDER PG02                                        
PG0639 CA                                                                       
PG07BRAND NAME                                                                  
PG10                   TEST                                                     
PG19MF 16 456378259                                                             
PG20                                                             US             
PG19DEQ16 456378259                                                             
PG20                                                             US             
PG19FD147 4568214563                                                            
PG20                                                             US             
PG21FD1JOHN SMITH                            JOHNTESTCONTACT@TEST.COM           
PG19TB                   CARGOWISE                       HOLLAND VILLAGE        
PG20HOLLAND VILLAGE2                     CHICAGO              IL US987654       
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21PK JOO YOUM               5555555555     JOO.YOUM@TEST.COM                  
PG25                                                    000000002000            
PG261000000010000CT                                                             
PG262000000232300PCS                                                            
PG30A071420151204                                                               ", message.EM_FormattedMessageText);
		}

		//test BLOOD DERIVATIVES
		[TestDate(2015, 04, 20)]
		public void TestBiologicMessageSetLayout()
		{
			SetUpData();

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "BLOOD DERIVATIVES";
			declaration.US_FDAContactName = "Joo Youm";
			declaration.US_FDAContactPhoneNo = "5555555555";
			declaration.US_FDAContactEmail = "joo.youm@test.com";

			//FDA line 1
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_BLO;
			fda.US_ProductCode = "57UH-12";
			fda.US_ProdCountry = "ES";
			fda.US_Description = "FLEBOGAMMA 5% 400ML";
			fda.US_TotalValue = 43466769m;

			var manufacturer = Factory.New<OrgHeader>();
			DeclarationTestHelper.AddPGAContact(manufacturer, "Test", "Contact For FDA For Testing", null, "TestContact@test.com", null);
			var address = manufacturer.Addresses.AddNew();
			address.OA_RL_NKRelatedPortCode = "US2CW";
			address.OA_State = "CA";
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "456378259");
			fda.US_ManufacturerAddress = address.PK;
			fda.US_OA_ShipperAddress = address.PK;

			var fdaImporter = Factory.New<OrgHeader>();
			DeclarationTestHelper.AddPGAContact(fdaImporter, "John", "Smith", null, "JohnTestContact@test.com", null);
			var impAddress = fdaImporter.Addresses.AddNew();
			impAddress.OA_RL_NKRelatedPortCode = "US2CW";
			impAddress.OA_State = "TX";
			impAddress.OA_City = "CITY";
			impAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "4568214563");
			fda.US_FDAImporterAddress = impAddress.PK;
			fda.US_Remarks = "1REMARKS SHOULD BE UNDER PG02";

			var aoc1 = fda.AffirmationCodes.AddNew();
			aoc1.CY_Code = ACE_AffirmationOfComplianceList.Codes.HDE;
			aoc1.CY_Data = "3002807257";

			var aoc2 = fda.AffirmationCodes.AddNew();
			aoc2.CY_Code = ACE_AffirmationOfComplianceList.Codes.STN;
			aoc2.CY_Data = "125077";

			var aoc3 = fda.AffirmationCodes.AddNew();
			aoc3.CY_Code = ACE_AffirmationOfComplianceList.Codes.NDA;
			aoc3.CY_Data = "61953-0005-3888888884444444444";

			var lot = fda.Lots.AddNew();
			lot.US_TemperatureQualifier = TemperatureQualifierList.Codes.Refrigerated;

			fda.US_Qty1 = 2323m;
			fda.US_UQ1 = FDABaseUQList.Codes.KG;

			//FDA line 2
			var fda2 = invoiceLine.ACE_FDALines.AddNew();
			fda2.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			fda2.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_BLO;
			fda2.US_ProductCode = "57UH-12";
			fda2.US_ProdCountry = "ES";
			fda2.US_Description = "ALBUMIN (HUMAN) 25% 50ML";
			fda2.US_ManufacturerAddress = address.PK;
			fda2.US_OA_ShipperAddress = address.PK;
			fda2.US_TotalValue = 496728m;
			fda2.US_Remarks = "2REMARKS SHOULD BE UNDER PG02";

			aoc1 = fda2.AffirmationCodes.AddNew();
			aoc1.CY_Code = ACE_AffirmationOfComplianceList.Codes.HDE;
			aoc1.CY_Data = "3002807257";

			aoc2 = fda2.AffirmationCodes.AddNew();
			aoc2.CY_Code = ACE_AffirmationOfComplianceList.Codes.STN;
			aoc2.CY_Data = "103352";

			aoc3 = fda2.AffirmationCodes.AddNew();
			aoc3.CY_Code = ACE_AffirmationOfComplianceList.Codes.NDA;
			aoc3.CY_Data = "61953-0001-1";

			var aoc4 = fda2.AffirmationCodes.AddNew();
			aoc4.CY_Code = ACE_AffirmationOfComplianceList.Codes.BLN;
			aoc4.CY_Data = "1181";

			lot = fda2.Lots.AddNew();
			lot.US_TemperatureQualifier = TemperatureQualifierList.Codes.Refrigerated;

			fda2.US_Qty1 = 2214m;
			fda2.US_UQ1 = FDABaseUQList.Codes.KG;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        BLOOD DERIVATIVES                                                     
PG01001FDABIOBLO                                                                
PG02PFDP 57UH-12                                                                
PG24GEN     1REMARKS SHOULD BE UNDER PG02                                       
PG0639 ES                                                                       
PG10                   FLEBOGAMMA 5% 400ML                                      
PG19MF 16 456378259                                                             
PG20                                                          CA US             
PG19DEQ16 456378259                                                             
PG20                                                          CA US             
PG19FD147 4568214563                                                            
PG20                                     CITY                 TX US             
PG21FD1JOHN SMITH                            JOHNTESTCONTACT@TEST.COM           
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21PK JOO YOUM               5555555555     JOO.YOUM@TEST.COM                  
PG23HDE  3002807257                                                             
PG23STN  125077                                                                 
PG23NDA  61953-0005-3888888884444444444                                         
PG25R  000000                                           000043466769            
PG261000000232300KG                                                             
PG01002FDABIOBLO                                                                
PG02PFDP 57UH-12                                                                
PG24GEN     2REMARKS SHOULD BE UNDER PG02                                       
PG0639 ES                                                                       
PG10                   ALBUMIN (HUMAN) 25% 50ML                                 
PG19MF 16 456378259                                                             
PG20                                                          CA US             
PG19DEQ16 456378259                                                             
PG20                                                          CA US             
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21PK JOO YOUM               5555555555     JOO.YOUM@TEST.COM                  
PG23HDE  3002807257                                                             
PG23STN  103352                                                                 
PG23NDA  61953-0001-1                                                           
PG23BLN  1181                                                                   
PG25R  000000                                           000000496728            
PG261000000221400KG                                                             ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 07, 10)]
		public void TestRadiationEmitting()
		{
			SetUpData();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "RADIATION EMITTING";
			declaration.US_FDAContactName = "Joo Youm";
			declaration.US_FDAContactPhoneNo = "5555555555";
			declaration.US_FDAContactEmail = "joo.youm@test.com";

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.RAD;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.RAD_REP;
			fda.US_ProductCode = "57UH-12";
			fda.US_ProdCountry = "CA";
			fda.US_Description = "FLEBOGAMMA 5% 400ML";
			fda.US_IntendedUseCode = "080.000";
			fda.US_IntendedUseDescr = "SAMPLE DEVICES";
			fda.US_Qty1 = 2323m;
			fda.US_UQ1 = FDABaseUQList.Codes.KG;
			fda.US_BrandName = "BRAND NAME";
			fda.US_TotalValue = 43466769m;
			fda.US_Remarks = "REMARKS SHOULD BE UNDER PG02";

			var manufacturer = Factory.New<OrgHeader>();
			DeclarationTestHelper.AddPGAContact(manufacturer, "Test", "Contact For FDA For Testing", null, "TestContact@test.com", null);
			var address = manufacturer.Addresses.AddNew();
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "456378259");
			fda.US_ManufacturerAddress = address.PK;
			fda.US_OA_ShipperAddress = address.PK;

			var fdaImporter = Factory.New<OrgHeader>();
			DeclarationTestHelper.AddPGAContact(fdaImporter, "John", "Smith", null, "JohnTestContact@test.com", null);
			var impAddress = fdaImporter.Addresses.AddNew();
			impAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "4568214563");
			fda.US_FDAImporterAddress = impAddress.PK;

			var aoc1 = fda.AffirmationCodes.AddNew();
			aoc1.CY_Code = ACE_AffirmationOfComplianceList.Codes.RA1;
			aoc1.CY_Data = "1234567";

			var aoc2 = fda.AffirmationCodes.AddNew();
			aoc2.CY_Code = ACE_AffirmationOfComplianceList.Codes.RA2;
			aoc2.CY_Data = "K123456";

			var aoc3 = fda.AffirmationCodes.AddNew();
			aoc3.CY_Code = ACE_AffirmationOfComplianceList.Codes.RA3;
			aoc3.CY_Data = "P123456";

			var lot = fda.Lots.AddNew();
			lot.US_TemperatureQualifier = TemperatureQualifierList.Codes.Refrigerated;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        RADIATION EMITTING                                                    
PG01001FDARADREP                         080.000         SAMPLE DEVICES         
PG02PFDP 57UH-12                                                                
PG24GEN     REMARKS SHOULD BE UNDER PG02                                        
PG0639 CA                                                                       
PG07BRAND NAME                                                                  
PG10                   FLEBOGAMMA 5% 400ML                                      
PG19MF 16 456378259                                                             
PG20                                                             US             
PG19DEQ16 456378259                                                             
PG20                                                             US             
PG19FD147 4568214563                                                            
PG20                                                             US             
PG21FD1JOHN SMITH                            JOHNTESTCONTACT@TEST.COM           
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21PK JOO YOUM               5555555555     JOO.YOUM@TEST.COM                  
PG23RA1  1234567                                                                
PG23RA2  K123456                                                                
PG23RA3  P123456                                                                
PG25R  000000                                           000043466769            
PG261000000232300KG                                                             ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 04, 20)]
		public void TestMedicalDevices()
		{
			SetUpData();
			declaration.US_FDAContactName = "Joo Youm";
			declaration.US_FDAContactPhoneNo = "5555555555";
			declaration.US_FDAContactEmail = "joo.youm@test.com";

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "UNIPOLAR EPICARDIAL LEAD";
			invoiceLine.Declaration.US_FDAADTA = new ZDateTime(2015, 04, 19, 14, 00, 00);
			invoiceLine.Declaration.US_SchDEntry = "2704";

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_RED;
			fda.US_ProductCode = "74D--TF";
			fda.US_ProdCountry = "US";
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._081000;
			fda.US_IntendedUseDescr = "MEDICAL DEVICE";
			fda.US_Description = "MYOPORE SUTURELESS MYOCARDIAL PACING LEAD";
			fda.US_BrandName = "MYOCARDIAL PACING LEAD";
			fda.US_InvCurrValue = 10000m;
			fda.US_Remarks = "2REMARKS SHOULD BE UNDER PG02";

			var manufacturer = Factory.New<OrgHeader>();
			var address = manufacturer.Addresses.AddNew();
			address.OA_RL_NKRelatedPortCode = "US2CW";
			address.OA_State = "TX";
			address.OA_City = "CITY";
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "4521000456");
			fda.US_ManufacturerAddress = address.PK;
			fda.US_OA_ShipperAddress = address.PK;

			var orgHeader = Factory.New<OrgHeader>();
			var address2 = orgHeader.Addresses.AddNew();
			address2.OA_RL_NKRelatedPortCode = "US2CW";
			address2.OA_State = "WA";
			address2.OA_City = "Olympia";
			address2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "456789652");
			DeclarationTestHelper.AddPGAContact(orgHeader, "JOHN TESTCONTACT FOR FDA LONG NAME", "SMITH", null, "test@test.com", null);
			fda.US_DeliverToPartyAddress = address2.PK;
			fda.US_FDAImporterAddress = address2.PK;

			var aoc1 = fda.AffirmationCodes.AddNew();
			aoc1.CY_Code = ACE_AffirmationOfComplianceList.Codes.DEV;
			aoc1.CY_Data = "1234567890";

			var aoc2 = fda.AffirmationCodes.AddNew();
			aoc2.CY_Code = ACE_AffirmationOfComplianceList.Codes.DFE;
			aoc2.CY_Data = "2345678901";

			var aoc3 = fda.AffirmationCodes.AddNew();
			aoc3.CY_Code = ACE_AffirmationOfComplianceList.Codes.LST;
			aoc3.CY_Data = "E123456";

			var aoc4 = fda.AffirmationCodes.AddNew();
			aoc4.CY_Code = ACE_AffirmationOfComplianceList.Codes.LWC;
			aoc4.CY_Data = "123";

			var lot = fda.Lots.AddNew();
			lot.US_LotNumber = "142536";
			lot.US_StartDate = new ZDateTime(2014, 11, 01);
			lot.US_EndDate = new ZDateTime(2014, 12, 31);

			fda.US_Qty1 = 8m;
			fda.US_UQ1 = "PCS";

			fda.US_Qty2 = 6m;
			fda.US_UQ2 = "BX";

			fda.US_Qty3 = 2m;
			fda.US_UQ3 = "CT";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        UNIPOLAR EPICARDIAL LEAD                                              
PG01001FDADEVRED                         081.000         MEDICAL DEVICE         
PG02PFDP 74D--TF                                                                
PG24GEN     2REMARKS SHOULD BE UNDER PG02                                       
PG0639 US                                                                       
PG07MYOCARDIAL PACING LEAD                                                      
PG10                   MYOPORE SUTURELESS MYOCARDIAL PACING LEAD                
PG19MF 47 4521000456                                                            
PG20                                     CITY                 TX US             
PG19DEQ47 4521000456                                                            
PG20                                     CITY                 TX US             
PG19FD116 456789652                                                             
PG20                                     OLYMPIA              WA US             
PG21FD1JOHN TESTCONTACT FOR FD               TEST@TEST.COM                      
PG60INAA LONG NAME SMITH                                                        
PG19DP 16 456789652                                                             
PG20                                     OLYMPIA              WA US             
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21PK JOO YOUM               5555555555     JOO.YOUM@TEST.COM                  
PG23DEV  1234567890                                                             
PG23DFE  2345678901                                                             
PG23LST  E123456                                                                
PG23LWC  123                                                                    
PG25   000000 1142536                   1101201412312014                        
PG261000000000200CT                                                             
PG262000000000600BX                                                             
PG263000000000800PCS                                                            
PG30A0419201514002   2704                                                       ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 04, 20)]
		public void TestPG30ForFDAArrivalLocation()
		{
			SetUpData();
			declaration.US_FDAContactName = "Joo Youm";
			declaration.US_FDAContactPhoneNo = "5555555555";
			declaration.US_FDAContactEmail = "joo.youm@test.com";

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "UNIPOLAR EPICARDIAL LEAD";
			invoiceLine.Declaration.US_FDAADTA = new ZDateTime(2015, 04, 19, 14, 00, 00);
			invoiceLine.Declaration.US_SchDEntry = "2704";

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_RED;
			fda.US_ProductCode = "74D--TF";
			fda.US_ProdCountry = "US";
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._081000;
			fda.US_IntendedUseDescr = "MEDICAL DEVICE";
			fda.US_Description = "MYOPORE SUTURELESS MYOCARDIAL PACING LEAD";
			fda.US_BrandName = "MYOCARDIAL PACING LEAD";
			fda.US_Remarks = "REMARKS SHOULD BE UNDER PG02";

			var manufacturer = Factory.New<OrgHeader>();
			var address = manufacturer.Addresses.AddNew();
			address.OA_RL_NKRelatedPortCode = "US2CW";
			address.OA_State = "TX";
			address.OA_City = "CITY";
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "004521000456");
			fda.US_ManufacturerAddress = address.PK;
			fda.US_OA_ShipperAddress = address.PK;

			var orgHeader = Factory.New<OrgHeader>();
			var address2 = orgHeader.Addresses.AddNew();
			address2.OA_RL_NKRelatedPortCode = "US2CW";
			address2.OA_State = "WA";
			address2.OA_City = "Olympia";
			address2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "456789652");
			DeclarationTestHelper.AddPGAContact(orgHeader, "JOHN TESTCONTACT FOR FDA LONG NAME", "SMITH", null, "test@test.com", null);
			fda.US_DeliverToPartyAddress = address2.PK;
			fda.US_FDAImporterAddress = address2.PK;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains("PG30A0419201514002   2704", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 07, 08)]
		public void TestVeterinarySetLayout()
		{
			SetUpData();

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "DRUGS FOR ANIMALS";
			invoiceLine.Declaration.US_SchDEntry = "3902";
			invoiceLine.Declaration.US_FDAADTA = new ZDateTime(2015, 07, 08, 15, 36, 00);
			declaration.US_FDAContactName = "Joo Youm";
			declaration.US_FDAContactPhoneNo = "5555555555";
			declaration.US_FDAContactEmail = "joo.youm@test.com";
			//FDA line 1
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.VME;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.VME_ADR;
			fda.US_ProductCode = "68YAA99";
			fda.US_ProdCountry = "FR";
			fda.US_Description = "TEST VET";
			fda.US_BrandName = "ANTIBDRUGS";
			fda.US_TotalValue = 2000m;
			fda.US_Remarks = "REMARKS SHOULD BE UNDER PG02";

			var manufacturer = Factory.New<OrgHeader>();
			DeclarationTestHelper.AddPGAContact(manufacturer, "Test", "Contact For FDA For Testing", null, "TestContact@test.com", null);
			var address = manufacturer.Addresses.AddNew();
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "456378259");
			address.OA_City = "ALBERTA";
			address.OA_RL_NKRelatedPortCode = "US2CW";
			address.OA_State = "TX";
			fda.US_ManufacturerAddress = address.PK;
			fda.US_OA_ShipperAddress = address.PK;

			var fdaImporter = Factory.New<OrgHeader>();
			DeclarationTestHelper.AddPGAContact(fdaImporter, "John", "Smith", null, "JohnTestContact@test.com", null);
			var impAddress = fdaImporter.Addresses.AddNew();
			impAddress.OA_City = "LOS ALAM";
			impAddress.OA_RL_NKRelatedPortCode = "US2CW";
			impAddress.OA_State = "CA";
			impAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "4568214563");
			fda.US_FDAImporterAddress = impAddress.PK;

			var aoc1 = fda.AffirmationCodes.OfType<ACEAffirmationCode>().FirstOrDefault(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.REG);
			aoc1.CY_Data = "0123458521";

			fda.US_Qty1 = 2m;
			fda.US_UQ1 = FDABaseUQList.Codes.G;

			fda.US_Qty2 = 10m;
			fda.US_UQ2 = FDABaseUQList.Codes.TAB;

			var activeIngr = fda.ProductConstituentElements.AddNew();
			activeIngr.US_PGANameOfTheConstituentElement = "Ibuprofen";
			activeIngr.US_PGAQuantityOfConstituentElement = 200m;
			activeIngr.US_PGAUnitOfMeasure = "MG";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        DRUGS FOR ANIMALS                                                     
PG01001FDAVMEADR                                                                
PG02PFDP 68YAA99                                                                
PG24GEN     REMARKS SHOULD BE UNDER PG02                                        
PG0639 FR                                                                       
PG07ANTIBDRUGS                                                                  
PG10                   TEST VET                                                 
PG04YIBUPROFEN                                          000000020000MG          
PG19MF 16 456378259                                                             
PG20                                     ALBERTA              TX US             
PG19DEQ16 456378259                                                             
PG20                                     ALBERTA              TX US             
PG19FD147 4568214563                                                            
PG20                                     LOS ALAM             CA US             
PG21FD1JOHN SMITH                            JOHNTESTCONTACT@TEST.COM           
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21PK JOO YOUM               5555555555     JOO.YOUM@TEST.COM                  
PG23REG  0123458521                                                             
PG25                                                    000000002000            
PG261000000001000TAB                                                            
PG262000000000200G                                                              
PG30A0708201515362   3902                                                       ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 07, 23)]
		public void TestFoodSubmission()
		{
			SetUpData();

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "NATURE’S FINEST REAL FRUIT JUICE, 12 OUNCE BOTTLES";
			invoiceLine.Declaration.US_FDAADTA = new ZDateTime(2015, 07, 24, 11, 05, 00);
			invoiceLine.Declaration.US_SchDEntry = "2704";
			invoiceLine.Declaration.US_FDAContactName = "PGA CONTACT FOR TEST";
			invoiceLine.Declaration.US_FDAContactEmail = "test@test.com";
			invoiceLine.Declaration.US_FDAContactPhoneNo = "091245022";

			var fdaSubmitter = Factory.New<OrgHeader>();
			fdaSubmitter.OH_Code = "ZXCVCXZV";
			fdaSubmitter.MainAddress.OA_Address1 = "HOLLAND VILLAGE";
			fdaSubmitter.MainAddress.OA_Address2 = "HOLLAND VILLAGE2";
			fdaSubmitter.MainAddress.OA_Phone = "+ 1 (234) 5678901";
			fdaSubmitter.MainAddress.OA_City = "CHICAGO";
			fdaSubmitter.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			fdaSubmitter.MainAddress.OA_State = "IL";
			fdaSubmitter.MainAddress.OA_PostCode = "987654";
			fdaSubmitter.OH_FullName = "CARGOWISE";

			var fdaSubmitterWrapped = OrgHeaderWrapper.New(fdaSubmitter);
			DeclarationTestHelper.AddPGAContact(fdaSubmitter, "BRENDON", "PAINE", "+ 1 (234) 5678317", "b.paine@submitter.com", null);
			invoiceLine.Declaration.JE_OH_FDASubmitter = fdaSubmitter.PK;

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_FEE;
			fda.US_ProductCode = "7654321";
			fda.US_ProdCountry = "FR";
			fda.US_Description = "FRUIT JUICE";
			fda.US_BrandName = "NATURAL";
			fda.US_ProducerType = ProducerFirmTypeList.Codes.M;
			fda.US_InvCurrValue = 10000m;
			fda.US_Remarks = "REMARKS SHOULD BE UNDER PG02";

			var manufacturer = Factory.New<OrgHeader>();
			var address = manufacturer.Addresses.AddNew();
			address.OA_RL_NKRelatedPortCode = "US2WC";
			address.OA_State = "TX";
			address.OA_City = "CITY";
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "4521000456");
			fda.US_ManufacturerAddress = address.PK;
			fda.US_OA_ShipperAddress = address.PK;

			var orgHeader = Factory.New<OrgHeader>();
			var address2 = orgHeader.Addresses.AddNew();
			address2.OA_RL_NKRelatedPortCode = "US2WC";
			address2.OA_State = "WA";
			address2.OA_City = "Olympia";
			address2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "456789652");
			DeclarationTestHelper.AddPGAContact(orgHeader, "JOHN TESTCONTACT FOR FDA LONG NAME", "SMITH", null, "test@test.com", null);
			fda.US_DeliverToPartyAddress = address2.PK;
			fda.US_FDAImporterAddress = address2.PK;

			fda.US_CanDim1 = 14.04m;
			fda.US_CanDim2 = 8m;
			fda.US_CanDim3 = 6.08m;
			fda.US_PackageTrackCode = "DHLX";
			fda.US_PackageTrackNumber = "1234567891";

			var aoc1 = fda.AffirmationCodes.AddNew();
			aoc1.CY_Code = ACE_AffirmationOfComplianceList.Codes.FME;
			aoc1.CY_Data = "Y";

			var aoc2 = fda.AffirmationCodes.AddNew();
			aoc2.CY_Code = ACE_AffirmationOfComplianceList.Codes.IFE;
			aoc2.CY_Data = "N";

			var aoc3 = fda.AffirmationCodes.AddNew();
			aoc3.CY_Code = ACE_AffirmationOfComplianceList.Codes.PKC;
			aoc3.CY_Data = "A00125";

			var aoc4 = fda.AffirmationCodes.AddNew();
			aoc4.CY_Code = ACE_AffirmationOfComplianceList.Codes.VOL;
			aoc4.CY_Data = "12 OUNCE";

			var lot = fda.Lots.AddNew();
			lot.US_LotNumber = "142536";
			lot.US_StartDate = new ZDateTime(2014, 11, 01);
			lot.US_EndDate = new ZDateTime(2014, 12, 31);

			fda.US_Qty1 = 12m;
			fda.US_UQ1 = "FOZ";

			fda.US_Qty2 = 24m;
			fda.US_UQ2 = "BO";

			fda.US_Qty3 = 1000m;
			fda.US_UQ3 = "CS";
			fda.US_Remarks = "TESTING FOOD WITH REMARKS";

			var license1 = fda.Licenses.AddNew();
			license1.US_CountryCode = "MX";
			license1.US_Number = "KT0554";
			license1.US_StateCode = "MX";
			license1.US_StateDescription = "UNKNOWN STATE";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        NATURE*S FINEST REAL FRUIT JUICE, 12 OUNCE BOTTLES                    
PG01001FDAFOOFEE                                                                
PG02PFDP 7654321                                                                
PG24GEN     TESTING FOOD WITH REMARKS                                           
PG0639 FR                                                                       
PG07NATURAL                                                                     
PG10                   FRUIT JUICE                                              
PG13POV                                MX MX UNKNOWN STATE                      
PG141POVKT0554                                                                  
PG19MF 47 4521000456                                                            
PG20                                     CITY                 TX US             
PG19DEQ47 4521000456                                                            
PG20                                     CITY                 TX US             
PG19FD116 456789652                                                             
PG20                                     OLYMPIA              WA US             
PG21FD1JOHN TESTCONTACT FOR FD               TEST@TEST.COM                      
PG60INAA LONG NAME SMITH                                                        
PG19DP 16 456789652                                                             
PG20                                     OLYMPIA              WA US             
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21PK PGA CONTACT FOR TEST   091245022      TEST@TEST.COM                      
PG23FME  Y                                                                      
PG23IFE  N                                                                      
PG23PKC  A00125                                                                 
PG23VOL  12 OUNCE                                                               
PG25   000000 1142536                   1101201412312014                        
PG261000000100000CS                                                             
PG262000000002400BO                                                             
PG263000000001200FOZ                                                            
PG281404 800 608DHLX1234567891                                                  
PG30A0724201511052   2704                                                       ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 07, 08)]
		public void TestDrugs()
		{
			SetUpData();

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "DRUGS";
			invoiceLine.Declaration.US_SchDEntry = "3902";
			invoiceLine.Declaration.US_FDAADTA = new ZDateTime(2015, 12, 15, 15, 36, 00);
			declaration.US_FDAContactName = "Joo Youm";
			declaration.US_FDAContactPhoneNo = "5555555555";
			declaration.US_FDAContactEmail = "joo.youm@test.com";
			//FDA line 1
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_PRE;
			fda.US_ProductCode = "68YAA99";
			fda.US_ProdCountry = "FR";
			fda.US_Description = "Amoxicillin tabs";
			fda.US_BrandName = "Amoxicillin Forte";
			fda.US_TotalValue = 2000m;

			var manufacturer = Factory.New<OrgHeader>();
			DeclarationTestHelper.AddPGAContact(manufacturer, "Test", "Contact For FDA For Testing", null, "TestContact@test.com", null);
			var address = manufacturer.Addresses.AddNew();
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "456378259");
			address.OA_City = "ALBERTA";
			address.OA_RL_NKRelatedPortCode = "US2CW";
			address.OA_State = "TX";
			fda.US_ManufacturerAddress = address.PK;
			fda.US_OA_ShipperAddress = address.PK;

			var fdaImporter = Factory.New<OrgHeader>();
			DeclarationTestHelper.AddPGAContact(fdaImporter, "John", "Smith", null, "JohnTestContact@test.com", null);
			var impAddress = fdaImporter.Addresses.AddNew();
			impAddress.OA_City = "LOS ALAM";
			impAddress.OA_RL_NKRelatedPortCode = "US2CW";
			impAddress.OA_State = "CA";
			impAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "4568214563");
			fda.US_FDAImporterAddress = impAddress.PK;
			fda.US_Remarks = "REMARKS SHOULD BE UNDER PG02";

			var aoc1 = fda.AffirmationCodes.AddNew();
			aoc1.CY_Code = ACE_AffirmationOfComplianceList.Codes.REG;
			aoc1.CY_Data = "0123458521";

			fda.US_Qty1 = 2m;
			fda.US_UQ1 = FDABaseUQList.Codes.G;

			fda.US_Qty2 = 10m;
			fda.US_UQ2 = FDABaseUQList.Codes.TAB;

			var activeIngr = fda.ProductConstituentElements.AddNew();
			activeIngr.US_PGANameOfTheConstituentElement = "amoxicillin trihydrate";
			activeIngr.US_PGAQuantityOfConstituentElement = 5m;
			activeIngr.US_PGAUnitOfMeasure = "ML";
			activeIngr.US_PGAPercentOfConstituentElement = 25m;

			var producer = Factory.New<OrgHeader>();
			DeclarationTestHelper.AddPGAContact(producer, "John", "Smith", null, "testContact@test.com", null);
			var address1 = producer.Addresses.AddNew();
			address1.OA_City = "LOS ALAM";
			address1.OA_RL_NKRelatedPortCode = "US2CW";
			address1.OA_State = "CA";
			address1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "4568214563");
			activeIngr.US_OA_ProducerAddress = address1.PK;

			//producer same as activeIngr
			var activeIngr2 = fda.ProductConstituentElements.AddNew();
			activeIngr2.US_PGANameOfTheConstituentElement = "VACCINE MODIFIED ANKARA VIRUS (MVA)EXPRESSING GLYCOP";
			activeIngr2.US_PGAQuantityOfConstituentElement = 5m;
			activeIngr2.US_PGAUnitOfMeasure = "ML";
			activeIngr2.US_PGAPercentOfConstituentElement = 25m;
			activeIngr2.US_OA_ProducerAddress = address1.PK;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(@"OI        DRUGS                                                                 
PG01001FDADRUPRE                                                                
PG02PFDP 68YAA99                                                                
PG24GEN     REMARKS SHOULD BE UNDER PG02                                        
PG0639 FR                                                                       
PG07AMOXICILLIN FORTE                                                           
PG10                   AMOXICILLIN TABS                                         
PG04YAMOXICILLIN TRIHYDRATE                             000000000500ML   0250000
PG04YVACCINE MODIFIED ANKARA VIRUS (MVA)EXPRESSING GLYCO000000000500ML   0250000
PG19MF 16 456378259                                                             
PG20                                     ALBERTA              TX US             
PG19DEQ16 456378259                                                             
PG20                                     ALBERTA              TX US             
PG19FD147 4568214563                                                            
PG20                                     LOS ALAM             CA US             
PG21FD1JOHN SMITH                            JOHNTESTCONTACT@TEST.COM           
PG19GD 47 4568214563                                                            
PG20                                     LOS ALAM             CA US             
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21PK JOO YOUM               5555555555     JOO.YOUM@TEST.COM                  
PG23REG  0123458521                                                             
PG25                                                    000000002000            
PG261000000001000TAB                                                            
PG262000000000200G                                                              
PG30A1215201515362   3902                                                       ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 04, 20)]
		public void TestFDADisclaimer()
		{
			SetUpData();

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_FDADisclaimReason = PGADisclaimReasonList.Codes.B;
			invoiceLine.JI_Description = "TEST FDA";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        TEST FDA                                                              
PG01001FDAFDA                                                                  B", message.EM_FormattedMessageText);
		}

		[TestDate(2016, 8, 29)]
		public void TestFDADisclaimerForPGACorrection()
		{
			SetUpData();

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_FDADisclaimReason = PGADisclaimReasonList.Codes.B;
			invoiceLine.JI_Description = "TEST FDA FOR PGA CORRECTION";

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			new PGACorrectionMessageBuilder(seEntry, ACEEntrySummaryMessageSendingOption.New(), false).GenerateMessage();
			AssertEquals(1, seEntry.Messages.Count);
			AssertMultilineASCIIEquals("ACE PGA Correction",
@"B         CA                                               <<MSGNO PLACEHOLDER>>
CA10RXJ5                                                                        
Y         CA", seEntry.Messages[0].EM_FormattedMessageText);

			seEntry.Messages.RemoveAndDeleteAll();
			var aceFDA = invoiceLine.ACE_FDALines.AddNew();
			aceFDA.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;

			new PGACorrectionMessageBuilder(seEntry, ACEEntrySummaryMessageSendingOption.New(), false).GenerateMessage();
			AssertEquals(1, seEntry.Messages.Count);
			AssertMultilineASCIIEquals("ACE PGA Correction",
@"B         CA                                               <<MSGNO PLACEHOLDER>>
CA10RXJ5                                                                        
CA4000001                                                                       
CA60                                                                            
OI        TEST FDA FOR PGA CORRECTION                                           
PG01001FDAFDA                                                                  B
Y         CA", seEntry.Messages[0].EM_FormattedMessageText);
		}

		public void TestPRCountryForPK()
		{
			SetUpData();
			declaration.Branch.OrgProxy.OH_RL_NKClosestPort = "PRADJ";
			declaration.Branch.OrgProxy.MainAddress.CompanyName = "CargoWise";
			declaration.Branch.OrgProxy.MainAddress.Address1 = "Address line 1";
			declaration.Branch.OrgProxy.MainAddress.Address2 = "Address line 2";
			declaration.Branch.OrgProxy.MainAddress.Postcode = "123455";
			declaration.Branch.OrgProxy.MainAddress.OA_State = "PR";
			declaration.Branch.OrgProxy.MainAddress.OA_City = "Olympia";
			declaration.Branch.OrgProxy.MainAddress.OA_RN_NKCountryCode = "PR";

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_ADD;
			fda.US_ProductCode = "74D--TF";
			fda.US_ProdCountry = "US";
			fda.US_FDAForcePN = true;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			AssertContains(@"PG19PNT                  CARGOWISE                       ADDRESS LINE 1         
PG20ADDRESS LINE 2                       OLYMPIA              PR US123455       
PG19PK                   CARGOWISE                       ADDRESS LINE 1         
PG20ADDRESS LINE 2                       OLYMPIA              PR US123455", message.EM_FormattedMessageText);
		}

		public void TestFDAPGAContactBlocksForFSVPImporter()
		{
			SetUpData();

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "UNIPOLAR EPICARDIAL LEAD";
			invoiceLine.Declaration.US_FDAADTA = new ZDateTime(2015, 04, 19, 14, 00, 00);
			invoiceLine.Declaration.US_SchDEntry = "2704";

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_ADD;
			fda.US_ProductCode = "74D--TF";
			fda.US_ProdCountry = "US";

			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			address.CompanyName = "CargoWise";
			address.Address1 = "No.123 Nanjing City";
			address.Address2 = "Address 2";
			address.Postcode = "123455";
			address.OA_State = "WA";
			address.OA_City = "Olympia";
			address.OA_RN_NKCountryCode = "US";
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "456789652");
			DeclarationTestHelper.AddFSVPContact(address, "JOHN", "SMITH", null, "test@test.com", null);

			fda.US_FSVPImporterAddress = address.PK;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG19FSV16 456789652      CARGOWISE                       NO.123 NANJING CITY    
PG20ADDRESS 2                            OLYMPIA              WA US123455       
PG21FSVJOHN SMITH                            TEST@TEST.COM   ", message.EM_FormattedMessageText);
		}

		public void TestFDAPGAContactBlocksForFSVPImporterForStandAlone()
		{
			SetUpData();

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "UNIPOLAR EPICARDIAL LEAD";
			invoiceLine.Declaration.US_FDAADTA = new ZDateTime(2015, 04, 19, 14, 00, 00);
			invoiceLine.Declaration.US_SchDEntry = "2704";

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_ADD;
			fda.US_ProductCode = "74D--TF";
			fda.US_ProdCountry = "US";

			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			address.CompanyName = "CargoWise";
			address.Address1 = "No.123 Nanjing City";
			address.Address2 = "Address 2";
			address.Postcode = "123455";
			address.OA_State = "WA";
			address.OA_City = "Olympia";
			address.OA_RN_NKCountryCode = "US";
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "456789652");
			DeclarationTestHelper.AddFSVPContact(orgHeader.MainAddress, "JOHN", "SMITH", null, "test@test.com", null);

			fda.US_FSVPImporterAddress = address.PK;

			var declaration = invoiceLine.Declaration;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = false;
			declaration.US_EnableSPN = true;
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.ENT;
			var priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(declaration);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);
			var priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			new ACEPriorNoticeMessageBuilder(priorNoticeHeader, "A").GenerateMessage();
			AssertEquals(1, declaration.Messages.Count);
			var message = declaration.Messages[0];

			AssertNotContains("PG19FSV", message.EM_FormattedMessageText);
			AssertNotContains("PG21FSV", message.EM_FormattedMessageText);
		}

		[TestDate(2017, 6, 29)]
		public void TestGoodsFromFTZForFDA()
		{
			SetUpData();

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "TEST GOODS FROM FTZ";
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			invoiceLine.Declaration.US_FDAADTA = new ZDateTime(2017, 06, 29, 13, 00, 00);
			invoiceLine.Declaration.US_SchDEntry = "2704";
			invoiceLine.Declaration.US_GoodsFromFTZ = "Z082";

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			fda.US_ProductCode = "40BAR01";
			fda.US_ProdCountry = "US";
			fda.US_ProducerType = ProducerFirmTypeList.Codes.M;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains("PG30F            4   Z082                                                       ", message.EM_FormattedMessageText);
		}

		public void TestFDAImporterForStandAlonePriorNotice()
		{
			SetUpData();

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "UNIPOLAR EPICARDIAL LEAD";
			invoiceLine.Declaration.US_FDAADTA = new ZDateTime(2015, 04, 19, 14, 00, 00);
			invoiceLine.Declaration.US_SchDEntry = "2704";

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_ADD;
			fda.US_ProductCode = "74D--TF";
			fda.US_ProdCountry = "US";

			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			address.CompanyName = "CargoWise";
			address.Address1 = "No.123 Nanjing City";
			address.Address2 = "Address 2";
			address.Postcode = "123455";
			address.OA_State = "WA";
			address.OA_City = "Olympia";
			address.OA_RN_NKCountryCode = "US";
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "456789652");
			fda.US_FDAImporterAddress = address.PK;

			var declaration = invoiceLine.Declaration;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = false;
			declaration.US_EnableSPN = true;
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.ENT;
			var priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(declaration);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);
			var priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			new ACEPriorNoticeMessageBuilder(priorNoticeHeader, "A").GenerateMessage();
			AssertEquals(1, declaration.Messages.Count);
			var message = declaration.Messages[0];

			AssertNotContains("PG19FD1", message.EM_FormattedMessageText);
			AssertNotContains("PG21FD1", message.EM_FormattedMessageText);
		}

		public void TestFDAPG19ForFOO_CCW()
		{
			SetUpData();

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "UNIPOLAR EPICARDIAL LEAD";
			invoiceLine.Declaration.US_FDAADTA = new ZDateTime(2024, 04, 19, 14, 00, 00);
			invoiceLine.Declaration.US_SchDEntry = "2704";

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_CCW;
			fda.US_ProductCode = "74D--TF";
			fda.US_ProdCountry = "US";
			fda.US_FDAForcePN = true;

			var org = Factory.New<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.OA_RL_NKRelatedPortCode = "US2WC";
			address.OA_State = "TX";
			address.OA_City = "CITY";
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "4521000456");
			fda.US_ManufacturerAddress = address.PK;
			fda.US_OA_ShipperAddress = address.PK;
			fda.US_FDAImporterAddress = address.PK;
			fda.US_DeliverToPartyAddress = address.PK;
			invoiceLine.Declaration.JE_OH_FDASubmitter = org.PK;
			fda.US_FSVPImporterAddress = address.PK;
			fda.US_LocationOfGoodsAddress = address.PK;
			fda.US_OwnerAddress = address.PK;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG19MF 47 4521000456                                                            
PG20                                     CITY                 TX US             
PG19DEQ47 4521000456                                                            
PG20                                     CITY                 TX US             
PG19FD147 4521000456                                                            
PG20                                     CITY                 TX US             
PG19DP 47 4521000456                                                            
PG20                                     CITY                 TX US             
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
", message.EM_FormattedMessageText);
		}
	}
}
