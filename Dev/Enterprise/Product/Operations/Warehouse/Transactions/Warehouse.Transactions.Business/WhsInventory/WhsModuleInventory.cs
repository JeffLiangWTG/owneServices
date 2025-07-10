using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	/// <summary>
	/// Used to Separate Grid Colour Schemes from the Module Inventory Grid and the Receive Inventory Grid
	/// </summary>
	public class WhsModuleInventory : WhsInventoryView
	{
		public WhsModuleInventory(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsModuleInventoryFetchStrategy(this);
		}

		#endregion
	}
}
