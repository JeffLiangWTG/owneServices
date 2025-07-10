using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Enterprise.MarketingManager.Business;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using static Enterprise.MarketingManager.GUI.TransitionProgressViewModelDataAdapter;
using OxyPlotLegend = OxyPlot.Legends.Legend;

namespace Enterprise.MarketingManager.GUI
{
	public class TransitionProgressViewModel : ViewModelBase, IDisposable
	{
		#region Public

		public TransitionProgressViewModel(IEnumerable<TouchStats> touches)
		{
			ChartModel = new PlotModel();
			ChartModel.DefaultFontSize = 11;
			CreateChartController();
			RefreshPlotModel(touches);
		}

		public void RefreshPlotModel(IEnumerable<TouchStats> touches)
		{
			Touches = Array.AsReadOnly((touches?.ToArray() ?? Array.Empty<TouchStats>()));
			RefreshPlotModelCore();
		}

		public PlotModel ChartModel { get; private set; }
		public PlotController ChartController { get; private set; }
		public string TransitionedLegendText { get; } = TrackingSummaryConstants.Descriptions.TRN;
		public string NotTransitionedLegendText { get; } = TrackingSummaryConstants.Descriptions.NTR;

		#endregion Public

		#region Implement

		void CreateChartController()
		{
			ChartController = new PlotController();
			ChartController.UnbindAll();
			ChartController.BindMouseDown(OxyMouseButton.Left, new DelegatePlotCommand<OxyMouseDownEventArgs>(
				(view, controller, args) =>
				{
					if (ChartModel.Legends.Any(l => l.LegendArea.Contains(args.Position) && ChartModel.Legends.Any(le => le.IsLegendVisible)))
					{
						var pair = ChartModel.Legends.FirstOrDefault(p => p.LegendArea.Contains(args.Position));
						if (pair.Key != null)
						{
							Series_MouseDown(pair.Key, args);
							args.Handled = true;
							return;
						}
					}
					ResetAllAnnotations();
					args.Handled = true;
				}));
		}

		void RefreshPlotModelCore()
		{
			UnhookEvents();
			ChartModel.Series.Clear();
			ChartModel.Axes.Clear();
			ChartModel.Annotations.Clear();
			ChartModel.PlotAreaBorderThickness = new OxyThickness(0);

			ChartModel.IsLegendVisible = true;

			ChartModel.Legends.Add(new OxyPlotLegend
			{
				LegendPlacement = LegendPlacement.Outside
			});

			if (Touches.Count <= 1)
			{
				return;
			}

			CreateAxes();
			CreateBackgroundArea();
			CreateColumnSeries();
		}

