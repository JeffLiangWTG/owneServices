using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsGroupedInventoryCollectionWebServiceResponse : WebServiceResponse
	{
		public WhsGroupedInventoryCollectionWebServiceResponse()
			: base()
		{
		}

		#region GroupedInventoryInfoCollection

		public WhsGroupedInventoryLineInfoCollection InventoryInfoCollection
		{
			get { return inventoryInfoCollection ?? (inventoryInfoCollection = new WhsGroupedInventoryLineInfoCollection()); }
			set { inventoryInfoCollection = value; }
		}

		WhsGroupedInventoryLineInfoCollection inventoryInfoCollection;

		#endregion

		#region TotalCount

		public int TotalCount { get; set; }

		#endregion

		#region CriteriaInfo

		public WhsInventorySearchCriteriaInfo CriteriaInfo
		{
			get { return criteriaInfo ?? (criteriaInfo = new WhsInventorySearchCriteriaInfo()); }
			set { criteriaInfo = value; }
		}

		WhsInventorySearchCriteriaInfo criteriaInfo;

		#endregion
	}
}
