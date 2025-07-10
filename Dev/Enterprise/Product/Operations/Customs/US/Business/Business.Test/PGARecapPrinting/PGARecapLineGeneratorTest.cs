using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using APHISArticleCategory = Enterprise.Customs.US.Business.APHIS.ArticleCategory;
using APHISCommodityCharacteristicQualifier = Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier;

namespace Enterprise.Customs.US.Business.PGARecapPrinting.Testing
{
	sealed class PGARecapLineGeneratorTest : TestCaseWithFactory
	{
		public void TestGenerateLines()
		{
			var testProduct = Factory.New<OrgSupplierPart>();
			testProduct.OP_PartNum = "TESTPRODUCT";

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "TV";
			broker.GS_FullName = "BROKER";
			broker.GS_WorkPhone = "04 010101";
			broker.GS_EmailAddress = "BROKER EMAIL";
			broker.GS_FaxNum = "BROKER FAX";

			var ior = Factory.New<OrgHeader>();
			ior.OH_FullName = "IMPORTER OF RECORD";
			var iorAddress = ior.MainAddress;
			iorAddress.OA_Address1 = "IOR ADDRESS 1";
			iorAddress.OA_Address2 = "IOR ADDRESS 2";
			iorAddress.OA_City = "SYDNEY";
			iorAddress.OA_State = "NSW";
			iorAddress.OA_PostCode = "2017";

			DeclarationTestHelper.AddPGAContact(ior, "IOR", "ALEXANDER THE GREATEST OF ALL", "04 123456", "IOR EMAIL", "IOR FAX");
			var customsBroker = Factory.New<OrgHeader>();
			customsBroker.OH_FullName = "CUSTOMS BROKER";
			customsBroker.OH_RL_NKClosestPort = "USLAX";
			var customsBrokerAddress = customsBroker.MainAddress;
			customsBrokerAddress.OA_Address1 = "CB ADDRESS 1";
			customsBrokerAddress.OA_Address2 = "CB ADDRESS 2";
			customsBrokerAddress.OA_RL_NKRelatedPortCode = "USLAX";
			customsBrokerAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.CBPAssignedNumber, "32-323422", Core.Constants.CountryCodes.UnitedStates);

			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER";
			importer.OH_RL_NKClosestPort = "USLAX";
			var importerAddress = importer.MainAddress;
			importerAddress.OA_Address1 = "IM ADDRESS 1";
			importerAddress.OA_Address2 = "IM ADDRESS 2";
			importerAddress.OA_RL_NKRelatedPortCode = "USLAX";
			importerAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.CBPAssignedNumber, "32-843933", Core.Constants.CountryCodes.UnitedStates);

