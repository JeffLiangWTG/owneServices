using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsClientParameterByWarehouseLookups : AutoWhsClientParameterByWarehouseLookups
	{
		public WhsClientParameterByWarehouseLookups(AutoWhsClientParameterByWarehouse parent)
			: base(parent)
		{
		}

		#region Parent

		public new WhsClientParameterByWarehouse Parent
		{
			get { return (WhsClientParameterByWarehouse)base.Parent; }
		}

		#endregion

		#region Organisations

		public virtual OrgHeaderCollection Organisations
		{
			get { return Factory.GetCachedValue("WhsClientParameterByWarehouseLookups|Organisations", () => new WarehouseClientCollection(Factory)); }
		}

		#endregion

		#region Warehouses

		public WhsWarehouseCollection Warehouses
		{
			get { return Factory.GetCachedValue("WhsClientParameterByWarehouseLookups|Warehouses", () => new WhsWarehouseCollection(Factory)); }
		}

		#endregion

		#region Areas

		public WhsAreaCollection PutawayAreas
		{
			get
			{
				var result = WhsAreaCollection.GetPutawayAreas(Factory, Parent.WY_WW_Whs);

				if (result.FilterBusinessObjectDefaults.ContainsDefaultFor("Warehouse:Property"))
				{
					if (Parent.WY_WW_Whs.IsEmpty)
					{
						result.FilterBusinessObjectDefaults.Remove("Warehouse:Property");
					}
				}
				else if (!Parent.WY_WW_Whs.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Warehouse", "Property", Parent.WY_WW_Whs));
				}

				return result;
			}
		}

		#endregion

		#region ReceiveCategories

		public ICodeDescriptionPairListWithDefaultCode ReceiveCategories
		{
			get { return WarehouseDataRegistry.Instance.ReceiveCategories.Value; }
		}

		#endregion
	}
}
