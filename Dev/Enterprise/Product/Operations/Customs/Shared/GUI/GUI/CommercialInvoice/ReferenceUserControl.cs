using System.ComponentModel;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class ReferenceUserControl : ZUserControl
	{
		public ReferenceUserControl()
		{
			InitializeComponent();
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public BaseJobComInvoiceHeader Invoice
		{
			get
			{
				if (fInvoice == null)
				{
					ErrorReporter.ReportOnce("CommInvoiceNull", "Invoice property has not been set on BaseInvoiceHeaderUserControl. Please set this property in your form constructor.");
				}

				return fInvoice;
			}
			set { fInvoice = value; }
		}
		BaseJobComInvoiceHeader fInvoice;

		#region IDisposable Members

		#endregion
	}
}
