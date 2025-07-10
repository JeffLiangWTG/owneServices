using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	sealed class ConsolContractAndRouteAllocationFormFilterDefaultsTest : TestCaseWithFactory
	{
		public void TestFilterDefaultingFromConsolidation()
		{
			var today = ZDateTime.Today;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = "CONTRACT";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RH_NKConsolCommodity = "HAZ";

			var transport = consol.Transports[0];
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_ETD = today;

			var shippingLine = Factory.New<OrgHeader>();
			var shippingLineAddress = shippingLine.Addresses.AddNew();
			consol.JK_OA_ShippingLineAddress = shippingLineAddress.PK;

			var filterDefaults = (new ConsolContractAndRouteAllocationFilterDefaults(consol) as IRatingContractSimulationFilterDefaults).GetFilterDefaultsForContract();
			AssertEquals(true, filterDefaults.ContainsDefaultFor($"{CarrierContractFilterConstants.ContractNumber}:Property"));
			AssertEquals("CONTRACT", filterDefaults[$"{CarrierContractFilterConstants.ContractNumber}:Property"].Value);

			AssertEquals(true, filterDefaults.ContainsDefaultFor($"{CarrierContractFilterConstants.TransportMode}:Property"));
			AssertEquals(Core.Constants.TransportModes.Sea, filterDefaults[$"{CarrierContractFilterConstants.TransportMode}:Property"].Value);

			AssertEquals(true, filterDefaults.ContainsDefaultFor($"{CarrierContractFilterConstants.StartDate}:Property2"));
			AssertEquals(today, filterDefaults[$"{CarrierContractFilterConstants.StartDate}:Property2"].Value);

			AssertEquals(true, filterDefaults.ContainsDefaultFor($"{CarrierContractFilterConstants.ServiceProvider}:Property"));
			AssertEquals(shippingLine.PK, filterDefaults[$"{CarrierContractFilterConstants.ServiceProvider}:Property"].Value);

			AssertEquals(true, filterDefaults.ContainsDefaultFor($"{CarrierContractFilterConstants.AllowHazardousCommodities}:Property2"));
			AssertEquals(true, filterDefaults[$"{CarrierContractFilterConstants.AllowHazardousCommodities}:Property2"].Value);
		}

		public void TestFilterDefaultingFromConsolidation_MainLeg()
		{
			var today = ZDateTime.Today;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = "CONTRACT";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var otherTransport = consol.Transports[0];
			otherTransport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			otherTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			otherTransport.JW_ETD = today;
			otherTransport.JW_Vessel = "WAH";
			otherTransport.JW_VoyageFlight = "DF2";
			otherTransport.JW_ServiceString = "QCF2";

			var mainTransport = consol.Transports.AddNew();
			mainTransport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			mainTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			mainTransport.JW_ETD = today.AddDays(5);
			mainTransport.JW_Vessel = "CD2";
			mainTransport.JW_VoyageFlight = "DORYA";
			mainTransport.JW_ServiceString = "SORYA";
			mainTransport.JW_RL_NKLoadPort = "AUSYD";
			mainTransport.JW_RL_NKDiscPort = "NZAKL";

			var filterDefaults = (new ConsolContractAndRouteAllocationFilterDefaults(consol) as IRatingContractSimulationFilterDefaults).GetFilterDefaultsForContract();

			AssertEquals(today.AddDays(5), filterDefaults[$"{CarrierContractFilterConstants.StartDate}:Property2"].Value);

			AssertEquals(today.AddDays(5), filterDefaults[$"{CarrierContractFilterConstants.ExpiryDate}:Property1:2"].Value);

			var routeChildDefaults = filterDefaults[$"{CarrierContractFilterConstants.AllocationRoutes}:Property:2"].ChildDefaults;

			AssertEquals(today.AddDays(5), routeChildDefaults[$"{AllocationRouteFilterConstants.StartDate}:Property2:2"].Value);
			AssertEquals(today.AddDays(5), routeChildDefaults[$"{AllocationRouteFilterConstants.ExpiryDate}:Property1:2"].Value);
			AssertEquals("DORYA", routeChildDefaults[$"{AllocationRouteFilterConstants.VoyageNumber}:Property:2"].Value);
			AssertEquals("CD2", routeChildDefaults[$"{AllocationRouteFilterConstants.Vessel}:Property:2"].Value);
			AssertEquals("SORYA", routeChildDefaults[$"{AllocationRouteFilterConstants.ServiceString}:Property:2"].Value);
			AssertEquals("AUSYD", routeChildDefaults[$"{AllocationRouteFilterConstants.LoadDischargePort}:Property1"].Value);
			AssertEquals("NZAKL", routeChildDefaults[$"{AllocationRouteFilterConstants.LoadDischargePort}:Property2"].Value);
		}

		public void TestFilterDefaulting_MainLegFallbackToConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = "CONTRACT";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "NZWEL";

			var filterDefaults = (new ConsolContractAndRouteAllocationFilterDefaults(consol) as IRatingContractSimulationFilterDefaults).GetFilterDefaultsForContract();
			var routeChildDefaults = filterDefaults[$"{CarrierContractFilterConstants.AllocationRoutes}:Property:2"].ChildDefaults;

			AssertEquals("AUBNE", routeChildDefaults[$"{AllocationRouteFilterConstants.LoadDischargePort}:Property1"].Value);
			AssertEquals("NZWEL", routeChildDefaults[$"{AllocationRouteFilterConstants.LoadDischargePort}:Property2"].Value);
		}

		public void TestFilterDefaultingFromConsolidation_AllocationRouteDefaultsHaveDifferentCategories()
		{
			var today = ZDateTime.Today;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = "CONTRACT";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.Transports[0].JW_Vessel = "PATRICKBATEMAN";
			consol.Transports[0].JW_VoyageFlight = "PAULALLEN";
			consol.Transports[0].JW_ETD = today;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "DORSIA";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			consol.Shipments.Add(shipment);

			var filterDefaults = (new ConsolContractAndRouteAllocationFilterDefaults(consol) as IRatingContractSimulationFilterDefaults).GetFilterDefaultsForContract();

			AssertEquals(true, filterDefaults.ContainsDefaultFor($"{CarrierContractFilterConstants.AllocationRoutes}:Property:2"));

			var routeChildDefaults = filterDefaults[$"{CarrierContractFilterConstants.AllocationRoutes}:Property:2"].ChildDefaults;

			AssertEquals(true, routeChildDefaults.ContainsDefaultFor($"{AllocationRouteFilterConstants.StartDate}:Property2:2"));
			AssertEquals(FilterOrCategory.Green, routeChildDefaults[$"{AllocationRouteFilterConstants.StartDate}:Property2:2"].Category);

			AssertEquals(true, routeChildDefaults.ContainsDefaultFor($"{AllocationRouteFilterConstants.ExpiryDate}:Property1:2"));
			AssertEquals(FilterOrCategory.Brown, routeChildDefaults[$"{AllocationRouteFilterConstants.ExpiryDate}:Property1:2"].Category);

			AssertEquals(true, routeChildDefaults.ContainsDefaultFor($"{AllocationRouteFilterConstants.VoyageNumber}:Property:2"));
			AssertEquals(FilterOrCategory.Red, routeChildDefaults[$"{AllocationRouteFilterConstants.VoyageNumber}:Property:2"].Category);

			AssertEquals(true, routeChildDefaults.ContainsDefaultFor($"{AllocationRouteFilterConstants.Vessel}:Property:2"));
			AssertEquals(FilterOrCategory.Blue, routeChildDefaults[$"{AllocationRouteFilterConstants.Vessel}:Property:2"].Category);
		}

		public void TestFilterDefaultingFromContainer_PopulatesAllocationIDFilter()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			container.JC_RCA_AllocationLine = allocationRoute.PK;

			var filterDefaults = (new ConsolContractAndRouteAllocationFilterDefaults(container) as IRatingContractSimulationFilterDefaults).GetFilterDefaultsForContract();
			var routeChildDefaults = filterDefaults[$"{CarrierContractFilterConstants.AllocationRoutes}:Property:2"].ChildDefaults;

			AssertEquals(true, routeChildDefaults.ContainsDefaultFor($"{AllocationRouteFilterConstants.AllocationRouteID}:Property"));
			AssertEquals(allocationRoute.RCA_AllocationLineID, routeChildDefaults[$"{AllocationRouteFilterConstants.AllocationRouteID}:Property"].Value);
		}

		public void TestFilterDefaultingFromContainer_NotPopulateAllocationIDFilter_WhenEmpty()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = consol.Containers.AddNew();

			var filterDefaults = (new ConsolContractAndRouteAllocationFilterDefaults(container) as IRatingContractSimulationFilterDefaults).GetFilterDefaultsForContract();
			var routeChildDefaults = filterDefaults[$"{CarrierContractFilterConstants.AllocationRoutes}:Property:2"].ChildDefaults;

			AssertEquals(false, routeChildDefaults.ContainsDefaultFor($"{AllocationRouteFilterConstants.AllocationRouteID}:Property"));
		}
	}
}
