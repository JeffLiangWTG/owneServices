using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(InvoiceLineDetailsUserControl))]
sealed class InvoiceLineDetailsUserControlTest : TestCaseWithFactory
{
	public void TestControls() => CombineAssertions(() =>
	{
		using var control = new InvoiceLineDetailsUserControl();
		_ = control.AssertContainsControl<ZDropEdit>(nameof(InvoiceLineDetailsUserControl.ProcedureCodeDropEdit), x => x
			.WithBindTo(nameof(JobComInvoiceLine.JI_Procedure))
			.WithCaption("Procedure Code")
		);

		_ = control.AssertContainsControl<ZDropEdit>(nameof(InvoiceLineDetailsUserControl.ReducedCustomsFlagDropEdit), x => x
			.WithBindTo(nameof(JobComInvoiceLine.JI_ReducedCustomsFlag))
			.WithCaption("Reduced custom")
			.WithWidthScaled(140)
		);

		_ = control.AssertContainsControl<LongTextControl>(nameof(InvoiceLineDetailsUserControl.GoodsMarksLongTextControl), x => x
			.WithBindTo(nameof(JobComInvoiceLine.JI_GoodsMarks))
		);

		_ = control.AssertContainsControl<CustomsRateOverrideUserControl>(nameof(InvoiceLineDetailsUserControl.CustomsRateOverrideUserControl), x => x
			.WithBindTo(".")
		);

		_ = control.AssertContainsControl<ZCalcEdit>("RtRateOverrideCalcEdit", x => x
			.WithBindTo(nameof(JobComInvoiceLine.JI_RTOValue))
			.WithCaption("RT rate override")
		);

		_ = control.AssertContainsControl<ZTextBox>(nameof(InvoiceLineDetailsUserControl.MergeOverrideTextBox), x => x
			.WithBindTo(nameof(JobComInvoiceLine.JI_MergeOverride))
			.WithCaption("Merge override")
			.WithFullDescription(
				"Add value (of own choice) here to avoid merging (when all other merge-fields are equal). " +
				"If the same value is added to multiple lines, they will be merged (when all other merge-fields are equal)."
			)
		);

		_ = control.AssertContainsControl<ZDropEdit>(nameof(InvoiceLineDetailsUserControl.PackageTypeDropEdit), x => x
			.WithBindTo(nameof(JobComInvoiceLine.JI_PackageType))
			.WithCaption("Beverage packing type")
			.WithShortCaption("Packing type")
			.WithFullDescription("Filter the beverage packing excise duty codes by selecting pack type.")
			.WithWidthScaled(140)
		);

		_ = control.AssertContainsControl<ZDropEdit>(nameof(InvoiceLineDetailsUserControl.SupplementaryCode1DropEdit), x => x
			.WithBindTo(nameof(JobComInvoiceLine.JI_SupplementaryCode1))
			.WithCaption("Excise Code 1")
			.WithFullDescription("Excise Code"));

		_ = control.AssertContainsControl<ZDropEdit>(nameof(InvoiceLineDetailsUserControl.SupplementaryCode2DropEdit), x => x
			.WithBindTo(nameof(JobComInvoiceLine.JI_SupplementaryCode2))
			.WithCaption("Excise Code 2")
			.WithFullDescription("Excise Code"));

		_ = control.AssertContainsControl<AdditionalSupplementaryCodesUserControl>(nameof(InvoiceLineDetailsUserControl.AdditionalSupplementaryCodesUserControl), x => x
			.WithBindTo(".")
			.WithCaption("Add. Exc. Codes")
			.WithFullDescription("Additional excise codes"));
	});
}
