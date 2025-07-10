using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierPartWorkflowDescriptor : WorkflowDescriptor
	{
		#region Identification

		public override string Code
		{
			get { return WorkflowDescriptors.OrgSupplierPartWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("MasterFiles|OrgSupplierPartWorkflowDescriptor|Description", "Product"); }
		}

		public override Type WorkflowProviderType
		{
			get { return OrgSupplierPartTypeDecider.GetOrgSupplierPartType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WhsConfigProduct; }
		}

		#endregion

		#region Capabilities

		public override bool SupportsEventTracking { get { return true; } }
		public override bool SupportsWorkflowTriggerActionXML { get { return true; } }
		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo) { return true; }
		public override bool SupportsPostOverseasAgentCharges { get { return false; } }
		public override bool IncludeWorkflowTriggerActionXMLDebtorBalance { get { return false; } }

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Client |
				MessageRecipientPartyType.Email;
		}

		#endregion

		#region Trigger Actions

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source)
		{
			IProcessor result;
			var action = source.Action;
			var job = source.Job;

			if (WorkflowTriggerActionTypeConstants.IsStandardXml(action.PQ_TriggerType))
			{
				var adapterType = ObjectFactory.GetType("ProductValueObjectDataAdapter");
				var communicationModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XML);
				result = WorkflowDescriptorHelper.GetWorkflowTriggerActionForStandardXmlActionType(adapterType, (IWorkflowProvider)job, communicationModes, action);
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

			if (partyType == MessageRecipientPartyTypeList.Codes.Client)
			{
				var part = (OrgSupplierPart)bizObj;

				foreach (OrgPartRelation relation in part.RelatedOrganisations)
				{
					if (relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner || relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Both)
					{
						var org = relation.Organisation;
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(org, org.MainAddress.OA_Email));
					}
				}
			}
		}

		#endregion

	}
}
