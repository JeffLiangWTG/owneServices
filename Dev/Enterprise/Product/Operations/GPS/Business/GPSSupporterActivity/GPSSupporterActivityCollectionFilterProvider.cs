using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.GPS.Business
{
	public class GPSSupporterActivityCollectionFilterProvider : NonPersistentBusinessObject
	{
		public GPSSupporterActivityCollectionFilterProvider(GPSSupporterActivityCollection collection)
			: base(collection.Factory)
		{
			this.collection = collection;
		}

		#region Collection

		public GPSSupporterActivityCollection Collection
		{
			get { return collection; }
		}

		readonly GPSSupporterActivityCollection collection;

		#endregion

		#region Properties

		public void ClearFilters()
		{
			ActivityFilterDateFrom = ZDateTime.Today;
			ActivityFilterDateTo = ZDateTime.Today.AddDays(1);
			FilterActivityType = GPSConstants.GPSEventTypeList.Codes.All;
			CartageLegPK = ZGuid.Empty;
			FilterActivityTypeInfo.RefreshBinding();
		}

		public ZDateTime ActivityFilterDateFrom
		{
			get { return Collection.ActivityFilterDateFrom; }
			set
			{
				Collection.ActivityFilterDateFrom = value;
				ActivityFilterDateFromInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ActivityFilterDateFromInfo
		{
			get { return GetZPropertyInfo(nameof(ActivityFilterDateFrom)); }
		}

		public ZDateTime ActivityFilterDateTo
		{
			get { return Collection.ActivityFilterDateTo; }
			set
			{
				Collection.ActivityFilterDateTo = value;
				ActivityFilterDateToInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ActivityFilterDateToInfo
		{
			get { return GetZPropertyInfo(nameof(ActivityFilterDateTo)); }
		}

		[CargoWise.ComponentModel.MaxLength(15)]
		public ZString FilterActivityType
		{
			get { return Collection.FilterActivityType; }
			set
			{
				CheckMaximumLength(FilterActivityTypeInfo, value);
				Collection.FilterActivityType = value;
				FilterActivityTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FilterActivityTypeInfo
		{
			get { return GetZPropertyInfo(nameof(FilterActivityType)); }
		}

		public ZGuid CartageLegPK
		{
			get { return Collection.CartageLegPK; }
			set
			{
				Collection.CartageLegPK = value;
				CartageLegPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CartageLegPKInfo
		{
			get { return GetZPropertyInfo(nameof(CartageLegPK)); }
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList ActivityTypes
		{
			get
			{
				activityTypes = new CodeDescriptionPairList();
				activityTypes.AddPair(GPSConstants.GPSEventTypeList.Codes.All, GPSConstants.GPSEventTypeList.Descriptions.All);
				activityTypes.AddPair(GPSConstants.GPSInOutActivityType.Codes.GIN, GPSConstants.GPSNotificationEventList.Descriptions.InTimeUpdatedByGeofence);
				activityTypes.AddPair(GPSConstants.GPSInOutActivityType.Codes.GOT, GPSConstants.GPSNotificationEventList.Descriptions.OutTimeUpdatedByGeofence);
				return activityTypes;
			}
		}

		CodeDescriptionPairList activityTypes;

		#endregion
	}
}
