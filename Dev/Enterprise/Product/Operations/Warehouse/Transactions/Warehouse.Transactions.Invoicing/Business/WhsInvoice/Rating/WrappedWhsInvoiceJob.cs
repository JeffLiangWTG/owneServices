using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public class WrappedWhsInvoiceJob<T> : IWrappedInvoiceJobType
		where T : BusinessObject
	{
		public string TableName => BusinessObjectFactory.GetTableNameFromType(typeof(T));

		public IJobInvoicingSupporter GetInvoicingSupporter(BusinessObjectFactory factory, ZGuid pk)
		{
			var bizo = (IJobInvoicingPlugIn)factory.Load<T>(pk);
			return bizo.InvoicingSupporter;
		}
	}
}
