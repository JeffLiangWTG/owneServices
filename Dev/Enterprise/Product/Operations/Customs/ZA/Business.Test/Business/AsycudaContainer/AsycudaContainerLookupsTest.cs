using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class AsycudaContainerLookupsTest : BusinessObjectValidationTestCase
	{
		public void TestEmptyFullList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			AssertEquals("MT, FCL, LCL", container.Lookups.EmptyFullList.CodesAsString);
		}

		public void TestSealTypeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			AssertEquals("AGT, CAR, CUS, EXP, TOR", container.Lookups.SealTypeList.CodesAsString);
		}

		public void TestParent()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			AssertType<AsycudaContainer>(container.Lookups.Parent);
		}
	}
}
