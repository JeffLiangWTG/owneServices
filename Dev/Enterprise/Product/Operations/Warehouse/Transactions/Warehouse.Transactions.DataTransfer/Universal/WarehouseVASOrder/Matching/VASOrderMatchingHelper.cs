using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public static class VASOrderMatchingHelper
	{
		public static WhsVASOrder GetMatchingVASOrder(UniversalObjectFactory factory, Shipment vasOrderDataObject, ZGuid clientPK)
		{
			WhsVASOrder result = null;

			if (clientPK.IsValid && vasOrderDataObject != null && vasOrderDataObject.Order != null)
			{
				var customerReferenceNumber = vasOrderDataObject.Order.OrderNumber.GetValueOrDefault();
				if (!customerReferenceNumber.IsEmpty)
				{
					var query = new ZQuery();
					query.AddToFilter(WhsVASOrderSchema.WVO_CustomerReferenceNo, vasOrderDataObject.Order.OrderNumber);
					query.AddToFilter(WhsVASOrderSchema.WVO_OH_Client, clientPK);

					result = factory.LoadTop1<WhsVASOrder>(query); // VAS Order is unique by Customer Ref and Client.
				}
			}

			return result;
		}
	}
}
