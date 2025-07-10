using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class HRRecruitmentJobCampaignValidation : AutoHRRecruitmentJobCampaignValidation
	{
		public HRRecruitmentJobCampaignValidation(AutoHRRecruitmentJobCampaign parent) : base(parent)
		{
		}

		public new HRRecruitmentJobCampaign Parent
		{
			get { return (HRRecruitmentJobCampaign)base.Parent; }
		}

		#region Staff Controlled By

		protected override void CheckHV_GS_NKControlledBy()
		{
			base.CheckHV_GS_NKControlledBy();
			MandatoryValidation.CheckEntered(Parent.HV_GS_NKControlledByInfo);
			ListValidation.ErrorIfInvalidCode(Parent.HV_GS_NKControlledByInfo, Parent.Lookups.ControlledBys);
		}

		#endregion

		protected override void CheckHV_WageHigh()
		{
			base.CheckHV_WageHigh();
			CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(Parent.HV_WageHighInfo, Parent.HV_WageLowInfo);
		}

		protected override void CheckHV_WageLow()
		{
			base.CheckHV_WageLow();
			CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Parent.HV_WageLowInfo, Parent.HV_WageHighInfo);
		}

		#region Wage Range Currency

		protected override void CheckHV_RX_NKWageRangeCurrency()
		{
			base.CheckHV_RX_NKWageRangeCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.HV_RX_NKWageRangeCurrencyInfo);
		}

		#endregion

		#region Client Contact

		protected override void CheckHV_OC_ClientContact()
		{
			base.CheckHV_OC_ClientContact();
			if (Parent.ClientContact != null)
			{
				if (Parent.ClientAccount == null && !Parent.HV_OC_ClientContact.IsEmpty)
				{
					Parent.HV_OC_ClientContactInfo.AddError(Res.GetString("82989872-0c88-4144-bd01-cb7edaac3dae", "Please select a Client Account and Address before entering a Client Contact"));
				}

				if (Parent.ClientAccount != null && Parent.ClientContact.OC_OH != Parent.ClientAccount.PK)
				{
					Parent.HV_OC_ClientContactInfo.AddError(Res.GetString("962e864c-0c67-4c9c-9ccd-a07fc2d82902", "Please choose a valid Client Contact"));
				}
			}
		}

		#endregion

		#region Campaign Start Date

		protected override void CheckHV_CampaignStartDate()
		{
			base.CheckHV_CampaignStartDate();
			if (Parent.HV_CampaignStartDate.IsValid && Parent.HV_CampaignEndDate.IsValid)
			{
				if (Parent.HV_CampaignStartDate.CompareTo(Parent.HV_CampaignEndDate) > 0)
				{
					Parent.HV_CampaignStartDateInfo.AddError(Res.GetString("16774e67-5130-4131-9bd4-cce71ddd8d35", "The Campaign Start Date should not be after the Campaign End Date"));
				}
			}
		}

		#endregion

		#region Campaign End Date

		protected override void CheckHV_CampaignEndDate()
		{
			base.CheckHV_CampaignEndDate();
			if (Parent.HV_CampaignStartDate.IsValid && Parent.HV_CampaignEndDate.IsValid)
			{
				if (Parent.HV_CampaignEndDate.CompareTo(Parent.HV_CampaignStartDate) < 0)
				{
					Parent.HV_CampaignEndDateInfo.AddError(Res.GetString("ec3a4b31-df50-4a67-9ca0-82cf7c4ff143", "The Campaign End Date should not be before the Campaign Start Date"));
				}
			}
		}

		#endregion

		#region Job Role

		protected override void CheckHV_HJ_JobRole()
		{
			base.CheckHV_HJ_JobRole();
			MandatoryValidation.CheckEntered(Parent.HV_HJ_JobRoleInfo);
		}

		#endregion

		#region CampaignID

		public void ValidateCampaignID()
		{
			ValidateCalculatedProperty(Parent.CampaignIDInfo);
		}

		protected virtual void CheckCampaignID()
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(HRRecruitmentJobCampaign));

			if (!Parent.HV_AdTitle.IsEmpty)
			{
				filter.AddToFilter(HRRecruitmentJobCampaignSchema.HV_AdTitle, Parent.HV_AdTitle);
			}

			if (Parent.HV_CampaignStartDate.IsValid)
			{
				filter.AddToFilter(HRRecruitmentJobCampaignSchema.HV_CampaignStartDate, Parent.HV_CampaignStartDate);
			}
			else if (Parent.HV_CampaignStartDate.IsEmpty)
			{
				filter.AddToFilter(HRRecruitmentJobCampaignSchema.HV_CampaignStartDate, null);
			}

			if (!Parent.CampaignUNLOCO.IsEmpty)
			{
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgAddress), HRRecruitmentJobCampaignSchema.HV_OA_ClientAddress);
				subQuery.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, Parent.CampaignUNLOCO);
				filter.AddSubQuery(subQuery, JoinCondition.And);
			}
			else
			{
				filter.AddToFilter(HRRecruitmentJobCampaignSchema.HV_OA_ClientAddress, null);
			}
			filter.AddToFilter(HRRecruitmentJobCampaignSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			if (Parent.Factory.ExistsInDatabase(HRRecruitmentJobCampaignSchema.Constants.TableName, filter))
			{
				Parent.CampaignIDInfo.AddError(Res.GetString("9663be76-c572-4b60-9489-11f3230356ec", "A campaign already exists with the same Ad Title, Start Date, and Client Address. Please ensure do not enter duplicate campaigns."));
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCampaignID();
		}

		#endregion
	}
}
