using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.Customs.US.Business.OrgSupplierPart;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondCargoDescValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2020, 11, 15)]
		public void TestCheckBY_FormattedHarmonisedTariffWithEffectiveDateNow()
		{
			USCTariff tariffA = Factory.New<USCTariff>();
			tariffA.UE_Tariff = "6671101001";
			tariffA.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariffA.UE_DateTo = new ZDateTime(2020, 09, 09);
			USCTariff tariffB = Factory.New<USCTariff>();
			tariffB.UE_Tariff = "2221101002";
			tariffB.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariffB.UE_DateTo = new ZDateTime(2099, 12, 31);
			Factory.Save();
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_ETA = new ZDateTime(2021, 01, 25);
			header.BH_FTZMove = true;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			var moveHeader = header.MovementHeaders.AddNew();
			AssertEquals(true, moveHeader.SupportsBondedWarehousing);
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container1 = moveDetail.Containers.AddNew();
			var commodity1 = container1.Commodities.AddNew();
			commodity1.BY_PieceCount = 10;
			commodity1.BY_FormattedHarmonisedTariff = tariffA.UE_Tariff;
			var commodity2 = container1.Commodities.AddNew();
			commodity2.BY_PieceCount = 20;
			commodity2.BY_FormattedHarmonisedTariff = tariffB.UE_Tariff;
			commodity1.Validation.ValidateBY_HarmonisedTariff();
			AssertHasMessageErrorContaining("'Tariff was found but is not on file for '15-Nov-20'.", commodity1.BY_FormattedHarmonisedTariffInfo, ValidationConstants.Commodity.TariffWasFoundButNotOnFile(commodity1.EffectiveDateForDutyRate.ToShortDateString()));
			commodity2.Validation.ValidateBY_HarmonisedTariff();
			AssertNoMessageErrorContaining("No Error message on Tariff for '15-Nov-20'.", commodity2.BY_FormattedHarmonisedTariffInfo, ValidationConstants.Commodity.TariffWasFoundButNotOnFile(commodity2.EffectiveDateForDutyRate.ToShortDateString()));
			var commodity3 = container1.Commodities.AddNew();
			commodity3.BY_PieceCount = 20;
			commodity3.BY_FormattedHarmonisedTariff = "5555555";
			commodity3.Validation.ValidateBY_HarmonisedTariff();
			AssertHasMessageErrorContaining("Error Tariff 5555555 is not found.", commodity3.BY_FormattedHarmonisedTariffInfo, ValidationConstants.Commodity.GetTariffNotFound("5555555"));
		}

		[TestDate(2020, 11, 20)]
		public void TestCheckBY_FormattedHarmonisedTariff()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1111101000";
			tariff1.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff1.UE_DateTo = ZDateTime.Today.AddYears(1);
			USCTariff tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "1111101030";
			tariff3.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff3.UE_DateTo = ZDateTime.Today.AddYears(1);
			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "2222101000";
			tariff2.UE_DateFrom = ZDateTime.Today.AddYears(1);
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(2);
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			CusInBondContainer container = moveDetail.Containers.AddNew();
			CusInBondCargoDesc commodity = container.Commodities.AddNew();
			string tariffFoundNotOnFileMessage = "Tariff was found but is not on file for '20-Nov-20'.";
			string tariffNotFound = "Tariff '1010101010' is not recognized as a valid tariff.";
			commodity.BY_FormattedHarmonisedTariff = ZString.Empty;
			AssertNoMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, tariffFoundNotOnFileMessage);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			commodity.BY_FormattedHarmonisedTariff = ZString.Empty;
			AssertHasMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, tariffFoundNotOnFileMessage);
			commodity.BY_FormattedHarmonisedTariff = string.Format(tariffNotFound, "1010101010");
			AssertNoMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, tariffNotFound);
			commodity.BY_FormattedHarmonisedTariff = "2222101000";
			AssertNoMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, tariffFoundNotOnFileMessage);
			commodity.BY_FormattedHarmonisedTariff = "11111";
			AssertNoMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, tariffFoundNotOnFileMessage);
			AssertHasMessageError(commodity.BY_FormattedHarmonisedTariffInfo, ValidationConstants.Commodity.TariffNumShouldBeAtLeast6Digits.ToString());
			commodity.BY_FormattedHarmonisedTariff = "111110";
			AssertNoMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, tariffFoundNotOnFileMessage);
			AssertNoMessageError(commodity.BY_FormattedHarmonisedTariffInfo, ValidationConstants.Commodity.TariffNumShouldBeAtLeast6Digits.ToString());
			commodity.BY_FormattedHarmonisedTariff = "1111101000";
			AssertNoMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, tariffFoundNotOnFileMessage);
			commodity.BY_FormattedHarmonisedTariff = "1111101010";
			tariffNotFound = "Tariff '1111101010' is not recognized as a valid tariff.";
			AssertNoMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, tariffNotFound);
			var childCommodity = commodity.ChildCommodities.AddNew();
			commodity.BY_FormattedHarmonisedTariff = "1111101030";
			tariffNotFound = "Tariff '1111101010' is not recognized as a valid tariff.";
			AssertNoMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, tariffNotFound);
			AssertNoMessageErrorContaining(commodity.BY_FormattedHarmonisedTariffInfo, tariffFoundNotOnFileMessage);
		}

		public void TestPartAttribute1Validation()
		{
			SetUpPartAttributeValidation();
			CheckPartAttributeValidation(commodity.BY_PartAttrib1Info, () => lineValidation.ValidateBY_PartAttrib1(), 1);
		}

		public void TestPartAttribute2Validation()
		{
			SetUpPartAttributeValidation();
			CheckPartAttributeValidation(commodity.BY_PartAttrib2Info, () => lineValidation.ValidateBY_PartAttrib2(), 2);
		}

		public void TestPartAttribute3Validation()
		{
			SetUpPartAttributeValidation();
			CheckPartAttributeValidation(commodity.BY_PartAttrib3Info, () => lineValidation.ValidateBY_PartAttrib3(), 3);
		}

		public void TestSerialNumberValidation()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_IsConsignee = true;
			importer.OH_FullName = "Test Importer";
			importer.MainAddress.OA_Address1 = "1 Importer Lane";
			importer.OH_Code = "IMP2293SYD";
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "~~~";
			part.RelatedOrganisations.AddOrganisationIfNotExist(header.ImporterOrgPK, OrgPartRelation.RelationshipTypes.Owner);
			commodity.BY_PartNumber = "~~~";
			AssertNotNull("Not null part", commodity.Part);
			commodity.BY_SerialNumber = "SN";
			commodity.Validation.ValidateBY_SerialNumber();
			AssertHasError(commodity.BY_SerialNumberInfo, "The part '~~~' is not set up to use Serial Number with the Importer 'Test Importer'. Please either remove the value 'SN' from the Serial Number field, or set up the Product and Importer to use Serial Number.");
		}

		public void TestSerialNumberValidation_MandatorySerialNumber()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_IsConsignee = true;
			importer.OH_FullName = "Test Importer";
			importer.MainAddress.OA_Address1 = "1 Importer Lane";
			importer.OH_Code = "IMP2293SYD";
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			importer.MiscServ.OM_IMUseSerialNumber = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "~~~";
			part.RelatedOrganisations.AddOrganisationIfNotExist(header.ImporterOrgPK, OrgPartRelation.RelationshipTypes.Owner);
			part.RelatedOrganisations[0].OU_UseSerialNumber = true;
			commodity.BY_PartNumber = "~~~";
			AssertNotNull("Not null part", commodity.Part);
			commodity.Validation.ValidateBY_SerialNumber();
			AssertHasError(commodity.BY_SerialNumberInfo, "Please enter a Serial Number.");
			commodity.BY_SerialNumber = "SN";
			commodity.Validation.ValidateBY_SerialNumber();
			AssertNoErrors(commodity.BY_SerialNumberInfo);
		}

		[TestDate(2014, 7, 20)]
		public void TestBY_PartNumberMatchETA()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "IMP2342";
			org1.OH_IsConsignee = true;
			org1.MiscServ.OM_IMPartAttrib1Name = "PA 1";
			org1.MiscServ.OM_IMPartAttrib1Type = "MAN";
			org1.MiscServ.OM_IMPartAttrib2Name = "PA 2";
			org1.MiscServ.OM_IMPartAttrib2Type = "MAN";
			org1.MiscServ.OM_IMPartAttrib3Name = "PA 3";
			org1.MiscServ.OM_IMPartAttrib3Type = "MAN";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "SUP2342";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART1";
			var relationShip1 = part.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			var relationShip2 = part.RelatedOrganisations.AddOrganisationIfNotExist(org2.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_OH = relationShip1.OU_OH;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1010101010";
			pivot.CI_DateStart = new ZDateTime(2014, 8, 1);
			var attrib1 = pivot.Attributes1.AddNew();
			attrib1.BG_AttributeValue1 = "1";
			var attrib2 = pivot.Attributes2.AddNew();
			attrib2.BG_AttributeValue1 = "2";
			var attrib3 = pivot.Attributes3.AddNew();
			attrib3.BG_AttributeValue1 = "2";
			var header = Factory.New<CusInBondHeader>();
			header.BH_ETA = new ZDateTime(2014, 7, 20);
			header.BH_OA_Importer = org1.MainAddress.PK;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			var commodity = container.Commodities.AddNew();
			commodity.BY_OH_Supplier = org2.PK;
			commodity.BY_PartNumber = "PART1";
			commodity.BY_PartAttrib1 = "1";
			commodity.BY_PartAttrib2 = "2";
			commodity.BY_PartAttrib3 = "2";
			commodity.BY_SerialNumber = "SN";
			var warningMessage = $"Cannot match classification for product (PART1) based on Importer (IMP2342), Supplier (SUP2342), PA 1 (1), PA 2 (2), PA 3 (2), Serial Number (SN) and Effective Date ({new ZDateTime(2014, 7, 20)}).";
			AssertHasWarning(commodity.BY_PartNumberInfo, warningMessage);
			container.Commodities.DeleteAll();
			pivot.CI_DateStart = new ZDateTime(2014, 4, 1);
			var commodity2 = container.Commodities.AddNew();
			commodity2.BY_OH_Supplier = org2.PK;
			commodity2.BY_PartNumber = "PART1";
			commodity2.BY_PartAttrib1 = "1";
			commodity2.BY_PartAttrib2 = "2";
			commodity2.BY_PartAttrib3 = "2";
			commodity2.BY_SerialNumber = "SN";
			commodity2.Validation.ValidateBY_PartNumber();
			AssertNoWarningContaining(commodity2.BY_PartNumberInfo, warningMessage);
		}

		public void TestBY_PartNumberWarnsIfThereIsMoreThanOneMatch()
		{
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			USCustomsDataRegistry.Instance.AnInBondCommodityMustHaveAValidProduct.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@");
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = Importer.MainAddress.PK;
			header.BH_OH_Supplier = Supplier.PK;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			var supplier2 = Factory.New<OrgHeader>();
			supplier2.FillWithValidTestData();
			var supplier3 = Factory.New<OrgHeader>();
			supplier3.FillWithValidTestData();
			var testValidPart = Factory.New<OrgSupplierPart>();
			testValidPart.OP_PartNum = TestValidPartNo;
			testValidPart.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);
			testValidPart.RelatedOrganisations.AddOrganisationIfNotExist(header.ImporterOrgPK, OrgPartRelation.RelationshipTypes.Owner);
			var testValidPart2 = Factory.New<OrgSupplierPart>();
			testValidPart2.OP_PartNum = TestValidPartNo;
			testValidPart2.RelatedOrganisations.AddOrganisationIfNotExist(supplier3.PK, OrgPartRelation.RelationshipTypes.Supplier);
			testValidPart2.RelatedOrganisations.AddOrganisationIfNotExist(header.ImporterOrgPK, OrgPartRelation.RelationshipTypes.Owner);
			var line = container.Commodities.AddNew();
			line.BY_PartNumber = TestValidPartNo;
			AssertEquals("Line.PartSyncManager.TotalMatchCount", 2, line.PartSyncManager.TotalMatchCount);
			AssertHasWarningContaining(line.BY_PartNumberInfo, ValidationConstants.Commodity.WarningMoreThanOneProductMatchFound);
			AssertNoMessageErrorContaining(line.BY_PartNumberInfo, ValidationConstants.Commodity.WarningMoreThanOneProductMatchFound);
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			line.BY_PartNumber = "";
			line.BY_PartNumber = TestValidPartNo;
			AssertNoWarningContaining(line.BY_PartNumberInfo, ValidationConstants.Commodity.WarningMoreThanOneProductMatchFound);
			AssertHasMessageErrorContaining(line.BY_PartNumberInfo, ValidationConstants.Commodity.WarningMoreThanOneProductMatchFound);
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			line.BY_PartNumber = "";
			line.BY_PartNumber = TestValidPartNo;
			AssertEquals("Line.PartSyncManager.TotalMatchCount", 0, line.PartSyncManager.TotalMatchCount);
			AssertNoWarningContaining(line.BY_PartNumberInfo, ValidationConstants.Commodity.WarningMoreThanOneProductMatchFound);
			AssertNoMessageErrorContaining(line.BY_PartNumberInfo, ValidationConstants.Commodity.WarningMoreThanOneProductMatchFound);
			moveHeader.BM_WarehouseTransactionStatus = ZString.Empty;
			line.BY_PartNumber = "";
			line.BY_PartNumber = TestValidPartNo;
			AssertEquals("Line.PartSyncManager.TotalMatchCount", 0, line.PartSyncManager.TotalMatchCount);
			AssertNoWarningContaining(line.BY_PartNumberInfo, ValidationConstants.Commodity.WarningMoreThanOneProductMatchFound);
			AssertNoMessageErrorContaining(line.BY_PartNumberInfo, ValidationConstants.Commodity.WarningMoreThanOneProductMatchFound);
		}

		public void TestBY_PartNumberWarnsIfSupplierOrImporterAreNotEntered()
		{
			USCustomsDataRegistry.Instance.AnInBondCommodityMustHaveAValidProduct.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@");
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			var commodity = container.Commodities.AddNew();
			commodity.BY_PartNumber = "IHOPENOBODYADDSTHIS";
			AssertHasWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCannotBeFoundBeforeEnteringASupplierOrAnImporter);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCannotBeFoundBeforeEnteringASupplierOrAnImporter);
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			commodity.Validation.ValidateBY_PartNumber();
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCannotBeFoundBeforeEnteringASupplierOrAnImporter);
			AssertHasMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCannotBeFoundBeforeEnteringASupplierOrAnImporter);
			header.BH_OA_Importer = Importer.MainAddress.PK;
			commodity.BY_OH_Supplier = Supplier.PK;
			commodity.BY_PartNumber = "ORTHISONEEITHER";
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCannotBeFoundBeforeEnteringASupplierOrAnImporter);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCannotBeFoundBeforeEnteringASupplierOrAnImporter);
			moveHeader.BM_WarehouseTransactionStatus = ZString.Empty;
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCannotBeFoundBeforeEnteringASupplierOrAnImporter);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCannotBeFoundBeforeEnteringASupplierOrAnImporter);
		}

		public void TestBY_PartNumberWarnsWhenFoundButNotRelatedOrInactive()
		{
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var importer1 = Importer;
			var supplier1 = Supplier;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			USCustomsDataRegistry.Instance.AnInBondCommodityMustHaveAValidProduct.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var helper = new WhsDataTestHelper(newFactory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@");
			var header = newFactory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			var commodity = container.Commodities.AddNew();
			header.BH_OA_Importer = importer1.MainAddress.PK;
			commodity.BY_OH_Supplier = supplier1.PK;
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier2 = Factory.New<OrgHeader>();
			supplier2.FillWithValidTestData();
			var importer3 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier3 = Factory.New<OrgHeader>();
			supplier3.FillWithValidTestData();
			var validPart1 = GetNewPart(TestValidPartNo, importer2, supplier2);
			Factory.Save();
			commodity.BY_PartNumber = TestValidPartNo;
			AssertHasWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			commodity.Validation.ValidateBY_PartNumber();
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			AssertHasMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			var validPart2 = GetNewPart(TestValidPartNo, importer3, supplier3);
			Factory.Save();
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertHasMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			moveHeader.BM_WarehouseTransactionStatus = ZString.Empty;
			commodity.Validation.ValidateBY_PartNumber();
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertHasWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			var validPart3 = GetNewPart(TestValidPartNo, importer1, supplier1);
			Factory.Save();
			commodity.Validation.ValidateBY_PartNumber();
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			commodity.Validation.ValidateBY_PartNumber();
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			validPart3.OP_IsActive = false;
			Factory.Save();
			commodity.Validation.ValidateBY_PartNumber();
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertHasMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			moveHeader.BM_WarehouseTransactionStatus = ZString.Empty;
			commodity.Validation.ValidateBY_PartNumber();
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertHasWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			var commodity2 = container.Commodities.AddNew();
			validPart3.OP_PartNum = "ANYTHING";
			Factory.Save();
			commodity2.BY_PartNumber = "ANYTHING";
			AssertHasWarning(commodity2.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoWarning(commodity2.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity2.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity2.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			commodity2.Validation.ValidateBY_PartNumber();
			AssertNoWarning(commodity2.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoWarning(commodity2.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			AssertHasMessageError(commodity2.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity2.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			validPart3.OP_IsActive = true;
			Factory.Save();
			var commodity3 = container.Commodities.AddNew();
			commodity3.BY_PartNumber = "ANYTHING";
			AssertNoWarning(commodity3.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoWarning(commodity3.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity3.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity3.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			moveHeader.BM_WarehouseTransactionStatus = ZString.Empty;
			commodity3.Validation.ValidateBY_PartNumber();
			AssertNoWarning(commodity3.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoWarning(commodity3.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity3.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity3.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
		}

		public void TestBY_PartNumberWarnsWhenFoundButNotRelatedWithComplexRelationships()
		{
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var importer1 = Importer;
			var supplier1 = Supplier;
			USCustomsDataRegistry.Instance.AnInBondCommodityMustHaveAValidProduct.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@");
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			var commodity = container.Commodities.AddNew();
			header.BH_OA_Importer = importer1.MainAddress.PK;
			commodity.BY_OH_Supplier = supplier1.PK;
			var supplier2 = Factory.New<OrgHeader>();
			supplier2.FillWithValidTestData();
			var supplier3 = Factory.New<OrgHeader>();
			supplier3.FillWithValidTestData();
			var validPart = GetNewPart(TestValidPartNo, supplier1, supplier1);
			validPart.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Both);
			validPart.RelatedOrganisations.AddOrganisationIfNotExist(supplier3.PK, OrgPartRelation.RelationshipTypes.Owner);
			var validOwner = validPart.RelatedOrganisations.AddOrganisationIfNotExist(importer1.PK, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();
			commodity.BY_PartNumber = TestValidPartNo;
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			validOwner.Delete();
			Factory.Save();
			commodity.BY_PartNumber = "";
			commodity.BY_PartNumber = TestValidPartNo;
			AssertHasWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			commodity.BY_PartNumber = "";
			commodity.BY_PartNumber = TestValidPartNo;
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertHasMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			validOwner = validPart.RelatedOrganisations.AddOrganisationIfNotExist(importer1.PK, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();
			commodity.BY_PartNumber = "";
			commodity.BY_PartNumber = TestValidPartNo;
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
		}

		public void TestCheckBY_InvoiceQuantity()
		{
			USCustomsDataRegistry.Instance.AnInBondCommodityMustHaveAValidProduct.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@");
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = helper.Importer.MainAddress.PK;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			var commodity = container.Commodities.AddNew();
			commodity.BY_PartNumber = helper.Part.OP_PartNum;
			commodity.BY_InvoiceQuantity = 0m;
			AssertHasMessageError(commodity.BY_InvoiceQuantityInfo, ValidationConstants.Commodity.ABondedWarehousingCommodityRequiresAnInvoiceQuantity);
			commodity.BY_InvoiceQuantity = 100m;
			AssertNoMessageError(commodity.BY_InvoiceQuantityInfo, ValidationConstants.Commodity.ABondedWarehousingCommodityRequiresAnInvoiceQuantity);
			moveHeader.BM_WarehouseTransactionStatus = ZString.Empty;
			commodity.BY_InvoiceQuantity = 0m;
			AssertNoMessageError(commodity.BY_InvoiceQuantityInfo, ValidationConstants.Commodity.ABondedWarehousingCommodityRequiresAnInvoiceQuantity);
		}

		public void TestCheckBY_WarehouseEntryNumber()
		{
			USCustomsDataRegistry.Instance.AnInBondCommodityMustHaveAValidProduct.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@");
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = helper.Importer.MainAddress.PK;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			var commodity = container.Commodities.AddNew();
			commodity.BY_PartNumber = helper.Part.OP_PartNum;
			commodity.BY_WarehouseEntryNumber = ZString.Empty;
			AssertHasMessageError(commodity.BY_WarehouseEntryNumberInfo, ValidationConstants.Commodity.ABondedWarehousingCommodityRequiresEntryDetails);
			commodity.BY_WarehouseEntryNumber = "ZJ5-343";
			AssertNoMessageError(commodity.BY_WarehouseEntryNumberInfo, ValidationConstants.Commodity.ABondedWarehousingCommodityRequiresEntryDetails);
			moveHeader.BM_WarehouseTransactionStatus = ZString.Empty;
			commodity.BY_WarehouseEntryNumber = ZString.Empty;
			AssertNoMessageError(commodity.BY_WarehouseEntryNumberInfo, ValidationConstants.Commodity.ABondedWarehousingCommodityRequiresEntryDetails);
		}

		public void TestCheckBY_WarehouseEntryLineNo()
		{
			USCustomsDataRegistry.Instance.AnInBondCommodityMustHaveAValidProduct.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@");
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = helper.Importer.MainAddress.PK;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			var commodity = container.Commodities.AddNew();
			commodity.BY_PartNumber = helper.Part.OP_PartNum;
			commodity.BY_WarehouseEntryLineNo = 0;
			AssertHasMessageError(commodity.BY_WarehouseEntryLineNoInfo, ValidationConstants.Commodity.ABondedWarehousingCommodityRequiresEntryDetails);
			commodity.BY_WarehouseEntryLineNo = 1;
			AssertNoMessageError(commodity.BY_WarehouseEntryLineNoInfo, ValidationConstants.Commodity.ABondedWarehousingCommodityRequiresEntryDetails);
			moveHeader.BM_WarehouseTransactionStatus = ZString.Empty;
			commodity.BY_WarehouseEntryLineNo = 0;
			AssertNoMessageError(commodity.BY_WarehouseEntryLineNoInfo, ValidationConstants.Commodity.ABondedWarehousingCommodityRequiresEntryDetails);
		}

		public void TestBY_PartNumberWarnsWhenPartCodeNotFoundAtAll()
		{
			var importer = Importer;
			var supplier = Supplier;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			USCustomsDataRegistry.Instance.AnInBondCommodityMustHaveAValidProduct.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var helper = new WhsDataTestHelper(newFactory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@");
			var header = newFactory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			var commodity = container.Commodities.AddNew();
			header.BH_OA_Importer = importer.MainAddress.PK;
			commodity.BY_OH_Supplier = supplier.PK;
			commodity.BY_PartNumber = TestValidPartNo;
			AssertHasWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeNotFoundAtAll);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeNotFoundAtAll);
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			commodity.Validation.ValidateBY_PartNumber();
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeNotFoundAtAll);
			AssertHasMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeNotFoundAtAll);
			var validPart = GetNewPart(TestValidPartNo, importer, supplier);
			Factory.Save();
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeNotFoundAtAll);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeNotFoundAtAll);
			moveHeader.BM_WarehouseTransactionStatus = ZString.Empty;
			commodity.Validation.ValidateBY_PartNumber();
			AssertNoWarning(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeNotFoundAtAll);
			AssertNoMessageError(commodity.BY_PartNumberInfo, ValidationConstants.Commodity.WarningPartCodeNotFoundAtAll);
		}

		public void TestCheckBY_MonetaryValue()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			CusInBondContainer container = moveDetail.Containers.AddNew();
			CusInBondCargoDesc commodity = container.Commodities.AddNew();
			commodity.BY_MonetaryValue = ZDecimal.Zero;
			AssertNoMessageError(commodity.BY_MonetaryValueInfo, ValidationConstants.Commodity.MonetaryValueIsRequired.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			commodity.BY_MonetaryValue = ZDecimal.Zero;
			AssertHasMessageError(commodity.BY_MonetaryValueInfo, ValidationConstants.Commodity.MonetaryValueIsRequired.ToString());
			commodity.BY_MonetaryValue = 10m;
			AssertNoMessageError(commodity.BY_MonetaryValueInfo, ValidationConstants.Commodity.MonetaryValueIsRequired.ToString());
			var childCommodity = commodity.ChildCommodities.AddNew();
			commodity.BY_MonetaryValue = ZDecimal.Zero;
			AssertNoMessageError(commodity.BY_MonetaryValueInfo, ValidationConstants.Commodity.MonetaryValueIsRequired.ToString());
		}

		public void TestCheckBY_GrossWeight()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			CusInBondContainer container = moveDetail.Containers.AddNew();
			CusInBondCargoDesc commodity = container.Commodities.AddNew();
			commodity.BY_GrossWeight = ZDecimal.Zero;
			AssertNoMessageError(commodity.BY_GrossWeightInfo, ValidationConstants.Commodity.WeightMustBeGreaterThanZero.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			commodity.BY_GrossWeight = ZDecimal.Zero;
			AssertHasMessageError(commodity.BY_GrossWeightInfo, ValidationConstants.Commodity.WeightMustBeGreaterThanZero.ToString());
			commodity.BY_GrossWeight = 10m;
			AssertNoMessageError(commodity.BY_GrossWeightInfo, ValidationConstants.Commodity.WeightMustBeGreaterThanZero.ToString());
			commodity.ChildCommodities.AddNew();
			commodity.BY_GrossWeight = ZDecimal.Zero;
			AssertNoMessageError(commodity.BY_GrossWeightInfo, ValidationConstants.Commodity.WeightMustBeGreaterThanZero.ToString());
		}

		public void TestCheckBY_GrossWeightUnit()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			CusInBondContainer container = moveDetail.Containers.AddNew();
			CusInBondCargoDesc commodity = container.Commodities.AddNew();
			commodity.BY_GrossWeightUnit = ZString.Empty;
			AssertNoMessageErrorContaining(commodity.BY_GrossWeightUnitInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			commodity.BY_GrossWeightUnit = ZString.Empty;
			AssertHasMessageErrorContaining(commodity.BY_GrossWeightUnitInfo, MandatoryValidation.YouHaveNotEntered);
			commodity.BY_GrossWeightUnit = "Z!";
			AssertNoMessageErrorContaining(commodity.BY_GrossWeightUnitInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(commodity.BY_GrossWeightUnitInfo, ListValidation.InvalidCodeMessageError);
			foreach (CodeDescriptionPair pair in new CodeDescriptionPairList(OLookUpEditType.Weight))
			{
				commodity.BY_GrossWeightUnit = pair.Code;
				AssertNoMessageErrorContaining(commodity.BY_GrossWeightUnitInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(commodity.BY_GrossWeightUnitInfo, ListValidation.InvalidCodeMessageError);
			}

			commodity.ChildCommodities.AddNew();
			commodity.BY_GrossWeightUnit = "Z!";
			AssertNoMessageErrorContaining(commodity.BY_GrossWeightUnitInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(commodity.BY_GrossWeightUnitInfo, ListValidation.InvalidCodeMessageError);
			commodity.BY_GrossWeightUnit = "";
			AssertNoMessageErrorContaining(commodity.BY_GrossWeightUnitInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(commodity.BY_GrossWeightUnitInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBY_PieceCount()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			CusInBondContainer container = moveDetail.Containers.AddNew();
			container.BC_PieceCount = 0;
			CusInBondCargoDesc commodity = container.Commodities.AddNew();
			commodity.BY_PieceCount = ZInt.Zero;
			AssertNoMessageError(commodity.BY_PieceCountInfo, ValidationConstants.Commodity.PieceCountMustBeGreaterThanZero.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			commodity.BY_PieceCount = ZInt.Zero;
			AssertNoMessageError(commodity.BY_PieceCountInfo, ValidationConstants.Commodity.PieceCountMustBeGreaterThanZero.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			commodity.BY_PieceCount = ZInt.Zero;
			AssertHasMessageError(commodity.BY_PieceCountInfo, ValidationConstants.Commodity.PieceCountMustBeGreaterThanZero.ToString());
			commodity.BY_PieceCount = 10;
			AssertNoMessageError(commodity.BY_PieceCountInfo, ValidationConstants.Commodity.PieceCountMustBeGreaterThanZero.ToString());
			commodity.BY_PieceCount = -1;
			AssertHasMessageError(commodity.BY_PieceCountInfo, ValidationConstants.Commodity.PieceCountMustBeGreaterThanZero.ToString());
			var childCommodity = commodity.ChildCommodities.AddNew();
			childCommodity.BY_PieceCount = -1;
			AssertNoMessageError(childCommodity.BY_PieceCountInfo, ValidationConstants.Commodity.PieceCountMustBeGreaterThanZero.ToString());
			container.BC_PieceCount = 200;
			commodity.BY_PieceCount = ZInt.Zero;
			AssertNoMessageError(commodity.BY_PieceCountInfo, ValidationConstants.Commodity.PieceCountMustBeGreaterThanZero.ToString());
			container.BC_PieceCount = 200;
			commodity.BY_PieceCount = 505;
			AssertHasWarning(commodity.BY_PieceCountInfo, ValidationConstants.Commodity.PieceCountIsNotRequired.ToString());
		}

		public void TestCheckBY_Description()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			CusInBondContainer container = moveDetail.Containers.AddNew();
			CusInBondCargoDesc commodity = container.Commodities.AddNew();
			commodity.BY_Description = ZString.Empty;
			AssertNoMessageErrorContaining(commodity.BY_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			commodity.BY_Description = ZString.Empty;
			AssertHasMessageErrorContaining(commodity.BY_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			commodity.BY_Description = "HELLO WORLD";
			AssertNoMessageErrorContaining(commodity.BY_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			var childCommodity = commodity.ChildCommodities.AddNew();
			childCommodity.BY_Description = ZString.Empty;
			AssertNoMessageErrorContaining(childCommodity.BY_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBY_MarksAndNumbers()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			commodity.BY_MarksAndNumbers = ZString.Empty;
			AssertNoMessageErrorContaining(commodity.BY_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			commodity.BY_MarksAndNumbers = ZString.Empty;
			AssertNoMessageErrorContaining(commodity.BY_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			commodity.BY_MarksAndNumbers = ZString.Empty;
			AssertHasMessageErrorContaining(commodity.BY_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			header.BH_FTZMove = true;
			commodity.Validation.ValidateBY_MarksAndNumbers();
			AssertHasMessageErrorContaining(commodity.BY_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
			commodity.BY_MarksAndNumbers = "MARKS";
			AssertNoMessageErrorContaining(commodity.BY_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
			var childCommodity = commodity.ChildCommodities.AddNew();
			childCommodity.BY_MarksAndNumbers = ZString.Empty;
			AssertNoMessageErrorContaining(childCommodity.BY_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
		}

		void CheckPartAttributeValidation(ZPropertyInfo attributeInfo, Action validationInvoker, int attributeNo)
		{
			using (new PartAttributeValidationChecker.AttributeCallChecker(header.ImporterOrg, commodity.Part, attributeInfo, attributeNo))
			{
				validationInvoker.Invoke();
			}
		}

		CusInBondHeader header;
		CusInBondCargoDesc commodity;
		CusInBondCargoDescValidation lineValidation;
		void SetUpPartAttributeValidation()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_IsConsignee = true;
			importer.OH_FullName = "Test Importer";
			importer.MainAddress.OA_Address1 = "1 Importer Lane";
			importer.OH_Code = "IMP2293SYD";
			header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			commodity = container.Commodities.AddNew();
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "~~~";
			part.RelatedOrganisations.AddOrganisationIfNotExist(header.ImporterOrgPK, OrgPartRelation.RelationshipTypes.Owner);
			commodity.BY_PartNumber = "~~~";
			AssertNotNull("Not null part", commodity.Part);
			lineValidation = commodity.Validation;
		}

		OrgSupplierPart GetNewPart(ZString partNo, OrgHeader importer, OrgHeader supplier)
		{
			var result = Factory.New<OrgSupplierPart>();
			result.OP_PartNum = partNo;
			result.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			result.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			return result;
		}

		OrgHeader importer;
		OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = Factory.New<OrgHeader>();
					importer.OH_IsConsignee = true;
					importer.OH_FullName = "Test Importer";
					importer.MainAddress.OA_Address1 = "1 Importer Lane";
					importer.OH_Code = "IMP2293SYD";
					importer.CompanyData.OB_IMUsedBondedWhs = true;
				}

				return importer;
			}
		}

		OrgHeader supplier;
		OrgHeader Supplier
		{
			get
			{
				if (supplier == null)
				{
					supplier = Factory.New<OrgHeader>();
					supplier.FillWithValidTestData();
					supplier.OH_IsConsignor = true;
					supplier.OH_FullName = "Test Supplier";
					supplier.MainAddress.OA_Address1 = "1 Supplier Court";
					supplier.OH_Code = "SUP3348USA";
				}

				return supplier;
			}
		}

		const string TestValidPartNo = "T44330";
	}
}
