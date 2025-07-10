using Enterprise.GlobalCommercialInvoice.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.GlobalCommercialInvoice.GUI
{
	public partial class GlobalCommercialInvoiceLineDetailUserControl : ZUserControl
	{
		public GlobalCommercialInvoiceLineDetailUserControl()
		{
			InitializeComponent();
			AdjustSpecificUserControlWidth();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is GlobalCommercialInvoicePluginBusinessObject)
			{
				base.SetDataBinding(dataSource: dataSource, dataMember: nameof(GlobalCommercialInvoicePluginBusinessObject.Lines));
			}
			else if (dataSource is null)
			{
				base.SetDataBinding(dataSource: null, dataMember: string.Empty);
			}
		}

		void AdjustSpecificUserControlWidth()
		{
#pragma warning disable CW1017 // Non DPI-aware code has been detected
			InvoiceNumberGuidDropEdit.CodeBox.Width = GetWidthSafe(InvoiceProductCodeFindBox.Width - (InvoiceNumberGuidDropEdit.Width - InvoiceNumberGuidDropEdit.CodeBox.Width), InvoiceNumberGuidDropEdit.CodeBox.Width);

			InvoiceTariff1FindBox.CodeBox.Width = GetWidthSafe(InvoiceProductCodeFindBox.Width - (InvoiceTariff1FindBox.Width - InvoiceTariff1FindBox.CodeBox.Width), InvoiceTariff1FindBox.CodeBox.Width);
			InvoiceTariff2FindBox.CodeBox.Width = GetWidthSafe(InvoiceProductCodeFindBox.Width - (InvoiceTariff2FindBox.Width - InvoiceTariff2FindBox.CodeBox.Width), InvoiceTariff2FindBox.CodeBox.Width);
#pragma warning restore CW1017 // Non DPI-aware code has been detected

			int GetWidthSafe(int width, int rawWidth)
			{
				return width > 0 ? width : rawWidth;
			}
		}
	}
}
