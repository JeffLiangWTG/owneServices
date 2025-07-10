using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.GPS.Business
{
	public class GPSSupporter : NonPersistentBusinessObject, ISupportGPS
	{
		public GPSSupporter(RefEquipment equipment)
			: base(equipment.Factory)
		{
			Equipment = equipment;
		}

		public RefEquipment Equipment { get; private set; }

		#region IsVehicle

		public ZBool IsVehicle
		{
			get { return Equipment.RQ_IsVehicle; }
		}

		public ZPropertyInfo IsVehicleInfo
		{
			get { return GetZPropertyInfo(nameof(IsVehicle)); }
		}

		#endregion

		#region ISupportGPS Members

		#region ActivityFilterProvider

		public GPSSupporterActivityCollectionFilterProvider ActivityFilterProvider
		{
			get
			{
				if (activityFilterProvider == null)
				{
					activityFilterProvider = new GPSSupporterActivityCollectionFilterProvider(Activities);
				}
				return activityFilterProvider;
			}
		}

		GPSSupporterActivityCollectionFilterProvider activityFilterProvider;

		#endregion

		#region Activities

		public GPSSupporterActivityCollection Activities
		{
			get
			{
				if (activities == null)
				{
					activities = new GPSSupporterActivityCollection(Equipment);
					activities.Load();
				}
				return activities;
			}
		}

		GPSSupporterActivityCollection activities;

		#endregion

		#region BranchPK

		public ZGuid BranchPK
		{
			get
			{
				return Equipment.OwnerBranchPK;
			}
		}

		#endregion

		#endregion
	}
}
