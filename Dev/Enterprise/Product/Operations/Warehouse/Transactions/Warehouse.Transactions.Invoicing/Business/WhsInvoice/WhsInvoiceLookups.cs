using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	public class WhsInvoiceLookups : JobStorageLookups
	{
		public WhsInvoiceLookups(WhsInvoice parent)
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
			get { return Factory.GetCachedValue("WhsProductParamsByWhsAndClientLookups|StockTakeCycles", () => GetWhsCollection()); }
		}

		WhsWarehouseCollectionWithSecurityCheck GetWhsCollection()
		{
			var warehouses = new WhsWarehouseCollectionWithSecurityCheck(Factory);
			warehouses.Load();
			return warehouses;
		}

		#endregion

		#region OffBandProcessingStatus

		public StorageOffBandProcessingStatus OffBandProcessingStatus => Factory.GetCachedValue<StorageOffBandProcessingStatus>();

		#endregion
	}
}
