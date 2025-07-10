using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCTariffDutyRate))]
	sealed class USCTariffDutyRateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDeletedByRemoveOnFactorySaving()
		{
			USCTariffDutyRate rate = Factory.New<USCTariffDutyRate>();
			rate.RemoveOnFactorySaving = true;
			AssertEquals(false, rate.IsDeleted);
			Factory.Save();
			AssertEquals(true, rate.IsDeleted);
		}

		public void TestIsSpecificSpecificTaxFee()
		{
			USCTariffDutyRate rate = Factory.New<USCTariffDutyRate>();
			rate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			AssertEquals("IsSpecificSpecificTaxFee", false, rate.IsSpecificSpecificTaxFee);
			rate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			AssertEquals("IsSpecificSpecificTaxFee", true, rate.IsSpecificSpecificTaxFee);
		}

		public void TestIsFeeApplicable()
		{
			USCTariffDutyRate rate = Factory.New<USCTariffDutyRate>();
			rate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Potato;
			AssertEquals(false, rate.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Potato));

			rate.UD_TaxFeeFlag = "1";
			AssertEquals(true, rate.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Potato));
			Assert(rate.CheckFeeApplicability(() => rate.IsFeeRequired, Core.Constants.USCustoms.FeeCodes.Potato));
			Assert(!rate.CheckFeeApplicability(() => rate.IsFeeRequired, Core.Constants.USCustoms.FeeCodes.Beef));

			rate.UD_TaxFeeFlag = "";
			AssertEquals(false, rate.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Potato));
			Assert(!rate.CheckFeeApplicability(() => rate.IsFeeRequired, Core.Constants.USCustoms.FeeCodes.Potato));
			Assert(!rate.CheckFeeApplicability(() => rate.IsFeeRequired, Core.Constants.USCustoms.FeeCodes.Beef));

			rate.UD_TaxFeeFlag = "2";
			AssertEquals(true, rate.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Potato));
		}

		public void TestGetRequiredFeeCode()
		{
			USCTariffDutyRate rate = Factory.New<USCTariffDutyRate>();
			rate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Potato;
			AssertEquals(ZString.Empty, rate.GetRequiredFeeCode());

			rate.UD_TaxFeeFlag = "1";
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Potato, rate.GetRequiredFeeCode());

			rate.UD_TaxFeeFlag = "";
			AssertEquals(ZString.Empty, rate.GetRequiredFeeCode());

			rate.UD_TaxFeeFlag = "2";
			AssertEquals(ZString.Empty, rate.GetRequiredFeeCode());
		}

		public void TestImportTariff()
		{
			USCTariff tariff = Factory.New<USCTariff>();

			USCTariffDutyRate dutyRate = Factory.New<USCTariffDutyRate>();
			AssertNull(dutyRate.ImportTariff);

			dutyRate.UD_UE = tariff.PK;
			AssertEquals(tariff, dutyRate.ImportTariff);
		}
	}
}
