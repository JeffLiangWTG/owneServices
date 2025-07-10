using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(MainFormForwardingConsolCollection))]
	sealed class MainFormForwardingConsolCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new MainFormForwardingConsolCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ForwardingConsol>();
		}

		public void TestIFilterModuleExtraNotificationProvider()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();

			var collection = new MainFormForwardingConsolCollection(Factory);
			collection.ParentShipment = shipment;

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);
			using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
			{
				var attachRequest = new ShipmentConsolAttachRequest(null, null);
				helper.Setup(m => m.IsAllowedToAttachConsol(shipment, consol)).Returns(attachRequest);
				helper.SetupProperty(p => p.IsGatewayServiceLevelCheckSuspended).SetReturnsDefault(true);
				var notificationProvider = (IFilterModuleExtraNotificationProvider)collection;
				AssertNull(notificationProvider.GetExtraNotification(null));
				AssertNull(notificationProvider.GetExtraNotification(Factory.New<DummyBusinessObject>()));

				INotification notification = notificationProvider.GetExtraNotification(consol);
				AssertNull("Attach is allowed, no notification", notification);
				attachRequest = new ShipmentConsolAttachRequest(() => "ERROR?", null);
				helper.Setup(m => m.IsAllowedToAttachConsol(shipment, consol)).Returns(attachRequest);
				notificationProvider = collection;
				notification = notificationProvider.GetExtraNotification(consol);
				AssertEquals("Attach is not allowed, error notification", true, notification.Type == CargoWise.ComponentModel.NotificationType.Error);
				AssertEquals("Attach is not allowed, error notification", "ERROR?", notification.Message);
				attachRequest = new ShipmentConsolAttachRequest(null, () => "WARNING?");
				helper.Setup(m => m.IsAllowedToAttachConsol(shipment, consol)).Returns(attachRequest);
				notificationProvider = collection;
				notification = notificationProvider.GetExtraNotification(consol);
				AssertEquals("warning notification", true, notification.Type == CargoWise.ComponentModel.NotificationType.Warning);
				AssertEquals("warning notification", "WARNING?", notification.Message);
			}
		}

		public void TestParentGridContext()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var collection = new MainFormForwardingConsolCollection(Factory, shipment);

			var parentGridContext = collection as IUseParentGridContext;
			AssertNotNull(parentGridContext);
			AssertEquals(typeof(ForwardingModuleConsol), parentGridContext.ParentType);
		}
	}
}
