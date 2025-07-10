using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	[ModuleID(ModuleId.QuotedBookings)]
	public class ViewQuotedBookingCollection : BusinessObjectCollection<ViewQuotedBooking>, Integration.QuotedBooking.IViewQuotedBookingCollection, IFilterModuleExtraNotificationProvider, IExternalListValidation
	{
		public ViewQuotedBookingCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public ViewQuotedBookingCollection(BusinessObjectFactory factory, ZQuery query, ForwardingConsol consol)
			: base(factory, query)
		{
			ParentConsol = consol;
		}

		internal ForwardingConsol ParentConsol { get; set; }

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("Can't AddNew to this ViewQuotedBookingCollection");
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return fetchStrategy ?? (fetchStrategy = new ViewQuotedBookingCollectionFetchStrategy(this));
		}
		IBusinessObjectCollectionFetchStrategy fetchStrategy;

		public event Func<BusinessObject, INotification> GetExtraNotificationHanlder;

		#region Find Box List Provider

		public bool CanHandleBothQuoteAndShipmentCodes { get; set; }

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get
			{
				if (AllowTemplateRecords)
				{
					return CanHandleBothQuoteAndShipmentCodes ? new QuoteOrShipmentListWithTemplatesProvider(this) : new ViewQuotedBookingListWithTemplatesProvider(this);
				}
				return CanHandleBothQuoteAndShipmentCodes ? new QuoteOrShipmentListProvider(this) : new ViewQuotedBookingListProvider(this);
			}
		}

		internal class ViewQuotedBookingListProvider : FindBoxListProvider
		{
			public ViewQuotedBookingListProvider(ViewQuotedBookingCollection list)
				: base(list) { }

			protected override void AddCodeEqualsFilter(ZQuery query, string code)
			{
				var quoteQuery = new ZQuery(RatingHeaderSchema.TH_QuoteNumber, code);
				// This query is triggered by the DocImportManager service task, which retrieves all One-Off Quotes (OOQ).
				// Setting UserInteractive to false bypasses the current company filter, allowing retrieval across companies.
				if (Globals.IsUserInteractive)
				{
					quoteQuery.AddToFilter(RatingHeaderSchema.TH_GC, GlbCompany.CurrentCompany.PK);
				}

				var quote = List.Factory.LoadTop1<RatingHeader>(quoteQuery);
				if (quote != null)
				{
					query.AddToFilter(ViewQuotedBookingSchema.VB_TH, quote.PK);
				}
				else
				{
					base.AddCodeEqualsFilter(query, code);
				}
			}
		}
		internal class ViewQuotedBookingListWithTemplatesProvider : TemplateRecordFindboxListProvider
		{
			public ViewQuotedBookingListWithTemplatesProvider(ViewQuotedBookingCollection list)
				: base(list) { }

			protected override void AddCodeEqualsFilter(ZQuery query, string code)
			{
				ZQuery quoteQuery = new ZQuery(RatingHeaderSchema.TH_QuoteNumber, code);
				RatingHeader quote = List.Factory.LoadTop1<RatingHeader>(quoteQuery);
				if (quote != null)
				{
					query.AddToFilter(ViewQuotedBookingSchema.VB_TH, quote.PK);
				}
				else
				{
					base.AddCodeEqualsFilter(query, code);
				}
			}
		}

		internal class QuoteOrShipmentListProvider : ViewQuotedBookingListProvider
		{
			public QuoteOrShipmentListProvider(ViewQuotedBookingCollection list) : base(list) { }

			protected override void AddCodeEqualsFilter(ZQuery query, string code)
			{
				ZQuery quoteQuery = new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, code);
				ForwardingShipment booking = List.Factory.LoadTop1<ForwardingShipment>(quoteQuery);
				if (booking != null)
				{
					query.AddToFilter(ViewQuotedBookingSchema.VB_JS, booking.PK);
				}
				else
				{
					base.AddCodeEqualsFilter(query, code);
				}
			}
		}

		internal class QuoteOrShipmentListWithTemplatesProvider : ViewQuotedBookingListWithTemplatesProvider
		{
			public QuoteOrShipmentListWithTemplatesProvider(ViewQuotedBookingCollection list) : base(list) { }

			protected override void AddCodeEqualsFilter(ZQuery query, string code)
			{
				ZQuery quoteQuery = new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, code);
				ForwardingShipment booking = List.Factory.LoadTop1<ForwardingShipment>(quoteQuery);
				if (booking != null)
				{
					query.AddToFilter(ViewQuotedBookingSchema.VB_JS, booking.PK);
				}
				else
				{
					base.AddCodeEqualsFilter(query, code);
				}
			}
		}

		#endregion

		#region IFilterModuleExtraNotificationProvider Members

		INotification IFilterModuleExtraNotificationProvider.GetExtraNotification(BusinessObject businessObject)
		{
			var booking = businessObject as ViewQuotedBooking;

			if (booking != null && ParentConsol != null)
			{
				if (ParentConsol.Shipments.Contains(booking.VB_JS))
				{
					var duplicatedError = Res.GetString("8e0832c1-5166-49f9-ac48-a87e1464b095", "This record has already been selected. Please ensure you select only records that have not already been used.");
					return new Notification(CargoWise.ComponentModel.NotificationType.Error, duplicatedError);
				}

				using
				(
					new DisposableAction(
						() => FreightShipmentVsConsolMessageHelper.Instance.IsGatewayServiceLevelCheckSuspended = true,
						() => FreightShipmentVsConsolMessageHelper.Instance.IsGatewayServiceLevelCheckSuspended = false)
				)
				{
					var attachRequest = FreightShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachShipment(ParentConsol, Factory.Load<ForwardingShipment>(booking.VB_JS));
					var error = attachRequest.Errors;
					if (!error.IsEmpty)
					{
						return new Notification(CargoWise.ComponentModel.NotificationType.Error, error);
					}

					var warning = attachRequest.Warnings;
					if (!warning.IsEmpty)
					{
						return new Notification(CargoWise.ComponentModel.NotificationType.Warning, warning);
					}
				}
			}

			if (GetExtraNotificationHanlder != null)
			{
				return GetExtraNotificationHanlder(businessObject);
			}

			return null;
		}

		#endregion

		#region Template Records

		public bool AllowTemplateRecords { get; set; }

		bool IExternalListValidation.IsValidTemplateRecordPK(ZGuid pk)
		{
			if (AllowTemplateRecords)
			{
				var templateRecord = Factory.Load<StmTemplateRecord>(pk);
				return templateRecord != null && templateRecord.STR_ModuleID == nameof(ModuleId.QuotedBookings);
			}
			{
				return false;
			}
		}

#if DEBUG
		internal IFindBoxListProvider FindBoxListProviderExposedForTest => FindBoxListProvider;
#endif

		#endregion
	}
}
