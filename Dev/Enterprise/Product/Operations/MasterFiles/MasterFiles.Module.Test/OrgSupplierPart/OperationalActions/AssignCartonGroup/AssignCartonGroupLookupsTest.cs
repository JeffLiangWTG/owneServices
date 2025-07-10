using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AssignCartonGroupLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestCartonGroups

		public void TestCartonGroups()
		{
			AssertType(ObjectFactory.GetType<IWhsCartonGroupCollection>(), Factory.New<OrgMiscServ>().Lookups.CartonGroups);
		}

		#endregion

		#region TestOrganisations

		public void TestOrganisations()
		{
			AssertType<OrgHeaderCollection>(GetNewLookups().Organisations);
		}

		#endregion

		#region Implementation

		AssignCartonGroupLookups GetNewLookups()
		{
			return new AssignCartonGroupLookups(new AssignCartonGroupMethodApplicator("test", Factory));
		}

		#endregion
	}
}
