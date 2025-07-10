using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.GUI.PlugIn;

public class SupportingDocumentFieldsLayoutBuilder : EU.GUI.PlugIn.SupportingDocumentFieldsLayoutBuilder<SupportingDocument>
{
	protected override int MaxColumns => 1;

	protected override void SetDefaultVisibilities()
	{
		var euBag = EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.Instance;
		var plBag = SupportingDocumentFieldsControlBag.Instance;

		SetVisibility(euBag.QuantityCalcDropEdit, doc => doc.IsParentInvoiceLine);
		SetVisibility(euBag.UnitOfQuantityDropEdit, doc => doc.IsParentInvoiceLine);
		SetVisibility(euBag.DateOfIssueDateEdit, doc => doc.IsParentInvoiceLine);
		SetVisibility(plBag.ValueConvertToLocalCurrencyControl, doc => (doc.IsParentInvoiceLine && IsExport(doc)));

		SetVisibility(euBag.DocumentLineNoCalcEdit, IsExport);
		SetVisibility(euBag.AdditionalDescriptionTextBox, IsExport);

		SetVisibility(euBag.DateOfExpiryDateEdit, IsExport);

		SetVisibility(plBag.SupDocDescriptionTextBox, IsImport);
		SetVisibility(plBag.SupDocReference2TextBox, IsImport);
	}

	static bool IsExport(SupportingDocument doc) => doc.Declaration?.IsExport ?? false;

	static bool IsImport(SupportingDocument doc) => doc.Declaration?.IsImport ?? false;
}
