using System.Linq;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class WarehouseCustomsDetailsChangeOfOwnership : IWarehouseCustomsDetailsChangeOfOwnership
	{
		public WarehouseCustomsDetailsChangeOfOwnership(Shipment shipment)
		{
			this.shipment = shipment;
		}
		protected readonly Shipment shipment;

		public OrganizationAddress OldOwner
		{
			get { return oldOwner ?? (oldOwner = GetOldOwner()); }
		}
		OrganizationAddress oldOwner;

		protected virtual OrganizationAddress GetOldOwner()
		{
			var importerDocumentaryAddress = nameof(DocAddressType.ImporterDocumentaryAddress);
			return shipment.OrganizationAddressCollection?.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == importerDocumentaryAddress);
		}

		public OrganizationAddress NewOwner
		{
			get { return newOwner ?? (newOwner = GetNewOwner()); }
		}
		OrganizationAddress newOwner;

		protected virtual OrganizationAddress GetNewOwner()
		{
			OrganizationAddress result = null;
			if (shipment.EntryInstructionCollection != null && shipment.EntryInstructionCollection.Count == 1)
			{
				var entryInstruction = shipment.EntryInstructionCollection[0];
				result = entryInstruction?.OrganizationAddressCollection?.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == AddressTypes.Owner);
			}
			return result;
		}

		public OrganizationAddress NewWarehouse
		{
			get { return newWarehouse ?? (newWarehouse = GetNewWarehouse()); }
		}
		OrganizationAddress newWarehouse;

		protected virtual OrganizationAddress GetNewWarehouse()
		{
			return null; // To be implemented by Customs
		}
	}
}
