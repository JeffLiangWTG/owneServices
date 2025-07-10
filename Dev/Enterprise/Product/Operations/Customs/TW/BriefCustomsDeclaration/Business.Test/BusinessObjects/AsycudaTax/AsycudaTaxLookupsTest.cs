using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	sealed class AsycudaTaxLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMethodOfCalculationList()
		{
			AssertEquals(TWRefCusCodeListTypes.GetMethodOfCalculationList(Factory), Lookups.MethodOfCalculationList);
		}

		public void TestRateOverrideReasonCodeList()
		{
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair(RateOverrideReasonCodeList.Codes.Additional, RateOverrideReasonCodeList.Descriptions.Additional);
			expectedList.AddPair(RateOverrideReasonCodeList.Codes.Override, RateOverrideReasonCodeList.Descriptions.Override);

			AssertEquals("Should have 2 element", 2, Lookups.RateOverrideReasonCodeList.Count);
			AssertContainsExactElementsInAnyOrder("RateOverrideReasonCodeList should have OVR and ADD", expectedList, Lookups.RateOverrideReasonCodeList);
		}

		public void TestMethodOfPaymentList()
		{
			var list = Lookups.MethodOfPaymentList;
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair(MethodOfPaymentList.Codes.DutyLevied, MethodOfPaymentList.Descriptions.DutyLevied);
			expectedList.AddPair(MethodOfPaymentList.Codes.DutyNotLevied, MethodOfPaymentList.Descriptions.DutyNotLevied);

			AssertContainsExactElementsInAnyOrder("MethodOfPaymentList should have CAS and DEF", expectedList, list);
		}

		public void TestChargeTypeList()
		{
			CodeDescriptionPairList GetListCAS()
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(ChargeTypeCASList.Codes.ImportDuty, ChargeTypeCASList.Descriptions.ImportDuty);
				result.AddPair(ChargeTypeCASList.Codes.CountervailingDuty, ChargeTypeCASList.Descriptions.CountervailingDuty);
				result.AddPair(ChargeTypeCASList.Codes.AntiDumpingDuty, ChargeTypeCASList.Descriptions.AntiDumpingDuty);
				result.AddPair(ChargeTypeCASList.Codes.RetaliatoryDuty, ChargeTypeCASList.Descriptions.RetaliatoryDuty);
				result.AddPair(ChargeTypeCASList.Codes.AdditionalDuty, ChargeTypeCASList.Descriptions.AdditionalDuty);
				result.AddPair(ChargeTypeCASList.Codes.CommodityTax, ChargeTypeCASList.Descriptions.CommodityTax);
				result.AddPair(ChargeTypeCASList.Codes.TobaccoAndAlcoholTax, ChargeTypeCASList.Descriptions.TobaccoAndAlcoholTax);
				result.AddPair(ChargeTypeCASList.Codes.HealthAndWelfareSurcharge, ChargeTypeCASList.Descriptions.HealthAndWelfareSurcharge);
				result.AddPair(ChargeTypeCASList.Codes.BusinessTax, ChargeTypeCASList.Descriptions.BusinessTax);
				result.AddPair(ChargeTypeCASList.Codes.SpecificallySelectedGoodsAndServicesTax, ChargeTypeCASList.Descriptions.SpecificallySelectedGoodsAndServicesTax);
				result.AddPair(ChargeTypeCASList.Codes.LateDeclarationFee, ChargeTypeCASList.Descriptions.LateDeclarationFee);
				result.AddPair(ChargeTypeCASList.Codes.LatePaymentFee, ChargeTypeCASList.Descriptions.LatePaymentFee);
				return result;
			}

			CodeDescriptionPairList GetListDEF()
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(ChargeTypeDEFList.Codes.ImportDuty, ChargeTypeDEFList.Descriptions.ImportDuty);
				result.AddPair(ChargeTypeDEFList.Codes.CommodityTax, ChargeTypeDEFList.Descriptions.CommodityTax);
				result.AddPair(ChargeTypeDEFList.Codes.BusinessTax, ChargeTypeDEFList.Descriptions.BusinessTax);
				result.AddPair(ChargeTypeDEFList.Codes.TobaccoAndAlcoholTax, ChargeTypeDEFList.Descriptions.TobaccoAndAlcoholTax);
				result.AddPair(ChargeTypeDEFList.Codes.HealthAndWelfareSurcharge, ChargeTypeDEFList.Descriptions.HealthAndWelfareSurcharge);
				result.AddPair(ChargeTypeDEFList.Codes.SpecificallySelectedGoodsAndServicesTax, ChargeTypeDEFList.Descriptions.SpecificallySelectedGoodsAndServicesTax);
				return result;
			}

			var expectedListCASIMP = GetListCAS();
			expectedListCASIMP.AddPair(ChargeTypeCASList.Codes.ImportTradePromotionFee, ChargeTypeCASList.Descriptions.ImportTradePromotionFee);

			var expectedListCASEXP = GetListCAS();
			expectedListCASEXP.AddPair(ChargeTypeCASList.Codes.ExportTradePromotionFee, ChargeTypeCASList.Descriptions.ExportTradePromotionFee);

			var expectedListDEFIMP = GetListDEF();
			expectedListDEFIMP.AddPair(ChargeTypeDEFList.Codes.ImportTradePromotionFee, ChargeTypeDEFList.Descriptions.ImportTradePromotionFee);

			var expectedListDEF = GetListDEF();

			CombineAssertions(() =>
			{
				Tax.AET_MethodOfPayment = MethodOfPaymentList.Codes.DutyLevied;
				Header.AMA_Nature = "IMP";
				AssertContainsExactElementsInAnyOrder("ChargeTypeList is not expect when MethodOfPayment is 'CAS', AsycudaManifestHeader.AMA_Nature is 'IMP'", expectedListCASIMP, Lookups.ChargeTypeList);

				Tax.AET_MethodOfPayment = MethodOfPaymentList.Codes.DutyLevied;
				Header.AMA_Nature = "EXP";
				AssertContainsExactElementsInAnyOrder("ChargeTypeList is not expect when MethodOfPayment is 'CAS', AsycudaManifestHeader.AMA_Nature is 'EXP'", expectedListCASEXP, Lookups.ChargeTypeList);

				Tax.AET_MethodOfPayment = MethodOfPaymentList.Codes.DutyNotLevied;
				Header.AMA_Nature = "IMP";
				AssertContainsExactElementsInAnyOrder("ChargeTypeList is not expect when MethodOfPayment is 'DEF', AsycudaManifestHeader.AMA_Nature is 'IMP'", expectedListDEFIMP, Lookups.ChargeTypeList);

				Tax.AET_MethodOfPayment = MethodOfPaymentList.Codes.DutyNotLevied;
				Header.AMA_Nature = "EXP";
				AssertContainsExactElementsInAnyOrder("ChargeTypeList is not expect when MethodOfPayment is 'DEF', AsycudaManifestHeader.AMA_Nature is 'EXP'", expectedListDEF, Lookups.ChargeTypeList);
			});
		}

		AsycudaManifestHeader header;
		AsycudaManifestHeader Header => header ?? (header = Factory.NewWithValidTestData<AsycudaManifestHeader>());

		AsycudaTax tax;
		AsycudaTax Tax => tax ?? (tax = Header.Bills.AddNew().AsycudaTaxes.AddNew());

		AsycudaTaxLookups Lookups => Tax.Lookups;
	}
}
