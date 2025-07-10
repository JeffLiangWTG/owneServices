using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Invoicing.Business
{
	public interface IWrappedInvoiceJobType
	{
		string TableName { get; }
		IJobInvoicingSupporter GetInvoicingSupporter(BusinessObjectFactory factory, ZGuid pk);
	}
}
