using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public abstract class HLPProcessingShipmentDataObjectReader : ShipmentDataObjectReader
	{
		public HLPProcessingShipmentDataObjectReader(Shipment shipmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ChildShipmentsParent childShipmentsParent, IContainerLinkManager<ForwardingConsol> linkManager, IShipmentDataObjectReader parentReader = null, IUniversalFreightHelper helper = null)
			: base(shipmentDataObject, logger, factory, childShipmentsParent, linkManager, parentReader, helper)
		{
		}

		protected override void PopulateBusinessObject(ForwardingShipment shipmentBO)
		{
			base.PopulateBusinessObject(shipmentBO);

			PopulateJobDocAddress(shipmentBO, OrganisationTypes.Consignor, DocAddressType.DepartureCFSAddress);
			PopulateJobDocAddress(shipmentBO, OrganisationTypes.Consignee, DocAddressType.ArrivalCFSAddress);
		}

		void PopulateJobDocAddress(ForwardingShipment shipmentBO, OrganisationTypes orgType, DocAddressType docAddressType)
		{
			var orgAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(docAddressType.ToString());
			if (orgAddress != null)
			{
				var addressBO = new OrganisationDataObjectReader(orgAddress, logger, factory).GetMatchedOrNew(shipmentBO, orgType, null, docAddressType);
				if (addressBO != null)
				{
					shipmentBO.DocAddresses.Add(addressBO);
				}
			}
		}
	}
}
