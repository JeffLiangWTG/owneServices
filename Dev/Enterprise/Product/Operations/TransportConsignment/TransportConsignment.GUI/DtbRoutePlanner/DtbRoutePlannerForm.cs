using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using static System.FormattableString;

namespace Enterprise.TransportConsignment.GUI
{
	public partial class DtbRoutePlannerForm : ZChildForm, INotifications
	{
		const DtbRoutePlannerViewMode DEFAULT_PLANNER_VIEWMODE = DtbRoutePlannerViewMode.NextAvailable;

		#region Construction

		public DtbRoutePlannerForm(DtbRoutePlannerCollection planners)
			: base(planners)
		{
			EnsurePlannerIsValid(planners);

			InitializeComponent();
			InitializeButtonText();
			InitializeFilterControl();
			InitializeHiddenButtons();
			InitializeDefaults();
			HookEvents();

#if !WINZOR
			ControlBoxToolStrip.Renderer = new ToolStripRendererWithDashedSeparator();
#endif

		}

		void EnsurePlannerIsValid(DtbRoutePlannerCollection planners)
		{
			if (planners == null || planners.Count != 1)
			{
				throw new ArgumentException("Route planners should contain 1 route planner to bind to.");
			}
		}

		void InitializeButtonText()
		{
			AssignPartialButton.Text = Res.GetString("d324f932-74d7-4fc6-b9b3-4e72bbda815a", "Partial\r\n{0}", AssignPartialButton.Text);
		}

		void InitializeFilterControl()
		{
			var iFilterControl = ObjectFactory.New<IDtbRoutePlannerFilterControl>();
			FilterControl = (ZFilterStripCommonControl)iFilterControl;

			FilterControl.Dock = DockStyle.Fill;
			FilterControl.BackColor = SystemColors.Control;
			FilterControl.Grid.IsWholeRowSelectedOnClick = true;
			EnableWhiteRowsOnGrid(FilterControl.Grid);
			iFilterControl.PerformingSearch += DtbRoutePlannerForm_PerformingSearch;
			iFilterControl.PerformedSearch += DtbRoutePlannerForm_PerformedSearch;
			FiltersPanel.Controls.Add(FilterControl);

			DriverFilterControl = (ZFilterStripCommonControl)ObjectFactory.New<IDtbDriverFilterControl>();
			CarrierFilterControl = (ZFilterStripCommonControl)ObjectFactory.New<IDtbCarrierFilterControl>();
			VehicleFilterControl = (ZFilterStripCommonControl)ObjectFactory.New<IDtbVehicleFilterControl>();
			AddChildFilterControl(CarrierFilterControl);
			AddChildFilterControl(DriverFilterControl);
			AddChildFilterControl(VehicleFilterControl);
		}

		void AddChildFilterControl(ZFilterStripCommonControl childControl)
		{
			childControl.Dock = DockStyle.Fill;
			childControl.BackColor = SystemColors.Control;
			childControl.Grid.IsWholeRowSelectedOnClick = true;
			RunSheetsVerticalSplitContainer.Panel1.Controls.Add(childControl);

			((IDtbRoutePlannerFilterControl)FilterControl).AddChildFilterControl((IDtbFilterControl)childControl);
		}

		protected ZFilterStripCommonControl DriverFilterControl;
		protected ZFilterStripCommonControl CarrierFilterControl;
		protected ZFilterStripCommonControl VehicleFilterControl;

		void InitializeHiddenButtons()
		{
			RemoveAddressPointButton.Visible = false; // not yet implemented
			IsRunSheetPaneVisible = false; // is true when designing so that we can see/design the panels
		}

		void InitializeDefaults()
		{
			// default, overridden when saved layout is loaded
			runSheetView = RunSheetView.RunSheets;
			runSheetDay = RunSheetDay.Today;
		}

		/// <summary>
		/// Show the move icon on the SOURCE grid we are dragging from. This looks better than the "no smoking" symbol :).
		/// </summary>
		void EnableDragFromOnGrid(ZGrid grid)
		{
			grid.AllowDrop = true;
			grid.DragOver += (o, e) => { e.Effect = DragDropEffects.Move; }; // show move icon when dragging
		}

		DtbRoutePlannerCollection Planners
		{
			get { return (DtbRoutePlannerCollection)DataSource; }
		}

		DtbRoutePlanner Planner
		{
			get { return Planners != null ? Planners.Cast<DtbRoutePlanner>().FirstOrDefault() : null; }
		}

#if DEBUG
		protected
#endif
 ZFilterStripCommonControl FilterControl;

		#endregion

		#region Hook / Unhook Events

		// hook

		void HookEvents()
		{
			// grid drag events
			EnableDragDropWithTargetRowHighlighting(RunSheetsGrid);
			EnableDragDropWithTargetRowHighlighting(CarrierFilterControl.Grid);
			EnableDragDropWithTargetRowHighlighting(DriverFilterControl.Grid);
			EnableDragDropWithTargetRowHighlighting(VehicleFilterControl.Grid);
			EnableDragDrop(RunSheetInstructionsControl.InstructionsGrid);
			EnableDragFromOnGrid(FilterControl.Grid);
			EnableDragFromOnGrid(DetailsUserControl.ConfirmationsGrid);
			EnableDragFromOnGrid(DirectDetailsUserControl.ConfirmationsGrid);

			// grid colours
			EnableWhiteRowsOnGrid(RunSheetsGrid);
			EnableWhiteRowsOnGrid(CarrierFilterControl.Grid);
			EnableWhiteRowsOnGrid(DriverFilterControl.Grid);
			EnableWhiteRowsOnGrid(VehicleFilterControl.Grid);

			// grid selection change
			CarrierFilterControl.Grid.AfterBind += delegate
			{
				CarrierFilterControl.Grid.ListManager.CurrentChanged += CarriersGrid_ListManager_CurrentChanged;
			};
			DriverFilterControl.Grid.AfterBind += delegate
			{
				DriverFilterControl.Grid.ListManager.CurrentChanged += DriversGrid_ListManager_CurrentChanged;
			};
			VehicleFilterControl.Grid.AfterBind += delegate
			{
				VehicleFilterControl.Grid.ListManager.CurrentChanged += VehiclesGrid_ListManager_CurrentChanged;
			};

			// other
			DomesticTransportGridHelper.HookDoubleClickToOpenRunSheet(RunSheetsGrid);
			RunSheetsDropButton.DropDownOpening += RunSheetsDropButton_DropDownOpening;

			RunSheetsGrid.AfterBind += delegate
			{
				RunSheetsGrid_ListChanged(this, null);
				RunSheetsGrid.ListManager.ListChanged += RunSheetsGrid_ListChanged;
			};
		}

		void EnableDragDropWithTargetRowHighlighting(ZGrid grid)
		{
			EnableDragDrop(grid);
			grid.DragOver += Grid_DragOver;
		}

		void EnableDragDrop(ZGrid grid)
		{
			grid.AllowDrop = true;
			grid.DragEnter += Grid_DragEnter;
			grid.DragDrop += Grid_DragDrop;
		}