			var permitHolder = Factory.New<OrgHeader>();
			permitHolder.OH_FullName = "PERMIT HOLDER";
			permitHolder.OH_RL_NKClosestPort = "USNYC";
			var permitHolderAddress = permitHolder.MainAddress;
			permitHolderAddress.OA_Address1 = "PH ADDRESS 1";
			permitHolderAddress.OA_Address2 = "PH ADDRESS 2";
			permitHolderAddress.OA_RL_NKRelatedPortCode = "USNYC";
			permitHolderAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.APHISAssignedNumber, "32KD443", Core.Constants.CountryCodes.UnitedStates);

			var ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.OH_FullName = "ULTIMATE CONSIGNEE";
			ultimateConsignee.OH_RL_NKClosestPort = "USCHI";
			var ultimateConsigneeAddress = ultimateConsignee.MainAddress;
			ultimateConsigneeAddress.OA_Address1 = "UC ADDRESS 1";
			ultimateConsigneeAddress.OA_Address2 = "UC ADDRESS 2";
			ultimateConsigneeAddress.OA_RL_NKRelatedPortCode = "USCHI";
			ultimateConsigneeAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "32-23-234232", Core.Constants.CountryCodes.UnitedStates);

			var cropGrower = Factory.New<OrgHeader>();
			cropGrower.OH_FullName = "CROP GROWER";
			cropGrower.OH_RL_NKClosestPort = "AUSYD";
			var cropGrowerAddress = cropGrower.MainAddress;
			cropGrowerAddress.OA_Address1 = "CG ADDRESS 1";
			cropGrowerAddress.OA_Address2 = "CG ADDRESS 2";
			cropGrowerAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = "FWS EXPORTER";
			exporter.OH_RL_NKClosestPort = "AUSYD";
			var exporterAddress = exporter.MainAddress;
			exporterAddress.OA_Address1 = "EXP ADDRESS 1";
			exporterAddress.OA_Address2 = "EXP ADDRESS 2";
			exporterAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			exporterAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUNS87433", Core.Constants.CountryCodes.UnitedStates);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_DateOfArrival = new ZDateTime(2016, 1, 1);
			declaration.US_CertifyCargoRelease = true;
			declaration.US_FDAContactEmail = "BOB@EMAIL.COM";
			declaration.US_FDAContactName = "CONTACT BOB";
			declaration.US_FDAContactPhoneNo = "6013942234";
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			declaration.IOROrgPK = ior.PK;

			var orgProxy = declaration.Branch.OrgProxy;
			UpdateAddress(orgProxy.MainAddress, "Proxy Address Line 1", "Proxy Address Line 2", "Chicago City", "CH", "86954-3251", "+1 641 8564 8653", "+1 641 8564 8654", "info@proxy.com");
			orgProxy.MainAddress.OA_RL_NKRelatedPortCode = "US2CW";
			var customsAddress = AddCustomsAddress(orgProxy, "Proxy Customs Address Line 1", "Proxy Customs Address Line 2", "Michigan City", "IL", "596508654", "+1 642 8564 8653", "+1 642 8564 8654", "customs@proxy.com");
			var contact = orgProxy.Contacts.AddNew();
			contact.OC_ContactName = "BOB THE BUILDER";
			contact.OC_Email = "BOB@WHERE.COM";

			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true);
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;
			invoiceLine1.JI_OA_ExporterAddress = ior.MainAddress.PK;
			invoiceLine1.JI_Tariff = "0106199120";
			invoiceLine1.JI_Description = "LIVE DOGS";
			invoiceLine1.JI_PartNo = "InexistingProduct";

			#region APHIS
			invoiceLine1.US_APHISInd = OGAIndicatorList.Codes.Declared;
			var aphisHeader = invoiceLine1.APHISHeaders.AddNew();

			// PG01
			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AAC;
			aphisHeader.Inspections.RemoveAndDeleteAll();
			aphisHeader.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.APHISVSPortVeterinarian;
			aphisHeader.US_IntendedUseCode = IntendedUseCodesList.Codes.AnimalOrPlantForEducationalUse;
			aphisHeader.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			aphisHeader.US_CategoryCode = APHISArticleCategory.LiveAnimalsList.Codes.CanidaeDogs;
			// PG02
			aphisHeader.US_ProductType = ProductCodeQualifiersList.Codes.TaxonomicSerialNumber;
			aphisHeader.US_ProductNumber = "726821";
			// PG10
			var product = aphisHeader.Products.AddNew();
			product.US_Gender = APHISCommodityCharacteristicQualifier.LiveAnimalsGenderList.Codes.Male;
			product.US_BreedVariety = APHISCommodityCharacteristicQualifier.LiveAnimalsDogList.Codes.BulldogDog;
			product.US_Color = APHISCommodityCharacteristicQualifier.LiveAnimalsColorList.Codes.Various;
			product.US_Age = APHISCommodityCharacteristicQualifier.LiveAnimalsAgeList.Codes._6Months;
			// PG07
			var identity = product.Identities.AddNew();
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			identity.CY_Data = "74CFF1A";

			// PG10
			product = aphisHeader.Products.AddNew();
			product.US_Gender = APHISCommodityCharacteristicQualifier.LiveAnimalsGenderList.Codes.Male;
			product.US_BreedVariety = APHISCommodityCharacteristicQualifier.LiveAnimalsDogList.Codes.BulldogDog;
			product.US_Color = APHISCommodityCharacteristicQualifier.LiveAnimalsColorList.Codes.Various;
			product.US_Age = APHISCommodityCharacteristicQualifier.LiveAnimalsAgeList.Codes._6Months;
			// PG07
			identity = product.Identities.AddNew();
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			identity.CY_Data = "6CA4B1B";

			// PG10
			product = aphisHeader.Products.AddNew();
			product.US_Gender = APHISCommodityCharacteristicQualifier.LiveAnimalsGenderList.Codes.Male;
			product.US_BreedVariety = APHISCommodityCharacteristicQualifier.LiveAnimalsDogList.Codes.BulldogDog;
			product.US_Color = APHISCommodityCharacteristicQualifier.LiveAnimalsColorList.Codes.Various;
			product.US_Age = APHISCommodityCharacteristicQualifier.LiveAnimalsAgeList.Codes._6Months;
			// PG07
			identity = product.Identities.AddNew();
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			identity.CY_Data = "6CA3988";

			// PG10
			product = aphisHeader.Products.AddNew();
			product.US_Gender = APHISCommodityCharacteristicQualifier.LiveAnimalsGenderList.Codes.Female;
			product.US_BreedVariety = APHISCommodityCharacteristicQualifier.LiveAnimalsDogList.Codes.BulldogDog;
			product.US_Color = APHISCommodityCharacteristicQualifier.LiveAnimalsColorList.Codes.Various;
			product.US_Age = APHISCommodityCharacteristicQualifier.LiveAnimalsAgeList.Codes._6Months;
			// PG07
			identity = product.Identities.AddNew();
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			identity.CY_Data = "6CA36D8";

			// PG10
			product = aphisHeader.Products.AddNew();
			product.US_Gender = APHISCommodityCharacteristicQualifier.LiveAnimalsGenderList.Codes.Female;
			product.US_BreedVariety = APHISCommodityCharacteristicQualifier.LiveAnimalsDogList.Codes.BulldogDog;
			product.US_Color = APHISCommodityCharacteristicQualifier.LiveAnimalsColorList.Codes.Various;
			product.US_Age = APHISCommodityCharacteristicQualifier.LiveAnimalsAgeList.Codes._6Months;
			// PG07
			identity = product.Identities.AddNew();
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			identity.CY_Data = "74C717C";

			// PG10
			product = aphisHeader.Products.AddNew();
			product.US_Gender = APHISCommodityCharacteristicQualifier.LiveAnimalsGenderList.Codes.Female;
			product.US_BreedVariety = APHISCommodityCharacteristicQualifier.LiveAnimalsDogList.Codes.BulldogDog;
			product.US_Color = APHISCommodityCharacteristicQualifier.LiveAnimalsColorList.Codes.Various;
			product.US_Age = APHISCommodityCharacteristicQualifier.LiveAnimalsAgeList.Codes._6Months;
			// PG07
			identity = product.Identities.AddNew();
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			identity.CY_Data = "74CC8F1";
			// PG05
			aphisHeader.US_ScientificGenusName = "CANIS";
			aphisHeader.US_ScientificSpeciesName = "FAMILIARIS";
			// PG06
			var source = aphisHeader.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSpeciesOrigin;
			source.US_CountryCode = Core.Constants.CountryCodes.Colombia;
			source.US_ProcessingStartDate = new ZDateTime(2014, 1, 12);
			source.US_ProcessingTypeCode = "AVRAB";

			// PG13 and PG14
			var license = aphisHeader.Licenses.AddNew();
			// PG13
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			// PG14
			license.US_Quantity = 6m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Number;
			license.US_Type = APHISLicenseTypeList.Codes.Aphis7040b7040c;
			license.US_Number = "IMPORTPERMITNUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2015, 2, 25);

			license = aphisHeader.Licenses.AddNew();
			license.US_RN_CountryCode = Core.Constants.CountryCodes.Colombia;
			license.US_Quantity = 6m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Number;
			license.US_Type = APHISLicenseTypeList.Codes.LiveAnimalHealthCertificate;
			license.US_Number = "NOHEALTHCERTIFICATENUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2015, 2, 14);

			license = aphisHeader.Licenses.AddNew();
			license.US_RN_CountryCode = Core.Constants.CountryCodes.Colombia;
			license.US_Quantity = 6m;
			license.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Number;
			license.US_Type = APHISLicenseTypeList.Codes.AphisRabiesVaccination;
			license.US_Number = "NORABIESVACCINATIONNUMBER";
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license.US_Date = new ZDateTime(2015, 2, 14);

			// PG17
			aphisHeader.US_CommoditySpecificName = "MINI ENGLISH BULLDOG";

			// PG19, PG20 and PG21
			aphisHeader.US_OA_ApplicantAddress = permitHolder.MainAddress.PK;
			aphisHeader.US_OA_CropGrowerAddress = cropGrower.MainAddress.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.US_EntryFilerCode = "XJ5";
			invoiceLine1.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			declaration.Branch.GB_OH_OrgProxy = customsBroker.PK;
			var invoice = invoiceLine1.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";
			//PG26
			aphisHeader.US_Qty1 = 6m;
			aphisHeader.US_UQ1 = APHISUnitOfMeasureList.Codes.NumberCount;

			// PG27
			aphisHeader.US_VehicleNumber = "NOCONTAINER";
			aphisHeader.US_VehicleLength = 10;

			// PG30
			var inspection = aphisHeader.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.PreviouslyPerformed;
			inspection.US_Location = "2720";
			inspection.US_Date = new ZDateTime(2015, 2, 15);
			// PG32
			var routing = aphisHeader.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.Colombia;
			routing.US_State = "BOG";

			routing = aphisHeader.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.TransitCountry;
			routing.US_Country = Core.Constants.CountryCodes.Mexico;
			routing.US_State = "MEX";

			#endregion

			#region FWS
			invoiceLine1.US_FWSInd = OGAIndicatorList.Codes.Declared;
			var fswHeader = invoiceLine1.FWSHeaders.AddNew();

			// PG01
			fswHeader.US_IsDocSubmitted = ZBool.True;
			fswHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			fswHeader.US_ProductType = GlobalUniqueProductCodeQualifierList.Codes.SRV;
			fswHeader.US_ProductNumber = "SRV3242";

			// PG05
			fswHeader.US_ScientificGenusName = "SPHYRNA";
			fswHeader.US_ScientificSpeciesName = "LEWINII";
			fswHeader.US_ScientificSubSpeciesName = "SCALLOPED";
			fswHeader.US_WildlifeCategoryCode = FWSWildlifeCategoryCodesList.Codes.FishOther;
			fswHeader.US_WildlifeDescriptionCode = FWSWildlifeDescriptionCodesList.Codes.LIV;

			fswHeader.US_Hybrid = FWSHybridTypeList.Codes.Intergeneric;
			// Second PG05 when hybrid
			fswHeader.US_Scientific2GenusName = "SPHYRNA";
			fswHeader.US_Scientific2SpeciesName = "MOKKARAN";
			fswHeader.US_Scientific2SubSpeciesName = "GREAT";

			// PG06
			fswHeader.US_SpeciesOrigin = Core.Constants.CountryCodes.SouthAfrica;

			// PG10
			fswHeader.US_WildlifeSource = FWSWildlifeSourceList.Codes.W;

			// PG14
			var license1 = fswHeader.Licenses.AddNew();
			license1.US_Type = FWSLicenseTypeList.Codes.ForeignWildlifeExportDocument;
			license1.US_Number = "382332KD";
			var license2 = fswHeader.Licenses.AddNew();
			license2.US_Type = FWSLicenseTypeList.Codes.FWSImportExportLicense;
			license2.US_Number = "83455333";

			// PG17
			fswHeader.US_CommoditySpecificName = "HAMMERHEAD SHARK A SCALLOPED";
			fswHeader.US_CommodityGeneralName = "HAMMERHEAD SHARK";
			fswHeader.US_IsLiveVenomous = true;
			fswHeader.US_CartonQty = 1;

			// PG19, PG20 and PG21
			fswHeader.US_OA_FWSImporterAddress = importer.MainAddress.PK;
			fswHeader.US_OA_FWSExporterAddress = exporter.MainAddress.PK;

			// PG24
			fswHeader.US_RemarksText = "HOW LONG SHOULD A REMARK BE TO BE CONSIDERED TOO LONG REMARKS?";

			// PG25
			fswHeader.US_Value = 300m;

			// PG27
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT2";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "CONT3";
			var containerPivots = invoiceLine1.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().OrderBy(x => x.ContainerNumber).ToArray();
			containerPivots[0].IsForInvoiceLine = true;
			containerPivots[1].IsForInvoiceLine = false;
			containerPivots[2].IsForInvoiceLine = true;

			// PG29
			fswHeader.US_NetCommodityUQ = FWSUnitOfMeasureList.Codes.Meters;
			fswHeader.US_NetCommodity = 1.55m;

			// PG30
			fswHeader.US_FIRMS = "H323";

			#endregion

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Tariff = "0106199120";
			invoiceLine.JI_Description = "LIVE DOGS";
			invoiceLine.JI_OP = testProduct.PK;
			#region FSIS
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

			#region DDTC
			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 10000m;
			invoiceLine3.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine3.US_DDTCLicenseNo = "555666777";
			invoiceLine3.US_DDTCExemptionCode = "567.89";
			invoiceLine3.US_DDTCRegistrationNo = LPCOTypeList.Codes.DD1;
			#endregion

			#endregion

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(declaration.ActiveEntryHeaders.EntrySummaryEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_CertifyCargoRelease = true;
			action.US_AcknowledgeAndSign = true;
			action.US_DateOfDeclaration = new ZDateTime(2016, 1, 2);
			cer1.InvoiceHeader.US_FSISSignDate = new ZDateTime(2016, 1, 2);
			fswHeader.InvoiceHeader.US_FWSSignDate = new ZDateTime(2016, 1, 2);

			var supporter = new JobDeclarationDocumentSupporter(declaration);
			var lines = PGARecapLineGenerator.GenerateLines(supporter.EntryLineMessageBlockCollection, declaration.Factory);
			var result = new ZStringBuilder(lines).ToStringWithNewLineBetweenAppends();
			AssertMultilineASCIIEquals("Data", string.Format(
@"Entry Line: 1   Product: InexistingProduct
        PGA Line: 1   Description: LIVE DOGS   Intended Use Code: 180.003 Animal or plant for educational use
        PGA: APH   Program: AAC (Animal Care)   Processing: A04 (APHIS VS Port Veterinarian)
        Taxonomic Serial Number: 726821
        Live Animal Tag (LAT): 74CFF1A
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A10   Characteristic: 6MO
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A11   Characteristic: DGBD
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A12   Characteristic: VARI
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A13   Characteristic: M
        Live Animal Tag (LAT): 6CA4B1B
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A10   Characteristic: 6MO
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A11   Characteristic: DGBD
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A12   Characteristic: VARI
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A13   Characteristic: M
        Live Animal Tag (LAT): 6CA3988
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A10   Characteristic: 6MO
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A11   Characteristic: DGBD
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A12   Characteristic: VARI
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A13   Characteristic: M
        Live Animal Tag (LAT): 6CA36D8
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A10   Characteristic: 6MO
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A11   Characteristic: DGBD
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A12   Characteristic: VARI
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A13   Characteristic: F
        Live Animal Tag (LAT): 74C717C
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A10   Characteristic: 6MO
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A11   Characteristic: DGBD
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A12   Characteristic: VARI
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A13   Characteristic: F
        Live Animal Tag (LAT): 74CC8F1
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A10   Characteristic: 6MO
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A11   Characteristic: DGBD
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A12   Characteristic: VARI
        APHIS Live Animals (AP0100)   Category: 118   Commodity: A13   Characteristic: F
        Genus: CANIS   Species: FAMILIARIS
        Country of species origin (267): CO Colombia   Processing Details (Start Date: 12-Jan-14Type: AVRAB (APHIS- Rabies Vaccination (Canine)))
        APHIS Live Animals (AP0100)   Category: 118
        ISO Country Code (ISO): US United States   APHIS 7040B/7040C (A31): IMPORTPERMITNUMBER   Date Issued or Signed (3): 25-Feb-15   Quantity: 6 NO
        ISO Country Code (ISO): CO Colombia   Health Certificate (A02): NOHEALTHCERTIFICATENUMBER   Date Issued or Signed (3): 14-Feb-15   Quantity: 6 NO
        ISO Country Code (ISO): CO Colombia   APHIS Rabies Vaccination (A30): NORABIESVACCINATIONNUMBER   Date Issued or Signed (3): 14-Feb-15   Quantity: 6 NO
        Specific Name: MINI ENGLISH BULLDOG
        LPCO Authorized Party: PERMIT HOLDER   APHIS-Assigned: 32KD443
                PH ADDRESS 1 PH ADDRESS 2 NY US
        Ultimate consignee: ULTIMATE CONSIGNEE   IRS-Assigned: 32-23-234232
                UC ADDRESS 1 UC ADDRESS 2 IL US
        Customs broker: CUSTOMS BROKER   CBP-Assigned: XJ5
                CB ADDRESS 1 CB ADDRESS 2 CA US
                BOB SMITH   Phone: 6301023498   Email or Fax: BOB@WHERE.COM
        Packaging: Qty1: 6 NO
        Container Numbers: NOCONTAINER (10 Not refrigerated)
        Inspection previously performed (Date: 15-Feb-15 Location: UNLOCO (3) - 2720)
        Original location (198): CO Colombia   Location: BOG
        Transit country/region (49): MX Mexico   Location: MEX
{0}Entry Line: 1   Product: InexistingProduct
        PGA Line: 1   Description: LIVE DOGS
        PGA: FWS   Program: FWS (Applicable to all FWS programs)   Processing: EDS (Entire Data Set)   GS1 Global Trade Item Number: SRV3242
        Genus: SPHYRNA   Species: LEWINII   Sub Species: SCALLOPED   FWS Wildlife: FSH (Fish, Other)   FWS Description: LIVA100
        Genus: SPHYRNA   Species: MOKKARAN   Sub Species: GREAT   FWS Wildlife: FSH (Fish, Other)   FWS Description: LIVA100
        Country of species origin (267): ZA South Africa
        Commodity: W
        Foreign Wildlife Export Document (FWE): 382332KD
        FWS Import/Export license number (FWL): 83455333
        Specific Name: HAMMERHEAD SHARK A SCALLOPED   General Name: HAMMERHEAD SHARK   Live Venomous: Y   Wildlife Cartons: 1
        FWS Foreign Exporter: FWS EXPORTER
                EXP ADDRESS 1 EXP ADDRESS 2 AU
        Customs broker: CUSTOMS BROKER
                CB ADDRESS 1 CB ADDRESS 2 CA US
                BOB SMITH   Phone: 6301023498   Email or Fax: BOB@WHERE.COM
        FWS Importer: IMPORTER
                IM ADDRESS 1 IM ADDRESS 2 CA US
        Entity Role: FWS Importer   Declaration: FW3 - Wildlife Certification   Certification Y   Date Of Signature: 02-Jan-16
        General Remarks (GEN): HOW LONG SHOULD A REMARK BE TO BE CONSIDERED TOO LONG REMARKS?
        Line Value: 10000 0
        Container Numbers: CONT1,  CONT3
        Line Detail:  Net - 1.55 MT
        Product location for regulatory authority inspection (Date: 01-Jan-16 Location: FIRMS (4) - H323)
{0}Entry Line: 2   Product: TESTPRODUCT
        PGA Line: 1   Description: FSIS TEST   Intended Use Code: 260.000 For Research Use as Human Food
        PGA: FSI   Program: FSI (Applicable to all USDA/FSIS programs)   GS1 Global Trade Item Number: 100578620002680
        Country of Production (39): VN Viet Nam
        ISO Country Code (ISO): AU Australia   FSIS Meat, Poultry or Egg Products Foreign Inspection Certificate (FS7): CER1
        Importer: IMPORTER OF RECORD
                IOR ADDRESS 1 IOR ADDRESS 2 SYDNEY NSW 2017 US
                IOR ALEXANDER THE GREATEST OF ALL   Phone: 04123456   Email or Fax: IOR EMAIL
        Additional Roles: CI - Certifying Individual
        Consignee: IMPORTER
                IM ADDRESS 1 IM ADDRESS 2 CA US
        Customs broker: CUSTOMS BROKER
                CB ADDRESS 1 CB ADDRESS 2 CA US
                BOB SMITH   Phone: 6301023498   Email or Fax: BOB@WHERE.COM
        Type: FSIS 9540-1 - 956   Entity Role: Certifying Individual   Declaration: FS3 - Agreement to hold goods intact (Form 9540-1)   Certification Y   Date Of Signature: 02-Jan-16
        General Remarks (GEN): SEAL1, SEAL2, SEAL3
        Product location for regulatory authority inspection (Date: 24-Sep-13 Location: Inspection Establishment Number Qualifier (8) - USLAX)
{0}Entry Line: 2   Product: TESTPRODUCT
        PGA Line: 2   Description: FSIS TEST   Intended Use Code: 210.000 For Personal Use as Human Food
        PGA: FSI   Program: FSI (Applicable to all USDA/FSIS programs)   UPC (Universal product code): 203918503910256
        Country of Production (39): YE Yemen
        ISO Country Code (ISO): VN Viet Nam   FSIS Meat, Poultry or Egg Products Foreign Inspection Certificate (FS7): CER2
        FSIS - Product Name Category (FS1)   Category: 12   Commodity: HTSS   Characteristic: 1F
        ID: EXP EST 2
        ID: PRO EST 2
        ID: SRC EST 2
        Country of Source (30): CA Canada
        Lot NumberLOT 3   Production Date: Start - 04-Jan-14 End - 05-Oct-14
        Packaging: Qty1: 10 BG (ID: MARK 3),  Qty2: 20 CT
        Line Detail:  Net - 0.15 LB
        FSIS - Product Name Category (FS1)   Category: 12   Commodity: HTSS   Characteristic: 1F
        ID: EXP EST 2
        ID: PRO EST 2
        Lot NumberLOT 4
        Packaging: Qty1: 13 CS (ID: MARK 4)
        Line Detail:  Net - 160937.45 LB
        Importer: IMPORTER OF RECORD
                IOR ADDRESS 1 IOR ADDRESS 2 SYDNEY NSW 2017 US
                IOR ALEXANDER THE GREATEST OF ALL   Phone: 04123456   Email or Fax: IOR EMAIL
        Additional Roles: CI - Certifying Individual
        Consignee: IMPORTER
                IM ADDRESS 1 IM ADDRESS 2 CA US
        Customs broker: CUSTOMS BROKER
                CB ADDRESS 1 CB ADDRESS 2 CA US
                BOB SMITH   Phone: 6301023498   Email or Fax: BOB@WHERE.COM
        Type: FSIS 9540-1 - 956   Entity Role: Certifying Individual   Declaration: FS3 - Agreement to hold goods intact (Form 9540-1)   Certification Y   Date Of Signature: 02-Jan-16
        General Remarks (GEN): SEALA, SEALB, SEALC
        Product location for regulatory authority inspection (Date: 24-Sep-13 Location: Inspection Establishment Number Qualifier (8) - USCHI)
{0}Entry Line: 3
        PGA Line: 1
        PGA: DTC   Program: DTC (Applicable to all DTC programs)
        LPCO Type: 555666777   Exemption: 567.89
        DDTC, Registration Number (DD1): DD1
        BTA anticipated arrival information", System.Environment.NewLine), result);
		}

		OrgAddress AddCustomsAddress(OrgHeader header, ZString adr1, ZString adr2, ZString city, ZString state, ZString postCode, ZString phone, ZString fax, ZString email)
		{
			var impoterCustomsAddress = header.Addresses.AddNew();
			impoterCustomsAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);
			UpdateAddress(impoterCustomsAddress, adr1, adr2, city, state, postCode, phone, fax, email);
			return impoterCustomsAddress;
		}

		void UpdateAddress(OrgAddress address, ZString adr1, ZString adr2, ZString city, ZString state, ZString postCode, ZString phone, ZString fax, ZString email)
		{
			address.OA_Address1 = adr1;
			address.OA_Address2 = adr2;
			address.OA_City = city;
			address.OA_State = state;
			address.OA_PostCode = postCode;
			address.OA_Phone = phone;
			address.OA_Fax = fax;
			address.OA_Email = email;
		}
	}
}
