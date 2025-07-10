using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsGroupedInventoryLineInfoCollectionTestCase : WhsInventoryLineBaseInfoCollectionTestCase<WhsGroupedInventoryInfo>
	{
		protected override WhsInventoryLineBaseInfoCollection<WhsGroupedInventoryInfo> GetCollection()
		{
			return new WhsGroupedInventoryLineInfoCollection();
		}

		protected override WhsGroupedInventoryInfo GetInventoryLineInfo(WhsInventoryLineBaseInfoCollection<WhsGroupedInventoryInfo> collection, WhsInventoryView inventory)
		{
			return new WhsGroupedInventoryInfo(collection, inventory);
		}

		protected override WhsGroupedInventoryInfo GetInventoryLineInfo()
		{
			return new WhsGroupedInventoryInfo();
		}
	}
}
