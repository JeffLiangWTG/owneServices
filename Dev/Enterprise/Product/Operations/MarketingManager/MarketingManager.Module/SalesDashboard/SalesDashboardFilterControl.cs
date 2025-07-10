using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Integration.Recruiter;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	public partial class SalesDashboardFilterControl : ZFilterStripControl
	{
		public SalesDashboardFilterControl(SalesDashboardActivityCollection gridCollection, SalesDashboardFilterBusinessObject filterBusinessObject, bool showCurrentLoginUserInfo)
				: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			SetupControls(showCurrentLoginUserInfo);

			if (filterBusinessObject.Org != null)
			{
				var orgNameColumn = this.Grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == ViewSalesDashboardActivitySchema.Constants.VSA_OrgFullName);
				if (orgNameColumn != null)
				{
					this.Grid.ColumnStyles.Remove(orgNameColumn);
				}
			}

			salesRelationControl.DataBindings.Clear();

			if (grid.ContextMenu != null)
			{
				grid.ContextMenu.Popup += ContextMenu_Popup;
			}

#if DEBUG

			System.ComponentModel.TypeDescriptor.AddAttributes(currentUserLabel, new SuppressFormsLocalizedTestAttribute());
			System.ComponentModel.TypeDescriptor.AddAttributes(currentDateLabel, new SuppressFormsLocalizedTestAttribute());

