using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class ViewQuotedBookingFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public ViewQuotedBookingFetchStrategy(ViewQuotedBooking viewQuotedBooking)
			: base(viewQuotedBooking)
		{
			ViewQuotedBooking = viewQuotedBooking;
		}

		ViewQuotedBooking ViewQuotedBooking { get; }

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			var shipmentQuery = new ZQuery(RateOneOffShipmentSchema.TT_TH, ViewQuotedBooking.VB_TH);

			if (ViewQuotedBooking.VB_JS.IsValid)
			{
				Factory.AddFetchHint(typeof(ForwardingShipment), JobShipmentSchema.PK, ViewQuotedBooking.VB_JS);
				Factory.AddFetchHint(typeof(ForwardingContainer), JobContainerSchema.JC_JS_FCLBookingOnlyLink, ViewQuotedBooking.VB_JS);

				// JobHeader
				var query = new ZQuery(JobHeaderSchema.JH_ParentID, ViewQuotedBooking.VB_JS);
				query.AddToFilter(JobHeaderSchema.JH_GC, ViewQuotedBooking.VB_GC);
				Factory.AddFetchHint(typeof(JobHeader), query);
			}

			if (ViewQuotedBooking.VB_TH.IsValid)
			{
				Factory.AddFetchHint(typeof(ForwardingShipment), new ZQuery(JobShipmentSchema.JS_TH_OneTimeQuote, ViewQuotedBooking.VB_TH));
				Factory.AddFetchHint(typeof(Quote), RatingHeaderSchema.PK, ViewQuotedBooking.VB_TH);

				// JobHeader
				var query = new ZQuery(JobHeaderSchema.JH_ParentID, ViewQuotedBooking.VB_TH);
				query.AddToFilter(JobHeaderSchema.JH_GC, ViewQuotedBooking.VB_GC);
				Factory.AddFetchHint(typeof(JobHeader), query);
			}

			Factory.AddFetchHint(typeof(RateOneOffShipment), shipmentQuery);
			Factory.AddFetchHint(GlbCompanySchema.PK, ViewQuotedBooking.VB_GC);

			// JobDocAddress
			var shipmentSubQuery = new ZDBOnlySubQuery(typeof(RateOneOffShipment), JobDocAddressSchema.E2_ParentID);
			shipmentSubQuery.AddToFilter(shipmentQuery);

			var addressesQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
			addressesQuery.AddSubQuery(shipmentSubQuery, JoinCondition.And);

			Factory.AddFetchHint(typeof(JobDocAddress), addressesQuery);

			// StmALog
			Factory.AddFetchHint(typeof(StmALog), StmALogSchema.SL_Parent, ViewQuotedBooking.VB_JS);
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			SalesRelationActivityFetchStrategyHelper.AddFetchHintsForView(Factory, (ISalesRelationActivity)BusinessObject, columns);

			var cusEntryNumRequired = false;

			foreach (var col in columns)
			{
				switch (col.ColumnName)
				{
					case "QuotedBooking+Booking+NumbersAsString":
						cusEntryNumRequired = true;
						break;
				}
			}

			if (cusEntryNumRequired && ViewQuotedBooking?.QuotedBooking?.Booking != null)
			{
				Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, ViewQuotedBooking.QuotedBooking.Booking.PK);
			}
		}
	}
}
