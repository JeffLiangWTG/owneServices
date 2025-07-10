using CargoWise.Types;
using CharterFilterCodes = Enterprise.Freight.Business.FreightConstants.CharterFilter;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class SailingScheduleDefaultFilterProviderTest : DefaultFilterProviderTest<SailingScheduleDefaultFilterProvider>
	{
		const string Charter = "Charter";
		const string LoadDischarge = "Load / Discharge";
		const string ETD = "Load Port ETD";
		const string ETA = "Load Port ETA";

		public void TestIsCharter()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, Charter);

			Provider.IsChartered = true;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, Charter, "Property", (ZString)CharterFilterCodes.CharterOnlyCode);

			Provider.IsChartered = false;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, Charter, "Property", (ZString)CharterFilterCodes.NonCharterOnlyCode);
		}

		public void TestLoadPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, LoadDischarge);

			Provider.LoadPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, LoadDischarge, "Property1", HomePort);
		}

		public void TestDischargePort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, LoadDischarge);

			Provider.DischargePort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, LoadDischarge, "Property2", HomePort);
		}

		public void TestETDFrom()
		{
			ZDateTime today = ZDateTime.Today;

			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ETD);

			Provider.ETDFrom = today.AddHours(8);
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ETD, "PropertySearch", (ZString)"Date range");
			AssertHasDefault("Should only keep the date part", Collection, ETD, "Property1", today);
		}

		public void TestETDTo()
		{
			ZDateTime today = ZDateTime.Today;

			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ETD);

			Provider.ETDTo = today.AddHours(8);
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ETD, "PropertySearch", (ZString)"Date range");
			AssertHasDefault("Should only keep the date part", Collection, ETD, "Property2", today);
		}

		public void TestETAFrom()
		{
			ZDateTime today = ZDateTime.Today;

			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ETA);

			Provider.ETAFrom = today.AddHours(8);
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ETA, "PropertySearch", (ZString)"Date range");
			AssertHasDefault("Should only keep the date part", Collection, ETA, "Property1", today);
		}

		public void TestETATo()
		{
			ZDateTime today = ZDateTime.Today;

			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ETA);

			Provider.ETATo = today.AddHours(8);
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ETA, "PropertySearch", (ZString)"Date range");
			AssertHasDefault("Should only keep the date part", Collection, ETA, "Property2", today);
		}
	}
}
