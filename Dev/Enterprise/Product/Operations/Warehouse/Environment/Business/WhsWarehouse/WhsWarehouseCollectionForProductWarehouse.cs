using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsWarehouseCollectionForProductWarehouse : WhsWarehouseCollectionWithSecurityCheck
	{
		#region Constructor 

		public WhsWarehouseCollectionForProductWarehouse(BusinessObjectFactory factory)
			: base(factory, WarehouseCollectionType.ProductWarehouse)
		{
		}

		#endregion

		#region CreateAdditionalFilter

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery filter = base.CreateAdditionalFilter();
			filter.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, SQLComparisonOperator.Equal, WarehouseTypes.Codes.Product);
			return filter;
		}

		#endregion

		#region AddNotificationWhenAdditionalFilterNotMet

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			var whs = (WhsWarehouse)selectedBusinessObject;

			if (whs.WW_WarehouseType != WarehouseTypes.Codes.Product)
			{
				errors.Add(Res.GetString("a45b1ed8-34de-470d-b564-7c089a56c040", "Only Product Warehouses may be selected."));
			}
		}

		#endregion

	}
}
