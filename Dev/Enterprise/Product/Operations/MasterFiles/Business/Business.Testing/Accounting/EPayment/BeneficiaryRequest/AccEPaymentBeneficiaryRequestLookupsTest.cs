using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccEPaymentBeneficiaryRequestLookupsTest : BusinessObjectLookupsTestCase
	{
		AccEPaymentBeneficiaryRequestLookups Lookups => lookups ?? (lookups = new AccEPaymentBeneficiaryRequestLookups(Factory.New<AccEPaymentBeneficiaryRequest>()));
		AccEPaymentBeneficiaryRequestLookups lookups;

		public void TestStatusCodeList()
		{
			AssertEquals(5, Lookups.StatusCodeList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "QUE", "REQ", "RCV", "ERR", "PAR" }, Lookups.StatusCodeList.GetAllCodes());
		}

		public void TestProviderCodeList()
		{
			AssertEquals(1, Lookups.ProviderCodeList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "OFX" }, Lookups.ProviderCodeList.GetAllCodes());
		}
	}
}
