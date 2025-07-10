using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicationValidation : AutoHRJobApplicationValidation
	{
		public HRJobApplicationValidation(AutoHRJobApplication parent) : base(parent)
		{
		}

		#region Staff Assigned To

		protected override void CheckHP_GS_NKAssignedTo()
		{
			base.CheckHP_GS_NKAssignedTo();
			ListValidation.ErrorIfInvalidCode(Parent.HP_GS_NKAssignedToInfo, Parent.Lookups.AssignedTos);
		}

		#endregion

		#region Duplicate Application

		protected override void CheckHP_HV()
		{
			base.CheckHP_HV();
			if (DuplicateApplicationExists)
			{
				Parent.HP_HVInfo.AddError(Res.GetString("bb58052b-b1ff-42eb-a7e3-e99a069c0e34", "There is already an application for this campaign from this applicant"));
			}
		}

		protected override void CheckHP_HA()
		{
			base.CheckHP_HA();
			if (DuplicateApplicationExists)
			{
				Parent.HP_HAInfo.AddError(Res.GetString("8c8d9d28-b179-4663-9d0e-7f4157a698d5", "There is already an application from this applicant on this campaign"));
			}
			if (Parent.Applicant != null && !Parent.Applicant.IsInDatabase && Parent.Applicant.HA_FullName.IsEmpty)
			{
				Parent.HP_HAInfo.AddError(Res.GetString("8FAE3E1E-D41F-4DB0-A156-0FFC0C1079BC", "Please enter a Full Name."));
			}
		}

		bool DuplicateApplicationExists
		{
			get
			{
				if (!Parent.HP_HA.IsValid)
				{
					return false;
				}

				var duplicateApplicationFilter = new ZQuery(HRJobApplicationSchema.HP_HA, Parent.HP_HA);
				duplicateApplicationFilter.AddToFilter(HRJobApplicationSchema.HP_HV, Parent.HP_HV.IsEmpty ? null : Parent.HP_HV);
				duplicateApplicationFilter.AddToFilter(HRJobApplicationSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				var duplicateApplications = Parent.Factory.Load<HRJobApplication>(duplicateApplicationFilter);

				return !Parent.HP_HV.IsEmpty && duplicateApplications.Length != 0;
			}
		}

		#endregion

		#region Application Status

		protected override void CheckHP_CurrentStatus()
		{
			base.CheckHP_CurrentStatus();
			ListValidation.ErrorIfInvalidCode(Parent.HP_CurrentStatusInfo, Parent.Lookups.ApplicationStatuses);
		}

		#endregion

		#region ApplicationOverallRating

		protected override void CheckHP_ApplicationOverallRating()
		{
			base.CheckHP_ApplicationOverallRating();
			if (!Parent.IsInDatabase || Parent.HP_ApplicationOverallRatingInfo.HasChanges)
			{
				ListValidation.ErrorIfInvalidCode(Parent.HP_ApplicationOverallRatingInfo);
			}
		}

		public void ValidateApplicationOverallRatingDescription()
		{
			ValidateCalculatedProperty(Parent.ApplicationOverallRatingDescriptionInfo);
		}

		protected virtual void CheckApplicationOverallRatingDescription()
		{
			var description = Parent.ApplicationOverallRatingDescription;
			if (!description.IsEmpty && Parent.Lookups.OverallRatings.GetCodeFromDescription(description) == null)
			{
				Parent.ApplicationOverallRatingDescriptionInfo.AddError(Res.GetString("171d0133-84bb-4185-9fd3-1a9e950375e3", "Please enter a valid value"));
			}
		}

		#endregion

		#region Referring Party

		protected override void CheckHP_SourceType()
		{
			base.CheckHP_SourceType();

			if (!Parent.IsInDatabase || Parent.HP_SourceTypeInfo.HasChanges)
			{
				MandatoryValidation.CheckEntered(Parent.HP_SourceTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.HP_SourceTypeInfo, Parent.Lookups.AllSourceTypes);
			}
		}

		protected override void CheckHP_OH_ReferringOrganisation()
		{
			base.CheckHP_OH_ReferringOrganisation();

			if (Parent.IsReferringOrganisationMandatory)
			{
				MandatoryValidation.CheckEntered(Parent.HP_OH_ReferringOrganisationInfo);
			}
		}

		protected override void CheckHP_PER_ReferringPerson()
		{
			base.CheckHP_PER_ReferringPerson();

			if (Parent.IsReferringOrganisationMandatory && Parent.IsReferringPersonApplicable && Parent.ReferringPerson != null && !Parent.ReferringPerson.ContactCollection.Any())
			{
				Parent.HP_PER_ReferringPersonInfo.AddError(Res.GetString("2fb39573-8b3d-4dc1-8273-11189a26fc50", "This Person does not have an Organization Contact Context."));
			}
			else if (Parent.ReferringOrganisation != null && Parent.ReferringPerson != null && !Parent.ReferringPerson.ContactCollection.OfType<OrgContact>().Any(x => x.OC_OH == Parent.HP_OH_ReferringOrganisation))
			{
				Parent.HP_PER_ReferringPersonInfo.AddError(Res.GetString("2c78c901-67b8-4b1a-ac09-4d61bbeea372", "This Person does not have a Related Organization {0}.", Parent.ReferringOrganisation.OH_Code));
			}
		}

		public void ValidateReferringStaffCode()
		{
			ValidateCalculatedProperty(Parent.ReferringStaffCodeInfo);
		}

		protected virtual void CheckReferringStaffCode()
		{
			if (Parent.HP_SourceType.EqualsIgnoringCase(ReferringSourcesTypes.Codes.StaffReferral) && !Parent.ReferringStaffCode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.ReferringStaffCodeInfo);

				if (!Parent.HP_PER_ReferringPerson.IsValid && !Parent.ReferringStaffCodeInfo.HasErrors())
				{
					Parent.ReferringStaffCodeInfo.AddError(Res.GetString("d379e722-80b7-47c7-9626-58f4cf8230ea", "This Staff is not linked to a Person."));
				}
			}
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateApplicationOverallRatingDescription();
			ValidateReferringStaffCode();
		}

		protected new HRJobApplication Parent => (HRJobApplication)base.Parent;
	}
}
