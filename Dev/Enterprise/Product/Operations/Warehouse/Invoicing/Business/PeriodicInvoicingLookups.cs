using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Invoicing.Business
{
	public class PeriodicInvoicingLookups : JobStorageLookups
	{
		public PeriodicInvoicingLookups(PeriodicInvoicing parent)
			: base(parent) { }

		#region Client

		public override OrgHeaderCollection Clients
		{
			get { return new WarehouseClientCollectionWithSecurityCheck(Factory); }
		}

		#endregion

		#region Warehouses

		public WhsWarehouseCollection Warehouses
		{
			get {
				var storageType = ((PeriodicInvoicing)Parent).ET_StorageType;
				var cacheKey = "PeriodicInvoicingLookupsWarehouses|" + storageType;
				var collectionType = GetWhsCollectionType(storageType);
				return Factory.GetCachedValue(cacheKey, () => GetWhsCollection(collectionType));
			}
		}

		WhsWarehouseCollectionWithSecurityCheck GetWhsCollection(WarehouseCollectionType collectionType)
		{
			var warehouses = new WhsWarehouseCollectionWithSecurityCheck(Factory, collectionType);
			warehouses.Load();
			return warehouses;
		}

		WarehouseCollectionType GetWhsCollectionType(string storageType)
		{
			return storageType switch
			{
				PeriodicInvoicingStorageTypes.Codes.ContainerYard => WarehouseCollectionType.CYDWarehouse,
				PeriodicInvoicingStorageTypes.Codes.TransitWarehouse => WarehouseCollectionType.TransitWarehouse,
				_ => WarehouseCollectionType.ProductWarehouse,
			};
		}

		#endregion

		#region OffBandProcessingStatus

		public StorageOffBandProcessingStatus OffBandProcessingStatus => Factory.GetCachedValue<StorageOffBandProcessingStatus>();

		#endregion
	}
}
