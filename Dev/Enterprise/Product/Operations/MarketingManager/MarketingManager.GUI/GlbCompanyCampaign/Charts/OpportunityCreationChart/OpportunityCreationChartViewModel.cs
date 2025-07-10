using System;
using Enterprise.MarketingManager.Business;
using OxyPlot;
using OxyPlot.Series;

namespace Enterprise.MarketingManager.GUI
{
	class OpportunityCreationChartViewModel : IDisposable
	{
		public OpportunityCreationChartViewModel(GlbCompanyCampaign campaign)
		{
			Campaign = campaign;
		}

		public GlbCompanyCampaign Campaign { get; }

		public PlotModel PieModel { get; private set; }

		#region Data Construction

		void LoadData()
		{
			var tempChartData = OpportunitiesCreation.LoadOpportunities(Campaign);
			if (tempChartData != null)
			{
				CurrentCount = tempChartData.CurrentCount;
				WonCount = tempChartData.WonCount;
				OtherCount = tempChartData.OtherCount;
				LostCount = tempChartData.LostCount;
				TotalCount = tempChartData.TotalCount;
				WinRatio = tempChartData.WinRatio;
			}
		}

		public void PopulatePieModel()
		{
			LoadData();

			var plotModel = new PlotModel();
			var pieSeries = new PieSeries
			{
				Diameter = 0.7,
				InnerDiameter = 0.5,
				ExplodedDistance = 0,
				Stroke = OxyColors.White,
				StrokeThickness = 1,
				AngleSpan = 360,
				StartAngle = 270,
				TickRadialLength = 0,
				TickHorizontalLength = 0,
				TickDistance = 5
			};
			plotModel.Series.Add(pieSeries);

			if (TotalCount == 0)
			{
				pieSeries.Slices.Add(new OpportunityCreationPieSlice(100, OxyColors.Silver));
			}
			else
			{
				AddSlice(pieSeries, CurrentCount, OxyColors.Orange);
				AddSlice(pieSeries, WonCount, OxyColors.Green);
				AddSlice(pieSeries, LostCount, OxyColors.Red);
				AddSlice(pieSeries, OtherCount, OxyColors.Black);
			}

			PieModel = plotModel;
		}

		#endregion

		#region Properties

		public int CurrentCount { get; set; }

		public int WonCount { get; set; }

		public int OtherCount { get; set; }

		public int LostCount { get; set; }

		public int TotalCount { get; set; }

		public decimal WinRatio { get; set; }

		#endregion

		public void Dispose()
		{
			PieModel = null;
		}

		void AddSlice(PieSeries pieSeries, int count, OxyColor color)
		{
			if (count > 0)
			{
				pieSeries.Slices.Add(new OpportunityCreationPieSlice(count, color));
			}
		}
	}
}
