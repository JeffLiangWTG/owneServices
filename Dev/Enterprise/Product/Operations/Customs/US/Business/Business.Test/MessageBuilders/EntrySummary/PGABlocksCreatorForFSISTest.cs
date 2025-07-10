using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForFSISTest : PGABlocksCreatorTest
	{
		[TestDate(2013, 9, 24)]
		public void TestFSISMessageBlocksCreation()
		{
			SetUpData();
			var orgProxy = declaration.Branch.OrgProxy;
			UpdateAddress(orgProxy.MainAddress, "Proxy Address Line 1", "Proxy Address Line 2", "Chicago City", "CH", "86954-3251", "+1 641 8564 8653", "+1 641 8564 8654", "info@proxy.com");
			orgProxy.MainAddress.OA_RL_NKRelatedPortCode = "US2CW";
			AddCustomsAddress(orgProxy, "Proxy Customs Address Line 1", "Proxy Customs Address Line 2", "Michigan City", "IL", "596508654", "+1 642 8564 8653", "+1 642 8564 8654", "customs@proxy.com");
			var contact = orgProxy.Contacts.AddNew();
			contact.OC_ContactName = "BOB THE BUILDER";
			contact.OC_Email = "BOB@WHERE.COM";

			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true);
			declaration.US_EnableCRL = false;
			declaration.JE_DateOfArrival = ZDateTime.Today;

			var ior = Factory.New<OrgHeader>();
			declaration.IOROrgPK = ior.PK;
			ior.OH_FullName = "IMPORTER OF RECORD";
			var iorAddress = ior.MainAddress;
			iorAddress.OA_Address1 = "IOR ADDRESS 1";
			iorAddress.OA_Address2 = "IOR ADDRESS 2";
			iorAddress.OA_City = "SYDNEY";
			iorAddress.OA_RL_NKRelatedPortCode = "US2CW";
			iorAddress.OA_State = "NSW";
			iorAddress.OA_PostCode = "2017";
			DeclarationTestHelper.AddPGAContact(ior, "IOR", "ALEXANDER THE GREATEST OF ALL", "04 123456", "IOR EMAIL", "IOR FAX");

			var importer = Factory.New<OrgHeader>();
			invoiceLine.InvoiceHeader.JZ_OH_Buyer = importer.PK;
			importer.OH_FullName = "BUYER";
			var imAddress = importer.MainAddress;
			imAddress.OA_Address1 = "BUYER ADDRESS 1";
			imAddress.OA_Address2 = "BUYER ADDRESS 2";
			imAddress.OA_City = "MELBOURN";
			imAddress.OA_RL_NKRelatedPortCode = "US2CW";
			imAddress.OA_State = "MEL";
			imAddress.OA_PostCode = "2011";
			DeclarationTestHelper.AddPGAContact(importer, "BUYER", "CONTACT", "04 654321", "BUYER EMAIL", "BUYER FAX");

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "TV";
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			broker.GS_FullName = "BROKER";
			broker.GS_WorkPhone = "04 010101";
			broker.GS_EmailAddress = "BROKER EMAIL";
			broker.GS_FaxNum = "BROKER FAX";
			declaration.US_FDAContactName = "WENDY THE DESTROYER";
			declaration.US_FDAContactPhoneNo = "+164285648734";
			declaration.US_FDAContactEmail = "WENDY@WHERE.COM";

			invoiceLine.JI_Description = "FSIS TEST";
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			var cer1 = invoiceLine.FSISLines.AddNew();
			cer1.US_HealthCertificateNumber = "CER1";
			cer1.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Australia;
			cer1.US_CommercialDescription = "PRODUCT ONE";
			cer1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.VietNam;
			cer1.US_ProductID = "100578620002680";
			cer1.US_ProductIDQualifier = GlobalUniqueProductCodeQualifierList.Codes.SRV;
			cer1.US_IntendedUseCode = ACEIntendedUseBaseCodeList.Codes._260000;
			cer1.US_ExportingEstNo = "EXP EST 1";
			cer1.US_ImportingEstNo = "USLAX";
			cer1.US_SealNumbers = "SEAL1,SEAL2,SEAL3";
			cer1.US_DateOfInspection = new ZDateTime(2013, 9, 24);

			var lot1 = cer1.Lots.AddNew();
			lot1.US_LotNumber = "LOT 1";
			lot1.US_NoOfUnit1 = 1;
			lot1.US_UQ1 = ShippingOrPackingingUnitList.Codes.Case;
			lot1.US_NoOfUnit2 = 2;
			lot1.US_UQ2 = ShippingOrPackingingUnitList.Codes.Cylinder;
			lot1.US_ShippingMarks = "MARK 1";
			lot1.US_NetWeight = 7m;
			lot1.US_WeightUQ = Core.Constants.Weight.Pounds;

			lot1.US_Species = FSISProductSpeciesNameList.Codes.GoatMeat;
			lot1.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.EEP;
			lot1.US_ProductCharacteristicQualifier = EEPCharacteristicList.Codes._3B;
			lot1.US_ProducingEstNo = "PRO EST 1";

			var lot2 = cer1.Lots.AddNew();
			lot2.US_LotNumber = "LOT 1";
			lot2.US_NoOfUnit1 = 11;
			lot2.US_UQ1 = ShippingOrPackingingUnitList.Codes.Cup;
			lot2.US_ShippingMarks = "MARK 2";
			lot2.US_NetWeight = 77;
			lot2.US_WeightUQ = Core.Constants.Weight.Ounces;

			lot2.US_Species = FSISProductSpeciesNameList.Codes.GoatMeat;
			lot2.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.EEP;
			lot2.US_ProductCharacteristicQualifier = EEPCharacteristicList.Codes._3B;
			lot2.US_ProducingEstNo = "PRO EST 1";

			var cer2 = invoiceLine.FSISLines.AddNew();
			cer2.US_HealthCertificateNumber = "CER2";
			cer2.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.VietNam;
			cer2.US_CommercialDescription = "PRODUCT TWO";
			cer2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Yemen;
			cer2.US_ProductID = "203918503910256";
			cer2.US_ProductIDQualifier = GlobalUniqueProductCodeQualifierList.Codes.AI;
			cer2.US_IntendedUseCode = ACEIntendedUseBaseCodeList.Codes._210000;
			cer2.US_ExportingEstNo = "EXP EST 2";
			cer2.US_ImportingEstNo = "USCHI";
			cer2.US_SealNumbers = "SEALA,SEALB,SEALC";
			cer2.US_DateOfInspection = new ZDateTime(2013, 9, 24);

			var lot3 = cer2.Lots[0];
			lot3.US_LotNumber = "LOT 3";
			lot3.US_NoOfUnit1 = 10;
			lot3.US_UQ1 = ShippingOrPackingingUnitList.Codes.Bag;
			lot3.US_NoOfUnit2 = 20;
			lot3.US_UQ2 = ShippingOrPackingingUnitList.Codes.Carton;
			lot3.US_StartDate = new ZDateTime(2014, 1, 4);
			lot3.US_EndDate = new ZDateTime(2014, 10, 5);

			lot3.US_ShippingMarks = "MARK 3";
			lot3.US_NetWeight = 70m;
			lot3.US_WeightUQ = Core.Constants.Weight.Grams;

			lot3.US_Species = FSISProductSpeciesNameList.Codes.GoosePoultry;
			lot3.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.HTSS;
			lot3.US_ProductCharacteristicQualifier = HTSSCharacteristicList.Codes._1F;
			lot3.US_ProducingEstNo = "PRO EST 2";

			lot3.US_SourceEstNo = "SRC EST 2";
			lot3.US_SourceCountry = "CA";

			var lot4 = cer2.Lots[1];
			lot4.US_LotNumber = "LOT 4";
			lot4.US_NoOfUnit1 = 13;
			lot4.US_UQ1 = ShippingOrPackingingUnitList.Codes.Case;
			lot4.US_ShippingMarks = "MARK 4";
			lot4.US_NetWeight = 73m;
			lot4.US_WeightUQ = Core.Constants.Weight.Tonnes;

			lot4.US_Species = FSISProductSpeciesNameList.Codes.GoosePoultry;
			lot4.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.HTSS;
			lot4.US_ProductCharacteristicQualifier = HTSSCharacteristicList.Codes._1F;
			lot4.US_ProducingEstNo = "PRO EST 2";

			cer1.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_FSISSignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000                                              
OI        FSIS TEST                                                             
PG01001FSIFSI     SRV 100578620002680    260.000                                
PG02P                                                                           
PG0639 VN                                                                       
PG13                                   ISOAU                                    
PG14 FS7CER1                                                                    
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG60INAEST OF ALL                                                               
PG55CI                                                                          
PG19CN                   BUYER                           BUYER ADDRESS 1        
PG20BUYER ADDRESS 2                      MELBOURN             MELUS2011         
PG21CN BUYER CONTACT          04654321       BUYER EMAIL                        
PG19CB                   EDI CUSTOMS BROKERS             PROXY CUSTOMS ADDRESS L
PG60AD1INE 1                                                                    
PG20PROXY CUSTOMS ADDRESS LINE 2         MICHIGAN CITY        IL US596508654    
PG21CB WENDY THE DESTROYER    164285648734   WENDY@WHERE.COM                    
PG22 956         CI FS3 Y09242013                                               
PG24GEN     SEAL1                                                               
PG24GEN     SEAL2                                                               
PG24GEN     SEAL3                                                               
PG30I09242013    8   USLAX                                                      
PG01002FSIFSI     AI  203918503910256    210.000                                
PG02P                                                                           
PG0639 YE                                                                       
PG13                                   ISOVN                                    
PG14 FS7CER2                                                                    
PG50                                                                            
PG10FS1   12   HTSS1F                                                           
PG19EXE   EXP EST 2                                                             
PG19PE    PRO EST 2                                                             
PG19SOE   SRC EST 2                                                             
PG0630 CA                                                                       
PG25           LOT 3                    0104201410052014                        
PG261000000001000BG   MARK 3                                                    
PG262000000002000CT                                                             
PG29LB 000000000015                                                             
PG51                                                                            
PG50                                                                            
PG10FS1   12   HTSS1F                                                           
PG19EXE   EXP EST 2                                                             
PG19PE    PRO EST 2                                                             
PG25           LOT 4                                                            
PG261000000001300CS   MARK 4                                                    
PG29LB 000016093745                                                             
PG51                                                                            
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG60INAEST OF ALL                                                               
PG55CI                                                                          
PG19CN                   BUYER                           BUYER ADDRESS 1        
PG20BUYER ADDRESS 2                      MELBOURN             MELUS2011         
PG21CN BUYER CONTACT          04654321       BUYER EMAIL                        
PG19CB                   EDI CUSTOMS BROKERS             PROXY CUSTOMS ADDRESS L
PG60AD1INE 1                                                                    
PG20PROXY CUSTOMS ADDRESS LINE 2         MICHIGAN CITY        IL US596508654    
PG21CB WENDY THE DESTROYER    164285648734   WENDY@WHERE.COM                    
PG22 956         CI FS3 Y09242013                                               
PG24GEN     SEALA                                                               
PG24GEN     SEALB                                                               
PG24GEN     SEALC                                                               
PG30I09242013    8   USCHI                                                      
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including FSIS data", expectedMessage, message.EM_FormattedMessageText);

			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false);
			message = builder.PopulateMessage();
			AssertContains("ACE Entry Summary message including FSIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		[TestDate(2013, 9, 24)]
		public void TestCertifyingIndividual()
		{
			SetUpData();
			var orgProxy = declaration.Branch.OrgProxy;
			UpdateAddress(orgProxy.MainAddress, "Proxy Address Line 1", "Proxy Address Line 2", "Chicago City", "CH", "86954-3251", "+1 641 8564 8653", "+1 641 8564 8654", "info@proxy.com");
			orgProxy.MainAddress.OA_RL_NKRelatedPortCode = "US2CW";
			AddCustomsAddress(orgProxy, "Proxy Customs Address Line 1", "Proxy Customs Address Line 2", "Michigan City", "IL", "596508654", "+1 642 8564 8653", "+1 642 8564 8654", "customs@proxy.com");

			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			var cer1 = invoiceLine.FSISLines.AddNew();
			cer1.US_HealthCertificateNumber = "CER1";
			cer1.US_CertifyingIndividual = "CB";
			cer1.US_PGAContactName = "wukong";
			cer1.US_PGAContactEmail = "wukong@shuiliandong.com";
			cer1.US_PGAContactPhoneNo = "9999999999";

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_FSISSignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var expectedMessage =
@"PG19CB                   EDI CUSTOMS BROKERS             PROXY CUSTOMS ADDRESS L
PG60AD1INE 1                                                                    
PG20PROXY CUSTOMS ADDRESS LINE 2         MICHIGAN CITY        IL US596508654    
PG21CB WUKONG                 9999999999     WUKONG@SHUILIANDONG.COM            
PG55CI                                                                          
PG22 956         CI FS3 Y09242013                                               
";
			AssertContains("ACE Entry Summary message including FSIS data with CB set to Certifying Individual", expectedMessage, message.EM_FormattedMessageText);

			var ior = Factory.New<OrgHeader>();
			declaration.IOROrgPK = ior.PK;
			ior.OH_FullName = "IMPORTER OF RECORD";
			var iorAddress = ior.MainAddress;
			iorAddress.OA_Address1 = "IOR ADDRESS 1";
			iorAddress.OA_Address2 = "IOR ADDRESS 2";
			iorAddress.OA_City = "SYDNEY";
			iorAddress.OA_RL_NKRelatedPortCode = "US2CW";
			iorAddress.OA_State = "NSW";
			iorAddress.OA_PostCode = "2017";
			DeclarationTestHelper.AddPGAContact(ior, "IOR", "ALEXANDER THE GREATEST OF ALL", "04 123456", "IOR EMAIL", "IOR FAX");

			cer1.US_CertifyingIndividual = "IM";
			cer1.US_PGAContactName = "wukong";
			cer1.US_PGAContactEmail = "wukong@shuiliandong.com";
			cer1.US_PGAContactPhoneNo = "9999999999";

			action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_FSISSignDate = ZDateTime.Today;
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			message = builder.PopulateMessage();

			expectedMessage =
@"PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM WUKONG                 9999999999     WUKONG@SHUILIANDONG.COM            
PG55CI                                                                          
";
			AssertContains("ACE Entry Summary message including FSIS data with CB set to Certifying Individual", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestPRCountryForCertifyingIndividual()
		{
			SetUpData();
			var orgProxy = declaration.Branch.OrgProxy;
			UpdateAddress(orgProxy.MainAddress, "Proxy Address Line 1", "Proxy Address Line 2", "Chicago City", "PR", "86954-3251", "+1 641 8564 8653", "+1 641 8564 8654", "info@proxy.com");
			orgProxy.MainAddress.OA_RL_NKRelatedPortCode = "PR2CW";
			AddCustomsAddress(orgProxy, "Proxy Customs Address Line 1", "Proxy Customs Address Line 2", "Michigan City", "PR", "596508654", "+1 642 8564 8653", "+1 642 8564 8654", "customs@proxy.com");

			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			var cer1 = invoiceLine.FSISLines.AddNew();
			cer1.US_HealthCertificateNumber = "CER1";
			cer1.US_CertifyingIndividual = "CB";
			cer1.US_PGAContactName = "wukong";
			cer1.US_PGAContactEmail = "wukong@shuiliandong.com";
			cer1.US_PGAContactPhoneNo = "9999999999";

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_FSISSignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var expectedMessage =
@"PG19CB                   EDI CUSTOMS BROKERS             PROXY CUSTOMS ADDRESS L
PG60AD1INE 1                                                                    
PG20PROXY CUSTOMS ADDRESS LINE 2         MICHIGAN CITY        PR US596508654    ";

			AssertContains("ACE Entry Summary message including FSIS data with CB set to Certifying Individual", expectedMessage, message.EM_FormattedMessageText);

			var ior = Factory.New<OrgHeader>();
			declaration.IOROrgPK = ior.PK;
			ior.OH_FullName = "IMPORTER OF RECORD";
			var iorAddress = ior.MainAddress;
			iorAddress.OA_Address1 = "IOR ADDRESS 1";
			iorAddress.OA_Address2 = "IOR ADDRESS 2";
			iorAddress.OA_City = "SYDNEY";
			iorAddress.OA_RL_NKRelatedPortCode = "PR2CW";
			iorAddress.OA_State = "PR";
			iorAddress.OA_PostCode = "2017";
			DeclarationTestHelper.AddPGAContact(ior, "IOR", "ALEXANDER THE GREATEST OF ALL", "04 123456", "IOR EMAIL", "IOR FAX");

			cer1.US_CertifyingIndividual = "IM";
			cer1.US_PGAContactName = "wukong";
			cer1.US_PGAContactEmail = "wukong@shuiliandong.com";
			cer1.US_PGAContactPhoneNo = "9999999999";

			action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_FSISSignDate = ZDateTime.Today;
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			message = builder.PopulateMessage();

			expectedMessage =
@"PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               PR US2017         ";
			AssertContains("ACE Entry Summary message including FSIS data with CB set to Certifying Individual", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestWhenTwoSetsOfPGADataExist()
		{
			SetUpData();

			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true);
			invoiceLine.JI_Description = "FSIS TEST";

			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;

			invoiceLine.FSISLines.AddNew();
			invoiceLine.PSTLines.AddNew();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			AssertContains("Only one OI per entry line", @"50           0000000000 0000010000                                              
OI        FSIS TEST                                                             
PG01", message.EM_FormattedMessageText);
		}

		public void TestCertifyingDateIsSetToCurrentDateIfEmpty()
		{
			SetUpData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "ACE";
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = "SE";
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.FSISLines.AddNew();

			invoiceLine.InvoiceHeader.US_FSISSignDate = ZDateTime.Empty;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var today = ZDate.Today;
			AssertContains("PG22 956         CI FS3 Y" + today.ToString("MMddyyyy"), message.EM_FormattedMessageText); //11212016

			invoiceLine.InvoiceHeader.US_FSISSignDate = new ZDateTime(2016, 11, 29);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			action = GetAction(declaration);
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			AssertContains("PG22 956         CI FS3 Y11292016", message.EM_FormattedMessageText);
		}
	}
}
