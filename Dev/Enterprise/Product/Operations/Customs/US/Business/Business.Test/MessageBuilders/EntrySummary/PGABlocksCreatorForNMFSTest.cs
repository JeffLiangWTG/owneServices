using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForNMFSTest : PGABlocksCreatorTest
	{
		[TestDate(2014, 11, 01)]
		public void TestNMFS370Data()
		{
			SetUpData();
			invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			nmfsLine.US_DolphinSafeStatus = DolphinSafeStatusList.Codes.B4;
			nmfsLine.US_CaptainStatement = true;
			nmfsLine.US_IDCPMemberCertification = true;
			nmfsLine.HarvestingDetails.RemoveAndDeleteAll();
			var harvestingDetail1 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail1.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail1.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail1.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail1.US_VesselCountry = "US";
			harvestingDetail1.US_ContainsYellowfinTuna = true;

			var harvestingDetail2 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail2.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail2.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail2.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail2.US_VesselCountry = "US";
			harvestingDetail2.US_ContainsYellowfinTuna = false;

			var harvestingDetail3 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail3.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail3.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail3.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail3.US_VesselCountry = "AU";
			harvestingDetail3.US_ContainsYellowfinTuna = false;

			nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			nmfsLine.US_DolphinSafeStatus = DolphinSafeStatusList.Codes.B2;
			nmfsLine.US_CaptainStatement = true;
			nmfsLine.US_IDCPMemberCertification = true;
			nmfsLine.US_ObserverStatement = true;
			nmfsLine.HarvestingDetails.RemoveAndDeleteAll();
			harvestingDetail1 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail1.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail1.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail1.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail1.US_VesselCountry = "US";
			harvestingDetail1.US_ContainsYellowfinTuna = true;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001NMF370YFT Y                                                              
PG02P                                                                           
PG142NM4                                                                        
PG50                                                                            
PG06HRVZZETP                                 LL                                 
PG10NM1   NOT                                                                   
PG22Y877    B4                                                                  
PG22Y897                                                                        
PG22Y899                                                                        
PG31VCRAU                                                                       
PG31VCRUS                                                                       
PG51                                                                            
PG50                                                                            
PG06HRVZZETP                                 LL                                 
PG10NM1   YFT          CONTAINS YELLOWFIN TUNA                                  
PG22Y877    B4                                                                  
PG22Y897                                                                        
PG22Y899                                                                        
PG31VCRUS                                                                       
PG51                                                                            
PG01002NMF370YFT Y                                                              
PG02P                                                                           
PG142NM4                                                                        
PG50                                                                            
PG06HRVZZETP                                 LL                                 
PG10NM1   YFT          CONTAINS YELLOWFIN TUNA                                  
PG22Y877    B2                                                                  
PG22Y897                                                                        
PG22Y898                                                                        
PG22Y899                                                                        
PG31VCRUS                                                                       
PG51                                                                            ", message.EM_FormattedMessageText);

			harvestingDetail1.US_ContainsYellowfinTuna = false;
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001NMF370YFT Y                                                              
PG02P                                                                           
PG142NM4                                                                        
PG50                                                                            
PG06HRVZZETP                                 LL                                 
PG10NM1   NOT                                                                   
PG22Y877    B4                                                                  
PG22Y897                                                                        
PG22Y899                                                                        
PG31VCRAU                                                                       
PG31VCRUS                                                                       
PG51                                                                            
PG50                                                                            
PG06HRVZZETP                                 LL                                 
PG10NM1   YFT          CONTAINS YELLOWFIN TUNA                                  
PG22Y877    B4                                                                  
PG22Y897                                                                        
PG22Y899                                                                        
PG31VCRUS                                                                       
PG51                                                                            
PG01002NMF370NOT Y                                                              
PG02P                                                                           
PG142NM4                                                                        
PG50                                                                            
PG06HRVZZETP                                 LL                                 
PG10NM1   NOT                                                                   
PG22Y877    B2                                                                  
PG22Y897                                                                        
PG22Y898                                                                        
PG22Y899                                                                        
PG31VCRUS                                                                       
PG51                                                                            ", message.EM_FormattedMessageText);
		}

		[TestDate(2014, 11, 01)]
		public void TestDisclaimNMFS0Data()
		{
			SetUpData();
			invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NMFS370DisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NMFSAMRDisclaimReason = PGADisclaimReasonList.Codes.B;
			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NMFSHMSDisclaimReason = PGADisclaimReasonList.Codes.A;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001NMF370                                                                  A
PG01002NMFAMR                                                                  B
PG01003NMFHMS                                                                  A", message.EM_FormattedMessageText);
		}

		[TestDate(2014, 11, 01)]
		public void TestNMFSAMRData()
		{
			SetUpData();
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsLine.US_AMLRPermitNumber = "AMR3242";
			nmfsLine.US_Commodity = FishStateList.Codes.FreshToothfish;
			nmfsLine.US_DocumentType = NMFSAMRDocumentIdentifierList.Codes.DissostichusCatchDocument;
			nmfsLine.US_DISDocumentID = "DIS23423";

			nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsLine.US_AMLRPermitNumber = "AMR3242";
			nmfsLine.US_Commodity = FishStateList.Codes.FreshToothfish;
			nmfsLine.US_DocumentType = NMFSAMRDocumentIdentifierList.Codes.ReportingFormForCatchDocumentsAccompanyingFresh;
			nmfsLine.US_DISDocumentID = "DIS093454";

			nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			nmfsLine.US_AMLRPermitNumber = "AMR3242";
			nmfsLine.US_PreApprovalIssuedNumber = "PAIN2342";
			nmfsLine.US_PreApprovalIssuedQuantity = 15.3223m;
			nmfsLine.US_PreApprovalIssuedQuantityUQ = Core.Constants.Weight.Tonnes;
			nmfsLine.US_DocumentType = NMFSAMRDocumentIdentifierList.Codes.ReportingFormForCatchDocumentsAccompanyingFresh;
			nmfsLine.US_DISDocumentID = "DIS093453";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001NMFAMRFREYY                                                              
PG02P                                                                           
PG10           FRE                                                              
PG142NM4                                                                        
PG22Y889                                          DIS23423                      
PG01002NMFAMRFREYY                                                              
PG02P                                                                           
PG10           FRE                                                              
PG142NM4                                                                        
PG22Y888                                          DIS093454                     
PG01003NMFAMRFRZYY                                                              
PG02P                                                                           
PG10           FRZ                                                              
PG142NM4                                                                        
PG141NM2PAIN2342                                  0000000153223000KG            ", message.EM_FormattedMessageText);
		}

		[TestDate(2014, 11, 01)]
		public void TestNMFSHMSData()
		{
			SetUpData();
			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			nmfsLine.US_HMSPermitNumber = "HMS324332";
			nmfsLine.US_DocumentType = NMFSHMSDocumentIdentifierList.Codes.CcsbtCatchMonitoringForm;
			nmfsLine.US_DISDocumentID = "DIS32342";
			nmfsLine.HarvestingDetails.RemoveAndDeleteAll();
			var harvestingDetail1 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail1.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail1.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail1.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail1.US_VesselCountry = "US";

			var harvestingDetail2 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail2.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail2.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail2.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail2.US_VesselCountry = "JP";

			nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			nmfsLine.US_HMSPermitNumber = "HMS324332";
			nmfsLine.US_DocumentType = NMFSHMSDocumentIdentifierList.Codes.CcsbtCatchMonitoringForm;
			nmfsLine.US_DISDocumentID = "DIS32349";
			nmfsLine.HarvestingDetails.RemoveAndDeleteAll();
			var harvestingDetail3 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail3.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail3.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.SAT;
			harvestingDetail3.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail3.US_VesselCountry = "BR";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001NMFHMS   YY                                                              
PG02P                                                                           
PG142NM4                                                                        
PG50                                                                            
PG06HRVZZETP                                 LL                                 
PG22Y886                                          DIS32342                      
PG31VCRJP                                                                       
PG31VCRUS                                                                       
PG51                                                                            
PG01002NMFHMS   YY                                                              
PG02P                                                                           
PG142NM4                                                                        
PG50                                                                            
PG06HRVZZSAT                                 LL                                 
PG22Y886                                          DIS32349                      
PG31VCRBR                                                                       
PG51                                                                            ", message.EM_FormattedMessageText);
		}

		[TestDate(2016, 09, 18)]
		public void TestNMFSHMSWithDocumentDetailsInLine()
		{
			SetUpData();
			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			nmfsLine.US_HMSPermitNumber = "HMS324332";
			nmfsLine.HarvestingDetails.RemoveAndDeleteAll();
			var harvestingDetail1 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail1.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail1.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail1.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail1.US_VesselCountry = "US";
			var documentDetail = nmfsLine.DocumentDetails.AddNew();
			documentDetail.CY_Code = "886";
			documentDetail.CY_Data = "DIS32342";

			var harvestingDetail2 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail2.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail2.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.CAR;
			harvestingDetail2.US_GearType = GearTypeList.Codes.Baitboat;
			harvestingDetail2.US_VesselCountry = "JP";
			var documentDetail2 = nmfsLine.DocumentDetails.AddNew();
			documentDetail2.CY_Code = "887";
			documentDetail2.CY_Data = "DIS123456";

			nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			nmfsLine.US_HMSPermitNumber = "HMS324332";
			nmfsLine.HarvestingDetails.RemoveAndDeleteAll();
			var harvestingDetail3 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail3.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail3.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.SAT;
			harvestingDetail3.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail3.US_VesselCountry = "BR";
			var documentDetail3 = nmfsLine.DocumentDetails.AddNew();
			documentDetail3.CY_Code = "886";
			documentDetail3.CY_Data = "DIS32349";
			var documentDetail4 = nmfsLine.DocumentDetails.AddNew();
			documentDetail4.CY_Code = "887";
			documentDetail4.CY_Data = "DIS32348";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001NMFHMS   YY                                                              
PG02P                                                                           
PG142NM4                                                                        
PG50                                                                            
PG06HRVZZCAR                                 BB                                 
PG22Y886                                          DIS32342                      
PG22Y887                                          DIS123456                     
PG31VCRJP                                                                       
PG51                                                                            
PG50                                                                            
PG06HRVZZETP                                 LL                                 
PG22Y886                                          DIS32342                      
PG22Y887                                          DIS123456                     
PG31VCRUS                                                                       
PG51                                                                            
PG01002NMFHMS   YY                                                              
PG02P                                                                           
PG142NM4                                                                        
PG50                                                                            
PG06HRVZZSAT                                 LL                                 
PG22Y886                                          DIS32349                      
PG22Y887                                          DIS32348                      
PG31VCRBR                                                                       
PG51                                                           ", message.EM_FormattedMessageText);
		}

		[TestDate(2016, 09, 18)]
		public void TestNMFSAMRWithDocumentDetailsInLine()
		{
			SetUpData();
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsLine.US_AMLRPermitNumber = "AMR3242";
			nmfsLine.US_Commodity = FishStateList.Codes.FreshToothfish;
			var documentDetail = nmfsLine.DocumentDetails.AddNew();
			documentDetail.CY_Code = "889";
			documentDetail.CY_Data = "DIS23423";

			nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsLine.US_AMLRPermitNumber = "AMR3242";
			nmfsLine.US_Commodity = FishStateList.Codes.FreshToothfish;
			var documentDetail2 = nmfsLine.DocumentDetails.AddNew();
			documentDetail2.CY_Code = "888";
			documentDetail2.CY_Data = "DIS093454";

			nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			nmfsLine.US_AMLRPermitNumber = "AMR3242";
			nmfsLine.US_PreApprovalIssuedNumber = "PAIN2342";
			nmfsLine.US_PreApprovalIssuedQuantity = 15.3223m;
			nmfsLine.US_PreApprovalIssuedQuantityUQ = Core.Constants.Weight.Tonnes;
			var documentDetail3 = nmfsLine.DocumentDetails.AddNew();
			documentDetail3.CY_Code = "888";
			documentDetail3.CY_Data = "DIS093453";
			var documentDetail4 = nmfsLine.DocumentDetails.AddNew();
			documentDetail4.CY_Code = "889";
			documentDetail4.CY_Data = "DIS093454";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001NMFAMRFREYY                                                              
PG02P                                                                           
PG10           FRE                                                              
PG142NM4                                                                        
PG22Y889                                          DIS23423                      
PG01002NMFAMRFREYY                                                              
PG02P                                                                           
PG10           FRE                                                              
PG142NM4                                                                        
PG22Y888                                          DIS093454                     
PG01003NMFAMRFRZYY                                                              
PG02P                                                                           
PG10           FRZ                                                              
PG142NM4                                                                        
PG141NM2PAIN2342                                  0000000153223000KG            ", message.EM_FormattedMessageText);
		}

		[TestDate(2017, 11, 01)]
		public void TestNMFSSIMPHCFData()
		{
			SetUpData();
			invoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			nmfsLine.US_SourceType = SourceTypeCodesList.Codes.HarvestOfCaptureFisheries;
			nmfsLine.US_SpeciesCode = "APW";
			nmfsLine.US_Confidential = true;
			nmfsLine.US_IFTPPermitNumber = "2222222";
			nmfsLine.HarvestingDetails.RemoveAndDeleteAll();

			var harvestingDetail = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail.US_HarvestedCountry = "US";
			harvestingDetail.US_OceanAreaOfCatch = "A";
			harvestingDetail.US_GearStartDate = ZDateTime.Today;
			harvestingDetail.US_ContactPartyType = EntityRoleCodeList.Codes.AquacultureFacility;
			harvestingDetail.US_OA_ContactParty = manufacturerAddress.PK;

			var harvestingVessel = harvestingDetail.HarvestingVessles.AddNew();
			harvestingVessel.US_HarvestedVessel = "AA";
			harvestingVessel.US_HarvestedCountry = "US";
			harvestingVessel.US_NetWeight = 100m;
			harvestingVessel.US_NetWeightUQ = "KG";
			harvestingVessel.US_FirstLandingCountry = "CA";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
				@"PG01001NMFSIM    Y                                                              
PG02P                                                                           
PG05                                                              APW           
PG06HCFUSA                   11012017                                           
PG142NM42222222                                                                 
PG19AQF                  TEST ATF                        1234 PEACHTREE STREET  
PG20                                     ATLANTA              GA US30301        
PG21AQFJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG50                                                                            
PG31VCRUS                                 KG 0000010000                         
PG31VNMAA                                                                       
PG3211 CA                                                                       ", message.EM_FormattedMessageText);
		}

		[TestDate(2017, 11, 01)]
		public void TestNMFSSIMPHBAData()
		{
			SetUpData();
			invoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			nmfsLine.US_SourceType = SourceTypeCodesList.Codes.HatcheryBasedAquaculture;
			nmfsLine.US_SpeciesCode = "APW";
			nmfsLine.US_Confidential = true;
			nmfsLine.US_OtherAuthorizationNumber = "1111111111";
			nmfsLine.US_AuthorizationType = "1";
			nmfsLine.US_IFTPPermitNumber = "2222222";
			nmfsLine.US_NetWeight = 100m;
			nmfsLine.US_NetWeightUQ = "KG";
			nmfsLine.HarvestingDetails.RemoveAndDeleteAll();

			var harvestingDetail = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail.US_HarvestedCountry = "US";
			harvestingDetail.US_GearStartDate = ZDateTime.Today;
			harvestingDetail.US_ContactPartyType = EntityRoleCodeList.Codes.AquacultureFacility;
			harvestingDetail.US_OA_ContactParty = manufacturerAddress.PK;
			harvestingDetail.US_GeographicLocation = "TEST LOCATION";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
				@"OI        ABCDEFG DESC                                                          
PG01001NMFSIM    Y                                                              
PG02P                                                                           
PG05                                                              APW           
PG06HBAUSTEST LOCATION       11012017                                           
PG142NM42222222                                                                 
PG141NM61111111111                                                              
PG19AQF                  TEST ATF                        1234 PEACHTREE STREET  
PG20                                     ATLANTA              GA US30301        
PG21AQFJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG29KG 000000010000                                                             ", message.EM_FormattedMessageText);
		}

		[TestDate(2017, 11, 01)]
		public void TestNMFSSIMPSVHData()
		{
			SetUpData();
			invoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			nmfsLine.US_SourceType = SourceTypeCodesList.Codes.SmallVesselHarvest;
			nmfsLine.US_SpeciesCode = "APW";
			nmfsLine.US_Confidential = true;
			nmfsLine.US_OtherAuthorizationNumber = "1111111111";
			nmfsLine.US_AuthorizationType = "2";
			nmfsLine.US_IFTPPermitNumber = "2222222";
			nmfsLine.US_NetWeight = 100m;
			nmfsLine.US_NetWeightUQ = "KG";
			nmfsLine.HarvestingDetails.RemoveAndDeleteAll();

			var harvestingDetail = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail.US_HarvestedCountry = "US";
			harvestingDetail.US_GearStartDate = ZDateTime.Today;
			harvestingDetail.US_ContactPartyType = EntityRoleCodeList.Codes.AquacultureFacility;
			harvestingDetail.US_OA_ContactParty = manufacturerAddress.PK;
			harvestingDetail.US_OceanAreaOfCatch = "A";
			harvestingDetail.US_NoSmallVessels = 120;
			harvestingDetail.US_FirstLandingCountry = "ZZ";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
				@"OI        ABCDEFG DESC                                                          
PG01001NMFSIM    Y                                                              
PG02P                                                                           
PG05                                                              APW           
PG06SVHUSA                   11012017                                           
PG142NM42222222                                                                 
PG142NM61111111111                                                              
PG19AQF                  TEST ATF                        1234 PEACHTREE STREET  
PG20                                     ATLANTA              GA US30301        
PG21AQFJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG24        120                                                                 
PG29KG 000000010000                                                             
PG3211 ZZ                                                                       ", message.EM_FormattedMessageText);
		}

		[TestDate(2019, 02, 22)]
		public void TestNMFSAMRWithFishStateListType()
		{
			SetUpData();
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsLine.US_AMLRPermitNumber = "AMR3242";
			nmfsLine.US_Commodity = FishStateList.Codes.FreshKrill;
			var documentDetail = nmfsLine.DocumentDetails.AddNew();
			documentDetail.CY_Code = "889";
			documentDetail.CY_Data = "DIS23423";

			nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsLine.US_AMLRPermitNumber = "AMR3242";
			nmfsLine.US_Commodity = FishStateList.Codes.FrozenKrill;
			var documentDetail2 = nmfsLine.DocumentDetails.AddNew();
			documentDetail2.CY_Code = "888";
			documentDetail2.CY_Data = "DIS093454";

			nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			nmfsLine.US_AMLRPermitNumber = "AMR3242";
			nmfsLine.US_PreApprovalIssuedNumber = "PAIN2342";
			nmfsLine.US_PreApprovalIssuedQuantity = 15.3223m;
			nmfsLine.US_PreApprovalIssuedQuantityUQ = Core.Constants.Weight.Tonnes;
			var documentDetail3 = nmfsLine.DocumentDetails.AddNew();
			documentDetail3.CY_Code = "888";
			documentDetail3.CY_Data = "DIS093453";
			var documentDetail4 = nmfsLine.DocumentDetails.AddNew();
			documentDetail4.CY_Code = "889";
			documentDetail4.CY_Data = "DIS093454";

			nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsLine.US_AMLRPermitNumber = "AMR3242";
			nmfsLine.US_Commodity = FishStateList.Codes.FreshToothfish;
			var documentDetail5 = nmfsLine.DocumentDetails.AddNew();
			documentDetail5.CY_Code = "888";
			documentDetail5.CY_Data = "DIS093454";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001NMFAMRFREYY                                                              
PG02P                                                                           
PG10           FRE                                                              
PG142NM4                                                                        
PG22Y889                                          DIS23423                      
PG01002NMFAMRFRZYY                                                              
PG02P                                                                           
PG10           FRZ                                                              
PG142NM4                                                                        
PG22Y888                                          DIS093454                     
PG01003NMFAMRFRZYY                                                              
PG02P                                                                           
PG10           FRZ                                                              
PG142NM4                                                                        
PG141NM2PAIN2342                                  0000000153223000KG            
PG01004NMFAMRFREYY                                                              
PG02P                                                                           
PG10           FRE                                                              
PG142NM4                                                                        
PG22Y888                                          DIS093454                     ", message.EM_FormattedMessageText);
		}

		public void TestNMFSCOAData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, RefCusConditionTypes.ConditionClass.Control, TariffConditionTypes.Codes.PGA);
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, TariffConditionValueTypes.Codes.PGA);
			Factory.Save();
			var zzTariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "1010101010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, zzTariff.PK, "test", true, false, ZDateTime.BrettsBirthday, ZDateTime.Today);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition.PK, GovernmentAgencyProgramCodeList.Codes.COA);
			Factory.Save();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;

			SetUpData();

			invoiceLine.JI_Tariff = "1010101010";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.US_NMFSCOAInd = OGAIndicatorList.Codes.Declared;

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.COA;
			nmfsLine.US_SourceType = SourceTypeCodesList.Codes.HarvestOfCaptureFisheries;
			nmfsLine.US_SpeciesCode = "ADD";
			nmfsLine.US_Confidential = true;
			nmfsLine.US_LineNo = 1;
			nmfsLine.US_IFTPPermitNumber = "123456789";
			nmfsLine.HarvestingDetails.RemoveAndDeleteAll();

			var harvestingDetail = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail.US_HarvestedCountry = "AU";
			harvestingDetail.US_GearStartDate = new ZDateTime(2023, 02, 15);
			harvestingDetail.US_OceanAreaOfCatch = "CAR";
			harvestingDetail.US_GearType = "GIL";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
				@"OI        ABCDEFG DESC                                                          
