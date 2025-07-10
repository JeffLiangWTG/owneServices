using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Matching
{
	public static class AddressTypeMatchHelper
	{
		public static ZString[] GetCarrierAddressTypesInPreferredOrder()
		{
			return new ZString[]
			{
				nameof(DocAddressType.Carrier),
				nameof(DocAddressType.ShippingLineAddress),
				AddressTypes.ShippingLine
			};
		}

		public static ZString[] GetConsignorAddressTypesInPreferredOrder()
		{
			return new ZString[]
			{
				nameof(DocAddressType.ConsignorDocumentaryAddress),
				nameof(DocAddressType.ConsignorPickupDeliveryAddress),
				AddressTypes.Supplier,
				nameof(DocAddressType.SupplierDocumentaryAddress),
				nameof(DocAddressType.SupplierPickupDeliveryAddress)
			};
		}

		public static ZString[] GetConsigneeAddressTypesInPreferredOrder()
		{
			return new ZString[]
			{
				nameof(DocAddressType.ConsigneeDocumentaryAddress),
				nameof(DocAddressType.ConsigneeAddress),
				nameof(DocAddressType.ConsigneePickupDeliveryAddress),
				AddressTypes.Importer,
				nameof(DocAddressType.ImporterDocumentaryAddress),
				nameof(DocAddressType.ImporterPickupDeliveryAddress)
			};
		}

		public static ZString[] GetSupplierAddressTypesInPreferredOrder()
		{
			return new ZString[]
			{
				AddressTypes.Supplier,
				nameof(DocAddressType.SupplierDocumentaryAddress),
				nameof(DocAddressType.SupplierPickupDeliveryAddress),
				nameof(DocAddressType.ConsignorDocumentaryAddress),
				nameof(DocAddressType.ConsignorPickupDeliveryAddress)
			};
		}

		public static ZString[] GetImporterAddressTypesInPreferredOrder()
		{
			return new ZString[]
			{
				AddressTypes.Importer,
				nameof(DocAddressType.ImporterDocumentaryAddress),
				nameof(DocAddressType.ImporterPickupDeliveryAddress),
				nameof(DocAddressType.ConsigneeAddress),
				nameof(DocAddressType.ConsigneeDocumentaryAddress),
				nameof(DocAddressType.ConsigneePickupDeliveryAddress)
			};
		}

		public static ZString[] GetNotifyPartyAddressTypesInPreferredOrder()
		{
			return new ZString[]
			{
				nameof(DocAddressType.NotifyParty),
				nameof(DocAddressType.NotifyParty2),
				nameof(DocAddressType.NotifyParty3)
			};
		}
	}
}