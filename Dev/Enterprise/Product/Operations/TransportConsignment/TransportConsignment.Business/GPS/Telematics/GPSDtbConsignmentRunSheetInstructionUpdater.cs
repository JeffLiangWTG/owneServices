using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Telematics.Integration;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public sealed class GPSDtbConsignmentRunSheetInstructionUpdater : IGPSDtbConsignmentRunSheetInstructionUpdater
	{
		const int MinimumGeofenceRadiusInMeters = 100;

		readonly BusinessObjectFactory businessObjectFactory;

		public GPSDtbConsignmentRunSheetInstructionUpdater(BusinessObjectFactory businessObjectFactory)
		{
			this.businessObjectFactory = businessObjectFactory;
		}

		public void ProcessLocation(IDeviceLocationWithEntity location)
		{
			var measurementTimeOffset = location.MeasurementTimeUtc.ToOffset();
			var runSheets = GetRunSheets(location, measurementTimeOffset);

			if (runSheets?.Length > 0)
			{
				ProcessLocation(runSheets, location, measurementTimeOffset);
			}
		}

		void ProcessLocation(IEnumerable<DtbConsignmentRunSheet> runSheets, IDeviceLocationWithEntity location, ZDateTimeOffset eventTimeOffset)
		{
			foreach (var runSheet in runSheets)
			{
				ProcessLocation(runSheet, location, eventTimeOffset);
			}
		}

		void ProcessLocation(DtbConsignmentRunSheet runSheet, IDeviceLocationWithEntity location, ZDateTimeOffset eventTimeOffset)
		{
			var currentInstruction = GetNextNonCompletedtInstruction(runSheet);
			if (currentInstruction == null)
			{
				return;
			}

			var hasNoTimeIn = currentInstruction.K1_TimeIn.IsEmpty;

			if (hasNoTimeIn || currentInstruction.IsInProgress)
			{
				var isInCurrentAddress = IsEventWithinAddress(location, currentInstruction);
				Event gateEvent = null;

				if (hasNoTimeIn && isInCurrentAddress && !currentInstruction.Logs.Find(log => log.Event.SE_Code == Events.GateIn.Code).Any())
				{
					gateEvent = Events.GateIn;
				}
				else if (currentInstruction.IsInProgress && !isInCurrentAddress && !currentInstruction.Logs.Find(log => log.Event.SE_Code == Events.GateOut.Code).Any())
				{
					gateEvent = Events.GateOut;
				}

				if (gateEvent != null)
				{
					currentInstruction.Logs.AddNew(gateEvent, GetEventReference(currentInstruction), eventTimeOffset);
				}
			}
		}

		ZString GetEventReference(DtbConsignmentRunSheetInstruction instruction)
		{
			var addressCode = instruction.Actions[0].ConsignmentAddress.Address.Address?.OA_Code ?? ZString.Empty;
			var organisationCode = instruction.Actions[0].ConsignmentAddress.Address.Address?.Header?.OH_Code ?? ZString.Empty;

			return ZString.Format("{0} - {1}", organisationCode, addressCode);
		}

		ZBool IsEventWithinAddress(IDeviceLocation location, DtbConsignmentRunSheetInstruction currentInstruction)
		{
			var geoFencePolygon = GetGeofencePolygon(currentInstruction);

			return geoFencePolygon != ZGeography.Empty
				? IsEventWithinAddressGeofence(location, geoFencePolygon)
				: IsEventWithinAddressRadius(location, currentInstruction);
		}

		ZGeography GetGeofencePolygon(DtbConsignmentRunSheetInstruction instruction)
		{
			return instruction.Actions[0].ConsignmentAddress.Address.E2_AddressOverride ? ZGeography.Empty : (instruction.Actions[0].ConsignmentAddress.Address.Address?.OA_GeofencePolygon ?? ZGeography.Empty);
		}

		ZBool IsEventWithinAddressRadius(IDeviceLocation location, DtbConsignmentRunSheetInstruction instruction)
		{
			var distanceBetweenAddressAndEvent = CalculateDistanceInMeters(instruction, location);

			return distanceBetweenAddressAndEvent <= MinimumGeofenceRadiusInMeters;
		}

		ZBool IsEventWithinAddressGeofence(IDeviceLocation location, ZGeography geoFencePolygon)
		{
			return geoFencePolygon.Intersects(ZGeography.CreatePoint(location.Location.Longitude.Value, location.Location.Latitude.Value));
		}

		ZDecimal CalculateDistanceInMeters(DtbConsignmentRunSheetInstruction instruction, IDeviceLocation location)
		{
			(var latitude, var longitude) = GetLatitudeLongitude(instruction);

			return CalculateDistanceInMeters(latitude, longitude, location.Location.Latitude.Value, location.Location.Longitude.Value);
		}

		(ZDecimal Latitude, ZDecimal Longitude) GetLatitudeLongitude(DtbConsignmentRunSheetInstruction instruction)
		{
			var docAddress = instruction.Actions[0].ConsignmentAddress.Address;
			var latitude = ZGeography.NormalizeLatitudeDegree(docAddress?.E2_Latitude ?? ZDecimal.Zero);
			var longitude = ZGeography.NormalizeLongitudeDegree(docAddress?.E2_Longitude ?? ZDecimal.Zero);

			return (latitude, longitude);
		}

		ZDecimal CalculateDistanceInMeters(ZDecimal firstLatitude, ZDecimal firstLongitude, double secondLatitude, double secondLongitude)
		{
			var distanceInKilometers = RefLatLongPostcode.CalculateDistance((double)firstLatitude, (double)firstLongitude, secondLatitude, secondLongitude);

			return distanceInKilometers * 1000;
		}

		DtbConsignmentRunSheetInstruction GetNextNonCompletedtInstruction(DtbConsignmentRunSheet runSheet)
		{
			return runSheet.RunSheetInstructions.Where(instruction => instruction.K1_TimeOut.IsEmpty).OrderBy(instruction => instruction.K1_Sequence).FirstOrDefault();
		}

		DtbConsignmentRunSheet[] GetRunSheets(IDeviceLocationWithEntity location, ZDateTimeOffset eventTimeOffset)
		{
			var runSheetsQuery = new ZQuery();

			if (location.EntityTableCode == RefEquipmentSchema.Constants.Prefix)
			{
				runSheetsQuery.AddToFilter(DtbConsignmentRunSheetSchema.KG_RQ_Truck, location.EntityId);
			}
			else if (location.EntityTableCode == GlbStaffSchema.Constants.Prefix)
			{
				var driver = businessObjectFactory.Load<GlbStaff>(location.EntityId);
				if (driver != null)
				{
					runSheetsQuery.AddToFilter(new ZQuery(DtbConsignmentRunSheetSchema.KG_GS_NKTruckDriver, driver.GS_Code));
				}
			}
			else
			{
				return Array.Empty<DtbConsignmentRunSheet>();
			}
			var runSheetTimeFilter = GetWorkingAndCompletedRunSheetFilter(eventTimeOffset);

			runSheetsQuery.AddToFilter(runSheetTimeFilter, JoinCondition.And);

			return businessObjectFactory.Load<DtbConsignmentRunSheet>(runSheetsQuery);
		}

		ZQuery GetWorkingAndCompletedRunSheetFilter(ZDateTimeOffset eventTimeOffset)
		{
			var workingRunSheetQuery = new ZQuery(DtbConsignmentRunSheetSchema.KG_ActualEndTime, SQLComparisonOperator.IsBlank, null);
			workingRunSheetQuery.AddToFilter(DtbConsignmentRunSheetSchema.KG_ActualStartTime, SQLComparisonOperator.LessThanOrEqualTo, eventTimeOffset);

			var completedRunSheetQuery = new ZQuery(DtbConsignmentRunSheetSchema.KG_ActualEndTime, SQLComparisonOperator.GreaterThanOrEqualTo, eventTimeOffset);
			completedRunSheetQuery.AddToFilter(DtbConsignmentRunSheetSchema.KG_ActualStartTime, SQLComparisonOperator.LessThanOrEqualTo, eventTimeOffset);

			var runSheetTimeFilter = new ZQuery(workingRunSheetQuery, JoinCondition.Or, completedRunSheetQuery);
			return runSheetTimeFilter;
		}
	}
}
