using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForCPSCTest : PGABlocksCreatorTest
	{
		public void TestCPSCForDisclaimed()
		{
			SetUpData();
			invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_CPSCDisclaimReason = "A";

			invoiceLine.CPSCHeaders[0].US_IntendedUseCode = "980.000";
			invoiceLine.CPSCHeaders[0].US_IntendedUseDescription = "TEST";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(@"OI                                                                              
PG01001CPSCPS                            980.000         TEST                  A", message.EM_FormattedMessageText);

			invoiceLine.US_CPSCDisclaimReason = "B";
			message = builder.PopulateMessage();
			AssertContains(@"OI                                                                              
PG01001CPSCPS                            130.006                               B", message.EM_FormattedMessageText);
		}

		public void TestCPSCForREF()
		{
			SetUpData();
			invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
			var cpscHeader = invoiceLine.CPSCHeaders.AddNew();
			cpscHeader.US_ProcessingCode = CPSCProcessingCodeList.Codes.REF;
			cpscHeader.US_RegisteredNumber = "123456";
			cpscHeader.US_ProductCode = "PRODCODE";
			cpscHeader.US_ProductCodeVersionNumber = "VERNUMBER";
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(@"OI                                                                              
PG01001CPSCPSREF                                                                
PG02PPRI PRODCODE           PRIVVERNUMBER                                       
PG07                                                        RN 123456           ", message.EM_FormattedMessageText);
		}

		public void TestCPSCForFCP()
		{
			SetUpData();
			invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
			var cpscHeader = invoiceLine.CPSCHeaders.AddNew();
			cpscHeader.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			cpscHeader.US_RegisteredNumber = "123456";
			cpscHeader.US_ProductIDType = "SRV";
			cpscHeader.US_ProductID = "5A102";
			cpscHeader.US_IntendedUseCode = "980.000";
			cpscHeader.US_SKUProductCode = "A789";
			cpscHeader.US_TradeBrandName = "BName";
			cpscHeader.US_ProductName = "Cotton";
			cpscHeader.US_ModelNumber = "47851,78945,54862";
			cpscHeader.US_SerialNumber = "87451,51862";
			cpscHeader.US_RegisteredNumber = "82451";
			cpscHeader.US_AltenateID = "82451,47851,78945,54862,112";
			cpscHeader.US_ModelColor = "RED";
			cpscHeader.US_ModelDescription = "TEST";
			cpscHeader.US_ModelStyle = "1";
			cpscHeader.US_OA_ManufacturerAddress = manufacturerAddress.PK;
			cpscHeader.US_CertificateExists = YesNoDefaultList.Codes.Yes;
			cpscHeader.US_ManufacturerMonthAndYear = "012023";
			cpscHeader.US_ManufacturerRegistryID = "MANREGID";
			cpscHeader.US_OA_CertifyingEntityAddress = certifyingEntityAddress.PK;
			cpscHeader.US_OA_ContactPointAddress = pointOfContactAddress.PK;

			var lot1 = cpscHeader.Lots.AddNew();
			lot1.US_LotNumberType = "1";
			lot1.US_LotNumber = "47851";
			lot1.US_StartDate = new ZDateTime(2023, 01, 01);
			lot1.US_EndDate = new ZDateTime(2023, 03, 31);

			var lot2 = cpscHeader.Lots.AddNew();
			lot2.US_LotNumberType = "1";
			lot2.US_LotNumber = "47851";

			var ruleAndLab = cpscHeader.RuleAndLabs.AddNew();
			ruleAndLab.US_OA_SafetyTestLocationAddress = safetyTestLocationAddress.PK;
			ruleAndLab.US_RuleCodes = "1203C,1215C";
			ruleAndLab.US_CPSCAccreditedLabID = "ACCLABID";
			ruleAndLab.US_PreviousInspectionDate = new ZDateTime(2022, 10, 15);

			var report = ruleAndLab.ReportAndLabs.AddNew();
			report.US_RemarksType = "CP1";
			report.US_RemarksText = "REPORT ID 112233";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(@"OI                                                                              
PG01001CPSCPSFCP  SRV 5A102              980.000                                
PG02PSKU A789                                                                   
PG07BNAME                              COTTON         012023MN 47851            
PG07BNAME                              COTTON         012023MN 78945            
PG07BNAME                              COTTON         012023MN 54862            
PG07BNAME                              COTTON         012023SE 87451            
PG07BNAME                              COTTON         012023SE 51862            
PG07BNAME                              COTTON         012023RN 82451            
PG07BNAME                              COTTON         012023ALT82451            
PG07BNAME                              COTTON         012023ALT47851            
PG07BNAME                              COTTON         012023ALT78945            
PG07BNAME                              COTTON         012023ALT54862            
PG07BNAME                              COTTON         012023ALT112              
PG10           PC9 MC  RED                                                      
PG10           PC9 MD  TEST                                                     
PG10           PC9 MS  1                                                        
PG19MF SBMMANREGID       TEST ATF                        1234 PEACHTREE STREET  
PG20                                     ATLANTA              GA US30301        
PG19CE                   AZIENDA DEMO SPA                CORSO STATI UNITI 1    
PG20CAMIN                                PADOVA                  IT35127        
PG19PK                   EAST DIRECT COMPANY             22/F, 3 LOCKHAKT RD.   
PG20                                     WANCHHC                 HK22333        
PG30L10152022                                                                   
PG19ITL   ACCLABID                                                              
PG60CIT1203C                                                                    
PG60CIT1215C                                                                    
PG60CP1REPORT ID 112233                                                         
PG22                CPY                                                         
PG25          147851                    0101202303312023                        
PG25          147851                                                            ", message.EM_FormattedMessageText);
		}

		public void TestCPSCForFGC()
		{
			SetUpData();
			invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
			var cpscHeader = invoiceLine.CPSCHeaders.AddNew();
			cpscHeader.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			cpscHeader.US_RegisteredNumber = "123456";
			cpscHeader.US_ProductIDType = "SRV";
			cpscHeader.US_ProductID = "5A102";
			cpscHeader.US_IntendedUseCode = "980.000";
			cpscHeader.US_SKUProductCode = "A789";
			cpscHeader.US_TradeBrandName = "BName";
			cpscHeader.US_ProductName = "Cotton";
			cpscHeader.US_ModelNumber = "47851,78945,54862";
			cpscHeader.US_SerialNumber = "87451,51862";
			cpscHeader.US_RegisteredNumber = "82451";
			cpscHeader.US_AltenateID = "82451,47851,78945,54862,112";
			cpscHeader.US_ModelColor = "RED";
			cpscHeader.US_ModelDescription = "TEST";
			cpscHeader.US_ModelStyle = "1";
			cpscHeader.US_OA_ManufacturerAddress = manufacturerAddress.PK;
			cpscHeader.US_NoLabTestingRequired = true;
			cpscHeader.US_RuleCodes = "1401A,1500B";

			var lot1 = cpscHeader.Lots.AddNew();
			lot1.US_LotNumberType = "1";
			lot1.US_LotNumber = "47851";

			var lot2 = cpscHeader.Lots.AddNew();
			lot2.US_LotNumberType = "2";
			lot2.US_LotNumber = "47852";

			var ruleAndLab = cpscHeader.RuleAndLabs.AddNew();
			ruleAndLab.US_RuleCodes = "1203C,1215C";
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "LAB TESTING INC.";
			var address = header.MainAddress;
			address.Address1 = "122 TEST RD.";
			address.Address2 = "UNIT 35";
			address.Postcode = "19019";
			address.City = "PHILADELPHIA";
			address.StateCode = "PA";
			address.OA_RN_NKCountryCode = "US";
			ruleAndLab.SafetyTestLocationOrgPK = header.PK;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(@"OI                                                                              
PG01001CPSCPSFGC  SRV 5A102              980.000                                
PG02PSKU A789                                                                   
PG07BNAME                              COTTON               MN 47851            
PG07BNAME                              COTTON               MN 78945            
PG07BNAME                              COTTON               MN 54862            
PG07BNAME                              COTTON               SE 87451            
PG07BNAME                              COTTON               SE 51862            
PG07BNAME                              COTTON               RN 82451            
PG07BNAME                              COTTON               ALT82451            
PG07BNAME                              COTTON               ALT47851            
PG07BNAME                              COTTON               ALT78945            
PG07BNAME                              COTTON               ALT54862            
PG07BNAME                              COTTON               ALT112              
PG10           PC9 MC  RED                                                      
PG10           PC9 MD  TEST                                                     
PG10           PC9 MS  1                                                        
PG19MF                   TEST ATF                        1234 PEACHTREE STREET  
PG20                                     ATLANTA              GA US30301        
PG19NOL                                                                         
PG60CIT1401A                                                                    
PG60CIT1500B                                                                    
PG30L                                                                           
PG19LAB                  LAB TESTING INC.                122 TEST RD.           
PG20UNIT 35                              PHILADELPHIA         PA US19019        
PG60CIT1203C                                                                    
PG60CIT1215C                                                                    
PG25          147851                                                            
PG25          247852                                                            ", message.EM_FormattedMessageText);
		}

		public void TestPG30InCPSCWhenRoleCodeLAB()
		{
			SetUpData();
			invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
			var cpscHeader = invoiceLine.CPSCHeaders.AddNew();
			cpscHeader.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			cpscHeader.US_RegisteredNumber = "123456";
			cpscHeader.US_ProductIDType = "SRV";
			cpscHeader.US_ProductID = "5A102";
			cpscHeader.US_IntendedUseCode = "980.000";
			cpscHeader.US_SKUProductCode = "A789";
			cpscHeader.US_TradeBrandName = "BName";
			cpscHeader.US_ProductName = "Cotton";
			cpscHeader.US_ModelNumber = "47851,78945,54862";
			cpscHeader.US_SerialNumber = "87451,51862";
			cpscHeader.US_RegisteredNumber = "82451";
			cpscHeader.US_AltenateID = "82451,47851,78945,54862,112";
			cpscHeader.US_ModelColor = "RED";
			cpscHeader.US_ModelDescription = "TEST";
			cpscHeader.US_ModelStyle = "1";
			cpscHeader.US_OA_ManufacturerAddress = manufacturerAddress.PK;
			cpscHeader.US_NoLabTestingRequired = true;
			cpscHeader.US_RuleCodes = "1401A,1500B";

			var lot1 = cpscHeader.Lots.AddNew();
			lot1.US_LotNumberType = "1";
			lot1.US_LotNumber = "47851";

			var lot2 = cpscHeader.Lots.AddNew();
			lot2.US_LotNumberType = "2";
			lot2.US_LotNumber = "47852";

			var ruleAndLab = cpscHeader.RuleAndLabs.AddNew();
			ruleAndLab.US_RuleCodes = "1203C,1215C";
			ruleAndLab.US_CPSCAccreditedLabID = "";

			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "LAB TESTING INC.";
			var address = header.MainAddress;
			address.Address1 = "122 TEST RD.";
			address.Address2 = "UNIT 35";
			address.Postcode = "19019";
			address.City = "PHILADELPHIA";
			address.StateCode = "PA";
			address.OA_RN_NKCountryCode = "US";
			ruleAndLab.SafetyTestLocationOrgPK = header.PK;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(@"OI                                                                              
PG01001CPSCPSFGC  SRV 5A102              980.000                                
PG02PSKU A789                                                                   
PG07BNAME                              COTTON               MN 47851            
PG07BNAME                              COTTON               MN 78945            
PG07BNAME                              COTTON               MN 54862            
PG07BNAME                              COTTON               SE 87451            
PG07BNAME                              COTTON               SE 51862            
PG07BNAME                              COTTON               RN 82451            
PG07BNAME                              COTTON               ALT82451            
PG07BNAME                              COTTON               ALT47851            
PG07BNAME                              COTTON               ALT78945            
PG07BNAME                              COTTON               ALT54862            
PG07BNAME                              COTTON               ALT112              
PG10           PC9 MC  RED                                                      
PG10           PC9 MD  TEST                                                     
PG10           PC9 MS  1                                                        
PG19MF                   TEST ATF                        1234 PEACHTREE STREET  
PG20                                     ATLANTA              GA US30301        
PG19NOL                                                                         
PG60CIT1401A                                                                    
PG60CIT1500B                                                                    
PG30L                                                                           
PG19LAB                  LAB TESTING INC.                122 TEST RD.           
PG20UNIT 35                              PHILADELPHIA         PA US19019        
PG60CIT1203C                                                                    
PG60CIT1215C                                                                    
PG25          147851                                                            
PG25          247852                                                            ", message.EM_FormattedMessageText);
		}

		protected override void SetUpData()
		{
			base.SetUpData();

			manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "TOYOTA (JAPAN)";
			manufacturer.MainAddress.OA_Address1 = "MAIN ADDRESS";
			manufacturerAddress = manufacturer.Addresses.AddNew();
			manufacturerAddress.OA_Address1 = "1234 PEACHTREE STREET";
			manufacturerAddress.OA_City = "ATLANTA";
			manufacturerAddress.OA_State = "GA";
			manufacturerAddress.OA_RL_NKRelatedPortCode = "USLAX";
			manufacturerAddress.OA_PostCode = "30301";
			manufacturerAddress.OA_CompanyNameOverride = "TEST ATF";
			DeclarationTestHelper.AddPGAContact(manufacturerAddress, "JANE", "SMITH", "7062345678", "J.SMITH@TOYOTAAMERICA.COM", null);
			safetyTestLocationAddress = manufacturer.Addresses.AddNew();
			safetyTestLocationAddress.OA_Address1 = "LocationAddress";
			safetyTestLocationAddress.OA_City = "NJ";
			safetyTestLocationAddress.OA_State = "JS";
			safetyTestLocationAddress.OA_RL_NKRelatedPortCode = "USLAX";
			safetyTestLocationAddress.OA_PostCode = "10110";
			safetyTestLocationAddress.OA_CompanyNameOverride = "TEST CPSC";
			var certifyingEntity = Factory.New<OrgHeader>();
			certifyingEntity.OH_FullName = "AZIENDA DEMO SPA";
			certifyingEntity.MainAddress.OA_Address1 = "MAIN ADDRESS";
			certifyingEntityAddress = certifyingEntity.Addresses.AddNew();
			certifyingEntityAddress.OA_Address1 = "CORSO STATI UNITI 1";
			certifyingEntityAddress.OA_Address2 = "CAMIN";
			certifyingEntityAddress.OA_City = "PADOVA";
			certifyingEntityAddress.OA_PostCode = "35127";
			certifyingEntityAddress.OA_RN_NKCountryCode = "IT";
			var pointOfContact = Factory.New<OrgHeader>();
			pointOfContact.OH_FullName = "EAST DIRECT COMPANY";
			pointOfContact.MainAddress.OA_Address1 = "MAIN ADDRESS";
			pointOfContactAddress = pointOfContact.Addresses.AddNew();
			pointOfContactAddress.OA_Address1 = "22/F, 3 LOCKHAKT RD.";
			pointOfContactAddress.OA_City = "WANCHHC";
			pointOfContactAddress.OA_PostCode = "22333";
			pointOfContactAddress.OA_RN_NKCountryCode = "HK";
		}
		OrgHeader manufacturer;
		OrgAddress manufacturerAddress;
		OrgAddress safetyTestLocationAddress;
		OrgAddress certifyingEntityAddress;
		OrgAddress pointOfContactAddress;
	}
}
