using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Interop;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.AutoRefresh;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class RunSheetDashboardControl : ZUserControl
	{
		public RunSheetDashboardControl()
		{
			InitializeComponent();
			this.BodyPanel.VerticalScroll.SmallChange = 10;

			if (!DesignModeFinder.IsDesigning)
			{
				SetupAutoRefreshCheckBox();
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			if (RunSheetDashboard != null)
			{
				RunSheetDashboard.FilterChanged -= new EventHandler(Filter_ValueChanged);
			}

			base.OnCurrentDataItemChanging(e);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (RunSheetDashboard != null)
			{
				RunSheetDashboard.FilterChanged += new EventHandler(Filter_ValueChanged);

				RefreshDatePanel();
				RefreshRunSheets();
			}
		}

		RunSheetDashboardCollection Dashboards
		{
			get { return (RunSheetDashboardCollection)DataSource; }
		}

		RunSheetDashboard RunSheetDashboard
		{
			get { return Dashboards != null && Dashboards.Any() ? Dashboards[0] : null; }
		}

		void Filter_ValueChanged(object sender, EventArgs e)
		{
			StartOrStopAutoRefreshTimer(ZDateTime.Today - ZDateTime.Today);

			RefreshDatePanel();
			RefreshRunSheets();

			lblErrorMessage.Visible = !RunSheetDashboard.HasRunSheets;
			lblErrorMessage.Text = RunSheetDashboard.ErrorMessage;
		}

		void RefreshDatePanel()
		{
			DateRangePanel.Visible = RunSheetDashboard.IsDateRange;
		}

		void RefreshRunSheets()
		{
			try
			{
#if !WINZOR
				if (SafeNativeMethods.CanLockWindow)
				{
					SafeNativeMethods.LockWindowUpdate(ParentForm.Handle);
				}
#endif

				// add Leg tiles that have been attached to the RunSheet
				// refresh existing leg tiles as they details may have changed
				var runSheets = RunSheetDashboard.RunSheets.ToArray();
				foreach (CommonWorkSheet runSheet in runSheets)
				{
					RunSheetTile runSheetTile;
					if (!RunSheetTiles.TryGetValue(runSheet.PK, out runSheetTile))
					{
						AddRunSheet(runSheet);
					}
					else
					{
						runSheetTile.SetDataBinding(runSheet, "");
					}
				}

				// remove leg tiles that have been detached from the RunSheet
				var runSheetPKs = runSheets.Select(r => r.PK);
				foreach (var runSheetPK in RunSheetTiles.Keys.ToArray())
				{
					if (!runSheetPKs.Contains(runSheetPK)) // ensure runSheets is an Array becuase BusinessObjectionCollection.Contains(RunSheetInOtherFactory) returns true
					{
						RemoveRunSheet(runSheetPK);
					}
				}

				PositionRunSheets();
			}
			finally
			{
#if !WINZOR
				if (SafeNativeMethods.CurrentlyLockedWindow == ParentForm.Handle)
				{
					SafeNativeMethods.UnlockWindowUpdate(ParentForm.Handle);
				}
#endif
			}
		}

		void AddRunSheet(CommonWorkSheet runSheet)
		{
			var tile = new RunSheetTile();
			RunSheetTiles.Add(runSheet.PK, tile);
			BodyPanel.Controls.Add(tile);
			tile.SetDataBinding(runSheet, "");
		}

		void RemoveRunSheet(ZGuid runSheetPK)
		{
			var tile = RunSheetTiles[runSheetPK];
			RunSheetTiles.Remove(runSheetPK);
			tile.Dispose();
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore(x, y, width, height, specified);
			PositionRunSheets();
		}

		void PositionRunSheets()
		{
			if (RunSheetDashboard != null)
			{
				int x = BodyPanel.AutoScrollPosition.X;
				int y = BodyPanel.AutoScrollPosition.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(3);

				foreach (CommonWorkSheet runSheet in RunSheetDashboard.RunSheets)
				{
					RunSheetTile runSheetTile;
					if (RunSheetTiles.TryGetValue(runSheet.PK, out runSheetTile))
					{
						if (x + runSheetTile.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(3) > this.Width)
						{
							y = y + runSheetTile.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(3);
							x = 0;
						}

						if (x != runSheetTile.Left)
						{
							ControlDpiScalingHelper.SetLeft(ref runSheetTile, x, false);
						}

						if (y != runSheetTile.Top)
						{
							ControlDpiScalingHelper.SetTop(ref runSheetTile, y, false);
						}

						x = x + runSheetTile.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(3);
					}
				}
			}
		}

		void RefreshButton_Click(object sender, EventArgs e)
		{
			PerformSearchCore();
		}

		public void PerformSearchCore()
		{
			// reset Timer
			StopwatchForRefresh.Restart();
			var startTime = ZDateTime.Now;

			// swaps in a new RunSheetDashboard with a new Factory
			// causes currentDataItemChange which causes Refresh RunSheets
			Dashboards.SwapFactoryRemoveAllAndAddNew();

			var endTime = ZDateTime.Now;

			StartOrStopAutoRefreshTimer(endTime - startTime);
		}

		void StartOrStopAutoRefreshTimer(TimeSpan queryTime)
		{
			bool isSlowQuery = (queryTime.TotalSeconds > AutoRefreshSlowQueryInSeconds);

			if (AutoRefreshManager.Instance.IsAutoRefreshEnabled(ModuleIDs.CartageRunSheetDashboard))
			{
				if (isSlowQuery && previousQueryWasSlow)
				{
					AutoRefreshStatus = AutoRefreshStatusType.PerformingSlowQuery;
				}
				else
				{
					AutoRefreshStatus = AutoRefreshStatusType.Running;
				}
			}
			previousQueryWasSlow = isSlowQuery;
		}

		const double AutoRefreshSlowQueryInSeconds = 20;
		bool previousQueryWasSlow;

		internal enum AutoRefreshStatusType
		{
			Disabled,
			PerformingSlowQuery,
			Running,
			PendingUserBusy,
			PendingHasChanges,
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		internal AutoRefreshStatusType AutoRefreshStatus
		{
			set
			{
				switch (value)
				{
					case AutoRefreshStatusType.Disabled:
						AutoRefreshTimer.Stop();
						WorkSheetTileForm.AutoRefreshWarningLabel.Text = "";
						break;

					case AutoRefreshStatusType.PerformingSlowQuery:
						AutoRefreshTimer.Stop();
						WorkSheetTileForm.AutoRefreshWarningLabel.ForeColor = Color.Orange;
						WorkSheetTileForm.AutoRefreshWarningLabel.Text = Res.GetString("7935d147-bde0-487a-99d8-cc94200b2308", "Auto-Refresh Suspended - The search is taking a long time...");
						break;

					case AutoRefreshStatusType.Running:
						if (!AutoRefreshTimer.Enabled) // IsRunning?
						{
							AutoRefreshTimer.Start();
						}
						var timeleft = TimeSpan.FromMinutes(minutes) - StopwatchForRefresh.Elapsed;
						WorkSheetTileForm.AutoRefreshWarningLabel.ForeColor = Color.DarkSeaGreen;
						WorkSheetTileForm.AutoRefreshWarningLabel.Text = Res.GetString("68f61079-7cab-4fef-9795-f66c5628bf1f", "Refreshing in {0}:{1}", (int)timeleft.TotalMinutes, timeleft.Seconds.ToString("00", CultureInfo.InvariantCulture));
						break;

					case AutoRefreshStatusType.PendingUserBusy:
						WorkSheetTileForm.AutoRefreshWarningLabel.ForeColor = Color.DarkSeaGreen;
						WorkSheetTileForm.AutoRefreshWarningLabel.Text = Res.GetString("314a6359-da56-460c-8564-b4d4bab96eee", "Auto-Refresh Pending - User Typing...");
						break;

					case AutoRefreshStatusType.PendingHasChanges:
						WorkSheetTileForm.AutoRefreshWarningLabel.ForeColor = Color.IndianRed;
						WorkSheetTileForm.AutoRefreshWarningLabel.Text = Res.GetString("f19e6ad7-7a33-408d-8466-40a2ce2de623", "Warning! Cannot perform Auto-Refresh. Please Save to Refresh.");
						break;

					default:
						throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "AutoRefreshStatusType '{0}' not supported", value.ToString()));
				}
			}
		}

		void SetupAutoRefreshCheckBox()
		{
			UpdateAutoRefreshMenuItemText();
			AutoRefreshCheckBox.Checked = AutoRefreshManager.Instance.IsAutoRefreshEnabled(ModuleIDs.CartageRunSheetDashboard);
			AutoRefreshCheckBox.CheckedChanged += new EventHandler(this.AutoRefreshCheckBox_CheckedChanged);
		}

		void AutoRefreshCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			HandleAutoRefreshCheckedChanged();
		}

		void UpdateAutoRefreshMenuItemText()
		{
			byte timeOut = AutoRefreshManager.Instance.GetAutoRefreshTimeOut(ModuleIDs.CartageRunSheetDashboard);
			AutoRefreshCheckBox.Text = Res.GetString("9b2de211-262a-4fe9-b0b7-d65f365e4c7f", "Auto &Refresh (every {0})", AutoRefreshManager.Instance.GetTimeoutDescription(timeOut));
		}

		void HandleAutoRefreshCheckedChanged()
		{
			ISecurityCheckpoint autoRefreshModuleGrids = EnvProxy.Instance.Security.AutoRefreshModuleGrids;
			if (autoRefreshModuleGrids.IsAllowed)
			{
				byte currentTimeout = AutoRefreshManager.Instance.GetAutoRefreshTimeOut(ModuleIDs.CartageRunSheetDashboard);

				if (AutoRefreshCheckBox.Checked)
				{
					using (AutoRefreshForm autoRefreshForm = new AutoRefreshForm(currentTimeout))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(autoRefreshForm) == DialogResult.OK)
						{
							AutoRefreshManager.Instance.SetAutoRefreshTimeOut(ModuleIDs.CartageRunSheetDashboard, true, autoRefreshForm.BizO.AutoRefreshTimeOut);
							UpdateAutoRefreshMenuItemText();
							SetAutoRefreshTimerInterval(autoRefreshForm.BizO.AutoRefreshTimeOut);
							AutoRefreshStatus = AutoRefreshStatusType.Running;
						}
						else
						{
							AutoRefreshCheckBox.Checked = false;
						}
					}
				}
				else
				{
					if (IsAutoRefreshTimerCreated)
					{
						AutoRefreshStatus = AutoRefreshStatusType.Disabled;
					}
					AutoRefreshManager.Instance.SetAutoRefreshTimeOut(ModuleIDs.CartageRunSheetDashboard, false, currentTimeout);
				}
			}
			else
			{
				AutoRefreshCheckBox.Checked = false;
				autoRefreshModuleGrids.ShowError();
			}
		}

		bool IsAutoRefreshTimerCreated
		{
			get { return fAutoRefreshTimer != null; }
		}

		internal IWindowsTimer AutoRefreshTimer
		{
			get
			{
				if (fAutoRefreshTimer == null && !DesignModeFinder.IsDesigning)
				{
					fAutoRefreshTimer = new ProxyWindowsTimer();
					fAutoRefreshTimer.Tick += new EventHandler(fAutoRefreshTimer_Tick);
					SetAutoRefreshTimerInterval(AutoRefreshManager.Instance.GetAutoRefreshTimeOut(ModuleIDs.CartageRunSheetDashboard));
				}
				return fAutoRefreshTimer;
			}
		}
		IWindowsTimer fAutoRefreshTimer;

		void SetAutoRefreshTimerInterval(byte mins)
		{
			this.minutes = mins;
			StopwatchForRefresh.Restart();

			int oneSecond = 1000;
			AutoRefreshTimer.Interval = oneSecond;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		byte minutes = 0;

		readonly internal Stopwatch StopwatchForRefresh = new Stopwatch();

		void fAutoRefreshTimer_Tick(object sender, EventArgs e)
		{
			bool timeToRefresh = StopwatchForRefresh.Elapsed >= TimeSpan.FromMinutes(minutes);

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
			PerformSearchCore();
		}

		internal RunSheetDashboardForm WorkSheetTileForm
		{
			get { return (RunSheetDashboardForm)ParentForm; }
		}

		Dictionary<ZGuid, RunSheetTile> RunSheetTiles
		{
			get { return runSheetTiles ?? (runSheetTiles = new Dictionary<ZGuid, RunSheetTile>()); }
		}
		Dictionary<ZGuid, RunSheetTile> runSheetTiles;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
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
	}
}
