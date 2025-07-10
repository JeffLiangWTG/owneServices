using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForNHTSATest : PGABlocksCreatorTest
	{
		public void TestBox1VehicleDeclared()
		{
			SetUpData();

			invoiceLine.JI_Description = "LAND ROVER VEHICLE";
			header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._01;
			header.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			detailsLine.US_NHTBrandName = "LAND ROVER";
			detailsLine.US_NHTModel = "DEFENDER 110";
			detailsLine.US_NHTYearOfMFR = "1983";
			detailsLine.US_NHTMonthOfMFR = MonthList.Codes._12;
			detailsLine.US_NHTCategoryCode = NHTSACategoryCode_MVSTYPList.Codes.MVS1;
			detailsLine.US_NHTModelYear = "1983";
			detailsLine.US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			detailsLine.US_NHTIdentityNumber = "SALLDHMV2AA100000";
			document.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.Importer;

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_NHTSASignDate = new ZDateTime(2015, 05, 07);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var expectedResult = @"OI        LAND ROVER VEHICLE                                                    
PG01001NHTMVS                                                                   
PG02P                                                                           
PG07LAND ROVER                         DEFENDER 110   121983AKGSALLDHMV2AA100000
PG10MVSTYPMVS1 V06 1983                                                         
PG19CN                   ULTIMATE CONSIGNEE              CNE ADDRESS 1          
PG20CNE ADDRESS 2                        CHARLESTON           IL US29492        
PG21CN                                                                          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG22Y946    1    IM NH1 Y05072015                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CI                                                                          ";
			AssertContains(expectedResult, message.EM_FormattedMessageText);
		}

		public void TestBox2AAutomotiveGlazingDeclared()
		{
			SetUpData();

			invoiceLine.JI_Description = "AUTO WINDSHIELDS";
			header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.REI;
			header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._2A;
			header.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			detailsLine.US_NHTBrandName = "LUNG TA CLASS INDUSTRIAL";
			detailsLine.US_NHTCategoryCode = NHTSACategoryCode_REITYPList.Codes.REI7;

			var fmOrg = Factory.New<OrgHeader>();
			header.US_NHTFabricatingMFROrgPK = fmOrg.PK;
			fmOrg.OH_FullName = "FABRICATING MANUFACTURER";
			var fmOrgAddress = fmOrg.MainAddress;
			fmOrgAddress.OA_Address1 = "FM ADDRESS 1";
			fmOrgAddress.OA_City = "SHANGHAI";
			fmOrgAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.GlazingManufacturerCode, "0568");
			DeclarationTestHelper.AddPGAContact(fmOrg, "FM", "INC TEST", null, "INC TEST EMAIL", null);

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_NHTSASignDate = new ZDateTime(2015, 05, 07);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        AUTO WINDSHIELDS                                                      
PG01001NHTREI                                                                   
PG02P                                                                           
PG07LUNG TA CLASS INDUSTRIAL                                                    
PG10REITYPREI7                                                                  
PG19CN                   ULTIMATE CONSIGNEE              CNE ADDRESS 1          
PG20CNE ADDRESS 2                        CHARLESTON           IL US29492        
PG21CN                                                                          
PG19FM GMC0568                                                                  
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG22Y946    2A   CI NH1 Y05072015                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CI                                                                          ", message.EM_FormattedMessageText);
		}

		public void TestBox2AMultipleMotorVehicleModelsDeclared()
		{
			SetUpData();

			invoiceLine.JI_Description = "TOYOTA VEHICLES";
			header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._2A;
			header.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;

			var detailsLineOne = detailsLine;
			detailsLineOne.US_NHTBrandName = "TOYOTA";
			detailsLineOne.US_NHTModel = "RAV4";
			detailsLineOne.US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			detailsLineOne.US_NHTIdentityNumber = "JTMBK35V395075946";
			detailsLineOne.US_NHTCategoryCode = NHTSACategoryCode_MVSTYPList.Codes.MVS2;
			detailsLineOne.US_NHTModelYear = "2013";
			var additionalNumberOneForLineOne = detailsLineOne.AdditionalNumbers.AddNew();
			additionalNumberOneForLineOne.US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			additionalNumberOneForLineOne.US_NHTAdditionalIdentityNumber = "JTMBK35V395076322";
			var additionalNumberTwoForLineOne = detailsLineOne.AdditionalNumbers.AddNew();
			additionalNumberTwoForLineOne.US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			additionalNumberTwoForLineOne.US_NHTAdditionalIdentityNumber = "JTMBK35V395076661";
			var additionalNumberThreeForLineOne = detailsLineOne.AdditionalNumbers.AddNew();
			additionalNumberThreeForLineOne.US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			additionalNumberThreeForLineOne.US_NHTAdditionalIdentityNumber = "JTMBK35V395075065";
			var additionalNumberFourForLineOne = detailsLineOne.AdditionalNumbers.AddNew();
			additionalNumberFourForLineOne.US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			additionalNumberFourForLineOne.US_NHTAdditionalIdentityNumber = "JTMBK35V395075454";
			var additionalNumberFiveForLineOne = detailsLineOne.AdditionalNumbers.AddNew();
			additionalNumberFiveForLineOne.US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			additionalNumberFiveForLineOne.US_NHTAdditionalIdentityNumber = "JTMBK35V395076006";

			var detailsLineTwo = header.NHTSADetails.AddNew();
			detailsLineTwo.US_NHTBrandName = "TOYOTA";
			detailsLineTwo.US_NHTModel = "PRIUS";
			detailsLineTwo.US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			detailsLineTwo.US_NHTIdentityNumber = "JTMKB22U940070052";
			detailsLineTwo.US_NHTCategoryCode = NHTSACategoryCode_MVSTYPList.Codes.MVS1;
			detailsLineTwo.US_NHTModelYear = "2014";
			var additionalNumberOneForLineTwo = detailsLineTwo.AdditionalNumbers.AddNew();
			additionalNumberOneForLineTwo.US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			additionalNumberOneForLineTwo.US_NHTAdditionalIdentityNumber = "JTMKB22U940070066";
			var additionalNumberTwoForLineTwo = detailsLineTwo.AdditionalNumbers.AddNew();
			additionalNumberTwoForLineTwo.US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			additionalNumberTwoForLineTwo.US_NHTAdditionalIdentityNumber = "JTMKB22U940070410";
			var additionalNumberThreeForLineTwo = detailsLineTwo.AdditionalNumbers.AddNew();
			additionalNumberThreeForLineTwo.US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.SerialNumber;
			additionalNumberThreeForLineTwo.US_NHTAdditionalIdentityNumber = "JTMKB22U940072206";

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_NHTSASignDate = new ZDateTime(2015, 05, 07);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        TOYOTA VEHICLES                                                       
PG01001NHTMVS                                                                   
PG02P                                                                           
PG50                                                                            
PG07TOYOTA                             RAV4                 AKGJTMBK35V395075946
PG08JTMBK35V395076322JTMBK35V395076661JTMBK35V395075065                         
PG10MVSTYPMVS2 V06 2013                                                         
PG51                                                                            
PG50                                                                            
PG07TOYOTA                             RAV4                 ENNJTMBK35V395075454
PG08JTMBK35V395076006                                                           
PG10MVSTYPMVS2 V06 2013                                                         
PG51                                                                            
PG50                                                                            
PG07TOYOTA                             PRIUS                AKGJTMKB22U940070052
PG08JTMKB22U940070066JTMKB22U940070410                                          
PG10MVSTYPMVS1 V06 2014                                                         
PG51                                                                            
PG50                                                                            
PG07TOYOTA                             PRIUS                SE JTMKB22U940072206
PG10MVSTYPMVS1 V06 2014                                                         
PG51                                                                            
PG19CN                   ULTIMATE CONSIGNEE              CNE ADDRESS 1          
PG20CNE ADDRESS 2                        CHARLESTON           IL US29492        
PG21CN                                                                          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG22Y946    2A   CI NH1 Y05072015                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CI                                                                          ", message.EM_FormattedMessageText);
		}

		public void TestBox3VehicleDeclared()
		{
			SetUpData();

			invoiceLine.JI_Description = "FREEARI VEHICLE";
			header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._03;
			header.US_NHTElectronicImage = true;
			header.US_NHTDOTSuretyCode = "421";
			header.US_NHTDOTBondNumber = "011-123456";
			header.US_NHTDOTBondType = DOTBondQualifierList.Codes.Single;
			header.US_NHTDOTBondAmount = 1250000;
			header.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;

			detailsLine.US_NHTBrandName = "FERRARI";
			detailsLine.US_NHTModel = "TESTAROSSA";
			detailsLine.US_NHTYearOfMFR = "1989";
			detailsLine.US_NHTMonthOfMFR = MonthList.Codes._06;
			detailsLine.US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			detailsLine.US_NHTIdentityNumber = "ZFFVA40B00000000";
			detailsLine.US_NHTCategoryCode = NHTSACategoryCode_MVSTYPList.Codes.MVS1;
			detailsLine.US_NHTModelYear = "1989";
			detailsLine.US_NHTDriveSide = DriverSideList.Codes.Left;
			detailsLine.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH0;
			detailsLine.US_NHTLPCONumber = "R-90-007";

			var additionalLPCO = detailsLine.PermitAndLicenses.AddNew();
			additionalLPCO.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH3;
			additionalLPCO.US_NHTLPCONumber = "VSA-039";
			var additionalDocOne = header.NHTSADocuments.AddNew();
			additionalDocOne.US_NHTDocumentType = NHTSADocumentTypeList.Codes._873;
			additionalDocOne.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.CertifyingIndividual;
			var additionalDocTwo = header.NHTSADocuments.AddNew();
			additionalDocTwo.US_NHTDocumentType = NHTSADocumentTypeList.Codes._165;
			additionalDocTwo.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.CertifyingIndividual;

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_NHTSASignDate = new ZDateTime(2015, 05, 07);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        FREEARI VEHICLE                                                       
PG01001NHTMVS   Y                                                               
PG02P                                                                           
PG07FERRARI                            TESTAROSSA     061989AKGZFFVA40B00000000 
PG10MVSTYPMVS1 V01 L                                                            
PG10MVSTYPMVS1 V06 1989                                                         
PG142NH0R-90-007                                                                
PG142NH3VSA-039                                                                 
PG19CN                   ULTIMATE CONSIGNEE              CNE ADDRESS 1          
PG20CNE ADDRESS 2                        CHARLESTON           IL US29492        
PG21CN                                                                          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG22Y946    3    CI NH1 Y05072015                                               
PG22Y873         CI NH1 Y05072015                                               
PG22Y165         CI NH1 Y05072015                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CI                                                                          
PG35421011-123456                    101250000                                  ", message.EM_FormattedMessageText);
		}

		public void TestSendMessageWhenIndicatorIsDisclaim()
		{
			SetUpData();

			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.JI_Description = "LAND ROVER VEHICLE";
			header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._01;
			header.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			detailsLine.US_NHTBrandName = "LAND ROVER";
			detailsLine.US_NHTModel = "DEFENDER 110";
			detailsLine.US_NHTYearOfMFR = "1983";
			detailsLine.US_NHTMonthOfMFR = MonthList.Codes._12;
			detailsLine.US_NHTCategoryCode = NHTSACategoryCode_MVSTYPList.Codes.MVS1;
			detailsLine.US_NHTModelYear = "1983";
			detailsLine.US_NHTIdentityNumber = "SALLDHMV2AA100000";
			document.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.Importer;

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_NHTSASignDate = new ZDateTime(2015, 05, 07);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        LAND ROVER VEHICLE                                                    
PG01001NHTOFF                                                                   ", message.EM_FormattedMessageText);
		}

		public void TestCertifyingIndividualIsOWN()
		{
			SetUpData();
			var owner = Factory.New<OrgHeader>();
			header.US_OA_NHTOwner = owner.MainAddress.PK;
			owner.OH_FullName = "OWNER";
			var ownerAddress = owner.MainAddress;
			ownerAddress.OA_Address1 = "OWN ADDRESS 1";
			ownerAddress.OA_Address2 = "OWN ADDRESS 2";
			ownerAddress.OA_City = "CHARLESTON";
			ownerAddress.OA_RL_NKRelatedPortCode = "US2CW";
			ownerAddress.OA_State = "IL";
			ownerAddress.OA_PostCode = "29492";
			DeclarationTestHelper.AddPGAContact(ownerAddress, "OWNER", "TEST", "12345678", "owner@ian.com", null);

			invoiceLine.JI_Description = "LAND ROVER VEHICLE";
			header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._01;
			header.US_CertifyingIndividual = PartyTypeList.Codes.Owner;
			detailsLine.US_NHTBrandName = "LAND ROVER";
			detailsLine.US_NHTModel = "DEFENDER 110";
			detailsLine.US_NHTYearOfMFR = "1983";
			detailsLine.US_NHTMonthOfMFR = MonthList.Codes._12;
			detailsLine.US_NHTCategoryCode = NHTSACategoryCode_MVSTYPList.Codes.MVS1;
			detailsLine.US_NHTModelYear = "1983";
			detailsLine.US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			detailsLine.US_NHTIdentityNumber = "SALLDHMV2AA100000";
			document.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.Importer;

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_NHTSASignDate = new ZDateTime(2015, 05, 07);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var expectedResult = @"OI        LAND ROVER VEHICLE                                                    
PG01001NHTMVS                                                                   
PG02P                                                                           
PG07LAND ROVER                         DEFENDER 110   121983AKGSALLDHMV2AA100000
PG10MVSTYPMVS1 V06 1983                                                         
PG19CN                   ULTIMATE CONSIGNEE              CNE ADDRESS 1          
PG20CNE ADDRESS 2                        CHARLESTON           IL US29492        
PG21CN                                                                          
PG19DFP                  OWNER                           OWN ADDRESS 1          
PG20OWN ADDRESS 2                        CHARLESTON           IL US29492        
PG21DFPOWNER TEST             12345678       OWNER@IAN.COM                      
PG55CI                                                                          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG22Y946    1    IM NH1 Y05072015                                               ";
			AssertContains(expectedResult, message.EM_FormattedMessageText);
		}

		public void TestCertifyingIndividualIsIMP()
		{
			SetUpData();
			var owner = Factory.New<OrgHeader>();
			header.US_OA_NHTOwner = owner.MainAddress.PK;
			owner.OH_FullName = "OWNER";
			var ownerAddress = owner.MainAddress;
			ownerAddress.OA_Address1 = "OWN ADDRESS 1";
			ownerAddress.OA_Address2 = "OWN ADDRESS 2";
			ownerAddress.OA_City = "CHARLESTON";
			ownerAddress.OA_RL_NKRelatedPortCode = "US2CW";
			ownerAddress.OA_State = "IL";
			ownerAddress.OA_PostCode = "29492";

			invoiceLine.JI_Description = "LAND ROVER VEHICLE";
			header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._01;
			header.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			detailsLine.US_NHTBrandName = "LAND ROVER";
			detailsLine.US_NHTModel = "DEFENDER 110";
			detailsLine.US_NHTYearOfMFR = "1983";
			detailsLine.US_NHTMonthOfMFR = MonthList.Codes._12;
			detailsLine.US_NHTCategoryCode = NHTSACategoryCode_MVSTYPList.Codes.MVS1;
			detailsLine.US_NHTModelYear = "1983";
			detailsLine.US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			detailsLine.US_NHTIdentityNumber = "SALLDHMV2AA100000";
			document.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.Importer;

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_NHTSASignDate = new ZDateTime(2015, 05, 07);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var expectedResult = @"OI        LAND ROVER VEHICLE                                                    
PG01001NHTMVS                                                                   
PG02P                                                                           
PG07LAND ROVER                         DEFENDER 110   121983AKGSALLDHMV2AA100000
PG10MVSTYPMVS1 V06 1983                                                         
PG19CN                   ULTIMATE CONSIGNEE              CNE ADDRESS 1          
PG20CNE ADDRESS 2                        CHARLESTON           IL US29492        
PG21CN                                                                          
PG19DFP                  OWNER                           OWN ADDRESS 1          
PG20OWN ADDRESS 2                        CHARLESTON           IL US29492        
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG55CI                                                                          
PG22Y946    1    IM NH1 Y05072015                                               ";
			AssertContains(expectedResult, message.EM_FormattedMessageText);
		}

		public void TestMultipleDocumentsEntered()
		{
			SetUpData();

			invoiceLine.JI_Description = "LAND ROVER VEHICLE";
			header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._01;
			header.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			detailsLine.US_NHTBrandName = "LAND ROVER";
			detailsLine.US_NHTModel = "DEFENDER 110";
			detailsLine.US_NHTYearOfMFR = "1983";
			detailsLine.US_NHTMonthOfMFR = MonthList.Codes._12;
			detailsLine.US_NHTCategoryCode = NHTSACategoryCode_MVSTYPList.Codes.MVS1;
			detailsLine.US_NHTModelYear = "1983";
			detailsLine.US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			detailsLine.US_NHTIdentityNumber = "SALLDHMV2AA100000";

			header.NHTSADocuments.RemoveAndDeleteAll();

			var document1 = header.NHTSADocuments.AddNew();
			document1.US_NHTDocumentType = NHTSADocumentTypeList.Codes._871;
			document1.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.OriginalVehicleManufacturer;

			var document2 = header.NHTSADocuments.AddNew();
			document2.US_NHTDocumentType = NHTSADocumentTypeList.Codes._946;
			document2.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.Importer;

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_NHTSASignDate = new ZDateTime(2015, 05, 07);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var expectedResult = @"OI        LAND ROVER VEHICLE                                                    
PG01001NHTMVS                                                                   
PG02P                                                                           
PG07LAND ROVER                         DEFENDER 110   121983AKGSALLDHMV2AA100000
PG10MVSTYPMVS1 V06 1983                                                         
PG19CN                   ULTIMATE CONSIGNEE              CNE ADDRESS 1          
PG20CNE ADDRESS 2                        CHARLESTON           IL US29492        
PG21CN                                                                          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG22Y871         OVMNH1 Y05072015                                               
PG22Y946    1    IM NH1 Y05072015                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         ";

			AssertContains(expectedResult, message.EM_FormattedMessageText);
		}

		public void TestCertifyingDateIsSetToCurrentDateIfEmpty()
		{
			SetUpData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "ACE";
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = "SE";

			invoiceLine.InvoiceHeader.US_NHTSASignDate = ZDateTime.Empty;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var today = ZDate.Today;
			AssertContains("PG22Y946         CI NH1 Y" + today.ToString("MMddyyyy"), message.EM_FormattedMessageText); //11212016

			invoiceLine.InvoiceHeader.US_NHTSASignDate = new ZDateTime(2016, 11, 29);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			action = GetAction(declaration);
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			AssertContains("PG22Y946         CI NH1 Y11292016", message.EM_FormattedMessageText);
		}

		public void TestNoPG21RecordIfPGAContactInfoNotExistForAnyOrg()
		{
			SetUpData();
			var owner = Factory.New<OrgHeader>();
			var ownerAddress = owner.MainAddress;
			DeclarationTestHelper.AddPGAContact(ownerAddress, "OWNER", "TEST", "12345678", "owner@gmail.com", null);

			header.US_OA_NHTOwner = owner.MainAddress.PK;
			header.US_NHTFabricatingMFRAddress = owner.MainAddress.PK;
			header.US_NHTOriginalMFRAddress = owner.MainAddress.PK;
			header.US_OA_NHTRetailer = owner.MainAddress.PK;

			header.US_CertifyingIndividual = "IM";
			header.US_PGAContactName = "OWNER";
			header.US_PGAContactPhoneNo = "12345678";
			header.US_PGAContactEmail = "owner@gmail.com";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains("PG21DFPOWNER TEST             12345678       OWNER@GMAIL.COM", message.EM_FormattedMessageText);
			AssertContains("PG21FM OWNER TEST             12345678       OWNER@GMAIL.COM", message.EM_FormattedMessageText);
			AssertContains("PG21RD OWNER TEST             12345678       OWNER@GMAIL.COM", message.EM_FormattedMessageText);
			AssertContains("PG21OVMOWNER TEST             12345678       OWNER@GMAIL.COM", message.EM_FormattedMessageText);

			DeclarationTestHelper.AddPGAContact(ownerAddress, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null);

			header.US_CertifyingIndividual = "IM";
			header.US_PGAContactName = ZString.Empty;
			header.US_PGAContactPhoneNo = ZString.Empty;
			header.US_PGAContactEmail = ZString.Empty;
			action = GetAction(declaration);
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			AssertNotContains("PG21DFP", message.EM_FormattedMessageText);
			AssertNotContains("PG21FM", message.EM_FormattedMessageText);
			AssertNotContains("PG21RD", message.EM_FormattedMessageText);
			AssertNotContains("PG21OVM", message.EM_FormattedMessageText);
		}

		protected override void SetUpData()
		{
			base.SetUpData();

			GlbStaff.CurrentUser.GS_Code = "INC";
			GlbStaff.CurrentUser.GS_FullName = "INC TEST";
			GlbStaff.CurrentUser.GS_WorkPhone = "04 010101";
			GlbStaff.CurrentUser.GS_EmailAddress = "INC TEST EMAIL";

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
			DeclarationTestHelper.AddPGAContact(ior, "IOR", "", "04 123456", "IOR EMAIL", null);

			var cne = Factory.New<OrgHeader>();
			invoiceLine.JI_OA_ConsigneeAddress = cne.MainAddress.PK;
			cne.OH_FullName = "ULTIMATE CONSIGNEE";
			var cneAddress = cne.MainAddress;
			cneAddress.OA_Address1 = "CNE ADDRESS 1";
			cneAddress.OA_Address2 = "CNE ADDRESS 2";
			cneAddress.OA_City = "CHARLESTON";
			cneAddress.OA_RL_NKRelatedPortCode = "US2CW";
			cneAddress.OA_State = "IL";
			cneAddress.OA_PostCode = "29492";

			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			header = invoiceLine.NHTSALines.AddNew();
			detailsLine = header.NHTSADetails.AddNew();
			document = header.NHTSADocuments.OfType<NHTSADocument>().FirstOrDefault();
		}
		NHTSAHeader header;
		NHTSADetails detailsLine;
		NHTSADocument document;
	}
}
