using System;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class CRMCampaignWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public const string WorkflowTypeCode = "CAM";

		public override string Code
		{
			get { return WorkflowTypeCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("MasterFiles|CRMCampaignWorkflowDescriptor|Description", "Campaign"); }
		}

		#endregion

		#region Requires Client

		public override bool RequiresClient
		{
			get { return false; }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			// Must match GlbCompanyCampaign.GetTemplateSelectionCriteria
			// and CRMCampaignWorkflowDescriptor.GetPropertiesThatAffectWorkflow
			get
			{
				if (subTypesList == null)
				{
					subTypesList = new[]
					{
						new ProcessTemplateSubType(OrganisationsDataRegistry.Instance.CampaignCategory1Label.Value, OrganisationsDataRegistry.Instance.CampaignCategory1List.Value),
						new ProcessTemplateSubType(OrganisationsDataRegistry.Instance.CampaignCategory2Label.Value, OrganisationsDataRegistry.Instance.CampaignCategory2List.Value.GetActiveCodeDescriptionPairList())
					};
				}
				return subTypesList;
			}
		}
		ProcessTemplateSubType[] subTypesList;

		#endregion

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email;
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.CompanyCampaign }; }
		}

		public override Type WorkflowProviderType
		{
			get { return ObjectFactory.GetType<IGlbCompanyCampaign>(); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.GlbCompanyCampaign; }
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new CampaignFormCustomisationSettingsProvider();
		}
	}
}
