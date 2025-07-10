using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.BR
{
	sealed class CargoControlAndTransitHouseManifestMessagingExtensionsTest : TestCaseWithFactory
	{
		[TestDate(2024, 02, 01)]
		public void TestContinueWithSendingMessage()
		{
			Helper.CreateNewCCTPassword();
			var notifications = new Mock<IUserNotifications>();

			var consol = Factory.New<ForwardingConsol>();
			var extensions = new CargoControlAndTransitHouseManifestMessagingExtensions(consol);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("Disallow sending messages to CCT", false, res);
			notifications.Verify(fake => fake.ShowMessage("To send messages to CCT you must have a valid certificate loaded against your staff profile.", "Information"), Times.Once);

			GlbStaff.CurrentUser.GetBRWrapper().CCTPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			notifications.Reset();
			res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertNull("Message sending returns null. (Message sending is deferred to base)", res);
			notifications.Verify(fake => fake.ShowMessage("To send messages to CCT you must have a valid certificate loaded against your staff profile.", "Information"), Times.Never);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_UniqueConsignRef = "CXXXX1001";
			shipment.AWBHeader.EH_AWBIssueDate = ZDateTime.Today.AddDays(1);
			notifications.Reset();
			res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals(false, res);
			notifications.Verify(fake => fake.ShowMessage("HAWB has been detected with a 'future' Issue Date for Shipment(s) CXXXX1001, which will lead to an erroneous association at CCT. As per CCT Requirements, the CCT House Manifest must be sent when each linked HAWB has an 'actual' Issue Date.\r\nIf the above Shipment(s) has been declared with a 'future' Issue Date by mistake, please withdraw the previous CCT Shipment Report and resend after correction.", "Information"), Times.Once);

			shipment.AWBHeader.EH_AWBIssueDate = ZDateTime.Today.AddDays(-1);
			notifications.Reset();
			res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertNull(res);
			notifications.Verify(fake => fake.ShowMessage("HAWB has been detected with a 'future' Issue Date for Shipment(s) CXXXX1001, which will lead to an erroneous association at CCT. As per CCT Requirements, the CCT House Manifest must be sent when each linked HAWB has an 'actual' Issue Date.\r\nIf the above Shipment(s) has been declared with a 'future' Issue Date by mistake, please withdraw the previous CCT Shipment Report and resend after correction.", "Information"), Times.Never);
		}

		public void TestContinueWithSendingMessageAmendment()
		{
			Helper.CreateNewCCTPassword();
			var notifications = new Mock<IUserNotifications>();

			var consol = Factory.New<ForwardingConsol>();
			var extensions = new CargoControlAndTransitHouseManifestMessagingExtensions(consol);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertEquals("Disallow sending messages to CCT", false, res);

			notifications.Verify(fake => fake.ShowMessage("To send messages to CCT you must have a valid certificate loaded against your staff profile.", "Information"), Times.Once);

			GlbStaff.CurrentUser.GetBRWrapper().CCTPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			notifications.Reset();
			res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertNull("Message sending returns null. (Message sending is deferred to base)", res);
			notifications.Verify(fake => fake.ShowMessage("To send messages to CCT you must have a valid certificate loaded against your staff profile.", "Information"), Times.Never);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_UniqueConsignRef = "CXXXX1001";
			shipment.AWBHeader.EH_AWBIssueDate = ZDateTime.Today.AddDays(1);
			notifications.Reset();
			res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals(false, res);
			notifications.Verify(fake => fake.ShowMessage("HAWB has been detected with a 'future' Issue Date for Shipment(s) CXXXX1001, which will lead to an erroneous association at CCT. As per CCT Requirements, the CCT House Manifest must be sent when each linked HAWB has an 'actual' Issue Date.\r\nIf the above Shipment(s) has been declared with a 'future' Issue Date by mistake, please withdraw the previous CCT Shipment Report and resend after correction.", "Information"), Times.Once);

			shipment.AWBHeader.EH_AWBIssueDate = ZDateTime.Today.AddDays(-1);
			notifications.Reset();
			res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertNull(res);
			notifications.Verify(fake => fake.ShowMessage("HAWB has been detected with a 'future' Issue Date for Shipment(s) CXXXX1001, which will lead to an erroneous association at CCT. As per CCT Requirements, the CCT House Manifest must be sent when each linked HAWB has an 'actual' Issue Date.\r\nIf the above Shipment(s) has been declared with a 'future' Issue Date by mistake, please withdraw the previous CCT Shipment Report and resend after correction.", "Information"), Times.Never);
		}

		public void TestContinueWithSendingMessageWithdrawal()
		{
			Helper.CreateNewCCTPassword();
			var notifications = new Mock<IUserNotifications>();

			var consol = Factory.New<ForwardingConsol>();
			var extensions = new CargoControlAndTransitHouseManifestMessagingExtensions(consol);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals("Disallow sending messages to CCT", false, res);
			notifications.Verify(fake => fake.ShowMessage("To send messages to CCT you must have a valid certificate loaded against your staff profile.", "Information"), Times.Once);

			GlbStaff.CurrentUser.GetBRWrapper().CCTPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			notifications.Reset();
			res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertNull("Message sending returns null. (Message sending is deferred to base)", res);
			notifications.Verify(fake => fake.ShowMessage("To send messages to CCT you must have a valid certificate loaded against your staff profile.", "Information"), Times.Never);
		}

		public void TestContinueWithResetToOriginal()
			=> AssertNull("ContinueWithResetToOriginal should return null so Reset To Original is not blocked.", new CargoControlAndTransitHouseManifestMessagingExtensions(Factory.New<ForwardingConsol>()).ContinueWithResetToOriginal(null));

		#region Implementation

		CargoControlAndTransitMessagingExtensionsTestHelper Helper => helper ?? (helper = new CargoControlAndTransitMessagingExtensionsTestHelper(Factory));
		CargoControlAndTransitMessagingExtensionsTestHelper helper;

		#endregion
	}
}
