using System;
using CargoWise.Definitions;
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
	public class OrgSalesCallWorkflowDescriptor : WorkflowDescriptor
	{
		#region Identification

		public const string WorkflowTypeCode = "COM";

		public override string Code
		{
			get { return WorkflowTypeCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("MasterFiles|OrgSalesCallWorkflowDescriptor|Description", "Communication Manager"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(OrgSalesCall); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Communication; }
		}

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			// Must match OrgSalesCall.GetTemplateSelectionCriteria
			// and OrgSalesCallFormCustomisationSettingsProvider.GetPropertiesThatAffectWorkflow
			get
			{
				return new ProcessTemplateSubType[]
				{
					new ProcessTemplateSubType(Res.GetString("8640a68e-4ce8-42f7-a328-3be00a7ee579", "Method"), OrganisationsDataRegistry.Instance.CommunicationType.Value.GetActiveCodeDescriptionPairList()),
					new ProcessTemplateSubType(Res.GetString("a55d3f02-0506-4737-b972-3c6cb15ebb7d", "Status"), OrganisationsDataRegistry.Instance.CommunicationStatusList.Value.GetActiveCodeDescriptionPairList()),
					new ProcessTemplateSubType(OrganisationsDataRegistry.Instance.CategoryListLabel.Value, OrganisationsDataRegistry.Instance.CategoryList.Value.GetActiveCodeDescriptionPairList() )
				};
			}
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get
			{
				return new[] { BusinessContext.Communication };
			}
		}

		#endregion

		#region Capabilities

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override bool RequiresClient
		{
			get { return false; }
		}

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

		protected internal override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);
			if (partyType == MessageRecipientPartyTypeList.Codes.Client)
			{
				OrgSalesCall communication = (OrgSalesCall)bizObj;
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(
					new MessageRecipientParty(
						communication.Header,
						communication.ContactEmail,
						communication.Header != null ? communication.Header.MainAddress.OA_Email : ZString.Empty
						)
					);
			}
		}

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return new SchemaColumn[] { OrgSalesCallSchema.OQ_Status };
		}

		#endregion

		#region Form Customisation

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new OrgSalesCallFormCustomisationSettingsProvider();
		}

		#endregion
	}
}
