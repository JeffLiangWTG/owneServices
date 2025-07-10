using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class GlobalBusinessIdentifierDataMessageImporter
	{
		public GlobalBusinessIdentifierDataMessageImporter(MQEDIMessage message, GlobalBusinessIdentifierData globalBusinessIdentifierData)
		{
			this.message = message;
			this.globalBusinessIdentifierData = globalBusinessIdentifierData;
		}
		readonly MQEDIMessage message;
		readonly GlobalBusinessIdentifierData globalBusinessIdentifierData;

		public void Populate()
		{
			var isManufacturerRole = false;
			var isShipperRole = false;
			var isSellerRole = false;
			var isExporterRole = false;
			var isPackagerRole = false;
			var isDistributorRole = false;
			var addressLine1 = ZString.Empty;
			var addressLine2 = ZString.Empty;

			foreach (var block in message.MessageBlock.MessageBlocks)
			{
				if (block is AGE21 ge21Block)
				{
					isManufacturerRole = ge21Block.ManufacturerRole == EntityCodeList.Codes.ManufacturerSupplier;
					isShipperRole = ge21Block.ShipperRole == EntityCodeList.Codes.Shipper;
					isSellerRole = ge21Block.SellerRole == EntityCodeList.Codes.SellingParty;
					isExporterRole = ge21Block.ExporterRole == EntityCodeList.Codes.Exporter;
					isPackagerRole = ge21Block.PackagerRole == EntityCodeList.Codes.Packager;
					isDistributorRole = ge21Block.DistributorRole == EntityCodeList.Codes.Distributor;
				}
				else if (block is AGE30 ge30Block)
				{
					addressLine1 = ge30Block.EntityAddressLine1;
				}
				else if (block is AGE31 ge31Block)
				{
					addressLine2 = ge31Block.EntityAddressLine2;
				}
			}

			var matchedAddressPK = GetMatchedAddressPKByName(addressLine1, addressLine2);
			if (matchedAddressPK.IsValid)
			{
				globalBusinessIdentifierData.US_OA_AddressDetails = matchedAddressPK;
			}

			globalBusinessIdentifierData.US_IsManufacturer = isManufacturerRole;
			globalBusinessIdentifierData.US_IsShipper = isShipperRole;
			globalBusinessIdentifierData.US_IsSeller = isSellerRole;
			globalBusinessIdentifierData.US_IsExporter = isExporterRole;
			globalBusinessIdentifierData.US_IsPackager = isPackagerRole;
			globalBusinessIdentifierData.US_IsDistributor = isDistributorRole;
		}

		ZGuid GetMatchedAddressPKByName(ZString addressLine1, ZString addressLine2)
		{
			var addressQuery = new ZQuery();
			addressQuery.AddToFilter(OrgAddressSchema.OA_Address1, SQLComparisonOperator.StartsWith, addressLine1);
			addressQuery.AddToFilter(OrgAddressSchema.OA_Address2, SQLComparisonOperator.StartsWith, addressLine2);
			addressQuery.AddToFilter(OrgAddressSchema.OA_IsActive, true);

			var matchedMailingAddresses = globalBusinessIdentifierData.Organization.Addresses.Find(addressQuery);
			return matchedMailingAddresses != null && matchedMailingAddresses.Length == 1 ? matchedMailingAddresses[0].PK : ZGuid.Empty;
		}
	}
}
