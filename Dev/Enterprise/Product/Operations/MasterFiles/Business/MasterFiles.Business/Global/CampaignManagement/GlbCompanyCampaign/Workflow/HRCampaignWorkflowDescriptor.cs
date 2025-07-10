using System;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.Integration.Recruiter;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class HRCampaignWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public const string WorkflowTypeCode = "HRC";

		public override string Code
		{
			get { return WorkflowTypeCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("MasterFiles|HRCampaignWorkflowDescriptor|Description", "Human Resources Campaign"); }
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
			// Must match HRGlbCompanyCampaign.GetTemplateSelectionCriteria
			// and CampaignFormCustomisationSettingsProvider.GetPropertiesThatAffectWorkflow
			get
			{
				if (subTypesList == null)
				{
					subTypesList = new[]
					{
						new ProcessTemplateSubType(OrganisationsDataRegistry.Instance.HRCampaignCategory1Label.Value, OrganisationsDataRegistry.Instance.HRCampaignCategory1List.Value),
						new ProcessTemplateSubType(OrganisationsDataRegistry.Instance.HRCampaignCategory2Label.Value, OrganisationsDataRegistry.Instance.HRCampaignCategory2List.Value.GetActiveCodeDescriptionPairList())
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
			get { return ObjectFactory.GetType<IHRGlbCompanyCampaign>(); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.HRGlbCompanyCampaign; }
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new CampaignFormCustomisationSettingsProvider();
		}
	}
}
