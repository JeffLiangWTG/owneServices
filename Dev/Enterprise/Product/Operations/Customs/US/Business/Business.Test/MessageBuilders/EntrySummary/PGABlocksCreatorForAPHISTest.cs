using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.APHIS.ArticleCategory;
using Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using FruitsAndVegetablesList = Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier.FruitsAndVegetablesList;
using SeedsNotForPlantingList = Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier.SeedsNotForPlantingList;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForAPHISTest : PGABlocksCreatorTest
	{
		public void TestDisclaim()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "2402106000";
			invoiceLine.JI_Description = "CATTLE";
			invoiceLine.APHISHeaders.RemoveAndDeleteAll();
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_APHISDisclaimReason = PGADisclaimReasonList.Codes.B;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000 000000000000K  000000000000KG                
OI        CATTLE                                                                
PG01001APHAVS                                                                  B
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestSendAPHISCropGrowerDetails()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.APHISAssignedNumber, "23GGFRD234");
			orgHeader.OH_Code = "KNZ";
			orgHeader.MainAddress.Address1 = "TST1";
			Factory.Save();

			SetUpData();
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForConsumerUseNonFoodProduct;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			header.US_CategoryCode = CutFlowersAndGreeneryList.Codes.CutFlowers;
			header.US_OA_CropGrowerAddress = orgHeader.MainAddress.PK;
			header.US_OA_ApplicantAddress = orgHeader.MainAddress.PK;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var expectedMessage = @"PG19DFI33323GGFRD234                                     TST1                   ";
			AssertContains("ACE Entry Summary message including APHIS Crop Grower Details data", expectedMessage, message.EM_FormattedMessageText);
			AssertContains("ACE Entry Summary message including APHIS Applicant Details data", "PG19LAP33323GGFRD234                                     TST1                   ", message.EM_FormattedMessageText);

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "123");
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			expectedMessage = @"PG19DFIMID123                                            TST1                   ";
			AssertContains("ACE Entry Summary message including APHIS Crop Grower Details data", expectedMessage, message.EM_FormattedMessageText);

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "789");
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			expectedMessage = @"PG19DFI16 789                                            TST1                   ";
			AssertContains("ACE Entry Summary message including APHIS Crop Grower Details data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestSendAPHISUltimateCosignee()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567");
			orgHeader.OH_Code = "KNZ";
			orgHeader.MainAddress.Address1 = "TST2";
			Factory.Save();

			SetUpData();

			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForConsumerUseNonFoodProduct;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			header.US_CategoryCode = CutFlowersAndGreeneryList.Codes.CutFlowers;
			header.Parent.JI_OA_ConsigneeAddress = orgHeader.MainAddress.PK;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var expectedMessage = @"PG19UC 34812-1234567                                     TST2                   ";
			AssertContains("ACE Entry Summary message including APHIS Ultimate Cosignee data", expectedMessage, message.EM_FormattedMessageText);

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "123456");
			action = GetAction(declaration);
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			expectedMessage = @"PG19UC 336123456                                         TST2                   ";
			AssertContains("ACE Entry Summary message including APHIS Ultimate Cosignee data", expectedMessage, message.EM_FormattedMessageText);

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "789");
			action = GetAction(declaration);
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			expectedMessage = @"PG19UC 370789                                            TST2                   ";
			AssertContains("ACE Entry Summary message including APHIS Ultimate Cosignee data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestEntityIdentificationCodeEmptyWhenNoCode()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.APHISAssignedNumber, "");
			orgHeader.OH_Code = "KNZ";
			orgHeader.MainAddress.Address1 = "TST1";
			Factory.Save();

			SetUpData();
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForConsumerUseNonFoodProduct;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			header.US_CategoryCode = CutFlowersAndGreeneryList.Codes.CutFlowers;
			header.US_OA_CropGrowerAddress = orgHeader.MainAddress.PK;
			header.US_OA_ApplicantAddress = orgHeader.MainAddress.PK;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var expectedMessage = @"PG19DFI                                                  TST1                   ";
			AssertContains("ACE Entry Summary message including APHIS Crop Grower Details data", expectedMessage, message.EM_FormattedMessageText);
			AssertContains("ACE Entry Summary message including APHIS Applicant Details data", "PG19LAP                                                  TST1                   ", message.EM_FormattedMessageText);
		}

		public void TestLiveAnimals_Cattle()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "0102.29.4082";
			invoiceLine.JI_Description = "CATTLE";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.APHISVSPortVeterinarian;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ConsumerProductIntendedForAdolescentsAged6To8Years;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			header.US_CategoryCode = LiveAnimalsList.Codes.BosAndBisonDomesticCattleHumpedCattleAndBison;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.TaxonomicSerialNumber;
			header.US_ProductNumber = "183838";
			// PG05
			header.US_ScientificGenusName = "BOS";
			header.US_ScientificSpeciesName = "TAURUS";
			// PG06
			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSpeciesOrigin;
			source.US_CountryCode = Core.Constants.CountryCodes.Mexico;
			source.US_ProcessingStartDate = new ZDateTime(2015, 7, 2);
			source.US_ProcessingTypeCode = "AVDIP";

			var product = header.Products.AddNew();
			// PG07 and PG08
			var identity = product.Identities.AddNew();
			// PG07
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			var range = identity.NumberRanges.AddNew();
			// PG08
			range.US_StartNumber = "TAMB218392";
			range.US_EndNumber = "TAMB218444";
			range = identity.NumberRanges.AddNew();
			range.US_StartNumber = "TAMB231113";
			range.US_EndNumber = "TAMB231159";
			// PG10
			product.US_Age = LiveAnimalsAgeList.Codes._12Years;
			product.US_AgeRangeDesc = "7-12YEARS";
			product.US_Gender = LiveAnimalsGenderList.Codes.Male;
			product.US_BreedVariety = LiveAnimalsCattleList.Codes.CorrienteCattle;
			product.US_Color = LiveAnimalsColorList.Codes.Various;
			// PG13 and PG14
			var license = header.Licenses.AddNew();
			// PG13
			license.US_RN_CountryCode = Core.Constants.CountryCodes.Mexico;
			license.US_StateDescription = "RIGHT HERE";
			// PG14
			license.US_Quantity = 100m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Number;
			license.US_Type = APHISLicenseTypeList.Codes.CertificateOfOrigin;
			license.US_Number = "COO1234";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2015, 7, 10);

			license = header.Licenses.AddNew();
			license.US_RN_CountryCode = Core.Constants.CountryCodes.Mexico;
			license.US_StateDescription = "RIGHT THERE";
			license.US_Quantity = 200m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Packs;
			license.US_Type = APHISLicenseTypeList.Codes.LiveAnimalHealthCertificate;
			license.US_Number = "HC1234";
			license.US_DateQualifier = LPCODateQualifierList.Codes.EffectiveDate;
			license.US_Date = new ZDateTime(2015, 8, 10);

			license = header.Licenses.AddNew();
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			license.US_Quantity = 300m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Pieces;
			license.US_Type = APHISLicenseTypeList.Codes.AphisVs1729;
			license.US_Number = "DOI323";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2015, 9, 10);

			license = header.Licenses.AddNew();
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			license.US_Quantity = 400m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Pairs;
			license.US_Type = APHISLicenseTypeList.Codes.AphisVs1732;
			license.US_Number = "IAD545";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2015, 9, 10);
			// PG17
			header.US_CommoditySpecificName = "STEERS";
			// PG19, PG20 and PG21
			header.US_OA_ApplicantAddress = PermitHolder.PK;
			header.US_OA_CropGrowerAddress = CropGrower.PK;
			declaration.JE_OH_Importer = Importer.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";
			//PG26
			header.US_Qty1 = 200m;
			header.US_UQ1 = ShippingOrPackingingUnitList.Codes.Package;
			header.US_Qty2 = 100m;
			header.US_UQ2 = ShippingOrPackingingUnitList.Codes.Basket;
			header.US_Qty3 = 2m;
			header.US_UQ3 = ShippingOrPackingingUnitList.Codes.Container;

			// PG27
			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "40@#";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT32423";
			container.CO_RC = containerType.PK;
			containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "20@#";
			containerType.RC_Length = 20m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.DryStorage;
			container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT85454";
			container.CO_RC = containerType.PK;
			var containers = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			containers[0].IsForInvoiceLine = true;
			containers[1].IsForInvoiceLine = true;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "2304";
			inspection.US_Date = new ZDateTime(2015, 7, 2);
			// PG32
			var routing = header.Routings[0];
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Mexico;
			routing.US_State = "TAMAULIPAS";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000                                              
