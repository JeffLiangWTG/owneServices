using System.Linq;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public partial class ScheduleUserControl : ZUserControl
	{
		public ScheduleUserControl()
		{
			InitializeComponent();
		}

		const string StatusText = "StatusText";

		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);
			this.RecipientTimeZonesGrid.Sort = "UtcOffset";
			if (!DesignModeFinder.IsDesigning)
			{
				HideOrShowStatusColumn();
			}
		}

		void HideOrShowStatusColumn()
		{
			var dataSource = (GlbCompanyCampaignItemSchedule)CurrentDataItem;
			var tableCode = dataSource.SelectedScheduleItems.Any() ? dataSource.SelectedScheduleItems.FirstOrDefault().TableCode.ToString() : GlbCompanyCampaignItemSchema.Constants.Prefix;

			if (tableCode == ViewCampaignContactSchema.Constants.Prefix)
			{
				RecipientTimeZonesGrid.RemoveFromAvailableColumns(StatusText);
			}
			else
			{
				RecipientTimeZonesGrid.AddToAvailableColumns(StatusText);
			}
		}
	}
}
