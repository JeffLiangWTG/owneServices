using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Confirmations.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Confirmations.GUI
{
	sealed class QuickPODFormTest : TestCaseWithFactory
	{
		public void TestEditShipmentMenuItem()
		{
			QuickPODs quickPODs = new QuickPODs(Factory);
			using (QuickPODForm quickPODForm = new QuickPODForm(quickPODs))
			{
				quickPODForm.Show();
				AssertEquals("Edit Shipment menu item should be at index 0", "Edit Shipment Job", quickPODForm.QuickPODGridInternal.ContextMenu.MenuItems[0].Text);
				AssertEquals("There should be a devider/separator under the Edit Shipment Job menu item", "-", quickPODForm.QuickPODGridInternal.ContextMenu.MenuItems[1].Text);
			}
		}

		public void TestShowShipmentWithChildEditableServiceStates()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_HouseBill = "12093487";
			shipment.OuterPackLines.AddNew();
			Factory.Save();

			QuickPODs quickPODs = new QuickPODs(Factory);
			using (QuickPODForm quickPODForm = new QuickPODForm(quickPODs))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				quickPODForm.Show();

				quickPODForm.ShipmentMenuItem_Click(null, EventArgs.Empty);

				QuickPOD pod1 = quickPODs.QuickPODsCollection.AddNew();

				CommonShipment shipment1 = Factory.New<CommonShipment>();
				shipment1.JS_UniqueConsignRef = "s1";
				pod1.Shipment = shipment1;

				quickPODForm.QuickPODGridInternal.Select(0);

				quickPODForm.ShipmentMenuItem_Click(null, EventArgs.Empty);

				AssertEquals("Should set Shipment State", ChildEditableServiceStates.Shipment, ChildEditableService.GetState(pod1.Shipment.Factory));
			}
		}

		public void TestEditShipmentMenuItemClick()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_HouseBill = "12093487";
			shipment.OuterPackLines.AddNew();
			Factory.Save();

			QuickPODs quickPODs = new QuickPODs(Factory);
			using (QuickPODForm quickPODForm = new QuickPODForm(quickPODs))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				quickPODForm.Show();
				AssertEquals("No SelectedElements Expected", 0, quickPODForm.QuickPODGridInternal.SelectedElements.Length);
				quickPODForm.ShipmentMenuItem_Click(null, EventArgs.Empty);
				AssertEquals("No Shipment Selected", "Please select Shipments.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				QuickPOD pod1 = quickPODs.QuickPODsCollection.AddNew();
				quickPODForm.QuickPODGridInternal.Select(0);
				AssertEquals("First SelectedElement should be pod1", pod1, quickPODForm.QuickPODGridInternal.SelectedElements[0]);
				quickPODForm.ShipmentMenuItem_Click(null, EventArgs.Empty);
				AssertEquals("Invalid Shipment Selected", @"Can not edit all selected shipments:
- Not all selected shipments have a House Bill #.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				pod1.HouseBill = "12131516";
				AssertEquals("First SelectedElement should be pod1", pod1, quickPODForm.QuickPODGridInternal.SelectedElements[0]);
				quickPODForm.ShipmentMenuItem_Click(null, EventArgs.Empty);
				AssertEquals("Invalid Shipment Selected", @"Can not edit all selected shipments:
- Shipment with House Bill '12131516' not found.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				pod1.HouseBill = "12093487";
				AssertEquals("First SelectedElement should be pod1", pod1, quickPODForm.QuickPODGridInternal.SelectedElements[0]);
				quickPODForm.ShipmentMenuItem_Click(null, EventArgs.Empty);
				AssertNull("Valid Shipment Selected - no msg expected", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("JobShipment Controlled ID should have been used", ControllerIDs.JobShipment, quickPODForm.lastControllerInternal.ID);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				pod1.HouseBill = "";
				QuickPOD pod2 = quickPODs.QuickPODsCollection.AddNew();
				quickPODForm.QuickPODGridInternal.Select(0);
				quickPODForm.QuickPODGridInternal.Select(1);
				Assert("SelectedElements should contain pod1", Array.Exists(quickPODForm.QuickPODGridInternal.SelectedElements, x => x == pod1));
				Assert("SelectedElements should contain pod2", Array.Exists(quickPODForm.QuickPODGridInternal.SelectedElements, x => x == pod2));
				quickPODForm.ShipmentMenuItem_Click(null, EventArgs.Empty);
				AssertEquals("Two Invalid Shipments Selected", @"Can not edit all selected shipments:
- Not all selected shipments have a House Bill #.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				pod1.HouseBill = "12131516";
				Assert("SelectedElements should contain pod1", Array.Exists(quickPODForm.QuickPODGridInternal.SelectedElements, x => x == pod1));
				Assert("SelectedElements should contain pod2", Array.Exists(quickPODForm.QuickPODGridInternal.SelectedElements, x => x == pod2));
				quickPODForm.ShipmentMenuItem_Click(null, EventArgs.Empty);
				AssertEquals("Two Invalid Shipments Selected", @"Can not edit all selected shipments:
- Not all selected shipments have a House Bill #.
- Shipment with House Bill '12131516' not found.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				pod2.HouseBill = "22131516";
				Assert("SelectedElements should contain pod1", Array.Exists(quickPODForm.QuickPODGridInternal.SelectedElements, x => x == pod1));
				Assert("SelectedElements should contain pod2", Array.Exists(quickPODForm.QuickPODGridInternal.SelectedElements, x => x == pod2));
				quickPODForm.ShipmentMenuItem_Click(null, EventArgs.Empty);
				AssertEquals("Two Invalid Shipments Selected", @"Can not edit all selected shipments:
- Shipment with House Bill '12131516' not found.
- Shipment with House Bill '22131516' not found.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				pod1.HouseBill = "12093487";
				Assert("SelectedElements should contain pod1", Array.Exists(quickPODForm.QuickPODGridInternal.SelectedElements, x => x == pod1));
				Assert("SelectedElements should contain pod2", Array.Exists(quickPODForm.QuickPODGridInternal.SelectedElements, x => x == pod2));
				quickPODForm.ShipmentMenuItem_Click(null, EventArgs.Empty);
				AssertEquals("One Invalid Shipments Selected", @"Can not edit all selected shipments:
- Shipment with House Bill '22131516' not found.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("JobShipment Controlled ID should have been used", ControllerIDs.JobShipment, quickPODForm.lastControllerInternal.ID);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				pod2.HouseBill = "12093487";
				Assert("SelectedElements should contain pod1", Array.Exists(quickPODForm.QuickPODGridInternal.SelectedElements, x => x == pod1));
				Assert("SelectedElements should contain pod2", Array.Exists(quickPODForm.QuickPODGridInternal.SelectedElements, x => x == pod2));
				quickPODForm.ShipmentMenuItem_Click(null, EventArgs.Empty);
				AssertNull("Valid Shipments Selected - no msg expected", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("JobShipment Controlled ID should have been used", ControllerIDs.JobShipment, quickPODForm.lastControllerInternal.ID);

				if (quickPODForm.lastControllerInternal != null && quickPODForm.lastControllerInternal.LastShownForm != null)
				{
					quickPODForm.lastControllerInternal.LastShownForm.Dispose();
				}
			}
		}
	}
}
