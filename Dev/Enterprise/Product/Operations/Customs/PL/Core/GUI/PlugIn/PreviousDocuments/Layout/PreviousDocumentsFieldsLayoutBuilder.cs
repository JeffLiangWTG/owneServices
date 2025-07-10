using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.PlugIn;

public class PreviousDocumentsFieldsLayoutBuilder : PreviousDocumentsFieldsLayoutBuilder<PreviousDocument>
{
	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;

	protected override int MaxColumns => 2;

	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();

		var controlBag = PreviousDocumentsFieldsControlBag.Instance;

		SetVisibility(controlBag.SubTypeDropEdit, x => !x.IsExport);
		SetVisibility(controlBag.LineNoCalcEdit, GetLineNoCalcEditVisibilty);
		SetVisibility(controlBag.QuantityCalcDropEdit, GetVisibilityDependantOnExportAndInvoiceLine);
		SetVisibility(controlBag.Quantity2CalcDropEdit, GetVisibilityDependantOnExportAndInvoiceLine);
		SetVisibility(controlBag.PackageQuantityCalcDropEdit, GetVisibilityDependantOnExportAndInvoiceLine);
		SetVisibility(controlBag.Reference2TextBox, GetVisibilityDependantOnExportAndInvoiceLine);
	}

	bool ParentIsInvoiceLine(PreviousDocument previousDocument) => previousDocument.Parent is JobComInvoiceLine;

	bool GetVisibilityDependantOnExportAndInvoiceLine(PreviousDocument previousDocument) => previousDocument.IsExport && ParentIsInvoiceLine(previousDocument);
	bool GetLineNoCalcEditVisibilty(PreviousDocument previousDocument) => !previousDocument.IsExport || previousDocument.ParentIsJobComInvoiceLineOrHeader;
}
