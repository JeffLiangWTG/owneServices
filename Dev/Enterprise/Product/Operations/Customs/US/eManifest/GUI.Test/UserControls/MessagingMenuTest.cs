using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.Messaging.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	sealed class MessagingMenuTest : TestCaseWithFactory
	{
		public void TestMessagingMenuStructure()
		{
			using (var form = new ManifestForm(Factory.New<Trip>()))
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var messagingMenu = form.Menu.MenuItems[3];
				AssertEquals("Messaging Menu", "Messaging", messagingMenu.Text);
				AssertEquals("Messaging.MenuItems.Count", 12, messagingMenu.MenuItems.Count);
				AssertEquals("Submit Complete e-Manifest w/ACE ID", messagingMenu.MenuItems[0].Text);
				AssertEquals("Change Complete e-Manifest w/ACE ID Header Only", messagingMenu.MenuItems[1].Text);
				AssertEquals("-", messagingMenu.MenuItems[2].Text);
				AssertEquals("Submit eManifest (No ACE ID)", messagingMenu.MenuItems[3].Text);
				AssertEquals("Submit Unassociated Shipments", messagingMenu.MenuItems[4].Text);
				AssertEquals("Submit Preliminary Trip Details", messagingMenu.MenuItems[5].Text);
				AssertEquals("Submit Crew/Passengers Details", messagingMenu.MenuItems[6].Text);
				AssertEquals("Confirm Trip Details Completed", messagingMenu.MenuItems[7].Text);
				AssertEquals("-", messagingMenu.MenuItems[8].Text);
				AssertEquals("Cancel Trip And Linked Shipments", messagingMenu.MenuItems[9].Text);
				AssertEquals("-", messagingMenu.MenuItems[10].Text);
				AssertEquals("Register Crew Information", messagingMenu.MenuItems[11].Text);
			}
		}

		public void TestSubmitCompleteManifestClick()
		{
			var trip = Factory.New<Trip>();
			trip.BH_ETA = ZDateTime.Empty;
			trip.OnSaving();
			using (var form = new ManifestForm(trip))
			{
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText("Submit Complete e-Manifest w/ACE ID", true);
				AssertNotNull("Submit Complete e-Manifest Menu", sendMenu);
				sendMenu.PerformClick();
				AssertEquals("System cannot send a Complete e-Manifest w/ACE ID message as Job not yet saved, Please save before sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not have sent a message", ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
				Factory.Save();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendMenu.PerformClick();
				AssertMultilineASCIIEquals("Popup message", @"It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

Carrier Code (SCAC): You have not entered a Carrier Code (SCAC).
Estimated Date of Arrival: You have not entered an Estimated Date of Arrival.
Job Reference: Responsible party is required on the Crew tab.
The responsible party may also be the driver, passenger, or crew member.
If this is the case, use RP in this data element to report that person.
Importer: You have not entered an Importer.
First Expected Port of Arrival (Schedule D): You have not entered a First Expected Port of Arrival (Schedule D).

Do you want to send the message(s) despite these errors?

", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not have sent a message", ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMenu.PerformClick();
				AssertEquals("Original Complete e-Manifest w/ACE ID message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should have sent a message", ++ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
			}
		}

		public void TestSubmitCompleteManifestChangeClick()
		{
			var trip = Factory.New<Trip>();
			trip.BH_CarrierSCAC = "LOCK";
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.HoldTrip;
			var shipment1 = trip.Shipments.AddNew();
			shipment1.B0_MasterBillNumber = "LOCK1234567890";
			shipment1.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.ShipmentHold;
			Factory.Save();
			using (var form = new ManifestForm(trip))
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText("Submit Complete e-Manifest w/ACE ID", true);
				AssertNotNull("Submit Complete e-Manifest Menu", sendMenu);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				sendMenu.PerformClick();
				AssertEquals("Please specify the reason code for this amendment message.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.Messages.Load();
				AssertEquals("No sent messages", 0, trip.Messages.Count);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddUserResponse(AmendmentReasonCodes.Codes.C03);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f => trip.ShipmentsActions[0].B0_AmendmentReason = ShipmentAmendmentCodes.Codes.C01);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMenu.PerformClick();
				AssertEquals(typeof(SendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Change Complete e-Manifest w/ACE ID message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.Messages.Load();
				AssertEquals("Should have sent a message", 1, trip.Messages.Count);
				AssertContains("Amendment reason set for the trip", "RFF+RFA:03", trip.Messages[0].EM_MessageText);
				AssertContains("Amendment reason set for the shipment", "RFF+RFA:01", trip.Messages[0].EM_MessageText);
			}
		}

		public void TestSubmitCompleteManifestChangeHeaderOnlyClick()
		{
			var trip = Factory.New<Trip>();
			Factory.Save();
			using (var form = new ManifestForm(trip))
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText("Change Complete e-Manifest w/ACE ID Header Only", true);
				AssertNotNull("Change Complete e-Manifest Header Only Menu", sendMenu);
				sendMenu.PerformClick();
				AssertEquals("System cannot send a Complete e-Manifest w/ACE ID Change Header Only message as original message has not been lodged yet.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.BH_ReleaseStatus = TripEntryStatusList.Codes.HoldTrip;
				Factory.Save();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddUserResponse(AmendmentReasonCodes.Codes.C03);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMenu.PerformClick();
				AssertNull("No SendingActionForm should be shown for this message ", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Change Header Only Complete e-Manifest w/ACE ID message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.Messages.Load();
				AssertEquals("Should have sent a message", 1, trip.Messages.Count);
				AssertContains("Amendment reason set for the trip", "RFF+RFA:03", trip.Messages[0].EM_MessageText);
			}
		}

		public void TestSubmitCompleteManifestChangeWithoutShipmentsClick()
		{
			var trip = Factory.New<Trip>();
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.HoldTrip;
			Factory.Save();
			using (var form = new ManifestForm(trip))
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText("Submit Complete e-Manifest w/ACE ID", true);
				AssertNotNull("Submit Complete e-Manifest Menu", sendMenu);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddUserResponse(AmendmentReasonCodes.Codes.C03);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMenu.PerformClick();
				AssertNull("No SendingActionForm should be shown if no shipments", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Change Complete e-Manifest w/ACE ID message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.Messages.Load();
				AssertEquals("Should have sent a message", 1, trip.Messages.Count);
				AssertContains("Amendment reason set for the trip", "RFF+RFA:03", trip.Messages[0].EM_MessageText);
			}
		}

		public void TestSubmitCompleteManifestCancellationClick()
		{
			var trip = Factory.New<Trip>();
			trip.BH_CarrierSCAC = "LOCK";
			Factory.Save();
			using (var form = new ManifestForm(trip))
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText("Cancel Trip And Linked Shipments", true);
				AssertNotNull("Cancel Trip And Linked Shipments Menu", sendMenu);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMenu.PerformClick();
				AssertEquals("System cannot send a Complete e-Manifest w/ACE ID Cancellation message as original message has not been lodged yet.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
				Factory.Save();
				sendMenu.PerformClick();
				trip.Messages.Load();
				AssertEquals("Cancellation Complete e-Manifest w/ACE ID message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should have sent a message", 1, trip.Messages.Count);
				AssertContains("Delete message sent", "BGM+85:::STANDARD+LOCKMAN0000001+3", trip.Messages[0].EM_MessageText);
			}
		}

		public void TestSubmitCrewPassengersDetailsClick()
		{
			var trip = Factory.New<Trip>();
			Factory.Save();
			using (var form = new ManifestForm(trip))
			{
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText("Submit Crew/Passengers Details", true);
				AssertNotNull("Submit Crew/Passengers Details Menu", sendMenu);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMenu.PerformClick();
				AssertEquals("System cannot send a Crew/Passengers Details message as preliminary trip has not been lodged yet.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedPreliminary;
				Factory.Save();
				sendMenu.PerformClick();
				AssertEquals("Original Crew/Passengers Details message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should have sent a message", ++ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
			}
		}

		public void TestSubmitEManifestNoACEID_Click()
		{
			var trip = Factory.New<Trip>();
			Factory.Save();
			using (var form = new ManifestForm(trip))
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText("Submit eManifest (No ACE ID)", true);
				AssertNotNull("Submit eManifest (No ACE ID) Menu", sendMenu);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendMenu.PerformClick();
				AssertContains("Validate for Unassociated Shipments", "System cannot send an Unassociated Shipments Original message as there are no shipments entered.", UnitTestUserNotification.Instance.LastMessage.Text);
				var shipment = trip.Shipments.AddNew();
				shipment.B0_MasterBillNumber = "1234567890";
				shipment = trip.Shipments.AddNew();
				shipment.B0_MasterBillNumber = "1234567891";
				shipment = trip.Shipments.AddNew();
				shipment.B0_MasterBillNumber = "1234567892";
				Factory.Save();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f => trip.ShipmentsActions[0].B0_ActionCode = ZString.Empty);
				sendMenu.PerformClick();
				AssertEquals(typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertContains("1 Unassociated Shipments message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("1 Preliminary Trip Details message set to pending on acceptance of previous message.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("1 Crew/Passengers Details message set to pending on acceptance of previous message.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("1 Confirm Preliminary Trip Details message set to pending on acceptance of previous message.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.Messages.Load();
				AssertEquals("Should have sent a message", 4, trip.Messages.Count);
				AssertContains("Message Action", "BGM+87:::STANDARD+SYSTEM+2", trip.Messages[0].EM_MessageText);
				AssertNotContains("No shipment 1", "RFF+AAM:LOCK1234567890'", trip.Messages[0].EM_MessageText);
				AssertContains("Create shipment 2", "CNI+1+:23'RFF+AAM:1234567891'", trip.Messages[0].EM_MessageText);
				AssertContains("Create shipment 3", "CNI+2+:23'RFF+AAM:1234567892'", trip.Messages[0].EM_MessageText);
				AssertContains(MessageTypes.Codes.UnassociatedShipments, trip.Messages[0].EM_MessageType);
				AssertContains(MessageActionCodes.Codes.Original, trip.Messages[0].EM_MessageSubType);
				AssertContains(MessageTypes.Codes.PreliminaryTrip, trip.Messages[1].EM_MessageType);
				AssertContains(MessageActionCodes.Codes.Original, trip.Messages[1].EM_MessageSubType);
				AssertContains(MessageTypes.Codes.CrewAndPassenger, trip.Messages[2].EM_MessageType);
				AssertContains(MessageActionCodes.Codes.Original, trip.Messages[2].EM_MessageSubType);
				AssertContains(MessageTypes.Codes.PreliminaryTrip, trip.Messages[3].EM_MessageType);
				AssertContains(MessageActionCodes.Codes.Confirmation, trip.Messages[3].EM_MessageSubType);
			}
		}

		public void TestSubmitUnassociatedShipmentsClick()
		{
			var trip = Factory.New<Trip>();
			trip.BH_CarrierSCAC = "LOCK";
			Factory.Save();
			using (var form = new ManifestForm(trip))
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText("Submit Unassociated Shipments", true);
				AssertNotNull("Submit Unassociated Shipments Menu", sendMenu);
				sendMenu.PerformClick();
				AssertEquals("System cannot send an Unassociated Shipments message as there are no shipments entered.", UnitTestUserNotification.Instance.LastMessage.Text);
				var shipment = trip.Shipments.AddNew();
				shipment.B0_MasterBillNumber = "1234567890";
				shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
				shipment = trip.Shipments.AddNew();
				shipment.B0_MasterBillNumber = "1234567891";
				shipment = trip.Shipments.AddNew();
				shipment.B0_MasterBillNumber = "1234567892";
				Factory.Save();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f => trip.ShipmentsActions[0].B0_ActionCode = ZString.Empty);
				sendMenu.PerformClick();
				AssertEquals(typeof(SendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Original Unassociated Shipments message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.Messages.Load();
				AssertEquals("Should have sent a message", 1, trip.Messages.Count);
				AssertContains("Message Action", "BGM+87:::STANDARD+SYSTEM+2", trip.Messages[0].EM_MessageText);
				AssertNotContains("No shipment 1", "RFF+AAM:LOCK1234567890'", trip.Messages[0].EM_MessageText);
				AssertContains("Create shipment 2", "CNI+1+:23'RFF+AAM:LOCK1234567891'", trip.Messages[0].EM_MessageText);
				AssertContains("Create shipment 3", "CNI+2+:23'RFF+AAM:LOCK1234567892'", trip.Messages[0].EM_MessageText);
			}
		}

		public void TestSubmitUnassociatedShipmentsWhenTripIsLodgedClick()
		{
			var trip = Factory.New<Trip>();
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedPreliminary;
			var shipment = trip.Shipments.AddNew();
			shipment.B0_IssuerSCAC = "LOCK";
			shipment.B0_MasterBillNumber = "1234567890";
			Factory.Save();
			using (var form = new ManifestForm(trip))
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText("Submit Unassociated Shipments", true);
				AssertNotNull("Submit Unassociated Shipments Menu", sendMenu);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendMenu.PerformClick();
				AssertEquals(typeof(SendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Change Unassociated Shipments message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.Messages.Load();
				AssertEquals("Should have sent a message", 1, trip.Messages.Count);
				AssertContains("Message Action", "BGM+87:::STANDARD+MAN0000001+4", trip.Messages[0].EM_MessageText);
				AssertContains("Create shipment", "RFF+AAM:LOCK1234567890'", trip.Messages[0].EM_MessageText);
			}
		}

		public void TestSubmitUnassociatedShipmentsChangeClick()
		{
			var trip = Factory.New<Trip>();
			trip.BH_CarrierSCAC = "LOCK";
			var shipment = trip.Shipments.AddNew();
			shipment.B0_MasterBillNumber = "1234567890";
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			shipment = trip.Shipments.AddNew();
			shipment.B0_MasterBillNumber = "1234567891";
			shipment = trip.Shipments.AddNew();
			shipment.B0_MasterBillNumber = "1234567892";
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			Factory.Save();
			using (var form = new ManifestForm(trip))
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText("Submit Unassociated Shipments", true);
				AssertNotNull("Submit Unassociated Shipments Menu", sendMenu);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
				{
					trip.ShipmentsActions[0].B0_AmendmentReason = ShipmentAmendmentCodes.Codes.C01;
					trip.ShipmentsActions[2].B0_ActionCode = MessageActionCodes.Codes.Cancellation;
				});
				sendMenu.PerformClick();
				AssertEquals(typeof(SendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Change Unassociated Shipments message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.Messages.Load();
				AssertEquals("Should have sent a message", 1, trip.Messages.Count);
				AssertContains("Message Action", "BGM+87:::STANDARD+SYSTEM+4", trip.Messages[0].EM_MessageText);
				AssertContains("Change shipment 1", "CNI+1+:24'RFF+AAM:LOCK1234567890'LOC+9+:78'GEI+7+135'TDT+11'RFF+RFA:01", trip.Messages[0].EM_MessageText);
				AssertContains("Create shipment 2", "CNI+2+:23'RFF+AAM:LOCK1234567891'", trip.Messages[0].EM_MessageText);
				AssertContains("Cancel shipment 3", "CNI+3+:22'RFF+AAM:LOCK1234567892'", trip.Messages[0].EM_MessageText);
			}
		}

		public void TestSubmitPreliminaryTripDetailsClick()
		{
			var trip = Factory.New<Trip>();
			Factory.Save();
			using (var form = new ManifestForm(trip))
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText("Submit Preliminary Trip Details", true);
				AssertNotNull("Submit Preliminary Trip Details Menu", sendMenu);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMenu.PerformClick();
				AssertEquals("Original Preliminary Trip Details message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.Messages.Load();
				AssertEquals("Should have sent a message", 1, trip.Messages.Count);
				AssertContains("Message action", "BGM+336:::STANDARD+MAN0000001+2", trip.Messages[0].EM_MessageText);
			}
		}

		public void TestSubmitPreliminaryTripDetailsChangeClick()
		{
			var trip = Factory.New<Trip>();
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedPreliminary;
			var shipment1 = trip.Shipments.AddNew();
			shipment1.B0_IssuerSCAC = "LOCK";
			shipment1.B0_MasterBillNumber = "LOCK1234567890";
			shipment1.B0_ReleaseStatus = MessageActionCodes.Codes.Link;
			Factory.Save();
			using (var form = new ManifestForm(trip))
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText("Submit Preliminary Trip Details", true);
				AssertNotNull("Submit Preliminary Trip Details Menu", sendMenu);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f => trip.ShipmentsActions[0].B0_ActionCode = MessageActionCodes.Codes.DeLink);
				sendMenu.PerformClick();
				AssertEquals(typeof(SendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Change Preliminary Trip Details message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.Messages.Load();
				AssertEquals("Should have sent a message", 1, trip.Messages.Count);
				AssertContains("Message action", "BGM+336:::STANDARD+MAN0000001+4", trip.Messages[0].EM_MessageText);
				AssertContains("Shipment action", "DOC+700+:22", trip.Messages[0].EM_MessageText);
			}
		}

		public void TestSubmitPreliminaryTripDetailsChangeWithSplitShipmentClick()
		{
			var trip = Factory.New<Trip>();
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedPreliminary;
			var shipment1 = trip.Shipments.AddNew();
			shipment1.B0_IssuerSCAC = "LOCK";
			shipment1.B0_ShipmentType = ShipmentTypes.Codes.SplitShipment;
			shipment1.B0_MasterBillNumber = "1234567890";
			Factory.Save();
			using (var form = new ManifestForm(trip))
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText("Submit Preliminary Trip Details", true);
				AssertNotNull("Submit Preliminary Trip Details Menu", sendMenu);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendMenu.PerformClick();
				AssertEquals(typeof(SendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Change Preliminary Trip Details message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.Messages.Load();
				AssertEquals("Should have sent a message", 1, trip.Messages.Count);
				AssertContains("Message action", "BGM+336:::STANDARD+MAN0000001+4", trip.Messages[0].EM_MessageText);
				AssertContains("Shipment action", "DOC+700+:23", trip.Messages[0].EM_MessageText);
			}
		}

		public void TestSubmitPreliminaryTripDetailsChangeWhenNoLodgedShipmentsClick()
		{
			var trip = Factory.New<Trip>();
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedPreliminary;
			var shipment1 = trip.Shipments.AddNew();
			shipment1.B0_IssuerSCAC = "LOCK";
			shipment1.B0_MasterBillNumber = "1234567890";
			Factory.Save();
			using (var form = new ManifestForm(trip))
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText("Submit Preliminary Trip Details", true);
				AssertNotNull("Submit Preliminary Trip Details Menu", sendMenu);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendMenu.PerformClick();
				AssertEquals(@"The entered shipments are not lodged with Customs and not going to be linked to the trip.
In order to lodge the shipments you have to submit unassociated shipments first.
Are you sure you want to submit the empty trip without shipments?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				sendMenu.PerformClick();
				AssertNull("No SendingActionForm should be shown if no lodged shipments", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Change Preliminary Trip Details message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.Messages.Load();
				AssertEquals("Should have sent a message", 1, trip.Messages.Count);
				AssertContains("Message action", "BGM+336:::STANDARD+MAN0000001+4", trip.Messages[0].EM_MessageText);
				AssertNotContains("Shipment action", "DOC", trip.Messages[0].EM_MessageText);
			}
		}

		public void TestSubmitPreliminaryTripDetailsCompletedConfirmationClick()
		{
			var trip = Factory.New<Trip>();
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.HoldTrip;
			Factory.Save();
			using (var form = new ManifestForm(trip))
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText("Confirm Trip Details Completed", true);
				AssertNotNull("Confirm Trip Details Completed Menu", sendMenu);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMenu.PerformClick();
				AssertEquals("System cannot send a Preliminary Trip Details Manifest Completed Confirmation message as the e-Manifest has already been marked as complete.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.BH_ReleaseStatus = ZString.Empty;
				Factory.Save();
				sendMenu.PerformClick();
				AssertEquals("System cannot send a Preliminary Trip Details Manifest Completed Confirmation message as preliminary trip has not been lodged yet.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedPreliminary;
				Factory.Save();
				sendMenu.PerformClick();
				AssertEquals("System cannot send a Preliminary Trip Details Manifest Completed Confirmation message as crew/passengers information has not been lodged yet.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.Messages.Add(MessagingTestHelper.GetReceivedEDIMessage(Factory, MessageTypes.Codes.CrewAndPassenger, TripEntryStatusList.Codes.AcceptedPreliminary));
				Factory.Save();
				sendMenu.PerformClick();
				AssertEquals("Manifest Completed Confirmation Preliminary Trip Details message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.Messages.Load();
				AssertEquals("Should have sent a message", 2, trip.Messages.Count);
				AssertContains("Message action", "BGM+336:::STANDARD+MAN0000001+6", trip.Messages[1].EM_MessageText);
				AssertEquals("BH_MessageStatus", MessageStatusList.Codes.AwaitingChange, trip.BH_MessageStatus);
			}
		}

		public void TestSubmitPreliminaryTripDetailsCancelationClick()
		{
			var trip = Factory.New<Trip>();
			Factory.Save();
			using (var form = new ManifestForm(trip))
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText("Cancel Trip And Linked Shipments", true);
				AssertNotNull("Cancel Trip And Linked Shipments Menu", sendMenu);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMenu.PerformClick();
				AssertEquals("System cannot send a Complete e-Manifest w/ACE ID Cancellation message as original message has not been lodged yet.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedPreliminary;
				Factory.Save();
				sendMenu.PerformClick();
				AssertEquals("Cancellation Preliminary Trip Details message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.Messages.Load();
				AssertEquals("Should have sent a message", 1, trip.Messages.Count);
				AssertContains("Message action", "BGM+336:::STANDARD+MAN0000001+3", trip.Messages[0].EM_MessageText);
			}
		}

		public void TestRegisterCrewClick()
		{
			var trip = Factory.New<Trip>();
			trip.CrewMembers.AddNew();
			Factory.Save();
			using (var form = new ManifestForm(trip))
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText("Register Crew Information", true);
				AssertNotNull("Register Crew Information Menu", sendMenu);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMenu.PerformClick();
				AssertEquals("Original Crew/Equipment ACE Registration message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				trip.Messages.Load();
				AssertEquals("Should have sent a message", 1, trip.Messages.Count);
			}
		}

		public void TestSkipValidateShipmentWhenIsFromHVLVAndRegistryIsEnabled()
		{
			var trip = Factory.NewWithValidTestData<Trip>();
			trip.BH_JobReference = "MAN01";

			var bill = Factory.NewWithValidTestData<Shipment>();
			bill.B0_BH = trip.PK;

			trip.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "HVL"),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, "MAN01")
			});

			Factory.Save();

			using (HVLVDataRegistry.Instance.SkipeManifestShipmentValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertShipmentHasNotification("Shipment is not validated when registry is set to true", false);
			}

			trip.BH_MessageStatus = ZString.Empty;
			Factory.Save();

			AssertShipmentHasNotification("Shipment is validated when registry is set to false", true);

			void AssertShipmentHasNotification(string message, bool hasNotification)
			{
				using (var form = new ManifestForm(trip))
				{
					form.Show();
					Application.DoEvents();

					var sendMenu = form.Menu.MenuItems.FindByText("Submit Complete e-Manifest w/ACE ID", true);
					AssertNotNull(sendMenu);

					sendMenu.PerformClick();
					AssertEquals(message, hasNotification, bill.HasNotifications());
				}
			}
		}

		public void TestSendMessageWhenMoreThan9Shipment()
		{
			AssertErrorMessageWhenMoreThan9ShipmentClick("Change Complete e-Manifest w/ACE ID Header Only", string.Empty);
			AssertErrorMessageWhenMoreThan9ShipmentClick("Submit Complete e-Manifest w/ACE ID", string.Empty);
			AssertErrorMessageWhenMoreThan9ShipmentClick("Cancel Trip And Linked Shipments", string.Empty);
			AssertErrorMessageWhenMoreThan9ShipmentClick("Register Crew Information", string.Empty);

			AssertErrorMessageWhenMoreThan9ShipmentClick("Submit Crew/Passengers Details", "Crew/Passengers Details");
			AssertErrorMessageWhenMoreThan9ShipmentClick("Submit eManifest (No ACE ID)", "eManifest (No ACE ID)");
			AssertErrorMessageWhenMoreThan9ShipmentClick("Submit Unassociated Shipments", "Unassociated Shipments");
			AssertErrorMessageWhenMoreThan9ShipmentClick("Submit Preliminary Trip Details", "Preliminary Trip Details");
			AssertErrorMessageWhenMoreThan9ShipmentClick("Confirm Trip Details Completed", "Preliminary Trip Details");
		}

		void AssertErrorMessageWhenMoreThan9ShipmentClick(string menu, string errorMessage)
		{
			var trip = GetTheTripWith10Shipments();
			Factory.Save();

			using (var form = new ManifestForm(trip))
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(form);
				form.Show();
				Application.DoEvents();
				var sendMenu = form.Menu.MenuItems.FindByText(menu, true);
				AssertNotNull($"{menu} Menu", sendMenu);
				sendMenu.PerformClick();
				if (!string.IsNullOrEmpty(errorMessage))
				{
					AssertContains("Validate for too many Shipments", string.Format("System cannot send {0} message as there are too many shipments for this type of message. Instead send the Completed Manifest with ACE ID.", errorMessage), UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertNotContains("there are too many shipments for this type of message", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		Trip GetTheTripWith10Shipments()
		{
			var trip = Factory.New<Trip>();
			for (int i = 0; i < 10; i++)
			{
				trip.Shipments.AddNew();
			}
			return trip;
		}

		static void SetCopyCaptionToPropertyHumanReadableNameForTest(Control control)
		{
			var extension = control.GetExtension<ZLabelCaptionRenderer>();
			if (extension != null)
			{
				extension.CopyCaptionToPropertyHumanReadableNameForTest = true;
			}

			foreach (Control childControl in control.Controls)
			{
				SetCopyCaptionToPropertyHumanReadableNameForTest(childControl);
			}
		}
	}
}
