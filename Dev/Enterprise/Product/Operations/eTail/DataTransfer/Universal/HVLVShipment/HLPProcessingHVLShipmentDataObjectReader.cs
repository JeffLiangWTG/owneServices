using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HLPProcessingHVLShipmentDataObjectReader : HLPProcessingShipmentDataObjectReader
	{
		public HLPProcessingHVLShipmentDataObjectReader(Shipment shipmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingConsol consol, SubLoadList subLoadList)
			: base(shipmentDataObject, logger, factory, ChildShipmentsParent.ToChildShipmentsParent(consol), new ContainerLinkManager<ForwardingConsol>(consol))
		{
			this.consol = consol;
			this.subLoadList = Argument.NotNull(subLoadList, nameof(subLoadList));
		}

		public HLPProcessingHVLShipmentDataObjectReader(Shipment shipmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment masterShipment, SubLoadList subLoadList)
			: base(shipmentDataObject, logger, factory, ChildShipmentsParent.ToChildShipmentsParent(masterShipment), new ContainerLinkManager<ForwardingConsol>(masterShipment.MostInterestingDepartureConsol))
		{
			this.masterShipment = masterShipment;
			this.subLoadList = Argument.NotNull(subLoadList, nameof(subLoadList));
		}

		readonly ForwardingShipment masterShipment;
		readonly ForwardingConsol consol;
		readonly SubLoadList subLoadList;

		bool IsReadingCoLoadShipmentFromMasterHouse => masterShipment != null;

		protected override ForwardingShipment GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var result = default(ForwardingShipment);
			var shipmentsToSearch = IsReadingCoLoadShipmentFromMasterHouse
				? masterShipment.CoLoadShipments.OfType<ForwardingShipment>().ToList()
				: consol.Shipments.OfType<ForwardingShipment>().ToList();

			if (shipmentsToSearch != null)
			{
				var matchingShipments = shipmentsToSearch.Where(s =>
					s.IsHighVolumeLowValue
					&& s.ConsignorDocumentaryAddress?.E2_OA_Address == subLoadList.BillToPartyPK
					&& s.JS_RS_NKServiceLevel == subLoadList.ServiceLevel);

				if (IsReadingCoLoadShipmentFromMasterHouse)
				{
					matchingShipments = matchingShipments.Where(s => s.Destination?.RL_RN_NKCountryCode == subLoadList.DestinationCountry);
				}

				result = matchingShipments.FirstOrDefault();
			}

			return result;
		}
	}
}
