using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public WhsPickCollectionFetchStrategy(IBusinessObjectCollection collection)
			: base(collection)
		{
		}

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			base.FetchForViewCore(businessObjects, columns);
			var columnsThatRequireLoadingOrderDocAddresses = new[]
			{
				nameof(WhsPick.TransportCodes),
				nameof(WhsPick.ConsigneeCodes),
				nameof(WhsPick.ConsigneeNames),
				nameof(WhsPick.DistributionCentreCodes),
			};

			if (columns.Any(c => columnsThatRequireLoadingOrderDocAddresses.Contains(c.ColumnName)))
			{
				Collection.Factory.AddFetchHint(WhsDocketSchema.Instance, new ZQuery(WhsDocketSchema.WD_WP, businessObjects.Select(p => p.PK).ToArray()));
			}
		}
	}
}
