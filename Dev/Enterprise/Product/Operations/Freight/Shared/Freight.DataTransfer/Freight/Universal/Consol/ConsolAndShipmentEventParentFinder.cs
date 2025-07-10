using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public abstract class ConsolAndShipmentEventParentFinder<TConsol, TShipment, TContainer> : EventParentFinder
		where TConsol : CommonConsol
		where TShipment : CommonShipment
		where TContainer : CommonContainer
	{
		protected ConsolAndShipmentEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger, IUniversalFreightHelper helper)
			: base(factory, manager, logger)
		{
			this.helper = Argument.NotNull(helper, "helper");
		}

		protected readonly IUniversalFreightHelper helper;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "these documents have version attached in data target, should not use this fallback matching.")]
		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			var documentName = xmlEvent?.DataContext?.DocumentaryOverride?.DocumentName ?? ZString.Empty;

			if (documentName == "Booking Request" || documentName == "Shipping Instruction" || documentName == "Shipping Order")
			{
				return null;
			}

			var consolFinder = new CommonConsolEventParentFinderHelper(xmlEvent, helper);
			var shipmentFinder = new ShipmentEventParentFinderHelper<TShipment>(xmlEvent, factory, logger, helper);
			var logParents = GetLogParents(consolFinder, shipmentFinder, xmlEvent);
			if (LoggerHasContainerCreationError())
			{
				return null;
			}
			else if (logParents != null)
			{
				return logParents;
			}

			var shipmentReferences = shipmentFinder.GetShipmentReferences();
			if (!shipmentReferences.IsEmpty)
			{
				var shipmentMatcher = new ShipmentMatcher<TShipment>(factory, shipmentReferences, logger, helper);
				var (bestMatchingShipmentParent, reason) = shipmentMatcher.GetBestMatchWithReason();

				if (xmlEvent.EventType.GetValueOrDefault() == AutoEvents.SubscriptionRequestedCode  &&
					xmlEvent.EventParameters?.Type?.ToString() == (NoResString)"Shipment Visibility")
				{
					if (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.Value.IsActive)
					{
						GenerateProcessLog(bestMatchingShipmentParent, reason, xmlEvent);
					}
					else
					{
						bestMatchingShipmentParent = null;
						GenerateProcessLog(bestMatchingShipmentParent, Res.GetString("237c705c-d628-40b3-ae89-28eafb46952a", "Tracking is not supported for this Carrier."), xmlEvent);
					}
				}
				return bestMatchingShipmentParent == null ? null : new BusinessObject[] { bestMatchingShipmentParent };
			}

			var consolReferences = consolFinder.GetConsolReferences();
			if (!consolReferences.IsEmpty)
			{
				return GetBestMatchingConsolParent(consolReferences);
			}

			return GetBestFallBackParent(shipmentReferences, consolReferences);
		}

		BusinessObject[] GetBestMatchingConsolParent(CommonConsolReferences consolReferences)
		{
			var consolMatcher = new ConsolMatcher<TConsol>(factory, consolReferences, logger, helper);
			var bestMatchingConsolParent = consolMatcher.GetBestMatch();

			return bestMatchingConsolParent == null ? null : new BusinessObject[] { bestMatchingConsolParent };
		}

		BusinessObject[] GetBestFallBackParent(ShipmentReferences shipmentReferences, CommonConsolReferences consolReferences)
		{
			if (helper.ShipmentHasAdditionalReferences && shipmentReferences.AdditionalReferences != null && shipmentReferences.AdditionalReferences.Count > 0)
			{
				var shipmentMatcher = new ShipmentMatcher<TShipment>(factory, shipmentReferences, logger, helper);
				var bestMatchingShipmentParent = shipmentMatcher.GetBestMatch();
				if (bestMatchingShipmentParent != null)
				{
					return new BusinessObject[] { bestMatchingShipmentParent };
				}
			}

			return GetBestMatchingConsolParent(consolReferences);
		}

		BusinessObject[] GetLogParents(CommonConsolEventParentFinderHelper consolFinder, ShipmentEventParentFinderHelper<TShipment> shipmentFinder, IXmlEventValueObject xmlEvent)
		{
			if (consolFinder.Masterbill.IsEmpty && consolFinder.BookingReference.IsEmpty)
			{
				var logParents = shipmentFinder.GetLogParents();
				if (logParents != null)
				{
					return logParents;
				}
			}
			else
			{
				var shipmentQuery = shipmentFinder.BuildShipmentQueryIfHouseBillPresent();
				if (shipmentQuery == null)
				{
					var logParents = new ConsolLinker<TConsol>(factory, helper, logger).GetLogParent(xmlEvent);
					if (logParents != null && logParents.Any())
					{
						return logParents;
					}
				}
				else
				{
					var consolSubQuery = consolFinder.BuildConsolSubQueryIfMasterBillPresent();
					var logParents = shipmentFinder.GetLogParents(consolSubQuery, shipmentQuery);
					if (logParents != null)
					{
						return logParents;
					}
				}
			}

			if (shipmentFinder.HouseBill.IsEmpty && consolFinder.Masterbill.IsEmpty && consolFinder.BookingReference.IsEmpty)
			{
				var containers = new ContainerLinker<TContainer, TConsol>(factory, helper).GetLogParent(xmlEvent);
				if (containers != null)
				{
					return containers;
				}
			}

			return null;
		}

		bool LoggerHasContainerCreationError()
		{
			return logger.Logs?.Any(log => log.Type == Enterprise.Integration.LogType.Error && log.Message.Contains((NoResString)"System cannot create new Container")) ?? false;
		}

		void GenerateProcessLog(CommonShipment bo, ZString reason, IXmlEventValueObject xmlEvent)
		{
			if (bo == null)
			{
				logger.Log(LogType.Information,$"Error - Cannot link shipment because: [* {reason} *]");
			}
			else
			{
				logger.Log(LogType.Information, $"Successfully saved [*Provider subscription{GetSuccessSubscriptionInfo(bo, xmlEvent)} was Confirmed. Subscription created*]");
			}
		}

		ZString GetSuccessSubscriptionInfo(CommonShipment booking, IXmlEventValueObject xmlEvent)
		{
			ZString matchReference = ZString.Empty;

			if (xmlEvent.Context.SubscriptionType == "MBOLNumber")
			{
				if (!booking.JS_HouseBill.IsEmpty && booking.JS_HouseBill.Equals(xmlEvent.Context.MBOLNumber))
				{
					matchReference = (NoResString)"Master Bill";
				}
				else if (!booking.JS_UniqueConsignRef.IsEmpty && booking.JS_UniqueConsignRef == xmlEvent.Context.CarriersBookingReference)
				{
					matchReference = (NoResString)"Carrier Booking Reference";
				}
			}
			else
			{
				if (!booking.JS_UniqueConsignRef.IsEmpty && booking.JS_UniqueConsignRef == xmlEvent.Context.CarriersBookingReference)
				{
					matchReference = (NoResString)"Carrier Booking Reference";
				}
				else if (!booking.JS_HouseBill.IsEmpty && booking.JS_HouseBill.Equals(xmlEvent.Context.MBOLNumber))
				{
					matchReference = (NoResString)"Master Bill";
				}
			}

			var subscriptionInfo = matchReference.IsEmpty
				? ZString.Empty
				: (NoResString)$" for Shipment {matchReference} '{(matchReference == "Master Bill" ? booking.JS_HouseBill : booking.JS_UniqueConsignRef)}'";
			return subscriptionInfo;
		}
	}
}
