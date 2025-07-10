using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public interface IDeclarationFormLayoutProvider
	{
		IPanelLayoutProvider GetDeclarationDetailsLayout();

		IPanelLayoutProvider GetDeclarationShipmentDetailsLayout();

		IPanelLayoutProvider GetDeclarationShipmentTypeLayout();

		IPanelLayoutProvider GetDeclarationTransportDetailsLayout(BaseJobDeclaration declaration);

		IPanelLayoutProvider GetDeclarationOrganisationsLayout();

		IPanelLayoutProvider GetMiscOptionsLayout(BaseJobDeclaration declaration);

		IPanelLayoutProvider GetCommercialInvoiceDetailsLayout(BaseJobDeclaration declaration);

		IPanelLayoutProvider GetInstructionDetailsLayoutProvider(BaseJobDeclaration declaration);

		IPanelLayoutProvider GetInvoiceLineCalculationsPanelLayout(BaseJobDeclaration declaration);

		IGridColumnLayoutProvider GetInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration);

		IGridColumnLayoutProvider GetApportionedInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration);

		IGridColumnLayoutProvider GetGroupChargesGridColumnLayout(BaseJobDeclaration declaration);
	}
}
