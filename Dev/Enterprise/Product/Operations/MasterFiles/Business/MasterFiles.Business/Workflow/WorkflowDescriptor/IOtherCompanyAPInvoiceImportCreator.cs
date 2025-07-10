using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IOtherCompanyAPInvoiceImportCreator
	{
		IProcessor CreateOtherCompanyAPInvoiceImport(IWorkflowProvider workflowProvider, ZGuid companyPK);
	}
}