PG01001NMFCOA    Y                                                              
PG02P                                                                           
PG05                                                              ADD           
PG06HCFAUCAR                 02152023        GIL                                
PG142NM4123456789                                                               
PG22Y894            COA1                                                        ", message.EM_FormattedMessageText);
		}

		protected override void SetUpData()
		{
			base.SetUpData();

			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.US_CertifyCargoRelease = true;

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "TV";
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			broker.GS_FullName = "BROKER";
			broker.GS_WorkPhone = "04 010101";
			broker.GS_EmailAddress = "BROKER EMAIL";
			broker.GS_FaxNum = "BROKER FAX";

			var ior = Factory.New<OrgHeader>();
			declaration.IOROrgPK = ior.PK;
			ior.OH_FullName = "IMPORTER OF RECORD";
			var iorAddress = ior.MainAddress;
			iorAddress.OA_Address1 = "IOR ADDRESS 1";
			iorAddress.OA_Address2 = "IOR ADDRESS 2";
			iorAddress.OA_City = "SYDNEY";
			iorAddress.OA_State = "NSW";
			iorAddress.OA_PostCode = "2017";
			DeclarationTestHelper.AddPGAContact(ior, "IOR", "ALEXANDER THE GREATEST OF ALL", "04 123456", "IOR EMAIL", "IOR FAX");
			invoiceLine.JI_OA_ExporterAddress = ior.MainAddress.PK;
			invoiceLine.JI_OA_ManufacturerAddress = ior.MainAddress.PK;
			invoiceLine.JI_Description = "ABCDEFG DESC";
			nmfsLine = invoiceLine.NMFSLines.AddNew();

			var manufacturer = Factory.New<OrgHeader>();
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
		}
		NMFSLine nmfsLine;
		OrgAddress manufacturerAddress;
	}
}
