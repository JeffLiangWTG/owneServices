using System.Windows.Forms;
using Enterprise.GlobalCommercialInvoice.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.GlobalCommercialInvoice.GUI
{
	public partial class GlobalCommercialInvoicePluginUserControl : ZUserControl
	{
		readonly GlobalCommercialInvoicePluginBusinessObject invoicePluginBusinessObject;

		public GlobalCommercialInvoicePluginUserControl(GlobalCommercialInvoicePluginBusinessObject invoicePluginBusinessObject)
		{
			this.invoicePluginBusinessObject = invoicePluginBusinessObject;

			base.SetDataBinding(dataSource: invoicePluginBusinessObject, dataMember: string.Empty);
			InitializeComponent();
			AddControls();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is null)
			{
				base.SetDataBinding(dataSource: null, dataMember: string.Empty);
			}
		}

		void AddControls()
		{
			var headerGridUserControl = new GlobalCommercialInvoiceHeaderGridUserControl() { Dock = DockStyle.Fill };
			var headerDetailUserControl = new GlobalCommercialInvoiceHeaderDetailUserControl() { Dock = DockStyle.Fill };
			var lineGridUserControl = new GlobalCommercialInvoiceLineGridUserControl() { Dock = DockStyle.Fill };
			var lineDetailUserControl = new GlobalCommercialInvoiceLineDetailUserControl() { Dock = DockStyle.Fill };

			InvoiceHeaderSplitContainer.Panel1.Controls.Add(headerGridUserControl);
			InvoiceHeaderSplitContainer.Panel2.Controls.Add(headerDetailUserControl);
			InvoiceLineSplitContainer.Panel1.Controls.Add(lineGridUserControl);
			InvoiceLineSplitContainer.Panel2.Controls.Add(lineDetailUserControl);

			InvoiceMainTabControl.SelectedIndexChanged += InvoiceMainTabControl_SelectedIndexChanged;
		}

		void InvoiceMainTabControl_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (InvoiceMainTabControl.SelectedTab == InvoiceLineTabPage && InvoiceMainTabControl.Visible)
			{
				if (invoicePluginBusinessObject.Headers.Count > 0)
				{
					InvoiceLineSplitContainer.Visible = true;
					InvoiceLineUserControlsVisibilityLabel.Visible = false;
				}
				else
				{
					InvoiceLineSplitContainer.Visible = false;
					InvoiceLineUserControlsVisibilityLabel.Visible = true;
				}
			}
		}
	}
}
