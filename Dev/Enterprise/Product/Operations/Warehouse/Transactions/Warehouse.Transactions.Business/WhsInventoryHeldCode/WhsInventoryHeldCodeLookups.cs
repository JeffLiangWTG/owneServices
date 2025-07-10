using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsInventoryHeldCodeLookups : AutoWhsInventoryHeldCodeLookups
	{
		public WhsInventoryHeldCodeLookups(AutoWhsInventoryHeldCode parent)
			: base(parent)
		{
		}

		#region Clients

		public override OrgHeaderCollection Clients
			=> Factory.GetCachedValue("WhsInventoryHeldCodeLookups|Clients", () => new WarehouseClientCollectionWithSecurityCheck(Factory));

		#endregion
	}
}
