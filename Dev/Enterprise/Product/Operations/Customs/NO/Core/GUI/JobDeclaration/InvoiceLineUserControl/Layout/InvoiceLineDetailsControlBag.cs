using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

sealed class InvoiceLineDetailsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new InvoiceLineDetailsUserControl();

	public static InvoiceLineDetailsControlBag Instance => instance ??= new InvoiceLineDetailsControlBag();

	[ThreadStatic]
	static InvoiceLineDetailsControlBag instance;
	InvoiceLineDetailsControlBag()
	{
		CustomsRateOverrideUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.CustomsRateOverrideUserControl));
		RtRateOverrideCalcEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.RtRateOverrideCalcEdit));
		GoodsMarksLongTextControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.GoodsMarksLongTextControl));
		MergeOverrideTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.MergeOverrideTextBox));
		PackageTypeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.PackageTypeDropEdit));
		ProcedureCodeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.ProcedureCodeDropEdit));
		ReducedCustomsFlagDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.ReducedCustomsFlagDropEdit));
		SupplementaryCode1DropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.SupplementaryCode1DropEdit));
		SupplementaryCode2DropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.SupplementaryCode2DropEdit));
		AdditionalSupplementaryCodesUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.AdditionalSupplementaryCodesUserControl));
	}

	public ControlReference CustomsRateOverrideUserControl { get; }
	public ControlReference RtRateOverrideCalcEdit { get; }
	public ControlReference GoodsMarksLongTextControl { get; }
	public ControlReference MergeOverrideTextBox { get; }
	public ControlReference ProcedureCodeDropEdit { get; }
	public ControlReference PackageTypeDropEdit { get; }
	public ControlReference ReducedCustomsFlagDropEdit { get; }
	public ControlReference SupplementaryCode1DropEdit { get; }
	public ControlReference SupplementaryCode2DropEdit { get; }
	public ControlReference AdditionalSupplementaryCodesUserControl { get; }
}
