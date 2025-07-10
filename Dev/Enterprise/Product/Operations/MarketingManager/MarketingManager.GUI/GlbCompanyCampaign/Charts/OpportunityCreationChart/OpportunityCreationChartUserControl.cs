using System;
using System.Globalization;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class OpportunityCreationChartUserControl : ZUserControl
	{
		public ZArchitecture.ZLabel CurrentOpportunitiesTotal => labelCurrentTotal;
		public ZArchitecture.ZLabel WonOpportunitiesTotal => labelWonTotal;
		public ZArchitecture.ZLabel OtherOpportunitiesTotal => labelOtherTotal;
		public ZArchitecture.ZLabel LostOpportunitiesTotal => labelLostTotal;
		public ZArchitecture.ZLabel LinkedOpportunitiesTotal => labelTotalTotal;
		public ZArchitecture.ZLabel WinRatioOpportunitiesTotal => labelWinRatioTotal;
		public ZArchitecture.ZLabel CurrentLabel => labelCurrentRect;
		public ZArchitecture.ZLabel WonLabel => labelWonRect;
		public ZArchitecture.ZLabel LostLabel => labelLostRect;
		public ZArchitecture.ZLabel OtherLabel => labelOtherRect;

		public OpportunityCreationChartUserControl()
		{
			InitializeComponent();
		}

		OpportunityCreationChartViewModel viewModel;
#if !WINZOR
		OxyplotView plotView;
#endif

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!this.IsDesignMode())
			{
				SetDataContext(CurrentDataItem as GlbCompanyCampaign);
			}
		}

		public virtual void SetDataContext(GlbCompanyCampaign dataSource)
		{
			viewModel?.Dispose();
			viewModel = new OpportunityCreationChartViewModel(dataSource);

#if !WINZOR
			if (plotView == null)
			{
				plotView = new OxyplotView();
				plotView.Size = ControlDpiScalingHelper.NewScaledSize(150, 120);
				plotView.Location = ControlDpiScalingHelper.NewScaledPoint(3, 3);
				plotView.Name = "OxyplotView";
				Controls.Add(plotView);
				plotView.InvalidatePlot(true);
			}
#endif
			SetupChart();
		}

		void SetupChart()
		{
			viewModel.PopulatePieModel();
			labelCurrentTotal.Text = viewModel.CurrentCount.ToString(CultureInfo.CurrentCulture);
			labelWonTotal.Text = viewModel.WonCount.ToString(CultureInfo.CurrentCulture);
			labelLostTotal.Text = viewModel.LostCount.ToString(CultureInfo.CurrentCulture);
			labelOtherTotal.Text = viewModel.OtherCount.ToString(CultureInfo.CurrentCulture);
			labelTotalTotal.Text = viewModel.TotalCount.ToString(CultureInfo.CurrentCulture);
			labelWinRatioTotal.Text = viewModel.WinRatio.ToString("P0", CultureInfo.CurrentCulture);
#if !WINZOR
			if (plotView != null)
			{
				plotView.Model = viewModel.PieModel;
				plotView.InvalidatePlot(true);
			}
#endif
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}

			if (viewModel != null)
			{
				viewModel.Dispose();
				viewModel = null;
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

		public void RefreshOpportunityCreationChart()
		{
			SetupChart();
		}
	}
}
