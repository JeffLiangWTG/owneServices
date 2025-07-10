using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AppendixBTaxRateListTest : TestCaseWithFactory
	{
		public void TestMatchUQForWineLiters()
		{
			Assert("Liters = Wine Liters", !AppendixBTaxRateList.DoesUQMatchNoneOfCustomsUQsCore("WL", "L", ""));
		}

		public void TestSpecifyDollars()
		{
			AssertEquals("new specify code", "Specify", AppendixBTaxRateList.Codes.Specify);
			AssertEquals("new specify description", "Specify a tax rate in Dollars per 1st Customs Qty.", AppendixBTaxRateList.Descriptions.Specify);
		}

		public void TestGetUQ()
		{
			AssertEquals("PFL", AppendixBTaxRateList.GetUQ(AppendixBTaxRateList.Codes.DistilledSpirits));
			AssertEquals("L", AppendixBTaxRateList.GetUQ(AppendixBTaxRateList.Codes.Wines_1));
			AssertEquals("", AppendixBTaxRateList.GetUQ(AppendixBTaxRateList.Codes.Specify));
		}

		public void TestGetComputationCode()
		{
			AssertEquals(ComputationCodeList.Codes.AdValorem, AppendixBTaxRateList.GetComputationCode(AppendixBTaxRateList.Codes.Tobacco_2, ZString.Empty, ZString.Empty, ZString.Empty));

			SetUpTariffsForTaxRelatedFields();
			AssertEquals(ComputationCodeList.Codes.SpecificRateFirstQuantity, AppendixBTaxRateList.GetComputationCode(AppendixBTaxRateList.Codes.Specify, ZString.Empty, ZString.Empty, ZString.Empty));
			AssertEquals(ComputationCodeList.Codes.SpecificRateFirstQuantity, AppendixBTaxRateList.GetComputationCode(AppendixBTaxRateList.CBMAEligible, ZString.Empty, ZString.Empty, ZString.Empty));
			AssertEquals(ComputationCodeList.Codes.SpecificRateFirstQuantity, AppendixBTaxRateList.GetComputationCode(AppendixBTaxRateList.Codes.Other_1, "2402209000", "K", "KG"));

			AssertEquals(ComputationCodeList.Codes.SpecificRateFirstQuantity, AppendixBTaxRateList.GetComputationCode(AppendixBTaxRateList.Codes.Wines_1, "00000000", "L", ZString.Empty));
			AssertEquals(ComputationCodeList.CustomComputationCodeForCalculatingIRTax, AppendixBTaxRateList.GetComputationCode(AppendixBTaxRateList.Codes.Wines_1, "00000000", "KG", ZString.Empty));

			AssertEquals(ComputationCodeList.Codes.SpecificRateFirstQuantity, AppendixBTaxRateList.GetComputationCode(AppendixBTaxRateList.Codes.Tobacco_6, "2403102050", "KG", ZString.Empty));
			AssertEquals(ComputationCodeList.CustomComputationCodeForCalculatingIRTax, AppendixBTaxRateList.GetComputationCode(AppendixBTaxRateList.Codes.Tobacco_1, "2403102050", "KG", ZString.Empty));

			AssertEquals(ComputationCodeList.Codes.SpecificRateFirstQuantity, AppendixBTaxRateList.GetComputationCode(AppendixBTaxRateList.Codes.Specify, "2403102050", "KG", ZString.Empty));

			AssertEquals(ComputationCodeList.Codes.SpecificRateSecondQuantity, AppendixBTaxRateList.GetComputationCode(AppendixBTaxRateList.Codes.Other_1, "2402209000", "KG", "K"));
		}

		public void TestDoesUQMatchNoneOfCustomsUQs()
		{
			Assert(AppendixBTaxRateList.DoesUQMatchNoneOfCustomsUQs(AppendixBTaxRateList.Codes.Wines_1, "KG", ZString.Empty));
			Assert(!AppendixBTaxRateList.DoesUQMatchNoneOfCustomsUQs(AppendixBTaxRateList.Codes.Wines_1, "L", ZString.Empty));
		}

		public void TestGetRate()
		{
			AssertEquals(0.0315m, AppendixBTaxRateList.GetRate(AppendixBTaxRateList.Codes.Other_1));
			AssertEquals(0.5275m, AppendixBTaxRateList.GetRate(AppendixBTaxRateList.Codes.Tobacco_2));
			AssertEquals(0m, AppendixBTaxRateList.GetRate(AppendixBTaxRateList.Codes.Specify));
		}

		public void TestFilteredListOnTaxCode()
		{
			AssertEquals(2, new AppendixBTaxRateList(Core.Constants.USCustoms.FeeCodes.DistilledSpirits).Count);
			AssertEquals(7, new AppendixBTaxRateList(Core.Constants.USCustoms.FeeCodes.Wines).Count);
			AssertEquals(9, new AppendixBTaxRateList(Core.Constants.USCustoms.FeeCodes.Tobacco).Count);
			AssertEquals(5, new AppendixBTaxRateList(Core.Constants.USCustoms.FeeCodes.OtherExcise).Count);
		}

		public void TestGetNormalTaxRateString()
		{
			SetUpTariffsForTaxRelatedFields();

			USCTariff tariff = new USCTariff.Loader(Factory).LoadBestMatch("00000000", ZDateTime.Today);
			AssertNotNull(tariff);
			AssertEquals("50c/KG", AppendixBTaxRateList.GetNormalTaxRateString(tariff, Core.Constants.USCustoms.FeeCodes.Wines, ZString.Empty));

			tariff = new USCTariff.Loader(Factory).LoadBestMatch("00000002", ZDateTime.Today);
			AssertNotNull(tariff);
			AssertEquals("70c/PFL", AppendixBTaxRateList.GetNormalTaxRateString(tariff, Core.Constants.USCustoms.FeeCodes.DistilledSpirits, RateTypeList.Codes.Primary));
			AssertEquals("80c/PFL", AppendixBTaxRateList.GetNormalTaxRateString(tariff, Core.Constants.USCustoms.FeeCodes.DistilledSpirits, RateTypeList.Codes.Secondary));
		}

		public void TestTaxRateUpdated()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CD_TaxApplicability = TaxApplyList.Codes.Override;
			pivot.CD_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			pivot.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.C;
			AssertEquals("pivot.CD_TaxRateDesc", ZString.Empty, pivot.CD_TaxRateDesc);
			AssertEquals("pivot.CD_TaxRate", ZDecimal.Zero, pivot.CD_TaxRate);

			pivot.CD_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			AssertEquals("pivot.CD_TaxRateDesc", ZString.Empty, pivot.CD_TaxRateDesc);
			AssertEquals("pivot.CD_TaxRate", ZDecimal.Zero, pivot.CD_TaxRate);
			AssertEquals("pivot.CD_TTBRateDesignationCode", ZString.Empty, pivot.CD_TTBRateDesignationCode);
			AssertEquals("pivot.CD_CBMADefaultTaxRate", ZDecimal.Zero, pivot.CD_CBMADefaultTaxRate);

			pivot.CD_TaxRateDesc = "28.26641c/L";
			AssertEquals("pivot.CD_TaxRate", 0.28266410m, pivot.CD_TaxRate);
			pivot.CD_TaxRateDesc = "41.47501c/L";
			AssertEquals("pivot.CD_TaxRate", 0.4147501m, pivot.CD_TaxRate);
			pivot.CD_TaxRateDesc = "83.2142c/L";
			AssertEquals("pivot.CD_TaxRate", 0.83214200m, pivot.CD_TaxRate);
			pivot.CD_TaxRateDesc = "89.8185c/L";
			AssertEquals("pivot.CD_TaxRate", 0.89818500m, pivot.CD_TaxRate);
			pivot.CD_TaxRateDesc = "3.5663227c/L";
			AssertEquals("pivot.CD_TaxRate", 0.035663227m, pivot.CD_TaxRate);
			pivot.CD_TaxRateDesc = "15.33905c/L";
			AssertEquals("pivot.CD_TaxRate", 0.15339050m, pivot.CD_TaxRate);

			AssertEquals("$3.5663227/PFL", AppendixBTaxRateList.Codes.DistilledSpirits);
			AssertEquals("15.33902c/L", AppendixBTaxRateList.Codes.Other_3);
			AssertEquals("$3.5663227/L", AppendixBTaxRateList.Codes.Other_4);
			AssertEquals("15.33902c/L", AppendixBTaxRateList.Codes.Wines_1);
			AssertEquals("28.26641c/WL", AppendixBTaxRateList.Codes.Wines_2);
			AssertEquals("41.47501c/WL", AppendixBTaxRateList.Codes.Wines_3);
			AssertEquals("83.2142c/WL", AppendixBTaxRateList.Codes.Wines_4);
			AssertEquals("89.8185c/WL", AppendixBTaxRateList.Codes.Wines_5);
			AssertEquals("87.17678c/WL", AppendixBTaxRateList.Codes.Wines_6);
		}

		void SetUpTariffsForTaxRelatedFields()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "KG";

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeSpecificRate = 0.5m;

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "00000001";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff2.UE_Unit1 = "L";

			USCTariffDutyRate dutyRate2 = tariff2.DutyRates.AddNew();
			dutyRate2.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			dutyRate2.UD_TaxFeeFlag = "2";
			dutyRate2.UD_TaxFeeComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;

			USCTariff tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "00000002";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff3.UE_Unit1 = "PFL";

			USCTariffDutyRate dutyRate3 = tariff3.DutyRates.AddNew();
			dutyRate3.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			dutyRate3.UD_TaxFeeFlag = "2";
			dutyRate3.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			dutyRate3.UD_TaxFeeSpecificRate = 0.7m;
			dutyRate3.UD_TaxFeeAdvalorem = 0.8m;
		}
	}
}
