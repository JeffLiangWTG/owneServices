using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.eTail.Documents.Business
{
	public class HVLVShipmentACASStateDetailsProvider : IHVLVShipmentACASStateDetailsProvider
	{
		public string Populate(IForwardingShipment forwardingShipment, ZString acasShipmentNumber, ZPropertyInfo displayInformationInfo)
		{
			var shipment = (ForwardingShipment)forwardingShipment;
			var displayMessageErrorList = new List<ZString>();
			var reportSent = false;
			var consignmentACASStatusIsEmpty = false;
			var consignmentHasOIJMessageStatus = false;
			var consignmentHasOSTMessageStatus = false;
			var consignmentHasCBPHoldResponse = false;

			foreach (HVLVItem item in shipment.HVLVItems)
			{
				var consignment = item.Consignment;
				if (!consignment.HVC_ACASMessageStatus.IsEmpty)
				{
					reportSent = true;
					if (consignment.HVC_ACASInterchangeStatus == HVLVACASInterchangeStatusList.Codes.OriginalInterchangeSent && consignment.HVC_ACASStatus.IsEmpty)
					{
						consignmentACASStatusIsEmpty = true;
					}
					else if (consignment.HVC_ACASInterchangeStatus == HVLVACASInterchangeStatusList.Codes.OriginalInterchangeRejected)
					{
						consignmentHasOIJMessageStatus = true;
						break;
					}
					else if (consignment.HVC_ACASMessageStatus == HVLVACASMessageStatusList.Codes.OriginalSent && consignment.HVC_ACASInterchangeStatus.IsEmpty)
					{
						consignmentHasOSTMessageStatus = true;
						break;
					}
					else if (consignment.HVC_ACASInterchangeStatus == HVLVACASInterchangeStatusList.Codes.OriginalInterchangeSent && consignment.IsLastCBPResponseOnHold)
					{
						consignmentHasCBPHoldResponse = true;
						break;
					}
				}
			}

			var displayInformation = !reportSent ? Res.GetString("7368869d-285d-41e1-b599-33f97b2a5f36", "The ACAS Shipment Report has not been sent.")
				: consignmentHasOIJMessageStatus ? Res.GetString("f34a529f-f074-4f92-8335-ba814f41df6f", "Not all ACAS Reports have been sent.")
				: consignmentHasOSTMessageStatus ? Res.GetString("fc9f4794-5fc5-4c28-98d2-c674d2b53bc0", "Await response - Original submitted to eHub.")
				: consignmentHasCBPHoldResponse ? Res.GetString("73d9fd10-680e-4877-93b9-e603c60c62ac", "Await response - \"Hold Currently in Place\" with CBP.")
				: consignmentACASStatusIsEmpty ? Res.GetString("c949dd28-262a-4082-9229-c0118077a8de", "Original sent to CBP.")
				: Res.GetString("6edd6825-bc75-4452-8f70-51e3f77ade5f", "Approved to be uplifted/loaded.");

			displayInformationInfo.AddMessageError(() => !reportSent || consignmentHasOIJMessageStatus,
				Res.GetString("b9633759-6050-4dd6-a978-7aac8a72eba5", "The ACAS House Checklist message can only be sent when all House Bills have been reported to ACAS."));

			displayInformationInfo.AddMessageError(() => consignmentHasOSTMessageStatus && !consignmentHasOIJMessageStatus,
				Res.GetString("15b12be9-d616-46cd-8595-4b81886cb8c4", "The ACAS Shipment Report messages are awaiting forwarding by eHub."));

			displayInformationInfo.AddMessageError(() => consignmentHasCBPHoldResponse && !consignmentHasOIJMessageStatus && !consignmentHasOSTMessageStatus,
				Res.GetString("52ab34b5-34eb-45f0-a5a0-236ec0e88e66", "Shipment {0} is currently on hold with CBP.", acasShipmentNumber));

			displayInformationInfo.AddMessageError(() => consignmentACASStatusIsEmpty && !consignmentHasOIJMessageStatus && !consignmentHasOSTMessageStatus && !consignmentHasCBPHoldResponse,
				 Res.GetString("69ac55d9-c747-4fcb-8158-7db2f96d3b1c", "Await response - CBP risk assessment ongoing."));

			return displayInformation;
		}
	}
}
