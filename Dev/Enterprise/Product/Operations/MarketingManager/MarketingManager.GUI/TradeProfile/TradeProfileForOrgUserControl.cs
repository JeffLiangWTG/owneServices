using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TradeProfileForOrgUserControl : ZUserControl
	{
		public TradeProfileForOrgUserControl()
		{
			InitializeComponent();
		}

		#region CurrentDataItem

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var org = (OrgHeader)dataSource;
			if (org == null)
			{
				salesBreakdown = null;
			}
			else if (salesBreakdown == null)
			{
				salesBreakdown = new SalesBreakdown(org);
				salesBreakdownControl.SetDataBinding(salesBreakdown, null);
			}

			base.SetDataBinding(dataSource, dataMember);
		}

		public new OrgHeader DataSource
		{
			get { return (OrgHeader)base.DataSource; }
		}

		SalesBreakdown salesBreakdown;

		#endregion
	}
}