OI        CATTLE                                                                
PG01001APHAVSA04 Y                       130.004                                
PG02PTSN 183838                                                                 
PG05BOS                   TAURUS                                                
PG06267MX                    07022015        AVDIP                              
PG07                                                        LAT                 
PG08#RSTAMB218392    #RETAMB218444    #RSTAMB231113    #RETAMB231159            
PG10AP0100101  A10 12YR7-12YEARS                                                
PG10AP0100101  A11 CACR                                                         
PG10AP0100101  A12 VARI                                                         
PG10AP0100101  A13 M                                                            
PG13                                   ISOMX RIGHT HERE                         
PG14 A33COO1234                          3071020150000000001000000NO            
PG13                                   ISOMX RIGHT THERE                        
PG14 A02HC1234                           2081020150000000002000000PK            
PG13                                   ISOUS                                    
PG14 A26DOI323                           3091020150000000003000000PCS           
PG13                                   ISOUS                                    
PG14 A29IAD545                           3091020150000000004000000PRS           
PG17STEERS                                                                      
PG19LAP33332KD443        PERMIT HOLDER                   PH ADDRESS 1           
PG20PH ADDRESS 2                                              NY US             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG263000000000200CON                                                            
PG262000000010000BK                                                             
PG261000000020000PK                                                             
PG27CONT32423           140CONT85454           220                              
PG30A07022015    2   2304                                                       
PG32198MX            TAMAULIPAS                                                 
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestLiveAnimals_Horses()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "0101.29.0090";
			invoiceLine.JI_Description = "HORSES";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.APHISVSPortVeterinarian;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.AnimalOrPlantForEducationalUse;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			header.US_CategoryCode = LiveAnimalsList.Codes.EquusHorse;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.TaxonomicSerialNumber;
			header.US_ProductNumber = "180691";
			// PG05
			header.US_ScientificGenusName = "EQUUS";
			header.US_ScientificSpeciesName = "CABALLUS";
			// PG06
			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSpeciesOrigin;
			source.US_CountryCode = Core.Constants.CountryCodes.Netherlands;

			var product = header.Products.AddNew();
			// PG07 and PG08
			var identity = product.Identities.AddNew();
			// PG07
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			identity.CY_Data = "58-556-14";
			// PG10
			product.US_Age = LiveAnimalsAgeList.Codes._7Years;
			product.US_Gender = LiveAnimalsGenderList.Codes.Female;
			product.US_BreedVariety = LiveAnimalsHorseList.Codes.FriesianHorse;
			product.US_Color = LiveAnimalsColorList.Codes.Bay;
			// PG13 and PG14
			var license = header.Licenses.AddNew();
			// PG13
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			// PG14
			license.US_Quantity = 1m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Number;
			license.US_Type = APHISLicenseTypeList.Codes.AphisVs17135;
			license.US_Number = "PERMITNUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.ExpirationDate;
			license.US_Date = new ZDateTime(2015, 5, 1);

			license = header.Licenses.AddNew();
			license.US_RN_CountryCode = Core.Constants.CountryCodes.Netherlands;
			license.US_Quantity = 1m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Number;
			license.US_Type = APHISLicenseTypeList.Codes.LiveAnimalHealthCertificate;
			license.US_Number = "HC1234";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2014, 4, 8);

			// PG17
			header.US_CommoditySpecificName = "HORSE";
			// PG19, PG20 and PG21
			header.US_OA_ApplicantAddress = PermitHolder.PK;
			header.US_OA_CropGrowerAddress = CropGrower.PK;
			declaration.JE_OH_Importer = Importer.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";
			//PG26
			header.US_Qty1 = 1m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.NumberCount;

			// PG27
			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "40@#";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT32423";
			container.CO_RC = containerType.PK;
			containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "20@#";
			containerType.RC_Length = 20m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.DryStorage;
			var containers = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			containers[0].IsForInvoiceLine = true;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "5206";
			inspection.US_Date = new ZDateTime(2014, 5, 8);
			// PG32
			var routing = header.Routings[0];
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Netherlands;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"OI        HORSES                                                                
PG01001APHAVSA04 Y                       180.003                                
PG02PTSN 180691                                                                 
PG05EQUUS                 CABALLUS                                              
PG06267NL                                                                       
PG07                                                        LAT58-556-14        
PG10AP0100107  A10 7YR                                                          
PG10AP0100107  A11 HFR                                                          
PG10AP0100107  A12 BAY                                                          
PG10AP0100107  A13 F                                                            
PG13                                   ISOUS                                    
PG14 A28PERMITNUMBER                     1050120150000000000010000NO            
PG13                                   ISONL                                    
PG14 A02HC1234                           3040820140000000000010000NO            
PG17HORSE                                                                       
PG19LAP33332KD443        PERMIT HOLDER                   PH ADDRESS 1           
PG20PH ADDRESS 2                                              NY US             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG261000000000100NO                                                             
PG27CONT32423           140                                                     
PG30A05082014    2   5206                                                       
PG32198NL                                                                       ";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestLiveAnimals_Dogs()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "0106199120";
			invoiceLine.JI_Description = "LIVE DOGS";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.AAC;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.APHISVSPortVeterinarian;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.AnimalOrPlantForEducationalUse;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			header.US_CategoryCode = LiveAnimalsList.Codes.CanidaeDogs;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.TaxonomicSerialNumber;
			header.US_ProductNumber = "726821";
			// PG10
			var product = header.Products.AddNew();
			product.US_Gender = LiveAnimalsGenderList.Codes.Male;
			product.US_BreedVariety = LiveAnimalsDogList.Codes.BulldogDog;
			product.US_Color = LiveAnimalsColorList.Codes.Various;
			product.US_Age = LiveAnimalsAgeList.Codes._6Months;
			// PG07
			var identity = product.Identities.AddNew();
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			identity.CY_Data = "74CFF1A";

			// PG10
			product = header.Products.AddNew();
			product.US_Gender = LiveAnimalsGenderList.Codes.Male;
			product.US_BreedVariety = LiveAnimalsDogList.Codes.BulldogDog;
			product.US_Color = LiveAnimalsColorList.Codes.Various;
			product.US_Age = LiveAnimalsAgeList.Codes._6Months;
			// PG07
			identity = product.Identities.AddNew();
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			identity.CY_Data = "6CA4B1B";

			// PG10
			product = header.Products.AddNew();
			product.US_Gender = LiveAnimalsGenderList.Codes.Male;
			product.US_BreedVariety = LiveAnimalsDogList.Codes.BulldogDog;
			product.US_Color = LiveAnimalsColorList.Codes.Various;
			product.US_Age = LiveAnimalsAgeList.Codes._6Months;
			// PG07
			identity = product.Identities.AddNew();
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			identity.CY_Data = "6CA3988";

			// PG10
			product = header.Products.AddNew();
			product.US_Gender = LiveAnimalsGenderList.Codes.Female;
			product.US_BreedVariety = LiveAnimalsDogList.Codes.BulldogDog;
			product.US_Color = LiveAnimalsColorList.Codes.Various;
			product.US_Age = LiveAnimalsAgeList.Codes._6Months;
			// PG07
			identity = product.Identities.AddNew();
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			identity.CY_Data = "6CA36D8";

			// PG10
			product = header.Products.AddNew();
			product.US_Gender = LiveAnimalsGenderList.Codes.Female;
			product.US_BreedVariety = LiveAnimalsDogList.Codes.BulldogDog;
			product.US_Color = LiveAnimalsColorList.Codes.Various;
			product.US_Age = LiveAnimalsAgeList.Codes._6Months;
			// PG07
			identity = product.Identities.AddNew();
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			identity.CY_Data = "74C717C";

			// PG10
			product = header.Products.AddNew();
			product.US_Gender = LiveAnimalsGenderList.Codes.Female;
			product.US_BreedVariety = LiveAnimalsDogList.Codes.BulldogDog;
			product.US_Color = LiveAnimalsColorList.Codes.Various;
			product.US_Age = LiveAnimalsAgeList.Codes._6Months;
			// PG07
			identity = product.Identities.AddNew();
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			identity.CY_Data = "74CC8F1";
			// PG05
			header.US_ScientificGenusName = "CANIS";
			header.US_ScientificSpeciesName = "FAMILIARIS";
			// PG06
			var source = header.Sources[0];
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSpeciesOrigin;
			source.US_CountryCode = Core.Constants.CountryCodes.Colombia;
			source.US_ProcessingStartDate = new ZDateTime(2014, 1, 12);
			source.US_ProcessingTypeCode = "AVRAB";

			// PG13 and PG14
			var license = header.Licenses.AddNew();
			// PG13
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			// PG14
			license.US_Quantity = 6m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Number;
			license.US_Type = APHISLicenseTypeList.Codes.Aphis7040b7040c;
			license.US_Number = "IMPORTPERMITNUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2015, 2, 25);

			license = header.Licenses.AddNew();
			license.US_RN_CountryCode = Core.Constants.CountryCodes.Colombia;
			license.US_Quantity = 6m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Number;
			license.US_Type = APHISLicenseTypeList.Codes.LiveAnimalHealthCertificate;
			license.US_Number = "NOHEALTHCERTIFICATENUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2015, 2, 14);

			license = header.Licenses.AddNew();
			license.US_RN_CountryCode = Core.Constants.CountryCodes.Colombia;
			license.US_Quantity = 6m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Number;
			license.US_Type = APHISLicenseTypeList.Codes.AphisRabiesVaccination;
			license.US_Number = "NORABIESVACCINATIONNUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2015, 2, 14);

			// PG17
			header.US_CommoditySpecificName = "MINI ENGLISH BULLDOG";

			// PG19, PG20 and PG21
			header.US_OA_ApplicantAddress = PermitHolder.PK;
			header.US_OA_CropGrowerAddress = CropGrower.PK;
			declaration.JE_OH_Importer = Importer.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";
			//PG26
			header.US_Qty1 = 6m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.NumberCount;

			// PG27
			header.US_VehicleNumber = "NOCONTAINER";
			header.US_VehicleLength = 10;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "2720";
			inspection.US_Date = new ZDateTime(2015, 2, 15);
			// PG32
			var routing = header.Routings[0];
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Colombia;
			routing.US_State = "BOG";

			routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.TransitCountry;
			routing.US_Country = Core.Constants.CountryCodes.Mexico;
			routing.US_State = "MEX";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"OI        LIVE DOGS                                                             
