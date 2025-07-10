
using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickLookups : AutoWhsPickLookups
	{
		public WhsPickLookups(AutoWhsPick parent)
			: base(parent)
		{
		}

		public WhsWarehouseCollection Warehouses
		{
			get { return new WhsWarehouseCollectionWithSecurityCheck(Factory); }
		}

		public WhsOrderCollectionForPicking OrderFindBoxList
		{
			get
			{
				var result = new WhsOrderCollectionForPicking(Factory, Parent);
				if (!Parent.WP_WW_Whs.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Warehouse", "Property", Parent.WP_WW_Whs));
				}

				if (!Parent.WP_PickOption.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Pick Option", "Property", Parent.WP_PickOption));
				}

				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Order Status", "Property", (ZString)DocketStatus.Codes.Entered));
				return result;
			}
		}

		public virtual ConsigneeCollection Consignees
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public virtual WhsPickOption PickOptions
		{
			get { return new WhsPickOption(); }
		}

		#region DockDoorLocations

		public WhsLocationCollection DockDoorLocations
		{
			get
			{
				return Factory.GetCachedValue("WhsWarehouseLookups|DockDoorLocations|" + Parent.WP_WW_Whs, () =>
				{
					return Parent.WP_WW_Whs.IsValid
						? new WhsLocationCollection(Parent.Warehouse, dockDoorLocationsOnly: true)
						: new WhsLocationCollection(Factory, dockDoorLocationsOnly: true);
				});
			}
		}

		#endregion

		#region PackingStationLocations

		public WhsLocationCollection PackingStationLocations
		{
			get
			{
				return Factory.GetCachedValue("WhsWarehouseLookups|PackingStationLocations|" + Parent.WP_WW_Whs, () =>
				{
					var locationCollection = Parent.WP_WW_Whs.IsValid
							? new WhsLocationCollection(Parent.Warehouse)
							: new WhsLocationCollection(Factory);

					locationCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(WhsLocationCollection.FilterSchema.PackingStationLocation, "Property0", ZBool.True));
					return locationCollection;
				});
			}
		}

		#endregion

		#region DynamicPickAreas

		public WhsDynamicAreaCollection DynamicPickAreas
			=> Factory.GetCachedValue("WhsWarehouseLookups|DynamicPickAreas|" + Parent.WP_WW_Whs, () => WhsDynamicAreaCollection.GetDynamicPickingAreas(Factory, Parent.Warehouse));

		#endregion

		new WhsPick Parent
		{
			get { return (WhsPick)base.Parent; }
		}
	}
}
