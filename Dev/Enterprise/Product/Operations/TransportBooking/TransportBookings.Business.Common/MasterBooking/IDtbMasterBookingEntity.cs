using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.Shared
{
	public interface IDtbMasterBookingEntity
	{
		IEnumerable<ZPropertyInfo> ReplicationFieldInfos { get; }
		short GetMasterBookingVersion();
		void SetMasterBookingVersion(short newMasterBookingVersion);
		bool IsMaster { get; }
	}
}
