using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public sealed class PlugInLayoutProvider : EU.GUI.PlugIn.IPlugInLayoutProvider
{
	public IPanelLayoutProvider GetInvoiceLinePreviousDocumentsDetailsLayout(JobDeclaration declaration) => new PreviousDocumentsFieldsLayout();

	public IPanelLayoutProvider GetInvoiceHeaderPreviousDocumentsDetailsLayout(JobDeclaration declaration) => new PreviousDocumentsFieldsLayout();

	public IPanelLayoutProvider GetDeclarationPreviousDocumentsDetailsLayout(JobDeclaration declaration) => new PreviousDocumentsFieldsLayout();

	public IPanelLayoutProvider GetEntryInstructionPreviousDocumentsDetailsLayout(JobDeclaration declaration) => new PreviousDocumentsFieldsLayout();
}