		void RunSheetsGrid_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (RunSheetsGrid.ListManager.Count > 0)
			{
				RunSheetsGrid.UnSelectAll();
				RunSheetsGrid.Select(0);
			}
		}

		// unhook

		void UnHookEvents()
		{
			UnHookRunSheetMenuItemEvents();

			// grid drag events
			DisableDragDrop(RunSheetsGrid);
			DisableDragDrop(CarrierFilterControl.Grid);
			DisableDragDrop(DriverFilterControl.Grid);
			DisableDragDrop(VehicleFilterControl.Grid);
			DisableDragDrop(RunSheetInstructionsControl.InstructionsGrid);

			// grid colors
			DisableWhiteRowsOnGrid(RunSheetsGrid);
			DisableWhiteRowsOnGrid(CarrierFilterControl.Grid);
			DisableWhiteRowsOnGrid(DriverFilterControl.Grid);
			DisableWhiteRowsOnGrid(VehicleFilterControl.Grid);

			// grid selection change
			if (CarrierFilterControl.Grid != null && CarrierFilterControl.Grid.ListManager != null)
			{
				CarrierFilterControl.Grid.ListManager.CurrentChanged -= CarriersGrid_ListManager_CurrentChanged;
			}
			if (DriverFilterControl.Grid != null && DriverFilterControl.Grid.ListManager != null)
			{
				DriverFilterControl.Grid.ListManager.CurrentChanged -= DriversGrid_ListManager_CurrentChanged;
			}
			if (VehicleFilterControl.Grid != null && VehicleFilterControl.Grid.ListManager != null)
			{
				VehicleFilterControl.Grid.ListManager.CurrentChanged -= VehiclesGrid_ListManager_CurrentChanged;
			}

			// filter control
			if (FilterControl != null)
			{
				var iFilterControl = (IDtbRoutePlannerFilterControl)FilterControl;
				iFilterControl.PerformingSearch -= DtbRoutePlannerForm_PerformingSearch;
				iFilterControl.PerformedSearch -= DtbRoutePlannerForm_PerformedSearch;

				DisableWhiteRowsOnGrid(FilterControl.Grid);
			}

			// other
			if (RunSheetsDropButton != null)
			{
				RunSheetsDropButton.DropDownOpening -= RunSheetsDropButton_DropDownOpening;
			}

			if (RunSheetsGrid != null && RunSheetsGrid.ListManager != null)
			{
				RunSheetsGrid.ListManager.ListChanged -= RunSheetsGrid_ListChanged;
			}

			if (calendar != null)
			{
				calendar.DateTimeSelected -= Calendar_DateTimeSelected;
				calendar.FormClosing -= Calendar_FormClosing;
			}
		}

		void UnHookRunSheetMenuItemEvents()
		{
			if (RunSheetsDropButton != null)
			{
				RunSheetsDropButton.DropDown.Closing -= RunSheetsDropButton_DropDown_Closing;
			}

			foreach (var item in RunSheetViewMenuItems)
			{
				item.Click -= RunSheetViewMenuItem_Click;
			}

			foreach (var item in RunSheetDayMenuItems)
			{
				if (item.Day.IsCustomDate())
				{
					item.Click -= CustomDateMenuItem_Click;
				}
				else
				{
					item.Click -= RunSheetDayMenuItem_Click;
				}
			}
		}

		void DisableDragDrop(ZGrid grid)
		{
			grid.AllowDrop = false;
			grid.DragEnter -= Grid_DragEnter;
			grid.DragDrop -= Grid_DragDrop;
			grid.DragOver -= Grid_DragOver;
		}

		#endregion

		#region Drag Drop

