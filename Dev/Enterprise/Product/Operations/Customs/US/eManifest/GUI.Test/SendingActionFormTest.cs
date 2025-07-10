using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	[TestedType(typeof(SendingActionForm))]
	sealed class SendingActionFormTest : ZFormBasherTest
	{
		public void TestSendWhenNoShipmentsSelected()
		{
			var trip = Factory.New<Trip>();
			var shipment1 = trip.Shipments.AddNew();
			shipment1.B0_MasterBillNumber = "LOCK1234567890";
			var shipment2 = trip.Shipments.AddNew();
			shipment2.B0_MasterBillNumber = "LOCK1234567891";
			shipment2.B0_ReleaseStatus = EntryStatusList.Codes.Error;
			using (var form = new SendingActionForm(trip, MessageTypes.Descriptions.UnassociatedShipments))
			{
				form.Show();
				trip.ShipmentsActions[0].B0_ActionCode = ZString.Empty;
				trip.ShipmentsActions[1].B0_ActionCode = ZString.Empty;
				Application.DoEvents();
				AssertEquals("Send Unassociated Shipments", form.Text);
				((ZButton)ManifestUserControlTestCase.FindControl(form, "SendButton")).PerformClick();
				Application.DoEvents();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Cannot Send Message", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("No shipments have been selected for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				((ZButton)ManifestUserControlTestCase.FindControl(form, "CancellButton")).PerformClick();
				Application.DoEvents();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestSendWhenValidationErrorsOnTheForm()
		{
			var trip = Factory.New<Trip>();
			var shipment1 = trip.Shipments.AddNew();
			shipment1.B0_MasterBillNumber = "LOCK1234567890";
			var shipment2 = trip.Shipments.AddNew();
			shipment2.B0_MasterBillNumber = "LOCK1234567891";
			shipment2.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			var shipment3 = trip.Shipments.AddNew();
			shipment3.B0_MasterBillNumber = "LOCK1234567892";
			shipment2.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Released;
			using (var form = new SendingActionForm(trip, MessageTypes.Descriptions.eManifest))
			{
				form.Show();
				trip.ShipmentsActions[0].B0_ActionCode = "XXX";
				trip.ShipmentsActions[1].B0_AmendmentReason = "XX";
				Application.DoEvents();
				AssertEquals("Send Complete e-Manifest w/ACE ID", form.Text);
				var sendButton = (ZButton)ManifestUserControlTestCase.FindControl(form, "SendButton");
				sendButton.PerformClick();
				Application.DoEvents();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Cannot Send Message", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertMultilineASCIIEquals("", @"Please fix these errors before sending any messages:

Action Code: Enter a valid Action Code.
Amendment Reason: Enter a valid Amendment Reason.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.ShipmentsActions[0].B0_ActionCode = MessageActionCodes.Codes.Original;
				trip.ShipmentsActions[1].B0_AmendmentReason = ShipmentAmendmentCodes.Codes.C01;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendButton.PerformClick();
				Application.DoEvents();
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendWhenNoNotifications()
		{
			var trip = Factory.New<Trip>();
			var shipment1 = trip.Shipments.AddNew();
			shipment1.B0_MasterBillNumber = "LOCK1234567890";
			using (var form = new SendingActionForm(trip, MessageTypes.Descriptions.eManifest))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Send Complete e-Manifest w/ACE ID", form.Text);
				((ZButton)ManifestUserControlTestCase.FindControl(form, "SendButton")).PerformClick();
				Application.DoEvents();
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			var trip = Factory.New<Trip>();
			trip.Shipments.AddNew();
			Factory.Save();
			return new SendingActionForm(trip, MessageTypes.Descriptions.UnassociatedShipments);
		}
		#endregion
	}
}
