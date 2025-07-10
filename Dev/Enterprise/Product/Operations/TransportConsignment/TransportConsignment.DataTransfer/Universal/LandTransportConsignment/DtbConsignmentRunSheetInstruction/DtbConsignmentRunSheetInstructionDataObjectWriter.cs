using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	public class DtbConsignmentRunSheetInstructionDataObjectWriter : TopLevelDataObjectWriter<DtbConsignmentRunSheetInstruction, UniversalShipment>
	{
		public DtbConsignmentRunSheetInstructionDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.TransportConsignmentRunSheetInstruction;
		}

		protected override void PopulateDataObject(DtbConsignmentRunSheetInstruction instruction, UniversalShipment shipment)
		{
			var recipientRoleCodes = writeManager.Action.RecipientRoleDetails.Select(r => r.Type);
			var sendingToPickupDepot = recipientRoleCodes.Contains(RecipientRoleType.DTW);
			var sendingToDeliveryDepot = recipientRoleCodes.Contains(RecipientRoleType.ATW);

			if (sendingToPickupDepot || sendingToDeliveryDepot)
			{
				var actionType = sendingToPickupDepot ? ActionTypes.Codes.PickUp : ActionTypes.Codes.Delivery;
				if (ConsignmentRunSheetHelper.IsDepotInstruction(instruction, actionType))
				{
					shipment.AddOrgAddress(writeManager, ConsignmentRunSheetHelper.GetFirstAction(instruction, actionType).ConsignmentAddress.Address);

					var consignments = ConsignmentRunSheetHelper.GetAllConsignments(instruction, actionType);
					var containerList = new DataObjectList<Container>();
					var subShipments = ProcessCollection(consignments, new DtbConsignmentDataObjectWriter(writeManager, containerList));
					shipment.SetSubShipmentCollection(() => subShipments != null ? new DataObjectList<UniversalShipment>(subShipments) : null);
					shipment.SetContainerCollection(() => containerList);
				}
			}
		}
	}
}


