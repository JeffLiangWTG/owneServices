using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Warehouse.Transit.Business.TransitConstants;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.EventParentFinderHelper;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitReceiveTransportationUnitEventParentFinder : EventParentFinder
	{
		public WhsTransitReceiveTransportationUnitEventParentFinder(WhsItemReceiveTransportationUnitDataContextManager manager, BusinessObjectFactory factory, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalDataBuss.DataObjects.Universal.Event eventDataObject)
		{
			var result = new List<BusinessObject>();

			var eventTypeCode = eventDataObject.EventType;
			if (eventTypeCode.HasValue && IsGateEventCode(eventTypeCode.Value) && RecipientIsATW(eventDataObject) && IsGateEventSource(eventDataObject) && (GetValueFromContextCollection(eventDataObject, GateEventContextType.Direction) == nameof(AddressType.DLV)))
			{
				switch (eventTypeCode)
				{
					case AutoEvents.GateInCode:
						result.Add(ProcessGINEvent(eventDataObject));
						break;
					case AutoEvents.BookingCancelledCode:
						var movementBookingNumber = eventDataObject.GetMatchingDataSource(DataContextType.GateMovementBooking)?.Key;
						result.Add(ProcessBKLEvent(movementBookingNumber));
						break;
					case AutoEvents.GateOutCode:
						result.Add(ProcessGOUEvent(eventDataObject));
						break;
					case AutoEvents.CancelledCode:
						result.Add(ProcessCNCEvent(eventDataObject));
						break;
				}
			}
			return result.Where(businessObject => businessObject != null).ToArray();
		}

		#region ProcessBKLEvent

		WhsItemReceiveTransportationUnit ProcessBKLEvent(ZString? movementBookingNumber)
		{
			if (!movementBookingNumber.HasValue || movementBookingNumber.Value == ZString.Empty)
			{
				throw new DataObjectReadFailureException("Movement booking number must be provided.");
			}

			var rtu = FindMatchingRTU(nameof(DataContextType.GateMovementBooking), movementBookingNumber.Value);

			if (rtu.GateInTime != ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException($"RTU - '{rtu.WRH_ReferenceNumber}' is already gated into the warehouse.");
			}

			rtu.WRH_IsBookingCancelled = true;
			logger.Log(LogType.Information, ResString.GetMultilingualString("7a77beb4-2ec9-4fa3-8a7b-5edc9a8c9b03", "Canceling booked RTU - '{0}'.", rtu.WRH_ReferenceNumber));
			var pivots = factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit, rtu.PK));
			if (pivots.Length > 0)
			{
				pivots.DeleteAll();
				logger.Log(LogType.Information, ResString.GetMultilingualString("2416f2f3-e8e8-4a67-8658-a8b25f7528d2", "Detaching RTU - '{0}' planned ASNs.", rtu.WRH_ReferenceNumber));
			}
			WhsTransitLogHelper.AddStmALogToBizoObject(rtu, AutoEvents.BookingCancelled, GetRTUEventReference(rtu));
			return rtu;
		}

		#endregion

		#region ProcessGINEvent

		WhsItemReceiveTransportationUnit ProcessGINEvent(UniversalDataBuss.DataObjects.Universal.Event eventDataObject)
		{
			var gateBookingNumber = GetValueFromContextCollection(eventDataObject, GateEventContextType.GateBookingNumber);
			var gateMovementBookingNumber = GetValueFromContextCollection(eventDataObject, GateEventContextType.MovementBookingNumber);
			var dockLocation = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, eventDataObject.EventParameters, eventDataObject.EventReference).GetValueOrDefault();
			if (dockLocation.IsEmpty)
			{
				throw new DataObjectReadFailureException("Dock Location is not provided.");
			}

			var rtuWithGateMovementBookingReference = FindMatchingRTU(nameof(DataContextType.GateMovementBooking), gateMovementBookingNumber);
			if (rtuWithGateMovementBookingReference.GateInTime != ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException($"RTU - '{rtuWithGateMovementBookingReference.WRH_ReferenceNumber}' is already gated into the warehouse.");
			}

			var stagingLocation = factory.LoadTop1<IWhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_LocationString, dockLocation).AddToFilter(WhsLocationViewSchema.WLV_WW_Whs, rtuWithGateMovementBookingReference.WRH_WW_Warehouse)) ?? throw new DataObjectReadFailureException("Matching Dock Location not found.");

			var gateInTime = eventDataObject.EventTime ?? ZDateTimeOffset.Now;
			gateInTime = new ZDateTimeOffset(gateInTime.Year, gateInTime.Month, gateInTime.Day, gateInTime.Hour, gateInTime.Minute, 0, 0, gateInTime.Offset);

			UpdateRTUForGateInEvent(rtuWithGateMovementBookingReference, stagingLocation, gateInTime);

			var rtuWithGateBookingReference = FindMatchingRTU(nameof(DataContextType.GateBooking), gateBookingNumber);
			if (rtuWithGateMovementBookingReference.PK != rtuWithGateBookingReference.PK && rtuWithGateBookingReference.GateInTime == ZDateTimeOffset.Empty)
			{
				UpdateRTUForGateInEvent(rtuWithGateBookingReference, stagingLocation, gateInTime);
			}

			return rtuWithGateMovementBookingReference;
		}

		void UpdateRTUForGateInEvent(WhsItemReceiveTransportationUnit rtu, IWhsLocation stagingLocation, ZDateTimeOffset gateInTime)
		{
			if (rtu.WRH_WL_StagingLocation != stagingLocation.PK)
			{
				if (rtu.WRH_WL_StagingLocation != ZGuid.Empty)
				{
					logger.Log(LogType.Information, ResString.GetMultilingualString("fac170c4-3c41-48a0-b3e3-a9ab2763d8f1", "Updating RTU - '{0}' staging location to '{1}' which is the staging Location in Gate In event.", rtu.WRH_ReferenceNumber, stagingLocation.WLV_LocationString));
				}

				rtu.WRH_WL_StagingLocation = stagingLocation.PK;
				if (rtu.ContainerizedPackageState != null)
				{
					rtu.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.GatedIn;
				}
			}

			logger.Log(LogType.Information, ResString.GetMultilingualString("e2811765-38c5-4b4b-a391-bb4317663c53", "Setting RTU - '{0}' Gate In time to '{1}'.", rtu.WRH_ReferenceNumber, gateInTime));
			WhsTransitLogHelper.AddStmALogToBizoObject(rtu, AutoEvents.GateIn, GetRTUEventReference(rtu), gateInTime);
			rtu.WRH_GateInTime = gateInTime;
		}

		#endregion

		#region ProcessGOUEvent

		WhsItemReceiveTransportationUnit ProcessGOUEvent(UniversalDataBuss.DataObjects.Universal.Event eventDataObject)
		{
			var gateBookingNumber = GetValueFromContextCollection(eventDataObject, GateEventContextType.GateBookingNumber);
			var gateMovementBookingNumber = GetValueFromContextCollection(eventDataObject, GateEventContextType.MovementBookingNumber);

			var rtuWithGateMovementBookingReference = FindMatchingRTU(nameof(DataContextType.GateMovementBooking), gateMovementBookingNumber);

			if (rtuWithGateMovementBookingReference.GateInTime == ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException("RTU is not yet gated into the warehouse.");
			}

			if (rtuWithGateMovementBookingReference.WRH_UnloadCompleteTime == ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException("RTU is not yet unloaded.");
			}

			if (rtuWithGateMovementBookingReference.WRH_GateOutTime != ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException("RTU is already gated out of the warehouse.");
			}

			var rtuWithGateBookingReference = FindMatchingRTU(nameof(DataContextType.GateBooking), gateBookingNumber);

			var isContainerBooking = rtuWithGateMovementBookingReference.PK != rtuWithGateBookingReference.PK;

			if (isContainerBooking && rtuWithGateBookingReference.WRH_UnloadCompleteTime == ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException("Vehicle RTU has not completed unloading yet.");
			}

			var gateOutTime = eventDataObject.EventTime ?? ZDateTimeOffset.Now;
			gateOutTime = new ZDateTimeOffset(gateOutTime.Year, gateOutTime.Month, gateOutTime.Day, gateOutTime.Hour, gateOutTime.Minute, 0, 0, gateOutTime.Offset);

			UpdateGateOutTimeForMatchingRTU(rtuWithGateMovementBookingReference, gateOutTime);

			if (isContainerBooking && rtuWithGateBookingReference.WRH_GateOutTime == ZDateTimeOffset.Empty)
			{
				UpdateGateOutTimeForMatchingRTU(rtuWithGateBookingReference, gateOutTime);
			}

			return rtuWithGateMovementBookingReference;
		}

		void UpdateGateOutTimeForMatchingRTU(WhsItemReceiveTransportationUnit rtu, ZDateTimeOffset gateOutTime)
		{
			logger.Log(LogType.Information, ResString.GetMultilingualString("27f59295-65a4-4f8d-9e9f-5ae3be0540fd", "Setting RTU - '{0}' Gate Out time to '{1}'.", rtu.WRH_ReferenceNumber, gateOutTime));
			rtu.WRH_GateOutTime = gateOutTime;

			var rtuEventReference = GetRTUEventReference(rtu);
			WhsTransitLogHelper.AddStmALogToBizoObject(rtu, AutoEvents.GateOut, rtuEventReference, gateOutTime);

			if (rtu.ContainerizedPackageState != null)
			{
				rtu.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.Departed;
			}
		}

		#endregion

		#region ProcessCNCEvent

		void UpdateRTUForCancelGateInEvent(WhsItemReceiveTransportationUnit rtu)
		{
			if (rtu.WRH_GateInTime == ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException("RTU is not yet gated into the warehouse.");
			}
			else if (rtu.WRH_UnloadStartTime != ZDateTimeOffset.Empty || rtu.WRH_UnloadCompleteTime != ZDateTimeOffset.Empty || rtu.WRH_UnloadCompleteNotYetProcessedTime != ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException("RTU has started to unload.");
			}
			else
			{
				rtu.WRH_GateInTime = ZDateTimeOffset.Empty;
				rtu.WRH_WL_StagingLocation = ZGuid.Empty;
				if (rtu.ContainerizedPackageState != null)
				{
					rtu.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.Booked;
				}

				logger.Log(LogType.Information, ResString.GetMultilingualString("FCA3970C-2238-454B-BD2E-556574FA3D44", "Cancel RTU - '{0}' Gate In Succeeded.", rtu.WRH_ReferenceNumber));
				WhsTransitLogHelper.AddStmALogToBizoObject(rtu, AutoEvents.Cancelled, GetRTUEventReference(rtu, AutoEvents.GateInCode));
			}
		}

		void UpdateRTUForCancelGateOutEvent(WhsItemReceiveTransportationUnit rtu)
		{
			if (rtu.WRH_GateOutTime == ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException("RTU is not yet gated out of the warehouse.");
			}
			else if (rtu.WRH_GateOutTime < ZDateTimeOffset.Now.AddDays(-30))
			{
				throw new DataObjectReadFailureException("RTU has gated out more than 30 days ago.");
			}
			else
			{
				rtu.WRH_GateOutTime = ZDateTimeOffset.Empty;
				if (rtu.ContainerizedPackageState != null)
				{
					rtu.ContainerizedPackageState.WPS_Status = rtu.PackageStates.Any() ? TransitWarehouseStatuses.Codes.Unpacked : TransitWarehouseStatuses.Codes.ArrivedPacked;
				}

				logger.Log(LogType.Information, ResString.GetMultilingualString("8ca8e246-c8c2-48ee-8803-fc32ea62624e", "Cancel RTU - '{0}' Gate Out Succeeded.", rtu.WRH_ReferenceNumber));
				WhsTransitLogHelper.AddStmALogToBizoObject(rtu, AutoEvents.Cancelled, GetRTUEventReference(rtu, AutoEvents.GateOutCode));
			}
		}

		BusinessObject ProcessCNCEvent(UniversalDataBuss.DataObjects.Universal.Event eventDataObject)
		{
			var cancelEventCode = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EventCode, eventDataObject.EventParameters, eventDataObject.EventReference).GetValueOrDefault();

			if (cancelEventCode == AutoEvents.GateInCode)
			{
				var gateMovementBookingNumber = GetValueFromContextCollection(eventDataObject, GateEventContextType.MovementBookingNumber);
				var containerRTU = FindMatchingRTU(nameof(DataContextType.GateMovementBooking), gateMovementBookingNumber);
				UpdateRTUForCancelGateInEvent(containerRTU);

				var gateBookingNumber = GetValueFromContextCollection(eventDataObject, GateEventContextType.GateBookingNumber);
				var vehicleRTU = FindMatchingRTU(nameof(DataContextType.GateBooking), gateBookingNumber);

				if (containerRTU.PK != vehicleRTU.PK && containerRTU.ContainerizedPackageState != null)
				{
					var hasOtherContainerGatedIn = vehicleRTU.ReceiveASNsPackageStates.Any(ps => ps.PK != containerRTU.ContainerizedPackageState.PK && ps.IsContainerizedPackageState && ps.WPS_Status == TransitWarehouseStatuses.Codes.GatedIn);
					if (!hasOtherContainerGatedIn)
					{
						UpdateRTUForCancelGateInEvent(vehicleRTU);
					}
				}

				return containerRTU;
			}
			else if (cancelEventCode == AutoEvents.GateOutCode)
			{
				var gateMovementBookingNumber = GetValueFromContextCollection(eventDataObject, GateEventContextType.MovementBookingNumber);
				var containerRTU = FindMatchingRTU(nameof(DataContextType.GateMovementBooking), gateMovementBookingNumber);
				UpdateRTUForCancelGateOutEvent(containerRTU);

				var gateBookingNumber = GetValueFromContextCollection(eventDataObject, GateEventContextType.GateBookingNumber);
				var vehicleRTU = FindMatchingRTU(nameof(DataContextType.GateBooking), gateBookingNumber);

				if (containerRTU.PK != vehicleRTU.PK && containerRTU.ContainerizedPackageState != null)
				{
					var hasOtherContainerDeparted = vehicleRTU.PackageStates.Any(ps => ps.PK != containerRTU.ContainerizedPackageState.PK && ps.IsContainerizedPackageState && ps.WPS_Status == TransitWarehouseStatuses.Codes.Departed);
					if (!hasOtherContainerDeparted)
					{
						UpdateRTUForCancelGateOutEvent(vehicleRTU);
					}
				}

				return containerRTU;
			}

			return null;
		}

		#endregion

		#region Implementation

		string GetRTUEventReference(WhsItemReceiveTransportationUnit rtu, string eventCode = "")
		{
			var eventReferences = new List<KeyValuePair<string, string>>()
			{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, CargoWise.EventReference.Constants.Facilities.Code.Depot),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, rtu.Warehouse?.WarehouseAddress?.OA_City ?? string.Empty),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Warehouse, rtu.Warehouse?.WW_WarehouseCode ?? string.Empty),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, rtu.WRH_UnitType == TransportUnitTypes.ULD ? "ULDID" : rtu.WRH_UnitType != TransportUnitTypes.Vehicle ? "ContainerID" : "VehicleReference"),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.DeclarationID, rtu.WRH_VehicleReference),
			};
			if (eventCode == AutoEvents.GateInCode || eventCode == AutoEvents.GateOutCode)
			{
				eventReferences.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EventCode, eventCode));
			}
			return WhsTransitLogHelper.GetEventReferenceString(eventReferences.ToArray());
		}

		WhsItemReceiveTransportationUnit FindMatchingRTU(ZString dataContextType, ZString dataContextValue)
		{
			return GateMatchingHelper.FindMatchingRTUByJobLink(factory, dataContextType, dataContextValue) ?? throw new DataObjectReadFailureException($"Matching RTU by {dataContextType} - {dataContextValue} not found.");
		}

		#endregion
	}
}
