using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	[TestedType(typeof(CartageLegPlannerForm))]
	public class CartageLegPlannerFormTest : ZFormBasherTest
	{
		public void TestRunSheetDashboard()
		{
			using (CartageLegPlannerFormForTest form = new CartageLegPlannerFormForTest(Factory))
			{
				form.Show();
				form.RunSheetDashboardButton.PerformClick();
				AssertEquals("RunSheetDashboardForm", form.dashBoardForm.GetType().Name);
				form.dashBoardForm.Dispose();
			}
		}

		public void TestNewRunSheetButton()
		{
			using (CartageLegPlannerFormForTest form = new CartageLegPlannerFormForTest(Factory))
			{
				form.Show();
				form.NewRunSheetButton.PerformClick();
				Application.DoEvents();
				ZForm runSheetForm = (ZForm)form.LastUsedControllerForTest.LastShownForm;
				AssertEquals("CartageWorkSheetForm", runSheetForm.GetType().Name);
				runSheetForm.Dispose();
			}
		}

		public void TestNewRunSheetButtonWithLimitedAccess()
		{
			bool originalValue = Env.Security.LocalTransportRunSheetNew.IsAllowed;
			try
			{
				using (var form = new CartageLegPlannerForm(Factory))
				{
					Env.Security.LocalTransportRunSheetNew.IsAllowed = false;
					Factory.Save();
					form.Show();
					AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () => form.NewRunSheetButton.PerformClick());
					AssertNull(form.LastUsedControllerForTest.LastShownForm);
					Env.Security.LocalTransportRunSheetNew.IsAllowed = true;
					AssertNoExceptionThrown(() => form.NewRunSheetButton.PerformClick());
					using (var runSheetForm = (ZForm)form.LastUsedControllerForTest.LastShownForm)
					{
						AssertEquals(typeof(CartageWorkSheetForm), runSheetForm.GetType());
					}
				}
			}
			finally
			{
				Env.Security.LocalTransportRunSheetNew.IsAllowed = originalValue;
			}
		}

		public void TestNewRunSheetButtonWithSelectedLegs()
		{
			using (var form = new CartageLegPlannerFormForTest(Factory))
			{
				var move1 = Factory.New<CommonCartage>().BookedMovesCollection.AddNew();
				var move2 = Factory.New<CommonCartage>().BookedMovesCollection.AddNew();
				var leg1 = Factory.New<CommonCartageLeg>();
				leg1.JU_PlannedPickupTime = ZDateTime.Now;
				leg1.JU_EY_RunSheet = Factory.New<CommonWorkSheet>().PK;
				leg1.JU_EW = move1.PK;
				var leg2 = Factory.New<CommonCartageLeg>();
				leg2.JU_PlannedPickupTime = ZDateTime.Now.AddDays(4);
				leg2.JU_EW = move2.PK;
				var leg3 = Factory.New<CommonCartageLeg>();
				leg3.JU_PlannedPickupTime = ZDateTime.Now.AddDays(-3);
				leg3.JU_EW = move2.PK;
				Factory.Save();
				form.Show();
				form.cartageLegsFilterStripUserControl.Find();
				form.cartageLegsFilterStripUserControl.Grid.SelectAllElements();
				form.NewRunSheetButton.PerformClick();
				Application.DoEvents();
				using (var runSheetForm = (ZForm)form.LastUsedControllerForTest.LastShownForm)
				{
					var runSheet = (CommonWorkSheet)runSheetForm.BusinessEntity;
					var runSheetFactory = runSheet.Factory;
					var leg2_inRunSheetFactory = runSheetFactory.Load<CommonCartageLeg>(leg2.PK);
					var leg3_inRunSheetFactory = runSheetFactory.Load<CommonCartageLeg>(leg3.PK);
					Assert("should allocate selected leg to the runsheet", runSheet.CartageLegs.Contains(leg2_inRunSheetFactory));
					Assert("should allocate selected leg to the runsheet", runSheet.CartageLegs.Contains(leg3_inRunSheetFactory));
					runSheet.Factory.Save();
					AssertNotNull(leg2_inRunSheetFactory.WorkSheet);
					AssertNotNull(leg3_inRunSheetFactory.WorkSheet);
					AssertEquals(leg3_inRunSheetFactory.JU_PlannedPickupTime.Date, runSheet.EY_StartTime);
					AssertEquals(leg2_inRunSheetFactory.JU_PlannedPickupTime.EndOfDay().AddSeconds(-59), runSheet.EY_EndTime);
				}
			}
		}

		[TestDate(2018, 11, 1)]
		public void TestNewRunSheetButtonWithSelectedLegs_WhenTimesAreEmpty()
		{
			using (var form = new CartageLegPlannerFormForTest(Factory))
			{
				var move = Factory.New<CommonCartage>().BookedMovesCollection.AddNew();
				var leg = Factory.New<CommonCartageLeg>();
				leg.JU_EW = move.PK;
				Factory.Save();
				form.Show();
				form.cartageLegsFilterStripUserControl.Find();
				form.cartageLegsFilterStripUserControl.Grid.SelectAllElements();
				form.NewRunSheetButton.PerformClick();
				Application.DoEvents();
				using (var runSheetForm = (ZForm)form.LastUsedControllerForTest.LastShownForm)
				{
					var runSheet = (CommonWorkSheet)runSheetForm.BusinessEntity;
					var runSheetFactory = runSheet.Factory;
					var legInRunSheetFactory = runSheetFactory.Load<CommonCartageLeg>(leg.PK);
					Assert("should allocate selected leg to the runsheet", runSheet.CartageLegs.Contains(legInRunSheetFactory));
					runSheet.Factory.Save();
					AssertNotNull(legInRunSheetFactory.WorkSheet);
					AssertEquals("Should have Created a Run Sheet for Today.", new ZDateTime(2018, 11, 1), runSheet.EY_StartTime);
					AssertEquals("Should have Created a Run Sheet for Today.", new ZDateTime(2018, 11, 1, 23, 59, 0), runSheet.EY_EndTime);
				}
			}
		}

		public void TestNewRunSheetButton_LoadDeletedLegs()
		{
			using (var form = new CartageLegPlannerFormForTest_DeletedLegsWhenCreateNew(Factory))
			{
				var factory = new BusinessObjectFactory();
				var move1 = factory.New<CommonCartage>().BookedMovesCollection.AddNew();
				var move2 = factory.New<CommonCartage>().BookedMovesCollection.AddNew();
				var baseTime = ZDateTime.Now;
				var leg1 = factory.New<CommonCartageLeg>();
				leg1.JU_PlannedPickupTime = baseTime;
				leg1.JU_EY_RunSheet = factory.New<CommonWorkSheet>().PK;
				leg1.JU_EW = move1.PK;
				leg1.JU_SystemCreateTimeUtc = baseTime;
				var leg2 = factory.New<CommonCartageLeg>();
				leg2.JU_PlannedPickupTime = baseTime.AddDays(4);
				leg2.JU_EW = move2.PK;
				leg2.JU_SystemCreateTimeUtc = baseTime.AddMinutes(1); // as when it is saved to the db it rounds seconds away, so we use minutes to differentiate records and ensure leg 3 is deleted
				var leg3 = factory.New<CommonCartageLeg>();
				leg3.JU_PlannedPickupTime = baseTime.AddDays(-3);
				leg3.JU_EW = move2.PK;
				leg3.JU_SystemCreateTimeUtc = baseTime.AddMinutes(2); // as when it is saved to the db it rounds seconds away, so we use minutes to differentiate records and ensure leg 3 is deleted
				factory.Save();
				form.Show();
				form.cartageLegsFilterStripUserControl.Find();
				form.cartageLegsFilterStripUserControl.Grid.SelectAllElements();
				form.NewRunSheetButton.PerformClick();
				Application.DoEvents();
				using (var runSheetForm = (ZForm)form.LastUsedControllerForTest.LastShownForm)
				{
					var runSheet = (CommonWorkSheet)runSheetForm.BusinessEntity;
					var runSheetFactory = runSheet.Factory;
					var leg2_inRunSheetFactory = runSheetFactory.Load<CommonCartageLeg>(leg2.PK);
					var leg3_inRunSheetFactory = runSheetFactory.Load<CommonCartageLeg>(leg3.PK);
					Assert("should allocate selected leg to the runsheet", runSheet.CartageLegs.Contains(leg2_inRunSheetFactory));
					Assert("should not allocate selected leg to the runsheet", !runSheet.CartageLegs.Contains(leg3_inRunSheetFactory));
					runSheet.Factory.Save();
					AssertNotNull(leg2_inRunSheetFactory.WorkSheet);
					AssertEquals(leg2_inRunSheetFactory.JU_PlannedPickupTime.EndOfDay().AddSeconds(-59), runSheet.EY_EndTime);
				}

				Assert("DeveloperException has been reported", ErrorReporter.TotalErrorCount > 0);
				AssertContains("DeveloperException contains correct data", @"Developer Error: Should not be accessing a property on a deleted business object
TableName: JobContainerLegs
Property name: JU_SplitDeliverySuffix
Business Object Type: Enterprise.Freight.LocalCartage.Business.CommonCartageLeg
DataRowState: Deleted", ErrorReporter.LastMessageReported);
				AssertContains("DeveloperException contains correct key", string.Format("PK: {0}", leg3.PK), ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestMinSize()
		{
			using (CartageLegPlannerForm form = new CartageLegPlannerForm(Factory))
			{
				form.Show();
				AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 289), form.MinimumSize);
				form.ShowSidePanelCheckBox.Checked = true;
				AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 289), form.MinimumSize);
			}
		}

		public void TestCartageLegsFilterStripUserControl()
		{
			using (CartageLegPlannerForm form = new CartageLegPlannerForm(Factory))
			{
				form.Show();
				AssertNotNull("Should have been loaded fine", form.cartageLegsFilterStripUserControl);
			}
		}

		public void TestShowCartageLegDetails()
		{
			using (CartageLegPlannerForm form = new CartageLegPlannerForm(Factory))
			{
				form.Show();
				form.ShowCartageLegDetails = true;
				Assert(form.CartageLegsControlWithDetails.Visible);
				form.Size = form.MinimumSize;
				Assert(!form.CartageLegsControlWithDetails.Visible);
				form.ShowCartageLegDetails = true;
				Assert(form.CartageLegsControlWithDetails.Visible);
				Assert(form.Size.Height > form.MinimumSize.Height);
				Assert(form.Size.Width > form.MinimumSize.Width);
			}
		}

		public void TestDragAndDropHandlersForLegPlanner()
		{
			var year = ZDateTime.Now.Year;
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			MethodInfo onDragEnter = typeof(ZGrid).GetMethod("OnDragEnter", BindingFlags.Instance | BindingFlags.NonPublic);
			MethodInfo onDragDrop = typeof(ZGrid).GetMethod("OnDragDrop", BindingFlags.Instance | BindingFlags.NonPublic);
			using (CartageLegPlannerForm form = new CartageLegPlannerForm(Factory))
			{
				var driver = Factory.New<GlbStaff>();
				driver.GS_FullName = "truck driver";
				driver.GS_IsActive = true;
				driver.GS_IsResource = false;
				driver.GS_Code = "cfk";
				var driverGroup = Factory.New<GlbGroup>();
				driver.Groups.Add(driverGroup);
				var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
				transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, driverGroup.PK.ToGuid());
				Factory.Save();
				form.Show();
				form.ShowSidePanelCheckBox.Checked = true;
				var leg = Factory.New<CommonCartageLeg>();
				leg.JU_PlannedPickupTime = new ZDateTime(year, 1, 1);
				var leg2 = Factory.New<CommonCartageLeg>();
				leg2.JU_PlannedPickupTime = new ZDateTime(year, 1, 1);
				var n = new ArrayList();
				n.Add(leg);
				n.Add(leg2);
				var d = new DataObject(n);
				var point = form.DriversGrid.PointToScreen(new Point(30, 30));
				var e = new DragEventArgs(d, 0, point.X, point.Y, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
				Factory.Save();
				onDragEnter.Invoke(form.DriversGrid, new object[] { e });
				AssertEquals(DragDropEffects.Copy | DragDropEffects.Scroll, e.Effect);
				onDragDrop.Invoke(form.DriversGrid, new object[] { e });
				AssertEquals(driver.GS_Code, leg.QuickGSDriver);
				AssertEquals(driver.GS_Code, leg2.QuickGSDriver);
				Factory.Save();
				AssertEquals(driver.GS_Code, leg.WorkSheet.EY_GS_NKTruckDriver);
				AssertEquals(leg.WorkSheet, leg2.WorkSheet);
				AssertEquals(1, leg.JU_RunSheetSequence);
				AssertEquals(2, leg2.JU_RunSheetSequence);
				AssertEquals("First leg should not have validation error", false, leg.HasErrors);
				AssertEquals(false, leg2.HasErrors);
				var truck = Factory.New<RefEquipment>();
				truck.RQ_IsVehicle = true;
				truck.RQ_ShortCode = "sc";
				n = new ArrayList();
				n.Add(leg);
				d = new DataObject(n);
				point = form.VehiclesGrid.PointToScreen(new Point(30, 30));
				e = new DragEventArgs(d, 0, point.X, point.Y, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
				Factory.Save();
				onDragEnter.Invoke(form.VehiclesGrid, new object[] { e });
				AssertEquals(DragDropEffects.Copy | DragDropEffects.Scroll, e.Effect);
				form.VehiclesGrid.Visible = true;
				form.DriversGrid.Visible = false;
				onDragDrop.Invoke(form.VehiclesGrid, new object[] { e });
				AssertEquals(leg.QuickRQTruck, truck.PK);
			}
		}

		[TestDate(2010, 2, 2, 11, 56, 0)]
		public void TestDragAndDropHandlersForLegPlannerWhenTruckCapacityIsExceeded()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			MethodInfo onDragDrop = typeof(ZGrid).GetMethod("OnDragDrop", BindingFlags.Instance | BindingFlags.NonPublic);
			var workSheet = Factory.New<CommonWorkSheet>();
			var truck = Factory.New<RefEquipment>();
			var newFact = new BusinessObjectFactory();
			var cartage = newFact.New<CommonCartage>();
			var move1 = newFact.New<CommonBookedCtgMove>();
			var move2 = newFact.New<CommonBookedCtgMove>();
			cartage.BookedMovesCollection.Add(move1);
			cartage.BookedMovesCollection.Add(move2);
			move1.EW_WeightUQ = Enterprise.Core.Constants.Weight.Kilograms;
			move2.EW_WeightUQ = Enterprise.Core.Constants.Weight.Kilograms;
			move1.EW_VolumeUQ = Enterprise.Core.Constants.Volume.Litre;
			move2.EW_VolumeUQ = Enterprise.Core.Constants.Volume.Litre;
			truck.RQ_ShortCode = "BHAPPY";
			truck.RQ_WeightCapacity = 2000;
			truck.RQ_WeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
			truck.RQ_CubicCapacity = 3000;
			truck.RQ_CubicUnit = Enterprise.Core.Constants.Volume.Litre;
			move1.EW_BookedWeight = 1900;
			move2.EW_BookedWeight = 600;
			move1.EW_BookedVolume = 2900;
			move2.EW_BookedVolume = 500;
			var leg1 = move1.CartageLegs.AddNew();
			var leg2 = move2.CartageLegs.AddNew();
			var now = ZDateTime.Now;
			leg1.JU_PlannedPickupTime = now;
			leg1.JU_EstimatedDeliveryTime = now.AddHours(2);
			leg2.JU_PlannedPickupTime = now.AddMinutes(20);
			leg2.JU_EstimatedDeliveryTime = now.AddHours(1);
			workSheet.EY_StartTime = now;
			workSheet.EY_EndTime = now.AddDays(1);
			leg1.JU_SplitDeliverySuffix = "l1";
			leg2.JU_SplitDeliverySuffix = "l2";
			workSheet.EY_RQ_Truck = truck.PK;
			workSheet.CartageLegs.Add(leg1);
			using (CartageLegPlannerForm form = new CartageLegPlannerForm(Factory))
			{
				form.Show();
				form.ShowSidePanelCheckBox.Checked = true;
				form.CartageLegPlanner.DriversWorkSheetSidePanelMode = "Run Sheets - Today";
				var n = new ArrayList();
				n.Add(leg2);
				var d = new DataObject(n);
				var point = form.WorkSheetsGrid.PointToScreen(new Point(30, 30));
				var e = new DragEventArgs(d, 0, point.X, point.Y, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Factory.Save();
				newFact.Save();
				onDragDrop.Invoke(form.WorkSheetsGrid, new object[] { e });
				AssertEquals("Adding this Port Transport Leg to this Run Sheet exceeds the allocated Truck's capacity. Are you sure? \r\n\r\nBetween 02-Feb-10 12:16:00 and 02-Feb-10 12:56:00, the vehicle's Weight Capacity  of 2000 KG has been exceeded by 500.  The Port Transport Legs in this period are  T00001000/l1,T00001000/l2\r\nBetween 02-Feb-10 12:16:00 and 02-Feb-10 12:56:00, the vehicle's Cubic Capacity  of 3000 L has been exceeded by 400.  The Port Transport Legs in this period are  T00001000/l1,T00001000/l2", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, workSheet.CartageLegs.Count);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				onDragDrop.Invoke(form.WorkSheetsGrid, new object[] { e });
				AssertEquals("Adding this Port Transport Leg to this Run Sheet exceeds the allocated Truck's capacity. Are you sure? \r\n\r\nBetween 02-Feb-10 12:16:00 and 02-Feb-10 12:56:00, the vehicle's Weight Capacity  of 2000 KG has been exceeded by 500.  The Port Transport Legs in this period are  T00001000/l1,T00001000/l2\r\nBetween 02-Feb-10 12:16:00 and 02-Feb-10 12:56:00, the vehicle's Cubic Capacity  of 3000 L has been exceeded by 400.  The Port Transport Legs in this period are  T00001000/l1,T00001000/l2", UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();
				newFact.Save();
				AssertEquals(2, workSheet.CartageLegs.Count);
			}
		}

		public void TestRunSheetSecurityGUIProvider_Register()
		{
			using (var form = new CartageLegPlannerFormForTest(Factory))
			{
				form.Show();
				var beforeFindFactory = form.BusinessEntity.Factory;
				var beforeFindProvider = RunSheetSecurityProvider.GetProvider(beforeFindFactory);
				AssertNotNull(beforeFindProvider);
				AssertEquals(typeof(RunSheetSecurityGUIProvider), beforeFindProvider.GetType());
				form.cartageLegsFilterStripUserControl.Find();
				var afterFindFactory = form.BusinessEntity.Factory;
				var afterFindProvider = RunSheetSecurityProvider.GetProvider(form.BusinessEntity.Factory);
				AssertNotNull(afterFindProvider);
				AssertEquals(typeof(RunSheetSecurityGUIProvider), afterFindProvider.GetType());
				AssertNotEquals("Should have been unregistered", beforeFindProvider, RunSheetSecurityProvider.GetProvider(beforeFindFactory));
			}
		}

		public void TestINotificationsAdd()
		{
			using (var form = new CartageLegPlannerFormForTest(Factory))
			{
				form.Show();
				((INotifications)form).Add(new NotificationTest());
				AssertEquals("Message should be shown to the user.", "Test Message", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		class NotificationTest : INotification
		{
			string INotification.Message
			{
				get
				{
					return "Test Message";
				}
			}

			INotification INotification.ReplaceMessage(string message)
			{
				throw new NotImplementedException();
			}

			INotificationType INotification.Type
			{
				get
				{
					throw new NotImplementedException();
				}
			}
		}

		public void TestOverlappingRunSheetsWithSameDriver()
		{
			var now = ZDateTime.Now;
			var factory = new BusinessObjectFactory();
			var driver = factory.New<GlbStaff>();
			driver.GS_FullName = "Truck Driver Bob";
			driver.GS_IsActive = true;
			driver.GS_IsResource = false;
			driver.GS_Code = "TDB";
			var move1 = factory.New<CommonCartage>().BookedMovesCollection.AddNew();
			var move2 = factory.New<CommonCartage>().BookedMovesCollection.AddNew();
			var vehicle1 = factory.New<RefEquipment>();
			vehicle1.RQ_ShortCode = "TR1";
			vehicle1.RQ_IsVehicle = true;
			vehicle1.RQ_Registration = "TR1";
			var vehicle2 = factory.New<RefEquipment>();
			vehicle2.RQ_ShortCode = "TR2";
			vehicle2.RQ_IsVehicle = true;
			vehicle2.RQ_Registration = "TR2";
			var ws1 = factory.New<CommonWorkSheet>();
			ws1.EY_StartTime = now;
			ws1.EY_EndTime = now.AddHours(1);
			ws1.EY_RQ_Truck = vehicle1.PK;
			var ws2 = factory.New<CommonWorkSheet>();
			ws2.EY_StartTime = now;
			ws2.EY_EndTime = now.AddHours(1);
			ws2.EY_RQ_Truck = vehicle2.PK;
			ws2.EY_GS_NKTruckDriver = driver.GS_Code;
			var leg1 = factory.New<CommonCartageLeg>();
			leg1.JU_PlannedPickupTime = now;
			leg1.JU_EY_RunSheet = ws1.PK;
			leg1.JU_EW = move1.PK;
			var leg2 = factory.New<CommonCartageLeg>();
			leg2.JU_PlannedPickupTime = now;
			leg2.JU_EY_RunSheet = ws2.PK;
			leg2.JU_EW = move2.PK;
			factory.Save();
			using (var form = new CartageLegPlannerFormForTest_DeletedLegsWhenCreateNew(Factory))
			{
				form.Show();
				form.cartageLegsFilterStripUserControl.Find();
				form.cartageLegsFilterStripUserControl.Grid.SelectAllElements(b => b.PK == leg1.PK);
				var leg1FromList = form.cartageLegsFilterStripUserControl.Grid.ListManager.List.Cast<CommonCartageLeg>().First(l => l.PK == leg1.PK);
				leg1FromList.QuickGSDriver = driver.GS_Code;
				form.Refresh();
				form.FireValidateAllForTest();
				Assert("Should have errors", form.BusinessEntity.Notifications.Any(n => n.Message.Contains("overlaps")));
			}
		}

		class CartageLegPlannerFormForTest : CartageLegPlannerForm
		{
			public CartageLegPlannerFormForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override IZForm GetFormForDashboard()
			{
				dashBoardForm = base.GetFormForDashboard();
				return dashBoardForm;
			}

			public IZForm dashBoardForm;
		}

		class CartageLegPlannerFormForTest_DeletedLegsWhenCreateNew : CartageLegPlannerFormForTest
		{
			public CartageLegPlannerFormForTest_DeletedLegsWhenCreateNew(BusinessObjectFactory factory) : base(factory)
			{
			}

			internal override void RecordControllerForTest(ZController controller)
			{
				var selectedLegs = cartageLegsFilterStripUserControl.Grid.GetSelectedElements<CommonCartageLeg>().OrderBy(l => l.JU_SystemCreateTimeUtc).ToArray();
				var factory = new BusinessObjectFactory();
				if (selectedLegs.Length > 1)
				{
					var lastLegs = factory.Load<CommonCartageLeg>(selectedLegs[selectedLegs.Length - 1].PK);
					lastLegs.Delete();
					factory.Save();
				}

				base.RecordControllerForTest(controller);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new CartageLegPlannerForm(Factory);
		}

		protected override void SetUp()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			base.SetUp();
		}
	}
}
