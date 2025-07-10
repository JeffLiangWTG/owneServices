using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public sealed class ImportInvoiceDetailsControlBag : ControlBag
{
	ImportInvoiceDetailsControlBag()
	{
		TranCircumstanceUserControl = RegisterControl(nameof(ImportInvoiceDetailsUserControl.TranCircumstanceUserControl));
		ValuationMethodDropEdit = RegisterControl(nameof(ImportInvoiceDetailsUserControl.ValuationMethodDropEdit));
	}

	public static ImportInvoiceDetailsControlBag Instance => instance ?? (instance = new ImportInvoiceDetailsControlBag());

	[ThreadStatic]
	static ImportInvoiceDetailsControlBag instance;

	protected override Control CreateTemplate() => new ImportInvoiceDetailsUserControl();

	public ControlReference TranCircumstanceUserControl { get; }
	public ControlReference ValuationMethodDropEdit { get; }
}
