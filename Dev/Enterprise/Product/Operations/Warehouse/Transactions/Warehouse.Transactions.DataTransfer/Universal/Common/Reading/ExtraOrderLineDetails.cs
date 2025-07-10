using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class ExtraOrderLineDetails
	{
		public ExtraOrderLineDetails(
			OrganizationAddress supplierAddress,
			IEnumerable<ZString> extraClassificationDetails,
			IEnumerable<CustomsDetail> extraCustomsDetails,
			ZString? newProductCode,
			ZString? newPartAttribute1,
			ZString? newPartAttribute2,
			ZString? newPartAttribute3,
			ZString? newSerialNumber,
			ZString? allocationKey)
		{
			SupplierAddress = supplierAddress;
			ExtraClassificationDetails = extraClassificationDetails;
			ExtraCustomsDetails = extraCustomsDetails;
			NewProductCode = newProductCode;
			NewPartAttribute1 = newPartAttribute1;
			NewPartAttribute2 = newPartAttribute2;
			NewPartAttribute3 = newPartAttribute3;
			NewSerialNumber = newSerialNumber;

			AllocationKey = allocationKey;
		}

		public readonly OrganizationAddress SupplierAddress;
		public readonly IEnumerable<ZString> ExtraClassificationDetails;
		public readonly IEnumerable<CustomsDetail> ExtraCustomsDetails;
		public readonly ZString? NewProductCode;
		public readonly ZString? NewPartAttribute1;
		public readonly ZString? NewPartAttribute2;
		public readonly ZString? NewPartAttribute3;
		public readonly ZString? NewSerialNumber;
		public readonly ZString? AllocationKey;

		public class CustomsDetail
		{
			public CustomsDetail(ZString type, ZString data)
			{
				Type = type;
				Data = data;
			}

			public readonly ZString Type;
			public readonly ZString Data;
		}
	}
}
