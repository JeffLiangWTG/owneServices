using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.eTail.Module.Testing
{
	[TestedType(typeof(HVLVOriginLoadListFilterProvider))]
	public class HVLVOriginLoadListFilterProviderTest : DefaultFilterProviderTest<HVLVOriginLoadListFilterProvider>
	{
		public void TestMasterBill()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, MasterBill);

			var masterBill = new ZString("MBN20181220");
			Provider.MasterBill = masterBill;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, MasterBill, "Property", masterBill);
		}

		public void TestFlightVoyageVessel()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, FlightVoyageVessel);

			var voyage = new ZString("VO01");
			Provider.Voyage = voyage;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, FlightVoyageVessel, "VoyageFlightNo", voyage);

			var vessel = new ZString("VE01");
			Provider.Vessel = vessel;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, FlightVoyageVessel, "Vessel", vessel);
		}

		public void TestETDFrom()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ETD);

			var today = ZDateTime.Today;
			Provider.ETDFrom = today.AddHours(8);
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ETD, "PropertySearch", (ZString)"Date range");
			AssertHasDefault("Should only keep the date part", Collection, ETD, "Property1", today);
		}

		public void TestETDTo()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ETD);

			var today = ZDateTime.Today;
			Provider.ETDTo = today.AddHours(8);
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ETD, "PropertySearch", (ZString)"Date range");
			AssertHasDefault("Should only keep the date part", Collection, ETD, "Property2", today);
		}

		public void TestETAFrom()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ETA);

			var today = ZDateTime.Today;
			Provider.ETAFrom = today.AddHours(8);
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ETA, "PropertySearch", (ZString)"Date range");
			AssertHasDefault("Should only keep the date part", Collection, ETA, "Property1", today);
		}

		protected override ModuleIdentifier ModuleID => ModuleIDs.HVLVOriginLoadList;

		const string MasterBill = "Master Bill #";              // Filter related.
		const string FlightVoyageVessel = "Voyage / Vessel";    // Filter related.
		const string ETD = "ETD";
		const string ETA = "ETA";
	}
}
