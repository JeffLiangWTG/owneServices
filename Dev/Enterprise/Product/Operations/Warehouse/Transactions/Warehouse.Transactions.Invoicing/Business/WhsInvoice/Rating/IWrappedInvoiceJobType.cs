using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	public interface IWrappedInvoiceJobType
	{
		string TableName { get; }
		IJobInvoicingSupporter GetInvoicingSupporter(BusinessObjectFactory factory, ZGuid pk);
	}
}
