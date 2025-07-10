using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.GPS.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business.GPS
{
	internal enum InOut
	{
		In = 1,
		Out = 2
	}

	public class GPSEventsHelper
	{
		public GPSEventsHelper(CommonWorkSheet worksheet)
		{
			Worksheet = worksheet;
		}
		CommonWorkSheet Worksheet { get; set; }

		public void PopulateEventLeg(CommonWorkSheet result)
		{
			var query = new ZQuery();

			var startTime = StartTime;
			if (startTime.IsValid)
			{
				query.AddToFilter(JoinCondition.And, LocalCartageVehicleActivitySchema.EN_ActivityTime, SQLComparisonOperator.GreaterThan, startTime);
			}

			var endTime = EndTime;
			if (endTime.IsValid)
			{
				query.AddToFilter(JoinCondition.And, LocalCartageVehicleActivitySchema.EN_ActivityTime, SQLComparisonOperator.LessThan, endTime);
			}

			if (Worksheet.Truck != null)
			{
				query.AddToFilter(JoinCondition.And, LocalCartageVehicleActivitySchema.EN_RQ_Vehicle, Worksheet.Truck.PK);
			}
			else
			{
				query.AddToFilter(JoinCondition.And, LocalCartageVehicleActivitySchema.EN_RQ_Vehicle, ZGuid.Empty);
			}
			query.AddToFilter(JoinCondition.And, LocalCartageVehicleActivitySchema.EN_EventType, GPSConstants.GPSEventTypeList.Codes.Custom);
			query.OrderBy = LocalCartageVehicleActivitySchema.EN_ActivityTime.Name + " ASC";
			result.GPSEvents.RemoveAll();
			GetEventLeg(result, Factory.Load<GPSSupporterActivity>(query));
		}

		ZDateTime EndTime
		{
			get
			{
				ZDateTime result = Worksheet.EY_EndTime;

				foreach (CommonCartageLeg leg in Worksheet.CartageLegs)
				{
					if (!leg.JU_DeliverTimeOut.IsEmpty && leg.JU_DeliverTimeOut > result)
					{
						result = leg.JU_DeliverTimeOut;
					}
				}

				return result;
			}
		}

		ZDateTime StartTime
		{
			get
			{
				ZDateTime result = Worksheet.EY_StartTime;

				foreach (CommonCartageLeg leg in Worksheet.CartageLegs)
				{
					if (!leg.JU_PickupTimeIn.IsEmpty && leg.JU_PickupTimeIn < result)
					{
						result = leg.JU_PickupTimeIn;
					}
				}

				return result;
			}
		}

		void GetEventLeg(CommonWorkSheet result, GPSSupporterActivity[] allActivities)
		{
			foreach (var activity in allActivities)
			{
				AddEvent(result, activity);
			}
		}

		GPSEvent AddEvent(CommonWorkSheet result, GPSSupporterActivity activity)
		{
			CommonCartageLeg leg = null;
			leg = Factory.Load<CommonCartageLeg>(activity.EN_JU);

			var gpsEvent = new GPSEvent();
			if (leg != null)
			{
				gpsEvent.EventLegSequence = leg.JU_RunSheetSequence.ToString();
			}
			else
			{
				gpsEvent.EventLegSequence = "-";
			}
			gpsEvent.EventTime = activity.EN_ActivityTime;
			gpsEvent.EventPK = activity.PK;
			gpsEvent.EventLeg = leg;
			gpsEvent.EventTypeCode = activity.EN_ActivityType;
			gpsEvent.EventShortInfo = activity.EN_ActivityInformation;
			if (gpsEvent.EventShortInfo.Contains(GPSConstants.IDMarkerBegin, StringComparison.Ordinal))
			{
				gpsEvent.EventShortInfo = gpsEvent.EventShortInfo.SubstringSafe(0, gpsEvent.EventShortInfo.IndexOf(GPSConstants.IDMarkerBegin, StringComparison.Ordinal) - 1);
			}
			result.GPSEvents.Add(gpsEvent);

			return gpsEvent;
		}

		BusinessObjectFactory Factory
		{ get { return Worksheet.Factory; } }
	}
}
