using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public interface IInvoiceHeaderForProductCreation
	{
		bool HasImportDeclaration { get; }
		bool HasExportDeclration { get; }
		bool InvoicesContainNotPersistentActiveProductWithMissingInvoiceUQ { get; }
		bool IsInwardBondedWarehousingEnabled { get; }
		OrgHeader Importer_Effective { get; }
		OrgHeader Supplier_Effective { get; }
		ZString InvoiceNumber { get; }
		ZString FinalDestinationCountryCode { get; }
		ZString BranchCompanyCountryCode { get; }
	}
}
