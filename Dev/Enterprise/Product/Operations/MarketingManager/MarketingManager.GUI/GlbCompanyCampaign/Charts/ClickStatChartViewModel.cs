using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace Enterprise.MarketingManager.GUI
{
	public class ClickStatChartViewModel
	{
		public ClickStatChartViewModel(ClickStatModel statModel)
		{
			this.StatModel = statModel;
			LoadData();
			BuildTotalClicksChartModel();
			BuildClicksPerTimeIntervalChartModel();
		}
		public ClickStatModel StatModel;

		#region Properties

		public PlotModel TotalClicksChartModel { get; private set; }
		public PlotModel ClicksPerTimeIntervalChartModel { get; private set; }
		ObservableCollection<ClickStatChartData> ClicksPerTimeInterval { get; set; }
		ObservableCollection<ClickStatChartData> OpensPerTimeInterval { get; set; }
		ObservableCollection<ClickStatChartData> TotalClicks { get; set; }

		#endregion

		#region Load Data

		void LoadDataByContext()
		{
			var linkClickStatMap = StatModel.LinkClicks.Cast<ClickStatData>().ToDictionary(c => c.Link.PK);
			LoadData(linkClickStatMap, (x) => x.LinkPk);
		}

		void LoadDataByUrl()
		{
			var linkClickStatMap = StatModel.LinkClicks.Cast<ClickStatData>().ToDictionary(c => c.Link.GCL_URL);
			LoadData(linkClickStatMap, (x) =>
			{
				GlbCompanyCampaignLink link = StatModel.CampaignTrackedLinks.FindByPK(x.LinkPk) as GlbCompanyCampaignLink;
				if (link != null)
				{
					return link.GCL_URL;
				}
				return ZString.Empty;
			});
		}

		void LoadData()
		{
			if (StatModel.ReportBy == ReportByList.Codes.Context)
			{
				LoadDataByContext();
			}
			else
			{
				LoadDataByUrl();
			}
		}

		void LoadData<T>(Dictionary<T, ClickStatData> linkClickStatMap, Func<CampaignClicksPerInterval, T> keyGetter)
		{
			ClicksPerTimeInterval = new ObservableCollection<ClickStatChartData>();
			OpensPerTimeInterval = new ObservableCollection<ClickStatChartData>();
			TotalClicks = new ObservableCollection<ClickStatChartData>();

			var distinctIntervalIndexList = StatModel.ClicksPerIntervalData.Select(d => d.IntervalIndex).Distinct().ToList();
			var distinctReportByValueList = StatModel.LinkClicks.Cast<ClickStatData>().Select(c => c.ReportBy).Distinct().ToList();
			var clicksStatMap = new Dictionary<int, Dictionary<string, int>>(distinctIntervalIndexList.Count);
			var opensStatMap = new Dictionary<int, int>(distinctIntervalIndexList.Count);

			foreach (var clickPerIntervalData in StatModel.ClicksPerIntervalData)
			{
				Dictionary<string, int> reportByMap;
				if (!clicksStatMap.TryGetValue(clickPerIntervalData.IntervalIndex, out reportByMap))
				{
					clicksStatMap[clickPerIntervalData.IntervalIndex] = new Dictionary<string, int>(distinctReportByValueList.Count);
				}

				ClickStatData clickStatData;
				if (linkClickStatMap.TryGetValue(keyGetter(clickPerIntervalData), out clickStatData))
				{
					if (clickStatData.ViewInChart)
					{
						if (clicksStatMap[clickPerIntervalData.IntervalIndex].ContainsKey(clickStatData.ReportBy))
						{
							clicksStatMap[clickPerIntervalData.IntervalIndex][clickStatData.ReportBy] += clickPerIntervalData.ClickCount;
						}
						else
						{
							clicksStatMap[clickPerIntervalData.IntervalIndex][clickStatData.ReportBy] = clickPerIntervalData.ClickCount;
						}
					}
				}
				else if (clickPerIntervalData.LinkPk.IsEmpty)
				{
					if (opensStatMap.ContainsKey(clickPerIntervalData.IntervalIndex))
					{
						opensStatMap[clickPerIntervalData.IntervalIndex] += clickPerIntervalData.ClickCount;
					}
					else
					{
						opensStatMap[clickPerIntervalData.IntervalIndex] = clickPerIntervalData.ClickCount;
					}
				}
			}

			foreach (var reportBy in distinctReportByValueList)
			{
				int clickTotalCount = 0;
				for (int interval = 0; interval < StatModel.ClicksTimeIntervalCount; interval++)
				{
					int clickCount;
					if (clicksStatMap.ContainsKey(interval) && clicksStatMap[interval].TryGetValue(reportBy, out clickCount))
					{
						ClicksPerTimeInterval.Add(new ClickStatChartData() { Category = reportBy, Clicks = clickCount, IntervalIndex = interval });
						clickTotalCount += clickCount;
					}
					else
					{
						ClicksPerTimeInterval.Add(new ClickStatChartData() { Category = reportBy, Clicks = 0, IntervalIndex = interval });
					}
				}

				TotalClicks.Add(new ClickStatChartData() { Category = reportBy, Clicks = clickTotalCount });
			}

			for (int interval = 0; interval < StatModel.ClicksTimeIntervalCount; interval++)
			{
				int clickCount = opensStatMap.ContainsKey(interval) ? opensStatMap[interval] : 0;
				OpensPerTimeInterval.Add(new ClickStatChartData() { Clicks = clickCount, IntervalIndex = interval });
			}

			ClicksPerTimeInterval = new ObservableCollection<ClickStatChartData>(ClicksPerTimeInterval.OrderBy(d => d.IntervalIndex).ThenBy(d => d.Category));
			TotalClicks = new ObservableCollection<ClickStatChartData>(TotalClicks.OrderBy(d => d.Category));
		}

		#endregion

		#region Build Chart Model

		public void RefreshChartModel(bool reloadData = false)
		{
			if (reloadData)
			{
				StatModel.LoadClicks();
			}

			LoadData();

			TotalClicksChartModel.Axes.Clear();
			TotalClicksChartModel.Series.Clear();
			TotalClicksChartModel.Annotations.Clear();
			PopulateTotalClicksChartModel();
			TotalClicksChartModel.InvalidatePlot(true);

			ClicksPerTimeIntervalChartModel.Axes.Clear();
			ClicksPerTimeIntervalChartModel.Series.Clear();
			ClicksPerTimeIntervalChartModel.Annotations.Clear();
			PopulateClicksPerTimeIntervalModel();
			ClicksPerTimeIntervalChartModel.InvalidatePlot(true);
		}

		#region Total Clicks Chart Model

		void BuildTotalClicksChartModel()
		{
			TotalClicksChartModel = GetBasicPlotModel();
			PopulateTotalClicksChartModel();
		}

		void PopulateTotalClicksChartModel()
		{
			TotalClicksChartModel.Title = Res.GetString("f81bcee0-ef88-4f18-a35b-9c01723f9ec5", "Clicks");
			TotalClicksChartModel.PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1);
			var visibleLinkReportByMap = GetDistinctVisibleReportByValues().ToDictionary(reportBy => reportBy);
			int numOfTotalClicks = StatModel.ClicksPerIntervalData.Where(d => !d.LinkPk.IsEmpty).Sum(d => d.ClickCount);

			int maxSumClicks = 0;
			var clicksByCatgory = ClicksPerTimeInterval.Where(d => visibleLinkReportByMap.ContainsKey(d.Category)).GroupBy(c => c.Category);
			foreach (var group in clicksByCatgory)
			{
				var totalClicks = group.Sum(d => d.Clicks);
				if (totalClicks > maxSumClicks)
				{
					maxSumClicks = totalClicks;
				}
			}

			var clicksAxis = new LinearAxis();
			clicksAxis.Minimum = 0;
			clicksAxis.Maximum = maxSumClicks > 0 ? maxSumClicks * 1.2 : 10;
			clicksAxis.IsPanEnabled = false;
			clicksAxis.Position = AxisPosition.Left;
			clicksAxis.MajorTickSize = 0;
			clicksAxis.MinorTickSize = 0;
			clicksAxis.LabelFormatter = (value) => { return value == (int)value ? value.ToString() : ""; };
			TotalClicksChartModel.Axes.Add(clicksAxis);

			var categoryAxis = new CategoryAxis();
			categoryAxis.MinorStep = 0;
			categoryAxis.Position = AxisPosition.Bottom;
			categoryAxis.IsPanEnabled = false;
			categoryAxis.MajorTickSize = 0;
			categoryAxis.Angle = -45;
			categoryAxis.AxisTickToLabelDistance = 30;
			TotalClicksChartModel.Axes.Add(categoryAxis);

			int categoryIndex = 0;
			foreach (var clickData in TotalClicks.OrderByDescending(d => d.Clicks).ToArray())
			{
				if (visibleLinkReportByMap.ContainsKey(clickData.Category))
				{
					var percentage = (clickData.Clicks / (double)(numOfTotalClicks)) * 100;

					var columnSeries = new BarSeries();
					columnSeries.Title = clickData.Category;
					columnSeries.StrokeColor = ClickColor;
					columnSeries.FillColor = ClickColor;
					columnSeries.ToolTip = string.Format("{0} : {1} ({2:0.00}%)", clickData.Category, clickData.Clicks, percentage);
					columnSeries.LabelPlacement = LabelPlacement.Outside;
					columnSeries.TextColor = TextColor;

					var totalItem = new BarItem(clickData.Clicks, categoryIndex);
					columnSeries.Items.Add(totalItem);

					TotalClicksChartModel.Series.Add(columnSeries);

					var categoryLabel = clickData.Category.Length > 15 ? clickData.Category.Substring(0, 12) + "..." : clickData.Category;
					categoryAxis.ActualLabels.Add(categoryLabel);

					var pointAnnotation = new PointAnnotation();
					pointAnnotation.Size = 0;
					pointAnnotation.Fill = OxyColors.Transparent;
					pointAnnotation.Stroke = OxyColors.Transparent;
					pointAnnotation.X = categoryIndex;
					pointAnnotation.Y = clickData.Clicks;
					pointAnnotation.Layer = AnnotationLayer.AboveSeries;
					pointAnnotation.TextHorizontalAlignment = HorizontalAlignment.Center;
					pointAnnotation.TextVerticalAlignment = VerticalAlignment.Bottom;
					pointAnnotation.Text = string.Format("{0} ({1:0.00}%)", clickData.Clicks, percentage);
					TotalClicksChartModel.Annotations.Add(pointAnnotation);

					categoryIndex++;
				}
			}
		}

		#endregion

		#region Clicks Per Time Interval Chart Model

		void BuildClicksPerTimeIntervalChartModel()
		{
			ClicksPerTimeIntervalChartModel = GetBasicPlotModel();
			PopulateClicksPerTimeIntervalModel();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Name", Justification = " 'OxyPlot.Legends.Legend' is used as 'Legend' does not contain definiton of LegendPosition, LegendPlacement, LegendOrientation")]
		void PopulateClicksPerTimeIntervalModel()
		{
			ClicksPerTimeIntervalChartModel.PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1);

			if (!StatModel.IsCampaignLinkTrackOnly)
			{
				ClicksPerTimeIntervalChartModel.Title = Res.GetString("b8bcbe42-b771-4b98-991c-107afa3b553e", "Clicks relative to First Opened by Date Range");

				ClicksPerTimeIntervalChartModel.Legends.Add(new OxyPlot.Legends.Legend
				{
					LegendOrientation = LegendOrientation.Horizontal,
					LegendPosition = LegendPosition.TopCenter,
					LegendPlacement = LegendPlacement.Outside
				});

				ClicksPerTimeIntervalChartModel.IsLegendVisible = true;
			}
			else
			{
				ClicksPerTimeIntervalChartModel.Title = Res.GetString("30a2d986-aa05-4499-bf5f-0229521b84c0", "Clicks relative to First Opened by Date Range");
			}

			int maxSumClicks = OpensPerTimeInterval.Any() ? OpensPerTimeInterval.Max(d => d.Clicks) : 0;
			var visibleLinkReportByMap = GetDistinctVisibleReportByValues().ToDictionary(reportBy => reportBy);
			var clicksByInterval = ClicksPerTimeInterval.Where(d => visibleLinkReportByMap.ContainsKey(d.Category)).GroupBy(c => c.IntervalIndex);
			foreach (var group in clicksByInterval)
			{
				var totalClicks = group.Sum(d => d.Clicks);
				if (totalClicks > maxSumClicks)
				{
					maxSumClicks = totalClicks;
				}
			}

			#region Y Axis

			var clicksAxis = new CategoryAxis();
			clicksAxis.Position = AxisPosition.Left;
			clicksAxis.Maximum = maxSumClicks > 0 ? maxSumClicks * 1.2 : 10;
			clicksAxis.Minimum = 0;
			clicksAxis.MinimumPadding = 1;
			clicksAxis.IsPanEnabled = false;
			clicksAxis.IsZoomEnabled = false;
			clicksAxis.MajorTickSize = 0;
			clicksAxis.MinorTickSize = 0;
			clicksAxis.LabelFormatter = (value) => { return value == (int)value ? value.ToString() : ""; };
			ClicksPerTimeIntervalChartModel.Axes.Add(clicksAxis);

			#endregion

			#region X Axis

			var categoryAxis = new CategoryAxis();
			categoryAxis.GapWidth = 0.3;
			categoryAxis.MinorStep = 1;
			categoryAxis.MajorStep = 1;
			categoryAxis.Position = AxisPosition.Bottom;
			categoryAxis.IsPanEnabled = false;
			categoryAxis.IsZoomEnabled = false;
			categoryAxis.MajorTickSize = 0;

			ClicksPerTimeIntervalChartModel.Axes.Add(categoryAxis);
			for (int i = 0; i < StatModel.ClicksTimeIntervalCount; i++)
			{
				categoryAxis.ActualLabels.Add("");
			}

			var datetimeMainAxis = GetBasicTimeLineAxis();
			datetimeMainAxis.TickStyle = TickStyle.Crossing;
			datetimeMainAxis.MajorGridlineStyle = LineStyle.LongDash;
			datetimeMainAxis.MajorGridlineColor = GridlineColor;
			datetimeMainAxis.MajorGridlineThickness = 1;
			datetimeMainAxis.AxisTickToLabelDistance = 15;
			datetimeMainAxis.AxislineColor = BorderColor;
			datetimeMainAxis.AxisTitleDistance = 5;
			datetimeMainAxis.AxislineThickness = 1;
			datetimeMainAxis.AxislineStyle = LineStyle.Solid;
			datetimeMainAxis.Layer = AxisLayer.BelowSeries;
			ClicksPerTimeIntervalChartModel.Axes.Add(datetimeMainAxis);

			var datetimeSubAxis = GetBasicTimeLineAxis();
			datetimeSubAxis.MajorTickSize = 0;
			datetimeSubAxis.MinorTickSize = 0;
			datetimeSubAxis.PositionTier = 1;
			ClicksPerTimeIntervalChartModel.Axes.Add(datetimeSubAxis);

			if (StatModel.TimeSpanPerInterval.TotalDays < 1)
			{
				datetimeMainAxis.LabelFormatter = GetTimeLabel;
				datetimeSubAxis.LabelFormatter = GetDateLabel;
			}
			else
			{
				datetimeMainAxis.LabelFormatter = GetDateLabel;
				datetimeSubAxis.LabelFormatter = GetMonthYearLabel;
			}

			if (StatModel.IsReportTimeRangeStartsFromCampaignSent)
			{
				var timeSpanMainAxis = GetBasicTimeLineAxis();
				timeSpanMainAxis.PositionTier = 2;
				timeSpanMainAxis.TickStyle = TickStyle.Crossing;
				timeSpanMainAxis.AxislineColor = BorderColor;
				timeSpanMainAxis.AxislineStyle = LineStyle.Solid;
				timeSpanMainAxis.AxisDistance = 10;
				timeSpanMainAxis.LabelFormatter = GetTimeSpanEmptyLabel;
				timeSpanMainAxis.MajorStep = 4;
				timeSpanMainAxis.MajorTickSize = 8;
				ClicksPerTimeIntervalChartModel.Axes.Add(timeSpanMainAxis);

				var timeSpanSubAxis = GetBasicTimeLineAxis();
				timeSpanSubAxis.PositionTier = 2;
				timeSpanSubAxis.TickStyle = TickStyle.Outside;
				timeSpanSubAxis.AxislineColor = BorderColor;
				timeSpanSubAxis.AxislineStyle = LineStyle.Solid;
				timeSpanSubAxis.MajorStep = 2;
				timeSpanSubAxis.MinorStep = 1;
				timeSpanSubAxis.MajorTickSize = 5;
				timeSpanSubAxis.MinorTickSize = 3;
				timeSpanSubAxis.AxisDistance = 10;
				timeSpanSubAxis.LabelFormatter = GetTimeSpanEmptyLabel;
				ClicksPerTimeIntervalChartModel.Axes.Add(timeSpanSubAxis);

				var timeSpanLabelAxis = GetBasicTimeLineAxis();
				timeSpanLabelAxis.MajorTickSize = 0;
				timeSpanLabelAxis.LabelFormatter = GetTimeSpanLabel;
				timeSpanLabelAxis.PositionTier = 3;
				timeSpanLabelAxis.FontSize = 16;
				ClicksPerTimeIntervalChartModel.Axes.Add(timeSpanLabelAxis);
			}

			#endregion

			AddClicksPerTimeIntervalChartSeries();
		}

		void AddClicksPerTimeIntervalChartSeries(bool isHighlighting = false, string highlightCategory = "")
		{
			if (!StatModel.IsCampaignLinkTrackOnly)
			{
				var openedSeriesColor = !isHighlighting ? OpenedColor : OxyColor.FromAColor(60, OpenedColor);
				var openedSeries = new BarSeries();
				openedSeries.FillColor = openedSeriesColor;
				openedSeries.StrokeColor = openedSeriesColor;
				openedSeries.BarWidth = 0.3;
				openedSeries.Title = Res.GetString("20a9ea10-81b7-4cf8-997b-a9a5380e1786", "First Opened");
				if (!isHighlighting)
				{
					openedSeries.LabelPlacement = LabelPlacement.Outside;
					openedSeries.LabelFormatString = "{0:#}";
				}
				foreach (var clickData in OpensPerTimeInterval)
				{
					openedSeries.Items.Add(new BarItem(clickData.Clicks, clickData.IntervalIndex));
				}
				ClicksPerTimeIntervalChartModel.Series.Add(openedSeries);
			}

			var visibleLinkReportByMap = GetDistinctVisibleReportByValues().ToDictionary(reportBy => reportBy);

			if (isHighlighting && visibleLinkReportByMap.ContainsKey(highlightCategory))
			{
				var highlightSeries = new BarSeries();
				highlightSeries.IsStacked = true;
				highlightSeries.LabelFormatString = "{0}";
				highlightSeries.TextColor = OxyColors.White;
				highlightSeries.FontWeight = 1.2d;
				highlightSeries.LabelPlacement = LabelPlacement.Middle;
				highlightSeries.FillColor = ClickColor;

				var highlightClickData = ClicksPerTimeInterval.Where(d => d.Category == highlightCategory);
				foreach (var clickData in highlightClickData)
				{
					var item = new BarItem(clickData.Clicks, clickData.IntervalIndex);
					highlightSeries.Items.Add(item);
				}
				ClicksPerTimeIntervalChartModel.Series.Add(highlightSeries);
			}

			List<ClickStatChartData> clicksPerInterval = new List<ClickStatChartData>();
			var clicksByInterval = ClicksPerTimeInterval.Where(d => visibleLinkReportByMap.ContainsKey(d.Category)).GroupBy(c => c.IntervalIndex);
			foreach (var group in clicksByInterval)
			{
				var clicksCount = group.Where(d => d.Category != highlightCategory).Sum(d => d.Clicks);
				clicksPerInterval.Add(new ClickStatChartData() { Category = "", IntervalIndex = group.Key, Clicks = clicksCount });
			}

			var clicksSeriesColor = !isHighlighting ? ClickColor : OxyColor.FromAColor(60, OxyColors.LightGray);
			var clicksSeries = new BarSeries();
			clicksSeries.IsStacked = true;
			clicksSeries.Title = Res.GetString("81806929-6cb7-4b75-a39e-00d60d88340c", "Clicks");
			clicksSeries.FillColor = clicksSeriesColor;
			if (!isHighlighting)
			{
				clicksSeries.LabelPlacement = LabelPlacement.Outside;
				clicksSeries.LabelFormatString = "{0:#}";
			}
			foreach (var clickData in clicksPerInterval)
			{
				var totalItem = new BarItem(clickData.Clicks, clickData.IntervalIndex);
				clicksSeries.Items.Add(totalItem);
			}
			ClicksPerTimeIntervalChartModel.Series.Add(clicksSeries);
		}

		LinearAxis GetBasicTimeLineAxis()
		{
			LinearAxis axis = new LinearAxis();
			axis.Position = AxisPosition.Bottom;
			axis.Minimum = 0;
			axis.Maximum = StatModel.ClicksTimeIntervalCount > 0 ? StatModel.ClicksTimeIntervalCount : 10;
			axis.IsPanEnabled = false;
			axis.IsZoomEnabled = false;
			axis.TicklineColor = BorderColor;
			axis.MajorTickSize = 5;
			axis.MinorTickSize = 0;
			axis.MajorStep = 1;
			axis.MinorStep = 1;

			return axis;
		}

		#endregion

		#region Axis Label Formatter

		string GetTimeLabel(double index)
		{
			if (!StatModel.IsCampaignLinkTrackOnly || index % 2 == 0)
			{
				ZDateTime localTime = GetLocalTimeByIndex(index);

				if (localTime.IsValid)
				{
					return localTime.ToShortTimeString();
				}
			}
			return string.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customised date format string")]
		string GetDateLabel(double index)
		{
			if (!StatModel.IsCampaignLinkTrackOnly || index % 2 == 0)
			{
				ZDateTime localTime = GetLocalTimeByIndex(index);
				ZDateTime previousTime = GetLocalTimeByIndex(index - 1);

				if (localTime.IsValid && (index == 0 || previousTime.Day != localTime.Day))
				{
					ZString dateFormat;
					if (StatModel.TimeSpanPerInterval.TotalDays >= 1)
					{
						dateFormat = "dd";
					}
					else
					{
						dateFormat = ZDateTime.ShortDateFormat;
					}
					return localTime.ToString(dateFormat);
				}
			}
			return string.Empty;
		}

		string GetMonthYearLabel(double index)
		{
			if (!StatModel.IsCampaignLinkTrackOnly || index % 2 == 0)
			{
				ZDateTime localTime = GetLocalTimeByIndex(index);
				ZDateTime previousTime = GetLocalTimeByIndex(index - 1);

				if (localTime.IsValid && (index == 0 || previousTime.Month != localTime.Month))
				{
					ZString dateFormat = Res.GetString("ClickStatChartViewModel|MonthYearFormat", "MMM-yy");
					return localTime.ToString(dateFormat);
				}
			}
			return string.Empty;
		}

		ZDateTime GetLocalTimeByIndex(double index)
		{
			var intervalIndex = index - 1;
			ZDateTime localTime = ZDateTime.Empty;
			if (StatModel.ClicksPerIntervalData.Any())
			{
				var startTime = StatModel.ActualStartTime.IsValid ? StatModel.ActualStartTime : Env.Time.GetLocalTimeFromUtc(StatModel.ClicksPerIntervalData.Min(d => d.StartDateInclusive).ToDateTime());
				localTime = startTime.Add(TimeSpan.FromTicks(StatModel.TimeSpanPerInterval.Ticks * ((int)intervalIndex + 1)));
			}
			return localTime;
		}

		string GetTimeSpanLabel(double timeIndex)
		{
			var effectiveTimeIndex = timeIndex;

			if (effectiveTimeIndex == 0)
			{
				if (StatModel.ReportTimeRange == ReportTimeRangeList.Codes.SixHours || StatModel.ReportTimeRange == ReportTimeRangeList.Codes.TwentyFourHours)
				{
					return Res.GetString("dd150fd3-b1b2-45e1-a83a-726434f9b44a", "Hour");
				}
				else
				{
					return Res.GetString("2fb24bc4-559e-4d28-a564-cbb5ff555066", "Day");
				}
			}
			else if (effectiveTimeIndex % 4 == 0)
			{
				return GetTimeSpanLabel(effectiveTimeIndex, false);
			}
			return string.Empty;
		}

		string GetTimeSpanEmptyLabel(double timeIndex)
		{
			return string.Empty;
		}

		string GetTimeSpanLabel(double index, bool appendUnitLabel)
		{
			if (StatModel.ReportTimeRange == ReportTimeRangeList.Codes.SixHours || StatModel.ReportTimeRange == ReportTimeRangeList.Codes.SevenDays)
			{
				return (index / 4).ToString();
			}
			else
			{
				return index.ToString();
			}
		}

		#endregion

		PlotModel GetBasicPlotModel()
		{
			PlotModel model = new PlotModel();

			model.PlotAreaBorderColor = BorderColor;
			model.TextColor = TextColor;
			model.DefaultFontSize = 11;

			model.IsLegendVisible = false;

			model.TitleFontSize = 12;
			model.TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinView;
			model.TitlePadding = 5.0;

			return model;
		}

		IEnumerable<ZString> GetDistinctVisibleReportByValues()
		{
			return StatModel.LinkClicks.Cast<ClickStatData>()
							.Where(link => link.ViewInChart)
							.Select(link => link.ReportBy)
							.Distinct();
		}

		#endregion

		#region Colors

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant color code")]
		OxyColor BorderColor
		{
			get { return OxyColor.Parse("#FFCCCCCC"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant color code")]
		OxyColor TextColor
		{
			get { return OxyColor.Parse("#FF808080"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant color code")]
		OxyColor GridlineColor
		{
			get { return OxyColor.Parse("#FFF3F3F3"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant color code")]
		OxyColor ClickColor
		{
			get { return OxyColor.Parse("#DD019FCC"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant color code")]
		OxyColor OpenedColor
		{
			get { return OxyColor.Parse("#DD7CCC14"); }
		}

		#endregion

		#region Highlight

		public void HandleMouseDown(object sender, OxyMouseDownEventArgs e)
		{
			var series = sender as Series;
			if (series != null)
			{
				Highlight(series.Title, true);
			}
		}

		public void Highlight(string category, bool fromChart)
		{
			foreach (BarSeries columnSeries in TotalClicksChartModel.Series)
			{
				if (columnSeries.Title == category)
				{
					columnSeries.FillColor = ClickColor;
					columnSeries.StrokeColor = ClickColor;
				}
				else
				{
					var itemColor = OxyColor.FromAColor(60, OxyColors.LightGray);
					columnSeries.FillColor = itemColor;
					columnSeries.StrokeColor = itemColor;
				}
			}
			TotalClicksChartModel.InvalidatePlot(true);

			ClicksPerTimeIntervalChartModel.Series.Clear();
			AddClicksPerTimeIntervalChartSeries(true, category);
			ClicksPerTimeIntervalChartModel.InvalidatePlot(true);

			if (OnHighlightEventHandler != null)
			{
				OnHighlightEventHandler(this, new HighlightEventArgs(category, fromChart));
			}
		}

		public void UnHighlight()
		{
			foreach (BarSeries columnSeries in TotalClicksChartModel.Series)
			{
				columnSeries.FillColor = ClickColor;
				columnSeries.StrokeColor = ClickColor;
			}
			TotalClicksChartModel.InvalidatePlot(true);

			ClicksPerTimeIntervalChartModel.Series.Clear();
			AddClicksPerTimeIntervalChartSeries();
			ClicksPerTimeIntervalChartModel.InvalidatePlot(true);
		}

		public event EventHandler OnHighlight
		{
			add { OnHighlightEventHandler += value; }
			remove { OnHighlightEventHandler -= value; }
		}

		public class HighlightEventArgs : EventArgs
		{
			public HighlightEventArgs(string category, bool fromChart)
				: base()
			{
				Category = category;
				FromChart = fromChart;
			}

			public string Category { get; private set; }
			public bool FromChart { get; private set; }
		}

		EventHandler OnHighlightEventHandler;

		#endregion
	}
}
