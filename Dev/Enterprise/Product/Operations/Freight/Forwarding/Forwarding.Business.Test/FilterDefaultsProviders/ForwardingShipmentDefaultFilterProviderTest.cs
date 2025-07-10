using System.Collections.Generic;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentDefaultFilterProvider))]
	sealed class ForwardingShipmentDefaultFilterProviderTest : DefaultFilterProviderTest<ForwardingShipmentDefaultFilterProvider>
	{
		const string TransportMode = "Transport Mode";
		const string ContainerMode = "Container Mode";
		const string LoadDischarge = "Load / Discharge";
		const string OriginDestination = "Origin / Destination";
		const string ETD = "ETD";
		const string ETA = "ETA";
		const string ConsolNo = "Consol #";

		public void TestShipmentType()
		{
			Provider.ShipmentType = ForwardingShipmentDefaultFilterProvider.ShipmentTypes.All;
			AssertNoDefaults(Collection, TransportMode);

			Provider.ShipmentType &= ~ForwardingShipmentDefaultFilterProvider.ShipmentTypes.Standard;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, "Shipment Type", "Property0", (ZBool)true);
			AssertHasDefault(Collection, "Shipment Type", "Property1", (ZBool)true);
			AssertHasDefault(Collection, "Shipment Type", "Property2", (ZBool)true);
			AssertHasDefault(Collection, "Shipment Type", "Property3", (ZBool)true);
			AssertHasDefault(Collection, "Shipment Type", "Property4", (ZBool)false);
			AssertHasDefault(Collection, "Shipment Type", "Property5", (ZBool)true);
			AssertHasDefault(Collection, "Shipment Type", "Property6", (ZBool)true);

			Provider.ShipmentType = ForwardingShipmentDefaultFilterProvider.ShipmentTypes.Standard;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, "Shipment Type", "Property0", (ZBool)false);
			AssertHasDefault(Collection, "Shipment Type", "Property1", (ZBool)false);
			AssertHasDefault(Collection, "Shipment Type", "Property2", (ZBool)false);
			AssertHasDefault(Collection, "Shipment Type", "Property3", (ZBool)false);
			AssertHasDefault(Collection, "Shipment Type", "Property4", (ZBool)true);
			AssertHasDefault(Collection, "Shipment Type", "Property5", (ZBool)false);
			AssertHasDefault(Collection, "Shipment Type", "Property6", (ZBool)false);
		}

		public void TestConsolNo()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ConsolNo);

			Provider.ConsolNo = "C00001001";
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ConsolNo, "Property", (ZString)"C00001001");
		}

		#region TestTransportMode

		public void TestTransportMode()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, TransportMode);

			Provider.TransportMode = Core.Constants.TransportModes.Sea;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, TransportMode, "Property", (ZString)Core.Constants.TransportModes.Sea);
		}

		#endregion

		#region TestContainerMode

		public void TestContainerMode()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ContainerMode);

			Provider.ContainerMode = Core.Constants.ContainerModes.FCL;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ContainerMode, "Property", (ZString)Core.Constants.ContainerModes.FCL);
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

		#region Implementation

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.JobShipment; }
		}

		// these two filter categories contain filters with the same Name = "Any Text Attribute"
		// so - we exclude them from Resource Mocking - otherwise these two filters would be attempted
		// to be placed into the same mocked FilterCategory which would cause exception
		protected override List<string> ExcludedResourcesForResourceStringFreeTest
		{
			get
			{
				List<string> result = base.ExcludedResourcesForResourceStringFreeTest;
				result.Add("a483fa40-040b-4396-9477-6b7b0f6d3509");
				result.Add("a2c13512-49c8-47ee-91d0-58bedba8b4ad");
				result.Add("FilterCategory.Order Manager Attribute Search");
				result.Add("FilterCategory.Commercial Invoice Attribute Search");
				return result;
			}
		}

		protected override void Populate(ForwardingShipmentDefaultFilterProvider provider, PropertyInfo info)
		{
			if (info.PropertyType == typeof(ForwardingShipmentDefaultFilterProvider.ShipmentTypes))
			{
				Populate(provider, info, ForwardingShipmentDefaultFilterProvider.ShipmentTypes.Standard);
			}
			else
			{
				base.Populate(provider, info);
			}
		}

		#endregion
	}
}
