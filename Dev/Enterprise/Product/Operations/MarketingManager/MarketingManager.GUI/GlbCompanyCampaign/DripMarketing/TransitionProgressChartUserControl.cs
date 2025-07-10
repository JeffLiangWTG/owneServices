using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TransitionProgressChartUserControl : ZUserControl
	{
#if !WINZOR
		OxyplotView plotView;
#endif

		public TransitionProgressChartUserControl()
		{
			InitializeComponent();
		}

		public void SetDataContext(GlbCompanyCampaign dataSource)
		{
			MasterCampaign = dataSource;
			ViewModel = new TransitionProgressViewModel(TransitionProgressViewModelDataAdapter.GetTouchStatsData(MasterCampaign));
#if !WINZOR
			CreateOxyplotControl();
#endif
		}

		public void RefreshChart()
		{
			ViewModel?.RefreshPlotModel(TransitionProgressViewModelDataAdapter.GetTouchStatsData(MasterCampaign));
#if !WINZOR
			if (plotView != null)
			{
				plotView.Model = ViewModel?.ChartModel;
				plotView.InvalidatePlot(true);
			}
#endif
		}

#if !WINZOR
		void CreateOxyplotControl()
		{
			plotView = new OxyplotView();
			plotView.Name = "OxyplotControl";
			plotView.TabIndex = 0;
			plotView.Model = ViewModel?.ChartModel;
			plotView.Dock = DockStyle.Fill;
			Controls.Add(plotView);
			transitionedLegendRectangle.AllowOverlap(plotView);
			transitionedLegendText.AllowOverlap(plotView);
			notTransitionedLegendRectangle.AllowOverlap(plotView);
			notTransitionedLegendText.AllowOverlap(plotView);
			plotView.InvalidatePlot(true);
		}
#endif

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			if (disposing && ViewModel != null)
			{
				ViewModel.Dispose();
			}

#if !WINZOR
			if (disposing)
			{
				if (plotView != null)
				{
					plotView.Dispose();
				}
			}
#endif

			base.Dispose(disposing);
		}

		TransitionProgressViewModel ViewModel;

		//avoid size issue explicitly
		void TransitionProgressChartUserControl_SizeChanged(object sender, System.EventArgs e)
		{
#if !WINZOR
			plotView.Size = this.Size;
#endif
		}

		GlbCompanyCampaign MasterCampaign;
	}
}