		void CreateAxes()
		{
			var maxValues = new List<int>
			{
				Touches.Select(t => t.SentCategoryCount).Max(),
				Touches.Select(t => t.QueuedCategoryCount).Max(),
				Touches.Select(t => t.FailedCategoryCount).Max(),
				Touches.Select(t => t.OpportunityQueuedCount).Max(),
				Touches.Select(t => t.OpportunityCreatedCount).Max()
			};

			var maxY = maxValues.Max();

			if (maxY == 0)
			{
				maxY = 1;
			}

			//linear axes
			CategoryYAxis = new CategoryAxis();
			CategoryYAxis.MaximumPadding = CategoryYAxis.MinimumPadding = 0;
			CategoryYAxis.IsAxisVisible = true;
			CategoryYAxis.Key = CategoryYAxis.GetHashCode().ToString(CultureInfo.InvariantCulture);
			CategoryYAxis.Position = AxisPosition.Left;
			CategoryYAxis.PositionTier = 3;
			CategoryYAxis.IsPanEnabled = false;
			CategoryYAxis.IsZoomEnabled = false;
			CategoryYAxis.AxislineThickness = 1;
			CategoryYAxis.AxislineStyle = LineStyle.Solid;
			CategoryYAxis.AxislineColor = OxyColors.LightGray;
			CategoryYAxis.TicklineColor = OxyColors.LightGray;
			CategoryYAxis.TextColor = OxyColors.Gray;
			CategoryYAxis.AbsoluteMinimum = 0;
			CategoryYAxis.AbsoluteMaximum = maxY == 1 ? 1 : maxY + 1;

			for (int i = 0; i <= CategoryYAxis.AbsoluteMaximum; i++)
			{
				CategoryYAxis.Labels.Add(i.ToString());
			}

			ChartModel.Axes.Add(CategoryYAxis);

			LinearXAxis = new CategoryAxis();
			LinearXAxis.MaximumPadding = LinearXAxis.MinimumPadding = 0;
			LinearXAxis.IsAxisVisible = false;
			LinearXAxis.Key = LinearXAxis.GetHashCode().ToString(CultureInfo.InvariantCulture);
			LinearXAxis.Position = AxisPosition.Bottom;
			LinearXAxis.PositionTier = 2;
			LinearXAxis.IsPanEnabled = false;
			LinearXAxis.IsZoomEnabled = false;
			ChartModel.Axes.Add(LinearXAxis);

			//category axes
			TouchStatsCategoryAxis = new CategoryAxis();
			TouchStatsCategoryAxis.MaximumPadding = TouchStatsCategoryAxis.MinimumPadding = 0;
			TouchStatsCategoryAxis.MinorStep = 1;
			TouchStatsCategoryAxis.MajorStep = 1;
			TouchStatsCategoryAxis.Position = AxisPosition.Bottom;
			TouchStatsCategoryAxis.IsPanEnabled = false;
			TouchStatsCategoryAxis.IsZoomEnabled = false;
			TouchStatsCategoryAxis.MajorTickSize = 0;
			TouchStatsCategoryAxis.PositionTier = 0;
			TouchStatsCategoryAxis.AxislineThickness = 0.5;
			TouchStatsCategoryAxis.TickStyle = TickStyle.Crossing;
			TouchStatsCategoryAxis.AxislineStyle = LineStyle.Solid;
			TouchStatsCategoryAxis.AxislineColor = OxyColors.LightGray;
			TouchStatsCategoryAxis.TicklineColor = OxyColors.LightGray;
			TouchStatsCategoryAxis.TextColor = OxyColors.Gray;
			TouchStatsCategoryAxis.Key = TouchStatsCategoryAxis.GetHashCode().ToString(CultureInfo.InvariantCulture);
			TouchStatsCategoryAxis.Title = " ";
			TouchStatsCategoryAxis.TitleFontSize = 1;
			TouchStatsCategoryAxis.AxisTitleDistance = 10;

			ChartModel.Axes.Add(TouchStatsCategoryAxis);

			var touchesCount = Touches.Count;
			var axeLabels = new List<string>
			{
				touchesCount > 7 ? TrackingSummaryConstants.Codes.SNT : TrackingSummaryConstants.Descriptions.SNT,
				touchesCount > 7 ? TrackingStatusCodes.Codes.QUE : TrackingStatusCodes.Descriptions.QUE,
				touchesCount > 7 ? TrackingSummaryConstants.Codes.FAI : TrackingSummaryConstants.Descriptions.FAI,
				touchesCount > 4 ? TrackingStatusCodes.Codes.OPQ : TrackingStatusCodes.Descriptions.OPQ,
				touchesCount > 4 ? TrackingStatusCodes.Codes.OPC : TrackingStatusCodes.Descriptions.OPC
			};

			for (var index = 1; index < Touches.Count; index++)
			{
				foreach (var lb in axeLabels)
				{
					TouchStatsCategoryAxis.ActualLabels.Add(lb);
				}
			}

			TouchNameLabelCategoryAxis = new TouchNameCategoryAxis();
			TouchNameLabelCategoryAxis.MaximumPadding = TouchNameLabelCategoryAxis.MinimumPadding = 0;
			TouchNameLabelCategoryAxis.MinorStep = 1;
			TouchNameLabelCategoryAxis.MajorStep = 1;
			TouchNameLabelCategoryAxis.Position = AxisPosition.Bottom;
			TouchNameLabelCategoryAxis.IsPanEnabled = false;
			TouchNameLabelCategoryAxis.IsZoomEnabled = false;
			TouchNameLabelCategoryAxis.MajorTickSize = 3;
			TouchNameLabelCategoryAxis.PositionTier = 1;
			TouchNameLabelCategoryAxis.AxislineThickness = 0.5;
			TouchNameLabelCategoryAxis.TickStyle = TickStyle.Crossing;
			TouchNameLabelCategoryAxis.AxislineStyle = LineStyle.Solid;
			TouchNameLabelCategoryAxis.AxislineColor = OxyColors.LightGray;
			TouchNameLabelCategoryAxis.TicklineColor = OxyColors.LightGray;
			TouchNameLabelCategoryAxis.MinorTickSize = 0;

			TouchNameLabelCategoryAxis.ActualLabels.Add(Touches[0].TouchName); //master list
			ChartModel.Axes.Add(TouchNameLabelCategoryAxis);
			foreach (var touch in Touches.Skip(1))
			{
				TouchNameLabelCategoryAxis.ActualLabels.Add(string.Empty);
				TouchNameLabelCategoryAxis.ActualLabels.Add(touch.TouchName);
				TouchNameLabelCategoryAxis.ActualLabels.Add(string.Empty);
			}

			//master list X offset
			LinearXAxis.Minimum = MasterListXAxisOffset;
			TouchStatsCategoryAxis.Minimum = MasterListXAxisOffset * 6.5;
			TouchNameLabelCategoryAxis.Minimum = MasterListXAxisOffset * 1.5;
		}

