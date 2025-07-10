using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

sealed class DeclarationSupportingDocumentsGridUserControl : EU.NCTS.GUI.DeclarationSupportingDocumentsGridUserControl
{
	protected override IGridColumnLayoutProvider GetGridColumnLayoutProvider()
		=> new SupportingDocumentsGridColumnLayout();
}