PG01001APHAACA04 Y                       180.003                                
PG02PTSN 726821                                                                 
PG07                                                        LAT74CFF1A          
PG10AP0100118  A10 6MO                                                          
PG10AP0100118  A11 DGBD                                                         
PG10AP0100118  A12 VARI                                                         
PG10AP0100118  A13 M                                                            
PG07                                                        LAT6CA4B1B          
PG10AP0100118  A10 6MO                                                          
PG10AP0100118  A11 DGBD                                                         
PG10AP0100118  A12 VARI                                                         
PG10AP0100118  A13 M                                                            
PG07                                                        LAT6CA3988          
PG10AP0100118  A10 6MO                                                          
PG10AP0100118  A11 DGBD                                                         
PG10AP0100118  A12 VARI                                                         
PG10AP0100118  A13 M                                                            
PG07                                                        LAT6CA36D8          
PG10AP0100118  A10 6MO                                                          
PG10AP0100118  A11 DGBD                                                         
PG10AP0100118  A12 VARI                                                         
PG10AP0100118  A13 F                                                            
PG07                                                        LAT74C717C          
PG10AP0100118  A10 6MO                                                          
PG10AP0100118  A11 DGBD                                                         
PG10AP0100118  A12 VARI                                                         
PG10AP0100118  A13 F                                                            
PG07                                                        LAT74CC8F1          
PG10AP0100118  A10 6MO                                                          
PG10AP0100118  A11 DGBD                                                         
PG10AP0100118  A12 VARI                                                         
PG10AP0100118  A13 F                                                            
PG05CANIS                 FAMILIARIS                                            
PG06267CO                    01122014        AVRAB                              
PG10AP0100118                                                                   
PG13                                   ISOUS                                    
PG14 A31IMPORTPERMITNUMBER               3022520150000000000060000NO            
PG13                                   ISOCO                                    
PG14 A02NOHEALTHCERTIFICATENUMBER        3021420150000000000060000NO            
PG13                                   ISOCO                                    
PG14 A30NORABIESVACCINATIONNUMBER        3021420150000000000060000NO            
PG17MINI ENGLISH BULLDOG                                                        
PG19LAP33332KD443        PERMIT HOLDER                   PH ADDRESS 1           
PG20PH ADDRESS 2                                              NY US             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG261000000000600NO                                                             
PG27NOCONTAINER         210                                                     
PG30A02152015    2   2720                                                       
PG32198CO            BOG                                                        
PG3249 MX            MEX                                                        ";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestLiveAnimals_Birds_Parakeets()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "0106.39.0000";
			invoiceLine.JI_Description = "PET BIRDS";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.APHISVSPortVeterinarian;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.AnimalOrPlantForCommercialSale;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			header.US_CategoryCode = LiveAnimalsList.Codes.OtherAvesBirds;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.TaxonomicSerialNumber;
			header.US_ProductNumber = "177672";
			// PG05
			header.US_ScientificGenusName = "ARATINGA";
			header.US_ScientificSpeciesName = "SPECIES";
			// PG06
			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSpeciesOrigin;
			source.US_CountryCode = Core.Constants.CountryCodes.Canada;

			var product = header.Products.AddNew();
			// PG10
			product.US_Age = LiveAnimalsAgeList.Codes._1To6Months;
			product.US_AgeRangeDesc = "SIXTOEIGHTWEEKS";
			product.US_BreedVariety = LiveAnimalsBirdsList.Codes.PsittacineSpeciesAves;
			// PG13 and PG14
			var license = header.Licenses.AddNew();
			// PG13
			license.US_RN_CountryCode = Core.Constants.CountryCodes.Canada;
			// PG14
			license.US_Quantity = 1750m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Number;
			license.US_Type = APHISLicenseTypeList.Codes.LiveAnimalHealthCertificate;
			license.US_Number = "HEALTHCERTIFICATENUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2014, 9, 6);

			license = header.Licenses.AddNew();
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;

			license.US_Quantity = 1750m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Number;
			license.US_Type = APHISLicenseTypeList.Codes.AphisVs17135;
			license.US_Number = "HC1234";
			license.US_DateQualifier = LPCODateQualifierList.Codes.ExpirationDate;
			license.US_Date = new ZDateTime(2015, 1, 23);

			// PG17
			header.US_CommoditySpecificName = "PARAKEET";
			// PG19, PG20 and PG21
			header.US_OA_ApplicantAddress = PermitHolder.PK;
			header.US_OA_CropGrowerAddress = CropGrower.PK;
			declaration.JE_OH_Importer = Importer.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";
			//PG26
			header.US_Qty1 = 1750m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.NumberCount;

			// PG27
			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "40@#";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.DryStorage;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT32423";
			container.CO_RC = containerType.PK;
			var containers = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			containers[0].IsForInvoiceLine = true;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "3401";
			inspection.US_Date = new ZDateTime(2014, 9, 7);
			// PG32
			var routing = header.Routings[0];
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Canada;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000 000000000000NO                               
OI        PET BIRDS                                                             
PG01001APHAVSA04 Y                       130.035                                
PG02PTSN 177672                                                                 
PG05ARATINGA              SPECIES                                               
PG06267CA                                                                       
PG10AP0100112  A10 1M6 SIXTOEIGHTWEEKS                                          
PG10AP0100112  A11 AVPS                                                         
PG13                                   ISOCA                                    
PG14 A02HEALTHCERTIFICATENUMBER          3090620140000000017500000NO            
PG13                                   ISOUS                                    
PG14 A28HC1234                           1012320150000000017500000NO            
PG17PARAKEET                                                                    
PG19LAP33332KD443        PERMIT HOLDER                   PH ADDRESS 1           
PG20PH ADDRESS 2                                              NY US             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG261000000175000NO                                                             
PG27CONT32423           240                                                     
PG30A09072014    2   3401                                                       
PG32198CA                                                                       
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestRelatedAnimalProducts_UsedFarmTractors()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "8701.90.1090";
			invoiceLine.JI_Description = "USED AGRICULTURAL TRACTORS";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			header.ApplicantOrgPK = ZGuid.Empty;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.MotorizedVehiclesOrEnginesIntendedForOffRoadUse;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts;
			header.US_CategoryCode = RelatedAnimalProductsList.Codes.UsedFarmMachinery;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode;
			header.US_ProductNumber = "25101901";
			// PG06
			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfProduction;
			source.US_CountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			// PG10
			header.US_ProductCondition = RelatedAnimalProductsConditionA20List.Codes.Used;
			// PG17
			header.US_CommoditySpecificName = "TRACORS";
			// PG19, PG20 and PG21
			header.US_OA_ShipperAddress = PermitHolder.PK;
			header.US_OA_CropGrowerAddress = CropGrower.PK;
			declaration.JE_OH_Importer = Importer.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";
			//PG26
			header.US_Qty1 = 2m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.NumberCount;

			// PG27
			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "40@#";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.DryStorage;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT32423";
			container.CO_RC = containerType.PK;
			var containers = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			containers[0].IsForInvoiceLine = true;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "5206";
			inspection.US_Date = new ZDateTime(2014, 12, 5);
			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.UnitedKingdom;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000 000000000000NO                               
OI        USED AGRICULTURAL TRACTORS                                            
PG01001APHAVSA01 Y                       130.018                                
PG02PUNS 25101901                                                               
PG0639 GB                                                                       
PG10AP0200206  A20 USED                                                         
PG17TRACORS                                                                     
PG19DEQ                  PERMIT HOLDER                   PH ADDRESS 1           
PG20PH ADDRESS 2                                              NY US             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG261000000000200NO                                                             
PG27CONT32423           240                                                     
PG30A12052014    2   5206                                                       
PG32198GB                                                                       
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestAnimalProductsAndAnimalByProducts_Bouillon()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "2104.10.0060";
			invoiceLine.JI_Description = "BOUILLON";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForConsumerUseHumanFood;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			header.US_CategoryCode = AnimalProductsAndByProductsList.Codes.MeatAndPoultryProducts;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.GlobalProductClasBrickCode;
			header.US_ProductNumber = "10006214";
			// PG06
			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSlaughter;
			source.US_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfProcessing;
			source.US_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfProduction;
			source.US_CountryCode = Core.Constants.CountryCodes.Guatemala;
			// PG10
			header.US_ProductCondition = AnimalProductsAndByProductsConditionA30List.Codes.EdibleShelfStable;
			header.US_ProductPhysicalState = AnimalProductsAndByProductsConditionA31List.Codes.Cubes;
			// PG17
			header.US_CommoditySpecificName = "CHICKEN BOUILLON";

			// PG02, PG06, PG10 and PG17
			var product = header.Products.AddNew();
			product.US_Origin = Core.Constants.CountryCodes.UnitedStates;
			product.US_Type = AnimalProductsAndByProductsConditionA32List.Codes.AvesPoultryProducts;
			product.US_SpecificName = "DEHYDRATED CHICKEN MEAT";
			product = header.Products.AddNew();
			product.US_Origin = Core.Constants.CountryCodes.Germany;
			product.US_Type = AnimalProductsAndByProductsConditionA32List.Codes.AvesPoultryProducts;
			product.US_SpecificName = "EGG INGREDIENT";
			product = header.Products.AddNew();
			product.US_Origin = Core.Constants.CountryCodes.NewZealand;
			product.US_Type = AnimalProductsAndByProductsConditionA32List.Codes.BovineBeefProducts;
			product.US_SpecificName = "MILK INGREDIENT";

			// PG13 and PG14
			var license = header.Licenses.AddNew();
			// PG13
			license.US_RN_CountryCode = Core.Constants.CountryCodes.Guatemala;
			// PG14
			license.US_Type = APHISLicenseTypeList.Codes.AphisPpq203;
			license.US_Number = "SC3532";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2014, 11, 4);
			license.US_Quantity = 12065m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Kilograms;

			license = header.Licenses.AddNew();
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;

			license.US_Type = APHISLicenseTypeList.Codes.AphisVs166A;
			license.US_Number = "AVS323";
			license.US_DateQualifier = LPCODateQualifierList.Codes.ExpirationDate;
			license.US_Date = new ZDateTime(2015, 11, 20);

			// PG19, PG20 and PG21
			header.US_OA_ApplicantAddress = PermitHolder.PK;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";
			//PG26
			header.US_Qty1 = 12065m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.KilogramsWeight;

			// PG27
			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "40@#";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT32423";
			container.CO_RC = containerType.PK;
			var invoiceLineContainers = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			invoiceLineContainers[0].IsForInvoiceLine = true;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "1101";
			inspection.US_Date = new ZDateTime(2014, 11, 5);
			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Guatemala;
			routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.TransitCountry;
			routing.US_Country = Core.Constants.CountryCodes.Mexico;
			routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.PlaceOfTransshipment;
			routing.US_Country = Core.Constants.CountryCodes.Mexico;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000 000000000000KG                               