#endif

			FilterStripsPanel.AllowOverlap(currentDateLabel);
			FilterStripsPanel.AllowOverlap(currentUserLabel);
			currentDateLabel.AllowOverlap(ToolStripPermissionsLabel);
			currentUserLabel.AllowOverlap(ToolStripPermissionsLabel);
		}

		void SetupControls(bool showCurrentLoginUserInfo)
		{
			if (!DesignModeFinder.IsDesigning && showCurrentLoginUserInfo)
			{
				var user = GlbStaff.CurrentUser;
				this.currentUserLabel.Text = string.IsNullOrEmpty(user.GS_FriendlyName) ? user.GS_FullName : user.GS_FriendlyName;
				this.currentUserLabel.Font = new Font(currentUserLabel.Font.FontFamily.Name, 13f, FontStyle.Regular);

				using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Res.CurrentLanguage)))
				{
					this.currentDateLabel.Text = ZDateTime.Now.ToString(DateTimeFormatStrings.LongDateFormatIncludingWeek);
				}
				this.currentDateLabel.Font = new Font(currentDateLabel.Font.FontFamily.Name, 10f, FontStyle.Regular);
			}
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			var campaignTrackingMenuItem = grid.ContextMenu.MenuItems.FindByName("CampaignTracking");
			if (campaignTrackingMenuItem == null)
			{
				return;
			}

			var currentCampaignActivity = grid.ListManager.GetCurrent() as CampaignSalesDashboardActivity;
			if (currentCampaignActivity == null)
			{
				campaignTrackingMenuItem.Visible = false;
			}
			else
			{
				var campaign = currentCampaignActivity.ParentCampaign as GlbCompanyCampaign;
				bool isLearningCentreCampaign = campaign is ILearningCentreCampaign;
				campaignTrackingMenuItem.Visible = !isLearningCentreCampaign;

				if (campaign?.IsTargetList ?? true)
				{
					campaignTrackingMenuItem.Visible = false;
				}
			}
		}

		#region ShowPreview

		void ShowPreviewFor(object item)
		{
			bool handled = false;

			SalesDashboardActivity activity = item as SalesDashboardActivity;
			if (activity != null)
			{
				if (!previewSplitContainer.Visible)
				{
					previewSplitContainer.Visible = true;
					LoadPreviewPanelGuiState();
				}

				salesRelationControl.DataBindings.Clear();
				salesRelationControl.SetDataBinding(activity.SalesRelationModel, "");
				salesRelationControl.ModelView.ShowCommunication = true;

				taskGrid.DataBindings.Clear();
				taskGrid.SetDataBinding(activity.ActiveAndCompletedTasks, "");

				var orderedTasks = activity.ActiveAndCompletedTasks.Cast<ProcessTask>().OrderBy(x => x.P9_Sequence);
				var selectedTask = orderedTasks.FirstOrDefault(x => x.IsCurrent)
					?? orderedTasks.LastOrDefault();
				if (selectedTask != null)
				{
					taskGrid.SelectSingleElement(selectedTask);
				}

				if (activity.ActiveAndCompletedTasks.Any())
				{
					taskNoteRichTextBox.Visible = true;
					taskNoteRichTextBox.DataBindings.Clear();
					taskNoteRichTextBox.SetDataBinding(activity.ActiveAndCompletedTasks, ProcessTasksSchema.Constants.P9_Notes);
				}
				else
				{
					taskNoteRichTextBox.Visible = false;
				}

				handled = true;
			}

			if (!handled)
			{
				previewSplitContainer.Visible = false;
			}
		}

		void gridListManager_CurrentChanged(object sender, EventArgs e)
		{
			ShowPreviewFor(Grid.ListManager.Count > 0 ? Grid.ListManager.GetCurrent() : null);
		}

		#endregion

		#region GUI State

		SalesDashboardFilterControlGuiState guiState;
		SalesDashboardFilterControlGuiState GuiState
		{
			get { return guiState ?? (guiState = new SalesDashboardFilterControlGuiState(this)); }
		}

		void LoadMainSplitterGuiState()
		{
			if (mainSplitter != null)
			{
				var loadedMainSplitterPosition = GuiState.MainSplitterPosition;
				if (loadedMainSplitterPosition > 0)
				{
					mainSplitter.SplitPosition = loadedMainSplitterPosition;
				}
			}
		}

		void LoadPreviewPanelGuiState()
		{
			if (previewSplitContainer != null)
			{
				var loadedPreviewSplitterDistance = GuiState.PreviewSplitterDistance;
				if (loadedPreviewSplitterDistance > 0)
				{
					previewSplitContainer.SplitterDistance = loadedPreviewSplitterDistance;
				}
			}

			if (salesRelationControl != null)
			{
				var loadedSalesRelationSplitterDistance = GuiState.SalesRelationSplitterDistance;
				if (loadedSalesRelationSplitterDistance > 0)
				{
					salesRelationControl.SplitterDistance = loadedSalesRelationSplitterDistance;
				}
			}

			if (taskSplitContainer != null)
			{
				var loadedTaskSplitterDistance = GuiState.TasksSplitterDistance;
				if (loadedTaskSplitterDistance > 0)
				{
					taskSplitContainer.SplitterDistance = loadedTaskSplitterDistance;
				}
			}
		}

		void SaveGuiState()
		{
			if (mainSplitter != null)
			{
				GuiState.MainSplitterPosition = mainSplitter.SplitPosition;
			}

			if (previewSplitContainer != null)
			{
				GuiState.PreviewSplitterDistance = previewSplitContainer.SplitterDistance;
			}

			if (salesRelationControl != null)
			{
				GuiState.SalesRelationSplitterDistance = salesRelationControl.SplitterDistance;
			}

			if (taskSplitContainer != null)
			{
				GuiState.TasksSplitterDistance = taskSplitContainer.SplitterDistance;
			}
		}

		#endregion

		#region Preview Pane

		protected override void OnLayout(LayoutEventArgs e)
		{
			RestrictMainSplitterPosition();
			base.OnLayout(e);
		}

		const int minimumGridHeight = 135;
		protected override void HandleGridSizing()
		{
			if (Grid != null)
			{
				if (IsFilterVisible)
				{
					Grid.Dock = DockStyle.None;
					if (Grid.Left != 0 || Grid.Right != ClientRectangle.Right)
					{
						ControlDpiScalingHelper.SetWidth(Grid, ClientSize.Width, false);
					}
				}
				else
				{
					Grid.Dock = DockStyle.Fill;
				}

				if (mainSplitter != null && Grid.Dock == DockStyle.None)
				{
					var targetGridHeight = ClientRectangle.Bottom - mainSplitter.SplitPosition - mainSplitter.Height - Grid.Top;
					if (targetGridHeight != Grid.Height)
					{
						ControlDpiScalingHelper.SetHeight(Grid, targetGridHeight, false);
						Grid.Refresh();
					}
				}
			}
		}

		void RestrictMainSplitterPosition()
		{
			if (mainSplitter != null)
			{
				var minExtraChanged = false;
				var targetMinExtra = Grid.Top + minimumGridHeight;
				if (mainSplitter.MinExtra != targetMinExtra)
				{
					mainSplitter.MinExtra = targetMinExtra;
					minExtraChanged = true;
				}

				if (minExtraChanged || (mainSplitter.SplitPosition > ClientRectangle.Bottom - Grid.Top - minimumGridHeight))
				{
					mainSplitter.SplitPosition = mainSplitter.SplitPosition; // force SplitPosition to be recalculated so that it lies within the MinExtra value
				}
			}
		}

		#endregion

		#region OnLoad/Dispose

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				Grid.ListManager.CurrentChanged += new EventHandler(gridListManager_CurrentChanged);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (!DesignModeFinder.IsDesigning)
			{
				LoadMainSplitterGuiState();
			}
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				SaveGuiState();
			}
			base.OnHandleDestroyed(e);
		}

		#endregion
	}
}
