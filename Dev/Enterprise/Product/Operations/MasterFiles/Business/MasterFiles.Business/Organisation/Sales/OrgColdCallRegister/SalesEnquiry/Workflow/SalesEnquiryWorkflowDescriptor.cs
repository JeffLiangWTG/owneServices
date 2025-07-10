using System;
using System.Collections;
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
	public class SalesEnquiryWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public const string WorkflowTypeCode = "INQ";

		public override string Code
		{
			get { return WorkflowTypeCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("MasterFiles|OrgEnquiryWorkflowDescriptor|Description", "Sales Inquiry"); }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			// Must match SalesEnquiry.GetTemplateSelectionCriteria
			// and SalesEnquiryFormCustomisationSettingsProvider.GetPropertiesThatAffectWorkflow
			get
			{
				ArrayList list = new ArrayList();
				list.AddRange(base.SubTypeInformation);
				list.Add(new ProcessTemplateSubType(Res.GetString("8DB36899-721A-4394-AF0C-46147ECE2424", "Inquiry Type"), SalesEnquiryLookups.GetAllEnquiryTypes()));
				list.Add(new ProcessTemplateSubType(Res.GetString("d9d8e90d-9ecd-4a72-a848-61dbb7f39fa5", "Source"), OrganisationsDataRegistry.Instance.OpportunitySource.Value));
				list.Add(new ProcessTemplateSubType(Res.GetString("1a4b1b9c-c52b-42a8-bf5e-504fe7046a17", "Organization"), new SalesEnquiryOrgRelationshipCodeList()));

				return (ProcessTemplateSubType[])list.ToArray(typeof(ProcessTemplateSubType));
			}
		}

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
			get { return typeof(SalesEnquiry); }
		}

		protected internal override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);
			if (partyType == MessageRecipientPartyTypeList.Codes.Client)
			{
				SalesEnquiry opportunity = (SalesEnquiry)bizObj;
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(
					new MessageRecipientParty(
						opportunity.Header,
						opportunity.Contact != null ? opportunity.Contact.OC_Email : ZString.Empty,
						opportunity.LinkedAddress != null ? opportunity.LinkedAddress.OA_Email : ZString.Empty
						)
					);
			}
		}

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.SalesEnquiry; }
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new SalesEnquiryFormCustomisationSettingsProvider();
		}

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return new SchemaColumn[] { OrgColdCallRegisterSchema.O1_LeadStatus };
		}
	}
}
