namespace Enterprise.MasterFiles.GUI
{
	public class MainPageModel : ModelBase<MainPageModel>
	{
		public EventsBannerModel EventsBannerModel
		{
			get => eventsBannerModel;
			set
			{
				eventsBannerModel = value;
				NotifyPropertyChanged();
			}
		}

		EventsBannerModel eventsBannerModel;

		public ReportsModel ReportsModel
		{
			get => reportsModel;
			set
			{
				reportsModel = value;
				NotifyPropertyChanged();
			}
		}

		ReportsModel reportsModel;

		public TopBannerModel TopBannerModel
		{
			get => topBannerModel;
			set
			{
				topBannerModel = value;
				NotifyPropertyChanged();
			}
		}

		TopBannerModel topBannerModel;
	}
}