		void Grid_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Copy;
		}

		void Grid_DragOver(object sender, DragEventArgs e)
		{
			var grid = (ZGrid)sender;
			var location = grid.PointToClient(ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Y)));
			var hit = grid.HitTest(location);

			if (hit.Row >= 0)
			{
				var bizO = (BusinessObject)grid.List[hit.Row];
				grid.SelectSingleElement(bizO);
			}
		}

		void Grid_DragDrop(object sender, DragEventArgs e)
		{
			if (sender == RunSheetInstructionsControl.InstructionsGrid)
			{
				// runsheet is already current but may *not* be selected, so select it to allow the drop
				RunSheetsGrid.SelectSingleElement(RunSheetsGrid.GetCurrent());
			}

			if (e.Data.GetDataPresent(typeof(ArrayList)))
			{
				var elements = (ArrayList)e.Data.GetData(typeof(ArrayList));
				if (elements.Count > 0)
				{
					// note we don't call button.performclick because it visually clicks the button

					if (elements[0] is DtbAddressPoint)
					{
						AssignSelectedAddressPointsToRunSheet();
					}
					else if (elements[0] is DtbConsignmentConfirmation)
					{
						AssignSelectedConfirmationsToRunSheet();
					}
					else
					{
						// should not happen and if it does we don't really care.
					}
				}
			}
		}

		#endregion

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (Planners != null)
			{
				Planners.CountChanged -= Planners_CountChanged;
			}

			base.SetDataBinding(dataSource, dataMember);

			if (Planners != null)
			{
				Planners.CountChanged += Planners_CountChanged;
			}

			OnPlannerChanged();
		}

		#region Planners_CountChanged

		void Planners_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			OnPlannerChanged();
		}

		#endregion

		#region OnPlannerChanged

		void OnPlannerChanged()
		{
			var newFactory = Planners != null ? Planners.Factory : null;
			if (currentFactory != newFactory)
			{
				currentFactory = newFactory;
				OnFactoryChanged();
			}
		}

		BusinessObjectFactory currentFactory;

		#endregion

		#endregion

		#region OnVisible

		protected override void SetVisibleCore(bool value)
		{
			if (value)
			{
				SuspendLayout();
				try
				{
					BackColor = SystemColors.Control; // get rid of that ugly blue.
					MoveBar.BackColor = SystemColors.Control; // get rid of the 'silver' sheen look.
					RefreshButton.Text = " " + RefreshButton.Text;

					RestoreScreenLayout();
				}
				finally
				{
					ResumeLayout();
				}
			}

			CarrierFilterControl.Grid.VisibleChanged += GetEventHandlerForAdditionColumns(CarrierFilterControl, CarrierAdditionalColumns);
			DriverFilterControl.Grid.VisibleChanged += GetEventHandlerForAdditionColumns(DriverFilterControl, DriverAdditionalColumns);
			VehicleFilterControl.Grid.VisibleChanged += GetEventHandlerForAdditionColumns(VehicleFilterControl, VehicleAdditionalColumns);

			base.SetVisibleCore(value); // call this LAST because it makes the form open instantly rather than drawing in front of the user

			if (value)
			{
				RestoreScreenLayout_SplittersAndMainFilterOnly();
			}
		}

		#region GetEventHandlerForAdditionColumns

		EventHandler GetEventHandlerForAdditionColumns<TEntity>(ZFilterStripCommonControl filterControl, CustomPropertyContainer<TEntity> customPropertyContainers)
			where TEntity : BusinessObject
		{
			EventHandler hookAdditionalColumns = null;

			hookAdditionalColumns =
				(sender, e) =>
				{
					var grid = sender as ZGrid;
					if (grid != null && grid.Visible)
					{
						var columnInitializer = new RoutePlannerGUICustomColumnsInitializer(filterControl.Grid, filterControl.GridCollection, null, customPropertyContainers);
						columnInitializer.AddCustomColumns();

						grid.VisibleChanged -= hookAdditionalColumns;
					}
				};

			return hookAdditionalColumns;
		}

		#endregion

		#region CarrierAdditionalColumns

		CustomPropertyContainer<OrgHeader> CarrierAdditionalColumns
		{
			get { return carrierAdditionalColumns ?? (carrierAdditionalColumns = GetAdditionalColumns<OrgHeader>((carrier) => Planner.GetRunSheetCountForEntity(carrier))); }
		}

		CustomPropertyContainer<OrgHeader> carrierAdditionalColumns;

		#endregion

		#region DriverAdditionalColumns

		CustomPropertyContainer<GlbStaff> DriverAdditionalColumns
		{
			get { return driverAdditionalColumns ?? (driverAdditionalColumns = GetAdditionalColumns<GlbStaff>((driver) => Planner.GetRunSheetCountForEntity(driver))); }
		}

		CustomPropertyContainer<GlbStaff> driverAdditionalColumns;

		#endregion

		#region VehicleAdditionalColumns

		CustomPropertyContainer<RefEquipment> VehicleAdditionalColumns
		{
			get { return vehicleAdditionalColumns ?? (vehicleAdditionalColumns = GetAdditionalColumns<RefEquipment>((vehicle) => Planner.GetRunSheetCountForEntity(vehicle))); }
		}

		CustomPropertyContainer<RefEquipment> vehicleAdditionalColumns;

		#endregion

		#region GetAdditionalColumns

		CustomPropertyContainer<T> GetAdditionalColumns<T>(ValueGetter<T> getCount)
			where T : BusinessObject
		{
			var collection = new CustomPropertyContainer<T>();
			collection.AddCustomProperty(Res.GetString("eb5bf6a9-ed80-4a92-99a9-b4eb212bd62e", "Run Sheets"), typeof(ZInt), getCount);
			return collection;
		}

		#endregion

		#endregion

		#region OnClosing

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (!e.Cancel)
			{
				SaveScreenLayoutData();
			}
		}

		#endregion

		#region FormHeading

		public override string FormHeading
		{
			get
			{
				switch (PlannerViewMode)
				{
					case DtbRoutePlannerViewMode.NextAvailable:
						return Res.GetString("67057283-279a-43d3-9bd7-a6127faef5b3", "Allocating Next Available");
					case DtbRoutePlannerViewMode.Pickups:
						return Res.GetString("e17802bf-61ea-48aa-910e-9701db6fe41e", "Allocating Pickups Only");
					case DtbRoutePlannerViewMode.Deliveries:
						return Res.GetString("4b815101-7bf1-429a-a45f-42fe829ff783", "Allocating Deliveries Only");
					case DtbRoutePlannerViewMode.Direct:
						return Res.GetString("92c3adde-1d46-4728-8054-e6d72ba9c895", "Allocating Direct Routes");

					default:
						return "";
				}
			}
		}

		#endregion

		//

		#region Actions

		#region Next Available / Pickups Only / Deliveries Only / Direct - Buttons

		void NextAvailableButton_CheckedChanged(object sender, EventArgs e)
		{
			PlannerViewMode = NextAvailableButton.ViewMode;
		}

		void PickupsOnlyButton_CheckedChanged(object sender, EventArgs e)
		{
			PlannerViewMode = PickupsOnlyButton.ViewMode;
		}

		void DeliveriesOnlyButton_CheckedChanged(object sender, EventArgs e)
		{
			PlannerViewMode = DeliveriesOnlyButton.ViewMode;
		}

		void DirectButton_CheckedChanged(object sender, EventArgs e)
		{
			PlannerViewMode = DirectButton.ViewMode;
		}

		#endregion

		#region Change View Mode Button

		void ChangeViewModeButton_Click(object sender, EventArgs e)
		{
			TogglePlannerViewModePaneVisibility(showViewModePanel: true);
		}

		#endregion

		#region Consignment Details Button

		void ConsignmentDetailsButton_Click(object sender, EventArgs e)
		{
			UpdateConsignmentDetailsPaneVisibility();
		}

		void UpdateConsignmentDetailsPaneVisibility()
		{
			AddressPointsAndConfirmationsSplitContainer.Panel2Collapsed = !ConsignmentDetailsButton.Checked;
			AssignPartialButton.Visible = ConsignmentDetailsButton.Checked;
		}

		#endregion

		#region RunSheet Details Button

		void RunSheetDetailsButton_Click(object sender, EventArgs e)
		{
			UpdateRunSheetDetailsPanelVisibility();
		}

		void UpdateRunSheetDetailsPanelVisibility()
		{
			RunSheetsHorizontalSplitContainer.Panel2Collapsed = !RunSheetDetailsButton.Checked;
			//RemoveAddressPointButton.Visible = RunSheetDetailsButton.Checked;
		}

		#endregion

		#region Filters Button

		void FiltersButton_Click(object sender, EventArgs e)
		{
			UpdateFiltersPaneVisibility();
		}

		void UpdateFiltersPaneVisibility()
		{
			FilterControl.IsFilterVisible = FiltersButton.Checked;
			LinePanel2.Visible = FiltersButton.Checked;
		}

		#endregion

		#region Refresh Button

		void RefreshButton_Click(object sender, EventArgs e)
		{
			FilterControl.FirePerformSearch();
		}

		#endregion

		#region ◄ (RunSheet-Arrow) Button

		void RunSheetsArrowButton_Click(object sender, EventArgs e)
		{
			IsRunSheetPaneVisible = !IsRunSheetPaneVisible;
		}

		protected bool IsRunSheetPaneVisible
		{
			get { return !MainSplitContainer.Panel2Collapsed; }
			private set
			{
				MainSplitContainer.Panel2Collapsed = !value;
				RunSheetsArrowButton.Text = IsRunSheetPaneVisible ? "4" : "3"; // 3 and 4 in webdings font are directional arrow chars
				RunSheetDetailsButton.Enabled = IsRunSheetPaneVisible;
			}
		}

		#endregion

		#region RunSheet DropList Opening

		void RunSheetsDropButton_DropDownOpening(object sender, EventArgs e)
		{
			if (HasDayChanged)
			{
				RebuildRunSheetDropList();
			}
		}

		void RebuildRunSheetDropList()
		{
			Today = ZDate.Today;

			UnHookRunSheetMenuItemEvents();
			RunSheetsDropButton.DropDownItems.Clear();

			// Custom date drop down may require delayed closing of the menu
			RunSheetsDropButton.DropDown.Closing += RunSheetsDropButton_DropDown_Closing;

			// add views
			foreach (RunSheetView view in Enum.GetValues(typeof(RunSheetView)))
			{
				RunSheetsDropButton.DropDownItems.Add(new RunSheetMenuItemForView(view, RunSheetViewMenuItem_Click));
			}
			RunSheetsDropButton.DropDownItems.Add(new ToolStripSeparator());

			// add yesterday, today, tomorrow
			foreach (var day in RunSheetDayExtensions.GetYesterdayTodayTomorrow())
			{
				RunSheetsDropButton.DropDownItems.Add(new RunSheetMenuItemForDay(day, RunSheetDayMenuItem_Click));
			}
			RunSheetsDropButton.DropDownItems.Add(new DashedToolStripSeparator());

			// add days of the week
			foreach (RunSheetDay day in RunSheetDayExtensions.GetNextSevenDaysAfterTomorrow())
			{
				RunSheetsDropButton.DropDownItems.Add(new RunSheetMenuItemForDay(day, RunSheetDayMenuItem_Click));
			}

			RunSheetsDropButton.DropDownItems.Add(new DashedToolStripSeparator());

			RunSheetsDropButton.DropDownItems.Add(new RunSheetMenuItemForDay(RunSheetDay.CustomDate, CustomDateMenuItem_Click));
		}

		bool HasDayChanged
		{
			get { return Today != ZDate.Today; }
		}

		ZDate Today;

		#region RunSheetsDropButton_DropDown_Closing

		bool IsCalendarOpen
		{
			get { return calendar != null && calendar.Visible; } // Dont instantiate to check
		}

		void RunSheetsDropButton_DropDown_Closing(object sender, ToolStripDropDownClosingEventArgs e)
		{
			// The drop down closing event is fired as soon as you click a Menu Item on the RunSheet DropDown.
			// Since we don't want to close the DropDown when 'CustomDate' is clicked, we need to cancel the Close.
			// Unfortunately this means that the other Menu Items won't close the DropDown as well, which we 
			// handle later on.
			if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked
				|| (e.CloseReason == ToolStripDropDownCloseReason.AppFocusChange && IsCalendarOpen))
			{
				e.Cancel = true;
			}
			else if (IsCalendarOpen)
			{
				// User selected "Custom Date", but then selected another Menu Item.
				Calendar.Close();
			}
		}

		#endregion

		#region CustomDateMenuItem_Click

		void CustomDateMenuItem_Click(object sender, EventArgs e)
		{
			if (!IsCalendarOpen)
			{
				var menuItem = (RunSheetMenuItemForDay)sender;
				var menuItemLocation = ControlDpiScalingHelper.NewScaledPoint(menuItem.Bounds.Left, menuItem.Bounds.Bottom, false);
				var popupLocation = RunSheetsDropButton.DropDown.PointToScreen(menuItemLocation);

				var dateToSelect = CustomDateOption.IsValid ? CustomDateOption.ToDateTime() : DateTime.MinValue;
				Calendar.Popup(popupLocation, dateToSelect, this, ZDateTimePickerFormat.Short);
			}
			else
			{
				// User hit "Custom Date" again after opening calendar
				Calendar.Close();
			}
		}

		#endregion

		#region Calendar

		protected ZPopupCalendar Calendar
		{
			get { return calendar ?? (calendar = GetNewCalendar()); }
		}

		ZPopupCalendar calendar;

		ZPopupCalendar GetNewCalendar()
		{
			var newCalendar = new ZPopupCalendar();
			newCalendar.DateTimeSelected += Calendar_DateTimeSelected;
			newCalendar.FormClosing += Calendar_FormClosing;
			return newCalendar;
		}

		void Calendar_DateTimeSelected(object sender, ZPopupCalendar.DateTimeSelectedEventArgs e)
		{
			CustomDateOption = (ZDate)e.Value;
			RunSheetsDropButton.DropDown.Close();
		}

		void Calendar_FormClosing(object sender, FormClosingEventArgs e)
		{
			RunSheetsDropButton.DropDown.Focus(); // Return focus to the drop down item so LostFocus behaviour (close menu) is retained
		}

		#endregion

		#region class DashedToolStripSeparator, ToolStripRendererWithDashedSeparator,

		class DashedToolStripSeparator : ToolStripSeparator
		{
		}

