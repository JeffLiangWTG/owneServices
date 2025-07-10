using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class AgencyShipmentEventParentFinder<T> : EventParentFinder where T : AgencyShipment
	{
		public AgencyShipmentEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			BusinessObject[] result = null;

			IXmlEventValueObject eventValueObject = xmlEvent;

			if (eventValueObject.Context != null && IsAgencyShipmentDataContext(eventValueObject.Context))
			{
				var references = new AgencyShipmentReferences(eventValueObject.Context);

				var query = new ZQuery(RefShippingLineSchema.RSL_CargoWiseOneCode, eventValueObject.Context.CarrierC1CCode.GetValueOrDefault());
				var shippingLine = factory.Load<RefShippingLine>(query).FirstOrDefault();

				if (eventValueObject.EventType == AutoEvents.SubscriptionRequestedCode)
				{
					if (shippingLine == null || !shippingLine.RSL_IsShippingLine)
					{
						return null;
					}
					else if (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.Value.IsActive)
					{
						references.ShipmentID = eventValueObject.Context.CarriersBookingReference;
						references.OriginUNLOCO = eventValueObject.Context.MBOLOriginUNLOCO;
						references.DestinationUNLOCO = eventValueObject.Context.MBOLDestinationUNLOCO;
					}
				}

				var matcher = new AgencyShipmentMatcher<T>(factory, references, logger, eventValueObject);
				var (bestMatch, reason) = matcher.GetBestMatchWithReason();
				if (xmlEvent.EventType.GetValueOrDefault() == AutoEvents.SubscriptionRequestedCode &&
					xmlEvent.EventParameters?.Type?.ToString() == (NoResString)"Shipment Visibility")
				{
					if (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.Value.IsActive)
					{
						GenerateProcessLog(bestMatch, reason, xmlEvent);
					}
					else
					{
						bestMatch = null;
						GenerateProcessLog(bestMatch, Res.GetString("8c1f0003-21d3-4e46-985d-45b36fe8c746", "Tracking is not supported for this Carrier."), xmlEvent);
					}
				}

				result = bestMatch != null ? new BusinessObject[] { bestMatch } : null;
			}

			return result;
		}

		bool IsAgencyShipmentDataContext(IXmlEventValueObjectContextValueList context)
		{
			var containerValueTypes = new[]
			{
				nameof(UniversalEvent.ContextTypes.ContainerNumber),
				nameof(UniversalEvent.ContextTypes.ContainerISOCode),
				nameof(UniversalEvent.ContextTypes.ContainerReleaseNumber),
				nameof(UniversalEvent.ContextTypes.GoodsItemID)
			};

			var receivedContextValueTypes = context.Values.Select(val => val.Key.Type.ToString());

			return !receivedContextValueTypes.Intersect(containerValueTypes).Any();
		}

		void GenerateProcessLog(AgencyShipment shipment, ZString reason, IXmlEventValueObject xmlEvent)
		{
			if (shipment == null)
			{
				logger.Log(LogType.Information,$"Cannot link {GetShipmentType()} because: [* {reason} *]");
			}
			else
			{
				logger.Log(LogType.Information, $"Successfully saved: [*Provider subscription {GetSuccessSubscriptionInfo(shipment, xmlEvent)} was Confirmed. Subscription created*]");
			}
		}

		ZString GetSuccessSubscriptionInfo(AgencyShipment shipment, IXmlEventValueObject xmlEvent)
		{
			var matchReference = ZString.Empty;

			if (xmlEvent.Context.SubscriptionType == "MBOLNumber")
			{
				if (!shipment.JS_HouseBill.IsEmpty && shipment.JS_HouseBill.Equals(xmlEvent.Context.MBOLNumber))
				{
					matchReference = (NoResString)"Master Bill";
				}
				else if (!shipment.JS_UniqueConsignRef.IsEmpty && shipment.JS_UniqueConsignRef == xmlEvent.Context.CarriersBookingReference)
				{
					matchReference = (NoResString)"Carrier Booking Reference";
				}
			}
			else
			{
				if (!shipment.JS_UniqueConsignRef.IsEmpty && shipment.JS_UniqueConsignRef == xmlEvent.Context.CarriersBookingReference)
				{
					matchReference = (NoResString)"Carrier Booking Reference";
				}
				else if (!shipment.JS_HouseBill.IsEmpty && shipment.JS_HouseBill.Equals(xmlEvent.Context.MBOLNumber))
				{
					matchReference = (NoResString)"Master Bill";
				}
			}

			var subscriptionInfo = matchReference.IsEmpty
				? (NoResString)"for Agency Shipment"
				: (NoResString)$"for {matchReference} '{(matchReference == "Master Bill" ? shipment.JS_HouseBill : shipment.JS_UniqueConsignRef)}'";
			return subscriptionInfo;
		}

		ZString GetShipmentType()
		{
			if (typeof(T) == typeof(AgencyBooking))
			{
				return (NoResString)"Agency Booking";
			}
			if (typeof(T) == typeof(BillOfLading))
			{
				return (NoResString)"Bill of Lading";
			}
			return (NoResString)"Agency Shipment";
		}
	}
}


