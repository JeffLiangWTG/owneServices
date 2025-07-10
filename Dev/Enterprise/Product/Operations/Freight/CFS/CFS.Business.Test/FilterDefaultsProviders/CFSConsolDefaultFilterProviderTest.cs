using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSConsolDefaultFilterProvider))]
	public class CFSConsolDefaultFilterProviderTest : DefaultFilterProviderTest<CFSConsolDefaultFilterProvider>
	{
		const string TransportContainerMode = "Transport / Container Modes";
		const string LoadDischarge = "Load / Discharge";
		const string OriginDestination = "Origin / Destination";
		const string ETD = "ETD";
		const string ETA = "ETA";
		const string Client = "Client";
		const string VoyageVessel = "Voyage / Flight / Vessel";

		#region TestTransportMode

		public void TestTransportMode()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, TransportContainerMode);

			Provider.TransportMode = Core.Constants.TransportModes.Sea;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, TransportContainerMode, "Property1", (ZString)Core.Constants.TransportModes.Sea);
		}

		#endregion

		#region TestContainerMode

		public void TestContainerMode()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, TransportContainerMode);

			Provider.ContainerMode = Core.Constants.ContainerModes.FCL;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, TransportContainerMode, "Property2", (ZString)Core.Constants.ContainerModes.FCL);
		}

		#endregion

		#region TestLoadPort

		public void TestLoadPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, LoadDischarge);

			Provider.LoadPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, LoadDischarge, "Property1", HomePort);
		}

		#endregion

		#region TestDischargePort

		public void TestDischargePort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, LoadDischarge);

			Provider.DischargePort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, LoadDischarge, "Property2", HomePort);
		}

		#endregion

		#region TestOriginPort

		public void TestOriginPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, OriginDestination);

			Provider.OriginPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, OriginDestination, "Property1", HomePort);
		}

		#endregion

		#region TestDestinationPort

		public void TestDestinationPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, OriginDestination);

			Provider.DestinationPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, OriginDestination, "Property2", HomePort);
		}

		#endregion

		#region TestETDFrom

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

		#endregion

		#region TestETDTo

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

		#endregion

		#region TestETAFrom

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

		#endregion

		#region TestETATo

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

		#endregion

		#region TestClient

		public void TestClient()
		{
			ZGuid pk = ZGuid.NewZGuid();

			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, Client);

			Provider.Client = pk;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, Client, "Property", pk);
		}

		#endregion

		#region TestVoyage

		public void TestVoyage()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, VoyageVessel);

			Provider.Voyage = TestVoyage1.JV_VoyageFlight;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, VoyageVessel, "VoyageFlightNo", TestVoyage1.JV_VoyageFlight);
		}

		#endregion

		#region TestVessel

		public void TestVessel()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, VoyageVessel);

			Provider.Vessel = TestVessel1.RV_Name;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, VoyageVessel, "Vessel", TestVessel1.RV_Name);
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.LoadListConsol; }
		}

		#endregion
	}
}
