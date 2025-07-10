using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsVASOrderLineFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsVASOrderLineFetchStrategy(WhsVASOrderLine vasOrderLine)
			: base(vasOrderLine)
		{
		}

		#region FetchForViewCore

		// Tested in WhsVASOrderLineTest.cs
		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(VASOrderline.Product):
					case nameof(VASOrderline.ProductDescription):
						Factory.AddFetchHint(OrgSupplierPartSchema.PK, VASOrderline.WVL_OP_Product);
						break;

					case nameof(VASOrderline.VASOrder):
						Factory.AddFetchHint(WhsVASOrderSchema.PK, VASOrderline.WVL_WVO_VASOrder);
						break;

					default:
						break;
				}
			}
		}

		#endregion

		#region Implementation

		WhsVASOrderLine VASOrderline
		{
			get { return (WhsVASOrderLine)BusinessObject; }
		}

		#endregion
	}
}
