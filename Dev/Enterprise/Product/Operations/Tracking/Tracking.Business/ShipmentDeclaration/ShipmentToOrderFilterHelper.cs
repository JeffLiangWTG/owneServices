using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.FilterStrips;

namespace Enterprise.Tracking.Business
{
	public class ShipmentToOrderFilterMapHelper : FilterStripBOMappingHelper
	{
		public ShipmentToOrderFilterMapHelper(TrackingShipmentFilterBusinessObject shipmentFilterBO, OrdersFilterBusinessObject orderFilterBO)
			: base(shipmentFilterBO, orderFilterBO)
		{
		}

		public void MapFilters()
		{
			MapUnattachedFilter();

			if (sourceFilterBO != null)
			{
				MapModuleGuidsFiltersSwapped(FreightDataRegistry.Instance.ConsignorShipperTerminology.Value + (NoResString)" / Consignee", (NoResString)"Buyer / Supplier");

				MapModuleFilters((NoResString)"Booking Reference #", (NoResString)"Booking Conf. Ref. #");
				MapModuleFilters((NoResString)"Commercial Invoice #", (NoResString)"Invoice #");
				MapModuleFilters((NoResString)"Flight/Voyage # and Vessel");
				MapModuleFilters((NoResString)"House Bill");
				MapModuleFilters((NoResString)"Master Bill");
				MapModuleFilters((NoResString)"Order #");
				MapModuleFilters((NoResString)"Shipment #");
				MapModuleFilters("ETA", (NoResString)"Estimated Arrival");
				MapModuleFilters("ETD", (NoResString)"Estimated Departure");
				MapModuleFilters((NoResString)"Load / Discharge");
				MapModuleFilters((NoResString)"Origin / Destination");
				MapModuleFilters((NoResString)"Consol Send / Receive Agents", (NoResString)"Send / Receive Agents");
				MapModuleFilters((NoResString)"Consignee Company Name", (NoResString)"Buyer Company Name");
				MapModuleFilters(FreightDataRegistry.Instance.ConsignorShipperTerminology.Value + (NoResString)" Company Name", (NoResString)"Supplier Company Name");
				MapModuleFilters((NoResString)"Container Mode");
				MapModuleFilters((NoResString)"Transport Mode");
				MapModuleFilters((NoResString)"Created Time");
				MapModuleFilters((NoResString)"Last Completed Milestone");
				MapModuleFilters((NoResString)"Milestone Completed");
				MapModuleFilters((NoResString)"Milestone Date");
				MapModuleFilters((NoResString)"Next Milestone");
				MapModuleFilters((NoResString)"Service Level");
			}
		}

		#region MapUnattachedFilter

		void MapUnattachedFilter()
		{
			ModuleTextFilter unattachedFilter = destinationFilterBO["Attached / Unattached Orders"] as ModuleTextFilter;
			if (unattachedFilter != null)
			{
				unattachedFilter.Property = OrdersConstants.OrdersAttachedState.UnattachedOnly;
				unattachedFilter.IsActive = true;
			}
		}

		#endregion
	}
}
