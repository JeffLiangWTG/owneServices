using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class LinkTrackCampaignClickStatUserControl : ZUserControl
	{
		public LinkTrackCampaignClickStatUserControl()
		{
			InitializeComponent();
			ReportByDropEdit.CodeBox.CharacterCasing = CharacterCasing.Normal;
			ReportTimeRangeDropEdit.CodeBox.CharacterCasing = CharacterCasing.Normal;
		}

		ClickStatModel StatModel => CurrentDataItem as ClickStatModel;
		ClickStatChartViewModel chartViewModel;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!this.IsDesignMode())
			{
				SetDataContext(CurrentDataItem as ClickStatModel);
			}
		}

		public void SetDataContext(ClickStatModel dataSource)
		{
			UnhookEvents();
			SummaryControlHost.Controls.Clear();
			SummaryControlHost.Controls.Clear();

			if (dataSource != null)
			{
				chartViewModel = new ClickStatChartViewModel(dataSource);

#if !WINZOR
				//SummaryControlHost.Child = new ClickSummaryChartControl(chartViewModel);
				summaryControlPlotView = new OxyplotView
				{
					Dock = DockStyle.Fill,
					Model = chartViewModel.TotalClicksChartModel,
					BackColor = ObjectFactory.Get<ISystemDataRegistry>().ColorTheme.FormBackgroundColor
				};

				summaryControlPlotView.InvalidatePlot(true);
				SummaryControlHost.Controls.Add(summaryControlPlotView);

				ChartContainerPanel1_Resize(this, EventArgs.Empty);

				timeControlPlotView = new OxyplotView
				{
					Dock = DockStyle.Fill,
					Model = chartViewModel.ClicksPerTimeIntervalChartModel
				};

				timeControlPlotView.InvalidatePlot(true);
				TimeControlHost.Controls.Add(timeControlPlotView);
				ChartContainerPanel2_Resize(this, EventArgs.Empty);
#endif
				HookEvents(dataSource);
			}

			RearrangeColumnsByReportBy();
		}

		public bool SummaryControlCollapsed
		{
			get { return chartContainer.Panel1Collapsed; }
			set { chartContainer.Panel1Collapsed = value; }
		}

		public bool TimeControlCollapsed
		{
			get { return chartContainer.Panel2Collapsed; }
			set { chartContainer.Panel2Collapsed = value; }
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
				LinksGrid.ReOrderColumns([ClickStatData.Schema.URL, ClickStatData.Schema.UniqueClicks, ClickStatData.Schema.ClicksRate, ClickStatData.Schema.Clicks]);
			}
			else
			{
				LinksGrid.AddToAvailableColumns(ClickStatData.Schema.Context);
				LinksGrid.ReOrderColumns([ClickStatData.Schema.Context, ClickStatData.Schema.URL, ClickStatData.Schema.UniqueClicks, ClickStatData.Schema.ClicksRate, ClickStatData.Schema.Clicks]);
			}
		}

		void ChartContainerPanel1_Resize(object sender, EventArgs e)
		{
			var container = chartContainer.Panel1;

			var control = (ZPanel)container.Controls[0];
			if (control != null)
			{
				RescaleControl(container, control);
			}
		}

		void ChartContainerPanel2_Resize(object sender, EventArgs e)
		{
			var container = chartContainer.Panel2;
			var control = (ZPanel)container.Controls[0];
			if (control != null)
			{
				RescaleControl(container, control);
			}
		}

		static void RescaleControl(Control container, ZPanel panel)
		{
			// elements inside ElementHost are DPI aware. Need to set height to be unscaled height
			var height = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(container.Height) - 35;
			ControlDpiScalingHelper.SetHeight(ref panel, Math.Max(height, 0), false);
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			UnhookEvents();
			base.OnHandleDestroyed(e);
		}

		void HookEvents(ClickStatModel statModel)
		{
			statModel.ReportByInfo.ValueChanged += RefreshChartViewModel;
			statModel.FromDateTimeInfo.ValueChanged += RefreshChartViewModel;
			statModel.ToDateTimeInfo.ValueChanged += RefreshChartViewModel;
			statModel.ReportByInfo.ValueChanged += ReportByInfo_ValueChanged;

			foreach (ClickStatData click in statModel.LinkClicks)
			{
				click.ViewInChartInfo.ValueChanged += RefreshChartViewModel;
			}

			LinksGrid.CurrentCellChanged += LinksGrid_CurrentCellChanged;
			LinksGrid.Leave += LinksGrid_Leave;

			chartViewModel.OnHighlight += ChartViewModel_OnHighlight;
		}

		void UnhookEvents()
		{
			var statModel = StatModel ?? chartViewModel?.StatModel;
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
			chartViewModel?.RefreshChartModel();
		}

		void LinksGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			if (!isHighlighting && LinksGrid.CurrentRowIndex > -1 && LinksGrid.CurrentCell.ColumnNumber > -1)
			{
				chartViewModel?.Highlight(((ClickStatData)LinksGrid.ListManager.GetCurrent()).ReportBy, false);
			}
		}

		void LinksGrid_Leave(object sender, EventArgs e)
		{
			chartViewModel.UnHighlight();
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
			chartViewModel?.RefreshChartModel(true);
		}
	}
}
