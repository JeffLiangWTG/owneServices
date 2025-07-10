using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class OrgSupplierPartLookupsTest : TestCaseWithFactory
	{
		public void TestSG4Classifications()
		{
			OrgSupplierPart parent = Factory.New<OrgSupplierPart>();
			OrgSupplierPartLookups lookup = new OrgSupplierPartLookups(parent);
			AssertNotNull(lookup.SG4Classifications);
		}
	}
}
