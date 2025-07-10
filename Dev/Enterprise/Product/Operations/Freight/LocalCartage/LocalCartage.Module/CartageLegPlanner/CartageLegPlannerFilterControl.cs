using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.AutoRefresh;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Module
{
	public partial class CartageLegPlannerFilterControl : ZFilterStripCommonControl
	{
		public CartageLegPlannerFilterControl()
			: base(new CartageLegPlannerFilterStripBusinessObject())
		{
			InitializeComponent();

			HookOnce();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new CartageModuleStrip();
		}

		protected override void InitialiseGridCore()
		{
			Grid.MouseDown += new MouseEventHandler(Grid_MouseDown);
			Grid.DragEnter += new DragEventHandler(this.Grid_DragEnter);
			Grid.DragOver += new DragEventHandler(this.Grid_DragOver);
			Grid.DragDrop += new DragEventHandler(this.Grid_DragDrop);

			Grid.ColorContextKey = LegGridColourScheme.LegColourKey;
			Grid.ShareActiveColorScheme = false;
			Grid.RemoveAction = RemoveAction.NoRemovePossible;
			Grid.SetModuleId(ModuleIDs.CartageLegPlanner);
			Grid.ReadOnly = false;
			Grid.ShouldSetErrorsOnTabPage = true;

			Grid.ContextMenu.MenuItems.Add(AutoRefreshMenuItem);

			var openTransportJobMenuItem = new ZMenuItem(ResString.GetMultilingualString("Freight.LocalTransport.OpenTransportJob", "Open Transport Job"));
			openTransportJobMenuItem.Click += new EventHandler(OpenTransportJobMenuItem_Click);
			Grid.ContextMenu.MenuItems.Add(0, openTransportJobMenuItem);
		}

		void HookOnce()
		{
			PerformSearch += new EventHandler<PerformSearchEventArgs>(CartageLegFilterControl_PerformSearch);
		}

		void Unhook()
		{
			PerformSearch -= new EventHandler<PerformSearchEventArgs>(CartageLegFilterControl_PerformSearch);
		}

		void HookCurrentDataItem()
		{
			if (CartageLegPlanner != null)
			{
				workflowCustomFields = WorkflowCustomFieldsGridInitializer.AddWorkflowCustomFieldsColumns(Grid, CartageLegPlanner.CartageLegs, true);

				CartageLegPlanner.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			}
		}

		void UnHookCurrentDataItem()
		{
			if (workflowCustomFields != null)
			{
				workflowCustomFields.Dispose();
				workflowCustomFields = null;
			}

			if (CartageLegPlanner != null)
			{
				CartageLegPlanner.Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			}
		}

		IDisposable workflowCustomFields;

		protected override ZFilterGrid GetNewFilteredGrid()
		{
			return new ZFilterGrid();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			UnHookCurrentDataItem();

			base.OnCurrentDataItemChanged(e);

			HookCurrentDataItem();
		}

		protected override ZBool ShouldPerformSearch()
		{
			if (!HasChanges)
			{
				Globals.Message.Show(Res.GetString("220a9fb2-d283-48cc-b4a1-1d8b0cde9fe9", "Some Port Transport Legs have been modified. Save before searching again."), Res.GetString("6e2d03a5-e347-43d5-a167-7a79d64407af", "Please Save"), MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK);
				return false;
			}
			else
			{
				return true;
			}
		}

		public override Size MinimumSize
		{
			get
			{
				if (DesignModeFinder.IsDesigning || Grid == null)
				{
					return base.MinimumSize;
				}
				else
				{
					return ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(base.MinimumSize.Width), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(Grid.Top) + MinGridHeight);
				}
			}
			set { base.MinimumSize = value; }
		}

		const int MinGridHeight = 60;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Unhook();

				if (components != null)
				{
					components.Dispose();
				}

				if (fAutoRefreshTimer != null)
				{
					fAutoRefreshTimer.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		void OpenTransportJobMenuItem_Click(object sender, EventArgs e)
		{
			BusinessObject[] selectedLegs = Grid.SelectedElements;
			int maximumAllowedOpens = 5;

			if (selectedLegs.Length > maximumAllowedOpens)
			{
				Globals.Message.Show(Res.GetString("222073f3-40d7-4631-90f7-f9b39d7fa57b", "Please select no more than {0} Transport Jobs to open. Opening more at the same time can put unnecessary strain on the system.", maximumAllowedOpens));
			}
			else if (selectedLegs.Length > 0)
			{
				foreach (CommonCartageLeg leg in selectedLegs)
				{
					cartageForm = (CartageForm)ZControllerFactory.Create(ControllerIDs.Cartage).ShowEditForm(leg.Cartage);
					if (cartageForm != null)
					{
						cartageForm.SelectCartageLeg(leg);
					}
				}
			}
		}
		internal CartageForm cartageForm;

		void Grid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks == 2 && e.Button == MouseButtons.Left)
			{
				if (Grid.HitTest(e.X, e.Y).Row > -1)
				{
					HandleDoubleClick();
				}
			}
		}

		void HandleDoubleClick()
		{
			if (CurrentBusinessObjectInGrid != null)
			{
				using (new ZWaitCursorChanger())
				{
					HandleFindBoxOKButton(new CommonCartageLeg[] { CurrentBusinessObjectInGrid });
				}
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		void ShowNoSelectedMessage()
		{
			Globals.Message.Show(Res.GetString("6ed33557-5388-44f6-a075-9255ac4acf5a", "Please select a record in the grid."),
				Res.GetString("c584f8e3-f8be-4bdd-bef3-98bd883647ab", "No record selected..."), MessageBoxButtons.OK, MessageBoxIcon.Information, DialogResult.OK);
		}

		void HandleFindBoxOKButton(CommonCartageLeg[] selectedBusinessObjects)
		{
			if (selectedBusinessObjects != null &&
				selectedBusinessObjects.Length > 0)
			{
				using (CartageLegModule module = new CartageLegModule())
				{
					IZForm form;
					form = module.ShowCartageLegForm(selectedBusinessObjects[0]);
#if DEBUG
					CartageForm_ForTesting = form;
#endif
				}
			}
		}
#if DEBUG
		internal IZForm CartageForm_ForTesting;
#endif

		void CartageLegFilterControl_PerformSearch(object sender, EventArgs e)
		{
			PerformSearchCore();
		}

		void PerformSearchCore()
		{
			if (CartageLegPlanner != null && FilterBusinessObject != null)
			{
				try
				{
					Grid.SuspendLayout(); // Ensures the grid is not refreshed on each add to the collection

					CartageLegPlannerCollection.SwapFactoryRemoveAllAndAddNew();
					Grid.ForcePreFetch();

					StopwatchForRefresh.Restart(); // reset Timer
					var startTime = ZDateTime.Now;
					var factory = CartageLegPlanner.CartageLegs.Factory;
					FactoryCacheHelper.SetIsViewingFromPortTransportLegPlanner(factory);

					var filter = new ZQuery(FilterBusinessObject.Filter);
					try
					{
						var legCountInDB = factory.GetDatabaseCount(typeof(CommonCartageLeg), filter);
						if (legCountInDB > 0 && legCountInDB <= MaximumAllowableQueriesPerSqlStatement)
						{
							filter.IncludeBlob(JobContainerLegsSchema.JU_LegNotes);
							var legsToAdd = factory.Load<CommonCartageLeg>(filter);
							CartageLegPlanner.CartageLegs.AddRange(legsToAdd);
						}

						ResultCountMessage.UpdateResultCountMessage(legCountInDB);
					}
					catch (SqlException ex) when (ex.Number == 8623)
					{
						Globals.Message.ShowWarning(ZGUIConstants.GetQueryTooComplicatedError());
					}

					var endTime = ZDateTime.Now;
					StartOrStopAutoRefreshTimer(endTime - startTime);

					ReapplySort();
				}
				finally
				{
					Grid.ResumeLayout();
				}
			}
		}

		void ReapplySort()
		{
			var listView = Grid?.ListManager?.List as IBindingListView;
			if (listView != null)
			{
				listView.ApplySort(listView.SortDescriptions);
			}
		}

		ResultCountMessage ResultCountMessage
		{
			get { return resultCountMessage ?? (resultCountMessage = new ResultCountMessage(this, MaximumAllowableQueriesPerSqlStatement, 100)); }
		}
		ResultCountMessage resultCountMessage;

		internal enum AutoRefreshStatusType
		{
			Disabled,
			PerformingSlowQuery,
			Running,
			PendingUserBusy,
			PendingHasChanges,
		}

		internal AutoRefreshStatusType AutoRefreshStatus
		{
			get { return autoRefreshStatus; }
			set
			{
				switch (value)
				{
					case AutoRefreshStatusType.Disabled:
						AutoRefreshTimer.Stop();
						CartageLegPlannerForm.AutoRefreshWarningLabel.Text = "";
						break;

					case AutoRefreshStatusType.PerformingSlowQuery:
						AutoRefreshTimer.Stop();
						CartageLegPlannerForm.AutoRefreshWarningLabel.ForeColor = Color.Orange;
						CartageLegPlannerForm.AutoRefreshWarningLabel.Text = Res.GetString("fb60c57a-1134-4100-86de-07549d0f8be9", "Auto-Refresh Suspended - The search is taking a long time...");
						break;

					case AutoRefreshStatusType.Running:
						if (!AutoRefreshTimer.Enabled) // IsRunning?
						{
							AutoRefreshTimer.Start();
						}
						var timeleft = TimeSpan.FromMinutes(Minutes) - StopwatchForRefresh.Elapsed;
						CartageLegPlannerForm.AutoRefreshWarningLabel.ForeColor = Color.DarkSeaGreen;
						CartageLegPlannerForm.AutoRefreshWarningLabel.Text = Res.GetString("5313634a-e805-46dd-aa01-731b36a9311c", "Refreshing in {0}:{1}", (int)timeleft.TotalMinutes, timeleft.Seconds.ToString("00", CultureInfo.InvariantCulture));
						break;

					case AutoRefreshStatusType.PendingUserBusy:
						CartageLegPlannerForm.AutoRefreshWarningLabel.ForeColor = Color.DarkSeaGreen;
						CartageLegPlannerForm.AutoRefreshWarningLabel.Text = Res.GetString("acbfbdd1-77d2-4436-a39d-a45a00a280dd", "Warning! Cannot perform Auto-Refresh. Please Save to Refresh.");
						break;

					case AutoRefreshStatusType.PendingHasChanges:
						CartageLegPlannerForm.AutoRefreshWarningLabel.ForeColor = Color.IndianRed;
						CartageLegPlannerForm.AutoRefreshWarningLabel.Text = Res.GetString("acbfbdd1-77d2-4436-a39d-a45a00a280dd", "Warning! Cannot perform Auto-Refresh. Please Save to Refresh.");
						break;

					default:
						throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "AutoRefreshStatusType '{0}' not supported", value.ToString()));
				}

				autoRefreshStatus = value;
			}
		}
		AutoRefreshStatusType autoRefreshStatus = AutoRefreshStatusType.Disabled;

		void StartOrStopAutoRefreshTimer(TimeSpan queryTime)
		{
			bool isSlowQuery = (queryTime.TotalSeconds > AutoRefreshSlowQueryInSeconds);

			if (AutoRefreshManager.Instance.IsAutoRefreshEnabled(ModuleIDs.CartageLegPlanner))
			{
				if (isSlowQuery && previousQueryWasSlow)
				{
					AutoRefreshWarning = AutoRefreshWarningType.SlowQuery;
					AutoRefreshStatus = AutoRefreshStatusType.PerformingSlowQuery;
				}
				else
				{
					AutoRefreshWarning = AutoRefreshWarningType.None;
					AutoRefreshStatus = AutoRefreshStatusType.Running;
				}
			}
			else
			{
				AutoRefreshWarning = AutoRefreshWarningType.None;
			}
			previousQueryWasSlow = isSlowQuery;
		}
		const double AutoRefreshSlowQueryInSeconds = 5;
		bool previousQueryWasSlow;

		protected void HandleAutoRefreshClick(object sender, EventArgs e)
		{
			ISecurityCheckpoint autoRefreshModuleGrids = EnvProxy.Instance.Security.AutoRefreshModuleGrids;
			if (autoRefreshModuleGrids.IsAllowed)
			{
				MenuItem item = (MenuItem)sender;
				item.Checked = !item.Checked;

				byte currentTimeout = AutoRefreshManager.Instance.GetAutoRefreshTimeOut(ModuleIDs.CartageLegPlanner);

				if (item.Checked)
				{
					using (AutoRefreshForm autoRefreshForm = new AutoRefreshForm(currentTimeout))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(autoRefreshForm) == DialogResult.OK)
						{
							AutoRefreshManager.Instance.SetAutoRefreshTimeOut(ModuleIDs.CartageLegPlanner, true, autoRefreshForm.BizO.AutoRefreshTimeOut);
							UpdateAutoRefreshMenuItemText();
							SetAutoRefreshTimerInterval(autoRefreshForm.BizO.AutoRefreshTimeOut);
							AutoRefreshStatus = AutoRefreshStatusType.Running;
						}
						else
						{
							item.Checked = false;
						}
					}
				}
				else
				{
					if (IsAutoRefreshTimerCreated)
					{
						AutoRefreshStatus = AutoRefreshStatusType.Disabled;
					}
					AutoRefreshManager.Instance.SetAutoRefreshTimeOut(ModuleIDs.CartageLegPlanner, false, currentTimeout);
				}
			}
			else
			{
				autoRefreshModuleGrids.ShowError();
			}
		}

		void UpdateAutoRefreshMenuItemText()
		{
			AutoRefreshMenuItem.Text = GetAutoRefreshMenuItemText();
		}

		MultilingualString GetAutoRefreshMenuItemText()
		{
			byte timeOut = AutoRefreshManager.Instance.GetAutoRefreshTimeOut(ModuleIDs.CartageLegPlanner);
			return ResString.GetMultilingualString("124795e5-0e84-4fdb-91f0-edfa3ff4750c", "Auto &Refresh (every {0})", AutoRefreshManager.Instance.GetTimeoutDescription(timeOut));
		}

		internal ZFilterGridMenuItem AutoRefreshMenuItem
		{
			get
			{
				if (fAutoRefreshMenuItem == null)
				{
					fAutoRefreshMenuItem = new ZFilterGridMenuItem(GetAutoRefreshMenuItemText(), HandleAutoRefreshClick)
					{
						Checked = AutoRefreshManager.Instance.IsAutoRefreshEnabled(ModuleIDs.CartageLegPlanner)
					};
				}
				return fAutoRefreshMenuItem;
			}
		}
		ZFilterGridMenuItem fAutoRefreshMenuItem;

		bool IsAutoRefreshTimerCreated
		{
			get { return fAutoRefreshTimer != null; }
		}

		internal IWindowsTimer AutoRefreshTimer
		{
			get
			{
				if (fAutoRefreshTimer == null)
				{
					fAutoRefreshTimer = new ProxyWindowsTimer();
					fAutoRefreshTimer.Tick += new EventHandler(fAutoRefreshTimer_Tick);
					SetAutoRefreshTimerInterval(AutoRefreshManager.Instance.GetAutoRefreshTimeOut(ModuleIDs.CartageLegPlanner));
				}
				return fAutoRefreshTimer;
			}
		}
		IWindowsTimer fAutoRefreshTimer;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		void SetAutoRefreshTimerInterval(byte minutes)
		{
			this.Minutes = minutes;
			StopwatchForRefresh.Restart();

			int oneSecond = 1000;
			AutoRefreshTimer.Interval = oneSecond;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		byte Minutes;

		readonly internal Stopwatch StopwatchForRefresh = new Stopwatch();

		void fAutoRefreshTimer_Tick(object sender, EventArgs e)
		{
			bool timeToRefresh = StopwatchForRefresh.Elapsed >= TimeSpan.FromMinutes(Minutes);

			if (timeToRefresh)
			{
				TryRefresh();
			}
			else
			{
				AutoRefreshStatus = AutoRefreshStatusType.Running;
			}
		}

		void TryRefresh()
		{
			if (!HasChanges)
			{
				AutoRefreshStatus = AutoRefreshStatusType.PendingHasChanges;
			}
			else if (!IsUserIdle)
			{
				AutoRefreshStatus = AutoRefreshStatusType.PendingUserBusy;
			}
			else
			{
				PerformSearchCore();
			}
		}

		ZBool IsUserIdle
		{
			get { return !IsEditingAControl; }
		}

		ZBool IsEditingAControl
		{
			get
			{
				Control control = ParentForm.GetFrontMostActiveControl();
				return control != null && EditableControl.Get(control).IsEditing;
			}
		}

		ZBool HasChanges
		{
			get { return CartageLegPlanner != null && !CartageLegPlanner.HasChanges; }
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			bool isSearchPending = AutoRefreshStatus == AutoRefreshStatusType.PendingHasChanges || AutoRefreshStatus == AutoRefreshStatusType.PendingUserBusy;
			if (isSearchPending)
			{
				PerformSearchCore();
			}
		}

		CartageLegPlanner CartageLegPlanner
		{
			get { return (CartageLegPlanner)CurrentDataItem; }
		}

		CartageLegPlannerCollection CartageLegPlannerCollection
		{
			get { return (CartageLegPlannerCollection)DataSource; }
		}

		CartageLegPlannerForm CartageLegPlannerForm
		{
			get { return (CartageLegPlannerForm)ParentForm; }
		}

		CommonCartageLeg CurrentBusinessObjectInGrid
		{
			get
			{
				bool hasCurrent = (Grid != null && Grid.ListManager != null && Grid.ListManager.Position >= 0);
				return hasCurrent ? (CommonCartageLeg)Grid.ListManager.GetCurrent() : null;
			}
		}

		void Grid_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data != null && e.Data.GetDataPresent(typeof(ArrayList)))
			{
				if (IsItemDriverOrWorkSheetOrTruck(e))
				{
					e.Effect = DragDropEffects.Scroll | DragDropEffects.Copy;
				}
			}
		}

		void Grid_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data != null && e.Data.GetDataPresent(typeof(ArrayList)))
			{
				if (IsItemDriverOrWorkSheetOrTruck(e))
				{
					Grid.UnSelectAll();

					DataGrid.HitTestInfo testInfo = Grid.HitTest(Grid.PointToClient(ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Y))));

					if (testInfo.Row >= 0)
					{
						Grid.Select(testInfo.Row);
					}
				}
			}
		}

		void Grid_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data != null && e.Data.GetDataPresent(typeof(ArrayList)))
			{
				if (IsItemDriverOrWorkSheetOrTruck(e))
				{
					var driverOrWorkSheetOrTrucks = (BusinessObject[])((ArrayList)e.Data.GetData(typeof(ArrayList))).ToArray(typeof(BusinessObject));
					var testInfo = Grid.HitTest(Grid.PointToClient(ControlDpiScalingHelper.NewScaledPoint(
						CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.X),
						CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Y))));
					if (testInfo.Row >= 0)
					{
						var cartageLeg = Grid.ListManager.List[testInfo.Row] as CommonCartageLeg;
						if (cartageLeg != null)
						{
							var isDriver = driverOrWorkSheetOrTrucks[0].GetType().IsAssignableFrom(typeof(GlbStaff));
							var isRunSheet = driverOrWorkSheetOrTrucks[0].GetType().IsAssignableFrom(typeof(CommonWorkSheet));
							var isTruck = driverOrWorkSheetOrTrucks[0].GetType().IsAssignableFrom(typeof(RefEquipment));

							ZString dropDescription = isDriver ? Res.GetString("d043890d-a159-4da3-a173-5c29148c2e7e", "Driver") :
								isTruck ? Res.GetString("7a6f0e7e-7343-4f29-9d85-bd9d3e2cf7b2", "Vehicle") : Res.GetString("4f284178-b56d-4e02-b789-013971bb5bf0", "Run Sheet");

							if (cartageLeg.QuickGSDriverInfo.ReadOnly)
							{
								Globals.Message.ShowError(Res.GetString("bec5327d-da0a-4f32-bd46-9fdc0d0097a7", "Cannot attach a {0} to a Port Transport Leg that has already begun. You will need to update the Port Transport Leg manually.", dropDescription));
							}
							else if (isDriver)
							{
								cartageLeg.QuickGSDriver = ((GlbStaff)driverOrWorkSheetOrTrucks[0]).GS_Code;
							}
							else if (isRunSheet)
							{
								var runSheet = (CommonWorkSheet)driverOrWorkSheetOrTrucks[0];
								if (runSheet.HasRunSheetError)
								{
									Globals.Message.ShowError(Res.GetString("b32fd791-33af-4381-9af4-be120e5978d1", "Cannot attach a Port Transport Leg to a Run Sheet with Status: {0}", runSheet.StatusDescription));
								}
								else
								{
									runSheet.CartageLegs.Add(cartageLeg);
								}
							}
							else if (isTruck)
							{
								cartageLeg.QuickRQTruck = driverOrWorkSheetOrTrucks[0].PK;
							}
						}
					}
				}
			}
		}

		bool IsItemDriverOrWorkSheetOrTruck(DragEventArgs e)
		{
			ArrayList list = ((ArrayList)e.Data.GetData(typeof(ArrayList)));

			bool isItemDriverOrWorkSheetOrTruck = list.Count > 0 && list[0] != null &&
				(list[0].GetType().IsAssignableFrom(typeof(GlbStaff))) || list[0].GetType().IsAssignableFrom(typeof(CommonWorkSheet))
				|| list[0].GetType().IsAssignableFrom(typeof(RefEquipment));
			return isItemDriverOrWorkSheetOrTruck;
		}
	}
}
