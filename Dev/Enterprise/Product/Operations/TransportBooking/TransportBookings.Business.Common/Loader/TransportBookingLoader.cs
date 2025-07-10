using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Shared
{
	public static class TransportBookingLoader
	{
		public static IEnumerable<BusinessObject> GetRelatedTransportBookingEvents(IDtbBookingParent parent)
		{
			var result = new List<BusinessObject>();

			var consolidatedBookings = GetBookingConsolidations(parent);
			foreach (IDtbBookingConsolidation consolidatedBooking in consolidatedBookings)
			{
				result.AddRange(consolidatedBooking.Bookings.Cast<BusinessObject>());
			}

			return result;
		}

		public static IEnumerable<BusinessObject> GetRelatedTransportBookingEDocs(IDtbBookingParent parent)
		{
			var result = new List<BusinessObject>();
			var consignmentLoader = ObjectFactory.Get<IDtbConsignmentLoader>();
			var commonCartageLoader = ObjectFactory.Get<ICommonCartageLoader>();

			var consolidatedBookings = GetBookingConsolidations(parent);
			result.AddRange(consolidatedBookings.Cast<BusinessObject>());

			foreach (IDtbBookingConsolidation consolidatedBooking in consolidatedBookings)
			{
				result.AddRange(consolidatedBooking.Bookings.Cast<BusinessObject>());
				foreach (var booking in consolidatedBooking.Bookings)
				{
					result.AddRange(consignmentLoader.GetRelatedTransportConsignmentsAndChildrenForLocatingEDocs(booking));
					result.AddRange(commonCartageLoader.GetRelatedCommonCartagesForLocatingEDocs(booking).Cast<ICommonCartage>().OrderBy(pt => pt.JJ_ConsignmentID).Cast<BusinessObject>());
				}
			}

			return result;
		}

		public static IEnumerable<IJobInvoicingPlugIn> GetRelatedTransportBookingJobInvoicingPlugIn(IDtbBookingParent parent)
		{
			var result = new List<IJobInvoicingPlugIn>();

			var consolidatedBookings = GetBookingConsolidations(parent);
			foreach (var consolidatedBooking in consolidatedBookings)
			{
				foreach (var booking in consolidatedBooking.Bookings)
				{
					var supporter = booking as IRatingSupporter;
					RatingAdaptersProvider provider;

					if (supporter != null && (provider = supporter.AdaptersProvider) != null)
					{
						var adapters = provider.GetAdapters(null, AutoRateOptions.AutorateRevenue);
						if (adapters.Count > 0 && adapters[0] != null)
						{
							result.Add((IJobInvoicingPlugIn)booking);
						}
					}
				}
			}

			return result;
		}

		public static IDtbBookingConsolidation[] GetBookingConsolidations(IDtbBookingParent parent)
		{
			var bookingConsolQuery = new ZQuery(DtbBookingConsolidationSchema.KB_ParentID, parent.PK);
			bookingConsolQuery.AddToFilter(DtbBookingConsolidationSchema.KB_JobType, SQLComparisonOperator.NotEqual, TransportConsolidationJobTypes.Codes.Consignment);

			return parent.Factory.Load<IDtbBookingConsolidation>(bookingConsolQuery);
		}

		public static ZGuid[] GetBookingPKs(IDtbBookingParent parent)
		{
			var bookingQuery = new ZDBOnlyQuery(typeof(IDtbBooking));
			var bookingConsolSubQuery = new ZDBOnlySubQuery(typeof(IDtbBookingConsolidation), DtbBookingConsolidationSchema.PK);
			bookingConsolSubQuery.AddToFilter(DtbBookingConsolidationSchema.KB_ParentID, parent.PK);
			bookingQuery.AddSubQuery(DtbBookingSchema.KM_KB_Booking, bookingConsolSubQuery, JoinCondition.And);

			var bookings = parent.Factory.Load<IDtbBooking>(bookingQuery);
			return bookings.Select(b => b.PK).ToArray();
		}

		public static IEnumerable<IDtbBooking> GetBookingsToDeliver(IDtbBookingParent parent, IStmMenuItem menu)
		{
			return GetGUIProvider().GetBookingsToDeliver(parent, menu);
		}

		public static IEnumerable<IDtbBooking> GetBookingsToDeliver(IDtbBookingParent[] parents, IStmMenuItem menu)
		{
			return GetGUIProvider().GetBookingsToDeliver(parents, menu);
		}

		static ITransportBookingGUIProvider GetGUIProvider()
		{
			return ObjectFactory.Get<ITransportBookingGUIProvider>();
		}
	}
}
