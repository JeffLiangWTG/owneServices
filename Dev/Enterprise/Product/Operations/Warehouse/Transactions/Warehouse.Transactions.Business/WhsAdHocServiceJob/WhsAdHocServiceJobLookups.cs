using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdHocServiceJobLookups : AutoWhsAdHocServiceJobLookups
	{
		public WhsAdHocServiceJobLookups(AutoWhsAdHocServiceJob parent)
			: base(parent)
		{
		}

		#region Parent

		protected new WhsAdHocServiceJob Parent
		{
			get { return (WhsAdHocServiceJob)base.Parent; }
		}

		#endregion

		#region Warehouses

		public WhsWarehouseCollection Warehouses
		{
			get { return Factory.GetCachedValue("WhsAdHocServiceJobLookups|Warehouses", () => new WhsWarehouseCollectionForProductWarehouse(Factory)); }
		}

		#endregion

		#region Clients

		public override OrgHeaderCollection Clients
		{
			get { return Factory.GetCachedValue("WhsAdHocServiceJobLookups|Clients", () => new WarehouseClientCollectionWithSecurityCheck(Factory)); }
		}

		#endregion
	}
}
