using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class JobDeclarationCreatorTest : TestCaseWithFactory
	{
		public void TestCreateImportDeclarationForForeignImporter()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER COMPANY";
			importer.OH_RL_NKClosestPort = "AUCNS";
			importer.MainAddress.OA_Address1 = "C/- CAIRNS INTL AIRFREIGHT";
			importer.MainAddress.OA_City = "TEST CITY";
			importer.MainAddress.OA_State = "SA";
			importer.MainAddress.OA_PostCode = "2030";
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PAS1233343");
			var header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_ShipmentType = ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects;
			header.BF_OH_Importer = importer.PK;
			header.BF_SuretyCode = "798";
			header.BF_OwnerReference = "MYREF";
			header.BF_HouseBill = "APLUHB1232112";
			header.BF_ConsigneeCode = "12-3456789XY";
			header.BF_ConsigneeCodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			header.BF_CountryOfIssue = Core.Constants.CountryCodes.UnitedStates;
			header.BF_DateOfBirth = new ZDateTime(1980, 2, 3);
			header.BF_JobReference = ZString.Empty;
			header.BF_RL_NKPlaceOfDelivery = "USLAX";
			header.BF_RL_NKPortOfUnload = "USLAX";
			header.BF_SCAC = "SVSM";
			header.BF_TransportMode = TransportModeCodes.Codes.OceanVesselNonContainerized;
			header.BF_OwnerReference = "OWNREF123";
			header.BF_EstimatedQuantity = 125;
			header.BF_EstimatedQuantityUQ = ShippingOrPackingingUnitList.Codes.Bag;
			header.BF_EstimatedValue = 2500m;
			header.BF_EstimatedWeight = 1;
			header.BF_EstimatedWeightUQ = Core.Constants.Weight.Tonnes;
			RefCountry unitedStates = Factory.Load<RefCountry>(Core.Constants.CountryGuids.UnitedStates);
			var manufacturer1 = Factory.New<OrgHeader>();
			manufacturer1.OH_FullName = "MANUFACTURER1 COMPANY";
			manufacturer1.OH_RL_NKClosestPort = "AUSYD";
			manufacturer1.MainAddress.OA_Address1 = "MANUFACTURER1 ADDRESS 1";
			manufacturer1.MainAddress.OA_Address2 = "MANUFACTURER1 ADDRESS 2";
			manufacturer1.MainAddress.OA_City = "SYDNEY";
			manufacturer1.MainAddress.OA_State = "NSW";
			manufacturer1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "653478956", unitedStates);
			var manufacturer1DocAddress = header.ManufacturerAddresses.AddNew();
			manufacturer1DocAddress.E2_AddressType = DocAddressTypes.Codes.Manufacturer;
			manufacturer1DocAddress.E2_OA_Address = manufacturer1.MainAddress.PK;
			var manufacturer2DocAddress = header.ManufacturerAddresses.AddNew();
			manufacturer2DocAddress.E2_AddressType = DocAddressTypes.Codes.Manufacturer;
			manufacturer2DocAddress.E2_AddressOverride = true;
			manufacturer2DocAddress.E2_CompanyName = "MANUFACTURER2 COMPANY";
			manufacturer2DocAddress.E2_Address1 = "MANUFACTURER2 ADDRESS 1";
			manufacturer2DocAddress.E2_Address2 = "MANUFACTURER2 ADDRESS 2";
			manufacturer2DocAddress.E2_City = "MELBOURNE";
			manufacturer2DocAddress.E2_Contact = "BOB THE DESTROYER";
			manufacturer2DocAddress.E2_Email = "BOB@DOOM.COM";
			manufacturer2DocAddress.E2_Fax = "+61 (2) 6666 6666";
			manufacturer2DocAddress.E2_Phone = "+61 (2) 9999 9999";
			manufacturer2DocAddress.E2_Postcode = "3666";
			manufacturer2DocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			manufacturer2DocAddress.E2_State = "VIR";
			manufacturer2DocAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			manufacturer2DocAddress.E2_GovRegNum = "324328694";
			manufacturer2DocAddress.E2_Mobile = "+61 433 666 666";
			var line1 = header.Lines.AddNew();
			line1.BL_HarmonisedNum = "7325.10.0010";
			line1.BL_ManufacturerDocAddressPK = manufacturer1DocAddress.PK;
			line1.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Finland;
			Factory.Save();
			var declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals(importer.PK, declaration.JE_OH_Importer);
			AssertEquals("Customs Qty should not be readonly", false, declaration.InvoiceLines[0].JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestUpdateLines()
		{
			var importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
			var product = Factory.New<US.Business.OrgSupplierPart>();
			product.OP_PartNum = "DWG Test Product";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_FormattedSupplementalTariff = "9817.00.5000";
			pivot1.CD_UC_NKCountryOfOrigin = "CA";
			var child1 = pivot1.Children.AddNew();
			child1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			child1.CI_FormattedTariffNum = "8481.80.5080";
			child1.CI_FormattedSupplementalTariff = "9889.80.5090";
			child1.CD_UC_NKCountryOfOrigin = "CA";
			var child2 = pivot1.Children.AddNew();
			child2.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			child2.CI_FormattedTariffNum = "8481.80.5090";
			child2.CI_FormattedSupplementalTariff = "9903.88.03";
			child2.CD_UC_NKCountryOfOrigin = "DE";

			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = importer.PK;
			var line = header.Lines.AddNew();
			line.BL_TextProductCode = "DWG Test Product";
			Factory.Save();

			var declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals(importer.PK, declaration.JE_OH_Importer);
			var invoiceLines = declaration.InvoiceLines;
			AssertEquals(3, invoiceLines.Count);
			AssertEquals("invoiceLines[0].SupTariffFormatted", "9817.00.5000", invoiceLines[0].SupTariffFormatted);
			AssertEquals("invoiceLines[0].JI_FormattedTariff", ZString.Empty, invoiceLines[0].JI_FormattedTariff);
			AssertEquals("invoiceLines[0].JI_PartNo", "DWG TEST PRODUCT", invoiceLines[0].JI_PartNo);
			AssertEquals("invoiceLines[0].US_UC_NKCountryOfOrigin", "CA", invoiceLines[0].US_UC_NKCountryOfOrigin);

			AssertEquals("invoiceLines[1].SupTariffFormatted", "9889.80.5090", invoiceLines[1].SupTariffFormatted);
			AssertEquals("invoiceLines[1].JI_FormattedTariff", "8481.80.5080", invoiceLines[1].JI_FormattedTariff);
			AssertEquals("invoiceLines[1].JI_PartNo", "DWG TEST PRODUCT", invoiceLines[1].JI_PartNo);
			AssertEquals("invoiceLines[1].US_UC_NKCountryOfOrigin", "CA", invoiceLines[1].US_UC_NKCountryOfOrigin);

			AssertEquals("invoiceLines[2].SupTariffFormatted", "9903.88.03", invoiceLines[2].SupTariffFormatted);
			AssertEquals("invoiceLines[2].JI_FormattedTariff", "8481.80.5090", invoiceLines[2].JI_FormattedTariff);
			AssertEquals("invoiceLines[2].JI_PartNo", "DWG TEST PRODUCT", invoiceLines[2].JI_PartNo);
			AssertEquals("invoiceLines[2].US_UC_NKCountryOfOrigin", "DE", invoiceLines[2].US_UC_NKCountryOfOrigin);
		}

		public void TestFindActiveOrgHeadersOnly()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER COMPANY";
			importer.OH_RL_NKClosestPort = "AUCNS";
			importer.MainAddress.OA_Address1 = "C/- CAIRNS INTL AIRFREIGHT";
			importer.MainAddress.OA_City = "TEST CITY";
			importer.MainAddress.OA_State = "SA";
			importer.MainAddress.OA_PostCode = "2030";
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789XY");
			importer.OH_IsActive = false;
			var header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_ShipmentType = ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects;
			header.BF_SuretyCode = "798";
			header.BF_OwnerReference = "MYREF";
			header.BF_HouseBill = "APLUHB1232112";
			header.BF_CountryOfIssue = Core.Constants.CountryCodes.UnitedStates;
			header.BF_DateOfBirth = new ZDateTime(1980, 2, 3);
			header.BF_JobReference = ZString.Empty;
			header.BF_RL_NKPlaceOfDelivery = "USLAX";
			header.BF_RL_NKPortOfUnload = "USLAX";
			header.BF_SCAC = "SVSM";
			header.BF_TransportMode = TransportModeCodes.Codes.OceanVesselNonContainerized;
			header.BF_OwnerReference = "OWNREF123";
			header.BF_EstimatedQuantity = 125;
			header.BF_EstimatedQuantityUQ = ShippingOrPackingingUnitList.Codes.Bag;
			header.BF_EstimatedValue = 2500m;
			header.BF_EstimatedWeight = 1;
			header.BF_EstimatedWeightUQ = Core.Constants.Weight.Tonnes;
			RefCountry unitedStates = Factory.Load<RefCountry>(Core.Constants.CountryGuids.UnitedStates);
			var manufacturer1 = Factory.New<OrgHeader>();
			manufacturer1.OH_FullName = "MANUFACTURER1 COMPANY";
			manufacturer1.OH_RL_NKClosestPort = "AUSYD";
			manufacturer1.MainAddress.OA_Address1 = "MANUFACTURER1 ADDRESS 1";
			manufacturer1.MainAddress.OA_Address2 = "MANUFACTURER1 ADDRESS 2";
			manufacturer1.MainAddress.OA_City = "SYDNEY";
			manufacturer1.MainAddress.OA_State = "NSW";
			manufacturer1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "653478956", unitedStates);
			var manufacturer1DocAddress = header.ManufacturerAddresses.AddNew();
			manufacturer1DocAddress.E2_AddressType = DocAddressTypes.Codes.Manufacturer;
			manufacturer1DocAddress.E2_OA_Address = manufacturer1.MainAddress.PK;
			var manufacturer2DocAddress = header.ManufacturerAddresses.AddNew();
			manufacturer2DocAddress.E2_AddressType = DocAddressTypes.Codes.Manufacturer;
			manufacturer2DocAddress.E2_AddressOverride = true;
			manufacturer2DocAddress.E2_CompanyName = "MANUFACTURER2 COMPANY";
			manufacturer2DocAddress.E2_Address1 = "MANUFACTURER2 ADDRESS 1";
			manufacturer2DocAddress.E2_Address2 = "MANUFACTURER2 ADDRESS 2";
			manufacturer2DocAddress.E2_City = "MELBOURNE";
			manufacturer2DocAddress.E2_Contact = "BOB THE DESTROYER";
			manufacturer2DocAddress.E2_Email = "BOB@DOOM.COM";
			manufacturer2DocAddress.E2_Fax = "+61 (2) 6666 6666";
			manufacturer2DocAddress.E2_Phone = "+61 (2) 9999 9999";
			manufacturer2DocAddress.E2_Postcode = "3666";
			manufacturer2DocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			manufacturer2DocAddress.E2_State = "VIR";
			manufacturer2DocAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			manufacturer2DocAddress.E2_GovRegNum = "324328694";
			manufacturer2DocAddress.E2_Mobile = "+61 433 666 666";
			var line1 = header.Lines.AddNew();
			line1.BL_HarmonisedNum = "7325.10.0010";
			line1.BL_ManufacturerDocAddressPK = manufacturer1DocAddress.PK;
			line1.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Finland;
			header.BF_ImporterCode = "12-3456789XY";
			header.BF_ImporterCodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			header.BF_ConsigneeCode = "12-3456789XY";
			header.BF_ConsigneeCodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			Factory.Save();
			var declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
			AssertEquals(ZGuid.Empty, declaration.ConsigneeAddressOrgPK);
			AssertEquals(ZGuid.Empty, declaration.JE_OH_Importer);
		}

		public void TestUpdatingBillManifestQty()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_MasterBill = "APLUMB1232112";
			header.BF_HouseBill = "APLUHB1232113";
			Factory.Save();
			var declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
			AssertEquals("declaration.JE_TotalNoOfPacks", ZInt.Zero, declaration.JE_TotalNoOfPacks);
			AssertEquals("declaration.JE_TotalNoOfPacksPackType", ZString.Empty, declaration.JE_TotalNoOfPacksPackType);
			AssertEquals("declaration.JE_TotalWeight", ZDecimal.Zero, declaration.JE_TotalWeight);
			AssertEquals("declaration.JE_TotalWeightUnit", ZString.Empty, declaration.JE_TotalWeightUnit);
			AssertBill(declaration.PrimaryMasterBill, "MB1232112", ZDecimal.Zero, "PK");
			AssertBill(declaration.PrimaryHouseBill, "HB1232113", ZDecimal.Zero, "PK");
			header.BF_EstimatedQuantity = 125;
			header.BF_EstimatedQuantityUQ = ShippingOrPackingingUnitList.Codes.Box;
			header.BF_EstimatedWeight = 1;
			header.BF_EstimatedWeightUQ = Core.Constants.Weight.Tonnes;
			Factory.Save();
			declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
			AssertEquals("declaration.JE_TotalNoOfPacks", 125, declaration.JE_TotalNoOfPacks);
			AssertEquals("declaration.JE_TotalNoOfPacksPackType", ShippingOrPackingingUnitList.Codes.Box, declaration.JE_TotalNoOfPacksPackType);
			AssertEquals("declaration.JE_TotalWeight", 1m, declaration.JE_TotalWeight);
			AssertEquals("declaration.JE_TotalWeightUnit", Core.Constants.Weight.Tonnes, declaration.JE_TotalWeightUnit);
			AssertBill(declaration.PrimaryMasterBill, "MB1232112", ZDecimal.Zero, "PK");
			AssertBill(declaration.PrimaryHouseBill, "HB1232113", 125m, ShippingOrPackingingUnitList.Codes.Box);
		}

		public void TestMiscFileDescription()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_MasterBill = "APLUMB1232112";
			header.BF_HouseBill = "APLUHB1232113";
			Factory.Save();
			CreateMISCEDocsRows(header.PK.ToGuid());
			AssertEquals("one eDocs row is created", 1, ((IDocManagerSupport)header).DocManagerInfo.AllEDocs.Count);
			var declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
			AssertEquals(1, ((IDocManagerSupport)declaration).DocManagerInfo.Files.Count);
			AssertEquals("MISC TEST", ((IDocManagerSupport)declaration).DocManagerInfo.Files[0].Description);
		}

		[ExpectNoExceptions]
		public void TestCopyeDocsToDeclarationWithBadImageWillNotThrowException()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_MasterBill = "APLUMB1232112";
			header.BF_HouseBill = "APLUHB1232113";
			CreateEDocsRows(header.PK.ToGuid(), new byte[] { 1, 2, 3, 4 }, "testFile", "JPG");
			Factory.Save();

			var declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();

			AssertEquals(1, ((IDocManagerSupport)declaration).DocManagerInfo.AllEDocs.Count);
			AssertEquals("testFile.jpg", ((IDocManagerSupport)declaration).DocManagerInfo.AllEDocs[0].FileName);
		}

		public void TestCreateDeclarationSettingCorrectContainerMode()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_TransportMode = TransportModeCodes.Codes.OceanVesselNonContainerized;
			Factory.Save();
			var declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
			AssertEquals(Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
			header.BF_TransportMode = TransportModeCodes.Codes.OceanVesselContainerized;
			Factory.Save();
			declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.JE_ContainerMode);
		}

		public void TestCreateDeclaration()
		{
			#region Setup data
			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "BUYER COMPANY";
			buyer.OH_RL_NKClosestPort = "USCHI";
			buyer.MainAddress.OA_Address1 = "BUYER ADDRESS 1";
			buyer.MainAddress.OA_Address2 = "BUYER ADDRESS 2";
			buyer.MainAddress.OA_City = "CHICAGO";
			buyer.MainAddress.OA_State = "IL";
			OrgHeader seller = Factory.New<OrgHeader>();
			seller.OH_FullName = "SELLER COMPANY";
			seller.OH_RL_NKClosestPort = "AUSYD";
			seller.MainAddress.OA_Address1 = "SELLER ADDRESS 1";
			seller.MainAddress.OA_Address2 = "SELLER ADDRESS 2";
			seller.MainAddress.OA_City = "SYDNEY";
			seller.MainAddress.OA_State = "NSW";
			OrgHeader shipTo = Factory.New<OrgHeader>();
			shipTo.OH_FullName = "SHIPTOPARTY COMPANY";
			shipTo.OH_RL_NKClosestPort = "AUSYD";
			shipTo.MainAddress.OA_Address1 = "SHIPTOPARTY ADDRESS 1";
			shipTo.MainAddress.OA_Address2 = "SHIPTOPARTY ADDRESS 2";
			shipTo.MainAddress.OA_City = "SYDNEY";
			shipTo.MainAddress.OA_State = "NSW";
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER COMPANY";
			importer.OH_RL_NKClosestPort = "USCHI";
			importer.MainAddress.OA_Address1 = "IMPORTER ADDRESS 1";
			importer.MainAddress.OA_Address2 = "IMPORTER ADDRESS 2";
			importer.MainAddress.OA_City = "CHICAGO";
			importer.MainAddress.OA_State = "IL";
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PAS1233343");
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE COMPANY";
			consignee.OH_RL_NKClosestPort = "USCHI";
			consignee.MainAddress.OA_Address1 = "CONSIGNEE ADDRESS 1";
			consignee.MainAddress.OA_Address2 = "CONSIGNEE ADDRESS 2";
			consignee.MainAddress.OA_City = "CHICAGO";
			consignee.MainAddress.OA_State = "IL";
			consignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789XY");
			OrgHeader consolidator = Factory.New<OrgHeader>();
			consolidator.OH_FullName = "CONSOLIDATOR COMPANY";
			consolidator.OH_RL_NKClosestPort = "USCHI";
			consolidator.MainAddress.OA_Address1 = "CONSOLIDATOR ADDRESS 1";
			consolidator.MainAddress.OA_Address2 = "CONSOLIDATOR ADDRESS 2";
			consolidator.MainAddress.OA_City = "CHICAGO";
			consolidator.MainAddress.OA_State = "IL";
			RefCountry unitedStates = Factory.Load<RefCountry>(Core.Constants.CountryGuids.UnitedStates);
			OrgHeader manufacturer1 = Factory.New<OrgHeader>();
			manufacturer1.OH_FullName = "MANUFACTURER1 COMPANY";
			manufacturer1.OH_RL_NKClosestPort = "AUSYD";
			manufacturer1.MainAddress.OA_Address1 = "MANUFACTURER1 ADDRESS 1";
			manufacturer1.MainAddress.OA_Address2 = "MANUFACTURER1 ADDRESS 2";
			manufacturer1.MainAddress.OA_City = "SYDNEY";
			manufacturer1.MainAddress.OA_State = "NSW";
			manufacturer1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "653478956", unitedStates);
			US.Business.OrgSupplierPart part = (US.Business.OrgSupplierPart)Factory.New<Integration.Customs.US.IOrgSupplierPart>();
			part.OP_PartNum = "DUMMYTEST1PART";
			part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part.RelatedOrganisations.AddOrganisationIfNotExist(seller.PK, OrgPartRelation.RelationshipTypes.Supplier);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = part.RelatedOrganisations[0].OU_OH;
			pivot.CI_TariffNum = "30.30.8320";
			pivot.CI_SupplementalTariff = "9915.61.03";
			pivot.Attributes1.AddNew().BG_AttributeValue1 = "1";
			pivot.Attributes2.AddNew().BG_AttributeValue1 = "2";
			pivot.Attributes3.AddNew().BG_AttributeValue1 = "3";
			pivot.CD_OA_Manufacturer = manufacturer1.MainAddress.PK;
			var pivotChild1 = pivot.Children.AddNew();
			pivotChild1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivotChild1.CI_TariffNum = "30.30.8321";
			pivotChild1.CD_UC_NKCountryOfOrigin = "SG";
			var pivotChild2 = pivot.Children.AddNew();
			pivotChild2.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			pivotChild2.CI_TariffNum = "9801.00.65";
			pivotChild2.CD_UC_NKCountryOfOrigin = "CN";
			Factory.Save();
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_ShipmentType = ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects;
			header.BF_OH_Importer = importer.PK;
			header.BF_SuretyCode = "798";
			header.BF_OwnerReference = "MYREF";
			header.BF_HouseBill = "APLUHB1232112";
			header.BF_ConsigneeCodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			header.BF_ConsigneeCode = "12-3456789XY";
			header.BF_CountryOfIssue = Core.Constants.CountryCodes.UnitedStates;
			header.BF_DateOfBirth = new ZDateTime(1980, 2, 3);
			header.BF_JobReference = ZString.Empty;
			header.BF_RL_NKPlaceOfDelivery = "USLAX";
			header.BF_RL_NKPortOfUnload = "USLAX";
			header.BF_SCAC = "SVSM";
			header.BF_TransportMode = TransportModeCodes.Codes.OceanVesselNonContainerized;
			header.BF_OwnerReference = "OWNREF123";
			header.BF_EstimatedQuantity = 125;
			header.BF_EstimatedQuantityUQ = ShippingOrPackingingUnitList.Codes.Box;
			header.BF_EstimatedValue = 2500m;
			header.BF_EstimatedWeight = 1;
			header.BF_EstimatedWeightUQ = Core.Constants.Weight.Tonnes;
			AddBill(header, "STZPMB1232112", BillTypeList.Codes.MasterBillOfLading);
			AddBill(header, "BRPAHB9866455", BillTypeList.Codes.HouseBillOfLading);
			AddBill(header, "ENTRY2112", BillTypeList.Codes.USCBPEntryNumber);
			header.SellingParty.E2_AddressOverride = false;
			header.SellingParty.E2_OA_Address = seller.MainAddress.PK;
			ISFDocAddress buyingParty = header.BuyingParty;
			buyingParty.E2_AddressOverride = true;
			buyingParty.E2_CompanyName = "BUYING COMPANY";
			buyingParty.E2_Address1 = "BUYING ADDRESS 1";
			buyingParty.E2_Address2 = "BUYING ADDRESS 2";
			buyingParty.E2_City = "SYDNEY";
			buyingParty.E2_Contact = "BUYER THE BUILDER";
			buyingParty.E2_Email = "BUYER@BUILDER.COM";
			buyingParty.E2_Fax = "+61 (2) 6953 6846";
			buyingParty.E2_Phone = "+61 (2) 6953 6855";
			buyingParty.E2_Postcode = "2200";
			buyingParty.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			buyingParty.E2_State = "NSW";
			buyingParty.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			buyingParty.E2_GovRegNum = "912-21-4568";
			buyingParty.E2_Mobile = "+61 403 864 456";
			ISFDocAddress stuffingLocation = header.StuffingLocation;
			stuffingLocation.E2_AddressOverride = true;
			stuffingLocation.E2_CompanyName = "STUFFING LOCATION COMPANY";
			stuffingLocation.E2_Address1 = "STUFFING LOCATION ADDRESS 1";
			stuffingLocation.E2_Address2 = "STUFFING LOCATION ADDRESS 2";
			stuffingLocation.E2_City = "CHICAGO";
			stuffingLocation.E2_Contact = "STUFFER THE BUILDER";
			stuffingLocation.E2_Email = "STUFFER@BUILDER.COM";
			stuffingLocation.E2_Postcode = "61022";
			stuffingLocation.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			ISFDocAddress consolidatorDocAddress = header.Consolidator;
			consolidatorDocAddress.E2_OA_Address = consolidator.MainAddress.PK;
			ISFDocAddress bookingPartyDocAddress = header.BookingParty;
			bookingPartyDocAddress.E2_AddressOverride = true;
			bookingPartyDocAddress.E2_CompanyName = "BOOKING PARTY COMPANY";
			bookingPartyDocAddress.E2_Address1 = "BOOKING PARTY ADDRESS 1";
			bookingPartyDocAddress.E2_Address2 = "BOOKING PARTY ADDRESS 2";
			bookingPartyDocAddress.E2_City = "MELBOURNE";
			bookingPartyDocAddress.E2_Contact = "WENDY THE BUILDER";
			bookingPartyDocAddress.E2_Email = "WENDY@BUILDER.COM";
			bookingPartyDocAddress.E2_Fax = "+61 (3) 8456 6846";
			bookingPartyDocAddress.E2_Phone = "+61 (3) 8456 6855";
			bookingPartyDocAddress.E2_Postcode = "3014";
			bookingPartyDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			bookingPartyDocAddress.E2_State = "VIC";
			bookingPartyDocAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			bookingPartyDocAddress.E2_GovRegNum = "326465678";
			bookingPartyDocAddress.E2_Mobile = "+61 403 112 695";
			ISFDocAddress shipToParty = header.MainShipToParty;
			shipToParty.OrganisationPK = shipTo.PK;
			shipToParty.E2_AddressOverride = true;
			shipToParty.E2_CompanyName = "SHIP TO PARTY COMPANY";
			shipToParty.E2_Address1 = "SHIP TO PARTY ADDRESS 1";
			shipToParty.E2_Address2 = "SHIP TO PARTY ADDRESS 2";
			shipToParty.E2_City = "MELBOURNE";
			shipToParty.E2_Contact = "SHIP TO THE BUILDER";
			shipToParty.E2_Email = "SHIPTO@BUILDER.COM";
			shipToParty.E2_Fax = "+61 (3) 8456 6846";
			shipToParty.E2_Phone = "+61 (3) 8456 6855";
			shipToParty.E2_Postcode = "3014";
			shipToParty.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			shipToParty.E2_State = "VIC";
			shipToParty.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			shipToParty.E2_GovRegNum = "869345684";
			shipToParty.E2_Mobile = "+61 403 112 695";
			shipToParty.E2_OA_Address = shipTo.MainAddress.PK;
			RefContainer containerType1 = Factory.New<RefContainer>();
			containerType1.RC_Code = "!Z1";
			containerType1.RC_ISOType = "21ZZ";
			RefContainer containerType2 = Factory.New<RefContainer>();
			containerType2.RC_Code = "!Z2";
			var containerCodeMap = containerType2.CodeMapCollection.AddNew();
			containerCodeMap.RCM_RN_NKCountry = "US";
			containerCodeMap.RCM_Code = "4Z";
			CusISFEquip container1 = header.Equipments.AddNew();
			container1.BE_ContainerNum = "TURE2323333";
			container1.BE_EquipCode = "2Z";
			container1.BE_ContainerISO = "21ZZ";
			CusISFEquip container2 = header.Equipments.AddNew();
			container2.BE_ContainerNum = "TURE1110111";
			container2.BE_EquipCode = "4Z";
			container2.BE_ContainerISO = "4ZFR";
			ISFDocAddress manufacturer1DocAddress = header.ManufacturerAddresses.AddNew();
			manufacturer1DocAddress.E2_AddressType = DocAddressTypes.Codes.Manufacturer;
			manufacturer1DocAddress.E2_OA_Address = manufacturer1.MainAddress.PK;
			ISFDocAddress manufacturer2DocAddress = header.ManufacturerAddresses.AddNew();
			manufacturer2DocAddress.E2_AddressType = DocAddressTypes.Codes.Manufacturer;
			manufacturer2DocAddress.E2_AddressOverride = true;
			manufacturer2DocAddress.E2_CompanyName = "MANUFACTURER2 COMPANY";
			manufacturer2DocAddress.E2_Address1 = "MANUFACTURER2 ADDRESS 1";
			manufacturer2DocAddress.E2_Address2 = "MANUFACTURER2 ADDRESS 2";
			manufacturer2DocAddress.E2_City = "MELBOURNE";
			manufacturer2DocAddress.E2_Contact = "BOB THE DESTROYER";
			manufacturer2DocAddress.E2_Email = "BOB@DOOM.COM";
			manufacturer2DocAddress.E2_Fax = "+61 (2) 6666 6666";
			manufacturer2DocAddress.E2_Phone = "+61 (2) 9999 9999";
			manufacturer2DocAddress.E2_Postcode = "3666";
			manufacturer2DocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			manufacturer2DocAddress.E2_State = "VIR";
			manufacturer2DocAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			manufacturer2DocAddress.E2_GovRegNum = "324328694";
			manufacturer2DocAddress.E2_Mobile = "+61 433 666 666";
			CusISFLine line1 = header.Lines.AddNew();
			line1.BL_HarmonisedNum = "10.10.8120";
			line1.BL_ManufacturerDocAddressPK = manufacturer1DocAddress.PK;
			line1.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			CusISFLine line2 = header.Lines.AddNew();
			line2.BL_HarmonisedNum = "20.20.8220";
			line2.BL_ManufacturerDocAddressPK = manufacturer2DocAddress.PK;
			line2.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			CusISFLine line3 = header.Lines.AddNew();
			line3.BL_HarmonisedNum = "30.30.8320";
			line3.BL_ManufacturerDocAddressPK = manufacturer1DocAddress.PK;
			line3.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			line3.BL_TextProductCode = part.OP_PartNum;
			line3.BL_PartAttrib1 = "1";
			line3.BL_PartAttrib2 = "2";
			line3.BL_PartAttrib3 = "3";
			AssertEquals("Precondition: a child line should be created", 1, line3.ChildLines.Count());
			var childLine = line3.AddChildLine();
			childLine.BL_HarmonisedNum = "30.30.8322";
			childLine.BL_RN_NKGoodsOrigin = "EG";
			var productLine = line3.AddProductRelatedLine();
			productLine.BL_HarmonisedNum = "9801.00.66";
			productLine.BL_RN_NKGoodsOrigin = "ZA";
			CusISFLine line4 = header.Lines.AddNew();
			line4.BL_HarmonisedNum = "40.40.8420";
			line4.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.NewZealand;
			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "CARRIER COMPANY";
			carrier.OH_RL_NKClosestPort = "USCHI";
			carrier.MainAddress.OA_Address1 = "CARRIER ADDRESS 1";
			carrier.MainAddress.OA_Address2 = "CARRIER ADDRESS 2";
			carrier.MainAddress.OA_City = "CHICAGO";
			carrier.MainAddress.OA_State = "IL";
			Transport transport = header.Transports.AddNew();
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_Vessel = "APL VESSEL";
			transport.JW_VoyageFlight = "328";
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_RL_NKDiscPort = "USCHI";
			transport.JW_ETD = new ZDateTime(2009, 3, 1);
			transport.JW_ATD = new ZDateTime(2009, 3, 2);
			transport.JW_ETA = new ZDateTime(2009, 4, 1);
			transport.JW_ATA = new ZDateTime(2009, 4, 2);
			transport.CarrierPK = carrier.PK;
			var task = header.WorkflowItems.Triggers.AddNew();
			task.P9_Description = "Test";
			task.TriggerConditions.TriggerEventCode = Events.TransferToCustomsImportsDec.Code;
			task.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			task.TriggerConditions.TriggerConditionValue = "B*";
			var notification = task.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_EmailAddr = "dummy1@where.com";
			Factory.Save();
			#endregion
			JobDeclaration declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
			AssertNotEquals(header.Factory, declaration.Factory);
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals(TransportTypeList.Codes.Sea, declaration.JE_TransportMode);
			AssertEquals("MB1232112", declaration.JE_MasterBill);
			AssertEquals("STZP", declaration.JE_MasterBillIssuerSCAC);
			AssertEquals("HB1232112", declaration.JE_HouseBill);
			AssertEquals("APLU", declaration.JE_HouseBillIssuerSCAC);
			AssertEquals("APL VESSEL", declaration.JE_VesselName);
			AssertEquals("328", declaration.JE_VoyageFlightNo);
			AssertEquals("AUBNE", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("USCHI", declaration.JE_RL_NKPortOfArrival);
			AssertEquals(new ZDateTime(2009, 3, 2), declaration.JE_ExportDate);
			AssertEquals(new ZDateTime(2009, 4, 2), declaration.JE_DateOfArrival);
			AssertEquals("", declaration.US_SuretyCode);
			AssertEquals("SVSM", declaration.US_UI_NKCarrierSCAC);
			AssertEquals(importer.PK, declaration.JE_OH_Importer);
			AssertEquals(consignee.PK, declaration.ConsigneeAddressOrgPK);
			AssertEquals(ZGuid.Empty, declaration.JE_OH_Buyer);
			AssertEquals(seller.PK, declaration.SellerOrgPK);
			AssertEquals(carrier.PK, declaration.JE_OH_ShippingLine);
			AssertEquals("OWNREF123", declaration.JE_OwnerRef);
			AssertEquals(125, declaration.JE_TotalNoOfPacks);
			AssertEquals(ShippingOrPackingingUnitList.Codes.Box, declaration.JE_TotalNoOfPacksPackType);
			AssertEquals(1m, declaration.JE_TotalWeight);
			AssertEquals(Core.Constants.Weight.Tonnes, declaration.JE_TotalWeightUnit);
			AssertEquals(header.MainShipToParty.Organisation.PK, declaration.ShipToParty.PK);
			AssertEquals(2, declaration.CusContainers.Count);
			CusContainer cusContainer1 = declaration.CusContainers[0];
			AsserContainer(cusContainer1, "TURE2323333", containerType1);
			CusContainer cusContainer2 = declaration.CusContainers[1];
			AsserContainer(cusContainer2, "TURE1110111", containerType2);
			Bill bill = declaration.Bills.FindByBillNumberAndType("HB9866455", Customs.Business.BillTypeList.Codes.HouseBill);
			AssertNotNull(bill);
			AssertEquals("BRPA", bill.US_UI_NKBillIssuerSCAC);
			CombineAssertions(() =>
			{
				AssertEquals(1, declaration.Invoices.Count);
				var invoice = declaration.Invoices[0];
				AssertEquals(2500m, invoice.JZ_InvoiceAmount);
				AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, invoice.JZ_RX_NKInvoice_Currency);
				AssertEquals(8, invoice.JobComInvoiceLines.Count);
				JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines[0];
				AssertInvoiceLine(invoiceLine1, ZString.Empty, Core.Constants.CountryCodes.Australia, "10108120", manufacturer1.MainAddress.PK);
				JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines[1];
				AssertInvoiceLine(invoiceLine2, ZString.Empty, Core.Constants.CountryCodes.Australia, "20208220", ZGuid.Empty);
				JobComInvoiceLine invoiceLine3 = invoice.JobComInvoiceLines[2];
				AssertInvoiceLine(invoiceLine3, "DUMMYTEST1PART", Core.Constants.CountryCodes.Australia, "30308320", manufacturer1.MainAddress.PK);
				JobComInvoiceLine invoiceLine4 = invoice.JobComInvoiceLines[3];
				AssertInvoiceLine(invoiceLine4, "DUMMYTEST1PART", Core.Constants.CountryCodes.Singapore, "30308321", manufacturer1.MainAddress.PK);
				AssertEquals("ParentTariffLine", invoiceLine3, invoiceLine4.ParentTariffLine);
				AssertEquals("Prov/Prog. Tariff", "", invoiceLine4.SupTariffFormatted);
				JobComInvoiceLine invoiceLine5 = invoice.JobComInvoiceLines[4];
				AssertInvoiceLine(invoiceLine5, "DUMMYTEST1PART", Core.Constants.CountryCodes.China, "98010065", manufacturer1.MainAddress.PK);
				AssertEquals("ParentTariffLine", invoiceLine3, invoiceLine5.ProductParentTariffLine);
				JobComInvoiceLine invoiceLine6 = invoice.JobComInvoiceLines[5];
				AssertInvoiceLine(invoiceLine6, "DUMMYTEST1PART", Core.Constants.CountryCodes.Australia, "30308322", manufacturer1.MainAddress.PK);
				AssertEquals("ParentTariffLine", invoiceLine3, invoiceLine6.ParentTariffLine);
				JobComInvoiceLine invoiceLine7 = invoice.JobComInvoiceLines[6];
				AssertInvoiceLine(invoiceLine7, "DUMMYTEST1PART", Core.Constants.CountryCodes.SouthAfrica, "98010066", ZGuid.Empty);
				AssertEquals("ParentTariffLine", invoiceLine3, invoiceLine7.ProductParentTariffLine);
				JobComInvoiceLine invoiceLine8 = invoice.JobComInvoiceLines[7];
				AssertInvoiceLine(invoiceLine8, ZString.Empty, Core.Constants.CountryCodes.NewZealand, "40408420", ZGuid.Empty);
			});
			CusISFHeader headerInDiffFactory = declaration.Factory.Load<CusISFHeader>(header.PK);
			AssertNull(headerInDiffFactory.Logs.MostRecentLogByEventTime(Events.TransferToCustomsImportsDec));
			AssertEquals(ZDateTime.Empty, headerInDiffFactory.WorkflowItems.Triggers[0].P9_ActualDate.ToZDateTime());
			declaration.Factory.Save();
			AssertNotNull(headerInDiffFactory.Logs.MostRecentLogByEventTime(Events.TransferToCustomsImportsDec, declaration.JE_DeclarationReference));
			AssertNotEquals(ZDateTime.Empty, headerInDiffFactory.WorkflowItems.Triggers[0].P9_ActualDate.ToZDateTime());
			header.SellingParty.E2_AddressOverride = true;
			header.SellingParty.E2_CompanyName = "SELLING COMPANY";
			header.SellingParty.E2_Address1 = "SELLING ADDRESS 1";
			header.SellingParty.E2_Address2 = "SELLING ADDRESS 2";
			header.SellingParty.E2_City = "SYDNEY";
			header.SellingParty.E2_Contact = "BOB THE BUILDER";
			header.SellingParty.E2_Email = "BOB@BUILDER.COM";
			header.SellingParty.E2_Fax = "+61 (2) 8456 6846";
			header.SellingParty.E2_Phone = "+61 (2) 8456 6855";
			header.SellingParty.E2_Postcode = "2214";
			header.SellingParty.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			header.SellingParty.E2_State = "NSW";
			header.SellingParty.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			header.SellingParty.E2_GovRegNum = "324325684";
			header.SellingParty.E2_Mobile = "+61 403 112 456";
			header.BuyingParty.E2_AddressOverride = false;
			header.BuyingParty.E2_OA_Address = buyer.MainAddress.PK;
			header.MainShipToParty.E2_AddressOverride = false;
			header.MainShipToParty.E2_OA_Address = shipTo.MainAddress.PK;
			header.ReferenceDatas.DeleteAll();
			header.BF_OceanBill = "TSPFHBJISS";
			transport.JW_ATD = ZDateTime.Empty;
			transport.JW_ATA = ZDateTime.Empty;
			Factory.Save();
			CreateEDocsRows(header.PK.ToGuid());
			AssertEquals("one eDocs row is created", 1, ((IDocManagerSupport)header).DocManagerInfo.AllEDocs.Count);
			JobDeclaration declaration2 = new JobDeclarationCreator(header.PK).CreateDeclaration();
			AssertNotEquals(header.Factory, declaration2.Factory);
			AssertNotEquals(declaration.Factory, declaration2.Factory);
			AssertEquals(buyer.PK, declaration2.JE_OH_Buyer);
			AssertEquals(shipTo.PK, declaration2.ShipToParty.PK);
			AssertEquals(Guid.Empty, declaration2.SellerOrgPK);
			AssertEquals(new ZDateTime(2009, 3, 1), declaration2.JE_ExportDate);
			AssertEquals(new ZDateTime(2009, 4, 1), declaration2.JE_DateOfArrival);
			AssertEquals("HBJISS", declaration2.JE_MasterBill);
			AssertEquals("TSPF", declaration2.JE_MasterBillIssuerSCAC);
			AssertEquals(ZString.Empty, declaration2.JE_HouseBill);
			AssertEquals(1, declaration2.DocManagerInfo.AllEDocs.Count);
			var edocs = declaration2.DocManagerInfo.AllEDocs[0];
			AssertEquals("FileName", "FileName.xml", edocs.FileName);
			AssertEquals("DocType", "ACV", edocs.DocType);
			AssertEquals("DataType", "XML", edocs.DataType);
			Assert(declaration2.ShouldSaveEDocsMasterFactoryTogether);
		}

		public void TestUpdateConsigneeWhenCreateDeclaration()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE COMPANY";
			consignee.OH_RL_NKClosestPort = "USCHI";
			consignee.MainAddress.OA_Address1 = "CONSIGNEE ADDRESS 1";
			consignee.MainAddress.OA_Address2 = "CONSIGNEE ADDRESS 2";
			consignee.MainAddress.OA_City = "CHICAGO";
			consignee.MainAddress.OA_State = "IL";
			consignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789XY");
			consignee.OH_SystemCreateTimeUtc = ZDateTime.Today;
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER COMPANY";
			importer.OH_RL_NKClosestPort = "USCHI";
			importer.MainAddress.OA_Address1 = "IMPORTER ADDRESS 1";
			importer.MainAddress.OA_Address2 = "IMPORTER ADDRESS 2";
			importer.MainAddress.OA_City = "CHICAGO";
			importer.MainAddress.OA_State = "IL";
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789XY");
			importer.OH_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			var header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_ShipmentType = ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects;
			header.BF_OwnerReference = "MYREF";
			header.BF_HouseBill = "APLUHB1232112";
			header.BF_ConsigneeCodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			header.BF_ConsigneeCode = "12-3456789XY";
			header.BF_CountryOfIssue = Core.Constants.CountryCodes.UnitedStates;
			header.BF_TransportMode = TransportModeCodes.Codes.OceanVesselNonContainerized;
			header.BF_OwnerReference = "OWNREF123";
			Factory.Save();
			var declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
			AssertEquals(consignee.PK, declaration.ConsigneeAddressOrgPK);
			header.BF_OH_Importer = importer.PK;
			Factory.Save();
			declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
			AssertEquals(importer.PK, declaration.ConsigneeAddressOrgPK);
			importer.CustomsCodes.RemoveAndDeleteAll();
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PAS1233343");
			Factory.Save();
			declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
			AssertEquals(consignee.PK, declaration.ConsigneeAddressOrgPK);
		}

		public void TestThereAreNoTwoFactoryForMainBusinessEntitiesWhenEDocsAreCopied()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER COMPANY";
			importer.OH_RL_NKClosestPort = "USCHI";
			importer.MainAddress.OA_Address1 = "IMPORTER ADDRESS 1";
			importer.MainAddress.OA_Address2 = "IMPORTER ADDRESS 2";
			importer.MainAddress.OA_City = "CHICAGO";
			importer.MainAddress.OA_State = "IL";
			importer.MainAddress.OA_FCLEquipmentNeeded = "WUP";
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PAS1233343");
			Factory.Save();
			var header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_ShipmentType = ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects;
			header.BF_OH_Importer = importer.PK;
			header.BF_SuretyCode = "798";
			header.BF_OwnerReference = "MYREF";
			header.BF_HouseBill = "APLUHB1232112";
			header.BF_CountryOfIssue = Core.Constants.CountryCodes.UnitedStates;
			header.BF_JobReference = ZString.Empty;
			header.BF_RL_NKPlaceOfDelivery = "USLAX";
			header.BF_RL_NKPortOfUnload = "USLAX";
			header.BF_SCAC = "SVSM";
			header.BF_TransportMode = TransportModeCodes.Codes.OceanVesselNonContainerized;
			header.BF_OwnerReference = "OWNREF123";
			header.BF_EstimatedQuantity = 125;
			header.BF_EstimatedQuantityUQ = ShippingOrPackingingUnitList.Codes.Box;
			header.BF_EstimatedValue = 2500m;
			header.BF_EstimatedWeight = 1;
			header.BF_EstimatedWeightUQ = Core.Constants.Weight.Tonnes;
			CreateEDocsRows(header.PK.ToGuid());
			Factory.Save();
			var declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
			var eDocFactory = declaration.DocManagerInfo.MasterFactory;
			BusinessObjectFactory.SaveTogether(declaration.Factory, eDocFactory);
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "C1";
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertNoExceptionThrown(delegate
			{
				BusinessObjectFactory.SaveTogether(declaration.Factory, eDocFactory);
			});
		}

		public void TestUseShipToPartySetFromISFDeclaration()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE COMPANY";
			consignee.OH_RL_NKClosestPort = "USCHI";
			consignee.MainAddress.OA_Address1 = "CONSIGNEE ADDRESS 1";
			consignee.MainAddress.OA_Address2 = "CONSIGNEE ADDRESS 2";
			consignee.MainAddress.OA_City = "CHICAGO";
			consignee.MainAddress.OA_State = "IL";
			consignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789XY");
			consignee.OH_SystemCreateTimeUtc = ZDateTime.Today;
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER COMPANY";
			importer.OH_RL_NKClosestPort = "USCHI";
			importer.MainAddress.OA_Address1 = "IMPORTER ADDRESS 1";
			importer.MainAddress.OA_Address2 = "IMPORTER ADDRESS 2";
			importer.MainAddress.OA_City = "CHICAGO";
			importer.MainAddress.OA_State = "IL";
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789XY");
			importer.OH_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			var header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_ShipmentType = ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects;
			header.BF_OwnerReference = "MYREF";
			header.BF_HouseBill = "APLUHB1232112";
			header.BF_ConsigneeCodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			header.BF_ConsigneeCode = "12-3456789XY";
			header.BF_CountryOfIssue = Core.Constants.CountryCodes.UnitedStates;
			header.BF_TransportMode = TransportModeCodes.Codes.OceanVesselNonContainerized;
			header.BF_OwnerReference = "OWNREF123";
			OrgHeader shipTo = Factory.New<OrgHeader>();
			shipTo.OH_FullName = "SHIPTOPARTY COMPANY";
			shipTo.OH_RL_NKClosestPort = "AUSYD";
			shipTo.MainAddress.OA_Address1 = "SHIPTOPARTY ADDRESS 1";
			shipTo.MainAddress.OA_Address2 = "SHIPTOPARTY ADDRESS 2";
			shipTo.MainAddress.OA_City = "SYDNEY";
			shipTo.MainAddress.OA_State = "NSW";
			Factory.Save();
			USCustomsDataRegistry.Instance.DoDefaultShipTo.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			var declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
			AssertNull(declaration.ShipToParty);
			AssertEquals(consignee.PK, declaration.ConsigneeAddressOrgPK);
			USCustomsDataRegistry.Instance.DoDefaultShipTo.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
			AssertEquals(consignee.PK, declaration.ConsigneeAddressOrgPK);
			AssertEquals(consignee.PK, declaration.ConsigneeAddressOrgPK);
			ISFDocAddress shipToParty = header.MainShipToParty;
			shipToParty.OrganisationPK = shipTo.PK;
			shipToParty.E2_AddressOverride = true;
			shipToParty.E2_CompanyName = "SHIP TO PARTY COMPANY";
			shipToParty.E2_Address1 = "SHIP TO PARTY ADDRESS 1";
			shipToParty.E2_Address2 = "SHIP TO PARTY ADDRESS 2";
			shipToParty.E2_City = "MELBOURNE";
			shipToParty.E2_Contact = "SHIP TO THE BUILDER";
			shipToParty.E2_Email = "SHIPTO@BUILDER.COM";
			shipToParty.E2_Fax = "+61 (3) 8456 6846";
			shipToParty.E2_Phone = "+61 (3) 8456 6855";
			shipToParty.E2_Postcode = "3014";
			shipToParty.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			shipToParty.E2_State = "VIC";
			shipToParty.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			shipToParty.E2_GovRegNum = "869345684";
			shipToParty.E2_Mobile = "+61 403 112 695";
			shipToParty.E2_OA_Address = shipTo.MainAddress.PK;
			Factory.Save();
			USCustomsDataRegistry.Instance.DoDefaultShipTo.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
			AssertEquals(shipTo.PK, declaration.ShipToParty.PK);
			AssertEquals(consignee.PK, declaration.ConsigneeAddressOrgPK);
			USCustomsDataRegistry.Instance.DoDefaultShipTo.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
			AssertEquals(shipTo.PK, declaration.ShipToParty.PK);
			AssertEquals(consignee.PK, declaration.ConsigneeAddressOrgPK);
		}

		public void TestUseBuyingPartyFromISFDeclaration()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = importer.PK;
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var buyingParty = header.BuyingParty;
			buyingParty.E2_OA_Address = buyer.MainAddress.PK;
			var header2 = Factory.New<CusISFHeader>();
			header2.BF_OH_Importer = importer.PK;
			Factory.Save();
			using (USCustomsDataRegistry.Instance.DoDefaultSoldToParty.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var declaration = new JobDeclarationCreator(header.PK).CreateDeclaration();
				AssertEquals("Importer from ISF should be used and not changed when Sold To Party is defaulted.", importer.MainAddress.PK, declaration.JE_OA_SoldToPartyAddress);
			}

			using (USCustomsDataRegistry.Instance.DoDefaultSoldToParty.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var declaration2 = new JobDeclarationCreator(header.PK).CreateDeclaration();
				AssertEquals("Buying Party from ISF should have been used as Sold To Party if there are no defaults.", buyer.MainAddress.PK, declaration2.JE_OA_SoldToPartyAddress);
				var declaration3 = new JobDeclarationCreator(header2.PK).CreateDeclaration();
				AssertEquals("Should not have a Sold To Party if ISF does not contain a Buying Party", ZGuid.Empty, declaration3.JE_OA_SoldToPartyAddress);
			}
		}

		void AssertBill(Bill bill, ZString billNum, ZDecimal noOfPacks, ZString packType)
		{
			AssertEquals("bill.CU_BillNum", billNum, bill.CU_BillNum);
			AssertEquals("bill.CU_NoOfPacks", noOfPacks, bill.CU_NoOfPacks);
			AssertEquals("bill.CU_PackType", packType, bill.CU_PackType);
		}

		void CreateEDocsRows(Guid parentFK)
		{
			CreateEDocsRows(parentFK, new byte[] { 1, 2, 3, 4 }, "FileName", "XML");
		}

		void CreateEDocsRows(Guid parentFK, byte[] imageData, string filename, string dataType)
		{
			// don't have access to the bizos here, so use sql to create
			Guid sM_PK = Guid.NewGuid();
			Guid sC_PK = Guid.NewGuid();
			//byte[] imageData System.IO.File.ReadAllBytes(SamplePdfPath);

			string cmdString = "INSERT INTO " + StorageMainSchema.Constants.SqlSchemaName + "." + StorageMainSchema.Constants.TableName
				+ " (" + StorageMainSchema.Constants.PK + ", "
				+ StorageMainSchema.Constants.SM_Type + ", "
				+ StorageMainSchema.Constants.SM_ParentFK + ", "
				+ StorageMainSchema.Constants.SM_DB + " ) "
				+ @" VALUES 
				(@SM_PK, 
				@SM_Type, 
				@SM_ParentFK,
				@SM_DB)";
			var cmd = CargoWise.Data.Db.Connection.Command(cmdString); // don't have access to BizOs here (they're built after us), use SQL to create
			cmd.AddParameterBasedOnDbColumn("@SM_PK", sM_PK, StorageMainSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@SM_Type", "ORG", StorageMainSchema.SM_Type);
			cmd.AddParameterBasedOnDbColumn("@SM_ParentFK", parentFK, StorageMainSchema.SM_ParentFK);
			cmd.AddParameterBasedOnDbColumn("@SM_DB", 1, StorageMainSchema.SM_DB);
			cmd.ExecuteNonQuery();

			cmdString = string.Format("INSERT INTO {0}_SD001..", Db.DatabaseName.Trim()) + StorageDocsSchema.Constants.TableName
				+ " (" + StorageDocsSchema.Constants.PK + ", "
				+ StorageDocsSchema.Constants.SC_Date + ", "
				+ StorageDocsSchema.Constants.SC_SystemCreateTimeUtc + ", "
				+ StorageDocsSchema.Constants.SC_SystemLastEditTimeUtc + ", "
				+ StorageDocsSchema.Constants.SC_DocType + ", "
				+ StorageDocsSchema.Constants.SC_Desc + ", "
				+ StorageDocsSchema.Constants.SC_FileName + ", "
				+ StorageDocsSchema.Constants.SC_SM + ", "
				+ StorageDocsSchema.Constants.SC_ImageData + ", "
				+ StorageDocsSchema.Constants.SC_IsSystemGenerated + ", "
				+ StorageDocsSchema.Constants.SC_DataType + " ) "
				+ @" VALUES 
				(@SC_PK, 
				getdate(), 
				getdate(), 
				getdate(), 
				@SC_DocType, 
				@SC_Desc,
				@SC_FileName,
				@SC_SM, 
				@SC_ImageData,
				@SC_IsSystemGenerated,
				@SC_DataType)";
			cmd = CargoWise.Data.Db.Connection.Command(cmdString); // don't have access to BizOs here (they're built after us), use SQL to create
			cmd.AddParameterBasedOnDbColumn("@SC_PK", sC_PK, StorageDocsSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@SC_DocType", "ACV", StorageDocsSchema.SC_DocType);
			cmd.AddParameterBasedOnDbColumn("@SC_Desc", "This is a test", StorageDocsSchema.SC_Desc);
			cmd.AddParameterBasedOnDbColumn("@SC_FileName", filename, StorageDocsSchema.SC_FileName);
			cmd.AddParameterBasedOnDbColumn("@SC_SM", sM_PK, StorageDocsSchema.SC_SM);
			cmd.AddParameterBasedOnDbColumn("@SC_ImageData", imageData, StorageDocsSchema.SC_ImageData);
			cmd.AddParameterBasedOnDbColumn("@SC_IsSystemGenerated", "Y", StorageDocsSchema.SC_IsSystemGenerated);
			cmd.AddParameterBasedOnDbColumn("@SC_DataType", dataType, StorageDocsSchema.SC_DataType);
			cmd.ExecuteNonQuery();
		}

		void CreateMISCEDocsRows(Guid parentFK)
		{
			// don't have access to the bizos here, so use sql to create
			Guid sM_PK = Guid.NewGuid();
			Guid sC_PK = Guid.NewGuid();
			byte[] imageData = new byte[] { 1, 2, 3, 4 }; //System.IO.File.ReadAllBytes(SamplePdfPath);

			string cmdString = "INSERT INTO " + StorageMainSchema.Constants.SqlSchemaName + "." + StorageMainSchema.Constants.TableName
				+ " (" + StorageMainSchema.Constants.PK + ", "
				+ StorageMainSchema.Constants.SM_Type + ", "
				+ StorageMainSchema.Constants.SM_ParentFK + ", "
				+ StorageMainSchema.Constants.SM_DB + " ) "
				+ @" VALUES 
				(@SM_PK, 
				@SM_Type, 
				@SM_ParentFK,
				@SM_DB)";
			var cmd = CargoWise.Data.Db.Connection.Command(cmdString); // don't have access to BizOs here (they're built after us), use SQL to create
			cmd.AddParameterBasedOnDbColumn("@SM_PK", sM_PK, StorageMainSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@SM_Type", "ORG", StorageMainSchema.SM_Type);
			cmd.AddParameterBasedOnDbColumn("@SM_ParentFK", parentFK, StorageMainSchema.SM_ParentFK);
			cmd.AddParameterBasedOnDbColumn("@SM_DB", 1, StorageMainSchema.SM_DB);
			cmd.ExecuteNonQuery();

			cmdString = string.Format("INSERT INTO {0}_SD001..", Db.DatabaseName.Trim()) + StorageDocsSchema.Constants.TableName
				+ " (" + StorageDocsSchema.Constants.PK + ", "
				+ StorageDocsSchema.Constants.SC_Date + ", "
				+ StorageDocsSchema.Constants.SC_SystemCreateTimeUtc + ", "
				+ StorageDocsSchema.Constants.SC_SystemLastEditTimeUtc + ", "
				+ StorageDocsSchema.Constants.SC_DocType + ", "
				+ StorageDocsSchema.Constants.SC_Desc + ", "
				+ StorageDocsSchema.Constants.SC_FileName + ", "
				+ StorageDocsSchema.Constants.SC_SM + ", "
				+ StorageDocsSchema.Constants.SC_ImageData + ", "
				+ StorageDocsSchema.Constants.SC_IsSystemGenerated + ", "
				+ StorageDocsSchema.Constants.SC_DataType + " ) "
				+ @" VALUES 
				(@SC_PK, 
				getdate(), 
				getdate(), 
				getdate(), 
				@SC_DocType, 
				@SC_Desc,
				@SC_FileName,
				@SC_SM, 
				@SC_ImageData,
				@SC_IsSystemGenerated,
				@SC_DataType)";
			cmd = CargoWise.Data.Db.Connection.Command(cmdString); // don't have access to BizOs here (they're built after us), use SQL to create
			cmd.AddParameterBasedOnDbColumn("@SC_PK", sC_PK, StorageDocsSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@SC_DocType", Core.Constants.RefDocTypes.MiscellaneousDocument, StorageDocsSchema.SC_DocType);
			cmd.AddParameterBasedOnDbColumn("@SC_Desc", "MISC TEST", StorageDocsSchema.SC_Desc);
			cmd.AddParameterBasedOnDbColumn("@SC_FileName", "FileName", StorageDocsSchema.SC_FileName);
			cmd.AddParameterBasedOnDbColumn("@SC_SM", sM_PK, StorageDocsSchema.SC_SM);
			cmd.AddParameterBasedOnDbColumn("@SC_ImageData", imageData, StorageDocsSchema.SC_ImageData);
			cmd.AddParameterBasedOnDbColumn("@SC_IsSystemGenerated", "Y", StorageDocsSchema.SC_IsSystemGenerated);
			cmd.AddParameterBasedOnDbColumn("@SC_DataType", "XML", StorageDocsSchema.SC_DataType);
			cmd.ExecuteNonQuery();
		}

		void AssertInvoiceLine(JobComInvoiceLine invoiceLine, ZString partNo, ZString countryOfOrigin, ZString tariff, ZGuid manufacturerPK)
		{
			AssertEquals(partNo, invoiceLine.JI_PartNo);
			AssertEquals(countryOfOrigin, invoiceLine.US_UC_NKCountryOfOrigin);
			AssertEquals(tariff, invoiceLine.JI_Tariff);
			AssertEquals(manufacturerPK, invoiceLine.JI_OA_ManufacturerAddress);
		}

		void AsserContainer(CusContainer cusContainer, ZString containerNumber, RefContainer containerType)
		{
			AssertEquals(containerNumber, cusContainer.CO_ContainerNumber);
			AssertEquals(containerType.PK, cusContainer.CO_RC);
		}

		void AddBill(CusISFHeader header, ZString billNum, ZString billType)
		{
			CusISFBill bill = header.ReferenceDatas.AddNew();
			bill.BB_BillNum = billNum;
			bill.BB_BillType = billType;
		}
	}
}
