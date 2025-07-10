using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicationWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public const string WorkflowTypeCode = "HRA";

		public override string Code
		{
			get { return WorkflowTypeCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("MasterFiles|HRJobApplicationWorkflowDescriptor|Description", "Job Application"); }
		}

		#endregion

		#region Requires Client

		public override bool RequiresClient
		{
			get { return true; }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			// Must match HRJobApplications.GetTemplateSelectionCriteria
			// and CampaignWorkflowDescriptor.GetPropertiesThatAffectWorkflow
			get
			{
				if (subTypesList == null)
				{
					var factory = CreateNewFactory();
					var countriesCollection = new RefCountryCollection(factory);
					var glbStaffCollection = new GlbStaffCollection(factory);

					subTypesList = new[]
					{
						new ProcessTemplateSubType(Res.GetString("eaecd177-1716-46f0-a8d3-dc577e45b809", "Country/Region"), countriesCollection, false),
						new ProcessTemplateSubType(Res.GetString("43e8fcf6-9849-4bdd-bb2a-a1b30013656c", "R. Coordinator"), glbStaffCollection, false)
					};
				}
				return subTypesList;
			}
		}
		ProcessTemplateSubType[] subTypesList;

		#endregion

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email;
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override Type WorkflowProviderType
		{
			get { return ObjectFactory.GetType<IHRJobApplication>(); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.HRJobApplication; }
		}

		protected override string GetSubjectForEmailParty(ProcessTaskNotification action)
		{
			return action.Parent.GetDescriptionWithReference();
		}
	}
}
