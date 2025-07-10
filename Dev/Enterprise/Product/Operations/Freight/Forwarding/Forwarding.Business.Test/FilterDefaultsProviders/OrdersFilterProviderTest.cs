using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(OrdersFilterProvider))]
	sealed class OrdersFilterProviderTest : DefaultFilterProviderTest<OrdersFilterProvider>
	{
		#region TestTransportMode

		public void TestTransportMode()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, OrdersFilterProvider.FilterNames.TransportMode);

			Provider.TransportMode = Core.Constants.TransportModes.Sea;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, OrdersFilterProvider.FilterNames.TransportMode, "Property", (ZString)Core.Constants.TransportModes.Sea);
		}

		#endregion

		#region TestContainerMode

		public void TestContainerMode()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, OrdersFilterProvider.FilterNames.ContainerMode);

			Provider.ContainerMode = Core.Constants.ContainerModes.FCL;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, OrdersFilterProvider.FilterNames.ContainerMode, "Property", (ZString)Core.Constants.ContainerModes.FCL);
		}

		#endregion

		#region TestLoadPort

		public void TestLoadPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, OrdersFilterProvider.FilterNames.LoadDischarge);

			Provider.LoadPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, OrdersFilterProvider.FilterNames.LoadDischarge, "Property1", HomePort);
		}

		#endregion

		#region TestDischargePort

		public void TestDischargePort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, OrdersFilterProvider.FilterNames.LoadDischarge);

			Provider.DischargePort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, OrdersFilterProvider.FilterNames.LoadDischarge, "Property2", HomePort);
		}

		#endregion

		#region TestOriginPort

		public void TestOriginPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, OrdersFilterProvider.FilterNames.OriginDestination);

			Provider.OriginPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, OrdersFilterProvider.FilterNames.OriginDestination, "Property1", HomePort);
		}

		#endregion

		#region TestDestinationPort

		public void TestDestinationPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, OrdersFilterProvider.FilterNames.OriginDestination);

			Provider.DestinationPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, OrdersFilterProvider.FilterNames.OriginDestination, "Property2", HomePort);
		}

		#endregion

		#region TestBuyer

		public void TestBuyer()
		{
			ZGuid pk = ZGuid.NewZGuid();

			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, OrdersFilterProvider.FilterNames.BuyerSupplier);

			Provider.Buyer = pk;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, OrdersFilterProvider.FilterNames.BuyerSupplier, "Property1", pk);
		}

		#endregion

		#region TestSupplier

		public void TestSupplier()
		{
			ZGuid pk = ZGuid.NewZGuid();

			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, OrdersFilterProvider.FilterNames.BuyerSupplier);

			Provider.Supplier = pk;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, OrdersFilterProvider.FilterNames.BuyerSupplier, "Property2", pk);
		}

		#endregion

		#region TestShowUnAttatchedOrders

		public void TestShowUnAttatchedOrders()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, OrdersFilterProvider.FilterNames.AttachedUnattachedOrders);

			Provider.ShowUnAttatchedOrders = true;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, OrdersFilterProvider.FilterNames.AttachedUnattachedOrders, "Property", (ZString)"All");

			Provider.ShowUnAttatchedOrders = false;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, OrdersFilterProvider.FilterNames.AttachedUnattachedOrders, "Property", (ZString)"Attached");
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Orders; }
		}

		#endregion
	}
}
