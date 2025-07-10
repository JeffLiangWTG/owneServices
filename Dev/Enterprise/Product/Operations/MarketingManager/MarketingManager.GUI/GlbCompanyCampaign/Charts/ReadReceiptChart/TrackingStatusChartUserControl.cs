using System;
using System.Globalization;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;
using OxyPlot;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TrackingStatusChartUserControl : ZUserControl
	{
		public TrackingStatusChartUserControl()
		{
			InitializeComponent();
			panelUnsubscribedRows.AllowOutsideOfParent();
		}

		internal TrackingStatusChartViewModel StatModel;
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
			StatModel?.Dispose();
			StatModel = new TrackingStatusChartViewModel(dataSource);
			StatModel.PopulatePieModel();
#if !WINZOR
			if (plotView == null)
			{
				plotView = new OxyplotView();
				plotView.MouseUp += (sender, args) => StatModel.HandleMouseUp(new ScreenPoint(args.X, args.Y));
				plotView.Name = "OxyplotView";
				plotView.Size = ControlDpiScalingHelper.NewScaledSize(150, 110);
				plotView.Location = ControlDpiScalingHelper.NewScaledPoint(263, 10);
				Controls.Add(plotView);
			}

			StatModel.PieModel.InvalidatePlot(true);
			plotView.Model = StatModel.PieModel;
			plotView.InvalidatePlot(true);

#endif
			SetupLegend();
		}

		void SetupLegend()
		{
			panelChart.Controls.Clear();
			panelUnsubscribedRows.Controls.Clear();

			labelTotalEmails.Text = StatModel.EmailsTotal.ToString("#,#0", CultureInfo.CurrentCulture);
			labelTotalOrganizations.Text = StatModel.ClientsTotal.ToString("#,#0", CultureInfo.CurrentCulture);
			panelUnsubscribedRows.Visible = StatModel.UnsubscribedRowVisible;
#if !WINZOR
			if (plotView != null)
			{
				plotView.InvalidatePlot(true);
			}
#endif
			var yPosition = 0;

			foreach (var data in StatModel.ChartData)
			{
				var chartRow = new TrackingStatusChartRowUserControl(data);
				chartRow.Location = ControlDpiScalingHelper.NewScaledPoint(0, yPosition);
				yPosition += 20;
				panelChart.Controls.Add(chartRow);
			}
			StatModel.FilterByDeliveryStatus += OpenTrackingTabAndFilterByDeliveryStatus;
			StatModel.FilterByUnsubscribeStatus += OpenTrackingTabAndFilterByUnsubscribeStatus;

			if (panelUnsubscribedRows.Visible)
			{
				yPosition = 0;
				foreach (var data in StatModel.UnsubscribedData)
				{
					var chartRow = new TrackingStatusChartRowUserControl(data);
					chartRow.Location = ControlDpiScalingHelper.NewScaledPoint(0, yPosition);
					yPosition += 20;
					chartRow.AllowOutsideOfParent();
					panelUnsubscribedRows.Controls.Add(chartRow);
				}
			}
		}

		void OpenTrackingTabAndFilterByDeliveryStatus(object sender, TrackingStatusDescriptionEventArgs e)
		{
			OpenTrackingTab(e,
				(campaign, args) => TouchSummaryViewModel.OpenCampaign(campaign, args.Description),
				(form, args) => form.FocusOnCampaignItem<ZString>(FocusOnTrackingTabTypes.StatusDescription, e.Description));
		}

		void OpenTrackingTabAndFilterByUnsubscribeStatus(object sender, UnsubscribedStatusDescriptionEventArgs e)
		{
			OpenTrackingTab(e,
				(campaign, args) => TouchSummaryViewModel.OpenCampaign(campaign, args.Unsubscribed),
				(form, args) => form.FocusOnCampaignItem<ZBool>(FocusOnTrackingTabTypes.UnsubscribeStatus, e.Unsubscribed));
		}

		void OpenTrackingTab<T>(T eventArgs, Action<GlbCompanyCampaign, T> ifDripCampaign, Action<GlbCompanyCampaignForm, T> ifStdCampaign) where T : EventArgs
		{
			if (eventArgs == null)
			{
				throw new ArgumentNullException(nameof(eventArgs));
			}
			if (ifDripCampaign == null)
			{
				throw new ArgumentNullException(nameof(ifDripCampaign));
			}
			if (ifStdCampaign == null)
			{
				throw new ArgumentNullException(nameof(ifStdCampaign));
			}
			var parentForm = ParentForm as GlbCompanyCampaignForm;
			var parentFormCampaign = parentForm?.DataSource as GlbCompanyCampaign;
			var statModelCampaign = StatModel?.Campaign;
			if (parentFormCampaign == null && statModelCampaign == null)
			{
				return;
			}

			if (parentFormCampaign == null || parentFormCampaign.PK != statModelCampaign?.PK)
			{
				ifDripCampaign(statModelCampaign, eventArgs);
			}
			else
			{
				ifStdCampaign(parentForm, eventArgs);
			}
		}

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

			if (StatModel != null)
			{
				StatModel.Dispose();
				StatModel.FilterByDeliveryStatus -= OpenTrackingTabAndFilterByDeliveryStatus;
				StatModel.FilterByUnsubscribeStatus -= OpenTrackingTabAndFilterByUnsubscribeStatus;
				StatModel = null;
			}
#if !WINZOR
			if (disposing && plotView != null)
			{
				plotView.Dispose();
			}
#endif
			base.Dispose(disposing);
		}

		internal void RefreshCampaignSummary()
		{
			StatModel.PopulatePieModel();
			StatModel.PieModel.InvalidatePlot(true);
			SetupLegend();
		}
	}
}
