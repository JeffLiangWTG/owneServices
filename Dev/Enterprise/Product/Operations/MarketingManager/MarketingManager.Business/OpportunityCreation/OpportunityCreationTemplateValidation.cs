using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class OpportunityCreationTemplateValidation : ZValidation
	{
		public OpportunityCreationTemplateValidation(OpportunityCreationTemplate parent) : base(parent)
		{
			this.Parent = parent;
		}

		public override Type AutoValidationType => typeof(OpportunityCreationTemplateValidation);

		protected string InvalidCharacterMessage => Res.GetString("5BEEC6E4-D0A0-4002-9911-666BB45D21D5", "Invalid character entered: '{0}'", Parent.Separator);

		protected readonly OpportunityCreationTemplate Parent;

		public override void ValidateAll()
		{
			ValidateOpportunityDescription();
			ValidatePackageType();
			ValidateOpportunityType();
			ValidateOpportunityStage();
			ValidateOpportunityStatus();
			ValidateSource();
			ValidateSourceDetails();
			ValidateActiveSourceDetails();
			ValidateOpportunityAssignment();
			ValidateSalesPerson();
			ValidateStaffAssignment();
			ValidateOpportunityNotes();
		}

		public void ValidateOpportunityDescription()
		{
			ValidateCalculatedProperty(Parent.OpportunityDescriptionInfo);
		}

		public void ValidatePackageType()
		{
			ValidateCalculatedProperty(Parent.PackageTypeInfo);
		}

		public void ValidateOpportunityType()
		{
			ValidateCalculatedProperty(Parent.OpportunityTypeInfo);
		}

		public void ValidateOpportunityStage()
		{
			ValidateCalculatedProperty(Parent.OpportunityStageInfo);
		}

		public void ValidateOpportunityStatus()
		{
			ValidateCalculatedProperty(Parent.OpportunityStatusInfo);
		}

		public void ValidateSource()
		{
			ValidateCalculatedProperty(Parent.SourceInfo);
		}

		public void ValidateSourceDetails()
		{
			ValidateCalculatedProperty(Parent.SourceDetailsInfo);
		}

		public void ValidateActiveSourceDetails()
		{
			ValidateCalculatedProperty(Parent.ActiveSourceDetailsInfo);
		}

		public void ValidateOpportunityAssignment()
		{
			ValidateCalculatedProperty(Parent.OpportunityAssignmentInfo);
		}

		public void ValidateSalesPerson()
		{
			ValidateCalculatedProperty(Parent.SalesPersonInfo);
		}

		public void ValidateStaffAssignment()
		{
			ValidateCalculatedProperty(Parent.StaffAssignmentInfo);
		}

		public void ValidateOpportunityNotes()
		{
			ValidateCalculatedProperty(Parent.OpportunityNotesInfo);
		}

		protected virtual void CheckOpportunityDescription()
		{
			if (!Parent.Campaign.IsOpportunityCreationCampaign)
			{
				return;
			}

			if (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value.GetBoolFromCode(OrgOpportunitySchema.Constants.P8_OpportunityDescription))
			{
				MandatoryValidation.CheckEntered(Parent.OpportunityDescriptionInfo);
			}

			CheckForInvalidChar(Parent.OpportunityDescriptionInfo);
		}

		protected virtual void CheckPackageType()
		{
			if (!Parent.Campaign.IsOpportunityCreationCampaign)
			{
				return;
			}

			if (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value.GetBoolFromCode(OrgOpportunitySchema.Constants.P8_PackageType))
			{
				MandatoryValidation.CheckEntered(Parent.PackageTypeInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.PackageTypeInfo);
			CheckForInvalidChar(Parent.PackageTypeInfo);
		}

		protected virtual void CheckOpportunityType()
		{
			if (!Parent.Campaign.IsOpportunityCreationCampaign)
			{
				return;
			}

			MandatoryValidation.CheckEntered(Parent.OpportunityTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OpportunityTypeInfo);
			CheckForInvalidChar(Parent.OpportunityTypeInfo);
		}

		protected virtual void CheckOpportunityStage()
		{
			if (!Parent.Campaign.IsOpportunityCreationCampaign)
			{
				return;
			}

			MandatoryValidation.CheckEntered(Parent.OpportunityStageInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OpportunityStageInfo);
			CheckForInvalidChar(Parent.OpportunityStageInfo);
		}

		protected virtual void CheckOpportunityStatus()
		{
			if (!Parent.Campaign.IsOpportunityCreationCampaign)
			{
				return;
			}

			MandatoryValidation.CheckEntered(Parent.OpportunityStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OpportunityStatusInfo);
			CheckForInvalidChar(Parent.OpportunityStatusInfo);
		}

		protected virtual void CheckSource()
		{
			if (!Parent.Campaign.IsOpportunityCreationCampaign)
			{
				return;
			}

			if (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value.GetBoolFromCode(OrgOpportunitySchema.Constants.P8_Source))
			{
				MandatoryValidation.CheckEntered(Parent.SourceInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.SourceInfo);
			CheckForInvalidChar(Parent.SourceInfo);
		}

		protected virtual void CheckSourceDetails()
		{
			if (!Parent.Campaign.IsOpportunityCreationCampaign)
			{
				return;
			}

			ListValidation.ErrorIfInvalidCode(Parent.SourceDetailsInfo);
			CheckForInvalidChar(Parent.SourceDetailsInfo);
		}

		protected virtual void CheckActiveSourceDetails()
		{
			if (!Parent.Campaign.IsOpportunityCreationCampaign)
			{
				return;
			}

			CheckForInvalidChar(Parent.ActiveSourceDetailsInfo);
		}

		protected virtual void CheckOpportunityAssignment()
		{
			if (!Parent.Campaign.IsOpportunityCreationCampaign)
			{
				return;
			}

			MandatoryValidation.CheckEntered(Parent.OpportunityAssignmentInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OpportunityAssignmentInfo);
			CheckForInvalidChar(Parent.OpportunityAssignmentInfo);

			if (Parent.OpportunityAssignment == OpportunityAssignmentList.Codes.MatchParentTouchSender)
			{
				if (!Parent.Campaign.HasPreviousTouches)
				{
					Parent.OpportunityAssignmentInfo.AddError(Res.GetString("DA7DF43B-C1BA-4853-B23D-4A5A97CAA1E9", "Match Parent Touch Assignment cannot be selected in first touch series."));
				}
			}

			if (Parent.OpportunityAssignment == OpportunityAssignmentList.Codes.StaffPoolAssignments)
			{
				var parentCampaignSenderPool = Parent.Campaign.SenderPool;
				parentCampaignSenderPool.ValidateAll();

				if (parentCampaignSenderPool.IsNullOrEmpty())
				{
					Parent.OpportunityAssignmentInfo.AddError(Res.GetString("3B39C289-8E29-4493-954D-89A721500384", "Staff Pool Assignments cannot be empty."));
				}
				if (parentCampaignSenderPool.HasErrors())
				{
					Parent.OpportunityAssignmentInfo.AddError(Res.GetString("DF652C1C-BA58-40AD-9A04-ADC96213984F", "Errors in Staff Pool Assignments."));
				}
			}
		}

		protected virtual void CheckSalesPerson()
		{
			if (!Parent.Campaign.IsOpportunityCreationCampaign || Parent.OpportunityAssignment != OpportunityAssignmentList.Codes.IndividualSalesPerson)
			{
				return;
			}

			if (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value.GetBoolFromCode(OrgOpportunitySchema.Constants.P8_GS_NKPrimarySalesPerson))
			{
				MandatoryValidation.CheckEntered(Parent.SalesPersonInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.SalesPersonInfo);
			CheckForInvalidChar(Parent.SalesPersonInfo);
		}

		protected virtual void CheckStaffAssignment()
		{
			if (!Parent.Campaign.IsOpportunityCreationCampaign)
			{
				return;
			}

			ListValidation.ErrorIfInvalidCode(Parent.StaffAssignmentInfo);
			CheckForInvalidChar(Parent.StaffAssignmentInfo);
		}

		protected virtual void CheckOpportunityNotes()
		{
			if (!Parent.Campaign.IsOpportunityCreationCampaign)
			{
				return;
			}

			if (ORtfTextUtil.RtfToText(Parent.OpportunityNotes).Contains((Parent.Separator.ToString())))
			{
				Parent.OpportunityNotesInfo.AddError(InvalidCharacterMessage);
			}
		}

		protected void CheckForInvalidChar(ZPropertyInfo info)
		{
			if (info.Value is ZString stringValue)
			{
				if (stringValue.Contains(Parent.Separator))
				{
					info.AddError(InvalidCharacterMessage);
				}
			}
			else
			{
				info.AddError(InvalidCharacterMessage);
			}
		}
	}
}
