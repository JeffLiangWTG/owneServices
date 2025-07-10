using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace Enterprise.MarketingManager.GUI
{
	public class SalesAnalysisChartViewModel
	{
		public SalesAnalysisChartViewModel(SalesBreakdown salesBreakdown)
		{
			this.salesBreakdown = salesBreakdown;
			SalesTimeLineChartModel = GetDefaultPlotModel();
			Populate();
			CreatePlotController();
		}
		readonly SalesBreakdown salesBreakdown;

		#region Properties

		public PlotModel SalesTimeLineChartModel { get; private set; }
		public PlotController SalesTimeLineChartController { get; private set; }
		public Collection<SalesAnalysisChartData> SalesAnalysisChartDataList { get; private set; }

		BusinessObjectFactory Factory
		{
			get { return salesBreakdown.Org.Factory; }
		}

		const int PeriodStartIndex = -12;
		const int PeriodEndIndex = 2;

		#endregion

		public void RefreshChartModel()
		{
			SalesTimeLineChartModel.Axes.Clear();
			SalesTimeLineChartModel.Series.Clear();
			Populate();
			SalesTimeLineChartModel.InvalidatePlot(true);
		}

		void CreatePlotController()
		{
			SalesTimeLineChartController = new PlotController();
			SalesTimeLineChartController.BindMouseDown(OxyMouseButton.Left, new DelegatePlotCommand<OxyMouseDownEventArgs>(
				(view, controller, args) =>
				{
					if (SalesTimeLineChartModel.Legends.Any(l => l.LegendArea.Contains(args.Position) && SalesTimeLineChartModel.IsLegendVisible))
					{
						var pair = SalesTimeLineChartModel.Legends.FirstOrDefault(p => p.IsLegendVisible && p.LegendArea.Contains(args.Position));
						if (pair != null)
						{
							args.Handled = true;
						}
					}

					if (!args.Handled)
					{
						ResetHighlightStatus();
					}
					args.Handled = true;
				}));
		}

		void Populate()
		{
			var today = ZDateTime.Today;
			var firstDayThisMonth = new ZDateTime(today.Year, today.Month, 1);

			PopulateChartData(firstDayThisMonth);
			BuildChartModel(firstDayThisMonth);
		}

		void PopulateChartData(ZDateTime firstDayThisMonth)
		{
			SalesAnalysisChartDataList = new Collection<SalesAnalysisChartData>();

			var tradedRevenueMap = GetTradedRevenueMapByPeriod(firstDayThisMonth);
			var committedRevenueMap = GetCommittedEstimatedRevenueMapByPeriod(firstDayThisMonth);
			var forecastRevenueMap = GetForecastEstimatedRevenueMapByPeriod(firstDayThisMonth);

			for (int index = PeriodStartIndex; index <= PeriodEndIndex; index++)
			{
				var currentPeriod = firstDayThisMonth.AddMonths(index);
				var chartData = new SalesAnalysisChartData()
				{
					Index = index
				};

				if (tradedRevenueMap.Contains(new Tuple<ZDateTime, bool>(currentPeriod, false)))
				{
					var orgValues = tradedRevenueMap[new Tuple<ZDateTime, bool>(currentPeriod, false)];
					chartData.ActualOrgRevenue = RevenueCalculator.GetTotalInEntityCurrency(orgValues, x => x.VPT_RX_NKCurrency, x => x.VPT_Revenue);
				}

				if (tradedRevenueMap.Contains(new Tuple<ZDateTime, bool>(currentPeriod, true)))
				{
					var jobValues = tradedRevenueMap[new Tuple<ZDateTime, bool>(currentPeriod, true)];
					chartData.ActualJobRevenue = RevenueCalculator.GetTotalInEntityCurrency(jobValues, x => x.VPT_RX_NKCurrency, x => x.VPT_Revenue);
				}

				if (committedRevenueMap.Contains(currentPeriod))
				{
					var committedPeriods = committedRevenueMap[currentPeriod];
					chartData.CommittedRevenue = RevenueCalculator.GetTotalInEntityCurrency(committedPeriods, x => x.PAS_RX_NKCurrency, x => x.PAS_EstimatedProfit);
					chartData.TotalEstimatedRevenue += chartData.CommittedRevenue;
				}

				if (forecastRevenueMap.Contains(currentPeriod))
				{
					var forecastPeriods = forecastRevenueMap[currentPeriod];
					chartData.ForecastRevenue = RevenueCalculator.GetTotalInEntityCurrency(forecastPeriods, x => x.PAS_RX_NKCurrency, x => x.PAS_EstimatedProfit);
					chartData.TotalEstimatedRevenue += chartData.ForecastRevenue;
				}

				SalesAnalysisChartDataList.Add(chartData);
			}
		}

		ILookup<Tuple<ZDateTime, bool>, OrgPeriodTradedValue> GetTradedRevenueMapByPeriod(ZDateTime firstDayThisMonth)
		{
			var reportingPeriodStart = firstDayThisMonth.AddMonths(PeriodStartIndex);
			var reportingPeriodEnd = firstDayThisMonth.AddMonths(PeriodEndIndex);

			var context = new SalesLoader.LoadingContext()
			{
				OrgPk = salesBreakdown.Org.PK,
				PeriodFrom = reportingPeriodStart,
				PeriodTo = reportingPeriodEnd,
				CompanyPk = Env.CurrentCompany.PK,
				SalesProduct = salesBreakdown.SelectedProduct
			};
			var tradedValues = SalesLoader.LoadTradedValuesByProduct(Factory, context);
			var revenueMap = tradedValues.ToLookup(x => new Tuple<ZDateTime, bool>(x.VPT_Period.ToZDateTime(), x.VPT_IsJobValue));
			return revenueMap;
		}

		ILookup<ZDateTime, OrgTradePeriod> GetCommittedEstimatedRevenueMapByPeriod(ZDateTime firstDayThisMonth)
		{
			var reportingPeriodStart = firstDayThisMonth.AddMonths(PeriodStartIndex);
			var reportingPeriodEnd = firstDayThisMonth.AddMonths(PeriodEndIndex);

			var context = new SalesLoader.LoadingContext()
			{
				OrgPk = salesBreakdown.Org.PK,
				PeriodFrom = reportingPeriodStart,
				PeriodTo = reportingPeriodEnd,
				IsPeriodToInclusive = true,
				IsTraded = false,
				IsForecast = false,
				TradeStatus = OpportunityTradeStatus.Codes.Successful,
				CompanyPk = salesBreakdown.CompanyFilter,
				SalesProduct = salesBreakdown.SelectedProduct,
				LoadByOption = SalesLoader.LoadingContext.LoadBy.Product
			};
			var prospectPeriods = SalesLoader.LoadPeriods(Factory, context);
			var revenueMap = prospectPeriods.ToLookup(x => x.PAS_Period.ToZDateTime());
			return revenueMap;
		}

		ILookup<ZDateTime, OrgTradePeriod> GetForecastEstimatedRevenueMapByPeriod(ZDateTime firstDayThisMonth)
		{
			var reportingPeriodStart = firstDayThisMonth.AddMonths(PeriodStartIndex);
			var reportingPeriodEnd = firstDayThisMonth.AddMonths(PeriodEndIndex);

			var context = new SalesLoader.LoadingContext()
			{
				OrgPk = salesBreakdown.Org.PK,
				PeriodFrom = reportingPeriodStart,
				PeriodTo = reportingPeriodEnd,
				IsPeriodToInclusive = true,
				IsTraded = false,
				IsForecast = true,
				TradeStatus = OpportunityTradeStatus.Codes.Successful,
				CompanyPk = salesBreakdown.CompanyFilter,
				SalesProduct = salesBreakdown.SelectedProduct,
				LoadByOption = SalesLoader.LoadingContext.LoadBy.Product
			};
			var prospectPeriods = SalesLoader.LoadPeriods(Factory, context);
			var revenueMap = prospectPeriods.ToLookup(x => x.PAS_Period.ToZDateTime());
			return revenueMap;
		}

		void BuildChartModel(ZDateTime firstDayThisMonth)
		{
			decimal maxValue = SalesAnalysisChartDataList.Max(x => new decimal[] { x.ActualJobRevenue, x.ActualOrgRevenue, x.CommittedRevenue, x.TotalEstimatedRevenue }.Max());

			var revenueAxis = GetDefaultLinearAxis();
			revenueAxis.Position = AxisPosition.Left;
			revenueAxis.Minimum = 0;
			revenueAxis.Maximum = maxValue > 0 ? Convert.ToDouble(maxValue) * 1.1 : 10;
			revenueAxis.MajorStep = GetRevenueAxisMajorStep(maxValue);
			revenueAxis.MajorGridlineStyle = LineStyle.Solid;
			revenueAxis.Title = RevenueCalculator.EntityCurrencyCode;
			revenueAxis.LabelFormatter = (value) => { return value.ToString("C0", SalesTimeLineChartModel.Culture); };
			SalesTimeLineChartModel.Axes.Add(revenueAxis);

			var periodAxis = GetDefaultLinearAxis();
			periodAxis.Position = AxisPosition.Bottom;
			periodAxis.Minimum = PeriodStartIndex - 0.5;
			periodAxis.Maximum = PeriodEndIndex + 0.5;
			periodAxis.MajorStep = 1;
			periodAxis.MinorStep = 1;
			periodAxis.AxisTickToLabelDistance = 10;
			periodAxis.LabelFormatter = (value) =>
			{
				var dateFormat = Res.GetString("SalesAnalysisChartViewModel|MonthYearFormat", "MMM");
				return value >= PeriodStartIndex && value <= PeriodEndIndex ? firstDayThisMonth.AddMonths(Convert.ToInt32(value)).ToDateTime().ToString(dateFormat, SalesTimeLineChartModel.Culture).ToUpper(SalesTimeLineChartModel.Culture) : "";
			};
			SalesTimeLineChartModel.Axes.Add(periodAxis);

			if (SalesAnalysisChartDataList.Any(x => x.TotalEstimatedRevenue > 0))
			{
				var estimatedRevenueSeries = new ClickableLineSeries(TotalEstimatedRevenueColor)
				{
					Title = CommittedAndForecastValueSeriesTitle
				};
				foreach (var data in SalesAnalysisChartDataList)
				{
					var point = new DataPoint(data.Index, Convert.ToDouble(data.TotalEstimatedRevenue));
					estimatedRevenueSeries.Points.Add(point);
				}
				SalesTimeLineChartModel.Series.Add(estimatedRevenueSeries);
			}

			if (SalesAnalysisChartDataList.Any(x => x.CommittedRevenue > 0))
			{
				var committedRevenueSeries = new ClickableLineSeries(CommittedRevenueColor)
				{
					Title = CommittedValueSeriesTitle
				};
				foreach (var data in SalesAnalysisChartDataList)
				{
					var point = new DataPoint(data.Index, Convert.ToDouble(data.CommittedRevenue));
					committedRevenueSeries.Points.Add(point);
				}
				SalesTimeLineChartModel.Series.Add(committedRevenueSeries);
			}

			if (SalesAnalysisChartDataList.Any(x => x.ActualJobRevenue > 0))
			{
				var actualJobRevenueSeries = new ClickableLineSeries(ActualJobRevenueColor)
				{
					Title = JobRevenueSeriesTitle
				};
				foreach (var data in SalesAnalysisChartDataList)
				{
					var point = new DataPoint(data.Index, Convert.ToDouble(data.ActualJobRevenue));
					actualJobRevenueSeries.Points.Add(point);
				}
				SalesTimeLineChartModel.Series.Add(actualJobRevenueSeries);
			}

			var actualOrgRevenueSeries = new ClickableLineSeries(ActualOrgRevenueColor)
			{
				LabelFormatString = "{1:C0}",
				Title = RevenueSeriesTitle
			};
			foreach (var data in SalesAnalysisChartDataList)
			{
				var point = new DataPoint(data.Index, Convert.ToDouble(data.ActualOrgRevenue));
				actualOrgRevenueSeries.Points.Add(point);
			}
			SalesTimeLineChartModel.Series.Add(actualOrgRevenueSeries);

			SalesTimeLineChartModel.Annotations.Add(new LineAnnotation { Type = LineAnnotationType.Vertical, X = 0, MaximumY = revenueAxis.Maximum, Color = OxyColors.LightGray });
		}

		OrgSalesRevenueCalculator RevenueCalculator
		{
			get { return revenueCalculator ?? (revenueCalculator = new OrgSalesRevenueCalculator(Factory, salesBreakdown.Org)); }
		}
		OrgSalesRevenueCalculator revenueCalculator;

		static double GetRevenueAxisMajorStep(decimal maxRevenue)
		{
			decimal majorStep = 10m;
			const int numberOfMajorSteps = 5;

			var maxRevenueBreakValues = new List<decimal>(10);
			decimal initBreakValue = 50m;
			for (int i = 10; i >= 1; i--)
			{
				maxRevenueBreakValues.Add(initBreakValue * (long)Math.Pow(10, i));
			}

			foreach (var breakValue in maxRevenueBreakValues)
			{
				if (maxRevenue > breakValue)
				{
					majorStep = Utilities.Round(maxRevenue / breakValue, 0) * (breakValue / numberOfMajorSteps);
					break;
				}
			}
			return Convert.ToDouble(majorStep);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Name", Justification = " 'OxyPlot.Legends.Legend' is used as 'Legend' does not contain definiton of LegendPosition, LegendPlacement, LegendOrientation")]
		PlotModel GetDefaultPlotModel()
		{
			var model = new PlotModel();

			model.PlotAreaBorderColor = BorderColor;
			model.PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1);
			model.TextColor = TextColor;
			model.DefaultFontSize = 11;

			model.IsLegendVisible = false;

			model.TitleFontSize = 12;
			model.TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinView;
			model.TitlePadding = 5.0;

			model.Legends.Add(new OxyPlot.Legends.Legend
			{
				LegendOrientation = LegendOrientation.Horizontal,
				LegendPosition = LegendPosition.TopCenter,
				LegendPlacement = LegendPlacement.Outside
			});

			model.IsLegendVisible = true;

			var staffCulture = Culture.GetCultureForLanguage(GlbStaff.CurrentUser.GS_WorkingLanguage);
			if (staffCulture != null)
			{
				staffCulture.NumberFormat.CurrencySymbol = RevenueCalculator.EntityCurrency.RX_Symbol;
				model.Culture = staffCulture;
			}

			return model;
		}

		LinearAxis GetDefaultLinearAxis()
		{
			var linearAxis = new LinearAxis
			{
				IsPanEnabled = false,
				IsZoomEnabled = false,
				TicklineColor = BorderColor,
				MajorTickSize = 0,
				MinorTickSize = 0
			};

			return linearAxis;
		}

		#region Highlight

		void ResetHighlightStatus()
		{
			foreach (var lineSeries in SalesTimeLineChartModel.Series.OfType<ClickableLineSeries>())
			{
				lineSeries.Reset();
				if (lineSeries.Title == RevenueSeriesTitle)
				{
					lineSeries.LabelFormatString = "{1:C0}";
				}
			}
			SalesTimeLineChartModel.InvalidatePlot(true);
		}

		class ClickableLineSeries : LineSeries
		{
			public ClickableLineSeries(OxyColor originalColor)
				: base()
			{
				Color = originalColor;
				OriginalColor = originalColor;
				LightColor = GetLighterColor(originalColor);
			}

			readonly OxyColor OriginalColor;
			readonly OxyColor LightColor;

			public void Highlight()
			{
				Color = OriginalColor;
				LabelFormatString = "{1:C0}";
			}

			public void Unhighlight()
			{
				Color = LightColor;
				LabelFormatString = "";
			}

			public void Reset()
			{
				Color = OriginalColor;
				LabelFormatString = "";
			}

			static OxyColor GetLighterColor(OxyColor color)
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

		#endregion

		#region Colors

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant color code")]
		OxyColor BorderColor => OxyColor.Parse("#FFCCCCCC");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant color code")]
		OxyColor TextColor => OxyColor.Parse("#FF808080");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant color code")]
		OxyColor ActualOrgRevenueColor => OxyColor.Parse("#FF00A8FF");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant color code")]
		OxyColor ActualJobRevenueColor => OxyColor.Parse("#FF8FD400");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant color code")]
		OxyColor CommittedRevenueColor => OxyColor.Parse("#FFFF7000");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant color code")]
		OxyColor TotalEstimatedRevenueColor => OxyColor.Parse("#FFFFDD00");

		#endregion

		#region Series Titles

		string CommittedAndForecastValueSeriesTitle => Res.GetString("6fdae32f-cdad-4b1c-878d-d94baacd5339", "Committed + Forecast Value");
		string CommittedValueSeriesTitle => Res.GetString("0da09a64-4ff2-40ce-ba77-af514fc5ff10", "Committed Value");
		string JobRevenueSeriesTitle => Res.GetString("c9e4baf3-bb4d-40cd-a99d-b9718190ef98", "Job Revenue");
		string RevenueSeriesTitle => Res.GetString("13c07b05-96ab-46dd-a76f-b00056374784", "Revenue");

		#endregion
	}
}
