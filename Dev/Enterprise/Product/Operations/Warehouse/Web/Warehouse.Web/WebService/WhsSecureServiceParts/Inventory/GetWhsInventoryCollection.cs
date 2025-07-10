using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetWhsInventoryCollection

		[WebMethod(Description = "Get (Grouped) Inventory Collection by Criteria")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsGroupedInventoryCollectionWebServiceResponse GetWhsInventoryCollection(WhsInventorySearchCriteriaInfo criteriaInfo)
		{
			return HandleWebServiceRequest<WhsGroupedInventoryCollectionWebServiceResponse>(response => GetWhsInventoryCollection(response, criteriaInfo));
		}

		void GetWhsInventoryCollection(WhsGroupedInventoryCollectionWebServiceResponse response, WhsInventorySearchCriteriaInfo criteriaInfo)
		{
			const int firstInventoriesCount = 500;

			var inventoryLoader = new WhsInventoryLoader(Factory, SecurityHeader.WarehouseCode);
			var inventories = inventoryLoader.LoadWhsInventory(criteriaInfo);
			if (inventories != null && inventories.Any())
			{
				criteriaInfo.UpdateInventoryLevel(inventories.First());

				var whs = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
				var groupedInventoryCollection = new WhsGroupedInventoryInfoCollection(criteriaInfo.DestinationInventoryLevel, inventories, whs, firstInventoriesCount);
				response.InventoryInfoCollection = groupedInventoryCollection.InventoryLineInfoCollection;
				response.TotalCount = groupedInventoryCollection.TotalCount;
				response.CriteriaInfo = criteriaInfo;
			}
		}

		#endregion
	}
}
