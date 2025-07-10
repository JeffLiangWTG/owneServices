using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Core;
using OxyPlot;
using OxyPlot.Series;

namespace Enterprise.MarketingManager.GUI
{
	class TrackingStatusChartViewModel : IDisposable
	{
		public TrackingStatusChartViewModel(GlbCompanyCampaign campaign)
		{
			Campaign = campaign;
			ChartData = new ObservableCollection<TrackingStatusChartData>();
			UnsubscribedData = new Collection<TrackingStatusChartData>();
			PieModel = new PlotModel();
		}

		public GlbCompanyCampaign Campaign { get; }
		public PlotModel PieModel { get; private set; }
		public ObservableCollection<TrackingStatusChartData> ChartData { get; }
		public Collection<TrackingStatusChartData> UnsubscribedData { get; }
		#region Data Construction

		void LoadData()
		{
			var tempChartData = ContactsAndUniqueOrganisations.LoadEmailsAndOrgsCount(Campaign);
			ChartData.Clear();
			UnsubscribedData.Clear();

			EmailsTotal = (ZInt)tempChartData.TotalData.EmailsCount;
			ClientsTotal = (ZInt)tempChartData.TotalData.ClientsCount;
			UnsubscribedData.Add(tempChartData.UnsubscribedData);
			UnsubscribedRowVisible = UnsubscribedData.Sum(data => data.ClientsCount + data.EmailsCount) > 0;

			tempChartData.DeliveryData.ForEach(trackData => ChartData.Add(trackData));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Invalid Tracking Status Text")]
		const string UnavailableDataText = "No Emails";

		public void PopulatePieModel()
		{
			LoadData();
			PieModel.Series.Clear();

			var pieSeries = new PieSeries
			{
				Diameter = 0.8,
				InnerDiameter = 0.5,
				ExplodedDistance = 0,
				Stroke = OxyColors.White,
				StrokeThickness = 1,
				AngleSpan = 360,
				StartAngle = 270,
				TickRadialLength = 0,
				TickHorizontalLength = 0,
				TickDistance = 7,
				TickLabelDistance = ChartData.Count > 0 ? 0 : 1000
			};
			PieModel.Series.Add(pieSeries);

			var pieUnsSeries = new PieSeries
			{
				Diameter = 0.7,
				InnerDiameter = 0.45,
				ExplodedDistance = 0,
				Stroke = OxyColors.White,
				StrokeThickness = 1,
				AngleSpan = 360,
				StartAngle = 270,
				TickRadialLength = 0,
				TickHorizontalLength = 0,
				TickDistance = -20,
				TickLabelDistance = UnsubscribedRowVisible ? 0 : 1000
			};
			pieUnsSeries.FontSize /= 1.5;
			pieUnsSeries.TextColor = OxyColors.Transparent;

			if (ChartData.Count == 0)
			{
				pieSeries.Slices.Add(new TrackingStatusPieSlice("", 100)
				{
					Fill = ColorCollection.GetOxyColor(TrackingStatusChartData.NotSentCampaignStatus),
					IsExploded = true,
					Tag = UnavailableDataText
				});
				return;
			}

			foreach (var chartData in ChartData)
			{
				pieSeries.Slices.Add(new TrackingStatusPieSlice("", chartData.EmailsCount)
				{
					Fill = ColorCollection.GetOxyColor(chartData.Status),
					IsExploded = true,
					Tag = chartData.Status
				});
			}

			if (UnsubscribedRowVisible)
			{
				foreach (var data in UnsubscribedData)
				{
					pieUnsSeries.Slices.Add(new TrackingStatusPieSlice("", data.EmailsCount)
					{
						Fill = ColorCollection.GetOxyColor(data.Status),
						IsExploded = true,
						Tag = data.Status
					});
				}
				pieUnsSeries.Slices.Add(new TrackingStatusPieSlice("", ChartData.Sum(data => data.EmailsCount) - UnsubscribedData.Sum(data => data.EmailsCount))
				{
					Fill = OxyColors.Transparent
				});

				PieModel.Series.Add(pieUnsSeries);
			}
		}

		#endregion

		#region Properties

		public ZInt EmailsTotal { get; private set; }

		public ZInt ClientsTotal { get; private set; }

		public ZBool UnsubscribedRowVisible { get; private set; }

		public static MultilingualString MultilingualEmails { get; } = ResString.GetMultilingualString("f63f31cd-27bd-43c7-8fbf-659c85a36a6e", "Emails");
		public static MultilingualString MultilingualOrganizations { get; } = ResString.GetMultilingualString("57e268c2-2432-4f60-b01f-e2cd962544dc", "Organizations");
		public static MultilingualString MultilingualSent { get; } = ResString.GetMultilingualString("2d1ec70f-dd77-4525-b3a9-9a572650b505", "Sent");

		#endregion

		#region Events

		public event EventHandler<TrackingStatusDescriptionEventArgs> FilterByDeliveryStatus;
		public event EventHandler<UnsubscribedStatusDescriptionEventArgs> FilterByUnsubscribeStatus;

		public void HandleMouseUp(ScreenPoint position)
		{
			var hitResult = PieModel.Series.Select(series => series.GetNearestPoint(position, false)).FirstOrDefault(result => result != null);
			var status = (hitResult?.Item as TrackingStatusPieSlice)?.Tag;
			if (string.IsNullOrEmpty(status))
			{
				return;
			}

			if (status == TrackingStatusChartData.UnsubscribeStatus)
			{
				OnFilterByUnsubscribedStatus(true);
			}
			else
			{
				OnFilterByDeliveryStatus(status);
			}
		}

		void OnFilterByDeliveryStatus(string status)
		{
			FilterByDeliveryStatus?.Invoke(null, new TrackingStatusDescriptionEventArgs(status));
		}

		void OnFilterByUnsubscribedStatus(bool status)
		{
			FilterByUnsubscribeStatus?.Invoke(null, new UnsubscribedStatusDescriptionEventArgs(status));
		}
		#endregion

		#region Remove Events

		void UnHookEvents()
		{
			if (FilterByDeliveryStatus != null)
			{
				FilterByDeliveryStatus -= FilterByDeliveryStatus;
			}
			if (FilterByUnsubscribeStatus != null)
			{
				FilterByUnsubscribeStatus -= FilterByUnsubscribeStatus;
			}

			if (PieModel != null)
			{
				PieModel = null;
			}
		}

		#endregion

		public void Dispose()
		{
			UnHookEvents();
		}
	}
}
