using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItemTax))]
	sealed class AsycudaPackedItemTaxTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var tax = Factory.New<AsycudaPackedItemTax>();
			AssertEquals(TaxFeePaymentMethodList.Codes.CAS, tax.AET_MethodOfPayment);
		}

		[ExpectNoExceptions]
		public void TestPropertyCaptions()
		{
			var tax = Tax;
			CombineAssertions(() =>
			{
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(tax.AET_ChargeTypeInfo, "Type", "The types of the duties other than tariffs.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(tax.AET_ChargeTypeDescInfo, "Type Description", "Type Desc.", "The type description of the duties other than tariffs.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(tax.AET_TariffInfo, "Tariff", "The types of the goods belonging to the duties other than tariffs.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(tax.AET_TariffDescInfo, "Tariff Description", "Tariff Desc.", "The types of the goods belonging to the duties other than tariffs.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(tax.AET_RateInfo, "Rate", "The rate for duties, taxes and fees.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(tax.AET_MethodOfCalculationInfo, "Method Of Calculation", "The Method Of Calculation for duties, taxes and fees.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(tax.AET_BaseValueInfo, "Base Amount", "The Base Amount for duties, taxes and fees.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(tax.AET_ChargeAmountInfo, "Amount", "The Amount of duties, taxes and fees.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(tax.AET_MethodOfPaymentInfo, "Payment Method", "The payment method for duties other than tariffs.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(tax.AET_MethodOfPaymentDescInfo, "Payment Method Description", "Payment Method Desc.", "The payment method description for duties other than tariffs.");
				BusinessObjectCaptionTestHelper.AssertCaptions(tax.AET_RateOverrideReasonCodeInfo, "Action");
			});
		}

		public void TestLookupsType()
		{
			AssertType<AsycudaPackedItemTaxLookups>(Tax.Lookups);
		}

		public void TestValidationType()
		{
			AssertType<AsycudaPackedItemTaxValidation>(Tax.Validation);
		}

		public void TestAET_ChargeType()
		{
			var tax = Tax;
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.AT;
			CombineAssertions(() =>
			{
				AssertEquals(true, tax.IsBelongVATTaxCharge);
				AssertEquals(true, tax.IsTaxCharge);
				AssertEquals(false, tax.IsTobaccoTax);
				AssertEquals(false, tax.IsHealthWelfareSurcharge);
				AssertEquals(false, tax.IsDuty);
				AssertEquals(false, tax.IsVAT);
				AssertEquals(false, tax.IsTPF);
				AssertEquals(false, tax.IsPercentageCharge);
			});

			tax.AET_ChargeType = ChargeTypeOtherList.Codes.CT;
			CombineAssertions(() =>
			{
				AssertEquals(true, tax.IsBelongVATTaxCharge);
				AssertEquals(true, tax.IsTaxCharge);
				AssertEquals(false, tax.IsTobaccoTax);
				AssertEquals(false, tax.IsHealthWelfareSurcharge);
				AssertEquals(false, tax.IsDuty);
				AssertEquals(false, tax.IsVAT);
				AssertEquals(false, tax.IsTPF);
				AssertEquals(false, tax.IsPercentageCharge);
			});

			tax.AET_ChargeType = ChargeTypeOtherList.Codes.SS;
			CombineAssertions(() =>
			{
				AssertEquals(false, tax.IsBelongVATTaxCharge);
				AssertEquals(true, tax.IsTaxCharge);
				AssertEquals(false, tax.IsTobaccoTax);
				AssertEquals(false, tax.IsHealthWelfareSurcharge);
				AssertEquals(false, tax.IsDuty);
				AssertEquals(false, tax.IsVAT);
				AssertEquals(false, tax.IsTPF);
				AssertEquals(false, tax.IsPercentageCharge);
			});

			tax.AET_ChargeType = ChargeTypeOtherList.Codes.TT;
			CombineAssertions(() =>
			{
				AssertEquals(true, tax.IsBelongVATTaxCharge);
				AssertEquals(true, tax.IsTaxCharge);
				AssertEquals(true, tax.IsTobaccoTax);
				AssertEquals(false, tax.IsHealthWelfareSurcharge);
				AssertEquals(false, tax.IsDuty);
				AssertEquals(false, tax.IsVAT);
				AssertEquals(false, tax.IsTPF);
				AssertEquals(false, tax.IsPercentageCharge);
			});

			tax.AET_ChargeType = ChargeTypeOtherList.Codes.HWS;
			CombineAssertions(() =>
			{
				AssertEquals(true, tax.IsBelongVATTaxCharge);
				AssertEquals(false, tax.IsTaxCharge);
				AssertEquals(false, tax.IsTobaccoTax);
				AssertEquals(true, tax.IsHealthWelfareSurcharge);
				AssertEquals(false, tax.IsDuty);
				AssertEquals(false, tax.IsVAT);
				AssertEquals(false, tax.IsTPF);
				AssertEquals(false, tax.IsPercentageCharge);
			});

			tax.AET_ChargeType = ChargeTypeOtherList.Codes.DTA;
			CombineAssertions(() =>
			{
				AssertEquals(false, tax.IsBelongVATTaxCharge);
				AssertEquals(false, tax.IsTaxCharge);
				AssertEquals(false, tax.IsTobaccoTax);
				AssertEquals(false, tax.IsHealthWelfareSurcharge);
				AssertEquals(true, tax.IsDuty);
				AssertEquals(false, tax.IsVAT);
				AssertEquals(false, tax.IsTPF);
				AssertEquals(true, tax.IsPercentageCharge);
			});

			tax.AET_ChargeType = ChargeTypeOtherList.Codes.DTS;
			CombineAssertions(() =>
			{
				AssertEquals(false, tax.IsBelongVATTaxCharge);
				AssertEquals(false, tax.IsTaxCharge);
				AssertEquals(false, tax.IsTobaccoTax);
				AssertEquals(false, tax.IsHealthWelfareSurcharge);
				AssertEquals(true, tax.IsDuty);
				AssertEquals(false, tax.IsVAT);
				AssertEquals(false, tax.IsTPF);
				AssertEquals(false, tax.IsPercentageCharge);
			});

			tax.AET_ChargeType = ChargeTypeOtherList.Codes.VAT;
			CombineAssertions(() =>
			{
				AssertEquals(false, tax.IsBelongVATTaxCharge);
				AssertEquals(false, tax.IsTaxCharge);
				AssertEquals(false, tax.IsTobaccoTax);
				AssertEquals(false, tax.IsHealthWelfareSurcharge);
				AssertEquals(false, tax.IsDuty);
				AssertEquals(true, tax.IsVAT);
				AssertEquals(false, tax.IsTPF);
				AssertEquals(true, tax.IsPercentageCharge);
			});

			tax.AET_ChargeType = ChargeTypeOtherList.Codes.TPF;
			CombineAssertions(() =>
			{
				AssertEquals(false, tax.IsBelongVATTaxCharge);
				AssertEquals(false, tax.IsTaxCharge);
				AssertEquals(false, tax.IsTobaccoTax);
				AssertEquals(false, tax.IsHealthWelfareSurcharge);
				AssertEquals(false, tax.IsDuty);
				AssertEquals(false, tax.IsVAT);
				AssertEquals(true, tax.IsTPF);
				AssertEquals(true, tax.IsPercentageCharge);
			});
		}

		public void TestAET_ChargeTypeDesc()
		{
			var tax = Tax;
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.CT;
			AssertEquals(ChargeTypeOtherList.Descriptions.CT, tax.AET_ChargeTypeDesc);
		}

		public void TestClearTariffWhenChargeTypeChanged()
		{
			var tax1 = Tax;
			tax1.AET_ChargeType = ChargeTypeOtherList.Codes.TT;
			tax1.AET_Tariff = "A";

			tax1.AET_ChargeType = ChargeTypeOtherList.Codes.HWS;
			AssertEquals("HWS of Tariff must be Clear", ZString.Empty, tax1["AET_Tariff"]);

			tax1.AET_Tariff = "B";
			tax1.AET_ChargeType = ChargeTypeOtherList.Codes.DTA;
			AssertEquals("DTA of Tariff must be Clear", ZString.Empty, tax1["AET_Tariff"]);

			tax1.AET_Tariff = "C";
			tax1.AET_ChargeType = ChargeTypeOtherList.Codes.DTS;
			AssertEquals("DTS of Tariff must be Clear", ZString.Empty, tax1["AET_Tariff"]);
		}

		public void TestAET_TariffReadOnly()
		{
			var tax = Tax;
			CombineAssertions(() =>
			{
				tax.AET_ChargeType = ChargeTypeOtherList.Codes.AT;
				AssertEquals("AET_Tariff should editable when AET_ChargeType is AT.", false, tax.AET_TariffReadOnly);
				tax.AET_ChargeType = ChargeTypeOtherList.Codes.CT;
				AssertEquals("AET_Tariff should editable when AET_ChargeType is CT.", false, tax.AET_TariffReadOnly);
				tax.AET_ChargeType = ChargeTypeOtherList.Codes.SS;
				AssertEquals("AET_Tariff should editable when AET_ChargeType is SS.", false, tax.AET_TariffReadOnly);
				tax.AET_ChargeType = ChargeTypeOtherList.Codes.TT;
				AssertEquals("AET_Tariff should editable when AET_ChargeType is TT.", false, tax.AET_TariffReadOnly);
				tax.AET_ChargeType = ChargeTypeOtherList.Codes.DTA;
				AssertEquals("AET_Tariff should read only when AET_ChargeType is DTA.", true, tax.AET_TariffReadOnly);
				tax.AET_ChargeType = ChargeTypeOtherList.Codes.DTS;
				AssertEquals("AET_Tariff should read only when AET_ChargeType is DTS.", true, tax.AET_TariffReadOnly);
				tax.AET_ChargeType = ChargeTypeOtherList.Codes.HWS;
				AssertEquals("AET_Tariff should read only when AET_ChargeType is HWS.", true, tax.AET_TariffReadOnly);
			});
		}

		public void TestAET_TariffDesc()
		{
			AsycudaPackedItemTaxHelperForTest.CreateTariffData(Factory);
			var tax = Tax;
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.CT;
			tax.AET_Tariff = "CTTariff";
			AssertEquals("Test Desc", tax.AET_TariffDesc);
		}

		public void TestTariffDefaultingFromType()
		{
			AsycudaPackedItemTaxHelperForTest.CreateTariffData(Factory);
			var packedItem = Tax.PackedItem;
			packedItem.API_Tariff = "87031000002";
			var tax = packedItem.AsycudaTaxes.AddNew();
			using (tax.SuspendPackedItemTaxDefaulting())
			{
				tax.AET_ChargeType = ChargeTypeOtherList.Codes.SS;
				AssertEquals(ZString.Empty, tax.AET_Tariff);
				tax.AET_ChargeType = ChargeTypeOtherList.Codes.CT;
				AssertEquals(ZString.Empty, tax.AET_Tariff);
			}

			tax.AET_ChargeType = ChargeTypeOtherList.Codes.SS;
			AssertEquals(ZString.Empty, tax.AET_Tariff);
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.CT;
			AssertEquals("CTTariff", tax.AET_Tariff);
		}

		public void TestDefaultAET_MethodOfCalculationWhenAET_ChargeTypeChanged()
		{
			var packedItem = Tax.PackedItem;
			var tax = packedItem.AsycudaTaxes.AddNew();
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.DTA;
			AssertEquals("AET_MethodOfCalculation should be % when DTA", "%", tax.AET_MethodOfCalculation);

			tax.AET_MethodOfCalculation = ZString.Empty;
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.VAT;
			AssertEquals("AET_MethodOfCalculation should be % when VAT", "%", tax.AET_MethodOfCalculation);

			tax.AET_MethodOfCalculation = ZString.Empty;
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.TPF;
			AssertEquals("AET_MethodOfCalculation should be % when TPF", "%", tax.AET_MethodOfCalculation);
		}

		public void TestSetRateAndMethodOfCalculation()
		{
			AsycudaPackedItemTaxHelperForTest.CreateTariffData(Factory);
			var packedItem = Tax.PackedItem;
			packedItem.API_Tariff = "87031000002";
			packedItem.API_RN_NKGoodsOrigin = "JP";
			packedItem.API_Preference = "PR1";
			var tax = packedItem.AsycudaTaxes.AddNew();
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.DTA;
			CombineAssertions("ZZ2_RateFormula is 0.15 * VFD", () =>
			{
				AssertEquals(0.15m, tax.AET_Rate);
				AssertEquals("%", tax.AET_MethodOfCalculation);
			});

			tax.AET_ChargeType = ChargeTypeOtherList.Codes.DTS;
			CombineAssertions("ZZ2_RateFormula is 170 * [LTR]", () =>
			{
				AssertEquals(170m, tax.AET_Rate);
				AssertEquals("LTR", tax.AET_MethodOfCalculation);
			});

			tax.AET_ChargeType = ChargeTypeOtherList.Codes.AT;
			tax.AET_Tariff = "ATTariff";
			CombineAssertions("ZZ2_RateFormula is 26 * [LTR]", () =>
			{
				AssertEquals(26m, tax.AET_Rate);
				AssertEquals("LTR", tax.AET_MethodOfCalculation);
			});

			tax.AET_ChargeType = ChargeTypeOtherList.Codes.CT;
			tax.AET_Tariff = "CTTariff";
			CombineAssertions("ZZ2_RateFormula is 0.2 * VFD", () =>
			{
				AssertEquals(0.2m, tax.AET_Rate);
				AssertEquals("%", tax.AET_MethodOfCalculation);
			});

			tax.AET_ChargeType = ChargeTypeOtherList.Codes.SS;
			tax.AET_Tariff = "SSTariff";
			CombineAssertions("ZZ2_RateFormula is 150 * [LTR]", () =>
			{
				AssertEquals(150m, tax.AET_Rate);
				AssertEquals("LTR", tax.AET_MethodOfCalculation);
			});

			tax.AET_Tariff = "SEDAN";
			CombineAssertions("ZZ2_RateFormula is 0.05*VFD", () =>
			{
				AssertEquals(0.05m, tax.AET_Rate);
				AssertEquals("%", tax.AET_MethodOfCalculation);
			});

			packedItem.API_CustomsValue = 500000m;
			tax.AET_Tariff = "ASUS";
			CombineAssertions("ZZ2_RateFormula is IF(UnitCustomsValue >= 3000000,0.1 * VFD,0) and CustomsValue = 500,000", () =>
			{
				AssertEquals(0m, tax.AET_Rate);
				AssertEquals("%", tax.AET_MethodOfCalculation);
			});

			tax.AET_Tariff = "OPPO";
			CombineAssertions("ZZ2_RateFormula is IF(UnitCustomsValue >= 500000,0.1 * VFD,0) and CustomsValue = 500,000", () =>
			{
				AssertEquals(0.1m, tax.AET_Rate);
				AssertEquals("%", tax.AET_MethodOfCalculation);
			});

			tax.AET_ChargeType = ChargeTypeOtherList.Codes.TT;
			tax.AET_Tariff = "TTTariff";
			CombineAssertions("ZZ2_RateFormula is 1590 * [KGM]", () =>
			{
				AssertEquals(1590m, tax.AET_Rate);
				AssertEquals("KGM", tax.AET_MethodOfCalculation);
			});

			var taxes = tax.PackedItem.AsycudaTaxes;
			var taxHws = taxes.Where(tax => tax.AET_ChargeType == ChargeTypeOtherList.Codes.HWS).FirstOrDefault();
			CombineAssertions("ZZ2_RateFormula is 1000 * [KGM]", () =>
			{
				AssertEquals(1000m, taxHws.AET_Rate);
				AssertEquals("KGM", taxHws.AET_MethodOfCalculation);
			});
		}

		[TestDate(2024, 5, 21)]
		public void TestEffectiveAssessmentDate()
		{
			var tax = Tax;
			var header = tax.PackedItem.Header;
			header.DeclarationDate = new ZDateTime(2024, 5, 20);
			AssertEquals(new ZDateTime(2024, 5, 20), tax.EffectiveAssessmentDate);

			var singleTax = Factory.New<AsycudaPackedItemTax>();
			AssertEquals(new ZDateTime(2024, 5, 21), singleTax.EffectiveAssessmentDate);
		}

		public void TestAET_MethodOfPaymentDesc()
		{
			var tax = Tax;
			tax.AET_MethodOfPayment = TaxFeePaymentMethodList.Codes.CAS;
			AssertEquals(TaxFeePaymentMethodList.Descriptions.CAS, tax.AET_MethodOfPaymentDesc);
		}

		public void TestCalculatedAET_ChargeAmount()
		{
			AsycudaPackedItemTaxHelperForTest.CreateVATRate(Factory);
			var tax = Tax;
			var packedItem = tax.PackedItem;
			packedItem.API_CustomsValue = 100m;
			var asycudaTaxes = packedItem.AsycudaTaxes.Cast<AsycudaPackedItemTax>();
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.CT;
			tax.AET_Rate = 0.01m;
			tax.AET_BaseValue = 155m;
			AssertEquals("AET_Rate = 0.01, AET_BaseValue = 155", 1.55m, tax.AET_ChargeAmount);
			AssertEquals("VAT.AET_ChargeAmount", 5.078m, asycudaTaxes.FirstOrDefault(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.VAT).AET_ChargeAmount);

			tax.AET_Rate = 0.03m;
			AssertEquals("AET_Rate = 0.03, AET_BaseValue = 155", 4.65m, tax.AET_ChargeAmount);
			AssertEquals("VAT.AET_ChargeAmount", 5.233m, asycudaTaxes.FirstOrDefault(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.VAT).AET_ChargeAmount);

			tax.AET_BaseValue = 20m;
			AssertEquals("AET_Rate = 0.03, AET_BaseValue = 20", 0.6m, tax.AET_ChargeAmount);
			AssertEquals("VAT.AET_ChargeAmount", 5.030m, asycudaTaxes.FirstOrDefault(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.VAT).AET_ChargeAmount);
		}

		public void TestDefaultBaseValueIfNeed()
		{
			var tax = Tax;
			var packedItem = tax.PackedItem;
			packedItem.API_CustomsValue = 100m;
			var asycudaTaxes = packedItem.AsycudaTaxes;
			var vatTax = asycudaTaxes.AddNew();
			vatTax.AET_ChargeType = ChargeTypeOtherList.Codes.VAT;
			AssertEquals(100m, vatTax.AET_BaseValue);

			var atTax = asycudaTaxes.AddNew();
			atTax.AET_ChargeType = ChargeTypeOtherList.Codes.AT;
			atTax.AET_Rate = 0.5m;
			atTax.AET_MethodOfCalculation = UniversalReferenceConstants.MethodOfCalculation.Percentage;
			atTax.AET_BaseValue = 100m;

			vatTax = asycudaTaxes.AddNew();
			vatTax.AET_ChargeType = ChargeTypeOtherList.Codes.VAT;
			AssertEquals(150m, vatTax.AET_BaseValue);
		}

		public void TestAET_MethodOfPaymentReadOnly()
		{
			AssertEquals(true, Tax.AET_MethodOfPaymentInfo.ReadOnly);
		}

		public void TestSetAET_BaseValue()
		{
			var tax = Tax;
			var packedItem = Tax.PackedItem;
			packedItem.API_CustomsQty = 120m;
			packedItem.API_CustomsUQ = "SET";
			packedItem.API_NetWeight = 140m;
			packedItem.API_NetWeightUQ = "KG";
			packedItem.API_CustomsValue = 160m;
			tax.AET_MethodOfCalculation = "SET";
			AssertEquals("AET_BaseValue equals API_CustomsQty when AET_MethodOfCalculation equals API_CustomsUQ", 120m, tax.AET_BaseValue);
			tax.AET_MethodOfCalculation = "DZN";
			AssertEquals("AET_BaseValue = API_CustomsQty / 12(SET/DZN)", 10m, tax.AET_BaseValue);
			tax.AET_MethodOfCalculation = "KGM";
			AssertEquals("AET_BaseValue = API_NetWeight / 1(KG/KGM)", 140m, tax.AET_BaseValue);
			tax.AET_MethodOfCalculation = "TNE";
			AssertEquals("AET_BaseValue = API_NetWeight / 1000(KG/TNE) ", 0.14m, tax.AET_BaseValue);
			tax.AET_MethodOfCalculation = UniversalReferenceConstants.MethodOfCalculation.Percentage;
			AssertEquals("AET_BaseValue equals API_CustomsValue when AET_MethodOfCalculation is %", 160m, tax.AET_BaseValue);
		}

		public void TestAET_RateReadOnly()
		{
			var tax = Tax;
			tax.AET_RateOverrideReasonCode = RateOverrideReasonCodeList.Codes.Override;
			AssertEquals("Editable When AET_RateOverrideReasonCode is not empty.", false, Tax.AET_RateInfo.ReadOnly);
			tax.AET_RateOverrideReasonCode = ZString.Empty;
			AssertEquals("ReadOnly When AET_RateOverrideReasonCode is empty.", true, Tax.AET_RateInfo.ReadOnly);
		}

		public void TestAET_MethodOfCalculationReadOnly()
		{
			var tax = Tax;
			tax.AET_RateOverrideReasonCode = RateOverrideReasonCodeList.Codes.Override;
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.VAT;
			AssertEquals("ReadOnly When AET_ChargeType is VAT.", true, Tax.AET_MethodOfCalculationInfo.ReadOnly);
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.TPF;
			AssertEquals("ReadOnly When AET_ChargeType is TPF.", true, Tax.AET_MethodOfCalculationInfo.ReadOnly);
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.DTA;
			AssertEquals("ReadOnly When AET_ChargeType is DTA.", true, Tax.AET_MethodOfCalculationInfo.ReadOnly);
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.HWS;
			AssertEquals("Editable When AET_ChargeType is HWS and AET_RateOverrideReasonCode is not empty. ", false, Tax.AET_MethodOfCalculationInfo.ReadOnly);
			tax.AET_RateOverrideReasonCode = ZString.Empty;
			AssertEquals("ReadOnly When AET_RateOverrideReasonCode is empty.", true, Tax.AET_MethodOfCalculationInfo.ReadOnly);
		}

		public void TestAET_ChargeAmountReadOnly()
		{
			var tax = Tax;
			tax.AET_RateOverrideReasonCode = RateOverrideReasonCodeList.Codes.Override;
			AssertEquals("Editable When AET_RateOverrideReasonCode is not empty.", false, Tax.AET_ChargeAmountInfo.ReadOnly);
			tax.AET_RateOverrideReasonCode = ZString.Empty;
			AssertEquals("ReadOnly When AET_RateOverrideReasonCode is empty.", true, Tax.AET_ChargeAmountInfo.ReadOnly);
		}

		public void TestAET_BaseValueReadOnly()
		{
			var tax = Tax;
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.CT;
			AssertEquals("Editable When AET_ChargeType is not empty.", false, Tax.AET_BaseValueReadOnly);
			tax.AET_ChargeType = ZString.Empty;
			AssertEquals("ReadOnly When AET_ChargeType is empty.", true, Tax.AET_BaseValueReadOnly);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_CustomsValue = 2000m;
			var asycudaTax = bill.PackedItems.AddNew().AsycudaTaxes.AddNew();
			asycudaTax.AET_MethodOfCalculation = UniversalReferenceConstants.MethodOfCalculation.Percentage;
			return asycudaTax;
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues() => new List<ZString>() { AsycudaTax.Schema.AET_API_AsycudaPackedItem };

		AsycudaPackedItemTax tax;
		AsycudaPackedItemTax Tax => tax ?? (tax = GetNewBusinessObject() as AsycudaPackedItemTax);
	}
}
