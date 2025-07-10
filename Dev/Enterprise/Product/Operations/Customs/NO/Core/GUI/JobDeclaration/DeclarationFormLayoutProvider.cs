using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

public sealed class DeclarationFormLayoutProvider : IDeclarationFormLayoutProvider
{
	public IPanelLayoutProvider GetDeclarationDetailsLayout() => new DeclarationDetailsLayouts();

	public IPanelLayoutProvider GetDeclarationShipmentDetailsLayout() => new ShipmentDetailsLayouts();

	public IPanelLayoutProvider GetDeclarationShipmentTypeLayout() => new ShipmentTypeLayouts();

	public IPanelLayoutProvider GetDeclarationTransportDetailsLayout(BaseJobDeclaration declaration) => new TransportDetailsLayout();

	public IPanelLayoutProvider GetDeclarationOrganisationsLayout() => new OrganisationsLayout();

	public IPanelLayoutProvider GetMiscOptionsLayout(BaseJobDeclaration declaration) => new MiscOptionsLayouts();

	public IPanelLayoutProvider GetCommercialInvoiceDetailsLayout(BaseJobDeclaration declaration) => new CommercialInvoiceDetailsLayout();

	public IPanelLayoutProvider GetInstructionDetailsLayoutProvider(BaseJobDeclaration declaration) => new EntryInstructionDetailsBasicLayout();

	public IPanelLayoutProvider GetInvoiceLineCalculationsPanelLayout(BaseJobDeclaration declaration) => null;

	public IGridColumnLayoutProvider GetInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

	public IGridColumnLayoutProvider GetApportionedInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

	public IGridColumnLayoutProvider GetGroupChargesGridColumnLayout(BaseJobDeclaration declaration) => null;
}