OI        BOUILLON                                                              
PG01001APHAVSA01 Y                       230.000                                
PG02PGPC 10006214                                                               
PG06243US                                                                       
PG06CPRUS                                                                       
PG0639 GT                                                                       
PG10AP0300301  A30 EDB                                                          
PG10AP0300301  A31 CUB                                                          
PG17CHICKEN BOUILLON                                                            
PG02C                                                                           
PG06267US                                                                       
PG10AP0300301  A32 AVE                                                          
PG17DEHYDRATED CHICKEN MEAT                                                     
PG02C                                                                           
PG06267DE                                                                       
PG10AP0300301  A32 AVE                                                          
PG17EGG INGREDIENT                                                              
PG02C                                                                           
PG06267NZ                                                                       
PG10AP0300301  A32 BOV                                                          
PG17MILK INGREDIENT                                                             
PG13                                   ISOGT                                    
PG14 A07SC3532                           3110420140000000120650000KG            
PG13                                   ISOUS                                    
PG14 A24AVS323                           111202015                              
PG19LAP33332KD443        PERMIT HOLDER                   PH ADDRESS 1           
PG20PH ADDRESS 2                                              NY US             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG261000001206500KG                                                             
PG27CONT32423           140                                                     
PG30A11052014    2   1101                                                       
PG32198GT                                                                       
PG3249 MX                                                                       
PG3213 MX                                                                       
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestAPHISProducts_ProgramTypeAPQ()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "0106.39.0000";
			invoiceLine.JI_Description = "BEEF FATTY TISSUE";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			// PG01
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForAnimalFoodFeed;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			header.US_CategoryCode = AnimalProductsAndByProductsList.Codes.AnimalConsumptionProducts;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode;
			header.US_ProductNumber = "50111513";
			// PG06
			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfProduction;
			source.US_CountryCode = Core.Constants.CountryCodes.Canada;
			// PG10
			header.US_ProductCondition = AnimalProductsAndByProductsConditionA30List.Codes.Inedible;
			header.US_ProductPhysicalState = AnimalProductsAndByProductsConditionA31List.Codes.FreshFrozen;

			// PG13 and PG14
			var license = header.Licenses.AddNew();
			// PG13
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			// PG14
			license.US_Type = APHISLicenseTypeList.Codes.AphisVs166A;
			license.US_Number = "SC3532";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2015, 3, 9);

			license = header.Licenses.AddNew();
			license.US_RN_CountryCode = Core.Constants.CountryCodes.Canada;

			license.US_Type = APHISLicenseTypeList.Codes.LiveAnimalHealthCertificate;
			license.US_Number = "AVS323";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2015, 5, 5);
			license.US_Quantity = 44000m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Pounds;
			//PG26
			header.US_Qty1 = 44000m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.PoundsAvdpWeight;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "VARIOUS";
			inspection.US_Date = new ZDateTime(2015, 5, 6);
			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Canada;
			// PG02C
			var product1 = header.Products.AddNew();
			// PG05
			product1.US_Genus = "Apple";
			product1.US_Species = "Orange";
			product1.US_Variety = "Potato";
			// PG06
			product1.US_SourceTypeCode = "VEG";
			product1.US_CountryCode = "CN";
			product1.US_GeographicLocation = "LOCATION1";
			product1.US_ProcessingStartDate = new ZDate(2021, 02, 02);
			product1.US_ProcessingEndDate = new ZDate(2021, 03, 03);
			product1.US_ProcessingDescription = "PRODESCRIPTION1";
			product1.US_ProcessingTypeCode = "ZZZZZ";
			// PG17
			product1.US_SpecificName = "OCTOPUS";

			// PG02C
			var product2 = header.Products.AddNew();
			// PG05
			product2.US_Genus = "Genus";
			product2.US_Species = "Species";
			product2.US_Variety = "Variety";
			// PG06
			product2.US_SourceTypeCode = "AAA";
			product2.US_CountryCode = "CN";
			product2.US_GeographicLocation = "LOCATION";
			product2.US_ProcessingStartDate = new ZDate(2021, 03, 12);
			product2.US_ProcessingEndDate = new ZDate(2021, 03, 16);
			product2.US_ProcessingDescription = "PRODESCRIPTION";
			product2.US_ProcessingTypeCode = "TYTYT";
			// PG17
			product2.US_SpecificName = "CAULIFLOWER";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000 000000000000NO                               
OI        BEEF FATTY TISSUE                                                     
PG01001APHAPQA01 Y                       010.000                                
PG02PUNS 50111513                                                               
PG0639 CA                                                                       
PG10AP0300305  A30 IDB                                                          
PG10AP0300305  A31 FRF                                                          
PG13                                   ISOUS                                    
PG14 A24SC3532                           303092015                              
PG13                                   ISOCA                                    
PG14 A02AVS323                           3050520150000000440000000LB            
PG19LAP                  IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG19CB 336XJ5            EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG261000004400000LB                                                             
PG30A05062015    2   VARIOUS                                                    
PG32198CA                                                                       
PG02C                                                                           
PG05APPLE                 ORANGE                POTATO                          
PG06VEGCNLOCATION1           0202202103032021ZZZZZPRODESCRIPTION1               
PG17OCTOPUS                                                                     
PG02C                                                                           
PG05GENUS                 SPECIES               VARIETY                         
PG06AAACNLOCATION            0312202103162021TYTYTPRODESCRIPTION                
PG17CAULIFLOWER                                                                 
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestAnimalProductsAndAnimalByProducts_PetFoodIngredients()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "0106.39.0000";
			invoiceLine.JI_Description = "BEEF FATTY TISSUE";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForAnimalFoodFeed;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			header.US_CategoryCode = AnimalProductsAndByProductsList.Codes.AnimalConsumptionProducts;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode;
			header.US_ProductNumber = "50111513";
			// PG06
			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfProduction;
			source.US_CountryCode = Core.Constants.CountryCodes.Canada;
			// PG10
			header.US_ProductCondition = AnimalProductsAndByProductsConditionA30List.Codes.Inedible;
			header.US_ProductPhysicalState = AnimalProductsAndByProductsConditionA31List.Codes.FreshFrozen;
			header.US_ProductComponent = AnimalProductsAndByProductsConditionA32List.Codes.BovineBeefProducts;

			// PG13 and PG14
			var license = header.Licenses.AddNew();
			// PG13
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			// PG14
			license.US_Type = APHISLicenseTypeList.Codes.AphisVs166A;
			license.US_Number = "SC3532";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2015, 3, 9);

			license = header.Licenses.AddNew();
			license.US_RN_CountryCode = Core.Constants.CountryCodes.Canada;

			license.US_Type = APHISLicenseTypeList.Codes.LiveAnimalHealthCertificate;
			license.US_Number = "AVS323";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2015, 5, 5);
			license.US_Quantity = 44000m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Pounds;
			// PG17
			header.US_CommoditySpecificName = "BEEFFATTYTISSUE";

			// PG19, PG20 and PG21
			header.US_OA_ApplicantAddress = PermitHolder.PK;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";
			//PG26
			header.US_Qty1 = 44000m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.PoundsAvdpWeight;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "VARIOUS";
			inspection.US_Date = new ZDateTime(2015, 5, 6);
			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Canada;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000 000000000000NO                               
