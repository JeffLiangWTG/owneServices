using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentRunSheetWorkflowDescriptor : WorkflowDescriptor, IWorkflowParentWithLines
	{
		public override string Code
		{
			get { return WorkflowDescriptors.DtbConsignmentRunSheetWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("TransportConsignment|DtbConsignmentRunSheetWorkflowDescriptor|Description", "Transport Run Sheet"); }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.DtbConsignRunSheet }; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(DtbConsignmentRunSheet); }
		}

		public override ControllerID ControllerID => ControllerIDs.DtbConsignmentRunSheet;

		public override bool SupportsBufferManagement => true;

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool SupportsUniversalTemplates => false;

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override bool RequiresClient
		{
			get { return false; }
		}

		IEnumerable<string> IWorkflowParentWithLines.SupportedTriggerLineTypes
		{
			get { return new[] { TriggerLineTypes.Codes.RunSheetInstruction }; }
		}

		#region Recipients

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy
				| MessageRecipientPartyType.Email
				| MessageRecipientPartyType.TransportCo
				| MessageRecipientPartyType.DepartureTransitWarehouse
				| MessageRecipientPartyType.ArrivalTransitWarehouse;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var runSheet = (DtbConsignmentRunSheet)bizObj;

			var transportCo = runSheet.TransportCo;
			if (partyType == MessageRecipientPartyTypeList.Codes.TransportCo && transportCo != null)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(transportCo.MainAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse || partyType == MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse)
			{
				var actionType = partyType == MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse ? ActionTypes.Codes.PickUp : ActionTypes.Codes.Delivery;
				var depotInstructions = runSheet.RunSheetInstructions.Where(i => ConsignmentRunSheetHelper.IsDepotInstruction(i, actionType)).ToArray();
				if (depotInstructions.Length == 1)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ConsignmentRunSheetHelper.GetInstructionAddress(depotInstructions[0], actionType)));
				}
			}
		}

		public override bool IsMessagingOrEmailNotificationTriggerAction(ZString triggerAction)
		{
			return triggerAction != WorkflowTriggerActionTypeConstants.Codes.SendDocument && base.IsMessagingOrEmailNotificationTriggerAction(triggerAction);
		}

		#endregion

		#region SupportedTriggerPartyServicesCore

		protected override ZString[] SupportedTriggerPartyServicesCore(ZString recipient)
		{
			switch (recipient)
			{
				case MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse:
					return new ZString[] { ServiceCodesList.Codes.TransitWarehouseDispatch };

				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					return new ZString[] { ServiceCodesList.Codes.TransitWarehouseReceive };
				default:
					return base.SupportedTriggerPartyServicesCore(recipient);
			}
		}

		#endregion
	}
}
