using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module.Testing
{
	sealed class FormControllerCommonTest : TestCaseWithFactory
	{
		public void TestSpecificControllersMakeUrlsOnlyOpenableForCurrentCompany_WhenAllowInterCompanyHyperlinks_ThenIsFalse()
		{
			using (FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions(() =>
				{
					foreach (var controllerID in ControllersShouldFollowTheRegistry)
					{
						var controller = ZControllerFactory.Create(controllerID);

						Assert($"Controller {controllerID}'s MakeUrlsOnlyOpenableForCurrentCompany should be false when allowing inter-company hyperlinks", !controller.MakeUrlsOnlyOpenableForCurrentCompany);
					}
				});
			}
		}

		public void TestSpecificControllersMakeUrlsOnlyOpenableForCurrentCompany_WhenNotAllowInterCompanyHyperlinks_ThenIsTrue()
		{
			using (FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				CombineAssertions(() =>
				{
					foreach (var controllerID in ControllersShouldFollowTheRegistry)
					{
						var controller = ZControllerFactory.Create(controllerID);

						Assert($"Controller {controllerID}'s MakeUrlsOnlyOpenableForCurrentCompany should be true when not allowing inter-company hyperlinks", controller.MakeUrlsOnlyOpenableForCurrentCompany);
					}
				});
			}
		}

		ControllerID[] ControllersShouldFollowTheRegistry => new ControllerID[] {
			ControllerIDs.AgencyBillContainers,
			ControllerIDs.AgencyBillOfLading,
			ControllerIDs.AgencyBooking,
			ControllerIDs.AgencyContainerDetention,
			ControllerIDs.AgencyContainerManager,
			ControllerIDs.AgencyContainerMove,
			ControllerIDs.AgencySundryCharges,
			ControllerIDs.AgencyVoyageAccounting,
			ControllerIDs.Cartage,
			ControllerIDs.CartageLeg,
			ControllerIDs.CartageLegPlanner,
			ControllerIDs.CartageRunSheetDashboard,
			ControllerIDs.CartageType,
			ControllerIDs.CartageWorkSheet,
			ControllerIDs.ConsolidatedTransportBooking,
			ControllerIDs.ConsolPlanningBoard,
			ControllerIDs.ContainerLoadList,
			ControllerIDs.Containers,
			ControllerIDs.DocumentTracking,
			ControllerIDs.GatewayConsolProfitShareRedistribution,
			ControllerIDs.JobAirSailing,
			ControllerIDs.JobConsol,
			ControllerIDs.JobMawb,
			ControllerIDs.JobRailSailing,
			ControllerIDs.JobRoadSailing,
			ControllerIDs.JobSeaSailing,
			ControllerIDs.JobSeaVoyage,
			ControllerIDs.JobShipment,
			ControllerIDs.JobShipmentPreplanning,
			ControllerIDs.LoadList,
			ControllerIDs.OneOffQuotes,
			ControllerIDs.OnlineSailingSchedules,
			ControllerIDs.OrderLine,
			ControllerIDs.OrderLineFromOrder,
			ControllerIDs.Orders,
			ControllerIDs.PackContainers,
			ControllerIDs.PickupDeliveryConfirm,
			ControllerIDs.PortHubSelection,
			ControllerIDs.PreAllocations,
			ControllerIDs.QuickPOD,
			ControllerIDs.QuotedBookings,
			ControllerIDs.RoutingLookups,
			ControllerIDs.SailingScheduleImporting,
			ControllerIDs.SupplierBooking,
			ControllerIDs.TradeLane,
		};
	}
}