OI        BEEF FATTY TISSUE                                                     
PG01001APHAVSA01 Y                       010.000                                
PG02PUNS 50111513                                                               
PG0639 CA                                                                       
PG10AP0300305  A30 IDB                                                          
PG10AP0300305  A31 FRF                                                          
PG10AP0300305  A32 BOV                                                          
PG13                                   ISOUS                                    
PG14 A24SC3532                           303092015                              
PG13                                   ISOCA                                    
PG14 A02AVS323                           3050520150000000440000000LB            
PG17BEEFFATTYTISSUE                                                             
PG19LAP33332KD443        PERMIT HOLDER                   PH ADDRESS 1           
PG20PH ADDRESS 2                                              NY US             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG261000004400000LB                                                             
PG30A05062015    2   VARIOUS                                                    
PG32198CA                                                                       
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestPropagativeMaterial_NurseryStock_PineTrees()
		{
			SetUpData();

			var usdaAPHISGrower = Factory.New<OrgHeader>();
			usdaAPHISGrower.OH_Code = "USDAGROW";
			usdaAPHISGrower.OH_FullName = "USDA APHIS Grower";
			usdaAPHISGrower.OH_RL_NKClosestPort = "AUSYD";
			var usdaAPHISGrowerAddress = usdaAPHISGrower.MainAddress;
			usdaAPHISGrowerAddress.OA_Address1 = "GR ADDRESS 1";
			usdaAPHISGrowerAddress.OA_Address2 = "GR ADDRESS 2";
			usdaAPHISGrowerAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			invoiceLine.JI_Tariff = "4407.10.0143";
			invoiceLine.JI_Description = "PINE TREES";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForBreedingAnimalPlant;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.PropagativeMaterial;
			header.US_CategoryCode = PropagativeMaterialList.Codes.PlantsForPlantingOrPropagationWhole;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.TaxonomicSerialNumber;
			header.US_ProductNumber = "183356";
			// PG05
			header.US_ScientificGenusName = "PINUS";
			header.US_ScientificSpeciesName = "MONTICOLA";
			// PG06
			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.PlaceOfGrowth;
			source.US_CountryCode = Core.Constants.CountryCodes.Canada;
			source.US_GeographicLocation = CanadaStatesList.Codes.ON;

			// PG10
			header.US_GrowingMedia = PropagativeMaterialLifeStageA43List.Codes.ArtificialSoilless;

			// PG13 and PG14
			var license = header.Licenses.AddNew();
			// PG13
			license.US_RN_CountryCode = Core.Constants.CountryCodes.Canada;
			// PG14
			license.US_Quantity = 5m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Number;
			license.US_Type = APHISLicenseTypeList.Codes.PhytosanitaryCertificate;
			license.US_Number = "PERMITNUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2014, 4, 23);

			license = header.Licenses.AddNew();
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;

			license.US_Type = APHISLicenseTypeList.Codes.AphisPpq58737can;
			license.US_Number = "HC1234";
			license.US_DateQualifier = LPCODateQualifierList.Codes.ExpirationDate;
			license.US_Date = new ZDateTime(2017, 4, 23);

			// PG17
			header.US_CommoditySpecificName = "WESTERN WHITE";

			// PG19, PG20 and PG21
			header.US_OA_ApplicantAddress = PermitHolder.PK;
			header.US_OA_CropGrowerAddress = CropGrower.PK;
			header.US_OA_USDAAPHISGrowerAddress = usdaAPHISGrowerAddress.PK;
			declaration.JE_OH_Importer = Importer.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";
			//PG26
			header.US_Qty1 = 5m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.NumberCount;

			// PG27
			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "40@#";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT32423";
			container.CO_RC = containerType.PK;
			containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "20@#";
			containerType.RC_Length = 20m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.DryStorage;
			var containers = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			containers[0].IsForInvoiceLine = true;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "0901";
			inspection.US_Date = new ZDateTime(2014, 4, 25);
			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Canada;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000 000000000000M3                               
OI        PINE TREES                                                            
PG01001APHAPQA01 Y                       020.000                                
PG02PTSN 183356                                                                 
PG05PINUS                 MONTICOLA                                             
PG06262CAON                                                                     
PG10AP0400402  A43 ARTIARTIFICIAL / SOILLESS                                    
PG13                                   ISOCA                                    
PG14 A01PERMITNUMBER                     3042320140000000000050000NO            
PG13                                   ISOUS                                    
PG14 A21HC1234                           104232017                              
PG17WESTERN WHITE                                                               
PG19LAP33332KD443        PERMIT HOLDER                   PH ADDRESS 1           
PG20PH ADDRESS 2                                              NY US             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG19AG1                  USDA APHIS GROWER               GR ADDRESS 1           
PG20GR ADDRESS 2                                                 AU             
PG261000000000500NO                                                             
PG27CONT32423           140                                                     
PG30A04252014    2   0901                                                       
PG32198CA                                                                       
5401N0000000000                                                                 
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestPropagativeMaterial_Bulbs()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "4407.10.0143";
			invoiceLine.JI_Description = "PINE TREES";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForBreedingAnimalPlant;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.PropagativeMaterial;
			header.US_CategoryCode = PropagativeMaterialList.Codes.BulbsAndUndergroundPortionsOfDormantPerennials;
			header.US_ProductStatus = "";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"PG10AP0400401                                                                   ";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);

			header.US_ProductStatus = "ABC";
			message = builder.PopulateMessage();
			AssertNotContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestPropagativeMaterial_MachineryVehicles()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "4407.10.0143";
			invoiceLine.JI_Description = "130.050 Machinery";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.APHISPreClearance;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.UsedFarmMachineryVehiclesOrTrailersForCommercialSale;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.PropagativeMaterial;
			header.US_CategoryCode = PropagativeMaterialList.Codes.RootCuttingsOrRootCrownForPlantingOrPropagation;
			header.US_ProductStatus = "";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"PG10AP0400405                                                                   ";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestSeedsNotForPlanting_FabaBeans()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "0713.31.2000";
			invoiceLine.JI_Description = "FABA BEANS";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForConsumerUseHumanFood;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting;
			header.US_CategoryCode = APHIS.ArticleCategory.SeedsNotForPlantingList.Codes.SeedsNotForPlanting;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.TaxonomicSerialNumber;
			header.US_ProductNumber = "26339";
			// PG05
			header.US_ScientificGenusName = "VICIA";
			header.US_ScientificSpeciesName = "FABA";
			// PG06
			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.PlaceOfGrowth;
			source.US_CountryCode = Core.Constants.CountryCodes.Peru;

			// PG10
			header.US_ProductPhysicalState = SeedsNotForPlantingList.Codes.WithHuskOrShells;
			// PG13 and PG14
			var license = header.Licenses.AddNew();
			// PG13
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			// PG14
			license.US_Type = APHISLicenseTypeList.Codes.AphisPpq58756;
			license.US_Number = "PERMITNUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.ExpirationDate;
			license.US_Date = new ZDateTime(2015, 10, 19);

			// PG17
			header.US_CommoditySpecificName = "FABA BEAN";

			// PG19, PG20 and PG21
			header.US_OA_ApplicantAddress = PermitHolder.PK;
			header.US_OA_CropGrowerAddress = CropGrower.PK;
			declaration.JE_OH_Importer = Importer.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";
			//PG26
			header.US_Qty1 = 19520m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.KilogramsWeight;

			// PG27
			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "40@#";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT32423";
			container.CO_RC = containerType.PK;
			containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "20@#";
			containerType.RC_Length = 20m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.DryStorage;
			var containers = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			containers[0].IsForInvoiceLine = true;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "VARIOUS";
			inspection.US_Date = new ZDateTime(2014, 12, 5);
			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Peru;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000 000000000000KG                               
OI        FABA BEANS                                                            
PG01001APHAPQA01 Y                       230.000                                
PG02PTSN 26339                                                                  
PG05VICIA                 FABA                                                  
PG06262PE                                                                       
PG10AP0500501  A51 WHS                                                          
PG13                                   ISOUS                                    
PG14 A19PERMITNUMBER                     110192015                              
PG17FABA BEAN                                                                   
PG19LAP33332KD443        PERMIT HOLDER                   PH ADDRESS 1           
PG20PH ADDRESS 2                                              NY US             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG261000001952000KG                                                             
PG27CONT32423           140                                                     
PG30A12052014    2   VARIOUS                                                    
PG32198PE                                                                       
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestFruitsAndVegetables_PreClearanceClementine()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "2008.30.5500";
			invoiceLine.JI_Description = "CLEMENTINES";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.APHISPreClearance;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForConsumerUseHumanFood;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.FruitsAndVegetables;
			header.US_CategoryCode = APHIS.ArticleCategory.FruitsAndVegetablesList.Codes.AboveGroundParts;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode;
			header.US_ProductNumber = "50304402";
			// PG05
			header.US_ScientificGenusName = "CITRUS";
			header.US_ScientificSpeciesName = "CLEMENTINA";
			// PG06
			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.PlaceOfGrowth;
			source.US_CountryCode = Core.Constants.CountryCodes.Spain;
			source.US_GeographicLocation = "CASTELLON";
			source.US_ProcessingStartDate = new ZDateTime(2014, 10, 23);
			source.US_ProcessingTypeCode = "ACT01";

			// PG10
			header.US_ProductPhysicalState = FruitsAndVegetablesList.Codes.FreshChilled;
			// PG13 and PG14
			var license = header.Licenses.AddNew();
			// PG13
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			// PG14
			license.US_Type = APHISLicenseTypeList.Codes.AphisPpq203;
			license.US_Number = "PERMITNUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2014, 10, 23);
			license.US_Quantity = 36000m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Packs;

			// PG13
			license = header.Licenses.AddNew();
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			// PG14
			license.US_Type = APHISLicenseTypeList.Codes.AphisPpq58756;
			license.US_Number = "PERMITNUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.ExpirationDate;
			license.US_Date = new ZDateTime(2015, 5, 30);

			// PG13
			license = header.Licenses.AddNew();
			license.US_RN_CountryCode = Core.Constants.CountryCodes.Spain;
			// PG14
			license.US_Type = APHISLicenseTypeList.Codes.PhytosanitaryCertificate;
			license.US_Number = "PERMITNUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2014, 10, 30);
			license.US_Quantity = 83040000m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Kilograms;

			// PG17
			header.US_CommoditySpecificName = "CLEMENTINE";

			// PG19, PG20 and PG21
			header.US_OA_ApplicantAddress = PermitHolder.PK;
			header.US_OA_CropGrowerAddress = CropGrower.PK;
			declaration.JE_OH_Importer = Importer.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";
			//PG26
			header.US_Qty1 = 83040000m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.KilogramsWeight;

			// PG27
			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "40@#";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT32423";
			container.CO_RC = containerType.PK;
			containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "20@#";
			containerType.RC_Length = 20m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.DryStorage;
			var containers = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			containers[0].IsForInvoiceLine = true;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "1101";
			inspection.US_Date = new ZDateTime(2014, 11, 5);
			inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.PreviouslyPerformed;
			inspection.US_Location = "ES";
			inspection.US_Date = new ZDateTime(2014, 10, 23);

			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Spain;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000 000000000000KG                               
OI        CLEMENTINES                                                           
PG01001APHAPQA03 Y                       230.000                                
PG02PUNS 50304402                                                               
PG05CITRUS                CLEMENTINA                                            
PG06262ESCASTELLON           10232014        ACT01                              
PG10AP0600601  A61 FRC                                                          
PG13                                   ISOUS                                    
PG14 A07PERMITNUMBER                     3102320140000000360000000PK            
PG13                                   ISOUS                                    
PG14 A19PERMITNUMBER                     105302015                              
PG13                                   ISOES                                    
PG14 A01PERMITNUMBER                     3103020140000830400000000KG            
PG17CLEMENTINE                                                                  
PG19LAP33332KD443        PERMIT HOLDER                   PH ADDRESS 1           
PG20PH ADDRESS 2                                              NY US             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG261008304000000KG                                                             
PG27CONT32423           140                                                     
PG30A11052014    2   1101                                                       
PG30P10232014    3   ES                                                         
PG32198ES                                                                       
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestFruitsAndVegetables_Cilantro()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "0710.80.9726";
			invoiceLine.JI_Description = "CILANTRO";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForConsumerUseHumanFood;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.FruitsAndVegetables;
			header.US_CategoryCode = APHIS.ArticleCategory.FruitsAndVegetablesList.Codes.AboveGroundParts;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode;
			header.US_ProductNumber = "50404106";
			// PG05
			header.US_ScientificGenusName = "CORIANDRUM";
			header.US_ScientificSpeciesName = "SATIVUM";
			// PG06
			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.PlaceOfGrowth;
			source.US_CountryCode = Core.Constants.CountryCodes.Mexico;
			source.US_GeographicLocation = "PUEBLA";

			// PG10
			header.US_ProductPhysicalState = FruitsAndVegetablesList.Codes.FreshChilled;
			// PG13 and PG14
			var license = header.Licenses.AddNew();
			// PG13
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			// PG14
			license.US_Type = APHISLicenseTypeList.Codes.AphisPpq58756;
			license.US_Number = "PERMITNUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.ExpirationDate;
			license.US_Date = new ZDateTime(2017, 7, 16);

			// PG13
			license = header.Licenses.AddNew();
			license.US_RN_CountryCode = Core.Constants.CountryCodes.Mexico;
			// PG14
			license.US_Type = APHISLicenseTypeList.Codes.PhytosanitaryCertificate;
			license.US_Number = "PERMITNUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2015, 12, 11);
			license.US_Quantity = 1078m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.LongTon2240LbWgt;

			// PG17
			header.US_CommoditySpecificName = "CILANTRO";

			// PG19, PG20 and PG21
			header.US_OA_ApplicantAddress = PermitHolder.PK;
			header.US_OA_CropGrowerAddress = CropGrower.PK;
			declaration.JE_OH_Importer = Importer.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";
			//PG26
			header.US_Qty1 = 10.78m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.MetricTon;

			// PG27
			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "40@#";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT32423";
			container.CO_RC = containerType.PK;
			containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "20@#";
			containerType.RC_Length = 20m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.DryStorage;
			var containers = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			containers[0].IsForInvoiceLine = true;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "2305";
			inspection.US_Date = new ZDateTime(2015, 12, 15);

			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Mexico;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000 000000000000KG                               
