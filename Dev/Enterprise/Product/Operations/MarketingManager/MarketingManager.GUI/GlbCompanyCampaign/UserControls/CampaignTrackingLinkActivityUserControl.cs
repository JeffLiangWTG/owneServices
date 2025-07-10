using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class CampaignTrackingLinkActivityUserControl : ZUserControl
	{
		public CampaignTrackingLinkActivityUserControl()
		{
			InitializeComponent();
			ReportByDropEdit.CodeBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		}

		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);
			uniqueOpensLabel.BringToFront();
			RearrangeColumnsByReportBy();
		}

		void ReportByInfo_ValueChanged(object sender, System.EventArgs e)
		{
			RearrangeColumnsByReportBy();
		}

		protected override void OnCurrentDataItemChanged(System.EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (CurrentDataItem != null)
			{
				CampaignItemClickStatModel statModel = CurrentDataItem as CampaignItemClickStatModel;

				statModel.ReportByInfo.ValueChanged -= ReportByInfo_ValueChanged;
				statModel.ReportByInfo.ValueChanged += ReportByInfo_ValueChanged;

				SetTrackingStatusLabelBackColor(statModel);

				EnableViewDetailsButton(statModel);

				RearrangeColumnsByReportBy();
			}
		}

		void EnableViewDetailsButton(CampaignItemClickStatModel statModel)
		{
			ViewDetailsButton.Enabled = statModel.FirstActivityDate.IsValid;
		}

		void SetTrackingStatusLabelBackColor(CampaignItemClickStatModel statModel)
		{
			if (statModel != null)
			{
				if (statModel.BusinessEntity.G8_TrackingStatus == TrackingStatusCodes.Codes.NDR)
				{
					TrackingStatusLabel.BackColor = System.Drawing.Color.Orange;
				}
				else if (statModel.BusinessEntity.G8_TrackingStatus == TrackingStatusCodes.Codes.VER)
				{
					TrackingStatusLabel.BackColor = System.Drawing.Color.LightSkyBlue;
				}
				else
				{
					TrackingStatusLabel.BackColor = System.Drawing.Color.Silver;
				}
			}
		}

		internal void ViewDetailsButton_Click(object sender, System.EventArgs e)
		{
			var linkActivityDetailsForm = new LinkActivityDetailsForm(CurrentDataItem as CampaignItemClickStatModel);
			ZFormModaliser.Show(linkActivityDetailsForm, ParentForm);
		}

		void RearrangeColumnsByReportBy()
		{
			if (CurrentDataItem != null && ((CampaignItemClickStatModel)CurrentDataItem).ReportBy == ReportByList.Codes.Url)
			{
				LinksGrid.RemoveFromAvailableColumns(CampaignItemClickStatData.Schema.Context);
				LinksGrid.SetAllColumnsVisible(true);
				LinksGrid.ReOrderColumns([CampaignItemClickStatData.Schema.URL, CampaignItemClickStatData.Schema.Clicks, CampaignItemClickStatData.Schema.FirstClick, CampaignItemClickStatData.Schema.LastClick]);
			}
			else
			{
				LinksGrid.AddToAvailableColumns(CampaignItemClickStatData.Schema.Context);
				LinksGrid.ReOrderColumns([CampaignItemClickStatData.Schema.Context, CampaignItemClickStatData.Schema.Clicks, CampaignItemClickStatData.Schema.FirstClick, CampaignItemClickStatData.Schema.LastClick, CampaignItemClickStatData.Schema.URL]);
				LinksGrid.SetColumnVisible(false, CampaignItemClickStatData.Schema.URL);
			}
		}
	}
}
