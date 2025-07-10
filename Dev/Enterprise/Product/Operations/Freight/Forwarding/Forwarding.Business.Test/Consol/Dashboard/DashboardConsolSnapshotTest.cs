using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class DashboardConsolSnapshotTest : TestCaseWithFactory
	{
		public void TestCtor()
		{
			var consolPK = ZGuid.NewZGuid();
			var consolHumanReadableName = "IAMACONSOL";
			var shipment1PK = ZGuid.NewZGuid();
			var shipment2PK = ZGuid.NewZGuid();

			var notifications = new[]
			{
				new Notification(NotificationType.Error, "ERROR1"),
				new Notification(NotificationType.Error, "ERROR2"),
			};

			var snapshot = new DashboardConsolSnapshot(consolPK, consolHumanReadableName, new[] { shipment1PK, shipment2PK }, notifications);
			AssertEquals(consolPK, snapshot.ConsolPK);
			AssertEquals(consolHumanReadableName, snapshot.ConsolHumanReadableName);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1PK, shipment2PK }, snapshot.ShipmentPKs);
			AssertContainsExactElementsInAnyOrder(notifications, snapshot.Notifications);
			AssertEquals(DashboardConsolSnapshot.SnapshotState.Unprocessed, snapshot.State);
		}

		public void TestSetState()
		{
			var consolPK = ZGuid.NewZGuid();
			var shipment1PK = ZGuid.NewZGuid();

			var snapshot = new DashboardConsolSnapshot(consolPK, "", new[] { shipment1PK });
			AssertEquals(DashboardConsolSnapshot.SnapshotState.Unprocessed, snapshot.State);

			snapshot.SetState(DashboardConsolSnapshot.SnapshotState.Saved);
			AssertEquals(DashboardConsolSnapshot.SnapshotState.Saved, snapshot.State);

			snapshot.SetState(DashboardConsolSnapshot.SnapshotState.ValidationErrors);
			AssertEquals(DashboardConsolSnapshot.SnapshotState.ValidationErrors, snapshot.State);

			snapshot.SetState(DashboardConsolSnapshot.SnapshotState.ModifiedByAnotherUser);
			AssertEquals(DashboardConsolSnapshot.SnapshotState.ModifiedByAnotherUser, snapshot.State);
		}

		public void TestSetNotifications()
		{
			var consolPK = ZGuid.NewZGuid();
			var shipment1PK = ZGuid.NewZGuid();

			var notifications = new[]
			{
				new Notification(NotificationType.Error, "ERROR1")
			};

			var snapshot = new DashboardConsolSnapshot(consolPK, "", new[] { shipment1PK }, notifications);

			var newNotifications = new[]
			{
				new Notification(NotificationType.Error, "NEW1"),
				new Notification(NotificationType.Error, "NEW2")
			};

			snapshot.SetNotifications(newNotifications);
			AssertContainsExactElementsInAnyOrder(newNotifications, snapshot.Notifications);
		}

		public void TestSetShipmentPKs()
		{
			var consolPK = ZGuid.NewZGuid();
			var shipment1PK = ZGuid.NewZGuid();
			var notifications = System.Array.Empty<INotification>();

			var snapshot = new DashboardConsolSnapshot(consolPK, "", new[] { shipment1PK }, notifications);

			var newShipmentPKs = new ZGuid[]
			{
				ZGuid.NewZGuid(),
				ZGuid.NewZGuid()
			};

			snapshot.SetShipmentPKs(newShipmentPKs);
			AssertContainsExactElementsInAnyOrder(newShipmentPKs, snapshot.ShipmentPKs);
		}
	}
}
