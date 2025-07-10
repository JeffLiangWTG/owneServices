using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	class ForwardingBookingEventParentFinder : EventParentFinder
	{
		internal ForwardingBookingEventParentFinder(BusinessObjectFactory factory, QuotedBookingDataContextManager manager, IXmlImportLogger logger, IUniversalFreightHelper helper)
			: base(factory, manager, logger)
		{
			this.helper = Argument.NotNull(helper, "helper");
		}

		readonly IUniversalFreightHelper helper;

		protected override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent)
		{
			return new ForwardingBookingEventParentFinderHelper(factory, logger, helper).GetLogParentsFor(xmlEvent);
		}

		protected override IKeysResult GetLogParentKeysForEventCore(IXmlEventValueObject xmlEvent)
		{
			var keys = base.GetLogParentKeysForEventCore(xmlEvent);
			if (keys.IsMatch)
			{
				return KeysResult.Match(keys.KeysInfo.Concat(new ForwardingBookingEventParentFinderHelper(factory, logger, helper).GetLogParentKeysFor(xmlEvent)).ToArray());
			}
			else
			{
				return keys;
			}
		}

		class ForwardingBookingEventParentFinderHelper : BaseShipmentEventParentFinderHelper
		{
			internal ForwardingBookingEventParentFinderHelper(BusinessObjectFactory factory, IXmlImportLogger logger, IUniversalFreightHelper helper)
				: base(factory, logger)
			{
				this.helper = helper;
			}

			readonly IUniversalFreightHelper helper;

			internal BusinessObject[] GetLogParentsFor(UniversalEvent xmlEvent)
			{
				var forwardingBookingReference = new ShipmentReferences();

				PopulateShipmentReference(xmlEvent, forwardingBookingReference, helper);

				IXmlEventValueObject eventValueObject = xmlEvent;

				var query = new ZQuery(RefShippingLineSchema.RSL_CargoWiseOneCode, eventValueObject.Context.CarrierC1CCode.GetValueOrDefault());
				var shippingLine = factory.Load<RefShippingLine>(query).FirstOrDefault();
				if (eventValueObject.EventType == AutoEvents.SubscriptionRequestedCode)
				{
					if (shippingLine == null || !shippingLine.RSL_IsNVO)
					{
						return null;
					}
					else if (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.Value.IsActive)
					{
						forwardingBookingReference.HBOLNumber = eventValueObject.Context.MBOLNumber.GetValueOrDefault();
						forwardingBookingReference.ShipmentID = eventValueObject.Context.CarriersBookingReference;
						forwardingBookingReference.OriginUNLOCO = eventValueObject.Context.MBOLOriginUNLOCO;
						forwardingBookingReference.DestinationUNLOCO = eventValueObject.Context.MBOLDestinationUNLOCO;
					}
				}

				var matcher = new QuotedBookingMatcher(factory, forwardingBookingReference, logger, helper, eventValueObject);
				var (quotedBooking, reason) = matcher.GetBestMatchWithReason();

				if (xmlEvent.EventType.GetValueOrDefault() == AutoEvents.SubscriptionRequestedCode  &&
					xmlEvent.EventParameters?.Type?.ToString() == (NoResString)"Shipment Visibility")
				{
					if (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.Value.IsActive)
					{
						GenerateProcessLog(quotedBooking, reason, xmlEvent);
					}
					else
					{
						quotedBooking = null;
						GenerateProcessLog(quotedBooking, Res.GetString("2e02d7c7-497f-449d-8f0e-ab036931f824", "Tracking is not supported for this Carrier."), xmlEvent);
					}
				}

				return quotedBooking == null ? null : new BusinessObject[] { quotedBooking };
			}

			internal IEnumerable<(string KeyValue, string KeySource)> GetLogParentKeysFor(IXmlEventValueObject xmlEvent)
			{
				var forwardingBookingReference = new ShipmentReferences();
				PopulateShipmentReference(xmlEvent, forwardingBookingReference, helper);
				var matcher = new QuotedBookingMatcher(factory, forwardingBookingReference, logger, helper, xmlEvent);
				return matcher.GetMatchingShipmentKeys(forwardingBookingReference);
			}

			void GenerateProcessLog(QuotedBooking booking, ZString reason, IXmlEventValueObject xmlEvent)
			{
				if (booking == null)
				{
					logger.Log(LogType.Information,$"Cannot link Forwarding Booking because: [* {reason} *]");
				}
				else
				{
					logger.Log(LogType.Information, $"Successfully saved: [*Provider subscription {GetSuccessSubscriptionInfo(booking, xmlEvent)} was Confirmed. Subscription created*]");
				}
			}

			ZString GetSuccessSubscriptionInfo(QuotedBooking booking, IXmlEventValueObject xmlEvent)
			{
				var matchReference = ZString.Empty;

				if (xmlEvent.Context.SubscriptionType == "MBOLNumber")
				{
					if (!booking.Booking.JS_HouseBill.IsEmpty && booking.Booking.JS_HouseBill.Equals(xmlEvent.Context.MBOLNumber))
					{
						matchReference = (NoResString)"Master Bill";
					}
					else if (!booking.Booking.JS_UniqueConsignRef.IsEmpty && booking.Booking.JS_UniqueConsignRef == xmlEvent.Context.CarriersBookingReference)
					{
						matchReference = (NoResString)"Carrier Booking Reference";
					}
				}
				else
				{
					if (!booking.Booking.JS_UniqueConsignRef.IsEmpty && booking.Booking.JS_UniqueConsignRef == xmlEvent.Context.CarriersBookingReference)
					{
						matchReference = (NoResString)"Carrier Booking Reference";
					}
					else if (!booking.Booking.JS_HouseBill.IsEmpty && booking.Booking.JS_HouseBill.Equals(xmlEvent.Context.MBOLNumber))
					{
						matchReference = (NoResString)"Master Bill";
					}
				}

				var subscriptionInfo = matchReference.IsEmpty
					? (NoResString)"for Forwarding Booking"
					: (NoResString)$"for {matchReference} '{(matchReference == "Master Bill" ? booking.Booking.JS_HouseBill : booking.Booking.JS_UniqueConsignRef)}'";
				return subscriptionInfo;
			}
		}
	}
}