OI        CILANTRO                                                              
PG01001APHAPQA01 Y                       230.000                                
PG02PUNS 50404106                                                               
PG05CORIANDRUM            SATIVUM                                               
PG06262MXPUEBLA                                                                 
PG10AP0600601  A61 FRC                                                          
PG13                                   ISOUS                                    
PG14 A19PERMITNUMBER                     107162017                              
PG13                                   ISOMX                                    
PG14 A01PERMITNUMBER                     3121120150000000010780000TON           
PG17CILANTRO                                                                    
PG19LAP33332KD443        PERMIT HOLDER                   PH ADDRESS 1           
PG20PH ADDRESS 2                                              NY US             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG261000000001078T                                                              
PG27CONT32423           140                                                     
PG30A12152015    2   2305                                                       
PG32198MX                                                                       
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.APHIS2024, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				header.US_ProductPhysicalState = FruitsAndVegetablesList.Codes.Fresh;
				header.US_ProductIngredientType = IngredientTypeList.Codes.SingleIngredient;

				message = builder.PopulateMessage();
				expectedMessage = @"50           0000000000 0000010000 000000000000KG                               
OI        CILANTRO                                                              
PG01001APHAPQA01 Y                       230.000                                
PG02PUNS 50404106                                                               
PG05CORIANDRUM            SATIVUM                                               
PG06262MXPUEBLA                                                                 
PG10AP0600601  A60 SGL                                                          
PG10AP0600601  A61 FRS                                                          
PG13                                   ISOUS                                    
PG14 A19PERMITNUMBER                     107162017                              
PG13                                   ISOMX                                    
PG14 A01PERMITNUMBER                     3121120150000000010780000TON           
PG17CILANTRO                                                                    
PG19LAP33332KD443        PERMIT HOLDER                   PH ADDRESS 1           
PG20PH ADDRESS 2                                              NY US             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG261000000001078T                                                              
PG27CONT32423           140                                                     
PG30A12152015    2   2305                                                       
PG32198MX                                                                       
6249900003464                                                                   
";
				AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
			}
		}

		public void TestMiscellaneousAndProcessedProducts_Soil()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "9808.00.6000";
			invoiceLine.JI_Description = "SOIL";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForResearchDevelopmentNonFoodProduct;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts;
			header.US_CategoryCode = MiscellaneousAndProcessedProductsList.Codes.SoilRocksAndGarbage;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode;
			header.US_ProductNumber = "11111501";

			// PG10
			header.US_ProductPhysicalState = MiscellaneousAndProcessedProductsPhysicalStateList.Codes.Samples;
			// PG13 and PG14
			var license = header.Licenses.AddNew();
			// PG13
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			// PG14
			license.US_Type = APHISLicenseTypeList.Codes.AphisPpq525b;
			license.US_Number = "PERMITNUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.ExpirationDate;
			license.US_Date = new ZDateTime(2017, 6, 24);
			license.US_Quantity = 3m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Pounds;

			// PG17
			header.US_CommoditySpecificName = "SOIL";

			// PG19, PG20 and PG21
			header.US_OA_ApplicantAddress = PermitHolder.PK;
			header.US_OA_CropGrowerAddress = CropGrower.PK;
			declaration.JE_OH_Importer = Importer.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";
			//PG26
			header.US_Qty1 = 3m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.PoundsAvdpWeight;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "4197";
			inspection.US_Date = new ZDateTime(2014, 12, 5);

			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Malaysia;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"OI        SOIL                                                                  
PG01001APHAPQA01 Y                       180.000                                
PG02PUNS 11111501                                                               
PG10AP0700721  A71 SAM                                                          
PG13                                   ISOUS                                    
PG14 A09PERMITNUMBER                     1062420170000000000030000LB            
PG17SOIL                                                                        
PG19LAP33332KD443        PERMIT HOLDER                   PH ADDRESS 1           
PG20PH ADDRESS 2                                              NY US             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG261000000000300LB                                                             
PG30A12052014    2   4197                                                       
PG32198MY                                                                       
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestMiscellaneousAndProcessedProducts_Brooms()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "0710.80.9726";
			invoiceLine.JI_Description = "BROOMS";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.AnimalOrPlantForCommercialSale;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts;
			header.US_CategoryCode = MiscellaneousAndProcessedProductsList.Codes.BroomcornAndBroomstraw;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode;
			header.US_ProductNumber = "47131600";

			// PG10
			header.US_ProductCondition = MiscellaneousAndProcessedProductsConditionList.Codes.New;
			header.US_ProductPhysicalState = MiscellaneousAndProcessedProductsPhysicalStateList.Codes.Manufactured;

			var license = header.Licenses.AddNew();
			// PG13
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			// PG14
			license.US_Type = APHISLicenseTypeList.Codes.AphisPpq58741;
			license.US_Number = "PERMITNUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.ExpirationDate;
			license.US_Date = new ZDateTime(2015, 4, 16);

			// PG17
			header.US_CommoditySpecificName = "BROOMCORN";

			// PG19, PG20 and PG21
			header.US_OA_ApplicantAddress = PermitHolder.PK;
			header.US_OA_CropGrowerAddress = CropGrower.PK;
			declaration.JE_OH_Importer = Importer.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";
			//PG26
			header.US_Qty1 = 10963m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.KilogramsWeight;

			// PG27
			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "40@#";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT32423";
			container.CO_RC = containerType.PK;
			containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "20@#";
			containerType.RC_Length = 20m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.DryStorage;
			var containers = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			containers[0].IsForInvoiceLine = true;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "2304";
			inspection.US_Date = new ZDateTime(2014, 12, 5);

			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Mexico;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000 000000000000KG                               
OI        BROOMS                                                                
PG01001APHAPQA01 Y                       130.035                                
PG02PUNS 47131600                                                               
PG10AP0700704  A70 NEW                                                          
PG10AP0700704  A71 MAN                                                          
PG13                                   ISOUS                                    
PG14 A17PERMITNUMBER                     104162015                              
PG17BROOMCORN                                                                   
PG19LAP33332KD443        PERMIT HOLDER                   PH ADDRESS 1           
PG20PH ADDRESS 2                                              NY US             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG261000001096300KG                                                             
PG27CONT32423           140                                                     
PG30A12052014    2   2304                                                       
PG32198MX                                                                       
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestAPHISOnlyHasStateOnlyInUSMACA()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "Snapdragons";
			invoiceLine.JI_Description = "SNAPDRAGONS";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			header.ApplicantOrgPK = ZGuid.Empty;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForConsumerUseNonFoodProduct;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			header.US_CategoryCode = CutFlowersAndGreeneryList.Codes.CutFlowers;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode;
			header.US_ProductNumber = "93010600";
			header.US_StockKeepingUnitNumber = "00034285564";
			// PG05
			header.US_ScientificGenusName = "ANTIRRHINUM";
			header.US_ScientificSpeciesName = "SP";
			// PG06
			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.PlaceOfGrowth;
			source.US_CountryCode = Core.Constants.CountryCodes.Colombia;

			// PG10
			header.US_ProductCondition = CutFlowersAndGreeneryTypeList.Codes.SingleGenusOfFlower;

			// PG17
			header.US_CommoditySpecificName = "SNAPDRAGONS";

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "CAIMPORTER";
			org.OH_RL_NKClosestPort = "CALAX";
			var caImporter = org.MainAddress;
			caImporter.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			caImporter.OA_Address1 = "1234 PEACHTREE STREET";
			caImporter.OA_Address2 = "Anne of Greengables";
			caImporter.OA_State = "PE";
			caImporter.OA_RL_NKRelatedPortCode = "CALAX";
			caImporter.OA_PostCode = "30301";
			caImporter.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.CBPAssignedNumber, "32-843944", Core.Constants.CountryCodes.Canada);

			var orgMX = Factory.New<OrgHeader>();
			orgMX.OH_FullName = "MXIMPORTER";
			orgMX.OH_RL_NKClosestPort = "MXLMX";
			var mxImporter = orgMX.MainAddress;
			mxImporter.OA_Address1 = "MX ADDRESS 1";
			mxImporter.OA_Address2 = "MX ADDRESS 2";
			mxImporter.State = "MM";
			mxImporter.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Mexico;
			mxImporter.OA_RL_NKRelatedPortCode = "MXLMX";
			mxImporter.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.CBPAssignedNumber, "32-843977", Core.Constants.CountryCodes.Mexico);

			var orgIT = Factory.New<OrgHeader>();
			orgIT.OH_Code = "ITRO";
			orgIT.OH_FullName = "IT APHIS Grower";
			orgIT.OH_RL_NKClosestPort = "ITROM";
			var itImporter = orgIT.MainAddress;
			itImporter = orgIT.MainAddress;
			itImporter.State = "IT";
			itImporter.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			itImporter.OA_Address1 = "IT ADDRESS 1";
			itImporter.OA_Address2 = "IT ADDRESS 2";
			itImporter.OA_RL_NKRelatedPortCode = "ITROM";

			// PG19, PG20 and PG21
			header.US_OA_CropGrowerAddress = itImporter.PK;
			declaration.JE_OH_Importer = caImporter.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = mxImporter.PK;
			declaration.Branch.GB_OH_OrgProxy = caImporter.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";

			//PG26
			header.US_Qty2 = 2400m;
			header.US_UQ2 = APHISUnitOfMeasureList.Codes.StemsOfCutFlowers;
			header.US_Qty1 = 100m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.Box;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "5206";
			inspection.US_Date = new ZDateTime(2014, 12, 5);

			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Colombia;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000                                              
