using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Module
{
	public partial class GatewayConsolProfitShareRedistributionFilterControl : ZFilterStripControl
	{
		public GatewayConsolProfitShareRedistributionFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			ShowAuditColumns();
		}

		void ShowAuditColumns()
		{
			foreach (var item in Grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(c => c.GroupName.Caption == FilterStripAuditDetails.AuditDetailsGroupText.Caption))
			{
				item.IsVisible = true;
			}
		}
	}
}
