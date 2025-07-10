using System;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class ResultsByRecipientUserControl : ZUserControl
	{
		protected ZArchitecture.ZGrid RecipientsGrid { private set; get; }

		public ResultsByRecipientUserControl(GlbCompanyCampaign campaign)
		{
			InitializeComponent();
			AddResultsByRecipientFilterControl(campaign);
		}

		void AddResultsByRecipientFilterControl(GlbCompanyCampaign campaign)
		{
			var filterControl = new ResultsByRecipientFilterControl(campaign, CreateResultsByRecipientFilter());
			filterControl.SuspendLayout();
			this.ResultsByRecipientsSplitContainer.Panel1.Controls.Add(filterControl);
			// 
			// filterControl
			// 
			filterControl.AllowDrop = true;
			BindingSource.SetBindingMember(filterControl, ".");
			filterControl.Dock = DockStyle.Fill;
			filterControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			filterControl.Name = "ResultsByRecipientFilterControl";
			filterControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 228, true);
			filterControl.TabIndex = 3;
			filterControl.ResumeLayout(true);
			filterControl.PerformLayout();

			RecipientsGrid = filterControl.Grid;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			RecipientsGrid.ListManager.CurrentChanged += ListManager_CurrentChanged;
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			var currentItem = RecipientsGrid.ListManager.GetCurrent() as GlbCompanyCampaignItem;
			if (currentItem != null)
			{
				RecipientAnswersGrid.SetDataBinding(currentItem.SubmittedAnswers, "");
			}
			else
			{
				RecipientAnswersGrid.List.Clear();
			}
		}

		protected virtual LearningCentreCampaignItemFilterBusinessObject CreateResultsByRecipientFilter()
			=> new LearningCentreCampaignItemFilterBusinessObject();
	}
}
