using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class SalesProductForm : ZTemplateForm
	{
		public SalesProductForm(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
			InitializeComponent();
		}

		public new OrgSalesProduct BusinessEntity
		{
			get { return (OrgSalesProduct)base.BusinessEntity; }
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
