using System;
using System.Data;
using System.Diagnostics;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.GPS.Business
{
	[DebuggerDisplay("Info: {EN_ActivityInformation} Time: {EN_ActivityTime} Evnt: {EN_EventType} Act: {EN_ActivityType} Stat: {ActivityStatus} PK: {PK}")]
	public class GPSSupporterActivity : AutoLocalCartageVehicleActivity, Integration.GPS.IGPSSupporterActivity
	{
		public GPSSupporterActivity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(EN_ActivityType), ConcurrencyPolicy.Strict);
		}

		public override bool ReadOnly
		{
			get { return true; }
			set { base.ReadOnly = value; }
		}

		public CodeDescriptionPairList Events
		{
			get { return new CodeDescriptionPairList(new GPSConstants.GPSNotificationEventList()); }
		}

		#region ActivityDescription

		public ZString ActivityDescription
		{
			get
			{
				return Events.GetDescriptionFromCode(EN_ActivityType) ?? ZString.Empty;
			}
		}

		public ZPropertyInfo ActivityDescriptionInfo
		{ get { return GetZPropertyInfo(nameof(ActivityDescription)); } }

		#endregion

		#region VehicleShortCode

		public ZString VehicleShortCode
		{
			get
			{
				ZString result = "";
				BusinessObject vehicle = Factory.Load<RefEquipment>(EN_RQ_Vehicle);
				if (vehicle != null)
				{
					result = vehicle[RefEquipmentSchema.RQ_ShortCode.Name].ToString();
				}
				return result;
			}
		}

		public ZPropertyInfo VehicleShortCodeInfo
		{ get { return GetZPropertyInfo(nameof(VehicleShortCode)); } }

		#endregion

		#region ActivityStatus

		public ZString ActivityStatus
		{
			get
			{
				ZString result = EN_ActivityType + " - ";

				if (EN_JU == ZGuid.Empty)
				{
					result = unused;
				}
				else
				{
					var commonCartageLeg = Factory.Load<ICommonCartageLeg>(EN_JU);
					var matchingLegTimeName = GetMatchingLegTimeName(commonCartageLeg);
					if (matchingLegTimeName != ZString.Empty)
					{
						result += matchingLegTimeName;
					}
					else
					{
						var overwrittenDate = GetDateThatThisWouldUpdate();

						GPSSupporterActivity otherActivity = null;
						if (!overwrittenDate.IsEmpty)
						{
							var otherEventQuery = new ZQuery(LocalCartageVehicleActivitySchema.EN_JU, EN_JU);
							otherEventQuery.AddToFilter(LocalCartageVehicleActivitySchema.PK, SQLComparisonOperator.NotEqual, PK);
							otherEventQuery.AddToFilter(LocalCartageVehicleActivitySchema.EN_ActivityType, SQLComparisonOperator.NotEqual, "DC"); // To keep current implementation.
							otherEventQuery.AddToFilter(LocalCartageVehicleActivitySchema.EN_ActivityType, SQLComparisonOperator.NotEqual, "DCM"); // To keep current implementation.
							otherEventQuery.AddToFilter(LocalCartageVehicleActivitySchema.EN_ActivityTime, SQLComparisonOperator.GreaterThanOrEqualTo, overwrittenDate); //Move this to common method AddTimeNoSecondsToFilter(Filter, Time)
							otherEventQuery.AddToFilter(LocalCartageVehicleActivitySchema.EN_ActivityTime, SQLComparisonOperator.LessThan, overwrittenDate.AddMinutes(1)); //Move this to common method AddTimeNoSecondsToFilter(Filter, Time)
							otherEventQuery.OrderBy = LocalCartageVehicleActivitySchema.EN_ActivityTime.Name + " ASC";

							otherActivity = Factory.LoadTop1<GPSSupporterActivity>(otherEventQuery);
						}

						if (otherActivity == null)
						{
							result += overridenManually;
						}
						else
						{
							result += superseededBy + otherActivity.EN_ActivityType;
						}
					}
				}

				return result;
			}
		}

		/// <remarks>
		/// We only know if this a IN or OUT event but we don't know which segment (pickup/delivery) it initially updated.
		/// If we're looking at a CIN event for example, and the leg's pickup in was overwritten manually and the leg's delivery in was overwritten by geofence
		/// we need to know which IN this would've updated (using the activity info) so we can look for other events with the same datetime to see what overwrote it.
		/// </remarks>
		ZDateTime GetDateThatThisWouldUpdate()
		{
			if (IsGeoFenceEvent)
			{
				ZString propertyName = ZString.Empty;
				JobDocAddress pickupDocAdddress = null;
				JobDocAddress waitDocAdddress = null;
				JobDocAddress deliveryDocAdddress = null;

				//DJB: Add JobDocAddresses to interface 
				pickupDocAdddress = Factory.Load<JobDocAddress>(new ZGuid(((BusinessObject)CartageLeg)[JobContainerLegsSchema.JU_E2PickupAddressID]));
				waitDocAdddress = Factory.Load<JobDocAddress>(new ZGuid(((BusinessObject)CartageLeg)[JobContainerLegsSchema.JU_E2WaitPointAddressID]));
				deliveryDocAdddress = Factory.Load<JobDocAddress>(new ZGuid(((BusinessObject)CartageLeg)[JobContainerLegsSchema.JU_E2DeliveryAddressID]));

				if (pickupDocAdddress != null && pickupDocAdddress.Address != null)
				{
					if (EN_ActivityInformation.Contains(pickupDocAdddress.Address.Header.OH_Code + GPSConstants.FenceSeperator + pickupDocAdddress.Address.OA_Code, StringComparison.OrdinalIgnoreCase))
					{
						propertyName = IsInEvent ? JobContainerLegsSchema.JU_PickupTimeIn.Name : JobContainerLegsSchema.JU_PickupTimeOut.Name;
					}
				}
				if (waitDocAdddress != null && waitDocAdddress.Address != null)
				{
					if (EN_ActivityInformation.Contains(waitDocAdddress.Address.Header.OH_Code + GPSConstants.FenceSeperator + waitDocAdddress.Address.OA_Code, StringComparison.OrdinalIgnoreCase))
					{
						propertyName = IsInEvent ? JobContainerLegsSchema.JU_WaitPointTimeIn.Name : JobContainerLegsSchema.JU_WaitPointTimeOut.Name;
					}
				}
				if (deliveryDocAdddress != null && deliveryDocAdddress.Address != null)
				{
					if (EN_ActivityInformation.Contains(deliveryDocAdddress.Address.Header.OH_Code + GPSConstants.FenceSeperator + deliveryDocAdddress.Address.OA_Code, StringComparison.OrdinalIgnoreCase))
					{
						propertyName = IsInEvent ? JobContainerLegsSchema.JU_DeliverTimeIn.Name : JobContainerLegsSchema.JU_DeliverTimeOut.Name;
					}
				}

				return propertyName.IsEmpty ? ZDateTime.Empty : (ZDateTime)((BusinessObject)CartageLeg)[propertyName];
			}

			return ZDateTime.Empty;
		}

		public ZPropertyInfo ActivityStatusInfo
		{ get { return GetZPropertyInfo(nameof(ActivityStatus)); } }

		#endregion

		#region ActivityShortTime

		public ZDateTime ActivityShortTime
		{
			get { return new ZDateTime(EN_ActivityTime.Year, EN_ActivityTime.Month, EN_ActivityTime.Day, EN_ActivityTime.Hour, EN_ActivityTime.Minute, 0); }
		}

		#endregion

		#region CartageLeg

		ICommonCartageLeg CartageLeg
		{
			get
			{
				if (fCartageLeg == null)
				{
					fCartageLeg = Factory.Load<ICommonCartageLeg>(EN_JU);
				}
				return fCartageLeg;
			}
		}

		ICommonCartageLeg fCartageLeg;

		#endregion

		#region Flags

		#region IsGeoFenceEvent

		public ZBool IsGeoFenceEvent
		{
			get
			{
				return EN_ActivityType == GPSConstants.GPSInOutActivityType.Codes.GIN || EN_ActivityType == GPSConstants.GPSInOutActivityType.Codes.GOT;
			}
		}

		#endregion

		#region IsInEvent

		public ZBool IsInEvent
		{
			get
			{
				return EN_ActivityType == GPSConstants.GPSInOutActivityType.Codes.GIN;
			}
		}

		#endregion

		#region IsOutEvent

		public ZBool IsOutEvent
		{
			get
			{
				return EN_ActivityType == GPSConstants.GPSInOutActivityType.Codes.GOT;
			}
		}

		#endregion

		#region IsInOutReplyMessage

		public ZBool IsInOutReplyMessage
		{
			get
			{
				return IsPickupInReplyMessage
				|| IsPickupOutReplyMessage
				|| IsWaitPointInReplyMessage
				|| IsWaitPointOutReplyMessage
				|| IsDeliveryInReplyMessage
				|| IsDeliveryOutReplyMessage;
			}
		}

		#endregion

		#region IsPickupInReplyMessage

		public ZBool IsPickupInReplyMessage
		{
			get { return EN_ActivityInformation.OccurrencesIgnoringCase(GPSConstants.DriverResponses.pickuptimein) > 0; }
		}

		#endregion

		#region IsPickupOutReplyMessage

		public ZBool IsPickupOutReplyMessage
		{
			get { return EN_ActivityInformation.OccurrencesIgnoringCase(GPSConstants.DriverResponses.pickuptimeout) > 0; }
		}

		#endregion

		#region IsWaitPointInReplyMessage

		public ZBool IsWaitPointInReplyMessage
		{
			get { return EN_ActivityInformation.OccurrencesIgnoringCase(GPSConstants.DriverResponses.waitpointtimein) > 0; }
		}

		#endregion

		#region IsWaitPointOutReplyMessage

		public ZBool IsWaitPointOutReplyMessage
		{
			get { return EN_ActivityInformation.OccurrencesIgnoringCase(GPSConstants.DriverResponses.waitpointtimeout) > 0; }
		}

		#endregion

		#region IsDeliveryInReplyMessage

		public ZBool IsDeliveryInReplyMessage
		{
			get { return EN_ActivityInformation.OccurrencesIgnoringCase(GPSConstants.DriverResponses.deliverytimein) > 0; }
		}

		#endregion

		#region IsDeliveryOutReplyMessage

		public ZBool IsDeliveryOutReplyMessage
		{
			get { return EN_ActivityInformation.OccurrencesIgnoringCase(GPSConstants.DriverResponses.deliverytimeout) > 0; }
		}

		#endregion

		#region IsDeliveredToReplyMessage

		public ZBool IsDeliveredToReplyMessage
		{
			get { return EN_ActivityInformation.OccurrencesIgnoringCase(GPSConstants.DriverResponses.deliveredto) > 0; }
		}

		#endregion

		#endregion

		#region Implementation

		string overridenManually
		{
			get { return f_overridenManually ?? (f_overridenManually = Res.GetString("50aa52fd-ebe2-4fe4-85c2-b0417e7bfbf5", "overridden manually")); }
		}
		string f_overridenManually;

		string superseededBy
		{
			get { return f_superseededBy ?? (f_superseededBy = Res.GetString("a3cf7cd8-1bd9-4f79-82e9-957f2aaf5cc5", "superseded by")); }
		}
		string f_superseededBy;

		string pickupTimeIn
		{
			get { return f_pickupTimeIn ?? (f_pickupTimeIn = Res.GetString("ea2fe6fd-bae1-4a39-b409-ec9f81b16338", "Pickup Time In")); }
		}
		string f_pickupTimeIn;
		string pickupTimeOut
		{
			get { return f_pickupTimeOut ?? (f_pickupTimeOut = Res.GetString("6a5e9fd2-385b-4ba1-ae9a-4484a2fbaff6", "Pickup Time Out")); }
		}
		string f_pickupTimeOut;
		string waitPointTimeIn
		{
			get { return f_waitPointTimeIn ?? (f_waitPointTimeIn = Res.GetString("2c14a89b-b3d9-4daa-abd0-42febad79d82", "Wait Point Time In")); }
		}
		string f_waitPointTimeIn;
		string waitPointTimeOut
		{
			get { return f_waitPointTimeOut ?? (f_waitPointTimeOut = Res.GetString("7b638a5b-943d-4457-923f-f9a104aa8eb6", "Wait Point Time Out")); }
		}
		string f_waitPointTimeOut;
		string deliveryTimeIn
		{
			get { return f_deliveryTimeIn ?? (f_deliveryTimeIn = Res.GetString("4bff1634-f592-4649-ac31-9993925abd2c", "Delivery Time In")); }
		}
		string f_deliveryTimeIn;
		string deliveryTimeOut
		{
			get { return f_deliveryTimeOut ?? (f_deliveryTimeOut = Res.GetString("213e6354-0509-4d67-86e3-82653104039d", "Delivery Time Out")); }
		}
		string f_deliveryTimeOut;

		string unused
		{
			get { return f_unused ?? (f_unused = Res.GetString("42e7b7ce-9871-4dba-849b-5abed25744f7", "Unused")); }
		}
		string f_unused;

		#region GetActivityStatus

		ZString GetMatchingLegTimeName(ICommonCartageLeg commonCartageLeg)
		{
			ZString result = ZString.Empty;

			if (commonCartageLeg != null)
			{
				if (ActivityShortTime == GetShortTime(commonCartageLeg.JU_PickupTimeIn))
				{
					result = pickupTimeIn;
				}
				else if (ActivityShortTime == GetShortTime(commonCartageLeg.JU_PickupTimeOut))
				{
					result = pickupTimeOut;
				}
				else if (ActivityShortTime == GetShortTime(commonCartageLeg.JU_WaitPointTimeIn))
				{
					result = waitPointTimeIn;
				}
				else if (ActivityShortTime == GetShortTime(commonCartageLeg.JU_WaitPointTimeOut))
				{
					result = waitPointTimeOut;
				}
				else if (ActivityShortTime == GetShortTime(commonCartageLeg.JU_DeliverTimeIn))
				{
					result = deliveryTimeIn;
				}
				else if (ActivityShortTime == GetShortTime(commonCartageLeg.JU_DeliverTimeOut))
				{
					result = deliveryTimeOut;
				}
			}

			return result;
		}

		ZDateTime GetShortTime(ZDateTime timeWithSeconds)
		{
			if (timeWithSeconds.IsEmpty)
			{
				return timeWithSeconds;
			}
			return new ZDateTime(timeWithSeconds.Year, timeWithSeconds.Month, timeWithSeconds.Day, timeWithSeconds.Hour, timeWithSeconds.Minute, 0);
		}

		#endregion

		#endregion
	}
}
