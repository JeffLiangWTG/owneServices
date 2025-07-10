using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	[TestedType(typeof(WarehouseEntryForm))]
	class WarehouseEntryFormBasherTest : ZFormBasherTest
	{
		#region TestHandleSaveException_ShowsMessageWhenMakingInvalidWarehouseTypeChange

		public void TestHandleSaveException_ShowsMessageWhenMakingInvalidWarehouseTypeChange()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A");
			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			using (var form = new WarehouseEntryForm(warehouse))
			{
				warehouse.WW_IsVirtualWarehouse = true;
				form.ValidatingForSave += delegate
				{
					var factory = form.BusinessEntity.Factory;
					var connection = ((IDbConnected)factory).Connection;
					throw new ZSaveException(new ZDataException(new ArgumentException(WhsWarehouse.CannotChangeWarehouseTypeToOrFromTransitIfAlreadyReferencedTriggerID),
						((INeedRow)form.BusinessEntity).Row, connection), factory);
				};

				UnitTestUserNotification.Instance.ClearMessages();
				form.FireSaveButton();
			}

			AssertEquals("Cannot Change Warehouse Type as this Warehouse is already in use.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region TestUpdateParametersGroupBoxesVisibility

		public void TestUpdateParametersGroupBoxesVisibility()
		{
			var whs = Helper.CreateWarehouse("WH1");
			using (var form = new WarehouseEntryFormForTest(whs))
			{
				form.Show();
				form.SelectParametersTabPageForTest();

				var controls = form.Controls;
				var productWhsGroupBoxes = new[]
				{
					controls.Find("AutoPrintGroupBox", searchAllChildren: true).Single(),
					controls.Find("ABCAnalysisGroupBox", searchAllChildren: true).Single(),
					controls.Find("PackageIDsGroupBox", searchAllChildren: true).Single(),
					controls.Find("FinalisedDateGroupBox", searchAllChildren: true).Single(),
					controls.Find("PickPackGroupBox", searchAllChildren: true).Single(),
					controls.Find("VerifyEmptyLocationsGroupBox", searchAllChildren: true).Single(),
					controls.Find("IsPickByUOMGroupBox", searchAllChildren: true).Single(),
					controls.Find("RFGroupBox", searchAllChildren: true).Single(),
					controls.Find("ReleasingGroupBox", searchAllChildren: true).Single(),
					controls.Find("DefaultOutboundDoorFindBox", searchAllChildren: true).Single(),
				};
				var transitWhsGroupBoxes = new[]
				{
					controls.Find("TransitSecurityGroupBox", searchAllChildren: true).Single(),
					controls.Find("TransitLoadingGroupBox", searchAllChildren: true).Single(),
				};
				var dockDoorGroupBox = controls.Find("DockDoorGroupBox", searchAllChildren: true).Single();
				var dockDoorGroupBoxes = new[] { dockDoorGroupBox };

				var defaultDockDoorGroupLocation = ControlDpiScalingHelper.NewScaledPoint(375, 374, true);
				var defaultDockDoorGroupSize = ControlDpiScalingHelper.NewScaledSize(350, 109, true);
				var transitDockDoorGroupLocation = ControlDpiScalingHelper.NewScaledPoint(3, 424, true);
				var transitDockDoorGroupSize = ControlDpiScalingHelper.NewScaledSize(350, 70, true);

				var transitCustomsGroupBox = controls.Find("TransitCustomsGroupBox", searchAllChildren: true).Single();
				var transitCustomsGroupBoxs = new[] { transitCustomsGroupBox };

				var defaultTransitCustomsGroupLocation = ControlDpiScalingHelper.NewScaledPoint(3, 590, true);
				var defaultTransitCustomsGroupSize = ControlDpiScalingHelper.NewScaledSize(350, 48, true);
				var transitCustomsGroupLocation = ControlDpiScalingHelper.NewScaledPoint(3, 541, true);
				var transitCustomsGroupSize = ControlDpiScalingHelper.NewScaledSize(350, 48, true);

				const bool IS_VISIBLE = true;
				whs.WW_WarehouseType = WarehouseTypes.Codes.Product;
				AssertControlVisibility(productWhsGroupBoxes, IS_VISIBLE);
				AssertControlVisibility(transitWhsGroupBoxes, !IS_VISIBLE);
				AssertControlVisibility(dockDoorGroupBoxes, IS_VISIBLE);
				AssertControlVisibility(transitCustomsGroupBoxs, !IS_VISIBLE);
				AssertControlLocationAndSize(dockDoorGroupBox, defaultDockDoorGroupLocation, defaultDockDoorGroupSize);
				AssertControlLocationAndSize(transitCustomsGroupBox, defaultTransitCustomsGroupLocation, defaultTransitCustomsGroupSize);

				whs.WW_WarehouseType = WarehouseTypes.Codes.Transit;
				AssertControlVisibility(productWhsGroupBoxes, !IS_VISIBLE);
				AssertControlVisibility(transitWhsGroupBoxes, IS_VISIBLE);
				AssertControlVisibility(dockDoorGroupBoxes, IS_VISIBLE);
				AssertControlVisibility(transitCustomsGroupBoxs, IS_VISIBLE);
				AssertControlLocationAndSize(dockDoorGroupBox, transitDockDoorGroupLocation, transitDockDoorGroupSize);
				AssertControlLocationAndSize(transitCustomsGroupBox, transitCustomsGroupLocation, transitCustomsGroupSize);

				whs.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
				AssertControlVisibility(productWhsGroupBoxes, IS_VISIBLE);
				AssertControlVisibility(transitWhsGroupBoxes, !IS_VISIBLE);
				AssertControlVisibility(dockDoorGroupBoxes, IS_VISIBLE);
				AssertControlVisibility(transitCustomsGroupBoxs, !IS_VISIBLE);
				AssertControlLocationAndSize(dockDoorGroupBox, defaultDockDoorGroupLocation, defaultDockDoorGroupSize);
				AssertControlLocationAndSize(transitCustomsGroupBox, defaultTransitCustomsGroupLocation, defaultTransitCustomsGroupSize);

				whs.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
				AssertControlVisibility(productWhsGroupBoxes, !IS_VISIBLE);
				AssertControlVisibility(dockDoorGroupBoxes, !IS_VISIBLE);
				AssertControlVisibility(transitWhsGroupBoxes, !IS_VISIBLE);
				AssertControlVisibility(transitCustomsGroupBoxs, !IS_VISIBLE);
				AssertControlLocationAndSize(transitCustomsGroupBox, defaultTransitCustomsGroupLocation, defaultTransitCustomsGroupSize);
			}
		}

		public void TestDetailedTrackingGroupBoxVisibility()
		{
			var whs = Helper.CreateWarehouse("WH1");
			using (var form = new WarehouseEntryFormForTest(whs))
			{
				form.Show();
				form.SelectParametersTabPageForTest();

				var controls = form.Controls;
				var ftzWhsGroupBoxes = new[] { controls.Find("DetailedTrackingGroupBox", searchAllChildren: true).Single() };

				const bool IS_VISIBLE = true;
				whs.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
				AssertControlVisibility(ftzWhsGroupBoxes, !IS_VISIBLE);

				var uSAddress = Factory.New<OrgAddress>();
				uSAddress.OA_RL_NKRelatedPortCode = "USLAX";

				whs.WW_OA_WarehouseAddress = uSAddress.PK;
				AssertControlVisibility(ftzWhsGroupBoxes, IS_VISIBLE);

				var pRAddress = Factory.New<OrgAddress>();
				pRAddress.OA_RL_NKRelatedPortCode = "PRADJ";

				whs.WW_OA_WarehouseAddress = pRAddress.PK;
				AssertControlVisibility(ftzWhsGroupBoxes, IS_VISIBLE);

				whs.WW_WarehouseType = WarehouseTypes.Codes.Product;
				AssertControlVisibility(ftzWhsGroupBoxes, !IS_VISIBLE);

				whs.WW_WarehouseType = WarehouseTypes.Codes.Transit;
				AssertControlVisibility(ftzWhsGroupBoxes, !IS_VISIBLE);

				whs.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
				AssertControlVisibility(ftzWhsGroupBoxes, !IS_VISIBLE);
			}
		}

		void AssertControlVisibility(IEnumerable<Control> controls, bool expectIsVisible)
		{
			CombineAssertions("", () =>
			{
				foreach (var control in controls)
				{
					AssertEquals($"{control.Name} should {(expectIsVisible ? "" : "not ")}be visible.", expectIsVisible, control.Visible);
				}
			});
		}

		void AssertControlLocationAndSize(Control control, Point location, Size size)
		{
			AssertEquals("Control location does not match.", location, control.Location);
			AssertEquals("Control size does not match.", size, control.Size);
		}

		#region TestCycleCountGroupBoxVisibility

		public void TestCycleCountGroupBoxVisibility()
		{
			TestCycleCountGroupBoxVisibilityWithWarehouseTypeCore(WarehouseTypes.Codes.ContainerYard, false);
			TestCycleCountGroupBoxVisibilityWithWarehouseTypeCore(WarehouseTypes.Codes.FreeTradeZone, false);
			TestCycleCountGroupBoxVisibilityWithWarehouseTypeCore(WarehouseTypes.Codes.Transit, true);
			TestCycleCountGroupBoxVisibilityWithWarehouseTypeCore(WarehouseTypes.Codes.Product, true);
		}

		void TestCycleCountGroupBoxVisibilityWithWarehouseTypeCore(string warehouseType, bool expectedVisible)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = warehouseType;
			var row = Helper.CreateRow(warehouse, "row", 1, 1);

			using (var form = new WarehouseEntryFormForTest(warehouse))
			{
				form.Show();
				form.SelectParametersTabPageForTest();

				var cycleCountGroupBox = form.Controls.Find("CycleCountGroupBox", searchAllChildren: true).Single();
				AssertEquals($"CycleCountGroupBox visibility for Warehouse Type {warehouseType} should be {expectedVisible}", expectedVisible, cycleCountGroupBox.Visible);
			}
		}

		public void TestCycleCountGroupBoxVisibility_Location()
		{
			var transitLocation = ControlDpiScalingHelper.NewScaledPoint(3, 495, true);
			var productWarehouseLocation = ControlDpiScalingHelper.NewScaledPoint(3, 545, true);

			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			var row = Helper.CreateRow(warehouse, "row", 1, 1);

			using (var form = new WarehouseEntryFormForTest(warehouse))
			{
				form.Show();
				form.SelectParametersTabPageForTest();

				var cycleCountGroupBox = form.Controls.Find("CycleCountGroupBox", searchAllChildren: true).Single();
				AssertEquals("Should be visible.", true, cycleCountGroupBox.Visible);
				AssertEquals("Should be in the PWH location.", productWarehouseLocation, cycleCountGroupBox.Location);

				warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
				AssertEquals("Should be visible.", true, cycleCountGroupBox.Visible);
				AssertEquals("Should be in the TWH location.", transitLocation, cycleCountGroupBox.Location);

				warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
				AssertEquals("Should be visible.", true, cycleCountGroupBox.Visible);
				AssertEquals("Should be in the PWH location.", productWarehouseLocation, cycleCountGroupBox.Location);
			}
		}

		#endregion

		#region TestTaskManagementGroupBoxVisibility

		public void TestTaskManagementGroupBoxVisibility_ProductWarehouse_TaskManagementRegistryEnabled()
			=> TestTaskManagementGroupBoxVisibilityCore(WarehouseTypes.Codes.Product, taskManagementRegistryEnabled: true, taskManagementGroupBoxVisible: true);

		public void TestTaskManagementGroupBoxVisibility_ProductWarehouse_TaskManagementRegistryDisabled()
			=> TestTaskManagementGroupBoxVisibilityCore(WarehouseTypes.Codes.Product, taskManagementRegistryEnabled: false, taskManagementGroupBoxVisible: false);

		public void TestTaskManagementGroupBoxVisibility_FreeTradeZone_TaskManagementRegistryEnabled()
			=> TestTaskManagementGroupBoxVisibilityCore(WarehouseTypes.Codes.FreeTradeZone, taskManagementRegistryEnabled: true, taskManagementGroupBoxVisible: true);

		public void TestTaskManagementGroupBoxVisibility_FreeTradeZone_TaskManagementRegistryDisabled()
			=> TestTaskManagementGroupBoxVisibilityCore(WarehouseTypes.Codes.FreeTradeZone, taskManagementRegistryEnabled: false, taskManagementGroupBoxVisible: false);

		public void TestTaskManagementGroupBoxVisibility_Transit()
			=> TestTaskManagementGroupBoxVisibilityCore(WarehouseTypes.Codes.Transit, taskManagementRegistryEnabled: true, taskManagementGroupBoxVisible: false);

		public void TestTaskManagementGroupBoxVisibility_ContainerYard()
			=> TestTaskManagementGroupBoxVisibilityCore(WarehouseTypes.Codes.ContainerYard, taskManagementRegistryEnabled: true, taskManagementGroupBoxVisible: false);

		void TestTaskManagementGroupBoxVisibilityCore(string warehouseType, bool taskManagementRegistryEnabled, bool taskManagementGroupBoxVisible)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = warehouseType;

			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, taskManagementRegistryEnabled))
			using (var form = new WarehouseEntryFormForTest(warehouse))
			{
				form.Show();
				form.SelectParametersTabPageForTest();

				var taskManagementGroupBox = form.Controls.Find("TaskManagementGroupBox", searchAllChildren: true).Single();
				AssertEquals(taskManagementGroupBoxVisible, taskManagementGroupBox.Visible);
			}
		}

		#endregion

		#endregion

		#region TestTRWParametersGroupBoxesVisibility

		public void TestTRWParametersGroupBoxesVisibility()
		{
			var whs = Helper.CreateCYDWarehouse("WHS");

			using (var form = new WarehouseEntryFormForTest(whs))
			{
				form.Show();
				form.SelectParametersTabPageForTest();
				var controls = form.Controls;

				const bool IS_VISIBLE = true;

				var transitWhsGroupBoxes = new[]
				{
					controls.Find("TransitSecurityGroupBox", searchAllChildren: true).Single(),
					controls.Find("TransitLoadingGroupBox", searchAllChildren: true).Single(),
				};

				whs.WW_WarehouseType = WarehouseTypes.Codes.Transit;

				AssertControlVisibility(transitWhsGroupBoxes, IS_VISIBLE);

				whs.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;

				AssertControlVisibility(transitWhsGroupBoxes, !IS_VISIBLE);
			}
		}

		#endregion

		#region TestINotifications

		public void TestINotifications()
		{
			using (var form = new WarehouseEntryForm(null))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				INotifications notifications = form;
				notifications.AddError("Some Error");
				AssertEquals("Some Error", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		#endregion

		#region TestINotificationSubscriberQueryUser

		public void TestINotificationSubscriberQueryUser()
		{
			using (var form = new WarehouseEntryForm(null))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				INotificationSubscriberQueryUser notifications = form;
				var eventArgs = new QueryUserYesNoEventArgs("Test", "Hello", false);
				notifications.QueryUser(eventArgs);
				AssertEquals("Should have been a Question.", true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("Caption should be correct.", "Test", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Message should be correct.", "Hello", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Response should be true if user pressed yes.", true, eventArgs.Response);
			}
		}

		#endregion

		#region TestGroupBoxes

		public void TestGroupBoxes()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			using (var form = new WarehouseEntryFormForTest(warehouse))
			{
				form.Show();
				form.SelectParametersTabPageForTest();
				foreach (var groupBox in form.ParametersPanelGroupBoxes())
				{
					Assert(groupBox.Name + " is too narrow for translation", groupBox.Width >= 350);
				}
			}
		}

		#endregion

		#region TestMinimumSize

		public void TestMinimumSize()
		{
			var warehouse = Factory.New<WhsWarehouse>();

			using (var form = new WarehouseEntryForm(warehouse))
			{
				var minimumSize = ControlDpiScalingHelper.NewScaledSize(837, 725);
				var validSize = ControlDpiScalingHelper.NewScaledSize(1025, 904);
				var invalidSize = ControlDpiScalingHelper.NewScaledSize(125, 100);
				form.Size = validSize;
				AssertEquals("Precondition: Form should resize to entered values", validSize, form.Size);

				form.Size = invalidSize;
				AssertEquals("Form should resize to correct minimum values", minimumSize, form.Size);
			}
		}

		#endregion

		#region TestFixedWidthLocationParameters

		public void TestFixedWidthLocationParameters()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			using (var form = new WarehouseEntryFormForTest(warehouse))
			{
				form.Show();
				form.SelectParametersTabPageForTest();

				var controls = form.Controls;
				Assert(controls.Find("IsFixedWidthLocationCheckBox", true).Single().Visible);
				Assert(controls.Find("LocationTraysFixedWidthCalcEdit", true).Single().Visible);
				Assert(controls.Find("LocationLevelsFixedWidthCalcEdit", true).Single().Visible);
				Assert(controls.Find("LocationColumnsFixedWidthCalcEdit", true).Single().Visible);
			}
		}

		#endregion

		#region TestLocationComponentDelimiterCaption

		public void TestLocationComponentDelimiterCaption()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			using (var form = new WarehouseEntryFormForTest(warehouse))
			{
				form.Show();
				form.SelectParametersTabPageForTest();

				var controls = form.Controls;
				var locationComponentDelimiterTextBox = (ZTextBox)controls.Find("LocationComponentDelimiterTextBox", true).Single();
				AssertEquals("Location Component Delimiter", locationComponentDelimiterTextBox.CaptionResourceString.Caption);

				warehouse.IsFixedWidthLocation = true;
				AssertEquals("User Friendly Delimiter", locationComponentDelimiterTextBox.CaptionResourceString.Caption);
			}
		}

		public void TestLocationComponentDelimiterCaption_FixedWidthLocationWarehouse()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.IsFixedWidthLocation = true;
			using (var form = new WarehouseEntryFormForTest(warehouse))
			{
				form.Show();
				form.SelectParametersTabPageForTest();

				var controls = form.Controls;
				var locationComponentDelimiterTextBox = (ZTextBox)controls.Find("LocationComponentDelimiterTextBox", true).Single();
				AssertEquals("User Friendly Delimiter", locationComponentDelimiterTextBox.CaptionResourceString.Caption);

				warehouse.IsFixedWidthLocation = false;
				AssertEquals("Location Component Delimiter", locationComponentDelimiterTextBox.CaptionResourceString.Caption);
			}
		}

		#endregion

		#region TestMouseClick

		public void TestUNDGLimitGrid_MouseClick()
		{
			var whs = Helper.CreateTRWWarehouse("WH1");
			var undgLimit = Helper.CreateWhsUNDGLimit(whs, "0004a");
			Factory.Save();
			using (var form = new WarehouseEntryFormForTest(whs))
			{
				form.Show();
				form.SelectUNDGThresholdsTabPageForTest();

				var grid = form.FindSingleOrDefault<ZGrid>(o => o.Name == "UNDGLimitGrid");
				Thread.Sleep(1000);
				grid.PerformMouseDownForTest(0, 1);
				Application.DoEvents();

				AssertEquals("0004a is allowed to store up to 10 KG and 10 M3 in the warehouse", form.MessageStatusBarPanel.Text);
			}
		}

		#endregion

		#region Implementation

		#region Helper

		WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}

		WhsTestHelperFunctionsEnv helper;

		#endregion

		protected override Form GetFormToBashCore()
		{
			return new WarehouseEntryForm(Factory.New<WhsWarehouse>());
		}

		#endregion
	}
}