OI        SNAPDRAGONS                                                           
PG01001APHAPQA01 Y                       130.000                                
PG02PUNS 93010600           SKU 00034285564                                     
PG05ANTIRRHINUM           SP                                                    
PG06262CO                                                                       
PG10AP0800801  A80 SGFL                                                         
PG17SNAPDRAGONS                                                                 
PG19DFI                  IT APHIS GROWER                 IT ADDRESS 1           
PG20IT ADDRESS 2                                                 IT             
PG19UC                   MXIMPORTER                      MX ADDRESS 1           
PG20MX ADDRESS 2                                              MM MX             
PG19CB 336XJ5            CAIMPORTER                      1234 PEACHTREE STREET  
PG20ANNE OF GREENGABLES                                       PE CA30301        
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG262000000240000STM                                                            
PG261000000010000BX                                                             
PG30A12052014    2   5206                                                       
PG32198CO                                                                       
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestCutFlowersAndGreenery_RoseBouquet()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "0603.11.0060";
			invoiceLine.JI_Description = "FRESH ROSES";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			header.ApplicantOrgPK = ZGuid.Empty;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForConsumerUseNonFoodProduct;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			header.US_CategoryCode = CutFlowersAndGreeneryList.Codes.CutFlowers;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode;
			header.US_ProductNumber = "103000000";
			header.US_StockKeepingUnitNumber = "00034275564";
			// PG05
			header.US_ScientificGenusName = "ROSA";
			header.US_ScientificSpeciesName = "SP";
			// PG06
			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.PlaceOfGrowth;
			source.US_CountryCode = Core.Constants.CountryCodes.Colombia;

			// PG07
			header.US_BouquetGroupingNumber = "1";

			// PG10
			header.US_ProductCondition = CutFlowersAndGreeneryTypeList.Codes.RoseBouquet;

			// PG17
			header.US_CommoditySpecificName = "ROSES";

			// PG19, PG20 and PG21
			header.US_OA_CropGrowerAddress = CropGrower.PK;
			declaration.JE_OH_Importer = Importer.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";

			//PG26
			header.US_Qty1 = 100m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.Box;
			header.US_Qty2 = 200;
			header.US_UQ2 = APHISUnitOfMeasureList.Codes.BouquetOfCutFlowers;
			header.US_Qty3 = 2400m;
			header.US_UQ3 = APHISUnitOfMeasureList.Codes.StemsOfCutFlowers;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "5206";
			inspection.US_Date = new ZDateTime(2014, 12, 05);

			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Colombia;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"OI        FRESH ROSES                                                           
PG01001APHAPQA01 Y                       130.000                                
PG02PUNS 103000000          SKU 00034275564                                     
PG05ROSA                  SP                                                    
PG06262CO                                                                       
PG07                                                        BQG1                
PG10AP0800801  A80 BROS                                                         
PG17ROSES                                                                       
PG19DFI                  CROP GROWER                     CG ADDRESS 1           
PG20CG ADDRESS 2                                                 AU             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG263000000240000STM                                                            
PG262000000020000BQT                                                            
PG261000000010000BX                                                             
PG30A12052014    2   5206                                                       
PG32198CO                                                                       ";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestCutFlowersAndGreenery_PompomBouquet()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "0603.14.0010";
			invoiceLine.JI_Description = "FRESH POM POM";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			header.ApplicantOrgPK = ZGuid.Empty;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForConsumerUseNonFoodProduct;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			header.US_CategoryCode = CutFlowersAndGreeneryList.Codes.CutFlowers;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode;
			header.US_ProductNumber = "10331600";
			header.US_StockKeepingUnitNumber = "00034275564";
			// PG05
			header.US_ScientificGenusName = "CHRYSANTHEM";
			header.US_ScientificSpeciesName = "MORIFOLIUM";
			// PG06
			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.PlaceOfGrowth;
			source.US_CountryCode = Core.Constants.CountryCodes.Colombia;

			// PG07
			header.US_BouquetGroupingNumber = "1";

			// PG10
			header.US_ProductCondition = CutFlowersAndGreeneryTypeList.Codes.RoseBouquet;

			// PG13 and PG14
			var license = header.Licenses.AddNew();
			// PG13
			license.US_RN_CountryCode = Core.Constants.CountryCodes.Colombia;
			// PG14
			license.US_Type = APHISLicenseTypeList.Codes.PhytosanitaryCertificate;
			license.US_Number = "PERMITNUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2014, 12, 5);

			// PG17
			header.US_CommoditySpecificName = "POM POM";

			// PG19, PG20 and PG21
			header.US_OA_CropGrowerAddress = CropGrower.PK;
			declaration.JE_OH_Importer = Importer.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";

			//PG26
			header.US_Qty1 = 1200m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.StemsOfCutFlowers;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "5206";
			inspection.US_Date = new ZDateTime(2014, 12, 5);

			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Colombia;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000 000000000000NO                               
OI        FRESH POM POM                                                         
PG01001APHAPQA01 Y                       130.000                                
PG02PUNS 10331600           SKU 00034275564                                     
PG05CHRYSANTHEM           MORIFOLIUM                                            
PG06262CO                                                                       
PG07                                                        BQG1                
PG10AP0800801  A80 BROS                                                         
PG13                                   ISOCO                                    
PG14 A01PERMITNUMBER                     312052014                              
PG17POM POM                                                                     
PG19DFI                  CROP GROWER                     CG ADDRESS 1           
PG20CG ADDRESS 2                                                 AU             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG261000000120000STM                                                            
PG30A12052014    2   5206                                                       
PG32198CO                                                                       
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestCutFlowersAndGreenery_LilyBouquet()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "0603.19.0110";
			invoiceLine.JI_Description = "FRESH ALSTROEMERIA";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			header.ApplicantOrgPK = ZGuid.Empty;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForConsumerUseNonFoodProduct;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			header.US_CategoryCode = CutFlowersAndGreeneryList.Codes.CutFlowers;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode;
			header.US_ProductNumber = "10311700";
			header.US_StockKeepingUnitNumber = "00034275564";
			// PG05
			header.US_ScientificGenusName = "ALSTROEMERIA";
			header.US_ScientificSpeciesName = "SP";
			// PG06
			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.PlaceOfGrowth;
			source.US_CountryCode = Core.Constants.CountryCodes.Colombia;

			// PG07
			header.US_BouquetGroupingNumber = "1";

			// PG10
			header.US_ProductCondition = CutFlowersAndGreeneryTypeList.Codes.RoseBouquet;

			// PG17
			header.US_CommoditySpecificName = "LILY";

			// PG19, PG20 and PG21
			header.US_OA_CropGrowerAddress = CropGrower.PK;
			declaration.JE_OH_Importer = Importer.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";

			//PG26
			header.US_Qty1 = 1200m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.StemsOfCutFlowers;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "5206";
			inspection.US_Date = new ZDateTime(2014, 12, 5);

			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Colombia;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"OI        FRESH ALSTROEMERIA                                                    
PG01001APHAPQA01 Y                       130.000                                
PG02PUNS 10311700           SKU 00034275564                                     
PG05ALSTROEMERIA          SP                                                    
PG06262CO                                                                       
PG07                                                        BQG1                
PG10AP0800801  A80 BROS                                                         
PG17LILY                                                                        
PG19DFI                  CROP GROWER                     CG ADDRESS 1           
PG20CG ADDRESS 2                                                 AU             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG261000000120000STM                                                            
PG30A12052014    2   5206                                                       
PG32198CO                                                                       ";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestCutFlowersAndGreenery_Snapdragons()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "Snapdragons";
			invoiceLine.JI_Description = "SNAPDRAGONS";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			header.ApplicantOrgPK = ZGuid.Empty;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForConsumerUseNonFoodProduct;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			header.US_CategoryCode = CutFlowersAndGreeneryList.Codes.CutFlowers;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode;
			header.US_ProductNumber = "93010600";
			header.US_StockKeepingUnitNumber = "00034285564";
			// PG05
			header.US_ScientificGenusName = "ANTIRRHINUM";
			header.US_ScientificSpeciesName = "SP";
			// PG06
			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.PlaceOfGrowth;
			source.US_CountryCode = Core.Constants.CountryCodes.Colombia;

			// PG10
			header.US_ProductCondition = CutFlowersAndGreeneryTypeList.Codes.SingleGenusOfFlower;

			// PG17
			header.US_CommoditySpecificName = "SNAPDRAGONS";

			// PG19, PG20 and PG21
			header.US_OA_CropGrowerAddress = CropGrower.PK;
			declaration.JE_OH_Importer = Importer.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";

			//PG26
			header.US_Qty2 = 2400m;
			header.US_UQ2 = APHISUnitOfMeasureList.Codes.StemsOfCutFlowers;
			header.US_Qty1 = 100m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.Box;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "5206";
			inspection.US_Date = new ZDateTime(2014, 12, 5);

			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Colombia;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000                                              
