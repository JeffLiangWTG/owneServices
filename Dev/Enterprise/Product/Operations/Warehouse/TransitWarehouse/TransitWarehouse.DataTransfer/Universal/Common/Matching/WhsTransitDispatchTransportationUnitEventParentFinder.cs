using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Warehouse.Transit.Business.TransitConstants;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.EventParentFinderHelper;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitDispatchTransportationUnitEventParentFinder : EventParentFinder
	{
		public WhsTransitDispatchTransportationUnitEventParentFinder(WhsItemDispatchTransportationUnitDataContextManager manager, BusinessObjectFactory factory, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalDataBuss.DataObjects.Universal.Event eventDataObject)
		{
			var result = new List<BusinessObject>();

			var eventTypeCode = eventDataObject.EventType;
			if (eventTypeCode.HasValue && IsGateEventCode(eventTypeCode.Value) && RecipientIsATW(eventDataObject) && IsGateEventSource(eventDataObject) && (GetValueFromContextCollection(eventDataObject, GateEventContextType.Direction) == nameof(AddressType.PIC)))
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

		WhsItemDispatchTransportationUnit ProcessBKLEvent(ZString? movementBookingNumber)
		{
			if (!movementBookingNumber.HasValue || movementBookingNumber.Value == ZString.Empty)
			{
				throw new DataObjectReadFailureException("Movement booking number must be provided.");
			}

			var dtu = FindMatchingDTU(nameof(DataContextType.GateMovementBooking), movementBookingNumber.Value);
			if (dtu.GateInTime != ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException($"DTU - '{dtu.WDH_ReferenceNumber}' is already gated into the warehouse.");
			}

			dtu.WDH_IsBookingCancelled = true;
			logger.Log(LogType.Information, ResString.GetMultilingualString("9338958d-421a-4aee-aa49-4f1f44943689", "Canceling booked DTU - '{0}'.", dtu.WDH_ReferenceNumber));
			var pivots = factory.Load<WhsItemDispatchLoadListDTUPivot>(new ZQuery(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit, dtu.PK));
			if (pivots.Length > 0)
			{
				pivots.DeleteAll();
				logger.Log(LogType.Information, ResString.GetMultilingualString("ad90898a-0951-49d4-b432-53f91f1cb689", "Detaching DTU - '{0}' planned DLLs.", dtu.WDH_ReferenceNumber));
			}

			var dtuEventReference = GetDTUEventReference(dtu);
			WhsTransitLogHelper.AddStmALogToBizoObject(dtu, AutoEvents.BookingCancelled, dtuEventReference);

			return dtu;
		}

		#endregion

		#region ProcessGINEvent

		void UpdateDTUForGINEvent(WhsItemDispatchTransportationUnit dtu, ZDateTimeOffset gateInTime)
		{
			logger.Log(LogType.Information, ResString.GetMultilingualString("cd18cca2-c629-4855-b8a2-70e18b342370", "Setting DTU - '{0}' Gate In time to '{1}'.", dtu.WDH_ReferenceNumber, gateInTime));
			dtu.WDH_GateInTime = gateInTime;
			if (dtu.ContainerizedPackageState != null)
			{
				dtu.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.GatedIn;
			}

			var dtuEventReference = GetDTUEventReference(dtu);
			WhsTransitLogHelper.AddStmALogToBizoObject(dtu, AutoEvents.GateIn, dtuEventReference);
		}

		WhsItemDispatchTransportationUnit ProcessGINEvent(UniversalDataBuss.DataObjects.Universal.Event eventDataObject)
		{
			var gateBookingNumber = GetValueFromContextCollection(eventDataObject, GateEventContextType.GateBookingNumber);
			var gateMovementBookingNumber = GetValueFromContextCollection(eventDataObject, GateEventContextType.MovementBookingNumber);

			var dtuWithGateMovementBookingReference = FindMatchingDTU(nameof(DataContextType.GateMovementBooking), gateMovementBookingNumber);
			if (dtuWithGateMovementBookingReference.GateInTime != ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException($"DTU - '{dtuWithGateMovementBookingReference.WDH_ReferenceNumber}' is already gated into the warehouse.");
			}

			var gateInTime = eventDataObject.EventTime ?? ZDateTimeOffset.Now;
			gateInTime = new ZDateTimeOffset(gateInTime.Year, gateInTime.Month, gateInTime.Day, gateInTime.Hour, gateInTime.Minute, 0, 0, gateInTime.Offset);
			UpdateDTUForGINEvent(dtuWithGateMovementBookingReference, gateInTime);

			var dtuWithGateBookingReference = FindMatchingDTU(nameof(DataContextType.GateBooking), gateBookingNumber);
			if (dtuWithGateMovementBookingReference.PK != dtuWithGateBookingReference.PK && dtuWithGateBookingReference.GateInTime == ZDateTimeOffset.Empty)
			{
				UpdateDTUForGINEvent(dtuWithGateBookingReference, gateInTime);
			}

			return dtuWithGateMovementBookingReference;
		}

		#endregion

		#region ProcessGOUEvent

		WhsItemDispatchTransportationUnit ProcessGOUEvent(UniversalDataBuss.DataObjects.Universal.Event eventDataObject)
		{
			var gateBookingNumber = GetValueFromContextCollection(eventDataObject, GateEventContextType.GateBookingNumber);
			var gateMovementBookingNumber = GetValueFromContextCollection(eventDataObject, GateEventContextType.MovementBookingNumber);

			var dtuWithGateMovementBookingReference = FindMatchingDTU(nameof(DataContextType.GateMovementBooking), gateMovementBookingNumber);

			if (dtuWithGateMovementBookingReference.GateInTime == ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException("DTU is not yet gated into the warehouse.");
			}

			if (dtuWithGateMovementBookingReference.WDH_LoadCompleteTime == ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException("DTU is not yet loaded.");
			}

			if (dtuWithGateMovementBookingReference.WDH_GateOutTime != ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException("DTU is already gated out of the warehouse.");
			}

			var dtuWithGateBookingReference = FindMatchingDTU(nameof(DataContextType.GateBooking), gateBookingNumber);

			var isContainerBooking = dtuWithGateMovementBookingReference.PK != dtuWithGateBookingReference.PK;

			if (isContainerBooking && dtuWithGateBookingReference.WDH_LoadCompleteTime == ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException("Vehicle DTU has not completed loading yet.");
			}

			if (isContainerBooking && (dtuWithGateMovementBookingReference.ContainerizedPackageState.WPS_WDH_TransitDispatchHeader.IsEmpty || dtuWithGateMovementBookingReference.ContainerizedPackageState.WPS_WDH_TransitDispatchHeader != dtuWithGateBookingReference.PK))
			{
				throw new DataObjectReadFailureException("Container/ULD is not loaded onto the expected vehicle.");
			}

			var gateOutTime = eventDataObject.EventTime ?? ZDateTimeOffset.Now;
			gateOutTime = new ZDateTimeOffset(gateOutTime.Year, gateOutTime.Month, gateOutTime.Day, gateOutTime.Hour, gateOutTime.Minute, 0, 0, gateOutTime.Offset);

			UpdateGateOutTimeForMatchingDTU(dtuWithGateMovementBookingReference, gateOutTime);

			if (isContainerBooking && dtuWithGateBookingReference.WDH_GateOutTime == ZDateTimeOffset.Empty)
			{
				UpdateGateOutTimeForMatchingDTU(dtuWithGateBookingReference, gateOutTime);
			}

			return dtuWithGateMovementBookingReference;
		}

		void UpdateGateOutTimeForMatchingDTU(WhsItemDispatchTransportationUnit dtu, ZDateTimeOffset gateOutTime)
		{
			logger.Log(LogType.Information, ResString.GetMultilingualString("070c5659-9883-4a0e-b762-91af5b6ec780", "Setting DTU - '{0}' Gate Out time to '{1}'.", dtu.WDH_ReferenceNumber, gateOutTime));
			dtu.WDH_GateOutTime = gateOutTime;

			var dtuEventReference = GetDTUEventReference(dtu);
			WhsTransitLogHelper.AddStmALogToBizoObject(dtu, AutoEvents.GateOut, dtuEventReference, gateOutTime);

			if (dtu.ContainerizedPackageState != null)
			{
				dtu.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.Departed;
			}

			var nonContainerizedInnerPackageStates = dtu.PackageStates.Where(ps => ps.WPS_Status != TransitWarehouseStatuses.Codes.Departed && (ps.WPS_UnitType != PackageStateUnitType.Codes.SeaContainer && ps.WPS_UnitType != PackageStateUnitType.Codes.AirULDContainer));

			foreach (var pkgState in nonContainerizedInnerPackageStates)
			{
				// Package State departed events are set within the WPS_Status Setter
				pkgState.WPS_Status = TransitWarehouseStatuses.Codes.Departed;
			}
		}

		#endregion

		#region ProcessCNCEvent

		void UpdateDTUForCancelGateInEvent(WhsItemDispatchTransportationUnit dtu)
		{
			if (dtu.WDH_GateInTime == ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException("DTU is not yet gated into the warehouse.");
			}
			else if (dtu.WDH_LoadStartTime != ZDateTimeOffset.Empty || dtu.WDH_LoadCompleteTime != ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException("DTU has started to load.");
			}
			else
			{
				dtu.WDH_GateInTime = ZDateTimeOffset.Empty;
				if (dtu.ContainerizedPackageState != null)
				{
					dtu.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.Booked;
				}
				logger.Log(LogType.Information, ResString.GetMultilingualString("59B98C18-2652-448F-B06B-4805108310D5", "Cancel DTU - '{0}' Gate In Succeeded.", dtu.WDH_ReferenceNumber));
				WhsTransitLogHelper.AddStmALogToBizoObject(dtu, AutoEvents.Cancelled, GetDTUEventReference(dtu, AutoEvents.GateInCode));
			}
		}

		void UpdateDTUForCancelGateOutEvent(WhsItemDispatchTransportationUnit dtu)
		{
			if (dtu.WDH_FinalisedTime != ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException("DTU has been finalised.");
			}
			else if (dtu.WDH_GateOutTime == ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException("DTU is not yet gated out of the warehouse.");
			}
			else if (dtu.WDH_GateOutTime < ZDateTimeOffset.Now.AddDays(-30))
			{
				throw new DataObjectReadFailureException("DTU has gated out more than 30 days ago.");
			}
			else
			{
				dtu.WDH_GateOutTime = ZDateTimeOffset.Empty;
				if (dtu.ContainerizedPackageState != null)
				{
					dtu.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
				}
				logger.Log(LogType.Information, ResString.GetMultilingualString("c1a04511-8a2f-4148-905d-3a0d4285a92f", "Cancel DTU - '{0}' Gate Out Succeeded.", dtu.WDH_ReferenceNumber));
				WhsTransitLogHelper.AddStmALogToBizoObject(dtu, AutoEvents.Cancelled, GetDTUEventReference(dtu, AutoEvents.GateOutCode));

				var nonContainerizedInnerPackageStates = dtu.PackageStates.Where(ps => ps.WPS_Status != TransitWarehouseStatuses.Codes.FreightLoaded && !ps.IsContainerizedPackageState);

				foreach (var pkgState in nonContainerizedInnerPackageStates)
				{
					// Package State departed events are set within the WPS_Status Setter
					pkgState.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
				}
			}
		}

		WhsItemDispatchTransportationUnit ProcessCNCEvent(UniversalDataBuss.DataObjects.Universal.Event eventDataObject)
		{
			var cancelEventCode = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EventCode, eventDataObject.EventParameters, eventDataObject.EventReference).GetValueOrDefault();

			if (cancelEventCode == AutoEvents.GateInCode)
			{
				var gateMovementBookingNumber = GetValueFromContextCollection(eventDataObject, GateEventContextType.MovementBookingNumber);
				var containerDTU = FindMatchingDTU(nameof(DataContextType.GateMovementBooking), gateMovementBookingNumber);
				UpdateDTUForCancelGateInEvent(containerDTU);

				var gateBookingNumber = GetValueFromContextCollection(eventDataObject, GateEventContextType.GateBookingNumber);
				var vehicleDTU = FindMatchingDTU(nameof(DataContextType.GateBooking), gateBookingNumber);

				if (vehicleDTU.PK != containerDTU.PK && containerDTU.ContainerizedPackageState != null)
				{
					var vehicleHasOtherContainersGatedIn = vehicleDTU.DispatchLoadLists.Where(dll => dll.WDL_IsActive).SelectMany(dll => dll.PackageStates).Any(ps => ps.PK != containerDTU.ContainerizedPackageState.PK && ps.IsContainerizedPackageState && ps.WPS_Status == TransitWarehouseStatuses.Codes.GatedIn);
					if (!vehicleHasOtherContainersGatedIn)
					{
						UpdateDTUForCancelGateInEvent(vehicleDTU);
					}
				}
				return containerDTU;
			}
			else if (cancelEventCode == AutoEvents.GateOutCode)
			{
				var gateMovementBookingNumber = GetValueFromContextCollection(eventDataObject, GateEventContextType.MovementBookingNumber);
				var containerDTU = FindMatchingDTU(nameof(DataContextType.GateMovementBooking), gateMovementBookingNumber);
				UpdateDTUForCancelGateOutEvent(containerDTU);

				var gateBookingNumber = GetValueFromContextCollection(eventDataObject, GateEventContextType.GateBookingNumber);
				var vehicleDTU = FindMatchingDTU(nameof(DataContextType.GateBooking), gateBookingNumber);

				if (vehicleDTU.PK != containerDTU.PK && containerDTU.ContainerizedPackageState != null)
				{
					var vehicleHasOtherContainersGatedOut = vehicleDTU.PackageStates.Any(ps => ps.PK != containerDTU.ContainerizedPackageState.PK && ps.IsContainerizedPackageState && ps.WPS_Status == TransitWarehouseStatuses.Codes.Departed);
					if (!vehicleHasOtherContainersGatedOut)
					{
						UpdateDTUForCancelGateOutEvent(vehicleDTU);
					}
				}
				return containerDTU;
			}

			return null;
		}

		#endregion

		#region Implementation

		string GetDTUEventReference(WhsItemDispatchTransportationUnit dtu, string eventCode = "")
		{
			var eventReferences = new List<KeyValuePair<string, string>>()
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, CargoWise.EventReference.Constants.Facilities.Code.Depot),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, dtu.Warehouse?.WarehouseAddress?.OA_City ?? string.Empty),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Warehouse, dtu.Warehouse?.WW_WarehouseCode ?? string.Empty),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, dtu.WDH_UnitType == TransportUnitTypes.ULD ? "ULDID" : dtu.WDH_UnitType != TransportUnitTypes.Vehicle ? "ContainerID" : "VehicleReference"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.DeclarationID, dtu.WDH_ReferenceNumber),
			};

			if (eventCode == AutoEvents.GateInCode || eventCode == AutoEvents.GateOutCode)
			{
				eventReferences.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EventCode, eventCode));
			}

			return WhsTransitLogHelper.GetEventReferenceString(eventReferences.ToArray());
		}

		WhsItemDispatchTransportationUnit FindMatchingDTU(ZString dataContextType, ZString dataContextValue)
		{
			return GateMatchingHelper.FindMatchingDTUByJobLink(factory, dataContextType, dataContextValue) ?? throw new DataObjectReadFailureException($"Matching DTU by {dataContextType} - {dataContextValue} not found.");
		}

		#endregion
	}
}
