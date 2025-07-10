using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class CampaignClickStatSummaryUserControl : ZUserControl
	{
		public CampaignClickStatSummaryUserControl()
		{
			InitializeComponent();
			ReportByDropEdit.CodeBox.CharacterCasing = CharacterCasing.Normal;
			ReportRangeDropEdit.CodeBox.CharacterCasing = CharacterCasing.Normal;
		}

		#region TrackingStatButton

		internal ZToolStripButton TrackingStatButton
		{
			get { return trackingStatToolStripButton; }
		}

		void TrackingStatButton_Click(object sender, EventArgs e)
		{
			var statModel = CurrentDataItem as ClickStatModel;
			if (statModel != null)
			{
				ZFormModaliser.Show(new TrackingStatForm(statModel), ParentForm);
			}
		}

		#endregion

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (CurrentDataItem != null)
			{
				((ClickStatModel)CurrentDataItem).ReportByInfo.ValueChanged -= ReportByInfo_ValueChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (CurrentDataItem != null)
			{
				((ClickStatModel)CurrentDataItem).ReportByInfo.ValueChanged += ReportByInfo_ValueChanged;
			}
		}

		void ReportByInfo_ValueChanged(object sender, EventArgs e)
		{
			if (CurrentDataItem != null && ((ClickStatModel)CurrentDataItem).ReportBy == ReportByList.Codes.Url)
			{
				LinksGrid.RemoveFromAvailableColumns(ClickStatData.Schema.Context);
				LinksGrid.SetAllColumnsVisible(true);
				LinksGrid.ReOrderColumns([ClickStatData.Schema.URL, ClickStatData.Schema.UniqueClicks, ClickStatData.Schema.ClicksRatePercentage, ClickStatData.Schema.Clicks]);
			}
			else
			{
				LinksGrid.AddToAvailableColumns(ClickStatData.Schema.Context);
				LinksGrid.ReOrderColumns([ClickStatData.Schema.Context, ClickStatData.Schema.UniqueClicks, ClickStatData.Schema.ClicksRatePercentage, ClickStatData.Schema.Clicks]);
				LinksGrid.SetColumnVisible(false, ClickStatData.Schema.URL);
			}
		}

		void LinksGrid_DoubleClick(object sender, EventArgs e)
		{
			if (LinksGrid.ListManager.List.Cast<ClickStatData>().Any())
			{
				ClickStatData selectedItem = (ClickStatData)LinksGrid.ListManager.GetCurrent();
				if (selectedItem != null && Control.ModifierKeys == Keys.Control)
				{
					WebUrlLauncher.Launch(selectedItem.URL);
				}
				else if (selectedItem != null)
				{
					if (CurrentDataItem != null && ((ClickStatModel)CurrentDataItem).ReportBy == ReportByList.Codes.Url)
					{
						((GlbCompanyCampaignForm)ParentForm).FocusOnCampaignItem(FocusOnTrackingTabTypes.DestinationUrl, selectedItem.URL);
					}
					else
					{
						((GlbCompanyCampaignForm)ParentForm).FocusOnCampaignItem(FocusOnTrackingTabTypes.ContextName, selectedItem.Context);
					}
				}
			}
		}
	}
}
