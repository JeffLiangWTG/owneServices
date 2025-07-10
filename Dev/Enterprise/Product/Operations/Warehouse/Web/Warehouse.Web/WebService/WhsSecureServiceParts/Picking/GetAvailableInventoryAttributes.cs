using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetAvailableInventoryAttributes

		[WebMethod(Description = "Get Available Inventory Attributes")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsInventoryCollectionWebServiceResponse GetAvailableInventoryAttributes(WhsPickLineInfo pickLineInfo, bool isAttributeNeutral)
		{
			return HandleWebServiceRequest<WhsInventoryCollectionWebServiceResponse>(response => GetAvailableInventoryAttributes(response, pickLineInfo, isAttributeNeutral));
		}

		void GetAvailableInventoryAttributes(WhsInventoryCollectionWebServiceResponse response, WhsPickLineInfo pickLineInfo, bool isAttributeNeutral)
		{
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			if (response.ValidateShouldNotBeNull(warehouse, nameof(warehouse)))
			{
				var query = GetAvailableInventoryAttributesQuery(warehouse, pickLineInfo, isAttributeNeutral);
				var inventoryCollection = WhsInventoryLoader.LoadWhsInventory(Factory, query);
				if (inventoryCollection.Length > 0)
				{
					var client = inventoryCollection[0].Client;
					var product = WhsProduct.GetWhsProduct(inventoryCollection[0].SupplierPart);
					AddInventorySerialNumbers(response, pickLineInfo, client, product, inventoryCollection);
				}

				response.TotalCount = response.AvailableInventorySerialNumbers.Count;
			}
		}

		void AddInventorySerialNumbers(WhsInventoryCollectionWebServiceResponse response, WhsPickLineInfo pickLineInfo, OrgHeader client, WhsProduct product, WhsInventoryView[] inventoryCollection)
		{
			var isSerialUsed = product.IsSerialNumberUsed(client);
			if (isSerialUsed)
			{
				foreach (var inventory in inventoryCollection)
				{
					if (inventory.WI_AvailableToPickQuantity > 0m || InventoryHasStockAndIsNotBeingTransacted(pickLineInfo, inventory))
					{
						response.AvailableInventorySerialNumbers.Add(inventory.WI_SerialNumber);
					}
				}
			}
			else
			{
				throw new ArgumentException("Only inventory with Serial Number Attributes are supported.");
			}
		}

		bool InventoryHasStockAndIsNotBeingTransacted(WhsPickLineInfo pickLineInfo, WhsInventoryView inventory)
		{
			var result = IsSwappable(inventory, inventory.CommittedPickLines.SingleOrDefault()) && inventory.ReservedPickLines.Count == 0; // swappable but not reserved

			if (result)
			{
				// exclude the inventory that is actually allocated to current pick line info
				result = pickLineInfo.SerialNumber != inventory.WI_SerialNumber;
			}

			return result;
		}

		ZDBOnlyQuery GetAvailableInventoryAttributesQuery(WhsWarehouse warehouse, WhsPickLineInfo pickLineInfo, bool isAttributeNeutral)
		{
			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));

			if (pickLineInfo.ClientPK != Guid.Empty)
			{
				query.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, pickLineInfo.ClientPK);
			}

			if (pickLineInfo.ProductPK != Guid.Empty)
			{
				query.AddToFilter(WhsInventoryViewSchema.WI_OP, pickLineInfo.ProductPK);
			}

			if (!string.IsNullOrEmpty(pickLineInfo.PalletID))
			{
				query.AddToFilter(WhsInventoryViewSchema.WI_PalletID, pickLineInfo.PalletID);
				query.AddToFilter(WhsInventoryViewSchema.WI_PalletID, SQLComparisonOperator.NotEqual, string.Empty);
			}

			if (!string.IsNullOrEmpty(pickLineInfo.Location))
			{
				var location = WebServiceHelper.GetLocationByLocationString(Factory, warehouse, pickLineInfo.Location);
				if (location != null)
				{
					query.AddToFilter(WhsInventoryViewSchema.WI_WL, location.PK);
				}
			}

			if (isAttributeNeutral)
			{
				if (!string.IsNullOrEmpty(pickLineInfo.OrderedPartAttribute1))
				{
					query.AddToFilter(WhsInventoryViewSchema.WI_PartAttrib1, pickLineInfo.OrderedPartAttribute1);
				}

				if (!string.IsNullOrEmpty(pickLineInfo.OrderedPartAttribute2))
				{
					query.AddToFilter(WhsInventoryViewSchema.WI_PartAttrib2, pickLineInfo.OrderedPartAttribute2);
				}

				if (!string.IsNullOrEmpty(pickLineInfo.OrderedPartAttribute3))
				{
					query.AddToFilter(WhsInventoryViewSchema.WI_PartAttrib3, pickLineInfo.OrderedPartAttribute3);
				}

				if (!string.IsNullOrEmpty(pickLineInfo.OrderedSerialNumber))
				{
					query.AddToFilter(WhsInventoryViewSchema.WI_SerialNumber, pickLineInfo.OrderedSerialNumber);
				}

				if (pickLineInfo.OrderedExpiryDate != DateTime.MinValue)
				{
					query.AddToFilter(WhsInventoryViewSchema.WI_ExpiryDate, new ZDateTime(pickLineInfo.OrderedExpiryDate));
				}

				if (pickLineInfo.OrderedPackingDate != DateTime.MinValue)
				{
					query.AddToFilter(WhsInventoryViewSchema.WI_PackingDate, new ZDateTime(pickLineInfo.OrderedPackingDate));
				}
			}

			query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);
			query.ReLoadExistingRows = true;

			return query;
		}

		#endregion
	}
}
