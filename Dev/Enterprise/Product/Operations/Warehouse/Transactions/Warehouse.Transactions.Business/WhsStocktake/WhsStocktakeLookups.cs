using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsStocktakeLookups : AutoWhsStocktakeLookups
	{
		public WhsStocktakeLookups(AutoWhsStocktake parent)
			: base(parent)
		{
		}

		new WhsStocktake Parent
		{
			get { return (WhsStocktake)base.Parent; }
		}

		#region ABCAnalysisCategories

		public ICodeDescriptionPairList ABCAnalysisCategories
		{
			get { return WarehouseDataRegistry.Instance.ABCAnalysisCategories.Value; }
		}

		#endregion

		#region CountEmptyLocations

		public ICodeDescriptionPairList CountEmptyLocationCategory
		{
			get { return new CountEmptyLocationCategory(); }
		}

		#endregion

		#region Warehouses

		public WhsWarehouseCollection Warehouses
		{
			get { return new WhsWarehouseCollectionWithSecurityCheck(Factory); }
		}

		#endregion

		#region Clients

		public override OrgHeaderCollection Clients
		{
			get { return new WarehouseClientCollectionWithSecurityCheck(Factory); }
		}

		#endregion

		#region SupplierParts

		public OrgSupplierPartCollection SupplierParts
		{
			get
			{
				var parent = Parent;

				var result = new WhsOrgSupplierPartCollection(Factory, null, parent.Client, false);
				if (parent.Client != null)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property1", parent.WS_OH_Client));
				}

				return result;
			}
		}

		#endregion

		#region Rows

		public WhsRowCollection Rows
		{
			get
			{
				var warehouse = Parent.Warehouse;
				return warehouse != null ? warehouse.Rows : new WhsRowCollection(null, Factory);
			}
		}

		#endregion

		#region Areas

		public WhsAreaCollection Areas
		{
			get
			{
				var warehouse = Parent.Warehouse;
				return warehouse != null ? warehouse.Areas : new WhsAreaCollection(Factory, warehouse); // null warehouse will give us an empty areas collection
			}
		}

		#endregion

		#region PickMethods

		public ICodeDescriptionPairListWithDefaultCode PickMethods
		{
			get { return WarehouseDataRegistry.Instance.PickMethod.Value; }
		}

		#endregion

		#region StocktakCycles

		public ReadOnlyCodeDescriptionPairList StockTakeCycles
		{
			get { return Factory.GetCachedValue("WhsStocktakeLookups|StockTakeCycles", () => WarehouseDataRegistry.Instance.StocktakeCycle.Value); }
		}

		#endregion

		#region Locations

		public WhsLocationCollection Locations
		{
			get
			{
				var warehouse = Parent.Warehouse;
				return (warehouse != null) ? new WhsLocationCollection(warehouse) : new WhsLocationCollection(Factory);
			}
		}

		#endregion

		#region StocktakeTypes

		public CodeDescriptionPairList StocktakeTypes
		{
			get { return Factory.GetCachedValue("WhsStocktakeLookups|StocktakeTypes" + Parent.WS_StocktakeStatus, GetStocktakeTypes); }
		}

		CodeDescriptionPairList GetStocktakeTypes()
		{
			var stocktakeTypes = WarehouseDataRegistry.Instance.StocktakeTypes.Value.GetCodeDescriptionPairList();

			var stocktakeStatus = Parent.WS_StocktakeStatus;
			if (stocktakeStatus == StocktakeStatus.Codes.New)
			{
				stocktakeTypes.RemoveCode("ATC");
				stocktakeTypes.RemoveCode("AZC");
			}

			return stocktakeTypes;
		}

		#endregion
	}
}
