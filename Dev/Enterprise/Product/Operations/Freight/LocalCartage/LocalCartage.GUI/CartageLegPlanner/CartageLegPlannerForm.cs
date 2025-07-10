using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.GUI
{
	[SuppressBindingMemberBashingTest] // for ShowRunSheetDetailsCheckBox
	public partial class CartageLegPlannerForm : ZForm, INotifications
	{
		public CartageLegPlannerForm(BusinessObjectFactory factory)
			: base(new CartageLegPlannerCollection(factory))
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			SetupCartageLegsFilterStripUserControl();
			this.SizeChanged += new EventHandler(CartageLegPlannerForm_SizeChanged);
		}

		void SetupCartageLegsFilterStripUserControl()
		{
			if (cartageLegsFilterStripUserControl == null)
			{
				SuspendLayout();
				Type userControlType = Type.GetType("Enterprise.Freight.LocalCartage.Module.CartageLegPlannerFilterControl, Enterprise.Freight.LocalCartage.Module", true);

				cartageLegsFilterStripUserControl = (ZFilterStripCommonControl)Activator.CreateInstance(userControlType);
				CartageLegsFilterPanel.Controls.Add(cartageLegsFilterStripUserControl);
				cartageLegsFilterStripUserControl.Dock = DockStyle.Fill;
				cartageLegsFilterStripUserControl.DockPadding.All = 3;
				cartageLegsFilterStripUserControl.TabIndex = 0;
				cartageLegsFilterStripUserControl.AutoSize = true;
				ShowCartageLegDetails = true;
				cartageLegsFilterStripUserControl.Layout += new LayoutEventHandler(cartageLegsFilterStripUserControl_Layout);

				new UNDGDataItemFormManager(cartageLegsFilterStripUserControl.Grid, "BookedCtgMove").Initialize(CartageLegsControlWithDetails.DGLinkLabel, CartageLegsControlWithDetails.DGSubstanceGuidFindBox, CartageLegsControlWithDetails.FlashPointCalcEdit, CartageLegsControlWithDetails.DGContactGuidFindBox);
				ResumeLayout(false);
			}
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (CartageLegPlannerCollection != null)
			{
				CartageLegPlannerCollection.CountChanged -= new CollectionCountChangedEventHandler(CartageLegPlannerCollection_CountChanged);
			}

			base.SetDataBinding(dataSource, dataMember);

			if (CartageLegPlannerCollection != null)
			{
				CartageLegPlannerCollection.CountChanged += new CollectionCountChangedEventHandler(CartageLegPlannerCollection_CountChanged);
			}

			OnCurrentDataItemChanged();
		}

		void CartageLegPlannerCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			OnCurrentDataItemChanged();
		}

		void OnCurrentDataItemChanged()
		{
			if (currentPlannerListening != null)
			{
				RunSheetSecurityGUIProvider.Unregister(currentPlannerListening.Factory);
				currentPlannerListening.DriversWorkSheetSidePanelModeInfo.ValueChanged -= new EventHandler(DriversWorkSheetSidePanelModeInfo_ValueChanged);

				CartageLegs_Listening.Clear();
			}

			currentPlannerListening = CartageLegPlanner;

			if (currentPlannerListening != null)
			{
				RunSheetSecurityGUIProvider.Register(currentPlannerListening.Factory);
				currentPlannerListening.DriversWorkSheetSidePanelModeInfo.ValueChanged += new EventHandler(DriversWorkSheetSidePanelModeInfo_ValueChanged);
			}

			OnFactoryChanged();
		}

		CartageLegPlanner currentPlannerListening;

		public bool ShowCartageLegDetails
		{
			get { return showCartageLegDetails; }
			set
			{
				if (showCartageLegDetails != value)
				{
					showCartageLegDetails = value;
					CartageLegsControlWithDetails.Visible = value;

					if (value && !DoCartageLegDetailsFit)
					{
						int widthDif = CartageLegsControlWithDetails.MinimumSize.Width - CartageLegsControlWithDetails.Parent.ClientRectangle.Width;
						int heightDif = CartageLegsSidePanelSplitContainer.MinimumSize.Height + CartageLegsControlWithDetails.MinimumSize.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(20) - CartageLegsControlWithDetails.Parent.ClientRectangle.Height;

						if (widthDif > 0)
						{
							ControlDpiScalingHelper.SetWidth(this, this.Width + widthDif, false);
						}
						if (heightDif > 0)
						{
							ControlDpiScalingHelper.SetHeight(this, this.Height + heightDif, false);
						}
					}
				}

				if (ShowRunSheetDetailsCheckBox.Checked != value)
				{
					ShowRunSheetDetailsCheckBox.Checked = value;
				}
			}
		}
		bool showCartageLegDetails;

		void CartageLegPlannerForm_SizeChanged(object sender, EventArgs e)
		{
			DoLayout();
		}

		void cartageLegsFilterStripUserControl_Layout(object sender, LayoutEventArgs e)
		{
			DoLayout();
		}

		void DoLayout()
		{
			if (!isInLayout)
			{
				try
				{
					isInLayout = true;

					if (!DoCartageLegDetailsFit)
					{
						ShowCartageLegDetails = false;
					}
				}
				finally
				{
					isInLayout = false;
				}
			}
		}
		bool isInLayout;

		bool DoCartageLegDetailsFit
		{
			get
			{
				return CartageLegsControlWithDetails.Parent.ClientRectangle.Width >= CartageLegsControlWithDetails.MinimumSize.Width &&
					CartageLegsControlWithDetails.Parent.ClientRectangle.Height >= CartageLegsSidePanelSplitContainer.MinimumSize.Height + CartageLegsControlWithDetails.MinimumSize.Height + 20;
			}
		}

		void ShowRunSheetDetailsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			ShowCartageLegDetails = ShowRunSheetDetailsCheckBox.Checked;
		}

		public CartageLegPlannerCollection CartageLegPlannerCollection
		{
			get { return (CartageLegPlannerCollection)DataSource; }
		}

		public CartageLegPlanner CartageLegPlanner
		{
			get
			{
				var planners = CartageLegPlannerCollection;
				return planners != null && planners.Count > 0 ? planners[0] : null;
			}
		}
		internal ZFilterStripCommonControl cartageLegsFilterStripUserControl;
		public ZFilterStripCommonControl cartageLegsFilterStripUserControlForTest => cartageLegsFilterStripUserControl;

		void Grid_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data != null && e.Data.GetDataPresent(typeof(ArrayList)))
			{
				var list = ((ArrayList)e.Data.GetData(typeof(ArrayList)));

				if (list.Count > 0 && list[0] != null && list[0].GetType().IsAssignableFrom(typeof(CommonCartageLeg)))
				{
					var commonCartageLegs = (CommonCartageLeg[])list.ToArray(typeof(CommonCartageLeg));
					e.Effect = commonCartageLegs.Length > 0 ? DragDropEffects.Scroll | DragDropEffects.Copy : DragDropEffects.None;
				}
			}
		}

		void Grid_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data != null && e.Data.GetDataPresent(typeof(ArrayList)))
			{
				var list = ((ArrayList)e.Data.GetData(typeof(ArrayList)));

				if (list.Count > 0 && list[0] != null && list[0].GetType().IsAssignableFrom(typeof(CommonCartageLeg)))
				{
					var grid = sender as ZGrid;
					var testInfo = grid.HitTest(grid.PointToClient(ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Y))));
					grid.UnSelectAll();

					if (testInfo.Row >= 0)
					{
						grid.Select(testInfo.Row);
					}
				}
			}
		}

		void Grid_DragDrop(object sender, DragEventArgs e)
		{
			ZGrid grid;
			if ((grid = sender as ZGrid) == null)
			{
				return;
			}

			bool isDriversGrid = grid == DriversGrid;
			bool isRunSheetsGrid = grid == WorkSheetsGrid;
			bool isVehiclesGrid = grid == VehiclesGrid;

			ZString gridDescription = isDriversGrid ? Res.GetString("34b51a83-6045-4944-a334-5bb5e48d31d2", "Driver") :
				isVehiclesGrid ? Res.GetString("c2422a74-aece-4e6f-a498-05dd90208bc7", "Vehicle") : Res.GetString("b8476f8c-4823-4ee3-aa8d-d55dfc6a1f37", "Run Sheet");

			BusinessObject driverOrRunSheetOrVehicle;

			if (e.Data != null && e.Data.GetDataPresent(typeof(ArrayList)))
			{
				ArrayList list = ((ArrayList)e.Data.GetData(typeof(ArrayList)));
				if (list.Count > 0 && list[0] != null && list[0].GetType().IsAssignableFrom(typeof(CommonCartageLeg)))
				{
					var xValue = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.X);
					var yValue = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Y);
					DataGrid.HitTestInfo testInfo = grid.HitTest(grid.PointToClient(ControlDpiScalingHelper.NewScaledPoint(xValue, yValue)));
					driverOrRunSheetOrVehicle = testInfo.Row >= 0 ? grid.ListManager.List[testInfo.Row] as BusinessObject : null;

					if (driverOrRunSheetOrVehicle != null)
					{
						var commonCartageLegs = (CommonCartageLeg[])list.ToArray(typeof(CommonCartageLeg));
						bool anyLegHaveChanges = false;
						foreach (var leg in commonCartageLegs)
						{
							if (leg.Factory != driverOrRunSheetOrVehicle.Factory && leg.HasChanges || !leg.IsInDatabase)
							{
								anyLegHaveChanges = true;
								break;
							}
						}

						bool anyLegHaveBegun = false;
						foreach (var leg in commonCartageLegs)
						{
							if (leg.QuickGSDriverInfo.ReadOnly)
							{
								anyLegHaveBegun = true;
								break;
							}
						}

						if (anyLegHaveChanges)
						{
							Globals.Message.ShowError(Res.GetString("24d03c12-5626-472f-bf22-35808151d98d", "Cannot attach Port Transport Legs to a {0} if they have changes. Please Save the Port Transport Legs before using drag/drop.", gridDescription));
						}
						else if (anyLegHaveBegun)
						{
							Globals.Message.ShowError(Res.GetString("b48af517-d5aa-441f-ab4b-4873930c1354", "Cannot attach Port Transport Legs to a {0} if they have already begun. You will need to update the Port Transport Legs manually.", gridDescription));
						}
						else
						{
							CartageLegGrid(isDriversGrid, isRunSheetsGrid, isVehiclesGrid, driverOrRunSheetOrVehicle, commonCartageLegs);
						}
					}
				}
			}
		}

		void CartageLegGrid(bool isDriversGrid, bool isRunSheetsGrid, bool isVehiclesGrid, BusinessObject driverOrRunSheetOrVehicle, CommonCartageLeg[] commonCartageLegs)
		{
			foreach (CommonCartageLeg cartageLeg in commonCartageLegs)
			{
				if (isDriversGrid)
				{
					GlbStaff driver = driverOrRunSheetOrVehicle as GlbStaff;
					if (driver != null)
					{
						cartageLeg.QuickGSDriver = driver.GS_Code;
					}
				}
				else if (isRunSheetsGrid)
				{
					CommonWorkSheet worksheet = driverOrRunSheetOrVehicle as CommonWorkSheet;
					if (worksheet != null)
					{
						RunSheetLegAttach(cartageLeg, worksheet);
					}
				}
				else if (isVehiclesGrid)
				{
					RefEquipment vehicle = driverOrRunSheetOrVehicle as RefEquipment;

					if (vehicle != null)
					{
						cartageLeg.QuickRQTruck = vehicle.PK;
					}
				}
			}

			CartageLegPlanner.RefreshBinding();
			CartageLegPlanner.RefreshBindingIncludingChildren();
			cartageLegsFilterStripUserControl.Refresh();
			Refresh();
		}

		void RunSheetLegAttach(CommonCartageLeg cartageLeg, CommonWorkSheet worksheet)
		{
			if (worksheet.HasRunSheetError)
			{
				Globals.Message.ShowError(Res.GetString("b4b59723-8ad3-402a-8ee5-a4a9f6d62743", "Cannot attach Port Transport Legs to a Run Sheet with Status: {0}", worksheet.StatusDescription));
			}
			else
			{
				ZString capacityWarning = new WorkSheetTruckCapacityChecker(worksheet).CheckTruckCapacity(cartageLeg);

				DialogResult response = DialogResult.Yes;

				if (!capacityWarning.IsEmpty)
				{
					capacityWarning = Res.GetString("72b12984-be78-44ca-8399-322d8f6f68aa", "Adding this Port Transport Leg to this Run Sheet exceeds the allocated Truck's capacity. Are you sure? \r\n\r\n{0}", capacityWarning);
					response = Globals.Message.Show(capacityWarning, Res.GetString("e002d591-2435-4fb2-a1ee-24f268e9260b", "Truck Capacity Exceeded"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				}

				if (response == DialogResult.Yes)
				{
					worksheet.CartageLegs.Add(cartageLeg);
				}
			}
		}

		void DriversWorkSheetSidePanelModeInfo_ValueChanged(object sender, EventArgs e)
		{
			DriversGrid.Visible = CartageLegPlanner.DriversWorkSheetSidePanelMode == CartageLegPlanner.DriversWorkSheetSidePanelMode_Drivers;
			VehiclesGrid.Visible = CartageLegPlanner.DriversWorkSheetSidePanelMode == CartageLegPlanner.DriversWorkSheetSidePanelMode_Vehicles;
			WorkSheetsGrid.Visible = !DriversGrid.Visible && !VehiclesGrid.Visible;
		}

		void SidePanelGrid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks == 2 && e.Button == MouseButtons.Left)
			{
				ZGrid sidePanelGrid = sender as ZGrid;
				if (sidePanelGrid.HitTest(e.X, e.Y).Row > -1)
				{
					HandleDoubleClick(sidePanelGrid);
				}
			}
		}

		void HandleDoubleClick(ZGrid driverOrRunSheetOrVehicleGrid)
		{
			bool hasCurrent = (driverOrRunSheetOrVehicleGrid != null && driverOrRunSheetOrVehicleGrid.ListManager != null && driverOrRunSheetOrVehicleGrid.ListManager.Position >= 0);
			BusinessObject currentSelection = hasCurrent ? (BusinessObject)driverOrRunSheetOrVehicleGrid.ListManager.GetCurrent() : null;

			if (currentSelection != null)
			{
				if (currentSelection is GlbStaff)
				{
					ZControllerFactory.Create(ControllerIDs.GlbStaff).ShowEditForm(currentSelection);
				}
				else if (currentSelection is CommonWorkSheet)
				{
					ZControllerFactory.Create(ControllerIDs.CartageWorkSheet).ShowEditForm(currentSelection);
				}
				else if (currentSelection is RefEquipment)
				{
					ZControllerFactory.Create(ControllerIDs.RefEquipment).ShowEditForm(currentSelection);
				}
				else
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "HandleDoubleClick - Doesn't support type: {0}", currentSelection.GetType().Name));
				}
			}
		}

		void ShowSidePanelCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			SidePanel.Visible = ShowSidePanelCheckBox.Checked;
			CartageLegsSidePanelSplitContainer.Panel2Collapsed = !ShowSidePanelCheckBox.Checked;

			if (ShowSidePanelCheckBox.Checked)
			{
				CartageLegsSidePanelSplitContainer.Panel2.Show();
				MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 289);
			}
			else
			{
				MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 289);
			}
		}

		void RunSheetDashboardButton_Click(object sender, EventArgs e)
		{
			GetFormForDashboard();
		}
		protected virtual IZForm GetFormForDashboard()
		{
			return ZControllerFactory.Create(ControllerIDs.CartageRunSheetDashboard).ShowNewForm();
		}

		void NewRunSheetButton_Click(object sender, EventArgs e)
		{
			var selectedCartageLegs = cartageLegsFilterStripUserControl.Grid.GetSelectedElements<CommonCartageLeg>();
			var controller = ZControllerFactory.Create(ControllerIDs.CartageWorkSheet);
#if DEBUG
			RecordControllerForTest(controller);
#endif
			var form = controller.ShowNewForm();
			if (form != null)
			{
				var runSheet = (CommonWorkSheet)((ZForm)form).BusinessEntity;

				if (selectedCartageLegs.Length > 0)
				{
					var startTime = ZDateTime.Empty;
					var endTime = ZDateTime.Empty;
					int count = 1;
					foreach (var leg in selectedCartageLegs)
					{
						if (leg.JU_EY_RunSheet.IsEmpty)
						{
							var legInRunSheetFactory = runSheet.Factory.Load<CommonCartageLeg>(leg.PK);
							var legsEarliestTime = leg.StartTime < leg.EndTime || leg.EndTime.IsEmpty ? leg.StartTime : leg.EndTime;
							var legsLatestTime = leg.StartTime > leg.EndTime || leg.EndTime.IsEmpty ? leg.StartTime : leg.EndTime;

							if (startTime.IsEmpty || legsEarliestTime < startTime)
							{
								startTime = legsEarliestTime;
							}

							if (endTime.IsEmpty || legsLatestTime > endTime)
							{
								endTime = legsLatestTime;
							}

							if (legInRunSheetFactory == null)
							{
								Globals.Message.ShowWarning(Res.GetString("4ac7f663-1d36-4647-9d7b-73564676d797", "Could not load the Transport Leg with selected row: {0}, Job Number/Unique ID: {1}. Reason: Deleted.", count, leg.UniqueIDWithJobNumber));
							}
							else
							{
								legInRunSheetFactory.JU_EY_RunSheet = runSheet.PK;
							}
						}
						count++;
					}

					runSheet.EY_StartTime = startTime.IsEmpty ? ZDateTime.Today : startTime.Date;
					runSheet.EY_EndTime = (endTime.IsEmpty ? runSheet.EY_StartTime : endTime).EndOfDay().AddSeconds(-59);
				}
			}
		}

		List<CommonCartageLeg> CartageLegs_Listening
		{
			get { return cartageLegs ?? (cartageLegs = new List<CommonCartageLeg>()); }
		}
		List<CommonCartageLeg> cartageLegs;

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification.Message);
		}

		internal ZController LastUsedControllerForTest { get; set; }
		internal List<CommonCartageLeg> CartageLegs_ListeningForTest { get { return CartageLegs_Listening; } }

		internal virtual void RecordControllerForTest(ZController controller)
		{
			LastUsedControllerForTest = controller;
		}
	}
}

