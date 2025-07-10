using Enterprise.GlobalCommercialInvoice.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.GlobalCommercialInvoice.GUI
{
	public partial class GlobalCommercialInvoiceHeaderDetailUserControl : ZUserControl
	{
		public GlobalCommercialInvoiceHeaderDetailUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is GlobalCommercialInvoicePluginBusinessObject)
			{
				base.SetDataBinding(dataSource: dataSource, dataMember: nameof(GlobalCommercialInvoicePluginBusinessObject.Headers));
			}
			else if (dataSource is null)
			{
				base.SetDataBinding(dataSource: null, dataMember: string.Empty);
			}
		}
	}
}
