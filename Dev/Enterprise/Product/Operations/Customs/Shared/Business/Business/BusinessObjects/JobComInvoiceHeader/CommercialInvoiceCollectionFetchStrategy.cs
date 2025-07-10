using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class CommercialInvoiceCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public CommercialInvoiceCollectionFetchStrategy(CommercialInvoiceCollection collection)
			: base(collection)
		{
		}

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			if (businessObjects.Length > 0)
			{
				var lineChargeRequiredFetchForView = false;
				foreach (var column in columns)
				{
					lineChargeRequiredFetchForView = lineChargeRequiredFetchForView || IsLineChargeRelatedColumn(column.ColumnName);
				}

				if (lineChargeRequiredFetchForView)
				{
					Collection.Factory.AddFetchHint(JobComInvoiceHeaderSchema.Instance, new ZQuery(JobComInvoiceHeaderSchema.JZ_GroupInvoice, 0));
					Collection.Factory.AddFetchHint(JobComInvHeaderChargeSchema.Instance, new ZQuery(JobComInvHeaderChargeSchema.J7_ParentID, businessObjects.Select(bo => bo.PK)));
					Collection.Factory.AddFetchHint(JobComInvoiceLineSchema.Instance, new ZQuery(JobComInvoiceLineSchema.JI_JZ, businessObjects.Select(bo => bo.PK)));

					foreach (BaseJobComInvoiceHeader header in businessObjects)
					{
						if (AdditionalInvoiceLineEntryLineLinkSupporter.DoesSupport(header.CountryCode))
						{
							Collection.Factory.AddFetchHint(CusUnderbondDecSchema.BU_ClusterKey, header.JZ_ClusterKey);
						}
					}

					foreach (BaseJobComInvoiceHeader header in businessObjects)
					{
						foreach (var line in header.InvoiceLines)
						{
							header.Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, line.PK);
						}
					}
				}
			}

			base.FetchForViewCore(businessObjects, columns);
		}

		bool IsLineChargeRelatedColumn(string columnName)
		{
			return columnName == BaseJobComInvoiceHeader.Schema.JZ_Calc_TNI
					|| columnName == BaseJobComInvoiceHeader.Schema.InvoiceLineTotal
					|| columnName == BaseJobComInvoiceHeader.Schema.JZ_Calc_Balance
					|| columnName == BaseJobComInvoiceHeader.Schema.JZ_Calc_BalanceString
					|| columnName == BaseJobComInvoiceHeader.Schema.JZ_Calc_ChargesExcludedFromITOT
					|| columnName == BaseJobComInvoiceHeader.Schema.JZ_Calc_CIFAmount
					|| columnName == BaseJobComInvoiceHeader.Schema.JZ_Calc_FOBAmount
					|| columnName == BaseJobComInvoiceHeader.Schema.JZ_Calc_LinesEntered;
		}
	}
}