		void CreateBackgroundArea()
		{
			//master list area
			var masterListY = Touches[0].SentCategoryCount;
			var masterListArea = new AreaSeries();
			masterListArea.XAxisKey = LinearXAxis.Key;
			masterListArea.Color = TransactionedAreaColor;
			masterListArea.Fill = TransactionedAreaColor;
			masterListArea.Points.Add(new DataPoint(MasterListXAxisOffset / 4.0, 0));
			masterListArea.Points.Add(new DataPoint(MasterListXAxisOffset / 4.0, masterListY));
			masterListArea.Points.Add(new DataPoint(MasterListXAxisOffset / 4.0 * 3.0, masterListY));
			masterListArea.Points.Add(new DataPoint(MasterListXAxisOffset / 4.0 * 3.0, 0));
			ChartModel.Series.Add(masterListArea);

			var masterListAnnotation = new PointAnnotation();
			masterListAnnotation.XAxisKey = LinearXAxis.Key;
			masterListAnnotation.X = MasterListXAxisOffset / 4.0 * 2.0;
			masterListAnnotation.Y = masterListY;
			masterListAnnotation.Text = masterListY.ToString(CultureInfo.InvariantCulture);
			masterListAnnotation.Shape = MarkerType.None;
			masterListAnnotation.TextVerticalAlignment = VerticalAlignment.Bottom;
			masterListAnnotation.TextMargin = -6;
			masterListAnnotation.TextColor = TransactionedAreaTextColor;
			ChartModel.Annotations.Add(masterListAnnotation);

			var maxYValue = Touches.Select(x => x.TransitionedToNextTouchCount).Concat(new int[] { masterListY }).Max() * 1.15 + 1;

			//background area
			var areaSeries1 = new AreaSeries();
			areaSeries1.XAxisKey = LinearXAxis.Key;
			areaSeries1.Color = TransactionedAreaColor;
			areaSeries1.Fill = TransactionedAreaColor;
			areaSeries1.Points.Add(new DataPoint(0, 0));

			for (var idx = 0; idx < Touches.Count; idx++)
			{
				var touch = Touches[idx];
				areaSeries1.Points.Add(new DataPoint(idx, touch.TransitionedToNextTouchCount));
			}
			areaSeries1.Points.Add(new DataPoint(Touches.Count - 1, 0));
			ChartModel.Series.Add(areaSeries1);

			//vertical lines
			for (var idx = 0; idx < Touches.Count; idx++)
			{
				var outerLines = new LineSeries();
				outerLines.XAxisKey = LinearXAxis.Key;
				outerLines.Points.Add(new DataPoint(idx, 0));
				outerLines.Points.Add(new DataPoint(idx, maxYValue));
				outerLines.StrokeThickness = 5;
				outerLines.Color = OxyColors.White;
				ChartModel.Series.Add(outerLines);

				var innerLines = new LineSeries();
				innerLines.XAxisKey = LinearXAxis.Key;
				innerLines.Points.Add(new DataPoint(idx, 0));
				innerLines.Points.Add(new DataPoint(idx, maxYValue));
				innerLines.StrokeThickness = 1;
				innerLines.Color = OxyColors.LightGray;
				ChartModel.Series.Add(innerLines);
			}

			//point annotation
			for (var idx = 0; idx < Touches.Count; idx++)
			{
				var touch = Touches[idx];
				var annotation = new ColumnSeriesAnnotation(touch.TransitionedToNextTouchCount.ToString(CultureInfo.InvariantCulture),
					idx, touch.TransitionedToNextTouchCount);
				annotation.XAxisKey = LinearXAxis.Key;
				annotation.YAxisKey = CategoryYAxis.Key;
				annotation.TextColor = TransactionedAreaTextColor;

				ChartModel.Annotations.Add(annotation);
			}
		}

