using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class MiningInformationValidationTest : CusCodeDataValidationTest
	{
		public void TestCheckCountryOfMining()
		{
			var miningInformation1 = InvoiceLine.MiningInformations.AddNew();
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(miningInformation1.CountryOfMiningInfo, "??", "CA");
			miningInformation1.CountryOfMining = "CA";
			AssertNoError(miningInformation1.CountryOfMiningInfo, "The Data has been duplicated and must be unique.");
			var miningInformation2 = InvoiceLine.MiningInformations.AddNew();
			miningInformation2.CountryOfMining = "CA";
			AssertHasError(miningInformation2.CountryOfMiningInfo, "The Data has been duplicated and must be unique.");
		}

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>()); }
		}
		JobComInvoiceLine invoiceLine;
	}
}
