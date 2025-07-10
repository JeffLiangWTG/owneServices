using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class OrderListForDelayedJobNumberFetchHints : List<WhsDocket>
	{
		OrderListForDelayedJobNumberFetchHints()
		{
		}

		public static OrderListForDelayedJobNumberFetchHints GetInstanceForFactory(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("OrderListForDelayedJobNumberFetchHints", () => new OrderListForDelayedJobNumberFetchHints());
		}

		public void LoadFetchHints(BusinessObjectFactory factory)
		{
			if (this.Count > 0)
			{
				// Adding these fetch hints requires loading collections, so we should only add them when needed AND when supporting fetch hints have already been added for all orders
				var bookingConsolidation = factory.Load<IDtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_ParentID, this.Select(c => c.PK)));

				var consolPKs = bookingConsolidation.Select(b => b.PK).ToArray();
				if (consolPKs.Length > 0)
				{
					factory.AddFetchHint(DtbBookingSchema.Instance, new ZQuery(DtbBookingSchema.KM_KB_Booking, consolPKs));
				}

				var bookingPKs = factory.Load<IDtbBooking>(new ZQuery(DtbBookingSchema.KM_KB_Booking, consolPKs)).Select(b => b.PK).ToArray();
				foreach (var pk in bookingPKs)
				{
					factory.AddFetchHint(JobCartageSchema.JJ_ParentID, pk);
				}

				var consignmentConsolidations = factory.Load<IDtbConsignmentConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_ParentID, bookingPKs));
				foreach (var consignmentConsol in consignmentConsolidations)
				{
					factory.AddFetchHint(DtbBookingSchema.KM_KB_Booking, consignmentConsol.PK);
				}

				Clear();
			}
		}
	}
}
