using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	public class QuotedBookingDataContextManager : ShipmentDataContextManager<QuotedBooking>, IEventTransformer, IDataContextCoordinator
	{
		public override ZString DataContextKey
		{
			get { return ParentBO == null ? ZString.Empty : ParentBO.Booking?.JS_UniqueConsignRef ?? ParentBO.Quote.TH_QuoteNumber; }
		}

		public override DataContextType DataContextType
		{
			get { return ParentBO == null || ParentBO.Booking != null ? DataContextType.ForwardingBooking : DataContextType.OneOffQuote; }
		}

		public override string DefaultOutputDirectory
		{
			get { return SystemDataRegistry.Instance.BookingExportDirectory.Value; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var query = new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, matchingValues.Key);
			query.AddToFilter(JobShipmentSchema.JS_IsCancelled, false);

			var shipment = factory.LoadTop1<ForwardingShipment>(query);
			if (shipment == null)
			{
				return null;
			}

			var bookingQuery = new ZQuery(ViewQuotedBookingSchema.VB_JS, shipment.PK);
			bookingQuery.AddToFilter(ViewQuotedBookingSchema.VB_IsCanceled, false);

			return bookingQuery;
		}

		#region Universal Event Management

		public override bool ManagesEvents
		{
			get { return ParentBO == null || ParentBO.Booking != null; }
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				var forwardingBookingEventContextReader = new ForwardingBookingEventContextReader(ParentBO, Helper);
				forwardingBookingEventContextReader.AddQuotedBookingContextValues(result);
			}

			return result.Count == 0 ? null : result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ForwardingBookingEventParentFinder(factory, this, logger, Helper);
		}

		protected override void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalEvent eventAdded)
		{
			base.OnUniversalEventAddedCore(logger, eventAdded);
			CO2eDataTransferHelper.OnCO2eRejectionEvent(eventAdded, ParentBO);
		}

		protected override bool CanUpdateLogParentFromEventCore(BusinessObject logParent, IXmlEventValueObject xmlEvent, out ZString failureReason)
		{
			if (logParent is QuotedBooking quotedBooking
				&& quotedBooking.Booking is ForwardingShipment booking
				&& booking.JS_IsForwardRegistered)
			{
				failureReason = Res.GetString("d7fd2f30-aade-4462-aee6-4073ad9c5e89",
					"[*XML Targeted a converted Booking. Target Type should be {0} for updating converted Bookings.*]",
					nameof(DataContextType.ForwardingShipment));
				return false;
			}

			return base.CanUpdateLogParentFromEventCore(logParent, xmlEvent, out failureReason);
		}

		#endregion

		#region Universal Shipment Management

		public override bool ManagesShipments => true;

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			if (writeManager?.Action?.ParentBO is QuotedBooking quotedBooking)
			{
				switch (quotedBooking.ObjectState)
				{
					case QuotedBookingState.QuoteOnly:
						return new OneOffQuoteDataObjectWriter(writeManager, ParentBO);
					case QuotedBookingState.UnacceptedBookingWithQuote:
					case QuotedBookingState.AcceptedBookingWithQuote:
						return new BookingWithQuoteDataObjectWriter(writeManager, ParentBO);
				}
			}
			return new ForwardingBookingDataObjectWriter(writeManager, ParentBO);
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return recipientRoles.Any(r => r.Code == RecipientRoleType.NVO && r.ServiceCode == ServiceCodeType.BRQ);
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return universalShipment.QuoteNumber.HasValue && DataContextType == DataContextType.ForwardingBooking
				? new BookingWithQuoteDataObjectReader(universalShipment, logger, factory, Helper)
				: new ForwardingBookingDataObjectReader(universalShipment, logger, factory, Helper);
		}

		#endregion

		protected override QuotedBooking[] LoadBusinessObjects(BusinessObjectFactory factory, ZQuery query)
		{
			return factory.Load<ViewQuotedBooking>(query).Select(view => view.QuotedBooking).ToArray();
		}

		public EventValue Transform(EventValue sourceEventValue, UniversalEvent sourceUniversalEvent, IStmALogParent logParent)
		{
			if (sourceEventValue.Code == AutoEvents.SubscriptionRequestedCode && sourceUniversalEvent != null)
			{
				return SubscriptionRequesteEventTransformer.Transform(sourceEventValue, sourceUniversalEvent);
			}

			return sourceEventValue;
		}

		IUniversalFreightHelper Helper
		{
			get { return helper ?? (helper = new UniversalForwardingHelper()); }
		}

		IUniversalFreightHelper helper;
		public string GetUniqueContextIdentifier(IXmlEventValueObject xmlEvent)
		{
			return xmlEvent.EventType == AutoEvents.SubscriptionRequested.Code ? JobShipmentSchema.Constants.TableName : string.Empty;
		}
	}
}
