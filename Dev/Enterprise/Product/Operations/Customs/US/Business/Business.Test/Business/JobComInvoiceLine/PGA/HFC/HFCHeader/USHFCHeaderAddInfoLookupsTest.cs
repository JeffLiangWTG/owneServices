using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USHFCHeaderAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestHFCCertifyingIndividualList()
		{
			AssertEquals(3, lookups.HFCCertifyingIndividualList.Count);
			AssertCollectionContains(EntityRoleCodeList.Codes.Importer, lookups.HFCCertifyingIndividualList.GetAllCodes());
			AssertCollectionContains(EntityRoleCodeList.Codes.Consignee, lookups.HFCCertifyingIndividualList.GetAllCodes());
			AssertCollectionContains(EntityRoleCodeList.Codes.CustomsBroker, lookups.HFCCertifyingIndividualList.GetAllCodes());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var header = invoiceLine.USHFCHeaders.AddNew();
			lookups = header.AddInfoLookups;
		}
		USHFCHeaderAddInfoLookups lookups;
	}
}
