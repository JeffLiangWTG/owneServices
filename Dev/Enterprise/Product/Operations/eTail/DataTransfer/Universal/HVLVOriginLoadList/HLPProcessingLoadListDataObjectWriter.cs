using System.Collections.Generic;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public abstract class HLPProcessingLoadListDataObjectWriter : HVLVOriginLoadListDataObjectWriter
	{
		protected HLPProcessingLoadListDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected sealed override void PopulateDataObject(HVLVOriginLoadList loadListBO, Shipment dataObject)
		{
			base.PopulateDataObject(loadListBO, dataObject);

			PopulateShipmentType(dataObject);
			PopulateContainerMode(loadListBO, dataObject);
			PopulateDataObjectCore(loadListBO, dataObject);
		}

		protected abstract void PopulateShipmentType(Shipment dataObject);

		protected abstract void PopulateDataObjectCore(HVLVOriginLoadList loadListBO, Shipment dataObject);

		protected override void PopulateOrganizationAddresses(HVLVOriginLoadList loadListBO, Shipment dataObject)
		{
			dataObject.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>() { };

				var shipperAddress = GetShipperAddress(loadListBO);

				if (shipperAddress != null)
				{
					var consignorAddress = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ConsignorDocumentaryAddress)).GetDataObject(shipperAddress);
					addresses.Add(consignorAddress);
				}

				if (loadListBO.DestinationDepot != null)
				{
					var consigneeAddress = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ConsigneeDocumentaryAddress)).GetDataObject(loadListBO.DestinationDepot);
					addresses.Add(consigneeAddress);
				}

				return addresses;
			});
		}

		protected abstract OrgAddress GetShipperAddress(HVLVOriginLoadList loadListBO);

		void PopulateContainerMode(HVLVOriginLoadList loadListBO, Shipment dataObject)
		{
			dataObject.ContainerMode = new ContainerMode() { Code = loadListBO.CalculateContainerMode() };
		}

		protected string VariousCargo = Res.GetString("b08e1b58-1805-425e-8df5-a85a02beade5", "Various Cargo");
	}
}
