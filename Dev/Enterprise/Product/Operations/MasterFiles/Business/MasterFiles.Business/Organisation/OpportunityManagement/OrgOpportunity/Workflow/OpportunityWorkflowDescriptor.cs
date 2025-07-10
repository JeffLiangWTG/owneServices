using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OpportunityWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public const string WorkflowTypeCode = "OPP";

		public override string Code
		{
			get { return WorkflowTypeCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("MasterFiles|OpportunityWorkflowDescriptor|Description", "Sales Opportunity"); }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			// Must match OrgOpportunity.GetTemplateSelectionCriteria
			// and OpportunityFormCustomisationSettingProvider.GetPropertiesThatAffectWorkflow
			get
			{
				if (subTypesList == null)
				{
					subTypesList = new[]
					{
						new ProcessTemplateSubType(Res.GetString("063cb7ce-0a63-4d0c-87b6-d0e8f0b3e6d3", "Sales Type"), OrganisationsDataRegistry.Instance.OpportunitySalesTypes.Value.GetActiveCodeDescriptionPairList()),
						new ProcessTemplateSubType(Res.GetString("405d4f24-3e8c-4279-939a-bc3da27ad034", "Source"), OrganisationsDataRegistry.Instance.OpportunitySource.Value.GetActiveCodeDescriptionPairList()),
						new ProcessTemplateSubType(OrganisationsDataRegistry.Instance.ProductTypeLabel.Value, OrganisationsDataRegistry.Instance.ProductTypeList.Value.GetActiveCodeDescriptionPairList()),
					};
				}
				return subTypesList;
			}
		}
		ProcessTemplateSubType[] subTypesList;

		#endregion

		#region Requires Client

		public override bool RequiresClient
		{
			get { return false; }
		}

		#endregion

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Client |
				MessageRecipientPartyType.Email;
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(OrgOpportunity); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Opportunity; }
		}

		protected internal override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);
			if (partyType == MessageRecipientPartyTypeList.Codes.Client)
			{
				OrgOpportunity opportunity = (OrgOpportunity)bizObj;
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(
					new MessageRecipientParty(
						opportunity.Header,
						opportunity.Contact != null ? opportunity.Contact.OC_Email : ZString.Empty,
						opportunity.Address != null ? opportunity.Address.OA_Email : ZString.Empty
						)
					);
			}
		}

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new OpportunityFormCustomisationSettingProvider();
		}

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return new SchemaColumn[] { OrgOpportunitySchema.P8_Status };
		}
	}
}