OI        SNAPDRAGONS                                                           
PG01001APHAPQA01 Y                       130.000                                
PG02PUNS 93010600           SKU 00034285564                                     
PG05ANTIRRHINUM           SP                                                    
PG06262CO                                                                       
PG10AP0800801  A80 SGFL                                                         
PG17SNAPDRAGONS                                                                 
PG19DFI                  CROP GROWER                     CG ADDRESS 1           
PG20CG ADDRESS 2                                                 AU             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG262000000240000STM                                                            
PG261000000010000BX                                                             
PG30A12052014    2   5206                                                       
PG32198CO                                                                       
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestGeneticallyEngineeredOrganisms_PomaceFly()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "0106.49.0090";
			invoiceLine.JI_Description = "POMACE FLY";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.ABS;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.APHISPlantInspectionStation;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForResearchDevelopmentNonFoodProduct;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms;
			header.US_CategoryCode = GeneticallyEngineeredOrganismsList.Codes.Insect;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.TaxonomicSerialNumber;
			header.US_ProductNumber = "146290";
			// PG05
			header.US_ScientificGenusName = "DROSOHILA";
			header.US_ScientificSpeciesName = "MELANOGASTER";

			// PG10
			header.US_ProductComponent = GeneticallyEngineeredOrganismsIntergenericA100List.Codes.NotIntergeneric;
			header.US_ProductCondition = GeneticallyEngineeredOrganismsTypeA101List.Codes.RecipientOrganism;
			header.US_ProductPhysicalState = GeneticallyEngineeredOrganismsLifeStageA102List.Codes.InvertebrateAnimalsAdults;

			var license = header.Licenses.AddNew();
			// PG13
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			// PG14
			license.US_Type = APHISLicenseTypeList.Codes.AphisPpq203;
			license.US_Number = "PERMITNUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.ExpirationDate;
			license.US_Date = new ZDateTime(2017, 2, 3);
			license.US_Quantity = 4m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Pieces;

			// PG17
			header.US_CommoditySpecificName = "POMACEFLY";

			// PG19, PG20 and PG21
			header.US_OA_ApplicantAddress = PermitHolder.PK;
			header.US_OA_CropGrowerAddress = CropGrower.PK;
			declaration.JE_OH_Importer = Importer.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";

			//PG26
			header.US_Qty1 = 4m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.NumberCount;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "4197";
			inspection.US_Date = new ZDateTime(2014, 12, 5);

			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Germany;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"OI        POMACE FLY                                                            
PG01001APHABSA02 Y                       180.000                                
PG02PTSN 146290                                                                 
PG05DROSOHILA             MELANOGASTER                                          
PG10AP10001004 A100N                                                            
PG10AP10001004 A101ROR                                                          
PG10AP10001004 A102IAD                                                          
PG13                                   ISOUS                                    
PG14 A07PERMITNUMBER                     1020320170000000000040000PCS           
PG17POMACEFLY                                                                   
PG19LAP33332KD443        PERMIT HOLDER                   PH ADDRESS 1           
PG20PH ADDRESS 2                                              NY US             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG261000000000400NO                                                             
PG30A12052014    2   4197                                                       
PG32198DE                                                                       
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestContainsReMarks()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "0106.49.0090";
			invoiceLine.JI_Description = "POMACE FLY";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			// PG01
			header.US_ProgramType = APHISProgramCodeList.Codes.ABS;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.APHISPlantInspectionStation;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForResearchDevelopmentNonFoodProduct;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms;
			header.US_CategoryCode = GeneticallyEngineeredOrganismsList.Codes.Insect;
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.TaxonomicSerialNumber;
			header.US_ProductNumber = "146290";
			// PG05
			header.US_ScientificGenusName = "DROSOHILA";
			header.US_ScientificSpeciesName = "MELANOGASTER";

			// PG10
			header.US_ProductComponent = GeneticallyEngineeredOrganismsIntergenericA100List.Codes.NotIntergeneric;
			header.US_ProductCondition = GeneticallyEngineeredOrganismsTypeA101List.Codes.RecipientOrganism;
			header.US_ProductPhysicalState = GeneticallyEngineeredOrganismsLifeStageA102List.Codes.InvertebrateAnimalsAdults;

			var license = header.Licenses.AddNew();
			// PG13
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			// PG14
			license.US_Type = APHISLicenseTypeList.Codes.AphisPpq203;
			license.US_Number = "PERMITNUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.ExpirationDate;
			license.US_Date = new ZDateTime(2017, 2, 3);
			license.US_Quantity = 4m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Pieces;

			// PG17
			header.US_CommoditySpecificName = "POMACEFLY";

			// PG19, PG20 and PG21
			header.US_OA_ApplicantAddress = PermitHolder.PK;
			header.US_OA_CropGrowerAddress = CropGrower.PK;
			declaration.JE_OH_Importer = Importer.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;
			declaration.Branch.GB_OH_OrgProxy = CustomsBroker.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";

			//PG24
			header.US_ReMarks = "TST";

			//PG26
			header.US_Qty1 = 4m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.NumberCount;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "4197";
			inspection.US_Date = new ZDateTime(2014, 12, 5);

			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Germany;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"OI        POMACE FLY                                                            
PG01001APHABSA02 Y                       180.000                                
PG02PTSN 146290                                                                 
PG05DROSOHILA             MELANOGASTER                                          
PG10AP10001004 A100N                                                            
PG10AP10001004 A101ROR                                                          
PG10AP10001004 A102IAD                                                          
PG13                                   ISOUS                                    
PG14 A07PERMITNUMBER                     1020320170000000000040000PCS           
PG17POMACEFLY                                                                   
PG19LAP33332KD443        PERMIT HOLDER                   PH ADDRESS 1           
PG20PH ADDRESS 2                                              NY US             
PG19UC 34832-23-234232   ULTIMATE CONSIGNEE              UC ADDRESS 1           
PG20UC ADDRESS 2                                              IL US             
PG19CB 336XJ5            CUSTOMS BROKER                  CB ADDRESS 1           
PG20CB ADDRESS 2                                              CA US             
PG21CB BOB SMITH              6301023498     BOB@WHERE.COM                      
PG24        TST                                                                 
PG261000000000400NO                                                             
PG30A12052014    2   4197                                                       
PG32198DE                                                                       
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including APHIS data", expectedMessage, message.EM_FormattedMessageText);
		}

		protected override void SetUpData()
		{
			base.SetUpData();

			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();

			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.US_CertifyCargoRelease = true;

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
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			header = invoiceLine.APHISHeaders.AddNew();
		}

		OrgAddress customsBroker;
		OrgAddress CustomsBroker
		{
			get
			{
				if (customsBroker == null)
				{
					var org = Factory.New<OrgHeader>();
					org.OH_FullName = "CUSTOMS BROKER";
					org.OH_RL_NKClosestPort = "USLAX";
					customsBroker = org.MainAddress;
					customsBroker.OA_Address1 = "CB ADDRESS 1";
					customsBroker.OA_Address2 = "CB ADDRESS 2";
					customsBroker.OA_RL_NKRelatedPortCode = "USLAX";
					customsBroker.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.CBPAssignedNumber, "32-323422", Core.Constants.CountryCodes.UnitedStates);
				}
				return customsBroker;
			}
		}

		OrgAddress importer;
		OrgAddress Importer
		{
			get
			{
				if (importer == null)
				{
					var org = Factory.New<OrgHeader>();
					org.OH_FullName = "IMPORTER";
					org.OH_RL_NKClosestPort = "USLAX";
					importer = org.MainAddress;
					importer.OA_Address1 = "IM ADDRESS 1";
					importer.OA_Address2 = "IM ADDRESS 2";
					importer.OA_RL_NKRelatedPortCode = "USLAX";
					importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.CBPAssignedNumber, "32-843933", Core.Constants.CountryCodes.UnitedStates);
				}
				return importer;
			}
		}

		OrgAddress permitHolder;
		OrgAddress PermitHolder
		{
			get
			{
				if (permitHolder == null)
				{
					var org = Factory.New<OrgHeader>();
					org.OH_FullName = "PERMIT HOLDER";
					org.OH_RL_NKClosestPort = "USNYC";
					permitHolder = org.MainAddress;
					permitHolder.OA_Address1 = "PH ADDRESS 1";
					permitHolder.OA_Address2 = "PH ADDRESS 2";
					permitHolder.OA_RL_NKRelatedPortCode = "USNYC";
					permitHolder.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.APHISAssignedNumber, "32KD443", Core.Constants.CountryCodes.UnitedStates);
				}
				return permitHolder;
			}
		}

		OrgAddress ultimateConsignee;
		OrgAddress UltimateConsignee
		{
			get
			{
				if (ultimateConsignee == null)
				{
					var org = Factory.New<OrgHeader>();
					org.OH_FullName = "ULTIMATE CONSIGNEE";
					org.OH_RL_NKClosestPort = "USCHI";
					ultimateConsignee = org.MainAddress;
					ultimateConsignee.OA_Address1 = "UC ADDRESS 1";
					ultimateConsignee.OA_Address2 = "UC ADDRESS 2";
					ultimateConsignee.OA_RL_NKRelatedPortCode = "USCHI";
					ultimateConsignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "32-23-234232", Core.Constants.CountryCodes.UnitedStates);
				}
				return ultimateConsignee;
			}
		}

		OrgAddress cropGrower;
		OrgAddress CropGrower
		{
			get
			{
				if (cropGrower == null)
				{
					var org = Factory.New<OrgHeader>();
					org.OH_FullName = "CROP GROWER";
					org.OH_RL_NKClosestPort = "AUSYD";
					cropGrower = org.MainAddress;
					cropGrower.OA_Address1 = "CG ADDRESS 1";
					cropGrower.OA_Address2 = "CG ADDRESS 2";
					cropGrower.OA_RL_NKRelatedPortCode = "AUSYD";
				}
				return cropGrower;
			}
		}

		APHISHeader header;
	}
}
