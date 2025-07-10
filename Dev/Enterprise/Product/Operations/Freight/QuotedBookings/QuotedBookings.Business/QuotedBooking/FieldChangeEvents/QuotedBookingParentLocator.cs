using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Rating.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class ShipmentToQuotedBookingParentLocator : IBusinessObjectParentLocator
	{
		public bool TryLocateParentBusinessObjects(BusinessObject child, out IEnumerable<BusinessObject> parents)
		{
			parents = Enumerable.Empty<BusinessObject>();

			if ((child is ForwardingShipment shipment)
				&& shipment.JS_IsBooking
				&& !shipment.JS_IsForwardRegistered)
			{
				parents = (shipment.Factory.Load<ViewQuotedBooking>(shipment.PK)?.QuotedBooking).Yield();
			}

			return parents.Any();
		}
	}

	public class QuoteToQuotedBookingParentLocator : IBusinessObjectParentLocator
	{
		public bool TryLocateParentBusinessObjects(BusinessObject child, out IEnumerable<BusinessObject> parents)
		{
			if (!(child is Quote))
			{
				parents = Enumerable.Empty<BusinessObject>();
				return false;
			}

			var quote = (Quote)child;
			parents = (quote.Factory.Load<ViewQuotedBooking>(quote.PK)?.QuotedBooking).Yield();
			return parents.Any();
		}
	}

	class ShipmentToQuotedBookingParentLocatorFactory : IBusinessObjectParentLocatorFactory
	{
		public IBusinessObjectParentLocator BusinessObjectParentLocator => new ShipmentToQuotedBookingParentLocator();

		public string ChildTableCodePrefix => JobShipmentSchema.Constants.Prefix;
	}

	class QuoteToQuotedBookingParentLocatorFactory : IBusinessObjectParentLocatorFactory
	{
		public IBusinessObjectParentLocator BusinessObjectParentLocator => new QuoteToQuotedBookingParentLocator();

		public string ChildTableCodePrefix => RatingHeaderSchema.Constants.Prefix;
	}
}
