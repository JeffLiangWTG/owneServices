using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.PlugIn;

public class SupportingDocumentFieldsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public SupportingDocumentFieldsLayout()
	{
		Layout = CreateLayout();
	}

	static PanelLayout CreateLayout()
	{
		var builder = new SupportingDocumentFieldsLayoutBuilder();

		var euBag = EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.Instance;
		var plBag = SupportingDocumentFieldsControlBag.Instance;

		builder.AddControlBag(euBag);
		builder.AddControlBag(plBag);

		builder.AddColumn();
		builder.Add(euBag.CodeCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.ReferenceNumberTextBox, ControlWidthClass.Auto);
		builder.Add(plBag.SupDocReference2TextBox, ControlWidthClass.Auto);
		builder.Add(plBag.SupDocDescriptionTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.DocumentLineNoCalcEdit, ControlWidthClass.Auto);
		builder.Add(euBag.AdditionalDescriptionTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.DateOfIssueDateEdit, ControlWidthClass.Auto);
		builder.Add(euBag.DateOfExpiryDateEdit, ControlWidthClass.Auto);
		builder.Add(euBag.QuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(plBag.ValueConvertToLocalCurrencyControl, ControlWidthClass.Auto);

		return builder.Build();
	}
}
