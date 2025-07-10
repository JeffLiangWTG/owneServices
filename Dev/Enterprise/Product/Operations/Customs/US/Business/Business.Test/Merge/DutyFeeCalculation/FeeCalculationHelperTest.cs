using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public sealed class FeeCalculationHelperTest : TestCaseWithFactory
	{
		public void TestValues()
		{
			PrepareFeeAndTexData();

			var helper = new FeeCalculationHelper(Factory, new ZDateTime(2017, 11, 16));
			AssertEquals(2m, helper.InformalFeeAmount);
			AssertEquals(5m, helper.MailFee);
			AssertEquals(3m, helper.ManualSurchargeAmount);
			AssertEquals(3m, helper.HMFThresholdAmount);
			AssertEquals(0.125m, helper.HMFRatePercentage);

			helper = new FeeCalculationHelper(Factory, new ZDateTime(2018, 08, 16));
			AssertEquals(2.05m, helper.InformalFeeAmount);
			AssertEquals(5.65m, helper.MailFee);
			AssertEquals(3.08m, helper.ManualSurchargeAmount);
			AssertEquals(3m, helper.HMFThresholdAmount);
			AssertEquals(0.125m, helper.HMFRatePercentage);

			helper = new FeeCalculationHelper(Factory, new ZDateTime(2018, 12, 16));
			AssertEquals(2.10m, helper.InformalFeeAmount);
			AssertEquals(5.77m, helper.MailFee);
			AssertEquals(3.15m, helper.ManualSurchargeAmount);
			AssertEquals(3m, helper.HMFThresholdAmount);
			AssertEquals(0.125m, helper.HMFRatePercentage);
		}

		public void PrepareFeeAndTexData()
		{
			var testHelper = new UniversalReferenceTestDataHelper(Factory);

			testHelper.CreateRefCusTaxOrFeeType("FLA", "Flat");
			testHelper.CreateRefCusTaxOrFeeType("AVL", "ad valorem");

			testHelper.CreateTaxOrFee("311", 2.00, "US", 0, 0, "FLA", new ZDateTime("1995-01-01"), new ZDateTime("2017-12-31"), "Merchandise Informal Fee");
			testHelper.CreateTaxOrFee("311", 2.05, "US", 0, 0, "FLA", new ZDateTime("2018-01-01"), new ZDateTime("2018-09-30"), "Merchandise Informal Fee");
			testHelper.CreateTaxOrFee("311", 2.10, "US", 0, 0, "FLA", new ZDateTime("2018-10-01"), new ZDateTime("2079-06-06"), "Merchandise Informal Fee");

			testHelper.CreateTaxOrFee("496", 5.00, "US", 0, 0, "FLA", new ZDateTime("1995-01-01"), new ZDateTime("2017-12-31"), "Dutiable Mail Fee");
			testHelper.CreateTaxOrFee("496", 5.65, "US", 0, 0, "FLA", new ZDateTime("2018-01-01"), new ZDateTime("2018-09-30"), "Dutiable Mail Fee");
			testHelper.CreateTaxOrFee("496", 5.77, "US", 0, 0, "FLA", new ZDateTime("2018-10-01"), new ZDateTime("2079-06-06"), "Dutiable Mail Fee");

			testHelper.CreateTaxOrFee("500", 3.00, "US", 0, 0, "FLA", new ZDateTime("1995-01-01"), new ZDateTime("2017-12-31"), "Merchandise Surcharge Fee");
			testHelper.CreateTaxOrFee("500", 3.08, "US", 0, 0, "FLA", new ZDateTime("2018-01-01"), new ZDateTime("2018-09-30"), "Merchandise Surcharge Fee");
			testHelper.CreateTaxOrFee("500", 3.15, "US", 0, 0, "FLA", new ZDateTime("2018-10-01"), new ZDateTime("2079-06-06"), "Merchandise Surcharge Fee");

			testHelper.CreateTaxOrFee("501", 0.1250, "US", 3.00, 0, "AVL", new ZDateTime("1995-01-01"), new ZDateTime("2079-06-06"), "Harbor Maintenance Fee");

			testHelper.CreateTaxOrFee("499", 0.2100, "US", 25.00, 485.00, "AVL", new ZDateTime("1995-01-01"), new ZDateTime("2011-09-30"), "Merchandise Processing Fee");
			testHelper.CreateTaxOrFee("499", 0.3464, "US", 25.00, 485.00, "AVL", new ZDateTime("2011-10-01"), new ZDateTime("2017-12-31"), "Merchandise Processing Fee");
			testHelper.CreateTaxOrFee("499", 0.3464, "US", 25.67, 497.99, "AVL", new ZDateTime("2018-01-01"), new ZDateTime("2079-06-06"), "Merchandise Processing Fee");

			Factory.Save();
		}

		public void TestIsAdValoremAndIsFlat()
		{
			var helper = new FeeCalculationHelper(Factory, new ZDateTime(2017, 11, 16));
			var taxOrFeeType = Factory.NewWithValidTestData<RefCusTaxOrFeeType>();

			taxOrFeeType.ZX0_TaxOrFeeType = "AVL";
			Assert(helper.IsAdValorem(taxOrFeeType.ZX0_TaxOrFeeType));

			taxOrFeeType.ZX0_TaxOrFeeType = "FLA";
			Assert(helper.IsFlat(taxOrFeeType.ZX0_TaxOrFeeType));

			taxOrFeeType.ZX0_TaxOrFeeType = "";
			Assert(!helper.IsAdValorem(taxOrFeeType.ZX0_TaxOrFeeType));
			Assert(!helper.IsFlat(taxOrFeeType.ZX0_TaxOrFeeType));
		}
	}
}
