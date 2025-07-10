using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class LicensingCertificateOfOriginUserControl : ZUserControl
	{
		public LicensingCertificateOfOriginUserControl()
		{
			InitializeComponent();
			this.BindingSource.SetBindingMember(this.ShippingMarksLongTextControl, "FilteredInvoiceLines.NX101ShippingMarks");
		}
	}
}