#if !WINZOR
		class ToolStripRendererWithDashedSeparator : ToolStripProfessionalRenderer
		{
			protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
			{
				if (e.Item is DashedToolStripSeparator)
				{
					DrawDashedSeparator(e.Graphics,
						e.Item,
						ControlDpiScalingHelper.NewScaledRectangle(Point.Empty.X,
							Point.Empty.Y,
							ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.Item.Size.Width),
							ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Item.Size.Height)));
				}
				else
				{
					base.OnRenderSeparator(e);
				}
			}

			void DrawDashedSeparator(Graphics g, ToolStripItem item, Rectangle bounds)
			{
				var currentParent = (ToolStripDropDownMenu)item.GetCurrentParent();

				// calculate bounds
				if (currentParent.RightToLeft == RightToLeft.No)
				{
					ControlDpiScalingHelper.SetX(ref bounds, bounds.X + currentParent.Padding.Left - ControlDpiScalingHelper.ScaleToCurrentDpiX(2), false);
					ControlDpiScalingHelper.SetWidth(ref bounds, currentParent.Width - bounds.X, false);
				}
				else
				{
					ControlDpiScalingHelper.SetX(ref bounds, bounds.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(2), false);
					ControlDpiScalingHelper.SetWidth(ref bounds, (currentParent.Width - bounds.X) - currentParent.Padding.Right, false);
				}
				// draw dashed line
				using (var pen = new Pen(ColorTable.SeparatorDark))
				{
					pen.DashStyle = DashStyle.Custom;
					pen.DashPattern = new[] { 2f, 3f };

					int y = bounds.Height / 2;
					g.DrawLine(pen, bounds.Left + 1, y, bounds.Right - 2, y);
				}
			}
		}
