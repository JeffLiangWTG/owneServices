using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CTOOrDepotOrWarehouseCollection))]
	sealed class CTOOrDepotOrWarehouseCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionFilter()
		{
			OrgHeader cTO = GetCTO();
			OrgHeader depot = GetDepot();
			OrgHeader warehouse = GetWarehouse();
			Collection.Load();
			Assert(Collection.Contains(cTO));
			Assert(Collection.Contains(depot));
			Assert(Collection.Contains(warehouse));
		}

		OrgHeader GetCTO()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_IsAirCTO = true;
			result.OH_IsMiscFreightServices = true;
			return result;
		}

		OrgHeader GetDepot()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_IsMiscFreightServices = true;
			result.OH_IsUnpackDepot = true;
			return result;
		}

		OrgHeader GetWarehouse()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_IsWarehouseClient = true;
			return result;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new CTOOrDepotOrWarehouseCollection(Factory);
		}
	}
}
