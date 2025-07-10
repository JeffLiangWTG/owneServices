using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.GPS.Business
{
	public class GPSSupporterActivityCollection : DependentBusinessObjectCollection<GPSSupporterActivity, BusinessObject>
	{
		public GPSSupporterActivityCollection(BusinessObject businessObject)
			: base(businessObject)
		{
			if (!(businessObject is RefEquipment))
			{
				CartageLegPK = businessObject.PK;
				ActivityFilterDateFrom = ZDateTime.MinSmallDateTimeValue;
				ActivityFilterDateTo = ZDateTime.MaxSmallDateTime;
			}
		}

		public GPSSupporterActivityCollection(ICommonCartageLeg leg)
			: base((BusinessObject)leg)
		{
			CartageLegPK = GetPKFromICommonCartageLeg(leg);
			ActivityFilterDateFrom = ZDateTime.MinSmallDateTimeValue;
			ActivityFilterDateTo = ZDateTime.MaxSmallDateTime;
		}

		ZGuid GetPKFromICommonCartageLeg(ICommonCartageLeg leg)
		{
			var bizo = (BusinessObject)leg;
			return bizo.PK;
		}

		#region Properties

		internal ZString FilterActivityType
		{
			get { return filterActivityType; }
			set { filterActivityType = value; }
		}

		ZString filterActivityType = GPSConstants.GPSEventTypeList.Codes.All;

		internal ZDateTime ActivityFilterDateFrom
		{
			get { return activityLogFilterDateFrom; }
			set { activityLogFilterDateFrom = value; }
		}

		ZDateTime activityLogFilterDateFrom = ZDateTime.Today.AddMonths(-1);

		internal ZDateTime ActivityFilterDateTo
		{
			get { return activityLogFilterDateTo; }
			set { activityLogFilterDateTo = value; }
		}

		ZDateTime activityLogFilterDateTo = ZDateTime.Today.AddDays(1);

		internal ZGuid CartageLegPK
		{
			get { return fCartageLegPK; }
			set { fCartageLegPK = value; }
		}

		ZGuid fCartageLegPK = ZGuid.Empty;

		#endregion

		#region Overrides

		public override bool ReadOnly
		{
			get { return true; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get
			{
				if (Master.PKSchemaColumn == RefEquipmentSchema.PK)
				{
					return LocalCartageVehicleActivitySchema.EN_RQ_Vehicle;
				}
				else
				{
					return LocalCartageVehicleActivitySchema.EN_JU;
				}
			}
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = base.CreateAdditionalFilter();

			if (ActivityFilterDateFrom.IsValidSmallDateTime)
			{
				query.AddToFilter(LocalCartageVehicleActivitySchema.EN_ActivityTime, SQLComparisonOperator.GreaterThan, ActivityFilterDateFrom);
			}
			if (ActivityFilterDateTo.IsValidSmallDateTime)
			{
				query.AddToFilter(LocalCartageVehicleActivitySchema.EN_ActivityTime, SQLComparisonOperator.LessThanOrEqualTo, ActivityFilterDateTo);
			}
			query.AddToFilter(JoinCondition.And, LocalCartageVehicleActivitySchema.EN_EventType, GPSConstants.GPSEventTypeList.Codes.Custom);
			if (FilterActivityType != GPSConstants.GPSEventTypeList.Codes.All)
			{
				query.AddToFilter(JoinCondition.And, LocalCartageVehicleActivitySchema.EN_ActivityType, SQLComparisonOperator.Equal, FilterActivityType);
			}

			if (!CartageLegPK.IsEmpty)
			{
				query.AddToFilter(JoinCondition.And, LocalCartageVehicleActivitySchema.EN_JU, CartageLegPK);
			}
			return query;
		}

		#endregion
	}
}