#endif

		#endregion

		#endregion

		#region RunSheet View MenuItem Click

		void RunSheetViewMenuItem_Click(object sender, EventArgs e)
		{
			UpdateRunSheetView((RunSheetMenuItemForView)sender);

			// since the Closing of the DropDown is cancelled on the base Click Event we specifically close below
			RunSheetsDropButton.DropDown.Close();
		}

		#endregion

		#region RunSheet Day MenuItem Click

		void RunSheetDayMenuItem_Click(object sender, EventArgs e)
		{
			UpdateRunSheetDay((RunSheetMenuItemForDay)sender);

			// since the Closing of the DropDown is cancelled on the base Click Event we specifically close below
			RunSheetsDropButton.DropDown.Close();
		}

		#endregion

		#region Assign Button

		void AssignAddressPointButton_Click(object sender, EventArgs e)
		{
			AssignSelectedAddressPointsToRunSheet();
		}

		void AssignSelectedAddressPointsToRunSheet()
		{
			var selectedAddressPoints = FilterControl.Grid.SelectedElements;
			if (selectedAddressPoints.Length > 0)
			{
				FindOrCreateRunSheetAndAssignAddressPoints(selectedAddressPoints);
				((IDtbRoutePlannerFilterControl)FilterControl).UpdateResultCountMessage(true);
			}
			else
			{
				var message = Res.GetString("5a6c6c62-4afd-4e42-b9fc-55e98ac0e7eb", "Select one or more Addresses to Allocate.");
				var caption = Res.GetString("403718ed-4b1d-4371-a45f-09c2674313dd", "Assign");
				Globals.Message.ShowInformation(message, caption);
			}
		}

		void FindOrCreateRunSheetAndAssignAddressPoints(BusinessObject[] selectedAddressPoints)
		{
			var isValid = ValidateRunSheetSelection(AssignCaption, Res.GetString("805fa04c-394e-4ed6-a8fa-450167680b98", "Address Point"));
			if (isValid)
			{
				AssignAddressPointsAndRemoveFromGrid(selectedAddressPoints);
			}
		}

		string AssignCaption
		{
			get { return Res.GetString("403718ed-4b1d-4371-a45f-09c2674313dd", "Assign"); }
		}

		#region ValidateRunSheetSelection

		bool ValidateRunSheetSelection(string caption, string entity)
		{
			var result = false;
			if (RunSheetView == RunSheetView.Carriers && CarrierFilterControl.Grid.SelectedElements.Length != 1)
			{
				var message = Res.GetString("172cfce8-df6f-4d05-9f37-dcd0677451cb", "Select a Single Carrier to Assign {0} to Carrier.", entity);
				Globals.Message.ShowInformation(message, caption);
			}
			else if (RunSheetView == RunSheetView.Drivers && DriverFilterControl.Grid.SelectedElements.Length != 1)
			{
				var message = Res.GetString("4b17beb6-f557-4022-93db-6110dd5801fd", "Select a Single Driver to Assign {0} to Driver.", entity);
				Globals.Message.ShowInformation(message, caption);
			}
			else if (RunSheetView == RunSheetView.Vehicles && VehicleFilterControl.Grid.SelectedElements.Length != 1)
			{
				var message = Res.GetString("60dd2a62-0e81-4f1d-8de5-3cd0bd221799", "Select a Single Vehicle to Assign {0} to Vehicle.", entity);
				Globals.Message.ShowInformation(message, caption);
			}
			else if (RunSheetView == RunSheetView.RunSheets && RunSheetsGrid.SelectedElements.Length != 1)
			{
				var message = Res.GetString("9cd83d08-dc4c-43cf-ba6f-42692d1e0899", "Select a Single Run Sheet to Assign {0} to Run Sheet.", entity);
				Globals.Message.ShowInformation(message, caption);
			}
			else
			{
				result = true;
			}

			return result;
		}

		#endregion

		#region AssignAddressPointsAndRemoveFromGrid

		void AssignAddressPointsAndRemoveFromGrid(BusinessObject[] selectedAddressPoints)
		{
			var addressPoints = Array.ConvertAll(selectedAddressPoints, b => (DtbAddressPoint)b);
			var saved = TryAssignAddressPointsToRunSheetsAndSave(addressPoints);
			if (saved)
			{
				foreach (var addressPoint in addressPoints)
				{
					FilterControl.GridCollection.RemoveFromRelationship(addressPoint);
				}
			}
			// else error saving and screen refreshed
		}

		bool TryAssignAddressPointsToRunSheetsAndSave(DtbAddressPoint[] addressPoints)
		{
			switch (RunSheetView)
			{
				case RunSheetView.Vehicles:
					AssignAddressPointsToRunSheets<RefEquipment>(VehicleFilterControl.Grid, addressPoints, Planner.AssignConfirmationsToRunSheet);
					break;
				case RunSheetView.Drivers:
					AssignAddressPointsToRunSheets<GlbStaff>(DriverFilterControl.Grid, addressPoints, Planner.AssignConfirmationsToRunSheet);
					break;
				case RunSheetView.Carriers:
					AssignAddressPointsToRunSheets<OrgHeader>(CarrierFilterControl.Grid, addressPoints, Planner.AssignConfirmationsToRunSheet);
					break;
				case RunSheetView.RunSheets:
				default:
					var runSheet = (DtbConsignmentRunSheet)RunSheetsGrid.SelectedElements[0];
					Planner.AssignConfirmationsToRunSheet(runSheet, addressPoints);
					break;
			}

			// we readon pass in confirmations is because a later WI is going to base
			// the save failure as well as perhaps do auto-merging.
			return TrySaveWithRecovery(addressPoints.SelectMany(a => a.Confirmations));
		}

		void AssignAddressPointsToRunSheets<T>(ZGrid grid, DtbAddressPoint[] addressPoints, Action<T, DtbConsignmentRunSheet, DtbAddressPoint[]> assignAddressPoints)
			where T : BusinessObject
		{
			var entity = (T)grid.SelectedElements[0];
			var runSheet = (DtbConsignmentRunSheet)RunSheetsGrid.SelectedElements.FirstOrDefault();
			assignAddressPoints(entity, runSheet, addressPoints);
		}

		#endregion

		#endregion

		#region Partial Assign Button

		DtbRoutePlannerDetailsUserControlBase ActiveDetailsUserControl
		{
			get { return PlannerViewMode != DtbRoutePlannerViewMode.Direct ? DetailsUserControl : DirectDetailsUserControl; }
		}

		void AssignPartialButton_Click(object sender, EventArgs e)
		{
			AssignSelectedConfirmationsToRunSheet();
		}

		void AssignSelectedConfirmationsToRunSheet()
		{
			var selectedConfirmations = ActiveDetailsUserControl.SelectedConfirmations;
			if (selectedConfirmations.Length > 0)
			{
				FindOrCreateRunSheetAndAssignConfirmations(selectedConfirmations);
			}
			else
			{
				var message = Res.GetString("0f5a144e-ab7c-47d0-8434-4a03c7232ddf", "Select a Consignment to Assign it.");
				Globals.Message.ShowInformation(message, PartialAssignCaption);
			}
		}

		string PartialAssignCaption
		{
			get { return Res.GetString("cc01304a-9376-4471-8ad4-1071c01785c2", "Partial Assign"); }
		}

		void FindOrCreateRunSheetAndAssignConfirmations(DtbConsignmentConfirmation[] selectedConfirmations)
		{
			var isValid = ValidateRunSheetSelection(PartialAssignCaption, Res.GetString("2a5fb9c7-6346-44b3-a512-34090e8f1b5c", "Consignment"));
			if (isValid)
			{
				var saved = TryAssignConfirmationsToRunSheetsAndSave(selectedConfirmations);
				if (saved)
				{
					var addressPoint = (DtbAddressPoint)ActiveDetailsUserControl.CurrentDataItem;
					foreach (var confirmation in selectedConfirmations)
					{
						addressPoint.Confirmations.RemoveFromRelationship(confirmation);
					}

					if (addressPoint.Confirmations.Count == 0)
					{
						FilterControl.GridCollection.RemoveFromRelationship(addressPoint);
					}
				}
				// else error saving and screen refreshed
			}
		}

		bool TryAssignConfirmationsToRunSheetsAndSave(DtbConsignmentConfirmation[] selectedConfirmations)
		{
			switch (RunSheetView)
			{
				case RunSheetView.Vehicles:
					AssignConfirmationsToRunSheets<RefEquipment>(VehicleFilterControl.Grid, selectedConfirmations, Planner.AssignConfirmationsToRunSheet);
					break;
				case RunSheetView.Drivers:
					AssignConfirmationsToRunSheets<GlbStaff>(DriverFilterControl.Grid, selectedConfirmations, Planner.AssignConfirmationsToRunSheet);
					break;
				case RunSheetView.Carriers:
					AssignConfirmationsToRunSheets<OrgHeader>(CarrierFilterControl.Grid, selectedConfirmations, Planner.AssignConfirmationsToRunSheet);
					break;
				case RunSheetView.RunSheets:
				default:
					Planner.AssignConfirmationsToRunSheetInstruction((DtbConsignmentRunSheet)RunSheetsGrid.SelectedElements.FirstOrDefault(), selectedConfirmations);
					break;
			}

			return TrySaveWithRecovery(selectedConfirmations);
		}

		void AssignConfirmationsToRunSheets<T>(ZGrid grid, DtbConsignmentConfirmation[] confirmations, Action<T, DtbConsignmentRunSheet, DtbConsignmentConfirmation[]> assignConfirmations)
			where T : BusinessObject
		{
			var entity = (T)grid.SelectedElements[0];
			var runSheet = (DtbConsignmentRunSheet)RunSheetsGrid.SelectedElements.FirstOrDefault();
			assignConfirmations(entity, runSheet, confirmations);
		}

		#endregion

		#endregion

		#region PlannerViewMode

		DtbRoutePlannerViewMode PlannerViewMode
		{
			get { return plannerViewMode; }
			set { UpdatePlannerViewMode(value); }
		}

		void UpdatePlannerViewMode(DtbRoutePlannerViewMode viewMode)
		{
			if (!IsChangingPlannerViewMode && viewMode != DtbRoutePlannerViewMode.None) // cannot set back to none (makes no sense)
			{
				SuspendLayout();
				try
				{
					IsChangingPlannerViewMode = true;

					var allButtons = ViewModeToolStrip.Items.Cast<PlannerButton>();
					var currentButton = allButtons.First(b => b.ViewMode == viewMode);
					var iFilterControl = (IDtbRoutePlannerFilterControl)FilterControl;

					if (viewMode != PlannerViewMode)
					{
						bool isSettingViewModeOnFormOpen = (plannerViewMode == DtbRoutePlannerViewMode.None);
						plannerViewMode = viewMode;
						UpdatePlannerViewModeButtonsAndFormCaption(allButtons, currentButton);

						if (isSettingViewModeOnFormOpen)
						{
							iFilterControl.SetViewModeWithoutRunningSearch(viewMode); // don't run search when opening the form
						}
						else
						{
							iFilterControl.ViewMode = viewMode; // performs the search
						}

						bool isViewModeDirect = plannerViewMode == DtbRoutePlannerViewMode.Direct;
						DirectDetailsUserControl.Visible = isViewModeDirect;
						DetailsUserControl.Visible = !isViewModeDirect;
					}
					// open the planner then click the *current* viewmode -- FilterControl thinks there's no change -- so run the search directly
					else if (!iFilterControl.HasSearchBeenRun)
					{
						UpdatePlannerViewModeButtonsAndFormCaption(allButtons, currentButton);
						FilterControl.FirePerformSearch();
					}
					else // user clicked the *current* ViewMode, re-check it
					{
						currentButton.Checked = true;
					}

					TogglePlannerViewModePaneVisibility(showViewModePanel: false);
				}
				finally
				{
					IsChangingPlannerViewMode = false;
					ResumeLayout();
				}
			}
		}

		void UpdatePlannerViewModeButtonsAndFormCaption(IEnumerable<PlannerButton> allButtons, PlannerButton currentButton)
		{
			currentButton.Checked = true;
			currentButton.ForeColor = SystemColors.HotTrack;

			foreach (var button in allButtons.Where(b => b != currentButton))
			{
				button.Checked = false;
				button.ForeColor = SystemColors.ControlText;
			}

			ChangeViewModeButton.Text = FormHeading;
			RefreshCaption();
		}

		/// <summary>
		/// Hides the ViewModeStrip and re-enables the planner.
		/// </summary>
		void TogglePlannerViewModePaneVisibility(bool showViewModePanel)
		{
			ViewModePanel.Visible = showViewModePanel;
			ControlBoxToolStrip.Enabled = !showViewModePanel;
			MainSplitContainer.Enabled = !showViewModePanel;
		}

		bool IsChangingPlannerViewMode;
		DtbRoutePlannerViewMode plannerViewMode;

		#endregion

		#region RunSheetView

		RunSheetView RunSheetView
		{
			get { return runSheetView; }
			set
			{
				var selectedViewMenuItem = GetRunSheetMenuItem(value);
				UpdateRunSheetView(selectedViewMenuItem);
			}
		}

		void UpdateRunSheetView(RunSheetMenuItemForView selectedViewMenuItem)
		{
			var shouldFilterChildGrid = this.runSheetView != selectedViewMenuItem.View;
			this.runSheetView = selectedViewMenuItem.View;

			SuspendLayout();
			try
			{
				// auto-show the runsheet pane if hidden
				IsRunSheetPaneVisible = true;

				// update grid visibility, menu items' check states and button text
				UpdateRunSheetGridVisibility();
				UpdateRunSheetMenuItemsCheckStatus(selectedViewMenuItem, RunSheetViewMenuItems);
				UpdateRunSheetButtonTextAndImage();
				UpdateRunSheetFilter();

				if (shouldFilterChildGrid)
				{
					((IDtbRoutePlannerFilterControl)FilterControl).PerformSearchOnChildren();
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		void UpdateRunSheetGridVisibility()
		{
			SuspendLayout();
			try
			{
				RunSheetsVerticalSplitContainer.Panel1Collapsed = (RunSheetView == RunSheetView.RunSheets);
				CarrierFilterControl.Visible = (RunSheetView == RunSheetView.Carriers);
				DriverFilterControl.Visible = (RunSheetView == RunSheetView.Drivers);
				VehicleFilterControl.Visible = (RunSheetView == RunSheetView.Vehicles);
			}
			finally
			{
				ResumeLayout();
			}
		}

		void UpdateRunSheetFilter()
		{
			switch (RunSheetView)
			{
				case RunSheetView.RunSheets:
					Planner.ClearRunSheetsCarrierDriverAndVehicleFilter();
					break;
				case RunSheetView.Carriers:
					FilterRunSheetsByCarrier();
					break;
				case RunSheetView.Drivers:
					FilterRunSheetsByDriver();
					break;
				case RunSheetView.Vehicles:
					FilterRunSheetsByVehicle();
					break;
			}
		}

		void CarriersGrid_ListManager_CurrentChanged(object sender, EventArgs e)
		{
			FilterRunSheetsByCarrier();
		}

		void DriversGrid_ListManager_CurrentChanged(object sender, EventArgs e)
		{
			FilterRunSheetsByDriver();
		}

		void VehiclesGrid_ListManager_CurrentChanged(object sender, EventArgs e)
		{
			FilterRunSheetsByVehicle();
		}

		void FilterRunSheetsByCarrier()
		{
			// When we swap the Factory (i.e Refresh) the List Changed event on the Route Planner Collection fires
			// Child List Change events. This check makes sure we only filter by Carriers if the Carrier's View is active.
			if (RunSheetView == RunSheetView.Carriers)
			{
				Planner.FilterRunSheetsByCarrier((OrgHeader)CarrierFilterControl.Grid.GetCurrent());
			}
		}

		void FilterRunSheetsByDriver()
		{
			// When we swap the Factory (i.e Refresh) the List Changed event on the Route Planner Collection fires
			// Child List Change events. This check makes sure we only filter by Drivers if the Driver's View is active.
			if (RunSheetView == RunSheetView.Drivers)
			{
				Planner.FilterRunSheetsByDriver((GlbStaff)DriverFilterControl.Grid.GetCurrent());
			}
		}

		void FilterRunSheetsByVehicle()
		{
			// When we swap the Factory (i.e Refresh) the List Changed event on the Route Planner Collection fires
			// Child List Change events. This check makes sure we only filter by Vehicles if the Vehicle's View is active.
			if (RunSheetView == RunSheetView.Vehicles)
			{
				Planner.FilterRunSheetsByVehicle((RefEquipment)VehicleFilterControl.Grid.GetCurrent());
			}
		}

		RunSheetMenuItemForView GetRunSheetMenuItem(RunSheetView view)
		{
			return RunSheetViewMenuItems.First(i => i.View == view);
		}

		protected IEnumerable<RunSheetMenuItemForView> RunSheetViewMenuItems
		{
			get { return RunSheetsDropButton.DropDownItems.OfType<RunSheetMenuItemForView>(); }
		}

		RunSheetView runSheetView;

		#endregion

		#region RunSheetDay

		RunSheetDay RunSheetDay
		{
			get { return runSheetDay; }
			set
			{
				var selectedDayMenuItem = GetRunSheetMenuItem(value);
				UpdateRunSheetDay(selectedDayMenuItem);
			}
		}

		void UpdateRunSheetDay(RunSheetMenuItemForDay selectedDayMenuItem)
		{
			this.runSheetDay = selectedDayMenuItem.Day;

			SuspendLayout();
			try
			{
				// apply the current filter if necessary
				Planner.CurrentDay = selectedDayMenuItem.Day;

				// auto-show the runsheet pane if hidden
				IsRunSheetPaneVisible = true;

				// update the menu items' check states and button text
				UpdateRunSheetMenuItemsCheckStatus(selectedDayMenuItem, RunSheetDayMenuItems);
				UpdateRunSheetButtonTextAndImage();

				// RefreshBinding so RunSheetCount is updated for the new Day 
				switch (RunSheetView)
				{
					case RunSheetView.Carriers:
						Planner.Carriers.RefreshBinding();
						break;
					case RunSheetView.Drivers:
						Planner.Drivers.RefreshBinding();
						break;
					case RunSheetView.Vehicles:
						Planner.Vehicles.RefreshBinding();
						break;
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		RunSheetMenuItemForDay GetRunSheetMenuItem(RunSheetDay day)
		{
			return RunSheetDayMenuItems.First(i => i.Day == day);
		}

		protected IEnumerable<RunSheetMenuItemForDay> RunSheetDayMenuItems
		{
			get { return RunSheetsDropButton.DropDownItems.OfType<RunSheetMenuItemForDay>(); }
		}

		RunSheetDay runSheetDay;

		#endregion

		#region CustomDateOption

		ZDate CustomDateOption
		{
			get { return Planner.CustomDateOption; }
			set
			{
				Planner.CustomDateOption = value;
				RunSheetDay = RunSheetDay.CustomDate;
			}
		}

		#endregion

		#region Updating RunSheet DropList MenuItems

		void UpdateRunSheetMenuItemsCheckStatus(RunSheetMenuItem itemToCheck, IEnumerable<RunSheetMenuItem> itemsToUncheck)
		{
			foreach (var item in itemsToUncheck)
			{
				item.Checked = false;
			}

			itemToCheck.Checked = true;
		}

		void UpdateRunSheetButtonTextAndImage()
		{
			SuspendLayout();
			try
			{
				switch (RunSheetView)
				{
					case RunSheetView.Carriers:
						RunSheetsDropButton.Image = Properties.Resources.Drivers.ToBitmap();
						break;
					case RunSheetView.Drivers:
						RunSheetsDropButton.Image = Properties.Resources.DriversPortrait.ToBitmap();
						break;
					case RunSheetView.Vehicles:
						RunSheetsDropButton.Image = Properties.Resources.Drivers.ToBitmap();
						break;
					case RunSheetView.RunSheets:
						RunSheetsDropButton.Image = Properties.Resources.Details;
						break;
				}

				RunSheetsDropButton.Text = Invariant($"{RunSheetView.GetDescription().Caption} ({DateDescription})"); // View description and date description are already translated.
			}
			finally
			{
				ResumeLayout();
			}
		}

		string DateDescription
		{
			get { return RunSheetDay.IsCustomDate() ? CustomDateOption.ToString() : RunSheetDay.GetDescription().Caption; }
		}

		#endregion

		#region Grid Colours

		void EnableWhiteRowsOnGrid(ZGrid grid)
		{
			grid.ColourDeciding += Grid_ColourDeciding;
		}

		void DisableWhiteRowsOnGrid(ZGrid grid)
		{
			if (grid != null)
			{
				grid.ColourDeciding -= Grid_ColourDeciding;
			}
		}

		// this will eventually implement colours based on the status of the addresspoint (ie. perhaps green for 'pickup completed', etc).
		void Grid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			e.Colour = SystemColors.Window;
		}

		#endregion

		//#warning attempt to extract out the layout stuff
		#region Screen Layout

		// restore

		void RestoreScreenLayout()
		{
			SuspendLayout();
			try
			{
				// view modes
				PlannerViewMode = ScreenLayout.PlannerViewMode != DtbRoutePlannerViewMode.None ? ScreenLayout.PlannerViewMode : DEFAULT_PLANNER_VIEWMODE;
				RebuildRunSheetDropList();
				RunSheetView = ScreenLayout.RunSheetView;

				if (ScreenLayout.CustomDate.HasValue)
				{
					// Setting the date will set the RunSheetDay as well
					CustomDateOption = ScreenLayout.CustomDate.Value;
				}
				else
				{
					RunSheetDay = ScreenLayout.RunSheetDay;
				}

				// button states (won't fire if value has not changed, so call Update() methods after setting)
				ConsignmentDetailsButton.Checked = ScreenLayout.IsConsignmentDetailsButtonChecked;
				UpdateConsignmentDetailsPaneVisibility();

				IsRunSheetPaneVisible = ScreenLayout.IsRunSheetsArrowButtonChecked;

				RunSheetDetailsButton.Checked = ScreenLayout.IsRunSheetDetailsButtonChecked;
				UpdateRunSheetDetailsPanelVisibility();
			}
			finally
			{
				ResumeLayout();
			}
		}

		void RestoreScreenLayout_SplittersAndMainFilterOnly()
		{
			SuspendLayout();
			try
			{
				//if filters are hidden when form is closed then during restore they appear half hidden
				FiltersButton.Checked = ScreenLayout.IsFiltersButtonChecked;
				UpdateFiltersPaneVisibility();

				// splitter positions (done after ResumeLayout() to prevent .NET recalc'ing the splitter distances)
				AddressPointsAndConfirmationsSplitContainer.SplitterDistance = ScreenLayout.ConsignmentHorizontalSplitterPosition;
				MainSplitContainer.SplitterDistance = ScreenLayout.ConsignmentVerticalSplitterPosition;
				RunSheetsVerticalSplitContainer.SplitterDistance = ScreenLayout.RunSheetVerticalSplitterPosition;
				RunSheetsHorizontalSplitContainer.SplitterDistance = ScreenLayout.RunSheetHorizontalSplitterPosition;
			}
			finally
			{
				ResumeLayout();
			}
		}

		DtbRoutePlannerScreenLayout ScreenLayout
		{
			get
			{
				if (screenLayout == null)
				{
					var layout = LoadScreenLayoutData();
					if (layout != null)
					{
						using (var stream = new MemoryStream(layout.SD_BinaryValue))
						{
							var serializer = ZXmlSerializer.New(typeof(DtbRoutePlannerScreenLayout));
							screenLayout = (DtbRoutePlannerScreenLayout)serializer.Deserialize(stream);
						}
					}

					if (screenLayout == null)
					{
						screenLayout = CreateScreenLayout();
					}
				}
				return screenLayout;
			}
		}

		StmData LoadScreenLayoutData()
		{
			var query = new ZQuery();
			query.AddToFilter(StmDataSchema.SD_Name, SCREEN_LAYOUT_KEY);
			query.AddToFilter(StmDataSchema.SD_Owner, GlbStaff.CurrentUser.PK);
			query.AddToFilter(StmDataSchema.SD_Type, RegistryDataTypes.Codes.Binary);

			return LayoutFactory.LoadTop1<StmData>(query);
		}

		// save

		void SaveScreenLayoutData()
		{
			var layout = LoadOrCreateScreenLayoutData();
			using (var stream = new MemoryStream())
			{
				var serializer = ZXmlSerializer.New(typeof(DtbRoutePlannerScreenLayout));
				serializer.Serialize(stream, CreateScreenLayout());

				layout.SD_BinaryValue = stream.ToArray();
				layout.Factory.Save();
			}
		}

		StmData LoadOrCreateScreenLayoutData()
		{
			var result = LoadScreenLayoutData();
			if (result == null)
			{
				result = LayoutFactory.New<StmData>();
				result.SD_Name = SCREEN_LAYOUT_KEY;
				result.SD_Owner = GlbStaff.CurrentUser.PK;
				result.SD_Type = RegistryDataTypes.Codes.Binary;
			}

			return result;
		}

		BusinessObjectFactory LayoutFactory
		{
			get { return layoutFactory ?? (layoutFactory = new BusinessObjectFactory()); }
		}

		const string SCREEN_LAYOUT_KEY = "RoutePlannerScreenLayout";
		DtbRoutePlannerScreenLayout screenLayout;
		BusinessObjectFactory layoutFactory;

		#region DtbRoutePlannerScreenLayout

		DtbRoutePlannerScreenLayout CreateScreenLayout()
		{
			var layout = new DtbRoutePlannerScreenLayout();

			// The ViewMode is set in the filterBizO so that Z can run the search when opening the form.
			// This is based on the registry item [Physical Server > Display Grid > Run Search on Entering a Module].
			// Therefore the search has already run and we just need to click the matching ViewMode button on the form.
			layout.PlannerViewMode = ((IDtbRoutePlannerFilterControl)FilterControl).ViewMode;
			layout.RunSheetView = RunSheetView;
			layout.RunSheetDay = RunSheetDay;

			var customDate = CustomDateOption;
			layout.CustomDate = customDate.IsValid && RunSheetDay.IsCustomDate() ? customDate : null;

			// button states
			layout.IsConsignmentDetailsButtonChecked = ConsignmentDetailsButton.Checked;
			layout.IsRunSheetDetailsButtonChecked = RunSheetDetailsButton.Checked;
			layout.IsFiltersButtonChecked = FiltersButton.Checked;
			layout.IsRunSheetsArrowButtonChecked = IsRunSheetPaneVisible; // button does not have check state

			// splitter positions
			layout.ConsignmentHorizontalSplitterPosition = AddressPointsAndConfirmationsSplitContainer.SplitterDistance;
			layout.ConsignmentVerticalSplitterPosition = MainSplitContainer.SplitterDistance;
			layout.RunSheetVerticalSplitterPosition = RunSheetsVerticalSplitContainer.SplitterDistance;
			layout.RunSheetHorizontalSplitterPosition = RunSheetsHorizontalSplitContainer.SplitterDistance;

			return layout;
		}

		#endregion

		#endregion

		#region Perform Search Events

		void DtbRoutePlannerForm_PerformingSearch(object sender, EventArgs e)
		{
			SetLastSelectedEntities();
		}

		void DtbRoutePlannerForm_PerformedSearch(object sender, EventArgs e)
		{
			// filter Runsheets by Child Filters
			Planner.FilterRunSheetsByChildFilters(((IDtbRoutePlannerFilterBusinessObject)FilterControl.FilterBusinessObject).ChildFiltersForRunsheets);
			ReSelectLastSelectedEntities();
		}

		#endregion

		#region Recent Selections

		void SetLastSelectedEntities()
		{
			// save user selections
			Planner.SetLastSelectedEntities(RunSheetsGrid.GetCurrentPK(), CarrierFilterControl.Grid.GetCurrentPK(), DriverFilterControl.Grid.GetCurrentPK(), VehicleFilterControl.Grid.GetCurrentPK());
		}

		void ReSelectLastSelectedEntities()
		{
			// restore user selections
			CarrierFilterControl.Grid.SelectSingleElementByPK(Planner.LastSelectedEntities.CarrierPK);
			DriverFilterControl.Grid.SelectSingleElementByPK(Planner.LastSelectedEntities.DriverPK);
			VehicleFilterControl.Grid.SelectSingleElementByPK(Planner.LastSelectedEntities.VehiclePK);

			// must select runsheet last as the RunSheets collection is re-filtered on Driver/Vehicle selection (above)
			RunSheetsGrid.SelectSingleElementByPK(Planner.LastSelectedEntities.RunSheetPK);
		}

		#endregion

		#region Save

		bool TrySaveWithRecovery(IEnumerable<DtbConsignmentConfirmation> confirmationsToSave)
		{
			SetLastSelectedEntities();

			var notificationBuffer = new NotificationBuffer(this);
			Planner.SaveWithRecovery(confirmationsToSave, notificationBuffer);

			var errorSaving = notificationBuffer.HasErrors;
			if (errorSaving)
			{
				FilterControl.FirePerformSearch(); // reselection occurs after perform search
			}
			else
			{
				ReSelectLastSelectedEntities();
			}

			return !errorSaving;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				UnHookEvents();

				if (calendar != null)
				{
					calendar.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		// interfaces

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}

		#endregion
	}
}
