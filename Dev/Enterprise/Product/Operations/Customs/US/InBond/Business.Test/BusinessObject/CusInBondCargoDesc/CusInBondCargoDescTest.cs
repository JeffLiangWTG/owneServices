using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.Customs.US.Business.OrgSupplierPart;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondCargoDesc))]
	sealed class CusInBondCargoDescTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUniversalCopy()
		{
			var ignoreElementAttributes = (UniversalCopyIgnoreElementAttribute[])typeof(CusInBondCargoDesc).GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), false);
			AssertEquals(1, ignoreElementAttributes.Length);
			AssertEquals(1, ignoreElementAttributes[0].ElementNames.Count);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.Commodity, ignoreElementAttributes[0].ElementNames);
		}

		public void TestIInBondTariffLineDetailsMembers()
		{
			var validator = new WeightValidator();
			IInBondTariffLineDetails lineDetails = CargoDesc;
			CargoDesc.BY_Description = "GOOD LOOKING BOB";
			AssertEquals("CargoDescription", "GOOD LOOKING BOB", lineDetails.CargoDescription);
			CargoDesc.BY_MonetaryValue = 10m;
			AssertEquals("CustomsValue", 10m, lineDetails.CustomsValue);
			CargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
			CargoDesc.BY_GrossWeight = 14m;
			AssertEquals("NetWeight", 14m, lineDetails.NetWeight);
			CargoDesc.BY_GrossWeight = 14.9m;
			AssertEquals("NetWeight", 15m, lineDetails.NetWeight);
			CargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Kilotonnes;
			AssertEquals("NetWeight", 14900000m, lineDetails.NetWeight);
			CargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals("NetWeight", 15m, lineDetails.NetWeight);
			CargoDesc.BY_GrossWeight = validator.MaximumWholeWeightAllowed + 1;
			AssertEquals("NetWeight", ZDecimal.Zero, lineDetails.NetWeight);
			CargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
			AssertEquals("NetWeight", ZDecimal.Zero, lineDetails.NetWeight);
			CargoDesc.BY_GrossWeight = validator.MaximumWholeWeightAllowed;
			AssertEquals("NetWeight", validator.MaximumWholeWeightAllowed, lineDetails.NetWeight);
			CargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals("NetWeight", validator.MaximumWholeWeightAllowed, lineDetails.NetWeight);
			CargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
			AssertEquals("NetWeightUQ", Core.Constants.Weight.Pounds, lineDetails.NetWeightUQ);
			CargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals("NetWeightUQ", Core.Constants.Weight.Kilograms, lineDetails.NetWeightUQ);
			CargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Grams;
			AssertEquals("NetWeightUQ", Core.Constants.Weight.Kilograms, lineDetails.NetWeightUQ);
			CargoDesc.BY_GrossWeightUnit = "Z!";
			AssertEquals("NetWeightUQ", "Z!", lineDetails.NetWeightUQ);
			CargoDesc.BY_PieceCount = 150;
			AssertEquals("PieceCount", 150m, lineDetails.PieceCount);
			CargoDesc.BY_HarmonisedTariff = "10.2030.45";
			AssertEquals("TariffNumber", "1020304500", lineDetails.TariffNumber);
		}

		[TestDate(2020, 11, 15)]
		public void TestGetEffectiveDateNow()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			header.BH_ETA = new ZDateTime(2020, 12, 30);
			var moveHeader = header.MovementHeaders.AddNew();
			AssertEquals(true, moveHeader.SupportsBondedWarehousing);
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container1 = moveDetail.Containers.AddNew();
			var commodity1 = container1.Commodities.AddNew();
			commodity1.BY_PieceCount = 10;
			AssertEquals("EffectiveDate Should be Today", new ZDateTime(2020, 11, 15), commodity1.EffectiveDateForDutyRate);
		}

		public void TestHasPieceCountInContainer()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var moveHeader = header.MovementHeaders.AddNew();
			AssertEquals(true, moveHeader.SupportsBondedWarehousing);
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container1 = moveDetail.Containers.AddNew();
			var commodity1 = container1.Commodities.AddNew();
			commodity1.BY_PieceCount = 10;
			container1.BC_PieceCount = 10;
			AssertEquals(true, commodity1.HasPieceCountInContainer);
			container1.BC_PieceCount = 0;
			AssertEquals(false, commodity1.HasPieceCountInContainer);
		}

		public void TestIInBondContainerMarksAndNumbersMembers()
		{
			IInBondContainerMarksAndNumbers lineDetails = CargoDesc;
			CargoDesc.BY_MarksAndNumbers = "FUNNY LOOKING STAIN";
			AssertEquals("MarksAndNumbers", "FUNNY LOOKING STAIN", lineDetails.MarksAndNumbers);
		}

		public void TestLookups()
		{
			AssertEquals(typeof(CusInBondCargoDescLookups), CargoDesc.Lookups.GetType());
		}

		public void TestValidation()
		{
			AssertEquals(typeof(CusInBondCargoDescValidation), CargoDesc.Validation.GetType());
		}

		public void TestBY_FormattedHarmonisedTariff()
		{
			CargoDesc.BY_HarmonisedTariff = "101 020 3010";
			AssertEquals("1010.20.3010", CargoDesc.BY_FormattedHarmonisedTariff);
			AssertEquals("1010203010", CargoDesc.BY_HarmonisedTariff);
			CargoDesc.BY_FormattedHarmonisedTariff = "10.1 569 3.534";
			AssertEquals("1015.69.3534", CargoDesc.BY_FormattedHarmonisedTariff);
			AssertEquals("1015693534", CargoDesc.BY_HarmonisedTariff);
			CargoDesc.BY_HarmonisedTariff = "10.2030.40";
			AssertEquals("TariffNumber", "10203040", CargoDesc.BY_HarmonisedTariff);
			AssertEquals("TariffNumber", "1020.30.40", CargoDesc.BY_FormattedHarmonisedTariff);
		}

		public void TestBY_HarmonisedTariff_MaxLength()
		{
			AssertEquals(10, CargoDesc.BY_HarmonisedTariffInfo.MaxLength);
		}

		public void TestBY_FormattedHarmonised_MaxLength()
		{
			AssertEquals(12, CargoDesc.BY_FormattedHarmonisedTariffInfo.MaxLength);
		}

		public void TestWeight()
		{
			CargoDesc.BY_GrossWeight = 10.1234m;
			CargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
			AssertEquals(new ZWeight(10.1234m, Core.Constants.Weight.Pounds), CargoDesc.Weight);
		}

		public void TestDeleteCommodities()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var container0 = declaration.CusContainers.AddNew();
			container0.CO_ContainerNumber = "CONT123456";
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MBTEST1";
			var package0 = declaration.Packages.AddNew();
			package0.CW_HouseBill = masterBill.CU_BillUniqueCode;
			package0.CW_ContainerNoOrEquipmentNo = "CONT123456";
			package0.CW_MarksAndNos = "ABC1";
			var package1 = declaration.Packages.AddNew();
			package1.CW_HouseBill = masterBill.CU_BillUniqueCode;
			package1.CW_ContainerNoOrEquipmentNo = "CONT123456";
			package1.CW_MarksAndNos = "ABC2";
			var header0 = Factory.New<CusInBondHeader>();
			header0.BH_ParentID = declaration.PK;
			header0.BH_ParentTableCode = declaration.TablePrefix;
			var synchronizer = (CusInBondHeaderDeclarationSynchronizer)header0.Synchroniser;
			synchronizer.Synchronise(true);
			AssertEquals(1, header0.Bills.Count);
			AssertEquals(1, header0.Bills[0].MoveDetails.Count);
			var commodities = header0.Bills[0].MoveDetails[0].Containers[0].Commodities;
			AssertEquals(2, commodities.Count);
			Assert("Commodity can't be deleted.", !commodities[0].CanDelete);
			AssertEquals(string.Format("This Commodity cannot be deleted {0}", ValidationConstants.Synchronize.SynchronizedFromParent(JobDeclarationSchema.Constants.TableName)), commodities[0].ReasonForNotAbleToDelete);
			Assert("Commodity can't be deleted.", !commodities[1].CanDelete);
			AssertEquals(string.Format("This Commodity cannot be deleted {0}", ValidationConstants.Synchronize.SynchronizedFromParent(JobDeclarationSchema.Constants.TableName)), commodities[1].ReasonForNotAbleToDelete);
			header0.BH_OverrideFreightDefaults = true;
			Assert("Commodity can be deleted.", commodities[0].CanDelete);
			Assert("Commodity can be deleted.", commodities[1].CanDelete);
			var header1 = Factory.New<CusInBondHeader>();
			var bill = header1.Bills.AddNew();
			var moveDetail = bill.MoveDetails.AddNew();
			var container = moveDetail.Containers.AddNew();
			var commdities0 = container.Commodities.AddNew();
			var commdities1 = container.Commodities.AddNew();
			Assert("Commodity can be deleted.", commdities0.CanDelete);
			Assert("Commodity can be deleted.", commdities1.CanDelete);
		}

		public void TestBY_SerialNumber_ReadOnly()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			Header.BH_OA_Importer = importer.MainAddress.PK;
			Header.BH_FTZMove = true;
			var helper = new Customs.Business.Testing.WhsDataTestHelper(Factory);
			var moveHeader = CargoDesc.MoveHeader;
			moveHeader.BM_OA_WarehouseAddress = helper.WhsWarehouse.WW_OA_WarehouseAddress;
			var org1 = Factory.New<OrgHeader>();
			AssertEquals(true, CargoDesc.BY_SerialNumberInfo.ReadOnly);
			AssertEquals(true, CargoDesc.ChildCommodities.AllowNew);
			CargoDesc.BY_OH_Supplier = org1.PK;
			AssertEquals("CargoDesc.BY_OH_Supplier", org1.PK, CargoDesc.BY_OH_Supplier);
			CargoDesc.BY_SerialNumber = "C";
			AssertEquals("CargoDesc.BY_SerialNumber", "", CargoDesc.BY_SerialNumber);
		}

		public void TestRelatedTariffDetailsDefaultFromProduct()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";
			product.OP_Desc = "BOB TEST";
			product.OP_Weight = 1.5m;
			product.OP_WeightUQ = Core.Constants.Weight.Pounds;
			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			var relatedTariffPivot1 = pivot.Children.AddNew();
			relatedTariffPivot1.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			relatedTariffPivot1.CI_TariffNum = USCTariff.CBTPABenefitsApplicable;
			relatedTariffPivot1.CD_GrossWeight = ZDecimal.Zero;
			relatedTariffPivot1.CD_WeightUQ = Core.Constants.Weight.Kilograms;
			var relatedTariffPivot2 = pivot.Children.AddNew();
			relatedTariffPivot2.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			relatedTariffPivot2.CI_TariffNum = USCTariff.CBTPABenefitsApplicable;
			relatedTariffPivot2.CD_GrossWeight = 0.002m;
			relatedTariffPivot2.CD_WeightUQ = Core.Constants.Weight.Tonnes;
			relatedTariffPivot2.CI_ChildListOrder = 1;
			relatedTariffPivot1.CI_ChildListOrder = 2;
			Factory.Save();
			Header.BH_OA_Importer = importer.MainAddress.PK;
			CargoDesc.BY_PartNumber = "Test1";
			CargoDesc.BY_InvoiceQuantity = 100m;
			AssertEquals("BY_HarmonisedTariff", "", CargoDesc.BY_HarmonisedTariff);
			AssertEquals("BY_Description", "", CargoDesc.BY_Description);
			AssertEquals("No commodity lines should have been generated", 0, CargoDesc.ChildCommodities.Count);
			CargoDesc.BY_PartNumber = "Test";
			AssertEquals("BY_HarmonisedTariff", "", CargoDesc.BY_HarmonisedTariff);
			AssertEquals("BY_Description", "BOB TEST", CargoDesc.BY_Description);
			AssertEquals("Two commodity lines should have been generated for this part", 2, CargoDesc.ChildCommodities.Count);
			AssertCommodity(CargoDesc.ChildCommodities[0], USCTariff.CottonFeeApplicable, ZDecimal.Zero, Core.Constants.Weight.Pounds);
			AssertCommodity(CargoDesc.ChildCommodities[1], USCTariff.CBTPABenefitsApplicable, 0.2m, Core.Constants.Weight.Tonnes);
		}

		public void TestCreateCommoditiesFromMultipleProductTariffs()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";
			product.OP_Weight = 10.50m;
			product.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "9101000010";
			pivot.CI_SupplementalTariff = "9801001010";
			var child1 = pivot.Children.AddNew();
			child1.CI_TariffNum = "9101000020";
			child1.CD_GrossWeight = 1500m;
			child1.CD_WeightUQ = Core.Constants.Weight.Grams;
			var child2 = pivot.Children.AddNew();
			child2.CI_TariffNum = "9101000030";
			child2.CD_GrossWeight = 210000m;
			child2.CD_WeightUQ = Core.Constants.Weight.Milligrams;
			var child3 = pivot.Children.AddNew();
			child3.CI_TariffNum = "9101000040";
			child3.CD_GrossWeight = 35m;
			child3.CD_WeightUQ = Core.Constants.Weight.Hectograms;
			var child4 = pivot.Children.AddNew();
			child4.CI_TariffNum = "9101000050";
			child4.CD_GrossWeight = 2.646m;
			child4.CD_WeightUQ = Core.Constants.Weight.Pounds;
			var child5 = pivot.Children.AddNew();
			child5.CI_TariffNum = "9101000060";
			child5.CD_GrossWeight = 77.603m;
			child5.CD_WeightUQ = Core.Constants.Weight.Ounces;
			Factory.Save();
			Header.BH_OA_Importer = importer.MainAddress.PK;
			CargoDesc.BY_InvoiceQuantity = 10m;
			CargoDesc.BY_PartNumber = "Test3";
			AssertEquals(1, container.Commodities.Count);
			AssertEquals("CargoDesc.BY_GrossWeight", ZDecimal.Zero, CargoDesc.BY_GrossWeight);
			AssertEquals("CargoDesc.BY_GrossWeightUnit", ZString.Empty, CargoDesc.BY_GrossWeightUnit);
			AssertEquals("Commodity lines should not have been generated for this part", 0, CargoDesc.ChildCommodities.Count);
			CargoDesc.BY_InvoiceQuantity = 10m;
			CargoDesc.BY_PartNumber = "Test";
			AssertEquals(1, container.Commodities.Count);
			AssertEquals("CargoDesc.BY_GrossWeight", ZDecimal.Zero, CargoDesc.BY_GrossWeight);
			AssertEquals("CargoDesc.BY_GrossWeightUnit", ZString.Empty, CargoDesc.BY_GrossWeightUnit);
			AssertEquals("Commodity lines should have been generated for this part", 6, CargoDesc.ChildCommodities.Count);
			AssertCommodity(CargoDesc.ChildCommodities[0], "9101000010", ZDecimal.Zero, Core.Constants.Weight.Kilograms);
			AssertCommodity(CargoDesc.ChildCommodities[1], "9101000020", 15000m, Core.Constants.Weight.Grams);
			AssertCommodity(CargoDesc.ChildCommodities[2], "9101000030", 2100000m, Core.Constants.Weight.Milligrams);
			AssertCommodity(CargoDesc.ChildCommodities[3], "9101000040", 350m, Core.Constants.Weight.Hectograms);
			AssertCommodity(CargoDesc.ChildCommodities[4], "9101000050", 26.46m, Core.Constants.Weight.Pounds);
			AssertCommodity(CargoDesc.ChildCommodities[5], "9101000060", 776.03m, Core.Constants.Weight.Ounces);
			CargoDesc.BY_InvoiceQuantity = 100m;
			AssertEquals("CargoDesc.BY_GrossWeight", ZDecimal.Zero, CargoDesc.BY_GrossWeight);
			AssertEquals("CargoDesc.BY_GrossWeightUnit", ZString.Empty, CargoDesc.BY_GrossWeightUnit);
			AssertEquals("Commodity lines should have been generated for this part", 6, CargoDesc.ChildCommodities.Count);
			AssertCommodity(CargoDesc.ChildCommodities[0], "9101000010", ZDecimal.Zero, Core.Constants.Weight.Kilograms);
			AssertCommodity(CargoDesc.ChildCommodities[1], "9101000020", 150000m, Core.Constants.Weight.Grams);
			AssertCommodity(CargoDesc.ChildCommodities[2], "9101000030", 21000000m, Core.Constants.Weight.Milligrams);
			AssertCommodity(CargoDesc.ChildCommodities[3], "9101000040", 3500m, Core.Constants.Weight.Hectograms);
			AssertCommodity(CargoDesc.ChildCommodities[4], "9101000050", 264.6m, Core.Constants.Weight.Pounds);
			AssertCommodity(CargoDesc.ChildCommodities[5], "9101000060", 7760.3m, Core.Constants.Weight.Ounces);
			container.Commodities.DeleteAll();
			var commodity2 = container.Commodities.AddNew();
			pivot.CI_TariffNum = ZString.Empty;
			pivot.CI_SupplementalTariff = ZString.Empty;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "OTP25";
			classification.CC_TariffNum = "1010101010";
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			pivot.CI_CC = classification.PK;
			Factory.Save();
			commodity2.BY_PartNumber = "Test";
			AssertEquals(1, container.Commodities.Count);
			AssertEquals("Commodity lines should have been generated for this part", 6, commodity2.ChildCommodities.Count);
		}

		public void TestIsTopLevelCommodity()
		{
			AssertEquals(true, CargoDesc.IsTopLevelCommodity);
			var commodity = CargoDesc.ChildCommodities.AddNew();
			AssertEquals(false, commodity.IsTopLevelCommodity);
		}

		public void TestParentCommodity()
		{
			var moveHeader = Header.MovementHeaders.AddNew();
			var bill = Header.Bills.AddNew("APLU", "789654");
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var cargoDesc = container.Commodities.AddNew();
			AssertNull(cargoDesc.ParentCommodity);
			var commodity = cargoDesc.ChildCommodities.AddNew();
			AssertEquals(cargoDesc, commodity.ParentCommodity);
			var childCommodity = commodity.ChildCommodities.AddNew();
			AssertEquals(commodity, childCommodity.ParentCommodity);
		}

		public void TestParent()
		{
			var moveHeader = Header.MovementHeaders.AddNew();
			var bill = Header.Bills.AddNew("APLU", "789654");
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var cargoDesc = container.Commodities.AddNew();
			AssertEquals(container, cargoDesc.Parent);
			var commodity = cargoDesc.ChildCommodities.AddNew();
			AssertEquals(cargoDesc, commodity.Parent);
		}

		public void TestChildCommoditiesAreDeletedWhenPartyIsCleared()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TEST1";
			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "9101000010";
			pivot.CI_SupplementalTariff = "9801001010";
			var child1 = pivot.Children.AddNew();
			child1.CI_TariffNum = "9101000020";
			Factory.Save();
			Header.BH_OA_Importer = importer.MainAddress.PK;
			var moveHeader = Header.MovementHeaders.AddNew();
			var bill = Header.Bills.AddNew("APLU", "789654");
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			commodity.BY_PartNumber = "TEST1";
			AssertEquals(true, commodity.ChildCommodities.AllowNew);
			AssertEquals(2, commodity.ChildCommodities.Count);
			commodity.BY_PartNumber = "";
			AssertEquals("Child commodities of a top level commodity should not be cleared when part is removed top level commodity", 2, commodity.ChildCommodities.Count);
			var childCommodity = commodity.ChildCommodities[0];
			childCommodity.BY_PartNumber = "TEST1";
			AssertEquals(2, childCommodity.ChildCommodities.Count);
			AssertEquals(true, childCommodity.ChildCommodities.AllowNew);
			childCommodity.BY_PartNumber = "";
			AssertEquals(0, childCommodity.ChildCommodities.Count);
			AssertEquals(false, childCommodity.ChildCommodities.AllowNew);
		}

		public void TestContainer()
		{
			var moveHeader = Header.MovementHeaders.AddNew();
			var bill = Header.Bills.AddNew("APLU", "789654");
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var cargoDesc = container.Commodities.AddNew();
			AssertEquals("1", container, cargoDesc.Container);
			var commodity = cargoDesc.ChildCommodities.AddNew();
			AssertEquals("2", container, commodity.Container);
			var childCommodity = commodity.ChildCommodities.AddNew();
			AssertEquals("3", container, childCommodity.Container);
		}

		public void TestReadOnlyDataAreSetToEmpty()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			Header.BH_OA_Importer = importer.MainAddress.PK;
			Header.BH_FTZMove = true;
			var helper = new Customs.Business.Testing.WhsDataTestHelper(Factory);
			var moveHeader = CargoDesc.MoveHeader;
			moveHeader.BM_OA_WarehouseAddress = helper.WhsWarehouse.WW_OA_WarehouseAddress;
			var org1 = Factory.New<OrgHeader>();
			AssertEquals(false, CargoDesc.BY_OH_SupplierInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PartNumberInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_PartAttrib1Info.ReadOnly);
			AssertEquals(true, CargoDesc.BY_PartAttrib2Info.ReadOnly);
			AssertEquals(true, CargoDesc.BY_PartAttrib3Info.ReadOnly);
			AssertEquals(true, CargoDesc.BY_SerialNumberInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_WarehouseEntryNumberInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_WarehouseEntryLineNoInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_InvoiceQuantityInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_FormattedHarmonisedTariffInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_MonetaryValueInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_GrossWeightInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_GrossWeightUnitInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_DescriptionInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PieceCountInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_MarksAndNumbersInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_ManifestUnitCodeInfo.ReadOnly);
			AssertEquals(true, CargoDesc.ChildCommodities.AllowNew);
			CargoDesc.BY_OH_Supplier = org1.PK;
			AssertEquals("CargoDesc.BY_OH_Supplier", org1.PK, CargoDesc.BY_OH_Supplier);
			CargoDesc.BY_PartAttrib1 = "A";
			AssertEquals("CargoDesc.BY_PartAttrib1", "", CargoDesc.BY_PartAttrib1);
			CargoDesc.BY_PartAttrib2 = "B";
			AssertEquals("CargoDesc.BY_PartAttrib2", "", CargoDesc.BY_PartAttrib2);
			CargoDesc.BY_PartAttrib3 = "C";
			AssertEquals("CargoDesc.BY_PartAttrib3", "", CargoDesc.BY_PartAttrib3);
			CargoDesc.BY_SerialNumber = "SN";
			AssertEquals("CargoDesc.BY_SerialNumber", "", CargoDesc.BY_SerialNumber);
			CargoDesc.BY_WarehouseEntryNumber = "XJ5-23423";
			AssertEquals("CargoDesc.BY_WarehouseEntryNumber", ZString.Empty, CargoDesc.BY_WarehouseEntryNumber);
			CargoDesc.BY_WarehouseEntryLineNo = (short)1;
			AssertEquals("CargoDesc.BY_WarehouseEntryLineNo", ZShort.Zero, CargoDesc.BY_WarehouseEntryLineNo);
			CargoDesc.BY_InvoiceQuantity = 10m;
			AssertEquals("CargoDesc.BY_InvoiceQuantity", ZDecimal.Zero, CargoDesc.BY_InvoiceQuantity);
			CargoDesc.BY_PartNumber = "PART1";
			AssertEquals("CargoDesc.BY_PartNumber", "PART1", CargoDesc.BY_PartNumber);
			AssertEquals("CargoDesc.BY_PartNumberForBinding", "PART1", CargoDesc.BY_PartNumberForBinding);
			AssertEquals(false, CargoDesc.BY_PartAttrib1Info.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PartAttrib2Info.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PartAttrib3Info.ReadOnly);
			AssertEquals(false, CargoDesc.BY_SerialNumberInfo.ReadOnly);
			CargoDesc.BY_PartAttrib1 = "A";
			AssertEquals("CargoDesc.BY_PartAttrib1", "A", CargoDesc.BY_PartAttrib1);
			CargoDesc.BY_PartAttrib2 = "B";
			AssertEquals("CargoDesc.BY_PartAttrib2", "B", CargoDesc.BY_PartAttrib2);
			CargoDesc.BY_PartAttrib3 = "C";
			AssertEquals("CargoDesc.BY_PartAttrib3", "C", CargoDesc.BY_PartAttrib3);
			CargoDesc.BY_SerialNumber = "SN";
			AssertEquals("CargoDesc.BY_SerialNumber", "SN", CargoDesc.BY_SerialNumber);
			AssertEquals(false, CargoDesc.BY_WarehouseEntryNumberInfo.ReadOnly);
			CargoDesc.BY_WarehouseEntryNumber = "XJ5-3242";
			AssertEquals("CargoDesc.BY_WarehouseEntryNumber", "XJ5-3242", CargoDesc.BY_WarehouseEntryNumber);
			AssertEquals(false, CargoDesc.BY_WarehouseEntryLineNoInfo.ReadOnly);
			CargoDesc.BY_WarehouseEntryLineNo = (short)1;
			AssertEquals("CargoDesc.BY_WarehouseEntryLineNo", (short)1, CargoDesc.BY_WarehouseEntryLineNo);
			AssertEquals(false, CargoDesc.BY_InvoiceQuantityInfo.ReadOnly);
			CargoDesc.BY_InvoiceQuantity = 10m;
			AssertEquals("CargoDesc.BY_InvoiceQuantity", 10m, CargoDesc.BY_InvoiceQuantity);
			CargoDesc.BY_FormattedHarmonisedTariff = "10101010";
			AssertEquals("CargoDesc.BY_FormattedHarmonisedTariff", "1010.10.10", CargoDesc.BY_FormattedHarmonisedTariff);
			AssertEquals("CargoDesc.BY_FormattedHarmonisedTariffForBinding", "1010.10.10", CargoDesc.BY_FormattedHarmonisedTariffForBinding);
			CargoDesc.BY_MonetaryValue = 1500m;
			AssertEquals("CargoDesc.BY_MonetaryValue", 1500m, CargoDesc.BY_MonetaryValue);
			CargoDesc.BY_GrossWeight = 150.50m;
			AssertEquals("CargoDesc.BY_GrossWeight", 150.50m, CargoDesc.BY_GrossWeight);
			CargoDesc.BY_GrossWeightUnit = "KG";
			AssertEquals("CargoDesc.BY_GrossWeightUnit", "KG", CargoDesc.BY_GrossWeightUnit);
			CargoDesc.BY_Description = "HELLO";
			AssertEquals("CargoDesc.BY_Description", "HELLO", CargoDesc.BY_Description);
			CargoDesc.BY_PieceCount = 10;
			AssertEquals("CargoDesc.BY_PieceCount", 10, CargoDesc.BY_PieceCount);
			CargoDesc.BY_MarksAndNumbers = "MARKS";
			AssertEquals("CargoDesc.BY_MarksAndNumbers", "MARKS", CargoDesc.BY_MarksAndNumbers);
			CargoDesc.BY_ManifestUnitCode = "NO";
			AssertEquals("CargoDesc.BY_ManifestUnitCode", "NO", CargoDesc.BY_ManifestUnitCode);
			// Test add commodity where the parent commodity has part
			var commodity = CargoDesc.ChildCommodities.AddNew();
			AssertEquals(false, CargoDesc.BY_OH_SupplierInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PartNumberInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PartAttrib1Info.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PartAttrib2Info.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PartAttrib3Info.ReadOnly);
			AssertEquals(false, CargoDesc.BY_SerialNumberInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_WarehouseEntryNumberInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_WarehouseEntryLineNoInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_InvoiceQuantityInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_FormattedHarmonisedTariffInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_MonetaryValueInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_GrossWeightInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_GrossWeightUnitInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_DescriptionInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PieceCountInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_MarksAndNumbersInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_ManifestUnitCodeInfo.ReadOnly);
			AssertEquals(true, CargoDesc.ChildCommodities.AllowNew);
			AssertEquals("CargoDesc.BY_OH_Supplier", org1.PK, CargoDesc.BY_OH_Supplier);
			AssertEquals("CargoDesc.BY_PartNumber", "PART1", CargoDesc.BY_PartNumber);
			AssertEquals("CargoDesc.BY_PartNumberForBinding", "PART1", CargoDesc.BY_PartNumberForBinding);
			AssertEquals("CargoDesc.BY_PartAttrib1", "A", CargoDesc.BY_PartAttrib1);
			AssertEquals("CargoDesc.BY_PartAttrib2", "B", CargoDesc.BY_PartAttrib2);
			AssertEquals("CargoDesc.BY_PartAttrib3", "C", CargoDesc.BY_PartAttrib3);
			AssertEquals("CargoDesc.BY_SerialNumber", "SN", CargoDesc.BY_SerialNumber);
			AssertEquals("CargoDesc.BY_WarehouseEntryNumber", "XJ5-3242", CargoDesc.BY_WarehouseEntryNumber);
			AssertEquals("CargoDesc.BY_WarehouseEntryLineNo", (short)1, CargoDesc.BY_WarehouseEntryLineNo);
			AssertEquals("CargoDesc.BY_InvoiceQuantity", 10m, CargoDesc.BY_InvoiceQuantity);
			AssertEquals("CargoDesc.BY_FormattedHarmonisedTariff", "", CargoDesc.BY_FormattedHarmonisedTariff);
			AssertEquals("CargoDesc.BY_FormattedHarmonisedTariffForBinding", CusInBondCargoDesc.CheckSubLevelMessage, CargoDesc.BY_FormattedHarmonisedTariffForBinding);
			AssertEquals("CargoDesc.BY_MonetaryValue", 0m, CargoDesc.BY_MonetaryValue);
			AssertEquals("CargoDesc.BY_GrossWeight", 0m, CargoDesc.BY_GrossWeight);
			AssertEquals("CargoDesc.BY_GrossWeightUnit", "", CargoDesc.BY_GrossWeightUnit);
			AssertEquals("CargoDesc.BY_Description", "HELLO", CargoDesc.BY_Description);
			AssertEquals("CargoDesc.BY_PieceCount", 10, CargoDesc.BY_PieceCount);
			AssertEquals("CargoDesc.BY_MarksAndNumbers", "MARKS", CargoDesc.BY_MarksAndNumbers);
			AssertEquals("CargoDesc.BY_ManifestUnitCode", "NO", CargoDesc.BY_ManifestUnitCode);
			AssertEquals(true, commodity.BY_OH_SupplierInfo.ReadOnly);
			AssertEquals(true, commodity.BY_PartNumberInfo.ReadOnly);
			AssertEquals(true, commodity.BY_PartAttrib1Info.ReadOnly);
			AssertEquals(true, commodity.BY_PartAttrib2Info.ReadOnly);
			AssertEquals(true, commodity.BY_PartAttrib3Info.ReadOnly);
			AssertEquals(true, commodity.BY_SerialNumberInfo.ReadOnly);
			AssertEquals(true, commodity.BY_WarehouseEntryNumberInfo.ReadOnly);
			AssertEquals(true, commodity.BY_WarehouseEntryLineNoInfo.ReadOnly);
			AssertEquals(true, commodity.BY_InvoiceQuantityInfo.ReadOnly);
			AssertEquals(false, commodity.BY_FormattedHarmonisedTariffInfo.ReadOnly);
			AssertEquals(false, commodity.BY_MonetaryValueInfo.ReadOnly);
			AssertEquals(false, commodity.BY_GrossWeightInfo.ReadOnly);
			AssertEquals(false, commodity.BY_GrossWeightUnitInfo.ReadOnly);
			AssertEquals(true, commodity.BY_DescriptionInfo.ReadOnly);
			AssertEquals(true, commodity.BY_PieceCountInfo.ReadOnly);
			AssertEquals(true, commodity.BY_MarksAndNumbersInfo.ReadOnly);
			AssertEquals(true, commodity.BY_ManifestUnitCodeInfo.ReadOnly);
			AssertEquals(false, commodity.ChildCommodities.AllowNew);
			commodity.BY_OH_Supplier = org1.PK;
			AssertEquals("commodity.BY_OH_Supplier", ZGuid.Empty, commodity.BY_OH_Supplier);
			commodity.BY_PartAttrib1 = "A";
			AssertEquals("commodity.BY_PartAttrib1", "", commodity.BY_PartAttrib1);
			commodity.BY_PartAttrib2 = "B";
			AssertEquals("commodity.BY_PartAttrib2", "", commodity.BY_PartAttrib2);
			commodity.BY_PartAttrib3 = "C";
			AssertEquals("commodity.BY_PartAttrib3", "", commodity.BY_PartAttrib3);
			commodity.BY_SerialNumber = "SN";
			AssertEquals("commodity.BY_SerialNumber", "", commodity.BY_SerialNumber);
			commodity.BY_WarehouseEntryNumber = "XJ5-4354";
			AssertEquals("commodity.BY_WarehouseEntryNumber", ZString.Empty, commodity.BY_WarehouseEntryNumber);
			commodity.BY_WarehouseEntryLineNo = (short)1;
			AssertEquals("commodity.BY_WarehouseEntryLineNo", ZShort.Zero, commodity.BY_WarehouseEntryLineNo);
			commodity.BY_InvoiceQuantity = 10m;
			AssertEquals("commodity.BY_InvoiceQuantity", ZDecimal.Zero, commodity.BY_InvoiceQuantity);
			commodity.BY_PartNumber = "PART1";
			AssertEquals("commodity.BY_PartNumber", "", commodity.BY_PartNumber);
			AssertEquals("commodity.BY_PartNumber", "", commodity.BY_PartNumberForBinding);
			commodity.BY_FormattedHarmonisedTariff = "10101010";
			AssertEquals("commodity.BY_FormattedHarmonisedTariff", "1010.10.10", commodity.BY_FormattedHarmonisedTariff);
			AssertEquals("commodity.BY_FormattedHarmonisedTariffForBinding", "1010.10.10", commodity.BY_FormattedHarmonisedTariffForBinding);
			commodity.BY_MonetaryValue = 1500m;
			AssertEquals("commodity.BY_MonetaryValue", 1500m, commodity.BY_MonetaryValue);
			commodity.BY_GrossWeight = 150.50m;
			AssertEquals("commodity.BY_GrossWeight", 150.50m, commodity.BY_GrossWeight);
			commodity.BY_GrossWeightUnit = "KG";
			AssertEquals("commodity.BY_GrossWeightUnit", "KG", commodity.BY_GrossWeightUnit);
			commodity.BY_Description = "HELLO";
			AssertEquals("commodity.BY_Description", "", commodity.BY_Description);
			commodity.BY_PieceCount = 10;
			AssertEquals("commodity.BY_PieceCount", 0, commodity.BY_PieceCount);
			commodity.BY_MarksAndNumbers = "MARKS";
			AssertEquals("commodity.BY_MarksAndNumbers", "", commodity.BY_MarksAndNumbers);
			commodity.BY_ManifestUnitCode = "NO";
			AssertEquals("commodity.BY_ManifestUnitCode", "", commodity.BY_ManifestUnitCode);
			commodity.Delete();
			AssertEquals(false, CargoDesc.BY_OH_SupplierInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PartNumberInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PartAttrib1Info.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PartAttrib2Info.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PartAttrib3Info.ReadOnly);
			AssertEquals(false, CargoDesc.BY_SerialNumberInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_WarehouseEntryNumberInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_WarehouseEntryLineNoInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_InvoiceQuantityInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_FormattedHarmonisedTariffInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_MonetaryValueInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_GrossWeightInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_GrossWeightUnitInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_DescriptionInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PieceCountInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_MarksAndNumbersInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_ManifestUnitCodeInfo.ReadOnly);
			AssertEquals(true, CargoDesc.ChildCommodities.AllowNew);
			AssertEquals("CargoDesc.BY_OH_Supplier", org1.PK, CargoDesc.BY_OH_Supplier);
			AssertEquals("CargoDesc.BY_PartNumber", "PART1", CargoDesc.BY_PartNumber);
			AssertEquals("CargoDesc.BY_PartNumberForBinding", "PART1", CargoDesc.BY_PartNumberForBinding);
			AssertEquals("CargoDesc.BY_PartAttrib1", "A", CargoDesc.BY_PartAttrib1);
			AssertEquals("CargoDesc.BY_PartAttrib2", "B", CargoDesc.BY_PartAttrib2);
			AssertEquals("CargoDesc.BY_PartAttrib3", "C", CargoDesc.BY_PartAttrib3);
			AssertEquals("CargoDesc.BY_SerialNumber", "SN", CargoDesc.BY_SerialNumber);
			AssertEquals("CargoDesc.BY_WarehouseEntryNumber", "XJ5-3242", CargoDesc.BY_WarehouseEntryNumber);
			AssertEquals("CargoDesc.BY_WarehouseEntryLineNo", (short)1, CargoDesc.BY_WarehouseEntryLineNo);
			AssertEquals("CargoDesc.BY_InvoiceQuantity", 10m, CargoDesc.BY_InvoiceQuantity);
			AssertEquals("CargoDesc.BY_FormattedHarmonisedTariff", "", CargoDesc.BY_FormattedHarmonisedTariff);
			AssertEquals("CargoDesc.BY_FormattedHarmonisedTariffForBinding", "", CargoDesc.BY_FormattedHarmonisedTariffForBinding);
			AssertEquals("CargoDesc.BY_MonetaryValue", 0m, CargoDesc.BY_MonetaryValue);
			AssertEquals("CargoDesc.BY_GrossWeight", 0m, CargoDesc.BY_GrossWeight);
			AssertEquals("CargoDesc.BY_GrossWeightUnit", "", CargoDesc.BY_GrossWeightUnit);
			AssertEquals("CargoDesc.BY_Description", "HELLO", CargoDesc.BY_Description);
			AssertEquals("CargoDesc.BY_PieceCount", 10, CargoDesc.BY_PieceCount);
			AssertEquals("CargoDesc.BY_MarksAndNumbers", "MARKS", CargoDesc.BY_MarksAndNumbers);
			AssertEquals("CargoDesc.BY_ManifestUnitCode", "NO", CargoDesc.BY_ManifestUnitCode);
			CargoDesc.BY_FormattedHarmonisedTariff = "10101010";
			AssertEquals("CargoDesc.BY_FormattedHarmonisedTariff", "1010.10.10", CargoDesc.BY_FormattedHarmonisedTariff);
			AssertEquals("CargoDesc.BY_FormattedHarmonisedTariffForBinding", "1010.10.10", CargoDesc.BY_FormattedHarmonisedTariffForBinding);
			CargoDesc.BY_MonetaryValue = 1500m;
			AssertEquals("CargoDesc.BY_MonetaryValue", 1500m, CargoDesc.BY_MonetaryValue);
			CargoDesc.BY_GrossWeight = 150.50m;
			AssertEquals("CargoDesc.BY_GrossWeight", 150.50m, CargoDesc.BY_GrossWeight);
			CargoDesc.BY_GrossWeightUnit = "KG";
			AssertEquals("CargoDesc.BY_GrossWeightUnit", "KG", CargoDesc.BY_GrossWeightUnit);
			CargoDesc.BY_Description = "HELLO";
			CargoDesc.BY_PartNumber = "";
			AssertEquals(true, CargoDesc.BY_PartAttrib1Info.ReadOnly);
			AssertEquals(true, CargoDesc.BY_PartAttrib2Info.ReadOnly);
			AssertEquals(true, CargoDesc.BY_PartAttrib3Info.ReadOnly);
			AssertEquals(true, CargoDesc.BY_SerialNumberInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_WarehouseEntryNumberInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_WarehouseEntryLineNoInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_InvoiceQuantityInfo.ReadOnly);
			AssertEquals("CargoDesc.BY_OH_Supplier", org1.PK, CargoDesc.BY_OH_Supplier);
			AssertEquals("CargoDesc.BY_PartNumber", "", CargoDesc.BY_PartNumber);
			AssertEquals("CargoDesc.BY_PartNumberForBinding", "", CargoDesc.BY_PartNumberForBinding);
			AssertEquals("CargoDesc.BY_PartAttrib1", "", CargoDesc.BY_PartAttrib1);
			AssertEquals("CargoDesc.BY_PartAttrib2", "", CargoDesc.BY_PartAttrib2);
			AssertEquals("CargoDesc.BY_PartAttrib3", "", CargoDesc.BY_PartAttrib3);
			AssertEquals("CargoDesc.BY_SerialNumber", "", CargoDesc.BY_SerialNumber);
			AssertEquals("CargoDesc.BY_WarehouseEntryNumber", ZString.Empty, CargoDesc.BY_WarehouseEntryNumber);
			AssertEquals("CargoDesc.BY_WarehouseEntryLineNo", ZShort.Zero, CargoDesc.BY_WarehouseEntryLineNo);
			AssertEquals("CargoDesc.BY_InvoiceQuantity", ZDecimal.Zero, CargoDesc.BY_InvoiceQuantity);
			AssertEquals("CargoDesc.BY_FormattedHarmonisedTariff", "1010.10.10", CargoDesc.BY_FormattedHarmonisedTariff);
			AssertEquals("CargoDesc.BY_FormattedHarmonisedTariffForBinding", "1010.10.10", CargoDesc.BY_FormattedHarmonisedTariffForBinding);
			AssertEquals("CargoDesc.BY_MonetaryValue", 1500m, CargoDesc.BY_MonetaryValue);
			AssertEquals("CargoDesc.BY_GrossWeight", 150.50m, CargoDesc.BY_GrossWeight);
			AssertEquals("CargoDesc.BY_GrossWeightUnit", "KG", CargoDesc.BY_GrossWeightUnit);
			AssertEquals("CargoDesc.BY_Description", "HELLO", CargoDesc.BY_Description);
			AssertEquals("CargoDesc.BY_PieceCount", 10, CargoDesc.BY_PieceCount);
			AssertEquals("CargoDesc.BY_MarksAndNumbers", "MARKS", CargoDesc.BY_MarksAndNumbers);
			AssertEquals("CargoDesc.BY_ManifestUnitCode", "NO", CargoDesc.BY_ManifestUnitCode);
			var row = ((IBusinessObjectInternals)CargoDesc).Row;
			row[CusInBondCargoDesc.Schema.BY_PartNumber] = "PART1";
			AssertEquals(false, CargoDesc.BY_PartNumberInfo.ReadOnly);
			AssertEquals("CargoDesc.BY_PartNumber", "PART1", CargoDesc.BY_PartNumber);
			AssertEquals("CargoDesc.BY_PartNumberForBinding", "PART1", CargoDesc.BY_PartNumberForBinding);
			row[CusInBondCargoDesc.Schema.BY_PartAttrib1] = "A";
			AssertEquals(false, CargoDesc.BY_PartAttrib1Info.ReadOnly);
			AssertEquals("CargoDesc.BY_PartAttrib1", "A", CargoDesc.BY_PartAttrib1);
			row[CusInBondCargoDesc.Schema.BY_PartAttrib2] = "B";
			AssertEquals(false, CargoDesc.BY_PartAttrib2Info.ReadOnly);
			AssertEquals("CargoDesc.BY_PartAttrib2", "B", CargoDesc.BY_PartAttrib2);
			row[CusInBondCargoDesc.Schema.BY_PartAttrib3] = "C";
			AssertEquals(false, CargoDesc.BY_PartAttrib3Info.ReadOnly);
			AssertEquals("CargoDesc.BY_PartAttrib3", "C", CargoDesc.BY_PartAttrib3);
			row[CusInBondCargoDesc.Schema.BY_SerialNumber] = "SN";
			AssertEquals(false, CargoDesc.BY_SerialNumberInfo.ReadOnly);
			AssertEquals("CargoDesc.BY_SerialNumber", "SN", CargoDesc.BY_SerialNumber);
			row[CusInBondCargoDesc.Schema.BY_WarehouseEntryNumber] = "XJ5-4353";
			AssertEquals(false, CargoDesc.BY_WarehouseEntryNumberInfo.ReadOnly);
			AssertEquals("CargoDesc.BY_WarehouseEntryNumber", "XJ5-4353", CargoDesc.BY_WarehouseEntryNumber);
			row[CusInBondCargoDesc.Schema.BY_WarehouseEntryLineNo] = (short)2;
			AssertEquals(false, CargoDesc.BY_WarehouseEntryLineNoInfo.ReadOnly);
			AssertEquals("CargoDesc.BY_WarehouseEntryLineNo", (ZShort)2, CargoDesc.BY_WarehouseEntryLineNo);
			row[CusInBondCargoDesc.Schema.BY_InvoiceQuantity] = 10m;
			AssertEquals(false, CargoDesc.BY_InvoiceQuantityInfo.ReadOnly);
			AssertEquals("CargoDesc.BY_InvoiceQuantity", (ZDecimal)10m, CargoDesc.BY_InvoiceQuantity);
			// Test add commodity where the parent commodity has no part
			commodity = CargoDesc.ChildCommodities.AddNew();
			AssertEquals(false, CargoDesc.BY_OH_SupplierInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PartNumberInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PartAttrib1Info.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PartAttrib2Info.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PartAttrib3Info.ReadOnly);
			AssertEquals(false, CargoDesc.BY_SerialNumberInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_WarehouseEntryNumberInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_WarehouseEntryLineNoInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_InvoiceQuantityInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_FormattedHarmonisedTariffInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_MonetaryValueInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_GrossWeightInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_GrossWeightUnitInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_DescriptionInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PieceCountInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_MarksAndNumbersInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_ManifestUnitCodeInfo.ReadOnly);
			AssertEquals(true, CargoDesc.ChildCommodities.AllowNew);
			AssertEquals("CargoDesc.BY_OH_Supplier", org1.PK, CargoDesc.BY_OH_Supplier);
			AssertEquals("CargoDesc.BY_PartNumber", "PART1", CargoDesc.BY_PartNumber);
			AssertEquals("CargoDesc.BY_PartNumberForBinding", "PART1", CargoDesc.BY_PartNumberForBinding);
			AssertEquals("CargoDesc.BY_PartAttrib1", "A", CargoDesc.BY_PartAttrib1);
			AssertEquals("CargoDesc.BY_PartAttrib2", "B", CargoDesc.BY_PartAttrib2);
			AssertEquals("CargoDesc.BY_PartAttrib3", "C", CargoDesc.BY_PartAttrib3);
			AssertEquals("CargoDesc.BY_SerialNumber", "SN", CargoDesc.BY_SerialNumber);
			AssertEquals("CargoDesc.BY_WarehouseEntryNumber", "XJ5-4353", CargoDesc.BY_WarehouseEntryNumber);
			AssertEquals("CargoDesc.BY_WarehouseEntryLineNo", (short)2, CargoDesc.BY_WarehouseEntryLineNo);
			AssertEquals("CargoDesc.BY_InvoiceQuantity", 10m, CargoDesc.BY_InvoiceQuantity);
			AssertEquals("CargoDesc.BY_FormattedHarmonisedTariff", "", CargoDesc.BY_FormattedHarmonisedTariff);
			AssertEquals("CargoDesc.BY_FormattedHarmonisedTariffForBinding", CusInBondCargoDesc.CheckSubLevelMessage, CargoDesc.BY_FormattedHarmonisedTariffForBinding);
			AssertEquals("CargoDesc.BY_MonetaryValue", 0m, CargoDesc.BY_MonetaryValue);
			AssertEquals("CargoDesc.BY_GrossWeight", 0m, CargoDesc.BY_GrossWeight);
			AssertEquals("CargoDesc.BY_GrossWeightUnit", "", CargoDesc.BY_GrossWeightUnit);
			AssertEquals("CargoDesc.BY_Description", "HELLO", CargoDesc.BY_Description);
			AssertEquals("CargoDesc.BY_PieceCount", 10, CargoDesc.BY_PieceCount);
			AssertEquals("CargoDesc.BY_MarksAndNumbers", "MARKS", CargoDesc.BY_MarksAndNumbers);
			AssertEquals("CargoDesc.BY_ManifestUnitCode", "NO", CargoDesc.BY_ManifestUnitCode);
			AssertEquals(true, commodity.BY_OH_SupplierInfo.ReadOnly);
			AssertEquals(true, commodity.BY_PartNumberInfo.ReadOnly);
			AssertEquals(true, commodity.BY_PartAttrib1Info.ReadOnly);
			AssertEquals(true, commodity.BY_PartAttrib2Info.ReadOnly);
			AssertEquals(true, commodity.BY_PartAttrib3Info.ReadOnly);
			AssertEquals(true, commodity.BY_SerialNumberInfo.ReadOnly);
			AssertEquals(true, commodity.BY_WarehouseEntryNumberInfo.ReadOnly);
			AssertEquals(true, commodity.BY_WarehouseEntryLineNoInfo.ReadOnly);
			AssertEquals(true, commodity.BY_InvoiceQuantityInfo.ReadOnly);
			AssertEquals(false, commodity.BY_FormattedHarmonisedTariffInfo.ReadOnly);
			AssertEquals(false, commodity.BY_MonetaryValueInfo.ReadOnly);
			AssertEquals(false, commodity.BY_GrossWeightInfo.ReadOnly);
			AssertEquals(false, commodity.BY_GrossWeightUnitInfo.ReadOnly);
			AssertEquals(true, commodity.BY_DescriptionInfo.ReadOnly);
			AssertEquals(true, commodity.BY_PieceCountInfo.ReadOnly);
			AssertEquals(true, commodity.BY_MarksAndNumbersInfo.ReadOnly);
			AssertEquals(true, commodity.BY_ManifestUnitCodeInfo.ReadOnly);
			AssertEquals(false, commodity.ChildCommodities.AllowNew);
			commodity.BY_OH_Supplier = org1.PK;
			AssertEquals(true, commodity.BY_OH_SupplierInfo.ReadOnly);
			AssertEquals("commodity.BY_OH_Supplier", ZGuid.Empty, commodity.BY_OH_Supplier);
			AssertEquals(true, commodity.BY_PartNumberInfo.ReadOnly);
			commodity.BY_PartNumber = "PART1";
			AssertEquals("commodity.BY_PartNumber", "", commodity.BY_PartNumber);
			commodity.BY_PartAttrib1 = "A";
			AssertEquals("commodity.BY_PartAttrib1", "", commodity.BY_PartAttrib1);
			commodity.BY_PartAttrib2 = "B";
			AssertEquals("commodity.BY_PartAttrib2", "", commodity.BY_PartAttrib2);
			commodity.BY_PartAttrib3 = "C";
			AssertEquals("commodity.BY_PartAttrib3", "", commodity.BY_PartAttrib3);
			commodity.BY_SerialNumber = "SN";
			AssertEquals("commodity.BY_SerialNumber", "", commodity.BY_SerialNumber);
			commodity.BY_WarehouseEntryNumber = "XJ5-4353";
			AssertEquals("commodity.BY_WarehouseEntryNumber", ZString.Empty, commodity.BY_WarehouseEntryNumber);
			commodity.BY_WarehouseEntryLineNo = (short)1;
			AssertEquals("commodity.BY_WarehouseEntryLineNo", ZShort.Zero, commodity.BY_WarehouseEntryLineNo);
			commodity.BY_InvoiceQuantity = 10m;
			AssertEquals("commodity.BY_InvoiceQuantity", ZDecimal.Zero, commodity.BY_InvoiceQuantity);
			commodity.BY_FormattedHarmonisedTariff = "10101010";
			AssertEquals("commodity.BY_FormattedHarmonisedTariff", "1010.10.10", commodity.BY_FormattedHarmonisedTariff);
			AssertEquals("commodity.BY_FormattedHarmonisedTariffForBinding", "1010.10.10", commodity.BY_FormattedHarmonisedTariffForBinding);
			commodity.BY_MonetaryValue = 1500m;
			AssertEquals("commodity.BY_MonetaryValue", 1500m, commodity.BY_MonetaryValue);
			commodity.BY_GrossWeight = 150.50m;
			AssertEquals("commodity.BY_GrossWeight", 150.50m, commodity.BY_GrossWeight);
			commodity.BY_GrossWeightUnit = "KG";
			AssertEquals("commodity.BY_GrossWeightUnit", "KG", commodity.BY_GrossWeightUnit);
			commodity.BY_Description = "HELLO";
			AssertEquals("commodity.BY_Description", "", commodity.BY_Description);
			commodity.BY_PieceCount = 10;
			AssertEquals("commodity.BY_PieceCount", 0, commodity.BY_PieceCount);
			commodity.BY_MarksAndNumbers = "MARKS";
			AssertEquals("commodity.BY_MarksAndNumbers", "", commodity.BY_MarksAndNumbers);
			commodity.BY_ManifestUnitCode = "NO";
			AssertEquals("commodity.BY_ManifestUnitCode", "", commodity.BY_ManifestUnitCode);
			commodity.Delete();
			CargoDesc.BY_PartNumber = "";
			commodity = CargoDesc.ChildCommodities.AddNew();
			AssertEquals(false, CargoDesc.BY_OH_SupplierInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PartNumberInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_PartAttrib1Info.ReadOnly);
			AssertEquals(true, CargoDesc.BY_PartAttrib2Info.ReadOnly);
			AssertEquals(true, CargoDesc.BY_PartAttrib3Info.ReadOnly);
			AssertEquals(true, CargoDesc.BY_SerialNumberInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_WarehouseEntryNumberInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_WarehouseEntryLineNoInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_InvoiceQuantityInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_FormattedHarmonisedTariffInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_MonetaryValueInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_GrossWeightInfo.ReadOnly);
			AssertEquals(true, CargoDesc.BY_GrossWeightUnitInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_DescriptionInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_PieceCountInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_MarksAndNumbersInfo.ReadOnly);
			AssertEquals(false, CargoDesc.BY_ManifestUnitCodeInfo.ReadOnly);
			AssertEquals(true, CargoDesc.ChildCommodities.AllowNew);
			AssertEquals("CargoDesc.BY_OH_Supplier", org1.PK, CargoDesc.BY_OH_Supplier);
			AssertEquals("CargoDesc.BY_PartNumber", "", CargoDesc.BY_PartNumber);
			AssertEquals("CargoDesc.BY_PartNumberForBinding", "", CargoDesc.BY_PartNumberForBinding);
			AssertEquals("CargoDesc.BY_PartAttrib1", "", CargoDesc.BY_PartAttrib1);
			AssertEquals("CargoDesc.BY_PartAttrib2", "", CargoDesc.BY_PartAttrib2);
			AssertEquals("CargoDesc.BY_PartAttrib3", "", CargoDesc.BY_PartAttrib3);
			AssertEquals("CargoDesc.BY_SerialNumber", "", CargoDesc.BY_SerialNumber);
			AssertEquals("CargoDesc.BY_WarehouseEntryNumber", ZString.Empty, CargoDesc.BY_WarehouseEntryNumber);
			AssertEquals("CargoDesc.BY_WarehouseEntryLineNo", ZShort.Zero, CargoDesc.BY_WarehouseEntryLineNo);
			AssertEquals("CargoDesc.BY_InvoiceQuantity", ZDecimal.Zero, CargoDesc.BY_InvoiceQuantity);
			AssertEquals("CargoDesc.BY_FormattedHarmonisedTariff", "", CargoDesc.BY_FormattedHarmonisedTariff);
			AssertEquals("CargoDesc.BY_FormattedHarmonisedTariffForBinding", CusInBondCargoDesc.CheckSubLevelMessage, CargoDesc.BY_FormattedHarmonisedTariffForBinding);
			AssertEquals("CargoDesc.BY_MonetaryValue", 0m, CargoDesc.BY_MonetaryValue);
			AssertEquals("CargoDesc.BY_GrossWeight", 0m, CargoDesc.BY_GrossWeight);
			AssertEquals("CargoDesc.BY_GrossWeightUnit", "", CargoDesc.BY_GrossWeightUnit);
			AssertEquals("CargoDesc.BY_Description", "HELLO", CargoDesc.BY_Description);
			AssertEquals("CargoDesc.BY_PieceCount", 10, CargoDesc.BY_PieceCount);
			AssertEquals("CargoDesc.BY_MarksAndNumbers", "MARKS", CargoDesc.BY_MarksAndNumbers);
			AssertEquals("CargoDesc.BY_ManifestUnitCode", "NO", CargoDesc.BY_ManifestUnitCode);
			AssertEquals(false, commodity.BY_OH_SupplierInfo.ReadOnly);
			AssertEquals(false, commodity.BY_PartNumberInfo.ReadOnly);
			AssertEquals(true, commodity.BY_PartAttrib1Info.ReadOnly);
			AssertEquals(true, commodity.BY_PartAttrib2Info.ReadOnly);
			AssertEquals(true, commodity.BY_PartAttrib3Info.ReadOnly);
			AssertEquals(true, commodity.BY_SerialNumberInfo.ReadOnly);
			AssertEquals(true, commodity.BY_WarehouseEntryNumberInfo.ReadOnly);
			AssertEquals(true, commodity.BY_WarehouseEntryLineNoInfo.ReadOnly);
			AssertEquals(true, commodity.BY_InvoiceQuantityInfo.ReadOnly);
			AssertEquals(false, commodity.BY_FormattedHarmonisedTariffInfo.ReadOnly);
			AssertEquals(false, commodity.BY_MonetaryValueInfo.ReadOnly);
			AssertEquals(false, commodity.BY_GrossWeightInfo.ReadOnly);
			AssertEquals(false, commodity.BY_GrossWeightUnitInfo.ReadOnly);
			AssertEquals(true, commodity.BY_DescriptionInfo.ReadOnly);
			AssertEquals(true, commodity.BY_PieceCountInfo.ReadOnly);
			AssertEquals(true, commodity.BY_MarksAndNumbersInfo.ReadOnly);
			AssertEquals(true, commodity.BY_ManifestUnitCodeInfo.ReadOnly);
			AssertEquals(false, commodity.ChildCommodities.AllowNew);
			var org2 = Factory.New<OrgHeader>();
			commodity.BY_OH_Supplier = org2.PK;
			AssertEquals("CargoDesc.BY_OH_Supplier", ZGuid.Empty, CargoDesc.BY_OH_Supplier);
			AssertEquals("CargoDesc.BY_OH_SupplierInfo.ReadOnly", true, CargoDesc.BY_OH_SupplierInfo.ReadOnly);
			AssertEquals("CargoDesc.BY_PartNumber", "", CargoDesc.BY_PartNumber);
			AssertEquals("CargoDesc.BY_PartNumberInfo.ReadOnly", true, CargoDesc.BY_PartNumberInfo.ReadOnly);
			AssertEquals("CargoDesc.BY_PartNumberForBinding", CusInBondCargoDesc.CheckSubLevelMessage, CargoDesc.BY_PartNumberForBinding);
			AssertEquals(false, commodity.BY_OH_SupplierInfo.ReadOnly);
			AssertEquals("commodity.BY_OH_Supplier", org2.PK, commodity.BY_OH_Supplier);
			AssertEquals(false, commodity.BY_PartNumberInfo.ReadOnly);
			AssertEquals(true, commodity.BY_PartAttrib1Info.ReadOnly);
			AssertEquals(true, commodity.BY_PartAttrib2Info.ReadOnly);
			AssertEquals(true, commodity.BY_PartAttrib3Info.ReadOnly);
			AssertEquals(true, commodity.BY_SerialNumberInfo.ReadOnly);
			AssertEquals(true, commodity.BY_WarehouseEntryNumberInfo.ReadOnly);
			AssertEquals(true, commodity.BY_WarehouseEntryLineNoInfo.ReadOnly);
			AssertEquals(true, commodity.BY_InvoiceQuantityInfo.ReadOnly);
			AssertEquals("commodity.BY_PartNumber", "", commodity.BY_PartNumber);
			commodity.BY_PartAttrib1 = "A";
			AssertEquals("commodity.BY_PartAttrib1", "", commodity.BY_PartAttrib1);
			commodity.BY_PartAttrib2 = "B";
			AssertEquals("commodity.BY_PartAttrib2", "", commodity.BY_PartAttrib2);
			commodity.BY_PartAttrib3 = "C";
			AssertEquals("commodity.BY_PartAttrib3", "", commodity.BY_PartAttrib3);
			commodity.BY_SerialNumber = "SN";
			AssertEquals("commodity.BY_SerialNumber", "", commodity.BY_SerialNumber);
			commodity.BY_WarehouseEntryNumber = "XJ5-3243";
			AssertEquals("commodity.BY_WarehouseEntryNumber", ZString.Empty, commodity.BY_WarehouseEntryNumber);
			commodity.BY_WarehouseEntryLineNo = (short)1;
			AssertEquals("commodity.BY_WarehouseEntryLineNo", ZShort.Zero, commodity.BY_WarehouseEntryLineNo);
			commodity.BY_InvoiceQuantity = 10m;
			AssertEquals("commodity.BY_InvoiceQuantity", ZDecimal.Zero, commodity.BY_InvoiceQuantity);
			commodity.BY_PartNumber = "PART1";
			AssertEquals(false, commodity.BY_PartAttrib1Info.ReadOnly);
			AssertEquals(false, commodity.BY_PartAttrib2Info.ReadOnly);
			AssertEquals(false, commodity.BY_PartAttrib3Info.ReadOnly);
			AssertEquals(false, commodity.BY_SerialNumberInfo.ReadOnly);
			AssertEquals(false, commodity.BY_WarehouseEntryNumberInfo.ReadOnly);
			AssertEquals(false, commodity.BY_WarehouseEntryLineNoInfo.ReadOnly);
			AssertEquals(false, commodity.BY_InvoiceQuantityInfo.ReadOnly);
			commodity.BY_PartAttrib1 = "A";
			AssertEquals("commodity.BY_PartAttrib1", "A", commodity.BY_PartAttrib1);
			commodity.BY_PartAttrib2 = "B";
			AssertEquals("commodity.BY_PartAttrib2", "B", commodity.BY_PartAttrib2);
			commodity.BY_PartAttrib3 = "C";
			AssertEquals("commodity.BY_PartAttrib3", "C", commodity.BY_PartAttrib3);
			commodity.BY_SerialNumber = "SN";
			AssertEquals("commodity.BY_SerialNumber", "SN", commodity.BY_SerialNumber);
			commodity.BY_WarehouseEntryNumber = "XJ5-23423";
			AssertEquals("commodity.BY_WarehouseEntryNumber", "XJ5-23423", commodity.BY_WarehouseEntryNumber);
			commodity.BY_WarehouseEntryLineNo = (ZShort)1;
			AssertEquals("commodity.BY_WarehouseEntryLineNo", (ZShort)1, commodity.BY_WarehouseEntryLineNo);
			commodity.BY_InvoiceQuantity = (ZDecimal)10m;
			AssertEquals("commodity.BY_InvoiceQuantity", (ZDecimal)10m, commodity.BY_InvoiceQuantity);
			commodity.BY_FormattedHarmonisedTariff = "10101010";
			AssertEquals("commodity.BY_FormattedHarmonisedTariff", "1010.10.10", commodity.BY_FormattedHarmonisedTariff);
			AssertEquals("commodity.BY_FormattedHarmonisedTariffForBinding", "1010.10.10", commodity.BY_FormattedHarmonisedTariffForBinding);
			commodity.BY_MonetaryValue = 1500m;
			AssertEquals("commodity.BY_MonetaryValue", 1500m, commodity.BY_MonetaryValue);
			commodity.BY_GrossWeight = 150.50m;
			AssertEquals("commodity.BY_GrossWeight", 150.50m, commodity.BY_GrossWeight);
			commodity.BY_GrossWeightUnit = "KG";
			AssertEquals("commodity.BY_GrossWeightUnit", "KG", commodity.BY_GrossWeightUnit);
			commodity.BY_Description = "HELLO";
			AssertEquals("commodity.BY_Description", "", commodity.BY_Description);
			commodity.BY_PieceCount = 10;
			AssertEquals("commodity.BY_PieceCount", 0, commodity.BY_PieceCount);
			commodity.BY_MarksAndNumbers = "MARKS";
			AssertEquals("commodity.BY_MarksAndNumbers", "", commodity.BY_MarksAndNumbers);
			commodity.BY_ManifestUnitCode = "NO";
			AssertEquals("commodity.BY_ManifestUnitCode", "", commodity.BY_ManifestUnitCode);
			var childCommodity = commodity.ChildCommodities.AddNew();
			AssertEquals(false, commodity.BY_OH_SupplierInfo.ReadOnly);
			AssertEquals(false, commodity.BY_PartNumberInfo.ReadOnly);
			AssertEquals(false, commodity.BY_PartAttrib1Info.ReadOnly);
			AssertEquals(false, commodity.BY_PartAttrib2Info.ReadOnly);
			AssertEquals(false, commodity.BY_PartAttrib3Info.ReadOnly);
			AssertEquals(false, commodity.BY_SerialNumberInfo.ReadOnly);
			AssertEquals(false, commodity.BY_WarehouseEntryNumberInfo.ReadOnly);
			AssertEquals(false, commodity.BY_WarehouseEntryLineNoInfo.ReadOnly);
			AssertEquals(false, commodity.BY_InvoiceQuantityInfo.ReadOnly);
			AssertEquals(true, commodity.BY_FormattedHarmonisedTariffInfo.ReadOnly);
			AssertEquals(true, commodity.BY_MonetaryValueInfo.ReadOnly);
			AssertEquals(true, commodity.BY_GrossWeightInfo.ReadOnly);
			AssertEquals(true, commodity.BY_GrossWeightUnitInfo.ReadOnly);
			AssertEquals(true, commodity.BY_DescriptionInfo.ReadOnly);
			AssertEquals(true, commodity.BY_PieceCountInfo.ReadOnly);
			AssertEquals(true, commodity.BY_MarksAndNumbersInfo.ReadOnly);
			AssertEquals(true, commodity.BY_ManifestUnitCodeInfo.ReadOnly);
			AssertEquals(true, commodity.ChildCommodities.AllowNew);
			AssertEquals("commodity.BY_OH_Supplier", org2.PK, commodity.BY_OH_Supplier);
			AssertEquals("commodity.BY_PartNumber", "PART1", commodity.BY_PartNumber);
			AssertEquals("commodity.BY_PartAttrib1", "A", commodity.BY_PartAttrib1);
			AssertEquals("commodity.BY_PartAttrib2", "B", commodity.BY_PartAttrib2);
			AssertEquals("commodity.BY_PartAttrib3", "C", commodity.BY_PartAttrib3);
			AssertEquals("commodity.BY_SerialNumber", "SN", commodity.BY_SerialNumber);
			AssertEquals("commodity.BY_WarehouseEntryNumber", "XJ5-23423", commodity.BY_WarehouseEntryNumber);
			AssertEquals("commodity.BY_WarehouseEntryLineNo", (ZShort)1, commodity.BY_WarehouseEntryLineNo);
			AssertEquals("commodity.BY_InvoiceQuantity", (ZDecimal)10m, commodity.BY_InvoiceQuantity);
			AssertEquals("commodity.BY_FormattedHarmonisedTariff", "", commodity.BY_FormattedHarmonisedTariff);
			AssertEquals("commodity.BY_FormattedHarmonisedTariffForBinding", CusInBondCargoDesc.CheckSubLevelMessage, commodity.BY_FormattedHarmonisedTariffForBinding);
			AssertEquals("commodity.BY_MonetaryValue", 0m, commodity.BY_MonetaryValue);
			AssertEquals("commodity.BY_GrossWeight", 0m, commodity.BY_GrossWeight);
			AssertEquals("commodity.BY_GrossWeightUnit", "", commodity.BY_GrossWeightUnit);
			AssertEquals("commodity.BY_Description", "", commodity.BY_Description);
			AssertEquals("commodity.BY_PieceCount", 0, commodity.BY_PieceCount);
			AssertEquals("commodity.BY_MarksAndNumbers", "", commodity.BY_MarksAndNumbers);
			AssertEquals("commodity.BY_ManifestUnitCode", "", commodity.BY_ManifestUnitCode);
			AssertEquals(true, childCommodity.BY_OH_SupplierInfo.ReadOnly);
			AssertEquals(true, childCommodity.BY_PartNumberInfo.ReadOnly);
			AssertEquals(true, childCommodity.BY_PartAttrib1Info.ReadOnly);
			AssertEquals(true, childCommodity.BY_PartAttrib2Info.ReadOnly);
			AssertEquals(true, childCommodity.BY_PartAttrib3Info.ReadOnly);
			AssertEquals(true, childCommodity.BY_SerialNumberInfo.ReadOnly);
			AssertEquals(true, childCommodity.BY_WarehouseEntryNumberInfo.ReadOnly);
			AssertEquals(true, childCommodity.BY_WarehouseEntryLineNoInfo.ReadOnly);
			AssertEquals(true, childCommodity.BY_InvoiceQuantityInfo.ReadOnly);
			AssertEquals(false, childCommodity.BY_FormattedHarmonisedTariffInfo.ReadOnly);
			AssertEquals(false, childCommodity.BY_MonetaryValueInfo.ReadOnly);
			AssertEquals(false, childCommodity.BY_GrossWeightInfo.ReadOnly);
			AssertEquals(false, childCommodity.BY_GrossWeightUnitInfo.ReadOnly);
			AssertEquals(true, childCommodity.BY_DescriptionInfo.ReadOnly);
			AssertEquals(true, childCommodity.BY_PieceCountInfo.ReadOnly);
			AssertEquals(true, childCommodity.BY_MarksAndNumbersInfo.ReadOnly);
			AssertEquals(true, childCommodity.BY_ManifestUnitCodeInfo.ReadOnly);
			AssertEquals(false, childCommodity.ChildCommodities.AllowNew);
			childCommodity.BY_OH_Supplier = org1.PK;
			AssertEquals("childCommodity.BY_OH_Supplier", ZGuid.Empty, childCommodity.BY_OH_Supplier);
			childCommodity.BY_PartNumber = "PART1";
			AssertEquals("childCommodity.BY_PartNumber", "", childCommodity.BY_PartNumber);
			childCommodity.BY_PartAttrib1 = "A";
			AssertEquals("childCommodity.BY_PartAttrib1", "", childCommodity.BY_PartAttrib1);
			childCommodity.BY_PartAttrib2 = "B";
			AssertEquals("childCommodity.BY_PartAttrib2", "", childCommodity.BY_PartAttrib2);
			childCommodity.BY_PartAttrib3 = "C";
			AssertEquals("childCommodity.BY_PartAttrib3", "", childCommodity.BY_PartAttrib3);
			childCommodity.BY_SerialNumber = "SN";
			AssertEquals("childCommodity.BY_SerialNumber", "", childCommodity.BY_SerialNumber);
			childCommodity.BY_WarehouseEntryNumber = "XJ5-4354";
			AssertEquals("childCommodity.BY_WarehouseEntryNumber", ZString.Empty, childCommodity.BY_WarehouseEntryNumber);
			childCommodity.BY_WarehouseEntryLineNo = (short)1;
			AssertEquals("childCommodity.BY_WarehouseEntryLineNo", ZShort.Zero, childCommodity.BY_WarehouseEntryLineNo);
			childCommodity.BY_InvoiceQuantity = 10m;
			AssertEquals("childCommodity.BY_InvoiceQuantity", ZDecimal.Zero, childCommodity.BY_InvoiceQuantity);
			childCommodity.BY_FormattedHarmonisedTariff = "10101010";
			AssertEquals("childCommodity.BY_FormattedHarmonisedTariff", "1010.10.10", childCommodity.BY_FormattedHarmonisedTariff);
			AssertEquals("childCommodity.BY_FormattedHarmonisedTariffForBinding", "1010.10.10", childCommodity.BY_FormattedHarmonisedTariffForBinding);
			childCommodity.BY_MonetaryValue = 1500m;
			AssertEquals("childCommodity.BY_MonetaryValue", 1500m, childCommodity.BY_MonetaryValue);
			childCommodity.BY_GrossWeight = 150.50m;
			AssertEquals("childCommodity.BY_GrossWeight", 150.50m, childCommodity.BY_GrossWeight);
			childCommodity.BY_GrossWeightUnit = "KG";
			AssertEquals("childCommodity.BY_GrossWeightUnit", "KG", childCommodity.BY_GrossWeightUnit);
			childCommodity.BY_Description = "HELLO";
			AssertEquals("childCommodity.BY_Description", "", childCommodity.BY_Description);
			childCommodity.BY_PieceCount = 10;
			AssertEquals("childCommodity.BY_PieceCount", 0, childCommodity.BY_PieceCount);
			childCommodity.BY_MarksAndNumbers = "MARKS";
			AssertEquals("childCommodity.BY_MarksAndNumbers", "", childCommodity.BY_MarksAndNumbers);
			childCommodity.BY_ManifestUnitCode = "NO";
			AssertEquals("childCommodity.BY_ManifestUnitCode", "", childCommodity.BY_ManifestUnitCode);
		}

		[TestDate(2014, 7, 20)]
		public void TestPartLookup()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PART1";
			var relationShip1 = part1.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			var relationShip2 = part1.RelatedOrganisations.AddOrganisationIfNotExist(org2.PK, OrgPartRelation.RelationshipTypes.Supplier);
			var pivot1 = part1.PivotsForBinding.AddNew();
			pivot1.CI_OH = relationShip1.OU_OH;
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "1010101010";
			pivot1.CI_DateStart = new ZDateTime(2014, 7, 1);
			var attrib1 = pivot1.Attributes1.AddNew();
			attrib1.BG_AttributeValue1 = "1";
			var attrib2 = pivot1.Attributes2.AddNew();
			attrib2.BG_AttributeValue1 = "2";
			var attrib3 = pivot1.Attributes3.AddNew();
			attrib3.BG_AttributeValue1 = "2";
			var pivot2 = part1.PivotsForBinding.AddNew();
			pivot2.CI_OH = relationShip1.OU_OH;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_TariffNum = "20010101010";
			var pivot2Attrib1 = pivot2.Attributes1.AddNew();
			pivot2Attrib1.BG_AttributeValue1 = "1";
			var pivot2Attrib2 = pivot2.Attributes2.AddNew();
			pivot2Attrib2.BG_AttributeValue1 = "2";
			var pivot2Attrib3 = pivot2.Attributes3.AddNew();
			pivot2Attrib3.BG_AttributeValue1 = "4";
			var pivot3 = part1.PivotsForBinding.AddNew();
			pivot3.CI_OH = relationShip2.OU_OH;
			pivot3.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot3.CI_TariffNum = "3010101010";
			pivot3.CI_DateStart = new ZDateTime(2014, 7, 1);
			var pivot3Attrib1 = pivot3.Attributes1.AddNew();
			pivot3Attrib1.BG_AttributeValue1 = "1";
			var pivot3Attrib2 = pivot3.Attributes2.AddNew();
			pivot3Attrib2.BG_AttributeValue1 = "2";
			var pivot3Attrib3 = pivot3.Attributes3.AddNew();
			pivot3Attrib3.BG_AttributeValue1 = "2";
			var pivot4 = part1.PivotsForBinding.AddNew();
			pivot4.CI_OH = relationShip1.OU_OH;
			pivot4.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot4.CI_TariffNum = "4010101010";
			pivot4.CI_DateStart = new ZDateTime(2014, 8, 1);
			var pivote4Attrib1 = pivot4.Attributes1.AddNew();
			pivote4Attrib1.BG_AttributeValue1 = "1";
			var pivot4Attrib2 = pivot4.Attributes2.AddNew();
			pivot4Attrib2.BG_AttributeValue1 = "2";
			var pivot4Attrib3 = pivot4.Attributes3.AddNew();
			pivot4Attrib3.BG_AttributeValue1 = "2";
			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PART1";
			var part2RelationShip1 = part2.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			var part2Pivot1 = part2.PivotsForBinding.AddNew();
			part2Pivot1.CI_OH = part2RelationShip1.OU_OH;
			part2Pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			part2Pivot1.CI_TariffNum = "1020101010";
			var part2Pivot1Attrib1 = part2Pivot1.Attributes1.AddNew();
			part2Pivot1Attrib1.BG_AttributeValue1 = "1";
			var part2Pivot1Attrib2 = part2Pivot1.Attributes2.AddNew();
			part2Pivot1Attrib2.BG_AttributeValue1 = "2";
			var part2Pivot1Attrib3 = part2Pivot1.Attributes3.AddNew();
			part2Pivot1Attrib3.BG_AttributeValue1 = "2";
			Header.BH_ETA = new ZDateTime(2014, 7, 20);
			CargoDesc.BY_OH_Supplier = org2.PK;
			CargoDesc.BY_PartNumber = "PART1";
			CargoDesc.BY_PartAttrib1 = "1";
			CargoDesc.BY_PartAttrib2 = "2";
			CargoDesc.BY_PartAttrib3 = "2";
			Header.BH_OA_Importer = ZGuid.Empty;
			AssertEquals("3010101010", CargoDesc.BY_HarmonisedTariff);
			Header.BH_OA_Importer = org1.MainAddress.PK;
			AssertEquals("1010101010", CargoDesc.BY_HarmonisedTariff);
			Header.BH_ETA = new ZDateTime(2014, 8, 20);
			AssertEquals("1010101010", CargoDesc.BY_HarmonisedTariff);
			CargoDesc.BY_OH_Supplier = org1.PK;
			AssertEquals("1020101010", CargoDesc.BY_HarmonisedTariff);
		}

		public void TestPivot()
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
			pivot.CI_DateStart = new ZDateTime(2014, 4, 1);
			var attrib1 = pivot.Attributes1.AddNew();
			attrib1.BG_AttributeValue1 = "1";
			var attrib2 = pivot.Attributes2.AddNew();
			attrib2.BG_AttributeValue1 = "2";
			var attrib3 = pivot.Attributes3.AddNew();
			attrib3.BG_AttributeValue1 = "3";
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
			AssertNull(commodity.Pivot);
			commodity.BY_PartAttrib2 = "2";
			AssertNull(commodity.Pivot);
			commodity.BY_PartAttrib3 = "3";
			AssertNotNull(commodity.Pivot);
		}

		protected override BusinessObject GetNewBusinessObject() => CargoDesc;

		protected override void SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(ZPropertyInfo info)
		{
			if (info.Name == CusInBondCargoDesc.Schema.BY_WarehouseEntryNumber || info.Name == CusInBondCargoDesc.Schema.BY_WarehouseEntryLineNo || info.Name == CusInBondCargoDesc.Schema.BY_InvoiceQuantity || info.Name == CusInBondCargoDesc.Schema.BY_PartAttrib1 || info.Name == CusInBondCargoDesc.Schema.BY_PartAttrib3 || info.Name == CusInBondCargoDesc.Schema.BY_PartAttrib3)
			{
				((CusInBondCargoDesc)info.BizObj).BY_PartNumber = "PART323";
			}
			base.SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(info);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill = header.Bills.AddNew("APLU", "789654");
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			container = moveDetail.Containers.AddNew();
			return container.Commodities.AddNew();
		}

		CusInBondCargoDesc cargoDesc;
		CusInBondCargoDesc CargoDesc => cargoDesc ?? (cargoDesc = GetCargoDesc());

		CusInBondContainer container;
		CusInBondCargoDesc GetCargoDesc()
		{
			var moveHeader = Header.MovementHeaders.AddNew();
			var bill = Header.Bills.AddNew("APLU", "789654");
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			container = moveDetail.Containers.AddNew();
			return container.Commodities.AddNew();
		}

		CusInBondHeader header;
		CusInBondHeader Header => header ?? (header = Factory.New<CusInBondHeader>());

		void AssertCommodity(CusInBondCargoDesc commodity, ZString tariffNo, ZDecimal weight, ZString weightUQ)
		{
			AssertEquals("commodity.BY_HarmonisedTariff", tariffNo, commodity.BY_HarmonisedTariff);
			AssertEquals("commodity.BY_GrossWeight", weight, commodity.BY_GrossWeight);
			AssertEquals("commodity.BY_GrossWeightUnit", weightUQ, commodity.BY_GrossWeightUnit);
		}
	}
}
