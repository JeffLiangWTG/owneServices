using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;

namespace Enterprise.Warehouse.Environment.Testing
{
	class WhsCartonGroupLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestCartonSize

		public void TestCartonSize()
		{
			AssertType<WhsCartonSizeCollection>(Factory.New<WhsCartonGroup>().Lookups.CartonSizes);
		}

		#endregion

		#region TestOrganisations

		public void TestOrganisations()
		{
			AssertType<ParentOrgLookupCollection>(Factory.New<WhsCartonGroup>().Lookups.Organisations);
		}

		#endregion

		#region TestOptimizationModes

		public void TestOptimizationModes()
		{
			AssertType<CartonizationOptimizationModes>(Factory.New<WhsCartonGroup>().Lookups.OptimizationModes);
		}

		#endregion
	}
}
