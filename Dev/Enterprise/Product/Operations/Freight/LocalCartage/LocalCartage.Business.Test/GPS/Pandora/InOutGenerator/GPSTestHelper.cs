using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.GPS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business.GPS.Testing
{
	public class GPSTestHelper : LocalCartageTestHelper
	{
		public void SetCurrentLeg(CommonCartageLeg leg)
		{
			SetAsCurrentLegAndBumpOthers(leg);
		}

		public void GroupLegWithCurrentLeg(CommonCartageLeg leg)
		{
			SetAsCurrentLegAndBumpOthers(leg, groupWithCurrent: true);
		}

		/// <summary>
		/// if 'group with current', bumps those after current sequence, otherwise bumps the current legs too.
		/// </summary>
		void SetAsCurrentLegAndBumpOthers(CommonCartageLeg legToMove, bool groupWithCurrent = false)
		{
			if (legToMove.IsComplete)
			{
				return;
			}

			var currentLegSequence = GetCurrentSequence(legToMove.WorkSheet);
			if (legToMove.JU_RunSheetSequence != currentLegSequence)
			{
				int bumpFrom = groupWithCurrent ? currentLegSequence + 1 : currentLegSequence;
				var legsToBump = legToMove.WorkSheet.CartageLegs.Where(l => l.PK != legToMove.PK && l.JU_RunSheetSequence >= bumpFrom);
				var startingSequence = currentLegSequence + 1;
				SequenceLegs(legsToBump, startingSequence);
				legToMove.JU_RunSheetSequence = currentLegSequence;
			}
		}

		/// <summary>
		/// Sequence Legs from startingSequence.
		/// Keep grouped legs (same sequence) grouped.
		/// </summary>
		void SequenceLegs(IEnumerable<CommonCartageLeg> legsToSequence, int startingSequence)
		{
			var sequence = startingSequence;
			var previousLegSequence = -1;
			foreach (var leg in legsToSequence)
			{
				var isGroupedWithPrevious = (leg.JU_RunSheetSequence == previousLegSequence);
				if (isGroupedWithPrevious)
				{
					leg.JU_RunSheetSequence = sequence;
				}
				else
				{
					previousLegSequence = leg.JU_RunSheetSequence;
					leg.JU_RunSheetSequence = sequence;
					sequence++;
				}
			}
		}

		int GetCurrentSequence(CommonWorkSheet runSheet)
		{
			var lowestLegSequence = GetOrderedAvailableLegs(runSheet).Select(l => l.JU_RunSheetSequence).FirstOrDefault();
			return Math.Max(lowestLegSequence, 1); // 1 is the lowest possible sequence
		}

		static IEnumerable<CommonCartageLeg> GetOrderedAvailableLegs(CommonWorkSheet runSheet)
		{
			return runSheet.CartageLegs.Where(l => !l.IsComplete).OrderBy(l => l.JU_RunSheetSequence);
		}

		public ZString GetLegStatus(CommonCartageLeg leg)
		{
			if (!leg.JU_DeliverTimeOut.IsEmpty)
			{
				return "Delivered"; // grey or another green
			}
			else if (!leg.JU_DeliverTimeIn.IsEmpty)
			{
				return "Delivering"; // green
			}
			else if (leg.JU_E2WaitPointAddressID.IsValid && !leg.JU_WaitPointTimeOut.IsEmpty)
			{
				return "WaitPoint Complete?"; // dark Orange?
			}
			else if (leg.JU_E2WaitPointAddressID.IsValid && !leg.JU_WaitPointTimeIn.IsEmpty)
			{
				return "Waiting?"; // light Orange?
			}
			else if (!leg.JU_PickupTimeOut.IsEmpty)
			{
				return "Picked Up"; // dark blue
			}
			else if (!leg.JU_PickupTimeIn.IsEmpty)
			{
				return "Picking Up"; // light blue
			}
			else
			{
				return "Not Started"; // white
			}
		}

		public GPSTestHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		public GPSTestHelper(BusinessObjectFactory factory, RefEquipment truck) : base(factory)
		{
			Truck = truck;
		}

		RefEquipment Truck { get; set; }

		ZDateTime LastEventTime { get; set; }

		public RefEquipment Truck1
		{
			get
			{
				return truck1 ?? (truck1 = CreateTruck("Truck1"));
			}
		}

		RefEquipment truck1;

		public RefEquipment Truck2
		{
			get
			{
				return truck2 ?? (truck2 = CreateTruck("Truck2"));
			}
		}

		RefEquipment truck2;

		public RefEquipment Truck3
		{
			get
			{
				return truck3 ?? (truck3 = CreateTruck("Truck3"));
			}
		}

		RefEquipment truck3;

		public GPSSupporterActivity[] GetInOutEventsForTruck(RefEquipment vehicle)
		{
			ZQuery query = new ZQuery(LocalCartageVehicleActivitySchema.EN_EventType, GPSConstants.GPSEventTypeList.Codes.Custom);
			query.AddToFilter(LocalCartageVehicleActivitySchema.EN_RQ_Vehicle, vehicle.PK);
			query.OrderBy = "EN_ActivityTime ASC";
			return Factory.Load<GPSSupporterActivity>(query);
		}

		public CommonCartageLeg CreateAndDispatchLegWithPickupDeliveryTime(OrgHeader fromAddress, OrgHeader waitAddress, OrgHeader toAddress, CommonWorkSheet workSheet, ZDateTime plannedPickupAndDispatchedTime, ZGuid truckPK)
		{
			var waitPointAddress = waitAddress != null ? waitAddress.MainAddress : null;
			var deliveryAddress = toAddress != null ? toAddress.MainAddress : null;
			var leg = CreateCartage(fromAddress.MainAddress, waitPointAddress, deliveryAddress, workSheet, truckPK).CartageLegs[0];
			leg.JU_PlannedPickupTime = plannedPickupAndDispatchedTime;
			return leg;
		}

		public CommonCartage CreateCartage(OrgAddress fromAddress, OrgAddress waitAddress, OrgAddress toAddress, CommonWorkSheet worksheet, ZGuid truckPK)
		{
			var cartage = Factory.New<CommonCartage>();
			var pickup = cartage.DocAddresses.AddNew(fromAddress, DocAddressType.LocalCartageCTO);
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			leg.JU_E2PickupAddressID = pickup.PK;
			if (toAddress != null)
			{
				var delivery = cartage.DocAddresses.AddNew(toAddress, DocAddressType.LocalCartageYard);
				leg.JU_E2DeliveryAddressID = delivery.PK;
			}

			if (waitAddress != null)
			{
				var waitpoint = cartage.DocAddresses.AddNew(waitAddress, DocAddressType.LocalCartageImporter);
				leg.JU_E2WaitPointAddressID = waitpoint.PK;
			}

			worksheet.EY_RQ_Truck = truckPK;
			leg.JU_EY_RunSheet = worksheet.PK;
			return cartage;
		}

		public CommonWorkSheet CreateRunSheet(RefEquipment vehicle)
		{
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.EY_RQ_Truck = vehicle.PK;
			return runSheet;
		}

		public CommonWorkSheet CreateRunSheet(RefEquipment vehicle, ZDateTime start, ZDateTime end)
		{
			var helper = new GPSTestHelper(Factory);
			var runSheet = helper.CreateRunSheet(vehicle);
			runSheet.EY_StartTime = start;
			runSheet.EY_EndTime = end;
			return runSheet;
		}

		public GPSSupporterActivity CreateActivity(string en_activityInfo, ZDateTime eventTime, string eventType, ZGuid truckPK)
		{
			LastEventTime = eventTime;
			return CreateActivity(en_activityInfo, eventTime, GPSConstants.GPSEventTypeList.Codes.Custom, eventType, truckPK);
		}

		public GPSSupporterActivity CreateActivity(string en_activityInfo, ZDateTime eventTime, string eventType)
		{
			if (Truck == null)
			{
				throw new NotSupportedException("use the ctor with truck");
			}

			LastEventTime = eventTime;
			return CreateActivity(en_activityInfo, eventTime, eventType, GPSConstants.GPSEventTypeList.Codes.Custom, Truck.PK);
		}

		public GPSSupporterActivity CreateNextActivity(string en_ActivityInfo, string eventType)
		{
			if (Truck == null)
			{
				throw new NotSupportedException("use the ctor with truck");
			}

			return CreateActivity(en_ActivityInfo, LastEventTime.AddMinutes(1), eventType);
		}

		public GPSSupporterActivity CreateActivity(ZString eN_ActivityInformation, ZDateTime eN_ActivityTime, ZString eN_ActivityType, ZString eN_EventType, ZGuid eN_RQ_Vehicle)
		{
			return CreateActivity(GetActivityID(), eN_ActivityInformation, eN_ActivityTime, eN_ActivityType, eN_EventType, ZGuid.Empty, eN_RQ_Vehicle);
		}

		public GPSSupporterActivity CreateActivity(ZString eN_ActivityInformation, ZString eN_ActivityType, ZString eN_EventType, ZGuid eN_JU, ZGuid eN_RQ_Vehicle)
		{
			return CreateActivity(eN_ActivityInformation, ZDateTime.Now, eN_ActivityType, eN_EventType, eN_JU, eN_RQ_Vehicle);
		}

		public GPSSupporterActivity CreateActivity(ZString eN_ActivityInformation, ZDateTime eN_ActivityTime, ZString eN_ActivityType, ZString eN_EventType, ZGuid eN_JU, ZGuid eN_RQ_Vehicle)
		{
			return CreateActivity(GetActivityID(), eN_ActivityInformation, eN_ActivityTime, eN_ActivityType, eN_EventType, eN_JU, eN_RQ_Vehicle);
		}

		public GPSSupporterActivity CreateCustomActivity(OrgAddress address, ZDateTime time, RefEquipment vehicle)
		{
			string siteName = Factory.Load<OrgHeader>(address.OA_OH).OH_Code + GPSConstants.FenceSeperator + address.OA_Code;
			return CreateActivity(GPSTestHelper.GetActivityID(), siteName, time, GPSConstants.GPSInOutActivityType.Codes.GIN, GPSConstants.GPSEventTypeList.Codes.Custom, ZGuid.Empty, vehicle.PK);
		}

		public GPSSupporterActivity CreateActivity(ZString eN_ActivityID, ZString eN_ActivityInformation, ZDateTime eN_ActivityTime, ZString eN_ActivityType, ZString eN_EventType, ZGuid eN_JU, ZGuid eN_RQ_Vehicle)
		{
			var activity = Factory.New<GPSSupporterActivity>();
			activity.EN_ActivityID = eN_ActivityID;
			activity.EN_ActivityInformation = eN_ActivityInformation;
			activity.EN_ActivityTime = eN_ActivityTime;
			activity.EN_ActivityType = eN_ActivityType;
			activity.EN_EventType = eN_EventType;
			activity.EN_JU = eN_JU;
			activity.EN_RQ_Vehicle = eN_RQ_Vehicle;
			return activity;
		}

		public OrgHeader CreateOrgWithAddress(ZString code, ZString addressCode, ZDecimal latitude, ZDecimal longitude)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = code;
			org.OH_FullName = code + " PTY LTD";
			var address = org.MainAddress;
			address.OA_GeoLocation = ZGeography.CreatePoint((double)longitude, (double)latitude);
			address.OA_Address1 = addressCode + " ST";
			address.OA_Code = addressCode;
			return org;
		}

		public OrgHeader CreateOrgHeaderWithAddress()
		{
			return CreateOrgHeaderWithAddress("org", "adr");
		}

		public OrgHeader CreateOrgHeader(ZString oH_Code)
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_Code = oH_Code;
			return header;
		}

		public OrgHeader CreateOrgHeaderWithAddress(ZString oH_Code, ZString oA_Code)
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, oH_Code));
			if (header == null)
			{
				header = Factory.New<OrgHeader>();
				header.OH_Code = oH_Code;
				header.MainAddress.OA_Code = oA_Code;
				header.MainAddress.OA_Address1 = oA_Code;
			}

			return header;
		}

		public OrgHeader Org1
		{
			get
			{
				return org1 ?? (org1 = CreateOrgHeaderWithAddressID(1));
			}
		}

		OrgHeader org1;

		public ZString Org1Code
		{
			get
			{
				return GetOrgAddressCode(Org1);
			}
		}

		public OrgHeader Org2
		{
			get
			{
				return org2 ?? (org2 = CreateOrgHeaderWithAddressID(2));
			}
		}

		OrgHeader org2;

		public ZString Org2Code
		{
			get
			{
				return GetOrgAddressCode(Org2);
			}
		}

		public OrgHeader Org3
		{
			get
			{
				return org3 ?? (org3 = CreateOrgHeaderWithAddressID(3));
			}
		}

		OrgHeader org3;

		public ZString Org3Code
		{
			get
			{
				return GetOrgAddressCode(Org3);
			}
		}

		public OrgHeader Org4
		{
			get
			{
				return org4 ?? (org4 = CreateOrgHeaderWithAddressID(4));
			}
		}

		OrgHeader org4;

		public ZString Org4Code
		{
			get
			{
				return GetOrgAddressCode(Org4);
			}
		}

		public OrgHeader Org5
		{
			get
			{
				return org5 ?? (org5 = CreateOrgHeaderWithAddressID(5));
			}
		}

		OrgHeader org5;

		public ZString Org5Code
		{
			get
			{
				return GetOrgAddressCode(Org5);
			}
		}

		public OrgHeader Org6
		{
			get
			{
				return org6 ?? (org6 = CreateOrgHeaderWithAddressID(6));
			}
		}

		OrgHeader org6;

		public ZString Org6Code
		{
			get
			{
				return GetOrgAddressCode(Org6);
			}
		}

		public ZString GetOrgAddressCode(OrgHeader org)
		{
			return org.OH_Code + " - " + org.MainAddress.OA_Code;
		}

		OrgHeader CreateOrgHeaderWithAddressID(int number)
		{
			return CreateOrgHeaderWithAddress("ORG" + number, "ADR" + number);
		}

		public GPSSupporterActivity[] GetInOutEvents(RefEquipment vehicle)
		{
			var query = new ZQuery(LocalCartageVehicleActivitySchema.EN_EventType, GPSConstants.GPSEventTypeList.Codes.Custom);
			query.AddToFilter(LocalCartageVehicleActivitySchema.EN_ActivityType, new GPSConstants.GPSInOutActivityType().ToArray().Select(l => l.Code));
			query.AddToFilter(LocalCartageVehicleActivitySchema.EN_RQ_Vehicle, vehicle.PK);
			query.OrderBy = LocalCartageVehicleActivitySchema.EN_ActivityTime.Name + ", " + LocalCartageVehicleActivitySchema.EN_ActivityID.Name + " ASC";
			var inOuts = Factory.Load<GPSSupporterActivity>(query);
			inOuts.StableSort(new GPSCustomEventInOutOrderer(GPSCustomEventInOutOrderer.Direction.EarliestFirst));
			return inOuts;
		}

		public static ZString GetActivityID()
		{
			return ((ZString)ZDateTime.Now.Ticks.ToString().TrimEnd('0')).SubstringSafe(((ZString)ZDateTime.Now.Ticks.ToString().TrimEnd('0')).Length - 10).SubstringSafe(0, 10);
		}

		//Order
		internal class GPSCustomEventInOutOrderer : IComparer<GPSSupporterActivity>
		{
			public enum Direction
			{
				EarliestFirst,
				LatestFirst,
			}

			public GPSCustomEventInOutOrderer(Direction direction)
			{
				this.direction = direction;
			}

			readonly Direction direction;

			int DirectionMultiplier
			{
				get
				{
					return direction == Direction.EarliestFirst ? 1 : -1;
				}
			}

			public int Compare(GPSSupporterActivity x, GPSSupporterActivity y)
			{
				var result = x.EN_ActivityTime.CompareTo(y.EN_ActivityTime);
				if (result == 0)
				{
					result = x.EN_ActivityID.CompareTo(y.EN_ActivityID);
				}

				if (result == 0)
				{
					if (x.EN_ActivityType.ToString().Contains("OT") && y.EN_ActivityType.ToString().Contains("IN"))
					{
						result = -1;
					}
					else if (x.EN_ActivityType.ToString().Contains("IN") && y.EN_ActivityType.ToString().Contains("OT"))
					{
						result = 1;
					}
				}

				return result * DirectionMultiplier;
			}
		}
	}
}
