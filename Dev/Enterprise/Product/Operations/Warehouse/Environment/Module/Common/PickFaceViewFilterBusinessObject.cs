using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Module
{
	public abstract class PickFaceViewFilterBusinessObject : FilterStripBusinessObject
	{
		protected PickFaceViewFilterBusinessObject()
		{
		}

		public abstract class Schema
		{
			public readonly static string Client = nameof(Client);
			public readonly static string Warehouse = nameof(Warehouse);
			public readonly static string Location = nameof(Location);
			public readonly static string Product = nameof(Product);
			public readonly static string ABCCategory = nameof(ABCCategory);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filterCollection = new ModuleFilterCollection();
			AddWarehouseFilter(filterCollection);
			AddLocationFilter(filterCollection);
			AddClientFilter(filterCollection);
			AddProductFilter(filterCollection);
			AddAbcCategoryFilter(filterCollection);

			return filterCollection;
		}

		void AddWarehouseFilter(ModuleFilterCollection filters)
		{
			var collection = new WhsWarehouseCollectionWithSecurityCheck(Factory);
			var warehouseFilter = GetNewWarehouseFilter(filters, Schema.Warehouse, collection);
			warehouseFilter.MultilingualDescription = ResString.GetMultilingualString("a47d76b6-9b0c-4ae2-8e8b-b59c558834b4", "Warehouse");
			warehouseFilter.Visibility = FilterVisibility.AlwaysVisible;

			warehouseFilter.PropertyValidation = info =>
			{
				if (warehouseFilter.Property.IsEmpty)
				{
					info.AddError(ResString.GetMultilingualString("f9717d40-baa5-42e9-bdf3-3e63c9c0cdc5", "Warehouse is required."));
				}
			};

			warehouseFilter.PropertyInfo.ValueChanged += (s, e) =>
			{
				LocationCollection = null;
				LocationFilter.Property = ZGuid.Empty;
			};
		}

		protected abstract ModuleGuidFilter GetNewWarehouseFilter(ModuleFilterCollection filters, string description, WhsWarehouseCollectionWithSecurityCheck collection);

		void AddLocationFilter(ModuleFilterCollection filters)
		{
			var locationFilter = filters.AddGuidFilter(Schema.Location, ModuleIDs.WhsConfigLocation, LocationColumn, GetLocationCollection);
			locationFilter.MultilingualDescription = ResString.GetMultilingualString("aace93d4-bcbb-4ae7-b720-48a8d31e075f", "Location");
			locationFilter.PropertyValidation = info =>
			{
				if (IsParentFilterInvalid(WarehouseFilter, LocationFilter))
				{
					info.AddError(ResString.GetMultilingualString("357a1665-0806-4c3c-a682-3d13d8b48740", "Location can not be entered without a Warehouse Code."));
				}
			};
		}

		protected abstract SchemaGuidColumn LocationColumn { get; }

		void AddClientFilter(ModuleFilterCollection filters)
		{
			var collection = new WarehouseClientCollectionWithSecurityCheck(Factory);
			var clientFilter = GetNewClientFilter(filters, Schema.Client, collection);
			clientFilter.MultilingualDescription = ResString.GetMultilingualString("59995e1c-aff4-4c07-9db0-58b8af18b8d7", "Client");

			clientFilter.PropertyInfo.ValueChanged += (s, e) =>
			{
				ProductCollection = null;
				ProductFilter.Property = ZGuid.Empty;
			};
		}

		protected abstract ModuleGuidFilter GetNewClientFilter(ModuleFilterCollection filters, string description, WarehouseClientCollectionWithSecurityCheck collection);

		void AddProductFilter(ModuleFilterCollection filters)
		{
			var productFilter = filters.AddGuidFilter(Schema.Product, ModuleIDs.SupplierPart, ProductColumn, GetProductCollection);
			productFilter.MultilingualDescription = ResString.GetMultilingualString("543d3a89-248c-4e97-9da6-d0c8b73ebfb1", "Product");

			productFilter.PropertyValidation = info =>
			{
				if (IsParentFilterInvalid(ClientFilter, ProductFilter))
				{
					info.AddError(ResString.GetMultilingualString("9d991b75-9320-4bab-b1ff-36d4d5d6bca3", "Product Code can not be entered without a Client Code."));
				}
			};
		}

		protected abstract SchemaGuidColumn ProductColumn { get; }

		void AddAbcCategoryFilter(ModuleFilterCollection filters)
		{
			var abcCategoryFilter = filters.AddTextFilter(Schema.ABCCategory, ABCCategoryColumn, WarehouseDataRegistry.Instance.ABCAnalysisCategories.Value);
			abcCategoryFilter.MultilingualDescription = ResString.GetMultilingualString("7896E15A-9115-46F9-BDAE-684C72AF3792", "ABC Category");
			abcCategoryFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			abcCategoryFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			abcCategoryFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			abcCategoryFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
		}

		protected abstract SchemaStringColumn ABCCategoryColumn { get; }

		protected static bool IsParentFilterInvalid(ModuleGuidFilter parent, ModuleGuidFilter child)
		{
			return !child.Property.IsEmpty &&
				(!parent.IsActive || parent.Property.IsEmpty || !parent.Property.IsValid);
		}

		protected ModuleGuidFilter WarehouseFilter => (ModuleGuidFilter)ModuleFilters[Schema.Warehouse];
		ModuleGuidFilter ClientFilter => (ModuleGuidFilter)ModuleFilters[Schema.Client];
		ModuleGuidFilter ProductFilter => (ModuleGuidFilter)ModuleFilters[Schema.Product];
		ModuleGuidFilter LocationFilter => (ModuleGuidFilter)ModuleFilters[Schema.Location];

		WhsLocationCollection GetLocationCollection()
		{
			if (LocationCollection == null)
			{
				var filter = WarehouseFilter;
				var warehouse = filter.IsActive ? Factory.Load<WhsWarehouse>(filter.Property) : null;
				if (warehouse != null)
				{
					LocationCollection = new WhsLocationCollection(warehouse);
				}
			}
			return LocationCollection;
		}

		WhsLocationCollection LocationCollection;

		OrgSupplierPartCollection GetProductCollection()
		{
			if (ProductCollection == null)
			{
				var filter = ClientFilter;
				var client = filter.IsActive ? Factory.Load<OrgHeader>(filter.Property) : null;
				if (client != null)
				{
					ProductCollection = new OrgSupplierPartCollection(Factory, null, client, false);
				}
			}
			return ProductCollection;
		}

		OrgSupplierPartCollection ProductCollection;
	}
}
