using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MarketingManager.GUI
{
	[CodeAlive("Used for Winzor")]
	public partial class TouchSummaryGrid : ZUserControl, ITouchSummary
	{
		public TouchSummaryGrid()
		{
			InitializeComponent();
			viewModel = new TouchSummaryViewModel();
			viewModel.TransitionProgressChanged += OnLongRefreshProgress;
			viewModel.EndTransitionProgress += OnEndLongRefresh;
			viewModel.NewCampaignSave += OnNewCampaignSaved;
			touchGrid.MouseDoubleClick += TouchGrid_DoubleClick;
			touchGrid.AfterBind += TouchGrid_AfterBind;
			SetDataBinding(viewModel, "");
		}

		public void SetDataContext(GlbCompanyCampaign campaign)
		{
			viewModel.SetDataSource(campaign);
			OnInitialCampaignSelected(campaign?.Horizontals?.FirstOrDefault()?.Campaigns?.FirstOrDefault());
		}

		public TouchSummaryViewModel ViewModel
		{
			get
			{
				return viewModel;
			}
		}

		public event EventHandler<GlbCompanyCampaign.TransitionProgressEventArgs> BeginLongRefresh;
		public event EventHandler<GlbCompanyCampaign.TransitionProgressEventArgs> LongRefreshProgress;
		public event EventHandler EndLongRefresh;
		public event EventHandler<CampaignSelectedEventArgs> CampaignSelected;
		public event EventHandler<CampaignSelectedEventArgs> InitialCampaignSelected;

		void AddHorizontalButton_Click(object sender, EventArgs e)
		{
			viewModel.AddHorizontal();
		}

		void AddVerticalButton_Click(object sender, EventArgs e)
		{
			var campaign = touchGrid.ListManager.GetCurrent() as GlbCompanyCampaign;
			if (campaign != null)
			{
				viewModel.AddVertical(campaign.G0_HorizontalId);
			}
			else
			{
				Globals.Message.Show(
					Res.GetString("27FD9857-0FBC-4316-9C76-6B4656B24F9A", "Please select a Touch"),
					Res.GetString("46E4E90E-0669-4A92-B821-491DA69A6363", "New Vertical Touch"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
		}

		void RemoveCampaignButton_Click(object sender, EventArgs e)
		{
			var deleteCaption = Res.GetString("FEF284D2-44B7-46B9-99CA-758E5CED9794", "Delete Campaign");
			var campaign = touchGrid.ListManager.GetCurrent() as GlbCompanyCampaign;
			if (campaign != null)
			{
				var result = Globals.Message.Show(
					Res.GetString("EA470E3B-402B-4F53-BEB3-2ACA7CD0C82E", "Are you sure you want to delete {0}", campaign.TouchFullName),
					deleteCaption,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
				{
					viewModel.RemoveCampaign(campaign);
					var touchCampaigns = viewModel?.MasterCampaign?.AllTouches;
					if (touchCampaigns != null && touchCampaigns.Count > 0)
					{
						var firstCampaign = touchCampaigns[0];
						touchGrid.SelectSingleElement(firstCampaign);
					}
					RefreshButton_Click(null, e);
				}
			}
			else
			{
				Globals.Message.Show(
					Res.GetString("FE9405B1-5DF4-42D4-913C-5E84F3C817D6", "Please select a Touch to delete"),
					deleteCaption,
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
		}

		void OpenCampaignButton_Click(object sender, EventArgs e)
		{
			var campaign = touchGrid.ListManager.GetCurrent() as GlbCompanyCampaign;
			if (campaign != null)
			{
				TouchSummaryViewModel.OpenCampaign(campaign);
			}
			else
			{
				Globals.Message.Show(
					Res.GetString("E8EF75F2-3830-4230-BC7A-D7E1D81C35E7", "Please select a Touch to edit"),
					Res.GetString("CFFE8942-B962-41A4-BC60-DEFE0410D7D4", "Edit Touch"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
		}

		void LaunchButton_Click(object sender, EventArgs e)
		{
			var caption = Res.GetString("86D4633C-4038-4002-BB9C-5CD8305A39FE", "Cannot Transition Master List");
			if (viewModel.MasterCampaignHasChanges)
			{
				Globals.Message.ShowError(Res.GetString("B1920141-BAF9-4D31-A5DC-E82B0DA40D20", "Please save this campaign before proceeding."), caption);
				return;
			}

			var transitionsRulesErrors = viewModel.CheckTransitionsRules();
			if (!string.IsNullOrEmpty(transitionsRulesErrors))
			{
				Globals.Message.ShowError(transitionsRulesErrors, caption);
				return;
			}

			try
			{
				OnBeginLongRefresh(ResString.GetMultilingualString("1318a399-089c-47b5-88ea-5d47317ee2c6", "The transition of Master List contacts is in progress..."));
				var errors = viewModel.Launch();
				if (!string.IsNullOrEmpty(errors))
				{
					Globals.Message.ShowError(errors);
				}
			}
			finally
			{
				OnEndLongRefresh(this, EventArgs.Empty);
			}
		}

		void RefreshButton_Click(object sender, EventArgs e)
		{
			try
			{
				OnBeginLongRefresh(ResString.GetMultilingualString("19ab3776-d430-480e-817f-ae6362466b85", "Refreshing summary data..."));
				viewModel.RefreshStats();
			}
			finally
			{
				OnEndLongRefresh(this, EventArgs.Empty);
			}
		}

		void OnBeginLongRefresh(string statusMessage)
		{
			BeginLongRefresh?.Invoke(this, new GlbCompanyCampaign.TransitionProgressEventArgs(statusMessage));
		}

		void OnLongRefreshProgress(object sender, GlbCompanyCampaign.TransitionProgressEventArgs eventArgs)
		{
			LongRefreshProgress?.Invoke(sender ?? this, eventArgs);
		}

		void OnEndLongRefresh(object sender, EventArgs eventArgs)
		{
			EndLongRefresh?.Invoke(this, EventArgs.Empty);
		}

		void OnNewCampaignSaved(object sender, EventArgs eventArgs)
		{
			var touchCampaigns = viewModel?.MasterCampaign?.AllTouches;
			if (touchCampaigns != null && touchCampaigns.Count == 1)
			{
				var newCampaign = touchCampaigns[0];
				touchGrid.SelectSingleElement(newCampaign);
			}
		}

		void OnCampaignSelected(GlbCompanyCampaign campaign)
		{
			CampaignSelected?.Invoke(this, new CampaignSelectedEventArgs(campaign));
		}

		void OnInitialCampaignSelected(GlbCompanyCampaign campaign)
		{
			InitialCampaignSelected?.Invoke(this, new CampaignSelectedEventArgs(campaign));
		}

		void TouchGrid_AfterBind(object sender, EventArgs e)
		{
			touchGrid.ListManager.PositionChanged += TouchGrid_PositionChanged;
		}

		void TouchGrid_DoubleClick(object sender, MouseEventArgs e)
		{
			if (touchGrid.HitTest(e.Location).Row >= 0)
			{
				OpenCampaignButton_Click(sender, e);
			}
		}

		void TouchGrid_PositionChanged(object sender, EventArgs e)
		{
			var campaign = touchGrid.ListManager.GetCurrent() as GlbCompanyCampaign;
			OnCampaignSelected(campaign);
		}

		readonly TouchSummaryViewModel viewModel;
	}
}
