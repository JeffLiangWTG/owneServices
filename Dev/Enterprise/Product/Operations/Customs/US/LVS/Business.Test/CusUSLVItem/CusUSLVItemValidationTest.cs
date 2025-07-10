using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	internal class CusUSLVItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestULI_RN_NKCountryOfOrigin()
		{
			var cusUSLVItem = Factory.NewWithValidTestData<CusUSLVItem>();
			new CusUSLVItemValidationTestHelper().CheckCountryOfOrigin(cusUSLVItem.ULI_RN_NKCountryOfOriginInfo);
		}

		public void TestULI_RN_NKCountryOfOrigin_ShouldSkipCanadianProvince()
		{
			var cusUSLVItem = Factory.NewWithValidTestData<CusUSLVItem>();
			cusUSLVItem.ULI_RN_NKCountryOfOrigin = "ZZ";
			cusUSLVItem.RunPreSaveValidation();
			AssertHasMessageError("pre condition", cusUSLVItem.ULI_RN_NKCountryOfOriginInfo, "The code you have selected is not in the list.");

			cusUSLVItem.ULI_RN_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XO;
			cusUSLVItem.RunPreSaveValidation();
			AssertNoMessageErrors(cusUSLVItem.ULI_RN_NKCountryOfOriginInfo);
		}

		public void TestULI_RX_NKCurrency()
		{
			var cusUSLVItem = Factory.NewWithValidTestData<CusUSLVItem>();
			new CusUSLVItemValidationTestHelper().CheckCurrency(cusUSLVItem.ULI_RX_NKCurrencyInfo);
		}

		public void TestULI_Tariff()
		{
			var cusUSLVItem = Factory.NewWithValidTestData<CusUSLVItem>();
			new CusUSLVItemValidationTestHelper().CheckTariff(cusUSLVItem.ULI_TariffInfo);
		}

		public void TestULI_TariffValidationIncludePGACollection()
		{
			const string tariffNumber = "8542996328";

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNumber;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDate.Today;
			tariff.UE_PGACodes = "FD1";

			var cusUSLVItem = Factory.NewWithValidTestData<CusUSLVItem>();
			var fdaWrapper = cusUSLVItem.ItemPGAWrapperCollection.OfType<CusUSLVItemPGAWrapper>().Single(w => w.Agency == "FDA");
			AssertNullOrEmpty("pre-condition", fdaWrapper.Requirement);
			AssertNoMessageErrors(fdaWrapper.DisclaimReasonInfo);

			cusUSLVItem.ULI_Tariff = tariffNumber;
			fdaWrapper = cusUSLVItem.ItemPGAWrapperCollection.OfType<CusUSLVItemPGAWrapper>().Single(w => w.Agency == "FDA");
			AssertNullOrEmpty("pre-condition", fdaWrapper.DisclaimReason);
			AssertNotNullOrEmpty("pre-condition", fdaWrapper.Requirement);
			cusUSLVItem.RunPreSaveValidation();
			AssertHasMessageError("Child pga wrapper should be validated", fdaWrapper.DisclaimReasonInfo, "Agency Program declarations are not supported in this module and will need to be completed using a stand alone declaration.");
		}

		public void TestCheckULI_TariffTaxFeeCode()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1234567890";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDate.Today;
			var dutyRate1 = tariff1.DutyRates.AddNew();
			dutyRate1.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			dutyRate1.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;

			var dutyRate2 = tariff1.DutyRates.AddNew();
			dutyRate2.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			dutyRate2.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "9876543210";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDate.Today;

			var cusUSLVItem = Factory.NewWithValidTestData<CusUSLVItem>();
			cusUSLVItem.ULI_Tariff = tariff1.UE_Tariff;
			AssertHasMessageError("Tariff has an IRC tax code of 022,016.", cusUSLVItem.ULI_TariffInfo, "Tariff has an IRC tax code of 022,016.");

			cusUSLVItem.ULI_Tariff = tariff2.UE_Tariff;
			AssertNoMessageErrors(cusUSLVItem.ULI_TariffInfo);
		}

		public void TestCheckULI_Tariff_Quota()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "0000000000";
			tariff1.UE_DateFrom = ZDateTime.Today.AddMonths(-1);
			tariff1.UE_DateTo = ZDateTime.Today.AddMonths(1);
			tariff1.UE_QuotaIndicator = true;

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "1111111111";
			tariff2.UE_DateFrom = ZDateTime.Today.AddMonths(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddMonths(1);

			USCQuota quota = Factory.New<USCQuota>();
			quota.UT_BeginDate = ZDateTime.Today.AddMonths(-1);
			quota.UT_EndDate = ZDateTime.Today.AddMonths(1);
			quota.UT_PeriodProcessDateIndicator = PeriodProcessingDateIndicatorList.Codes.PresentationDate;
			quota.UT_Code = "0000000000";

			var cusUSLVItem = Factory.NewWithValidTestData<CusUSLVItem>();
			cusUSLVItem.ULI_Tariff = tariff1.UE_Tariff;
			AssertHasWarningContaining(cusUSLVItem.ULI_TariffInfo, "According to CBP reference files, this tariff may be subject to quota.");
			AssertHasWarningContaining(cusUSLVItem.ULI_TariffInfo, "There is a query record which indicates that the quota for this tariff was updated at Customs on");

			cusUSLVItem.ULI_Tariff = tariff2.UE_Tariff;
			AssertNoWarnings(cusUSLVItem.ULI_TariffInfo);

			tariff1.UE_QuotaIndicator = false;
			cusUSLVItem.ULI_Tariff = tariff1.UE_Tariff;
			AssertNoWarnings(cusUSLVItem.ULI_TariffInfo);
		}

		public void TestCheckULI_AntiDumpingApplies()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignent = shipment.CusUSLVConsignments.AddNew();
			var cusUSLVItem = consignent.CusUSLVItems.AddNew();
			new CusUSLVItemValidationTestHelper().CheckAntiDumping(cusUSLVItem.ULI_AntiDumpingInfo, cusUSLVItem);
		}

		public void TestCheckULI_CountervailingApplies()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignent = shipment.CusUSLVConsignments.AddNew();
			var cusUSLVItem = consignent.CusUSLVItems.AddNew();
			new CusUSLVItemValidationTestHelper().CheckCountervailing(cusUSLVItem.ULI_CountervailingInfo, cusUSLVItem);
		}

		public void TestCheckULI_PartNo_WarnIfSupplierOrImporterAreNotEntered()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();

			consignment.CusUSLVItems.AddNew();
			new CusUSLVItemValidationTestHelper().CheckProductCode_WarnIfSupplierOrImporterAreNotEntered(item, item.ULI_PartNoInfo);
		}

		public void TestCheckULI_PartNo_WarnWhenFoundButNotRelated()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();

			new CusUSLVItemValidationTestHelper().CheckProductCode_WarnWhenFoundButNotRelated(item, item.ULI_PartNoInfo);
		}

		public void TestCheckULI_PartNo_WarnWhenPartFoundButInactive()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();

			new CusUSLVItemValidationTestHelper().CheckProductCode_WarnWhenPartFoundButInactive(item, item.ULI_PartNoInfo);
		}

		public void TestCheckULI_PartNo_WarnWhenFoundButNotRelated_WithComplexRelationships()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();

			new CusUSLVItemValidationTestHelper().CheckProductCode_WarnWhenFoundButNotRelated_WithComplexRelationships(item, item.ULI_PartNoInfo);
		}

		public void TestCheckULI_PartNo_WarnPartCodeNotFoundAtAll()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();

			new CusUSLVItemValidationTestHelper().CheckProductCode_WarnPartCodeNotFoundAtAll(item, item.ULI_PartNoInfo);
		}

		public void TestCheckULI_PartNo_WarnIfMoreThanOneMatchingPart()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();

			new CusUSLVItemValidationTestHelper().CheckProductCode_WarnIfMoreThanOneMatchingPart(item, item.ULI_PartNoInfo);
		}

		public void TestCheckULI_GoodsValue()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();

			new CusUSLVItemValidationTestHelper().CheckLineValue(Factory, clearance, item, item.ULI_GoodsValueInfo);
		}

		public void TestCountryOfOrigin_WhenEqualsCA_HasMessageError()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();
			item.ULI_RN_NKCountryOfOrigin = "CA";
			AssertHasMessageError(item.ULI_RN_NKCountryOfOriginInfo, "'CA' is not a valid country of origin if the goods was exported from 'CA'. You should specify a province code starting with X.");

			item.ULI_RN_NKCountryOfOrigin = "XA";
			AssertNoMessageError(item.ULI_RN_NKCountryOfOriginInfo, "'CA' is not a valid country of origin if the goods was exported from 'CA'. You should specify a province code starting with X.");
		}

		public void TestGoodsValue_WhenEntryTypeIs86_IsMandatory()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			AssertEquals("precondition", ((IHeaderCommon)consignment).EntryType, "86");

			var cusUSLVItem = consignment.CusUSLVItems.AddNew();
			cusUSLVItem.RunPreSaveValidation();
			AssertHasMessageError(cusUSLVItem.ULI_GoodsValueInfo, "Line price is mandatory for entry type '86'.");
		}

		public void TestULI_GoodsDescription_IsMandatory_WhenThereIsPGARequirement()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			var cusUSLVItem = consignment.CusUSLVItems.AddNew();

			Assert("precondition", !(cusUSLVItem.CusUSLVItemPGAs.Count > 0));
			cusUSLVItem.ULI_GoodsDescription = "";
			AssertNoMessageError(cusUSLVItem.ULI_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			cusUSLVItem.CusUSLVItemPGAs.AddNew();
			Assert("precondition", cusUSLVItem.CusUSLVItemPGAs.Count > 0);
			cusUSLVItem.ULI_GoodsDescription = "";
			AssertHasMessageErrorContaining(cusUSLVItem.ULI_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
