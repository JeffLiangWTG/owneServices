using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	public class DashboardConsolSnapshot
	{
		public DashboardConsolSnapshot(ZGuid consolPK, ZString consolHumanReadableName, IEnumerable<ZGuid> shipmentPKs, IEnumerable<INotification> notifications = null, ForwardingContainerCollection containers = null)
		{
			ConsolPK = consolPK;
			ConsolHumanReadableName = consolHumanReadableName;
			ShipmentPKs = shipmentPKs?.ToArray() ?? System.Array.Empty<ZGuid>();
			Notifications = notifications?.ToArray() ?? System.Array.Empty<INotification>();
			State = SnapshotState.Unprocessed;
			Containers = containers;
		}

		public enum SnapshotState
		{
			Unprocessed,
			ModifiedByAnotherUser,
			ValidationErrors,
			Saved,
			SaveExceptions
		}

		public void SetState(SnapshotState state)
		{
			State = state;
		}

		public void SetNotifications(IEnumerable<INotification> notifications)
		{
			Notifications = notifications?.ToArray() ?? System.Array.Empty<INotification>();
		}

		public void SetShipmentPKs(IEnumerable<ZGuid> shipmentPKs)
		{
			ShipmentPKs = shipmentPKs?.ToArray() ?? System.Array.Empty<ZGuid>();
		}

		public void SetContainers(ForwardingContainerCollection containers)
		{
			Containers = containers;
		}

		public readonly ZGuid ConsolPK;
		public readonly ZString ConsolHumanReadableName;
		public ForwardingContainerCollection Containers;
		public IEnumerable<ZGuid> ShipmentPKs { get; private set; }
		public IEnumerable<INotification> Notifications { get; private set; }
		public SnapshotState State { get; private set; }
	}
}
