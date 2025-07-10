using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.GPS.Testing;
using Enterprise.GPS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	internal class JobCartageRunSheetMainControlTest : TestCaseWithFactory
	{
		public void TestSplit()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			var workSheet = Factory.New<CommonWorkSheet>();
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			var leg1 = move.CartageLegs.AddNew();
			var leg2 = move.CartageLegs.AddNew();
			var leg3 = move.CartageLegs.AddNew();
			var leg4 = move.CartageLegs.AddNew();
			workSheet.CartageLegs.Add(leg1);
			workSheet.CartageLegs.Add(leg2);
			workSheet.CartageLegs.Add(leg3);
			workSheet.CartageLegs.Add(leg4);
			leg4.JU_RunSheetSequence = leg3.JU_RunSheetSequence;
			using (var form = new CartageWorkSheetForm(workSheet))
			{
				form.Show();
				form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.SelectAllElements();
				form.jobCartageRunSheetMainControl1.UnGroupButton.PerformClick();
				AssertEquals("Selected legs should all have the same sequence number.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Splitting Legs", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		public void TestContextMenus()
		{
			using (JobCartageRunSheetMainControl control = new JobCartageRunSheetMainControl())
			{
				AssertNotNull(control.WorkSheetModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Calculate distance"));
			}
		}

		public void TestAddGPSEvent()
		{
			using (var control = new JobCartageRunSheetMainControl())
			{
				AssertEquals(true, Env.CurrentUser.IsSupportUser);
				AssertEquals("AddGPSEvent should be visible", true, control.AddGPSEvent.Visible);
			}

			GlbStaff.CurrentUser.GS_LoginName = "test_notsupport";
			using (var control = new JobCartageRunSheetMainControl())
			{
				AssertEquals(false, Env.CurrentUser.IsSupportUser);
				AssertEquals("AddGPSEvent should not be visible", false, control.AddGPSEvent.Visible);
			}
		}

		public void TestAddGPSEvent_Shows_GPSClientActivityTestDataEntryForm_AndRefreshEventGrid()
		{
			var year = ZDateTime.Now.Year;
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			var truck = Factory.New<RefEquipment>();
			var workSheet = Factory.New<CommonWorkSheet>();
			workSheet.EY_RQ_Truck = truck.PK;
			workSheet.EY_StartTime = new ZDateTime(year, 01, 01);
			workSheet.EY_EndTime = new ZDateTime(year, 01, 02);
			var helper = new GPSTestHelper(Factory);
			var activity1 = helper.CreateActivity("", new ZDateTime(year, 01, 01, 01, 00, 00), GPSConstants.GPSEventTypeList.Codes.Custom, truck.PK);
			var activity2 = helper.CreateActivity("", new ZDateTime(year, 01, 01, 02, 00, 00), GPSConstants.GPSEventTypeList.Codes.Custom, truck.PK);
			using (var form = new CartageWorkSheetForm(workSheet))
			{
				form.Show();
				form.jobCartageRunSheetMainControl1.ShowGPSEventsCheckBox.Visible = true;
				form.jobCartageRunSheetMainControl1.ShowGPSEventsCheckBox.CheckState = CheckState.Checked;
				var eventsGrid = form.jobCartageRunSheetMainControl1.GPSEventsGrid;
				AssertEquals("There should be 2 rows", 2, eventsGrid.VisibleRowCount);
				helper.CreateActivity("", new ZDateTime(year, 01, 01, 02, 00, 00), GPSConstants.GPSEventTypeList.Codes.Custom, truck.PK);
				AssertEquals("There should be 2 rows", 2, eventsGrid.VisibleRowCount);
				form.jobCartageRunSheetMainControl1.AddGPSEvent.PerformClick();
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("GPSClientActivityTestDataEntryForm dialog was shown", typeof(GPSClientActivityTestDataEntryForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("There should be 3 rows", 3, eventsGrid.VisibleRowCount);
			}
		}

		public void TestUpdateLegFromEvent()
		{
			var year = ZDateTime.Now.Year;
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			var truck = Factory.New<RefEquipment>();
			var workSheet = Factory.New<CommonWorkSheet>();
			workSheet.EY_RQ_Truck = truck.PK;
			workSheet.EY_StartTime = new ZDateTime(year, 01, 01);
			workSheet.EY_EndTime = new ZDateTime(year, 01, 02);
			workSheet.CartageLegs.ApplySort(JobContainerLegsSchema.Constants.JU_RunSheetSequence, ListSortDirection.Ascending);
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			var leg1 = move.CartageLegs.AddNew();
			var leg2 = move.CartageLegs.AddNew();
			var leg3 = move.CartageLegs.AddNew();
			var leg4 = move.CartageLegs.AddNew();
			var leg5 = move.CartageLegs.AddNew();
			var leg6 = move.CartageLegs.AddNew();
			workSheet.CartageLegs.Add(leg1);
			workSheet.CartageLegs.Add(leg2);
			workSheet.CartageLegs.Add(leg3);
			workSheet.CartageLegs.Add(leg4);
			workSheet.CartageLegs.Add(leg5);
			workSheet.CartageLegs.Add(leg6);
			var helper = new GPSTestHelper(Factory);
			var activity1 = helper.CreateActivity("", new ZDateTime(year, 01, 01, 01, 00, 00), GPSConstants.GPSEventTypeList.Codes.Custom, truck.PK);
			var activity2 = helper.CreateActivity("", new ZDateTime(year, 01, 01, 02, 00, 00), GPSConstants.GPSEventTypeList.Codes.Custom, truck.PK);
			var activity3 = helper.CreateActivity("", new ZDateTime(year, 01, 01, 03, 00, 00), GPSConstants.GPSEventTypeList.Codes.Custom, truck.PK);
			var activity4 = helper.CreateActivity("", new ZDateTime(year, 01, 01, 04, 00, 00), GPSConstants.GPSEventTypeList.Codes.Custom, truck.PK);
			var activity5 = helper.CreateActivity("", new ZDateTime(year, 01, 01, 05, 00, 00), GPSConstants.GPSEventTypeList.Codes.Custom, truck.PK);
			var activity6 = helper.CreateActivity("", new ZDateTime(year, 01, 01, 06, 00, 00), GPSConstants.GPSEventTypeList.Codes.Custom, truck.PK);
			using (var frm = new CartageWorkSheetForm(workSheet))
			{
				frm.Show();
				frm.jobCartageRunSheetMainControl1.ShowGPSEventsCheckBox.Visible = true;
				frm.jobCartageRunSheetMainControl1.ShowGPSEventsCheckBox.CheckState = CheckState.Checked;
				var legsGrid = frm.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid;
				var eventsGrid = frm.jobCartageRunSheetMainControl1.GPSEventsGrid;
				legsGrid.Select(0);
				eventsGrid.Select(0);
				AssertEquals("Pre-condidtion", ZDateTime.Empty, leg1.JU_PickupTimeIn);
				frm.jobCartageRunSheetMainControl1.UpdatePickInButton.PerformClick();
				AssertEquals("Pickup time in not updated", activity1.EN_ActivityTime, leg1.JU_PickupTimeIn);
				legsGrid.UnSelect(0);
				eventsGrid.UnSelect(0);
				legsGrid.Select(1);
				eventsGrid.Select(1);
				AssertEquals("Pre-condidtion", ZDateTime.Empty, leg2.JU_PickupTimeOut);
				frm.jobCartageRunSheetMainControl1.UpdatePickOutButton.PerformClick();
				AssertEquals("Pickup time out not updated", activity2.EN_ActivityTime, leg2.JU_PickupTimeOut);
				legsGrid.UnSelect(1);
				eventsGrid.UnSelect(1);
				legsGrid.Select(2);
				eventsGrid.Select(2);
				AssertEquals("Pre-condidtion", ZDateTime.Empty, leg3.JU_DeliverTimeIn);
				frm.jobCartageRunSheetMainControl1.UpdateDeliverInButton.PerformClick();
				AssertEquals("Deliver time in not updated", activity3.EN_ActivityTime, leg3.JU_DeliverTimeIn);
				legsGrid.UnSelect(2);
				eventsGrid.UnSelect(2);
				legsGrid.Select(3);
				eventsGrid.Select(3);
				AssertEquals("Pre-condidtion", ZDateTime.Empty, leg4.JU_DeliverTimeOut);
				frm.jobCartageRunSheetMainControl1.UpdateDeliverOutButton.PerformClick();
				AssertEquals("Deliver time out not updated", activity4.EN_ActivityTime, leg4.JU_DeliverTimeOut);
				legsGrid.UnSelect(3);
				eventsGrid.UnSelect(3);
				legsGrid.Select(4);
				eventsGrid.Select(4);
				AssertEquals("Pre-condidtion", ZDateTime.Empty, leg5.JU_WaitPointTimeIn);
				frm.jobCartageRunSheetMainControl1.UpdateWaitPointInButton.PerformClick();
				AssertEquals("Wait Point time in not updated", activity5.EN_ActivityTime, leg5.JU_WaitPointTimeIn);
				legsGrid.UnSelect(4);
				eventsGrid.UnSelect(4);
				legsGrid.Select(5);
				eventsGrid.Select(5);
				AssertEquals("Pre-condidtion", ZDateTime.Empty, leg6.JU_WaitPointTimeOut);
				frm.jobCartageRunSheetMainControl1.UpdateWaitPointOutButton.PerformClick();
				AssertEquals("Wait Point time out not updated", activity6.EN_ActivityTime, leg6.JU_WaitPointTimeOut);
				legsGrid.UnSelect(5);
				eventsGrid.UnSelect(5);
				var notify = UnitTestUserNotification.Instance;
				var expectedMessage = "You need to select only one event and one leg to update. All grouped legs will be updated together even if you select only one of them.";
				AssertEquals(null, notify.LastMessage.Text);
				frm.jobCartageRunSheetMainControl1.UpdateWaitPointOutButton.PerformClick();
				AssertEquals("No leg or event selected", expectedMessage, notify.LastMessage.Text);
				notify.ClearMessagesAndAnswers();
				legsGrid.Select(1);
				AssertEquals(null, notify.LastMessage.Text);
				frm.jobCartageRunSheetMainControl1.UpdateWaitPointOutButton.PerformClick();
				AssertEquals("No event selected", expectedMessage, notify.LastMessage.Text);
				legsGrid.UnSelect(1);
				notify.ClearMessagesAndAnswers();
				eventsGrid.Select(1);
				AssertEquals(null, notify.LastMessage.Text);
				frm.jobCartageRunSheetMainControl1.UpdateWaitPointOutButton.PerformClick();
				AssertEquals("No leg selected", expectedMessage, notify.LastMessage.Text);
				eventsGrid.UnSelect(1);
				notify.ClearMessagesAndAnswers();
				legsGrid.Select(1);
				eventsGrid.Select(1);
				eventsGrid.Select(2);
				AssertEquals(null, notify.LastMessage.Text);
				frm.jobCartageRunSheetMainControl1.UpdateWaitPointOutButton.PerformClick();
				AssertEquals("Multiple events selected", expectedMessage, notify.LastMessage.Text);
				legsGrid.UnSelect(1);
				eventsGrid.UnSelectAll();
				eventsGrid.UnSelect(2);
				notify.ClearMessagesAndAnswers();
				legsGrid.Select(1);
				legsGrid.Select(2);
				eventsGrid.Select(1);
				AssertEquals(null, notify.LastMessage.Text);
				frm.jobCartageRunSheetMainControl1.UpdateWaitPointOutButton.PerformClick();
				AssertEquals("Multiple legs selected", expectedMessage, notify.LastMessage.Text);
				legsGrid.UnSelectAll();
				eventsGrid.UnSelect(1);
				notify.ClearMessagesAndAnswers();
			}
		}

		public void TestMoveSortSelect()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			workSheet.CartageLegs.ApplySort(JobContainerLegsSchema.Constants.JU_RunSheetSequence, ListSortDirection.Ascending);
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg1 = move.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move.CartageLegs.AddNew();
			CommonCartageLeg leg3 = move.CartageLegs.AddNew();
			CommonCartageLeg leg4 = move.CartageLegs.AddNew();
			workSheet.CartageLegs.Add(leg1);
			workSheet.CartageLegs.Add(leg2);
			workSheet.CartageLegs.Add(leg3);
			workSheet.CartageLegs.Add(leg4);
			using (CartageWorkSheetForm form = new CartageWorkSheetForm(workSheet))
			{
				form.Show();
				AssertEquals(leg1, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.List[0]);
				AssertEquals(leg2, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.List[1]);
				AssertEquals(leg3, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.List[2]);
				AssertEquals(leg4, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.List[3]);
				form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.Position = 3;
				form.jobCartageRunSheetMainControl1.MoveUpButton.PerformClick();
				AssertEquals(leg1, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.List[0]);
				AssertEquals(leg2, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.List[1]);
				AssertEquals(leg4, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.List[2]);
				AssertEquals(leg3, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.List[3]);
				AssertEquals(2, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.Position);
				AssertEquals(1, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.SelectedElements.Length);
				AssertEquals(leg4, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.SelectedElements[0]);
				form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.Select(1);
				AssertEquals(2, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.SelectedElements.Length);
				form.jobCartageRunSheetMainControl1.GroupButton.PerformClick();
				AssertEquals(2, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.SelectedElements.Length);
				form.jobCartageRunSheetMainControl1.MoveDownButton.PerformClick();
				AssertEquals(2, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.SelectedElements.Length);
				AssertEquals(leg1, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.List[0]);
				AssertEquals(leg3, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.List[1]);
				//2 & 4 in any order 
			}
		}

		public void TestMoveUpOrDownWhenNoLongerLinkedToRunsheet()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			var leg1 = move.CartageLegs.AddNew();
			var leg2 = move.CartageLegs.AddNew();
			var leg3 = move.CartageLegs.AddNew();
			var workSheet = Factory.New<CommonWorkSheet>();
			workSheet.CartageLegs.ApplySort(JobContainerLegsSchema.Constants.JU_RunSheetSequence, ListSortDirection.Ascending);
			workSheet.CartageLegs.Add(leg1);
			workSheet.CartageLegs.Add(leg2);
			workSheet.CartageLegs.Add(leg3);
			using (var form = new CartageWorkSheetForm(workSheet))
			{
				form.Show();
				AssertEquals(leg1, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.List[0]);
				AssertEquals(leg2, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.List[1]);
				AssertEquals(leg3, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.List[2]);
				using (((IBusinessObjectCollection)workSheet.CartageLegs).SuspendListChanged())
				{
					form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.Position = 1;
					leg2.JU_EY_RunSheet = ZGuid.Empty;
					AssertNoExceptionThrown("Should not throw an exception if the Leg is not attached to a runsheet.", () => form.jobCartageRunSheetMainControl1.MoveDownButton.PerformClick());
					AssertNoExceptionThrown("Should not throw an exception if the Leg is not attached to a runsheet.", () => form.jobCartageRunSheetMainControl1.MoveUpButton.PerformClick());
				}
			}
		}

		public void TestOrder()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			workSheet.CartageLegs.ApplySort(JobContainerLegsSchema.Constants.JU_RunSheetSequence, ListSortDirection.Ascending);
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg1 = move.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move.CartageLegs.AddNew();
			CommonCartageLeg leg3 = move.CartageLegs.AddNew();
			CommonCartageLeg leg4 = move.CartageLegs.AddNew();
			workSheet.CartageLegs.Add(leg1);
			workSheet.CartageLegs.Add(leg2);
			workSheet.CartageLegs.Add(leg3);
			workSheet.CartageLegs.Add(leg4);
			leg1.JU_PlannedPickupTime = ZDateTime.Now.AddDays(2);
			leg2.JU_PlannedPickupTime = ZDateTime.Now.AddDays(4);
			leg3.JU_PlannedPickupTime = ZDateTime.Now.AddDays(3);
			leg4.JU_PlannedPickupTime = ZDateTime.Now.AddDays(1);
			using (CartageWorkSheetForm form = new CartageWorkSheetForm(workSheet))
			{
				form.Show();
				AssertEquals(leg1, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.List[0]);
				AssertEquals(leg2, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.List[1]);
				AssertEquals(leg3, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.List[2]);
				AssertEquals(leg4, form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid.ListManager.List[3]);
				AssertEquals(1, leg1.JU_RunSheetSequence);
				AssertEquals(2, leg2.JU_RunSheetSequence);
				AssertEquals(3, leg3.JU_RunSheetSequence);
				AssertEquals(4, leg4.JU_RunSheetSequence);
				form.jobCartageRunSheetMainControl1.OrderButton.PerformClick();
				AssertEquals(2, leg1.JU_RunSheetSequence);
				AssertEquals(4, leg2.JU_RunSheetSequence);
				AssertEquals(3, leg3.JU_RunSheetSequence);
				AssertEquals(1, leg4.JU_RunSheetSequence);
			}
		}

		public void TestDragAndDropHandlers()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			MethodInfo onDragEnter = typeof(ZGrid).GetMethod("OnDragEnter", BindingFlags.Instance | BindingFlags.NonPublic);
			MethodInfo onDragDrop = typeof(ZGrid).GetMethod("OnDragDrop", BindingFlags.Instance | BindingFlags.NonPublic);
			using (CartageWorkSheetForm form = new CartageWorkSheetForm(workSheet))
			{
				form.Show();
				CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
				ArrayList n = new ArrayList();
				n.Add(leg);
				DataObject d = new DataObject(n);
				DragEventArgs e = new DragEventArgs(d, 0, 10, 10, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
				onDragEnter.Invoke(form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid, new object[] { e });
				AssertEquals(DragDropEffects.None, e.Effect);
				onDragDrop.Invoke(form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid, new object[] { e });
				AssertEquals(ZGuid.Empty, leg.JU_EY_RunSheet);
				Factory.Save();
				onDragEnter.Invoke(form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid, new object[] { e });
				AssertEquals(DragDropEffects.None, e.Effect);
				onDragDrop.Invoke(form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid, new object[] { e });
				AssertEquals(ZGuid.Empty, leg.JU_EY_RunSheet);
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				CommonCartageLeg legNewFactory = newFactory.Load<CommonCartageLeg>(leg.PK);
				n.Remove(leg);
				n.Add(legNewFactory);
				onDragDrop.Invoke(form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid, new object[] { e });
				leg.Factory.Save();
				legNewFactory.Factory.Save();
				AssertEquals(workSheet.PK, legNewFactory.JU_EY_RunSheet);
				AssertEquals(workSheet.PK, leg.JU_EY_RunSheet);
			}
		}

		[TestDate(2010, 2, 2, 11, 56, 0)]
		public void TestDragAndDropWithTruckCapacityExceeded()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			RefEquipment truck = Factory.New<RefEquipment>();
			BusinessObjectFactory newFact = new BusinessObjectFactory();
			CommonCartage cartage = newFact.New<CommonCartage>();
			CommonBookedCtgMove move1 = newFact.New<CommonBookedCtgMove>();
			CommonBookedCtgMove move2 = newFact.New<CommonBookedCtgMove>();
			cartage.BookedMovesCollection.Add(move1);
			cartage.BookedMovesCollection.Add(move2);
			move1.EW_WeightUQ = Enterprise.Core.Constants.Weight.Kilograms;
			move2.EW_WeightUQ = Enterprise.Core.Constants.Weight.Kilograms;
			move1.EW_VolumeUQ = Enterprise.Core.Constants.Volume.Litre;
			move2.EW_VolumeUQ = Enterprise.Core.Constants.Volume.Litre;
			truck.RQ_ShortCode = "HEYTHERE";
			truck.RQ_WeightCapacity = 2000;
			truck.RQ_WeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
			truck.RQ_CubicCapacity = 3000;
			truck.RQ_CubicUnit = Enterprise.Core.Constants.Volume.Litre;
			move1.EW_BookedWeight = 1900;
			move2.EW_BookedWeight = 600;
			move1.EW_BookedVolume = 2900;
			move2.EW_BookedVolume = 500;
			CommonCartageLeg leg1 = move1.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move2.CartageLegs.AddNew();
			ZDateTime now = ZDateTime.Now;
			leg1.JU_PlannedPickupTime = now;
			leg1.JU_EstimatedDeliveryTime = now.AddHours(2);
			leg2.JU_PlannedPickupTime = now.AddMinutes(20);
			leg2.JU_EstimatedDeliveryTime = now.AddHours(1);
			workSheet.EY_StartTime = now.AddDays(-1);
			workSheet.EY_EndTime = now.AddDays(1);
			leg1.JU_SplitDeliverySuffix = "l1";
			leg2.JU_SplitDeliverySuffix = "l2";
			workSheet.EY_RQ_Truck = truck.PK;
			workSheet.CartageLegs.Add(leg1);
			MethodInfo onDragDrop = typeof(ZGrid).GetMethod("OnDragDrop", BindingFlags.Instance | BindingFlags.NonPublic);
			using (CartageWorkSheetForm form = new CartageWorkSheetForm(workSheet))
			{
				form.Show();
				ArrayList n = new ArrayList();
				n.Add(leg2);
				DataObject d = new DataObject(n);
				DragEventArgs e = new DragEventArgs(d, 0, 10, 10, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Factory.Save();
				newFact.Save();
				onDragDrop.Invoke(form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid, new object[] { e });
				AssertEquals("Adding this Port Transport Leg to this Run Sheet exceeds the allocated Truck's capacity. Are you sure? \r\n\r\nBetween 02-Feb-10 12:16:00 and 02-Feb-10 12:56:00, the vehicle's Weight Capacity  of 2000 KG has been exceeded by 500.  The Port Transport Legs in this period are  T00001000/l1,T00001000/l2\r\nBetween 02-Feb-10 12:16:00 and 02-Feb-10 12:56:00, the vehicle's Cubic Capacity  of 3000 L has been exceeded by 400.  The Port Transport Legs in this period are  T00001000/l1,T00001000/l2", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, workSheet.CartageLegs.Count);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				onDragDrop.Invoke(form.jobCartageRunSheetMainControl1.WorkSheetModuleButtonGrid.InnerGrid, new object[] { e });
				AssertEquals("Adding this Port Transport Leg to this Run Sheet exceeds the allocated Truck's capacity. Are you sure? \r\n\r\nBetween 02-Feb-10 12:16:00 and 02-Feb-10 12:56:00, the vehicle's Weight Capacity  of 2000 KG has been exceeded by 500.  The Port Transport Legs in this period are  T00001000/l1,T00001000/l2\r\nBetween 02-Feb-10 12:16:00 and 02-Feb-10 12:56:00, the vehicle's Cubic Capacity  of 3000 L has been exceeded by 400.  The Port Transport Legs in this period are  T00001000/l1,T00001000/l2", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(2, workSheet.CartageLegs.Count);
			}
		}

		public void TestRemoveQuickAllocationBinding()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			using (CartageWorkSheetForm form = new CartageWorkSheetForm(workSheet))
			{
				form.Show();
				CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
				form.jobCartageRunSheetMainControl1.CartageLegsControlWithDetails.CartageLegPanel.CartageLegHasWaitPoint = true;
				workSheet.CartageLegs.Add(leg);
				int found = 0;
				AssertControls(ref found, form.jobCartageRunSheetMainControl1.CartageLegsControlWithDetails.CartageLegPanel);
				AssertEquals("Should have found all controls", 6, found);
			}
		}

		public void TestWhenShowSequenceRegistryYes_ShowOptimizeButton()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			using (CartageWorkSheetForm form = new CartageWorkSheetForm(workSheet))
			using (TransportCommon.Registry.TransportRegistry.Instance.ShowSequence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				form.Show();
				AssertEquals("Button should be visible when registry is set to 'Yes'", true, form.jobCartageRunSheetMainControl1.OptimizeButton.Visible);
			}
		}

		public void TestWhenShowSequenceRegistryNo_DontShowOptimizeButton()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			using (CartageWorkSheetForm form = new CartageWorkSheetForm(workSheet))
			using (TransportCommon.Registry.TransportRegistry.Instance.ShowSequence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				form.Show();
				AssertEquals("Button should not be visible when registry is set to 'No'", false, form.jobCartageRunSheetMainControl1.OptimizeButton.Visible);
			}
		}

		void AssertControls(ref int found, Control container)
		{
			foreach (Control control in container.Controls)
			{
				switch (control.Name)
				{
					case "zDateEdit17":
						AssertControl(ref found, "CartageLegs.JU_PlannedPickupTime", control);
						break;
					case "zDateEdit12":
						AssertControl(ref found, "CartageLegs.JU_EstimatedDeliveryTime", control);
						break;
					case "DeliveryETADateEdit":
						AssertControl(ref found, "CartageLegs.JU_EstimatedDeliveryTime", control);
						break;
					case "zGuidFindBox2":
						AssertControl(ref found, "CartageLegs.WorkSheet+EY_RQ_Truck", control);
						break;
					case "DriverCodeFindBox":
						AssertControl(ref found, "CartageLegs.WorkSheet+EY_GS_NKTruckDriver", control);
						break;
					case "zGuidFindBox4":
						AssertControl(ref found, "CartageLegs.WorkSheet+EY_OH_TransportCo", control);
						break;
					default:
						AssertControls(ref found, control);
						break;
				}
			}
		}

		void AssertControl(ref int found, string binding, Control control)
		{
			AssertEquals(binding, ((IDataBoundControl)control).DataMember);
			found++;
		}
	}
}
