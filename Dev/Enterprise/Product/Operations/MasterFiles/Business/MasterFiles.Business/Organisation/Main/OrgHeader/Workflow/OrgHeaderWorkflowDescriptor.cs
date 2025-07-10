using System;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class OrgHeaderWorkflowDescriptor : WorkflowDescriptor
	{
		#region Identification

		public const string WorkflowTypeCode = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

		public override string Code { get { return WorkflowTypeCode; } }

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("MasterFiles|OrgHeaderWorkflowDescriptor|Description", "Organization"); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Organisation; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(OrgHeader); }
		}

		#endregion

		#region Capabilities

		public override bool RequiresClient { get { return false; } }

		public override bool RequiresBranch { get { return true; } }

		public override bool RequiresDepartment { get { return true; } }

		public override bool SupportsEventTracking { get { return true; } }

		public override bool SupportsWorkflowTriggerActionXML { get { return true; } }

		public override bool IncludeWorkflowTriggerActionXMLDebtorBalance { get { return false; } }

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.EDICommunication;
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.Organisation }; }
		}

		#endregion

		#region Trigger Actions

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source)
		{
			IProcessor result;
			var action = source.Action;

			if (WorkflowTriggerActionTypeConstants.IsStandardXml(action.PQ_TriggerType))
			{
				var adapterType = ObjectFactory.GetType("OrganisationXmlDataTransferAdapter");
				var communicationModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XML);
				result = WorkflowDescriptorHelper.GetWorkflowTriggerActionForStandardXmlActionType(adapterType, (IWorkflowProvider)source.Job, communicationModes, action);
			}
			else
			{
				result = base.GetWorkflowTriggerActionCore(source);
			}

			return result;
		}

		protected internal override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);
			if (partyType == MessageRecipientPartyTypeList.Codes.EDICommunication)
			{
				var org = (OrgHeader)bizObj;
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(org, org.MainAddress.OA_Email));
			}
		}
		#endregion
	}
}
