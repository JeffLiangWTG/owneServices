using CargoWise.Common;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class ACEAMSMessageBuilder
	{
		public ACEAMSMessageBuilder(IACEBillManifestMessageAttachee attachee, ActionCode action)
		{
			this.applicationIdentifier = GetApplicationIdentifier(action);
			Argument.NotNull(attachee, "attachee");
			this.attachee = attachee;
			this.action = action;
		}

		string GetApplicationIdentifier(ActionCode action)
		{
			var result = string.Empty;
			switch (action)
			{
				case ActionCode.Creating:
					result = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
					break;
				case ActionCode.AmendingAdd:
				case ActionCode.AmendingUpdate:
				case ActionCode.AmendingDelete:
					result = AMSApplicationIdentifierCodeList.Codes.ManifestAmendment;
					break;
				case ActionCode.SubsequentInBondOriginal:
				case ActionCode.SubsequentInBondAmendment:
				case ActionCode.SubsequentInBondDelete:
					result = AMSApplicationIdentifierCodeList.Codes.SubsequentInBond;
					break;
				case ActionCode.Equipment:
					result = AMSApplicationIdentifierCodeList.Codes.EquipmentInventory;
					break;
				case ActionCode.GeneralOrderStatus:
					result = AMSApplicationIdentifierCodeList.Codes.GeneralOrderStatus;
					break;
				case ActionCode.InBondArrival:
				case ActionCode.InBondDiversion:
				case ActionCode.InBondExportation:
				case ActionCode.InBondTransferOfLiability:
				case ActionCode.CancelPermitToTransfer:
				case ActionCode.VesselArrival:
				case ActionCode.VesselDeparture:
				case ActionCode.ChangeEstDateOfArrival:
					result = AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival;
					break;
				case ActionCode.PermitToTransfer:
					result = AMSApplicationIdentifierCodeList.Codes.PermitToTransfer;
					break;
				default:
					ErrorReporter.ReportOnce("Not Supported Action Code '" + action.ToString(), "Action Code '" + action.ToString() + "' is not supported.");
					break;
			}
			return result;
		}

		public AMSEDIMessage PopulateMessage()
		{
			var block = new AMSInputBlockControlGenerator();
			block.B.ApplicationIdentifier = applicationIdentifier;
			UpdateMessageBlocks(block);

			var message = block.CreateMessage<AMSEDIMessage>(attachee.Factory);
			message.EM_MessageOwner = Constants.ACE;
			var branch = attachee.Branch;
			if (branch != null)
			{
				message.EM_GB = branch.PK;
			}

			var billPKAsString = attachee.BillPKAsString;
			if (!billPKAsString.IsEmpty)
			{
				message.EM_ApplicationReference = billPKAsString;
				attachee.BillMessages.Add(message);
			}

			attachee.Messages.Add(message);
			SetMessageSubType(message);
			attachee.UpdateOutgoingBillStatus();
			return message;
		}

		void UpdateMessageBlocks(AMSInputBlockControlGenerator block)
		{
			if (action == ActionCode.PermitToTransfer)
			{
				block.MessageBlocks.AddRange(new PTTMessageBlockBuilder(attachee).Build());
			}
			else if (ActionCodeTool.IsInBondVesselArrivalDeparture(action))
			{
				block.MessageBlocks.AddRange(new InBondVesselEventMessageBlockBuilder(attachee).Build());
			}
			else
			{
				block.MessageBlocks.AddRange(new ACEAMSMessageBlockBuilder(attachee, action).Build());
			}
		}

		void SetMessageSubType(AMSEDIMessage message)
		{
			message.EM_MessageSubType = AMSMessageSubTypeList.GetSubTypeFromActionCode(action);
		}

		readonly IACEBillManifestMessageAttachee attachee;
		readonly ActionCode action;
		readonly string applicationIdentifier;
	}
}
