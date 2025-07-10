using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public sealed class InvoiceLineDetailsControlBag : ControlBag
{
	InvoiceLineDetailsControlBag()
	{
		CountryOfSupplyCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.CountryOfSupplyCodeFindBox));
		CPCUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.CPCUserControl));
	}

	public static InvoiceLineDetailsControlBag Instance => instance ?? (instance = new InvoiceLineDetailsControlBag());

	[ThreadStatic]
	static InvoiceLineDetailsControlBag instance;

	protected override Control CreateTemplate() => new InvoiceLineDetailsUserControl();

	public ControlReference CountryOfSupplyCodeFindBox { get; }

	public ControlReference CPCUserControl { get; }
}