		void CreateColumnSeries()
		{
			var barSeries = new BarSeries { XAxisKey = CategoryYAxis.Key, YAxisKey = TouchStatsCategoryAxis.Key };
			var touchIndex = 0;

			foreach (var touch in Touches.Skip(1))
			{
				//sent
				var totalSent = touch.SentCategoryCount;
				barSeries.Items.Add(new BarItem(totalSent)
				{
					Color = ColorCollection.GetOxyColor(TrackingStatusCodes.Codes.NDR)
				});
				var annotationSent = new ColumnSeriesAnnotation(totalSent.ToString(CultureInfo.InvariantCulture),
					touchIndex + 0.1, totalSent)
				{ XAxisKey = LinearXAxis.Key, YAxisKey = CategoryYAxis.Key };
				ChartModel.Annotations.Add(annotationSent);

				//queue
				var totalQueued = touch.QueuedCategoryCount;
				barSeries.Items.Add(new BarItem(totalQueued)
				{
					Color = ColorCollection.GetOxyColor(TrackingStatusCodes.Codes.QUE)
				});
				var annotationQueued = new ColumnSeriesAnnotation(totalQueued.ToString(CultureInfo.InvariantCulture),
					touchIndex + 0.3, totalQueued)
				{ XAxisKey = LinearXAxis.Key, YAxisKey = CategoryYAxis.Key };
				ChartModel.Annotations.Add(annotationQueued);

				//failed
				var totalFailed = touch.FailedCategoryCount;
				barSeries.Items.Add(new BarItem(totalFailed)
				{
					Color = ColorCollection.GetOxyColor(TrackingSummaryConstants.Codes.UNS)
				});
				var annotationFailed = new ColumnSeriesAnnotation(totalFailed.ToString(CultureInfo.InvariantCulture),
					touchIndex + 0.5, totalFailed)
				{ XAxisKey = LinearXAxis.Key, YAxisKey = CategoryYAxis.Key };
				ChartModel.Annotations.Add(annotationFailed);

				//opportunityQueued
				var totalOppQueuedCount = touch.OpportunityQueuedCount;
				barSeries.Items.Add(new BarItem(totalOppQueuedCount)
				{
					Color = ColorCollection.GetOxyColor(TrackingStatusCodes.Codes.OPQ)
				});
				var annotationOppQueuedCount =
					new ColumnSeriesAnnotation(totalOppQueuedCount.ToString(CultureInfo.InvariantCulture),
						touchIndex + 0.7, totalOppQueuedCount)
					{
						XAxisKey = LinearXAxis.Key,
						YAxisKey = CategoryYAxis.Key
					};
				ChartModel.Annotations.Add(annotationOppQueuedCount);

				//opportunityCreated
				var totalOppCreatedCount = touch.OpportunityCreatedCount;
				barSeries.Items.Add(new BarItem(totalOppCreatedCount)
				{
					Color = ColorCollection.GetOxyColor(TrackingStatusCodes.Codes.OPC)
				});
				var annotationOppCreatedCount =
					new ColumnSeriesAnnotation(totalOppCreatedCount.ToString(CultureInfo.InvariantCulture),
						touchIndex + 0.9, totalOppCreatedCount)
					{
						XAxisKey = LinearXAxis.Key,
						YAxisKey = CategoryYAxis.Key
					};
				ChartModel.Annotations.Add(annotationOppCreatedCount);

				touchIndex++;
			}
			ChartModel.Series.Add(barSeries);
		}

		#region Event Handling

		void ResetAllAnnotations()
		{
			foreach (var columnSeries in ChartModel.Series.OfType<ClickableColumnSeries>())
			{
				columnSeries.Reset();
			}

			foreach (var annotation in ChartModel.Annotations.OfType<ColumnSeriesAnnotation>())
			{
				annotation.Reset();
			}

			ChartModel.InvalidatePlot(true);
		}

		void Highlight(int groupIndex)
		{
			foreach (var columnSeries in ChartModel.Series.OfType<ClickableColumnSeries>())
			{
				columnSeries.ClearAnnotationText();
			}

			Array.ForEach(ChartModel.Series.OfType<ClickableColumnSeries>().Where(x => x.GroupIndex != groupIndex).ToArray(), (x) => x.Unhighlight());
			Array.ForEach(ChartModel.Series.OfType<ClickableColumnSeries>().Where(x => x.GroupIndex == groupIndex).ToArray(), (x) => x.Highlight());

			ChartModel.InvalidatePlot(true);
		}

		void Series_MouseDown(object sender, OxyMouseDownEventArgs e)
		{
			var series = sender as ClickableColumnSeries;
			if (series != null)
			{
				Highlight(series.GroupIndex);
				e.Handled = true;
			}
		}

