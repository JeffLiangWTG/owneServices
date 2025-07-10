using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class WarehouseEntryDataObjectWriter : TopLevelDataObjectWriter<CusEntryHeader, Shipment>
	{
		public WarehouseEntryDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected sealed override void AddTableFetchHintCreators(IExternalFetchHintSupporter externalFetchHintSupporter, CusEntryHeader sourceBO)
		{
			var declaration = sourceBO.Declaration;
			if (declaration == null)
			{
				decWriter = null;
			}
			else
			{
				decWriter = JobDeclarationDataContextManager.GetShipmentDataObjectWriter(this.writeManager, declaration) as DeclarationDataObjectWriter;
				decWriter?.AddTableFetchHintCreatorsCore(externalFetchHintSupporter, declaration);
			}
		}

		bool IsChangeOfOwnershipAction
		{
			get { return writeManager.HasRecipientRoleDetail(RecipientRoleType.BCO); }
		}

		bool IsChangeOfRegimeAction
		{
			get { return writeManager.HasRecipientRoleDetail(RecipientRoleType.BCR); }
		}

		bool IsInwardAction
		{
			get { return writeManager.HasRecipientRoleDetail(RecipientRoleType.BWI); }
		}

		bool IsOutwardAction
		{
			get { return writeManager.HasRecipientRoleDetail(RecipientRoleType.BWR); }
		}

		protected override void PopulateDataObject(CusEntryHeader entryBO, Shipment headerData)
		{
			decWriter?.PopulateDataObject(entryBO, headerData);
			var isInwardAction = IsInwardAction;
			var isChangeOfOwnershipAction = IsChangeOfOwnershipAction;
			if (isInwardAction || isChangeOfOwnershipAction)
			{
				PopulateIntoWarehouseAddress(entryBO, headerData);
				if (!isChangeOfOwnershipAction)
				{
					PopulateWarehouseClient((entryBO.EntryInstruction?.HasBothOutOfAndIntoRegimeProcedure ?? false) ? entryBO.EntryInstruction?.Owner : entryBO.Declaration?.WarehouseClient, headerData);
				}
			}
			else
			{
				var isChangeOfRegimeAction = IsChangeOfRegimeAction;
				if (isChangeOfRegimeAction || IsOutwardAction)
				{
					PopulateOutOfWarehouseAddress(entryBO, headerData);
					var declaration = entryBO.Declaration;
					if (declaration != null)
					{
						PopulateWarehouseClient(declaration.WarehouseClient, headerData);
					}
				}
			}
		}

		void PopulateWarehouseClient(OrgHeader client, Shipment headerData)
		{
			var importerDocumentaryAddress = headerData.OrganizationAddressCollection?.FirstOrDefault(AddressTypes.WarehouseClient);
			if (importerDocumentaryAddress != null)
			{
				headerData.OrganizationAddressCollection.Remove(importerDocumentaryAddress);
			}
			headerData.AddOrgAddress(writeManager, client, AddressTypes.WarehouseClient);
		}

		void PopulateIntoWarehouseAddress(CusEntryHeader entryBO, Shipment headerData)
		{
			var customsWarehouseAddress = headerData.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.CustomsWarehouseAddress));
			if (customsWarehouseAddress != null)
			{
				headerData.OrganizationAddressCollection.Remove(customsWarehouseAddress);
			}
			headerData.AddOrgAddress(writeManager, entryBO.GetIntoWarehouseAddress(), DocAddressType.CustomsWarehouseAddress);
		}

		void PopulateOutOfWarehouseAddress(CusEntryHeader entryBO, Shipment headerData)
		{
			var customsWarehouseAddress = headerData.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.CustomsWarehouseAddress));
			if (customsWarehouseAddress != null)
			{
				headerData.OrganizationAddressCollection.Remove(customsWarehouseAddress);
			}
			headerData.AddOrgAddress(writeManager, entryBO.GetOutOfWarehouseAddress(), DocAddressType.CustomsWarehouseAddress);
		}

		DeclarationDataObjectWriter decWriter;

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.WarehouseCustomsEntry;
		}
	}
}
