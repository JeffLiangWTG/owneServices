using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.PlugIn;

public partial class LayoutSupportingDocumentsFieldsControl : EU.GUI.PlugIn.LayoutSupportingDocumentsFieldsControl
{
	public LayoutSupportingDocumentsFieldsControl()
	{
		InitializeComponent();
	}

	public LayoutSupportingDocumentsFieldsControl(JobDeclaration declaration) : base(declaration)
	{
		InitializeComponent();
	}

	protected override IPanelLayoutProvider GetLayout() => new SupportingDocumentFieldsLayout();
}
