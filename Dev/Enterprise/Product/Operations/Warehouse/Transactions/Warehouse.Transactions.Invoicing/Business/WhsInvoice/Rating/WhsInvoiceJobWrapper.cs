using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	public class WhsInvoiceJobWrapper : IJobInvoicingPlugIn
	{
		WhsInvoiceJobWrapper(ZGuid pk, IWrappedInvoiceJobType invoiceType, BusinessObjectFactory factory)
		{
			PK = pk;
			InvoiceType = invoiceType;
			Factory = factory;
		}

		public IJobInvoicingSupporter InvoicingSupporter => InvoiceType.GetInvoicingSupporter(Factory, PK);

		public bool AllowInvoiceDeletion { get; set; }

		public string JobNumber { get; set; }

		public bool IsDeleted { get; set; }

		public ZGuid PK { get; }

		public string TableName => InvoiceType.TableName;

		public bool IsInDatabase => true;

		public BusinessObjectFactory Factory { get; }

		public void OnJobCreated(JobHeader job)
		{
		}

		public void OnJobCreating(JobHeader job)
		{
		}

		public void OnJobDeleted(JobHeader job)
		{
		}

		public void OnJobDeleting(JobHeader job)
		{
		}

		public void SetJobNumberFieldOnSaving()
		{
		}

		public IWrappedInvoiceJobType InvoiceType { get; }

		public static WhsInvoiceJobWrapper[] CreateInvoiceJobsFromDataRows(IEnumerable<DataRow> rows, string pkRow, IWrappedInvoiceJobType invoiceType)
		{
			var newFactory = new BusinessObjectFactory();
			return rows.Select(r => new WhsInvoiceJobWrapper(new ZGuid(r[pkRow]), invoiceType, newFactory)).ToArray();
		}
	}
}
