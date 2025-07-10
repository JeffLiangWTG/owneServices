using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using APHISArticleCategory = Enterprise.Customs.US.Business.APHIS.ArticleCategory;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForDEATest : PGABlocksCreatorTest
	{
		public void TestDEAForDisclaimed()
		{
			SetUpData();
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_DEADisclaimReason = "A";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains("ACE Entry Summary message including DEA data",
@"OI                                                                              
PG01001DEADEA                                                                  A", message.EM_FormattedMessageText);
		}

		public void TestDEA_236FormWithOneProductContainingOneComponent()
		{
			SetUpData();
			declaration.US_FDAADTA = new ZDateTime(2015, 11, 9);
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			var deaHeader = invoiceLine.DEAHeaders.AddNew();
			deaHeader.US_CountryOfShipment = "CN";
			deaHeader.US_PermitNumber = "1QQSOU4";
			deaHeader.US_RegistrationNumber = "RD0445355";
			deaHeader.US_FormID = "DEA-236";
			var constituent = deaHeader.Constituents.AddNew();
			constituent.US_ProductCode = "4000";
			constituent.US_Weight = 114400m;
			constituent.US_WeightUQ = "G";
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains("ACE Entry Summary message including DEA data",
@"OI                                                                              
PG01001DEADEA    Y                                                              
PG02P                                                                           
PG02CCSA 4000                                                                   
PG04                                                    000011440000G           
PG06CSHCN                                                                       
PG141   1QQSOU4                                                                 
PG19LAP164RD0445355                                                             
PG22 921                                                                        
PG30A11092015                                                                   ", message.EM_FormattedMessageText);
		}

		public void TestDEA_486AFormWithTwoProductsContainingTheSameComponent()
		{
			SetUpData();
			declaration.US_FDAADTA = new ZDateTime(2015, 10, 17);
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			var deaHeader = invoiceLine.DEAHeaders.AddNew();
			deaHeader.US_CountryOfShipment = "IN";
			deaHeader.US_PermitNumber = "Y27TFG7";
			deaHeader.US_RegistrationNumber = "RD0445355";
			deaHeader.US_FormID = "DEA-486A";
			var constituent1 = deaHeader.Constituents.AddNew();
			constituent1.US_ProductCode = "8112";
			constituent1.US_Weight = 9780.96m;
			constituent1.US_WeightUQ = "G";
			var constituent2 = deaHeader.Constituents.AddNew();
			constituent2.US_ProductCode = "8112";
			constituent2.US_Weight = 3911.40m;
			constituent2.US_WeightUQ = "G";
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains("ACE Entry Summary message including DEA data",
@"OI                                                                              
PG01001DEADEA    Y                                                              
PG02P                                                                           
PG02CCSA 8112                                                                   
PG04                                                    000000978096G           
PG02CCSA 8112                                                                   
PG04                                                    000000391140G           
PG06CSHIN                                                                       
PG141   Y27TFG7                                                                 
PG19LAP164RD0445355                                                             
PG22 923                                                                        
PG30A10172015                                                                   ", message.EM_FormattedMessageText);
		}

		public void TestDEA_35FormWithTwoProductsContainingTheSameComponent()
		{
			SetUpData();
			declaration.US_FDAADTA = new ZDateTime(2015, 10, 12);
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			var deaHeader = invoiceLine.DEAHeaders.AddNew();
			deaHeader.US_CountryOfShipment = "UK";
			deaHeader.US_PermitNumber = "FU77R3A";
			deaHeader.US_RegistrationNumber = "RD0445355";
			deaHeader.US_FormID = "DEA-35";
			var constituent1 = deaHeader.Constituents.AddNew();
			constituent1.US_ProductCode = "9064";
			constituent1.US_Weight = 46500m;
			constituent1.US_WeightUQ = "G";
			var constituent2 = deaHeader.Constituents.AddNew();
			constituent2.US_ProductCode = "9064";
			constituent2.US_Weight = 12090m;
			constituent2.US_WeightUQ = "G";
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains("ACE Entry Summary message including DEA data",
@"OI                                                                              
PG01001DEADEA    Y                                                              
PG02P                                                                           
PG02CCSA 9064                                                                   
PG04                                                    000004650000G           
PG02CCSA 9064                                                                   
PG04                                                    000001209000G           
PG06CSHUK                                                                       
PG141   FU77R3A                                                                 
PG19LAP164RD0445355                                                             
PG22 911                                                                        
PG30A10122015                                                                   ", message.EM_FormattedMessageText);
		}

		public void TestMessageLAPPartyWithAthorisedLicenseHolder()
		{
			SetUpData();

			var ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.OH_Code = "TESTCNE";
			ultimateConsignee.OH_FullName = "ULTIMATE CONSIGNEE";
			ultimateConsignee.OH_RL_NKClosestPort = "USCHI";
			var ultimateConsigneeAddress = ultimateConsignee.MainAddress;
			ultimateConsigneeAddress.OA_Address1 = "UC ADDRESS 1";
			ultimateConsigneeAddress.OA_Address2 = "UC ADDRESS 2";
			ultimateConsigneeAddress.OA_RL_NKRelatedPortCode = "USCHI";
			ultimateConsigneeAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "32-23-234232", Core.Constants.CountryCodes.UnitedStates);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTIMP";
			importer.OH_FullName = "IMPORTER";
			importer.OH_RL_NKClosestPort = "USLAX";
			var importerAddress = importer.MainAddress;
			importerAddress.OA_Address1 = "IM ADDRESS 1";
			importerAddress.OA_Address2 = "IM ADDRESS 2";
			importerAddress.OA_RL_NKRelatedPortCode = "USLAX";
			importerAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.CBPAssignedNumber, "32-843933", Core.Constants.CountryCodes.UnitedStates);

			var customsBroker = Factory.New<OrgHeader>();
			customsBroker.OH_FullName = "CUSTOMS BROKER";
			customsBroker.OH_RL_NKClosestPort = "USLAX";
			var customsBrokerAddress = customsBroker.MainAddress;
			customsBrokerAddress.OA_Address1 = "CB ADDRESS 1";
			customsBrokerAddress.OA_Address2 = "CB ADDRESS 2";
			customsBrokerAddress.OA_RL_NKRelatedPortCode = "USLAX";
			customsBrokerAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.CBPAssignedNumber, "32-323422", Core.Constants.CountryCodes.UnitedStates);

			var cropGrower = Factory.New<OrgHeader>();
			cropGrower.OH_Code = "TESTCROP";
			cropGrower.OH_FullName = "CROP GROWER";
			cropGrower.OH_RL_NKClosestPort = "AUSYD";
			var cropGrowerAddress = cropGrower.MainAddress;
			cropGrowerAddress.OA_Address1 = "CG ADDRESS 1";
			cropGrowerAddress.OA_Address2 = "CG ADDRESS 2";
			cropGrowerAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			invoiceLine.JI_Tariff = "9808.00.6000";
			invoiceLine.JI_Description = "SOIL";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;

			var aphisLine = invoiceLine.APHISHeaders.AddNew();
			aphisLine.US_OA_ApplicantAddress = PermitHolder.PK;

			// PG01
			aphisLine.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			aphisLine.Inspections.RemoveAndDeleteAll();
			aphisLine.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			aphisLine.US_IntendedUseCode = IntendedUseCodesList.Codes.ForResearchDevelopmentNonFoodProduct;
			aphisLine.US_CategoryType = APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts;
			aphisLine.US_CategoryCode = APHISArticleCategory.MiscellaneousAndProcessedProductsList.Codes.SoilRocksAndGarbage;
			// PG02
			aphisLine.US_ProductType = ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode;
			aphisLine.US_ProductNumber = "11111501";

			// PG10
			aphisLine.US_ProductPhysicalState = APHIS.CommodityCharacteristicQualifier.MiscellaneousAndProcessedProductsPhysicalStateList.Codes.Samples;
			// PG13 and PG14
			var license1 = aphisLine.Licenses.AddNew();

			// PG13
			license1.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			// PG14
			license1.US_Type = APHISLicenseTypeList.Codes.AphisPpq525b;
			license1.US_Number = "PERMITNUMBER";
			license1.US_DateQualifier = LPCODateQualifierList.Codes.ExpirationDate;
			license1.US_Date = new ZDateTime(2017, 6, 24);
			license1.US_Quantity = 3m;
			license1.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Pounds;

			// PG19, PG20 and PG21
			aphisLine.US_OA_ApplicantAddress = PermitHolder.PK;
			aphisLine.US_OA_CropGrowerAddress = cropGrowerAddress.PK;
			declaration.JE_OH_Importer = importerAddress.OA_OH;
			invoiceLine.JI_OA_ConsigneeAddress = ultimateConsigneeAddress.PK;
			declaration.Branch.GB_OH_OrgProxy = customsBrokerAddress.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";
			//PG26
			aphisLine.US_Qty1 = 3m;
			aphisLine.US_UQ1 = APHISUnitOfMeasureList.Codes.PoundsAvdpWeight;

			// PG30
			var inspection = aphisLine.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			inspection.US_Location = "4197";
			inspection.US_Date = new ZDateTime(2014, 12, 5);

			// PG32
			var routing = aphisLine.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Malaysia;

			var action = GetAction(declaration);
			action.US_DateOfDeclaration = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"OI        SOIL                                                                  
PG01001APHAPQA01 Y                       180.000                                
PG02PUNS 11111501                                                               
PG10AP0700721  A71 SAM                                                          
PG13                                   ISOUS                                    
PG14 A09PERMITNUMBER                     1062420170000000000030000LB            
PG19LAP33332PH443        PERMIT HOLDER                   PH ADDRESS 1           
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
			AssertContains("ACE Entry Summary message with AuthorizedLicenseHolder", expectedMessage, message.EM_FormattedMessageText);
		}

		OrgAddress permitHolder;
		OrgAddress PermitHolder
		{
			get
			{
				if (permitHolder == null)
				{
					var org = Factory.New<OrgHeader>();
					org.OH_Code = "TESTPERMIT";
					org.OH_FullName = "PERMIT HOLDER";
					org.OH_RL_NKClosestPort = "USNYC";
					permitHolder = org.MainAddress;
					permitHolder.OA_Address1 = "PH ADDRESS 1";
					permitHolder.OA_Address2 = "PH ADDRESS 2";
					permitHolder.OA_RL_NKRelatedPortCode = "USNYC";
					permitHolder.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.APHISAssignedNumber, "32PH443", Core.Constants.CountryCodes.UnitedStates);
				}
				return permitHolder;
			}
		}
	}
}
