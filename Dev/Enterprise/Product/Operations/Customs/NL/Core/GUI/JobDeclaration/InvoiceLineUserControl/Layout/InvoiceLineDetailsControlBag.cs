using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI
{
	public sealed class InvoiceLineDetailsControlBag : ControlBag
	{
		public static InvoiceLineDetailsControlBag Instance => invoiceLineDetailsControlBag.Value;

		InvoiceLineDetailsControlBag()
		{
			ECCNCodesUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.ECCNCodesUserControl));
		}

		public ControlReference ECCNCodesUserControl { get; }

		protected override Control CreateTemplate() => new InvoiceLineDetailsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<InvoiceLineDetailsControlBag> invoiceLineDetailsControlBag = new Lazy<InvoiceLineDetailsControlBag>(() => new InvoiceLineDetailsControlBag());
	}
}
