using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class DashboardAttachDetachResultTest : TestCaseWithFactory
	{
		public void TestCtor()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			var shipment3 = Factory.New<ForwardingShipment>();

			var requested = new[] { shipment1, shipment2, shipment3 };
			var accepted = new[] { shipment1, shipment2 };
			var rejected = requested.Except(accepted);

			var error1 = new Notification(NotificationType.Error, "Error ONE");
			var error2 = new Notification(NotificationType.Error, "Error TWO");

			var warning1 = new Notification(NotificationType.Warning, "Warning ONE");
			var warning2 = new Notification(NotificationType.Warning, "Warning TWO");

			var notifications = new[]
			{
				error1, error2, warning1, warning2
			};

			var result = new DashboardAttachDetachResult(requested, accepted, notifications);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Requested", requested, result.RequestedShipments);
				AssertContainsExactElementsInAnyOrder("Accepted", accepted, result.AcceptedShipments);
				AssertContainsExactElementsInAnyOrder("Rejected", rejected, result.RejectedShipments);

				Assert("HasErrors", result.HasErrors);
				AssertEquals("Error ONE" + System.Environment.NewLine + "Error TWO", result.Errors);

				Assert("HasWarnings", result.HasWarnings);
				AssertEquals("Warning ONE" + System.Environment.NewLine + "Warning TWO", result.Warnings);
			});
		}
	}
}
