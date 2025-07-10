using CargoWise.Windows.UI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI.Helpers;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class InvoiceHeaderUserControl : EU.GUI.CommercialInvoice.InvoiceHeaderUserControl
{
	public InvoiceHeaderUserControl()
	{
		InitializeComponent();
	}

	const string IsVisibleForBindingString = nameof(IIsVisibleForBindingControl.IsVisibleForBinding);

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, dataMember);
		ExportJZ_IncoTermDropDownEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);
		JZ_IncoTermPlaceTextBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
		AgreedPlaceCodeFindBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
		ExportIncoTermExplainButton.DataBindings.RemoveBinding(IsVisibleForBindingString);
		IncoTermExplainButton.DataBindings.RemoveBinding(IsVisibleForBindingString);
		JZ_IncoTermDropDownEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);

		if (dataSource != null)
		{
			ExportIncoTermExplainButton.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, nameof(JobDeclaration.IsExport)));
			ExportJZ_IncoTermDropDownEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, nameof(JobDeclaration.IsExport)));
			JZ_IncoTermPlaceTextBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, nameof(JobDeclaration.IsExport)));
			AgreedPlaceCodeFindBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, nameof(JobDeclaration.IsExport)));
			JZ_IncoTermDropDownEdit.DataBindings.Add(new NegateBoolBinding(IsVisibleForBindingString, BindingSource.DataSource, nameof(JobDeclaration.IsExport)));
			IncoTermExplainButton.DataBindings.Add(new NegateBoolBinding(IsVisibleForBindingString, BindingSource.DataSource, nameof(JobDeclaration.IsExport)));
		}
	}

	void ExportIncoTermExplainButton_Click(object sender, System.EventArgs e)
	{
		ShowIncoTermDescriptionForm();
	}
}
