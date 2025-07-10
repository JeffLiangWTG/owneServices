using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsBondedWarehouseAttributeLookupsTest : WhsBusinessObjectLookupsTestCase
	{
		#region TestUQList

		public void TestUQList()
		{
			Assert(Lookup.UQList.Count > 0);
		}

		#endregion

		#region TestCountryList

		public void TestCountryList()
		{
			Assert(Lookup.CountryList.Count > 0);
		}

		#endregion

		#region TestManufacturers

		public void TestManufacturers()
		{
			AssertEquals(typeof(OrganisationsFindBoxCollection), Lookup.Manufacturers.GetType());
		}

		#endregion

		#region TestOutwardTypes

		public void TestOutwardTypes()
		{
			AssertEquals(typeof(WhsBondedWarehouseAttributeOutwardType), Lookup.OutwardTypes.GetType());
		}

		#endregion

		#region TestZoneStatusList

		public void TestZoneStatusList()
		{
			AssertEquals(typeof(CodeDescriptionPairList), Lookup.ZoneStatusList.GetType());
		}

		public void TestZoneStatusList_USFTZ()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS("WHS");
			var receive = Helper.CreateWhsReceive(data.Org1, whs);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			AssertEquals(typeof(ZoneStatusList), receive.Lines[0].CustomsData.Lookups.ZoneStatusList.GetType());
		}

		#endregion

		#region Implementation

		protected WhsBondedWarehouseAttributeLookups Lookup
		{
			get { return lookup ?? (lookup = new WhsBondedWarehouseAttributeLookups(Factory.New<WhsBondedWarehouseAttribute>())); }
		}

		WhsBondedWarehouseAttributeLookups lookup;

		#endregion
	}
}
