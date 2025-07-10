using System.ComponentModel;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public partial class ShipmentDetailsIncoTermsUserControl : ZUserControl
{
	public ShipmentDetailsIncoTermsUserControl()
	{
		InitializeComponent();

#if DEBUG
		TypeDescriptor.AddAttributes(IncoTermDropEdit, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(IncoTermExplainButton, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(ShipmentIncoTermPlaceTextBox, new SuppressControlRequiresTextBasherAttribute());
#endif

		IncoTermDropEdit.AllowOutsideOfParent();
		IncoTermExplainButton.AllowOutsideOfParent();

		IncoTermDropEdit.AllowOverlap(IncoTermExplainButton);
	}

	#region INCOTERMS Explanation

	void IncoTermExplainButton_Click(object sender, System.EventArgs e)
	{
		ZFormModaliser.Show(new IncoTermDescriptionForm(JobDeclaration.JE_ShipmentIncoTerm), ParentForm as ZForm);
	}

	JobDeclaration JobDeclaration => (JobDeclaration)BindingSource.DataSource;

	#endregion
}
