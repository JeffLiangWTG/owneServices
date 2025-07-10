using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class InvoiceLineCompleteCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public InvoiceLineCompleteCollectionFetchStrategy(InvoiceLineCompleteCollection collection)
			: base(collection)
		{
		}

		protected new InvoiceLineCompleteCollection Collection
		{
			get { return (InvoiceLineCompleteCollection)base.Collection; }
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			var factory = Collection.Factory;
			var invoiceLinesPK = new List<ZGuid>();
			foreach (BaseJobComInvoiceLine invoiceLine in Collection)
			{
				factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, invoiceLine.PK);
				factory.AddFetchHint(CusContainerInvoiceLinePivotSchema.C2_JI, invoiceLine.PK);
			}
		}
	}
}
