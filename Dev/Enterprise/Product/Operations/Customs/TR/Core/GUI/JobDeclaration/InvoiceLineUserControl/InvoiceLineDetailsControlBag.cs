using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public sealed class InvoiceLineDetailsControlBag : ControlBag
	{
		public InvoiceLineDetailsControlBag()
		{
			SupplementaryCode1DropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.SupplementaryCode1DropEdit));
			SupplementaryCode2DropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.SupplementaryCode2DropEdit));
			BrandNameTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.BrandNameTextBox));
			PartNoCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.PartNoCodeFindBox));
		}
		public static InvoiceLineDetailsControlBag Instance => instance ?? (instance = new InvoiceLineDetailsControlBag());

		[ThreadStatic]
		static InvoiceLineDetailsControlBag instance;

		protected override Control CreateTemplate() => new InvoiceLineDetailsUserControl();
		public ControlReference SupplementaryCode1DropEdit { get; }
		public ControlReference SupplementaryCode2DropEdit { get; }
		public ControlReference BrandNameTextBox { get; }
		public ControlReference PartNoCodeFindBox { get; }
	}
}
