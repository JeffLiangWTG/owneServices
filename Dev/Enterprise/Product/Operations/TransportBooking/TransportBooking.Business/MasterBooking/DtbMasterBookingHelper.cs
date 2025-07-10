using CargoWise.EntityFramework;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.TransportBookings.Business
{
	class DtbMasterBookingHelper
	{
		public DtbMasterBookingHelper(IDtbMasterBookingEntity masterBookingEntity)
		{
			MasterBookingEntity = masterBookingEntity;
		}

		IDtbMasterBookingEntity MasterBookingEntity { get; }

		BusinessObject MasterBookingEntityBusinessObject => (BusinessObject)MasterBookingEntity;

		public void UpdateMasterBookingVersion()
		{
			if (MasterBookingEntity.IsMaster && MasterBookingEntityBusinessObject.IsInDatabase && !MasterBookingEntityBusinessObject.IsDeleted && MasterBookingEntityBusinessObject.HasChanges && HasChangesToReplicationFields())
			{
				var currentMasterBookingVersion = MasterBookingEntity.GetMasterBookingVersion();
				var newMasterBookingVersion = (short)((currentMasterBookingVersion >= short.MaxValue) ? 1 : (currentMasterBookingVersion + 1));
				MasterBookingEntity.SetMasterBookingVersion(newMasterBookingVersion);
			}
		}

		bool HasChangesToReplicationFields()
		{
			foreach (var propertyInfo in MasterBookingEntity.ReplicationFieldInfos)
			{
				if (propertyInfo.HasChanges)
				{
					return true;
				}
			}
			return false;
		}
	}
}
