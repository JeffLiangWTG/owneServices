using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public static class WhsOrderPackageDataObjectHelper
	{
		public static UniversalShipment GetWhsOrderPackageDataObject(IDataWritingManager writeManager, WhsOrder order, ExcludeElement elementsToExclude, Dictionary<ZGuid, ZInt> orderLineDictionary = null)
		{
			// For the Single Package Export we want to remove all the Child Collections since
			// they are not relevant, in future this will be done via Universal XML filtering.
			var writer = new WhsOrderDataObjectWriter(writeManager, elementsToExclude, orderLineDictionary);
			var orderDataObject = writer.GetDataObject(order);
			var addressTypesToFind = new List<string>()
			{
				// The customs Warehouse Address is just a repeat of the Warehouse Address (already stored as 'ConsignorPickupDeliveryAddress')
				nameof(DocAddressType.CustomsWarehouseAddress),
				// The Carrier Booking Agent Address is not necessary when sending to the Carrier Booking Agent
				nameof(DocAddressType.CarrierBookingAgent),
			};

			// remove all addresses of the types we are searching, stopping as soon as all are found
			for (int index = orderDataObject.OrganizationAddressCollection.Count - 1; index >= 0 && addressTypesToFind.Count > 0; index--)
			{
				var address = orderDataObject.OrganizationAddressCollection[index];
				var type = address.AddressType.GetValueOrDefault();
				if (addressTypesToFind.Contains(type))
				{
					addressTypesToFind.Remove(type);
					orderDataObject.OrganizationAddressCollection.RemoveAt(index);
				}
			}

			return orderDataObject;
		}
	}
}
