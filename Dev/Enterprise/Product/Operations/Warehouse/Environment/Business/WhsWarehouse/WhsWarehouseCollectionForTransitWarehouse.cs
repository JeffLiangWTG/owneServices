using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsWarehouseCollectionForTransitWarehouse : WhsWarehouseCollection, IWhsWarehouseCollectionForTransitWarehouse
	{
		#region Constructor 

		public WhsWarehouseCollectionForTransitWarehouse(BusinessObjectFactory factory)
			: base(factory, WarehouseCollectionType.TransitWarehouse)
		{
		}

		#endregion

		#region CreateAdditionalFilter

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery filter = base.CreateAdditionalFilter();
			filter.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, SQLComparisonOperator.Equal, WarehouseTypes.Codes.Transit);
			filter.AddToFilter(WhsWarehouseSchema.WW_GB_RelatedCompanyBranch, SQLComparisonOperator.Equal, GlbBranch.CurrentBranch.PK);
			return filter;
		}

		#endregion

		#region AddNotificationWhenAdditionalFilterNotMet

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			var whs = (WhsWarehouse)selectedBusinessObject;

			if (whs.WW_WarehouseType != WarehouseTypes.Codes.Transit)
			{
				errors.Add(Res.GetString("C12EE0B9-FE67-40E7-9248-C629E322C3FB", "Only Transit Warehouses may be selected."));
			}

			if (whs.WW_GB_RelatedCompanyBranch != GlbBranch.CurrentBranch.PK)
			{
				errors.Add(Res.GetString("73E2057C-51EE-4416-9E1D-C96BD0A27004", "This Transit Warehouse cannot be chosen at this time, log into the correct branch to select this warehouse."));
			}
		}

		#endregion

	}
}
