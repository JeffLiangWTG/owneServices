using CargoWise.Windows.UI;
using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class MessageSendingEDocsUserControl : ZUserControl
{
	public MessageSendingEDocsUserControl()
	{
		InitializeComponent();
		InitializeNewColumns();
	}

	const string IsVisibleForBindingString = nameof(IIsVisibleForBindingControl.IsVisibleForBinding);

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, dataMember);
		UserInputGroupBox.DataBindings.RemoveBinding(IsVisibleForBindingString);

		if (dataSource != null)
		{
			UserInputGroupBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, nameof(CustomsDeclarationMessageSendingObjectParent.HasEDocs)));
		}
	}

	void InitializeNewColumns()
	{
		var additionalInformationCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
		var supportingDocumentCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
		var documentDescriptionTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();

		additionalInformationCodeFindBoxColumnStyleInfo.ColumnName = AutoJobDeclarationMessageSendingEDocs.Schema.AdditionalInformation;
		additionalInformationCodeFindBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
		SupportingDocumentsGrid.ColumnStyles.Add(additionalInformationCodeFindBoxColumnStyleInfo);

		supportingDocumentCodeFindBoxColumnStyleInfo.ColumnName = AutoJobDeclarationMessageSendingEDocs.Schema.SupportingDocument;
		supportingDocumentCodeFindBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
		SupportingDocumentsGrid.ColumnStyles.Add(supportingDocumentCodeFindBoxColumnStyleInfo);

		documentDescriptionTextBoxColumnStyleInfo.ColumnName = AutoJobDeclarationMessageSendingEDocs.Schema.DocumentDescription;
		documentDescriptionTextBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		SupportingDocumentsGrid.ColumnStyles.Add(documentDescriptionTextBoxColumnStyleInfo);
	}
}
