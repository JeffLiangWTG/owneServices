using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class InvoiceHeaderActiveCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public InvoiceHeaderActiveCollectionFetchStrategy(InvoiceHeaderActiveCollection collection)
			: base(collection)
		{
		}

		protected new InvoiceHeaderActiveCollection Collection
		{
			get { return (InvoiceHeaderActiveCollection)base.Collection; }
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			var factory = Collection.Factory;
			var invoiceGroupsPK = new List<ZGuid>();
			var invoicesPK = new List<ZGuid>();
			foreach (BaseJobComInvoiceHeader invoice in Collection)
			{
				invoicesPK.Add(invoice.PK);
				if (!invoiceGroupsPK.Contains(invoice.JZ_JZ_GroupInvoiceFK))
				{
					invoiceGroupsPK.Add(invoice.JZ_JZ_GroupInvoiceFK);
				}
				if (invoice.SupportAdditionalDeclarations)
				{
					factory.AddFetchHint(GenPivotSchema.XX_Relation1ID, invoice.PK);
				}
			}
			var invoiceGroupsQuery = new ZQuery(JobComInvoiceHeaderSchema.PK, invoiceGroupsPK);
			var invoiceGroups = factory.Load<BaseJobComInvoiceGroupHeader>(invoiceGroupsQuery);
			if (invoiceGroups.Length > 0)
			{
				foreach (var invoiceGroup in invoiceGroups)
				{
					factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, invoiceGroup.PK);
				}
			}
			invoicesPK.ForEach(x =>
				{
					factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, x);
					factory.AddFetchHint(JobComInvoiceLineSchema.JI_JZ, x);
				});
		}

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			base.FetchForViewCore(businessObjects, columns);

			if (businessObjects.Length > 0 && columns.Any(x => x.ColumnName == BaseJobComInvoiceHeader.Schema.InvoiceLineTotal))
			{
				var factory = Collection.Factory;

				foreach (BaseJobComInvoiceHeader header in businessObjects)
				{
					factory.AddFetchHint(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_JZ, header.PK);
				}

				foreach (BaseJobComInvoiceHeader header in businessObjects)
				{
					foreach (var line in header.InvoiceLines)
					{
						factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, line.PK);
					}
				}
			}
		}
	}
}
