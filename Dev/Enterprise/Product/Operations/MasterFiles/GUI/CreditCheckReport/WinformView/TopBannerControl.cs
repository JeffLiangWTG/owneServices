using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class TopBannerControl : ZUserControl
	{
		public TopBannerControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				DataSource.PropertyChanged -= TopBannerModel_PropertyChanged;
			}

			base.SetDataBinding(dataSource, dataMember);

			if (DataSource != null)
			{
				DataSource.PropertyChanged += TopBannerModel_PropertyChanged;
			}
		}

		protected new TopBannerModel DataSource => BindingSource.Current as TopBannerModel;

		void TopBannerModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(TopBannerModel.StatusImageAndDetailEventInfoVisible))
			{
				statusImage.Image = DataSource.StatusImage;
				statusImage.Visible = DataSource.StatusImageAndDetailEventInfoVisible;
				statusDetailInfo.ForeColor = DataSource.StatusDetailsEventsInfoColor;
				statusDetailInfo.Visible = DataSource.StatusImageAndDetailEventInfoVisible;
			}
		}
	}
}
