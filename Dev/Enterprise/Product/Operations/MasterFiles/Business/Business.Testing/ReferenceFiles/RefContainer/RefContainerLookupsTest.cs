using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestContainerTypes()
		{
			var lookups = container.Lookups;
			AssertEquals(11, lookups.ContainerTypes.Count);
			Assert(lookups.ContainerTypes.ContainsCode(Constants.ContainerTypes.Refrigerated));
			Assert(lookups.ContainerTypes.ContainsCode(Constants.ContainerTypes.DryStorage));
			Assert(lookups.ContainerTypes.ContainsCode(Constants.ContainerTypes.Other));
			Assert(lookups.ContainerTypes.ContainsCode(Constants.ContainerTypes.AircraftPallet));
			Assert(lookups.ContainerTypes.ContainsCode(Constants.ContainerTypes.AircraftContainer));
			Assert(lookups.ContainerTypes.ContainsCode(Constants.ContainerTypes.AutomobileTransportEquipment));
			Assert(lookups.ContainerTypes.ContainsCode(Constants.ContainerTypes.AircraftEngineTransportEquipment));
			Assert(lookups.ContainerTypes.ContainsCode(Constants.ContainerTypes.FireResistantContainer));
			Assert(lookups.ContainerTypes.ContainsCode(Constants.ContainerTypes.CattleStalls));
			Assert(lookups.ContainerTypes.ContainsCode(Constants.ContainerTypes.HorseStalls));
			Assert(lookups.ContainerTypes.ContainsCode(Constants.ContainerTypes.ThermalAircraftContainer));
		}

		public void TestStorageTypes_Air()
		{
			var lookups = container.Lookups;
			AssertEquals(true, lookups.StorageClassList.ContainsCode("MD"));
			AssertEquals(true, lookups.StorageClassList.ContainsCode("LD"));
		}

		public void TestISOTypesIsCollection()
		{
			var container = Factory.New<RefContainer>();
			container.RC_ShippingMode = RefContainerLookups.ShippingModes.Air;
			Assert(container.Lookups.ISOTypes is RefContainerISOTypesCollection);
		}

		RefContainer container;

		protected override void SetUp()
		{
			container = Factory.New<RefContainer>();
			container.RC_ShippingMode = RefContainerLookups.ShippingModes.Air;

			base.SetUp();
		}
	}
}
