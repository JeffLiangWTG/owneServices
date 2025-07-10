using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class CusEntryHeaderCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public CusEntryHeaderCollectionFetchStrategy(ICusEntryHeaderCollection<CusEntryHeader> collection)
			: base(collection)
		{
		}

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			if (businessObjects.Length > 0)
			{
				var cusEntryLineFeeRequiredFetchForView = false;
				foreach (var column in columns)
				{
					cusEntryLineFeeRequiredFetchForView = cusEntryLineFeeRequiredFetchForView || IsCusEntryLineFeeRelatedColumn(column.ColumnName);
				}

				if (cusEntryLineFeeRequiredFetchForView)
				{
					Collection.Factory.AddFetchHint(CusEntryLineSchema.Instance, new ZQuery(CusEntryLineSchema.CL_CH, businessObjects.Select(bo => bo.PK)));

					foreach (CusEntryHeader entryHeader in businessObjects)
					{
						foreach (var line in entryHeader.AllEntryLines)
						{
							entryHeader.Factory.AddFetchHint(CusEntryLineFeeSchema.CF_CL, line.PK);
						}
					}
				}
			}

			base.FetchForViewCore(businessObjects, columns);
		}

		bool IsCusEntryLineFeeRelatedColumn(string columnName)
		{
			return columnName == CusEntryHeader.Schema.GSTAmount;
		}
	}
}
