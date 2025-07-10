using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HLPProcessingHVMShipmentDataObjectReader : HLPProcessingShipmentDataObjectReader
	{
		public HLPProcessingHVMShipmentDataObjectReader(Shipment shipmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingConsol consol, IContainerLinkManager<ForwardingConsol> linkManager, IShipmentDataObjectReader parentReader = null, IUniversalFreightHelper helper = null)
			: base(shipmentDataObject, logger, factory, ChildShipmentsParent.ToChildShipmentsParent(consol), linkManager, parentReader, helper)
		{
			parentConsol = consol;
		}

		readonly ForwardingConsol parentConsol;

		protected override ForwardingShipment GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var wayBillNumber = dataObject.WayBillNumber.GetValueOrDefault().SubstringSafe(0, 20);
			var serviceLevel = dataObject.ServiceLevel?.Code ?? ZString.Empty;
			var result = default(ForwardingShipment);

			var matchedHVMShipments = parentConsol.Shipments
					.OfType<ForwardingShipment>()
					.Where(shipment =>
						shipment.IsHighVolumeLowValueMaster &&
						(wayBillNumber.IsEmpty || shipment.JS_HouseBill == wayBillNumber) &&
						shipment.JS_RS_NKServiceLevel == serviceLevel);

			if (matchedHVMShipments.Count() == 1)
			{
				result = matchedHVMShipments.Single();
			}
			else if (matchedHVMShipments.Count() > 1)
			{
				logger.Log(LogType.Error, Res.GetString("61e3b658-2bd5-477d-a6c0-0209fa92f0b8", "There are multiple HVM shipments on consol '{0}'. Load list failed to be merged.", parentConsol.JK_UniqueConsignRef));
			}

			return result;
		}

		protected override IMatchingBusinessEntityFinder<ForwardingShipment> GetCombinedReferenceMatcher() => null;
	}
}
