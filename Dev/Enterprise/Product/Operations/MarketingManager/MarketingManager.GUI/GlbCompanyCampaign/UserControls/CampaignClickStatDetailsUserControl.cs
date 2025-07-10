using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class CampaignClickStatDetailsUserControl : ZUserControl
	{
#if !WINZOR
		internal OxyplotView SummaryChartPlotView;
		internal OxyplotView TimePlotView;
#endif

		public CampaignClickStatDetailsUserControl()
		{
			InitializeComponent();
			ReportByDropEdit.CodeBox.CharacterCasing = CharacterCasing.Normal;
			ReportTimeRangeDropEdit.CodeBox.CharacterCasing = CharacterCasing.Normal;
		}

		ClickStatModel StatModel
		{
			get { return ((ClickStatModel)CurrentDataItem); }
		}

		ClickStatChartViewModel ChartViewModel
		{
			get { return chartViewModel ?? (chartViewModel = new ClickStatChartViewModel(StatModel)); }
		}
		ClickStatChartViewModel chartViewModel;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!this.IsDesignMode())
			{
#if !WINZOR
				SummaryChartPlotView = new OxyplotView
				{
					Dock = DockStyle.Fill,
					BackColor = Color.White
				};
				innerSplitContainer.Panel2.Controls.Add(SummaryChartPlotView);
				innerSplitContainer.Panel2.Resize += InnerContainerPanel2_Resize;

				TimePlotView = new OxyplotView
				{
					Dock = DockStyle.Fill,
					BackColor = Color.White
				};
				outerSplitContainer.Panel2.Controls.Add(TimePlotView);
				outerSplitContainer.Panel2.Resize += OuterContainerPanel2_Resize;

				SetDataContext(StatModel);
#endif
			}
		}

		public void SetDataContext(ClickStatModel model)
		{
			if (model != null && StatModel != null)
			{
				UnhookEvents();

				chartViewModel = new ClickStatChartViewModel(model);

#if !WINZOR
				SummaryChartPlotView.Model = ChartViewModel.TotalClicksChartModel;
				SummaryChartPlotView.InvalidatePlot(true);

				TimePlotView.Model = ChartViewModel.ClicksPerTimeIntervalChartModel;
				TimePlotView.InvalidatePlot(true);

				InnerContainerPanel2_Resize(this, EventArgs.Empty);
				OuterContainerPanel2_Resize(this, EventArgs.Empty);
#endif
				HookEvents();
			}

			RearrangeColumnsByReportBy();
		}

		public bool SummaryControlCollapsed
		{
			get { return innerSplitContainer.Panel2Collapsed; }
			set { innerSplitContainer.Panel2Collapsed = value; }
		}

		public void RefreshCharts()
		{
			ChartViewModel?.RefreshChartModel(true);
		}

		void ReportByInfo_ValueChanged(object sender, EventArgs e)
		{
			RearrangeColumnsByReportBy();
		}

		void RearrangeColumnsByReportBy()
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

		void InnerContainerPanel2_Resize(object sender, EventArgs e)
		{
			var container = innerSplitContainer.Panel2;
#if !WINZOR
			var chart = (OxyplotView)container.Controls[0];

			if (chart != null)
			{
				// elements inside ElementHost are DPI aware. Need to set height to be unscaled height
				var height = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(container.Height) - 35;
				ControlDpiScalingHelper.SetHeight(ref chart, Math.Max(height, 0), false);
			}
#endif
		}

		void OuterContainerPanel2_Resize(object sender, EventArgs e)
		{
			var container = outerSplitContainer.Panel2;
#if !WINZOR
			var chart = (OxyplotView)container.Controls[0];
			if (chart != null)
			{
				// elements inside ElementHost are DPI aware. Need to set height to be unscaled height
				var width = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(container.Width) - 50;
				var height = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(container.Height) - 25;
				ControlDpiScalingHelper.SetWidth(ref chart, Math.Max(width, 0), false);
				ControlDpiScalingHelper.SetHeight(ref chart, Math.Max(height, 0), false);
			}
#endif
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			UnhookEvents();
			base.OnHandleDestroyed(e);
		}

		void HookEvents()
		{
			StatModel.ReportByInfo.ValueChanged += RefreshChartViewModel;
			StatModel.FromDateTimeInfo.ValueChanged += RefreshChartViewModel;
			StatModel.ToDateTimeInfo.ValueChanged += RefreshChartViewModel;
			StatModel.ReportByInfo.ValueChanged += ReportByInfo_ValueChanged;

			foreach (ClickStatData click in StatModel.LinkClicks)
			{
				click.ViewInChartInfo.ValueChanged += RefreshChartViewModel;
			}

			LinksGrid.CurrentCellChanged += LinksGrid_CurrentCellChanged;
			LinksGrid.Leave += LinksGrid_Leave;

			ChartViewModel.OnHighlight += ChartViewModel_OnHighlight;
		}

		void UnhookEvents()
		{
			var statModel = StatModel;
			if (statModel == null && chartViewModel != null)
			{
				statModel = chartViewModel.StatModel;
			}
			if (statModel != null)
			{
				statModel.ReportByInfo.ValueChanged -= RefreshChartViewModel;
				statModel.FromDateTimeInfo.ValueChanged -= RefreshChartViewModel;
				statModel.ToDateTimeInfo.ValueChanged -= RefreshChartViewModel;
				statModel.ReportByInfo.ValueChanged -= ReportByInfo_ValueChanged;

				foreach (ClickStatData click in statModel.LinkClicks)
				{
					click.ViewInChartInfo.ValueChanged -= RefreshChartViewModel;
				}
			}

			LinksGrid.CurrentCellChanged -= LinksGrid_CurrentCellChanged;
			LinksGrid.Leave -= LinksGrid_Leave;

			if (chartViewModel != null)
			{
				chartViewModel.OnHighlight -= ChartViewModel_OnHighlight;
			}
		}

		void RefreshChartViewModel(object sender, EventArgs e)
		{
			if (ChartViewModel != null)
			{
				ChartViewModel.RefreshChartModel();
			}
		}

		void LinksGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			if (!isHighlighting && LinksGrid.CurrentRowIndex > -1 && LinksGrid.CurrentCell.ColumnNumber > -1)
			{
				ChartViewModel.Highlight(((ClickStatData)LinksGrid.ListManager.GetCurrent()).ReportBy, false);
			}
		}

		void LinksGrid_Leave(object sender, EventArgs e)
		{
			ChartViewModel.UnHighlight();
			LinksGrid.UnSelectAll();
		}

		void ChartViewModel_OnHighlight(object sender, EventArgs e)
		{
			var highlightEventArgs = e as ClickStatChartViewModel.HighlightEventArgs;
			if (highlightEventArgs != null && highlightEventArgs.FromChart)
			{
				isHighlighting = true;

				LinksGrid.Focus();
				LinksGrid.UnSelectAll();

				var highlightedLinks = StatModel.LinkClicks.Cast<ClickStatData>().Where(d => d.ReportBy == highlightEventArgs.Category);
				int index = 0;
				foreach (var link in highlightedLinks)
				{
					int highlightedRowIndex = LinksGrid.List.IndexOf(link);
					LinksGrid.Select(highlightedRowIndex);
					if (index == 0)
					{
						LinksGrid.CurrentRowIndex = highlightedRowIndex;
					}
					index++;
				}

				isHighlighting = false;
			}
		}
		bool isHighlighting;

		void RefreshButton_Click(object sender, EventArgs e)
		{
			if (ChartViewModel != null)
			{
				ChartViewModel.RefreshChartModel(true);
			}
		}

		void LinksGrid_DoubleClick(object sender, EventArgs e)
		{
			if (LinksGrid.ListManager.List.Cast<ClickStatData>().Any())
			{
				ClickStatData selectedItem = (ClickStatData)LinksGrid.ListManager.GetCurrent();
				if (selectedItem != null)
				{
					WebUrlLauncher.Launch(selectedItem.URL);
				}
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (chartViewModel != null)
			{
				chartViewModel.StatModel = null;
			}

			if (disposing)
			{
				components?.Dispose();
#if !WINZOR
				if (SummaryChartPlotView != null)
				{
					SummaryChartPlotView.Dispose();
				}

				if (TimePlotView != null)
				{
					TimePlotView.Dispose();
				}
#endif
			}
			base.Dispose(disposing);
		}
	}
}
