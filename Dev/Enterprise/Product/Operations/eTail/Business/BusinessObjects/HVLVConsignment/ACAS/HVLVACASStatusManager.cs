using Enterprise.ZArchitecture.Business;

namespace Enterprise.eTail.Business
{
	public static class HVLVACASStatusManager
	{
		public static void UpdateACASStatus(HVLVConsignment consignment, string eventType, string acasReason)
		{
			switch (eventType)
			{
				case AutoEvents.HeldCode:
					if (consignment.Lookups.HVC_ACASStatus_List.ContainsCode(acasReason))
					{
						consignment.HVC_ACASStatus = acasReason;
					}

					if (consignment.IsLastCBPResponseOnHold)
					{
						consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AcknowledgementRequired;
					}

					break;
				case AutoEvents.MessagePendingProcessingCode:
				case AutoEvents.ClearedHoldCode:
				case AutoEvents.ClearanceCompletedCode:
					if (consignment.Lookups.HVC_ACASStatus_List.ContainsCode(acasReason))
					{
						consignment.HVC_ACASStatus = acasReason;
					}

					break;
				case AutoEvents.InterchangeSentCode:
					if (consignment.HVC_ACASMessageStatus == HVLVACASMessageStatusList.Codes.OriginalSent)
					{
						consignment.HVC_ACASInterchangeStatus = HVLVACASInterchangeStatusList.Codes.OriginalInterchangeSent;
					}
					else if (consignment.IsLastCBPResponseOnSelecteeDataIssueHold && consignment.HVC_ACASMessageStatus == HVLVACASMessageStatusList.Codes.AcknowledgementSent)
					{
						consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AmendmentRequired;
						consignment.HVC_ACASInterchangeStatus = HVLVACASInterchangeStatusList.Codes.AcknowledgmentInterchangeSent;
					}
					else if (consignment.IsLastCBPResponseOnSelecteeDataIssueHold && consignment.HVC_ACASMessageStatus == HVLVACASMessageStatusList.Codes.AmendmentSent)
					{
						consignment.HVC_ACASInterchangeStatus = HVLVACASInterchangeStatusList.Codes.AmendmentInterchangeSent;
					}
					else if (consignment.IsLastCBPResponseOnHold && consignment.HVC_ACASMessageStatus == HVLVACASMessageStatusList.Codes.AcknowledgementSent)
					{
						consignment.HVC_ACASInterchangeStatus = HVLVACASInterchangeStatusList.Codes.AcknowledgmentInterchangeSent;
					}

					break;
				case AutoEvents.InterchangeRejectedCode:
					if (consignment.HVC_ACASMessageStatus == HVLVACASMessageStatusList.Codes.OriginalSent)
					{
						consignment.HVC_ACASInterchangeStatus = HVLVACASInterchangeStatusList.Codes.OriginalInterchangeRejected;
						consignment.HVC_ACASMessageStatus = string.Empty;
					}
					else if (consignment.IsLastCBPResponseOnSelecteeDataIssueHold && consignment.HVC_ACASMessageStatus == HVLVACASMessageStatusList.Codes.AmendmentSent)
					{
						consignment.HVC_ACASInterchangeStatus = HVLVACASInterchangeStatusList.Codes.AmendmentInterchangeRejected;
						consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AmendmentRequired;
					}
					else if (consignment.IsLastCBPResponseOnHold && consignment.HVC_ACASMessageStatus == HVLVACASMessageStatusList.Codes.AcknowledgementSent)
					{
						consignment.HVC_ACASInterchangeStatus = HVLVACASInterchangeStatusList.Codes.AcknowledgementInterchangeRejected;
						consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AcknowledgementRequired;
					}

					break;
			}
		}
	}
}
