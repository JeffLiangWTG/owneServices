using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public class WrappedInvoiceJobCartageJob : IWrappedInvoiceJobType
	{
		WrappedInvoiceJobCartageJob()
		{
		}

		public static WrappedInvoiceJobCartageJob Instance { get; } = new WrappedInvoiceJobCartageJob();

		public string TableName
		{
			get => JobCartageSchema.Constants.TableName;
		}

		public IJobInvoicingSupporter GetInvoicingSupporter(BusinessObjectFactory factory, ZGuid pk)
		{
			return null;
		}
	}
}
