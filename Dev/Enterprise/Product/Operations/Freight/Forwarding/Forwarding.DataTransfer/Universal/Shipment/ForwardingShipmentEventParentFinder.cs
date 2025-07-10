using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingShipmentEventParentFinder : ConsolAndShipmentEventParentFinder<ForwardingConsol, ForwardingShipment, ForwardingContainer>
	{
		public ForwardingShipmentEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger, new UniversalForwardingHelper())
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			var transitReceiveKey = xmlEvent.GetMatchingDataSource(DataContextType.TransitReceive)?.Key.GetValueOrDefault() ?? ZString.Empty;
			if (!transitReceiveKey.IsEmpty)
			{
				var shipments = TryFindMatchingShipmentsForTransitReceive(transitReceiveKey);
				shipments?.ForEach(shipment => UpdateMatchingPackLinesForFranceTransitReceive(shipment, xmlEvent));

				return shipments;
			}

			var logParents = new ForwardingShipmentEventParentFinderHelper(factory, logger, helper).GetLogParentsForEvent(xmlEvent);
			if (logParents != null)
			{
				return logParents;
			}

			return xmlEvent.HasMatchingDataTarget(DataContextType.ForwardingShipment)
				&& !xmlEvent.HasMatchingDataTarget(DataContextType.ForwardingConsol)
				? base.GetLogParentsForEventUsingContext(xmlEvent)
				: null;
		}

		protected override BusinessObject[] GetChildrenIfSpecifiedInContext(BusinessObject[] logParents, UniversalEvent eventData)
		{
			var result = new List<BusinessObject>();

			if (logParents?.FirstOrDefault() is ForwardingShipment shipment
				&& eventData is IXmlEventValueObject xmlEvent)
			{
				if (ShouldUpdateTransitReceiveByEvent(shipment, eventData))
				{
					UpdateMatchingPackLinesForFranceTransitReceive(shipment, eventData);
				}

				var containerNumbers = xmlEvent.Context.ContainerNumbers;
				if (containerNumbers != null && containerNumbers.Any())
				{
					var matchingContainers = TryFindMatchingContainers(shipment, eventData, containerNumbers);
					if (matchingContainers.Any())
					{
						return matchingContainers;
					}
				}

				result.AddRange(logParents);

				var supporter = shipment.GetSupporter();
				var documentEventParent = supporter?.GetEventParent(eventData);

				if (documentEventParent is BusinessObject bizObj)
				{
					result.Add(bizObj);
				}
			}

			return base.GetChildrenIfSpecifiedInContext(result.ToArray(), eventData);
		}

		ForwardingShipment[] TryFindMatchingShipmentsForTransitReceive(string transitReceiveKey)
		{
			var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, ForwardingPackLine.Schema.TableName);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, transitReceiveKey);

			var packLineQuery = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.JL_JS);
			packLineQuery.AddSubQuery(entryNumberQuery, JoinCondition.And);

			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobShipmentSchema.JS_OA_ExportReceivingDepot);
			orgAddressQuery.AddToFilter(OrgAddressSchema.OA_RN_NKCountryCode, Constants.CountryCodes.FranceAndOverseasDepartmentsAndTerritories);

			var shipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
			shipmentQuery.AddSubQuery(packLineQuery, JoinCondition.And);
			shipmentQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);

			var shipments = factory.Load<ForwardingShipment>(shipmentQuery);
			return shipments.Any() ? shipments : null;
		}

		CommonContainer[] TryFindMatchingContainers(ForwardingShipment shipment, UniversalEvent eventData, List<ZString> contextContainerNumbers)
		{
			var matchingContainers = shipment.Containers.Where(container => contextContainerNumbers.Contains(container.JC_ContainerNum));

			var eventCode = eventData.EventType ?? ZString.Empty;
			var eventReference = eventData.EventReference ?? ZString.Empty;

			if ((eventCode == Events.PickedUpCode || eventCode == Events.DeliveredCode) && shipment.Consols.Count > 1)
			{
				var earliestConsol = shipment.Consols.GetEarliestConsol();
				if (earliestConsol != null && ForwardingContainerEventHelper.IsExportPickupOrDeliveryEvent(eventCode, eventReference))
				{
					return matchingContainers.Where(container => container.JC_JK == earliestConsol.PK).ToArray();
				}

				var latestConsol = shipment.Consols.GetLatestConsol();
				if (latestConsol != null && ForwardingContainerEventHelper.IsImportPickupOrDeliveryEvent(eventCode, eventReference))
				{
					return matchingContainers.Where(container => container.JC_JK == latestConsol.PK).ToArray();
				}
			}

			return matchingContainers.ToArray();
		}

		bool ShouldUpdateTransitReceiveByEvent(ForwardingShipment shipment, UniversalEvent eventData)
		{
			var eventCode = eventData.EventType;
			if (!eventCode.HasValue)
			{
				return false;
			}

			var isTransitReceive = eventData.GetMatchingDataSource(DataContextType.TransitReceive) != null;
			var isFranceOrTerritoryCFS = Constants.CountryCodes.IsFranceOrTerritory(shipment.ExportReceivingDepot?.OA_RN_NKCountryCode);

			return isTransitReceive && isFranceOrTerritoryCFS;
		}

		void UpdateMatchingPackLinesForFranceTransitReceive(ForwardingShipment shipment, UniversalEvent eventData)
		{
			var eventCode = eventData.EventType.GetValueOrDefault();

			switch (eventCode)
			{
				case AutoEvents.MessageAcceptedCode:
					ProcessMessageAcceptedEventForTransitReceive(shipment, eventData);
					break;
				case AutoEvents.HeldCode:
					ProcessMessageHeldEventData(shipment, eventData);
					break;
				case AutoEvents.ClearanceCompletedCode:
					ProcessMessageClearanceCompletedEventData(shipment, eventData);
					break;
				default:
					break;
			}
		}

		void ProcessMessageAcceptedEventForTransitReceive(ForwardingShipment shipment, UniversalEvent eventData)
		{
			if (IsTransitReceiveMAAEvent(eventData))
			{
				var pickupCFSCountryCode = shipment.ExportReceivingDepot?.OA_RN_NKCountryCode ?? Constants.CountryCodes.France;

				var packLinesNeedBeUpdated = shipment.OuterPackLines.OfType<ForwardingPackLine>().Where(packLine =>
					HasCusEntryNumber(packLine.AdditionalReferenceNumbers.Cast<CusEntryNumber>(),
						ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC,
						pickupCFSCountryCode,
						eventData.GetMatchingDataSource(DataContextType.TransitReceive)?.Key.GetValueOrDefault() ?? ZString.Empty));

				if (packLinesNeedBeUpdated.Any())
				{
					SetPackLinesPortReference(packLinesNeedBeUpdated, eventData);
				}
			}
		}

		void ProcessMessageHeldEventData(ForwardingShipment shipment, UniversalEvent eventData)
		{
			if (IsTransitReceiveSHLEvent(eventData))
			{
				var packLinesNeedBeUpdated = GetPackLinesForSHLAndSCMEvent(shipment, eventData);

				if (packLinesNeedBeUpdated.Any())
				{
					UpdatePackLinesPANStatus(packLinesNeedBeUpdated, eventData);
				}
			}
		}

		void ProcessMessageClearanceCompletedEventData(ForwardingShipment shipment, UniversalEvent eventData)
		{
			if (IsTransitReceiveSCMEvent(eventData))
			{
				var packLinesNeedBeUpdated = GetPackLinesForSHLAndSCMEvent(shipment, eventData);

				if (packLinesNeedBeUpdated.Any())
				{
					UpdatePackLinesPANStatus(packLinesNeedBeUpdated, eventData, TransitWarehouseReferenceStatus.Codes.Cleared);
				}
			}
		}

		IEnumerable<ForwardingPackLine> GetPackLinesForSHLAndSCMEvent(ForwardingShipment shipment, UniversalEvent eventData)
		{
			var pickupCFSCountryCode = shipment.ExportReceivingDepot?.OA_RN_NKCountryCode ?? Constants.CountryCodes.France;
			var customsReferenceNumber = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, eventData.EventParameters, eventData.EventReference)
				.GetValueOrDefault();

			return shipment.OuterPackLines.OfType<ForwardingPackLine>().Where(packLine =>
					HasCusEntryNumber(packLine.AdditionalReferenceNumbers.Cast<CusEntryNumber>(),
						ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC,
						pickupCFSCountryCode,
						eventData.GetMatchingDataSource(DataContextType.TransitReceive)?.Key.GetValueOrDefault() ?? ZString.Empty)
					&& HasCusEntryNumber(packLine.PortReferences.Cast<CusEntryNumber>(),
						ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN,
						pickupCFSCountryCode,
						customsReferenceNumber));
		}

		bool IsTransitReceiveMAAEvent(UniversalEvent eventData)
		{
			return ParametersMatchedOnEvent(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, eventData, (NoResString)"Goods Received (CRESA)");
		}

		bool IsTransitReceiveSHLEvent(UniversalEvent eventData)
		{
			return ParametersMatchedOnEvent(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, eventData, (NoResString)"Port Notification Export Status")
				&& ParametersMatchedOnEvent(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, eventData, CargoWise.EventReference.Constants.Facilities.Code.Depot);
		}

		bool IsTransitReceiveSCMEvent(UniversalEvent eventData)
		{
			return ParametersMatchedOnEvent(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, eventData, (NoResString)"Customs")
				&& ParametersMatchedOnEvent(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, eventData, (NoResString)"Export");
		}

		bool HasCusEntryNumber(IEnumerable<CusEntryNumber> number, string type, string countryCode, string entryNum)
		{
			return number.Any(x => x.CE_EntryType == type && x.CE_RN_NKCountryCode == countryCode && x.CE_EntryNum == entryNum);
		}

		bool ParametersMatchedOnEvent(string code, UniversalEvent eventData, string expectedValue)
		{
			return EventParameters.GetEventParameter(code, eventData.EventParameters, eventData.EventReference)
						.GetValueOrDefault().EqualsIgnoringCase(expectedValue);
		}

		void SetPackLinesPortReference(IEnumerable<ForwardingPackLine> packLines, UniversalEvent eventData)
		{
			var issueDate = eventData.EventTime ?? ZDateTimeOffset.Empty;
			var customsReferenceNumber = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, eventData.EventParameters, eventData.EventReference).GetValueOrDefault();
			var issueCountry = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, eventData.EventParameters, eventData.EventReference)
				.GetValueOrDefault()
				.SubstringSafe(0, 2);

			foreach (var packLine in packLines)
			{
				packLine.JL_ExportRefNumber = customsReferenceNumber;

				var panNumber = packLine.PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_RN_NKCountryCode == issueCountry && x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN)
					?? packLine.PortReferences.AddNew() as CusEntryNumber;

				panNumber.CE_RN_NKCountryCode = issueCountry;
				panNumber.CE_EntryNum = customsReferenceNumber;
				panNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN;
				panNumber.CE_IssueDate = issueDate.ToZDateTime();
				panNumber.CE_Category = CusEntryNumber.Categories.PortReferenceNumber;
				panNumber.CE_EntryIsSystemGenerated = true;
			}
		}

		void UpdatePackLinesPANStatus(IEnumerable<ForwardingPackLine> packLines, UniversalEvent eventData, string status = "")
		{
			var issueCountry = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, eventData.EventParameters, eventData.EventReference)
						.GetValueOrDefault()
						.SubstringSafe(0, 2);

			foreach (var packline in packLines)
			{
				var number = packline.PortReferences.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_RN_NKCountryCode == issueCountry && x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN);

				if (number != null)
				{
					number.CE_EntryStatus = status;
				}
			}
		}

		class ForwardingShipmentEventParentFinderHelper : BaseShipmentEventParentFinderHelper
		{
			internal ForwardingShipmentEventParentFinderHelper(BusinessObjectFactory factory, IXmlImportLogger logger, IUniversalFreightHelper helper)
				: base(factory, logger)
			{
				this.helper = helper;
			}

			readonly IUniversalFreightHelper helper;

			internal BusinessObject[] GetLogParentsForEvent(UniversalEvent xmlEvent)
			{
				if (xmlEvent.EventType.GetValueOrDefault() != AutoEvents.SubscriptionRequestedCode ||
					xmlEvent.EventParameters?.Type?.ToString() != (NoResString)"Shipment Visibility")
				{
					return null;
				}

				var forwardingShipmentReference = new ShipmentReferences();
				PopulateShipmentReference(xmlEvent, forwardingShipmentReference, helper);

				IXmlEventValueObject eventValueObject = xmlEvent;
				var query = new ZQuery(RefShippingLineSchema.RSL_CargoWiseOneCode, eventValueObject.Context.CarrierC1CCode.GetValueOrDefault());
				var shippingLine = factory.Load<RefShippingLine>(query).FirstOrDefault();
				if (shippingLine == null || !shippingLine.RSL_IsNVO)
				{
					return null;
				}

				var (bestMatch, reason) = GetLogParent(xmlEvent, forwardingShipmentReference);
				if (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.Value.IsActive)
				{
					GenerateProcessLog(bestMatch, reason, xmlEvent);
				}
				else
				{
					bestMatch = null;
					GenerateProcessLog(bestMatch, Res.GetString("fa53e5d1-a530-49fb-b7a8-39207a31921f", "Tracking is not supported for this Carrier."), xmlEvent);
				}

				return bestMatch != null ? new BusinessObject[] { bestMatch } : null;
			}

			(CommonShipment bo, ZString reason) GetLogParent(IXmlEventValueObject xmlEvent, ShipmentReferences forwardingShipmentReference)
			{
				if (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.Value.IsActive)
				{
					forwardingShipmentReference.HBOLNumber = xmlEvent.Context.MBOLNumber.GetValueOrDefault();
					forwardingShipmentReference.ShipmentID = xmlEvent.Context.CarriersBookingReference;
					forwardingShipmentReference.OriginUNLOCO = xmlEvent.Context.MBOLOriginUNLOCO;
					forwardingShipmentReference.DestinationUNLOCO = xmlEvent.Context.MBOLDestinationUNLOCO;
				}

				var matcher = new ForwardingShipmentMatcher(factory, forwardingShipmentReference, logger, helper, xmlEvent);
				return matcher.GetBestMatchWithReason();
			}

			void GenerateProcessLog(CommonShipment shipment, ZString reason, IXmlEventValueObject xmlEvent)
			{
				if (shipment == null)
				{
					logger.Log(LogType.Information,$"Cannot link Forwarding Shipment because: [* {reason} *]");
				}
				else
				{
					logger.Log(LogType.Information, $"Successfully saved: [*Provider subscription {GetSuccessSubscriptionInfo(shipment, xmlEvent)} was Confirmed. Subscription created*]");
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
					? (NoResString)"for Forwarding Shipment"
					: (NoResString)$"for {matchReference} '{(matchReference == "Master Bill" ? booking.JS_HouseBill : booking.JS_UniqueConsignRef)}'";
				return subscriptionInfo;
			}
		}
	}
}
