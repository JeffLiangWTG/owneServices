using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class AttachBookingsModuleButtonGridDecisionProvider : ModuleDecisionProvider
	{
		public AttachBookingsModuleButtonGridDecisionProvider(BusinessObjectFactory factory, IFindBox findBox, ForwardingConsol consol)
			: base(findBox, null)
		{
			Argument.NotNull(consol, "consol");

			this.factory = factory;
			this.consol = consol;
		}

		public override bool AllowExcelExport
		{
			get { return false; }
		}

		public override bool EnablePreviousNextSupport
		{
			get { return false; }
		}

		public override bool ShouldDisplayNotifications
		{
			get { return true; }
		}

		public override bool ShouldLoadFilterBizObj
		{
			get { return false; }
		}

		public override bool ShouldIgnoreAdditionalFilter
		{
			get { return true; }
		}

		public override IBusinessObjectCollection List
		{
			get
			{
				if (collection == null)
				{
					var query = new ZQuery();
					query.AddToFilter(ViewQuotedBookingSchema.VB_JS, SQLComparisonOperator.NotEqual, null);

					collection = ObjectFactory.Get<BusinessObjectCollection>("IViewQuotedBookingCollection", factory, query, consol);

					var defaultFilterProvider = new ForwardingShipmentDefaultFilterProvider();
					defaultFilterProvider.OriginPort = consol.JK_RL_NKLoadPort;
					defaultFilterProvider.DestinationPort = consol.JK_RL_NKDischargePort;
					defaultFilterProvider.ETDTo = consol.Transports.DepartureTransport.JW_ETD;
					defaultFilterProvider.ETAFrom = consol.Transports.ArrivalTransport.JW_ETA;
					defaultFilterProvider.SetDefaultFilters((IFilterBusinessObjectDefaultsProvider)collection);
				}

				return collection;
			}
		}

		IBusinessObjectCollection collection;

		protected override bool ValidateSelection(IEnumerable<BusinessObject> selectedObjects)
		{
			ForwardingShipmentCollection businessObjectsToAttach = new ForwardingShipmentCollection(factory);

			foreach (IViewQuotedBooking businessObject in selectedObjects)
			{
				var shipment = factory.Load<ForwardingShipment>(businessObject.VB_JS);
				businessObjectsToAttach.Add(shipment);
			}

			return AttachShipmentToConsolHelper.CheckAttaching(businessObjectsToAttach.ToList(), consol);
		}

		readonly BusinessObjectFactory factory;
		readonly ForwardingConsol consol;
	}
}
