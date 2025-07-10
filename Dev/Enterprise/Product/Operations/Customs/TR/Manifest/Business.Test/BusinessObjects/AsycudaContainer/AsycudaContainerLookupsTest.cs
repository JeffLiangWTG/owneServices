using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class AsycudaContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRelationList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			var list = container.Lookups.RelationList;
			AssertEquals(true, list.ContainsCode("Local"));
			AssertEquals(true, list.ContainsCode("Foreign"));
		}
	}
}
