using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

sealed class EntryInstructionSupportingDocumentsFieldsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public EntryInstructionSupportingDocumentsFieldsLayout()
	{
		Layout = CreateLayout();
	}

	static PanelLayout CreateLayout()
	{
		var builder = new SupportingDocumentFieldsLayoutBuilder<SupportingDocument>();

		var euBag = SupportingDocumentFieldsControlBag.Instance;

		builder.AddControlBag(euBag);

		builder.AddColumn();
		builder.Add(euBag.CodeCodeFindBox, ControlWidthClass.Auto);
		builder.Add(euBag.ReferenceNumberTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.DocumentLineNoCalcEdit, ControlWidthClass.Auto);
		builder.Add(euBag.AdditionalDescriptionTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.DateOfExpiryDateEdit, ControlWidthClass.Auto);

		return builder.Build();
	}
}
