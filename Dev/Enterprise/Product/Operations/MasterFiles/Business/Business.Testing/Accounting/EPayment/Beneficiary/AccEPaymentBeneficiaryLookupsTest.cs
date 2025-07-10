using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccEPaymentBeneficiaryLookupsTest : BusinessObjectLookupsTestCase
	{
		AccEPaymentBeneficiaryLookups Lookups => lookups ?? (lookups = new AccEPaymentBeneficiaryLookups(Factory.New<AccEPaymentBeneficiary>()));
		AccEPaymentBeneficiaryLookups lookups;

		public void TestProviderCodeList()
		{
			AssertEquals(1, Lookups.ProviderCodeList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "OFX" }, Lookups.ProviderCodeList.GetAllCodes());
		}
	}
}