		void UnhookEvents()
		{
			if (ChartModel != null)
			{
				ChartController.UnbindMouseDown(OxyMouseButton.Left);
			}
		}

		//CodeAnalysisRules
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//CodeAnalysisRules
		protected virtual void Dispose(bool disposing)
		{
			UnhookEvents();
			ChartModel = null;
		}

		#endregion Event Handling

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant color code")]
		OxyColor TransactionedAreaColor { get; } = OxyColor.Parse("#FFF1F1F1");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant color code")]
		OxyColor TransactionedAreaTextColor { get; } = OxyColor.Parse("#FF8D8D8D");

		ReadOnlyCollection<TouchStats> Touches;
		CategoryAxis CategoryYAxis;
		LinearAxis LinearXAxis;
		internal CategoryAxis TouchStatsCategoryAxis;
		TouchNameCategoryAxis TouchNameLabelCategoryAxis;
		const double MasterListXAxisOffset = -1.0 / 3.0;

		#region Inner Classes

		class ClickableColumnSeries : BarSeries
		{
			public ClickableColumnSeries(string statusCode,
										int seriesValue,
										PointAnnotation targetAnnotation,
										double highlightAnnotationY) : base()
			{
				var originalColor = ColorCollection.GetOxyColor(statusCode);
				FillColor = originalColor;
				OriginalColor = originalColor;
				TargetAnnotation = targetAnnotation;
				IsStacked = true;
				SeriesValue = seriesValue;
				HighlightAnnotationY = highlightAnnotationY;
			}

			readonly OxyColor OriginalColor;
			readonly PointAnnotation TargetAnnotation;

			readonly int SeriesValue;
			readonly double HighlightAnnotationY;

			public void Highlight()
			{
				FillColor = OriginalColor;
				if (TargetAnnotation != null)
				{
					TargetAnnotation.Text = SeriesValue.ToString(CultureInfo.InvariantCulture);
					TargetAnnotation.Y = HighlightAnnotationY;
				}
				LabelPlacement = LabelPlacement.Base;
				Highlighted?.Invoke(this, null);
			}

			public void Unhighlight()
			{
				FillColor = GetLighterColor(OriginalColor);
			}

			public void Reset()
			{
				FillColor = OriginalColor;
				LabelFormatString = "";
			}

			public void ClearAnnotationText()
			{
				if (TargetAnnotation != null)
				{
					TargetAnnotation.Text = "";
				}
			}

			public int GroupIndex { get; set; }

			public event EventHandler Highlighted;

			OxyColor GetLighterColor(OxyColor color)
			{
#if !WINZOR
				var sysColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
				var lightColor = System.Windows.Forms.ControlPaint.LightLight(sysColor);
				return OxyColor.FromArgb(lightColor.A, lightColor.R, lightColor.G, lightColor.B);
#else
				return default(OxyColor);
#endif
			}
		}

		class ColumnSeriesAnnotation : PointAnnotation
		{
			public ColumnSeriesAnnotation(string text, double x, double y) : base()
			{
				OriginalText = text;
				OriginalX = x;
				OriginalY = y;
				Text = text;
				X = x;
				Y = y;
				TextVerticalAlignment = VerticalAlignment.Bottom;
				TextHorizontalAlignment = HorizontalAlignment.Center;
				Shape = MarkerType.None;
				TextMargin = -6;
			}

			public void Reset()
			{
				base.Text = OriginalText;
				X = OriginalX;
				Y = OriginalY;
			}

			readonly string OriginalText;
			readonly double OriginalX;
			readonly double OriginalY;
		}

		class TouchNameCategoryAxis : CategoryAxis
		{
			public override void GetTickValues(out IList<double> majorLabelValues, out IList<double> majorTickValues, out IList<double> minorTickValues)
			{
				base.GetTickValues(out majorLabelValues, out majorTickValues, out minorTickValues);

				//remove redundant tick marks.
				majorLabelValues = majorLabelValues.Skip(2).Select((x, i) => new { value = x, index = i }).Where(x => x.index % 3 == 0)
					.Select(x => x.value).Concat(majorLabelValues.Take(2)).ToList();
				majorTickValues = majorTickValues.Skip(1).Select((x, i) => new { value = x, index = i }).Where(x => x.index % 3 == 0)
					.Select(x => x.value).Concat(majorTickValues.Take(1)).ToList();
			}
		}

		#endregion Inner Classes

		#endregion Implement
	}
}
