using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbSecurityLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGroupLists()
		{
			GlbStaff staff = Factory.New<GlbStaff>();

			GlbSecurity security = Factory.New<GlbSecurity>();
			Assert("Staff list", security.Lookups.StaffMembers.Count > 0);
		}
	}
}
