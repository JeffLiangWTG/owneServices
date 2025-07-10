using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Recruiter.Business
{
	[DependentBusinessObject(typeof(HRRecruitmentJobCampaign), "AdPlacements")]
	public class HRJobAdPlacement : AutoHRJobAdPlacement
	{
		public HRJobAdPlacement(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoHRJobAdPlacement.Schema
		{
			public const string HQ_EffectiveStartDateLocal = "HQ_EffectiveStartDateLocal";
			public const string HQ_EffectiveEndDateLocal = "HQ_EffectiveEndDateLocal";
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			hq_EffectiveStartDateLocal = HQ_EffectiveStartDate.IsValid ? Env.Time.GetUnlocoTimeFromUtc(Campaign.CampaignDateUNLOCO, HQ_EffectiveStartDate.ToDateTime()) : HQ_EffectiveStartDate;
			hq_EffectiveEndDateLocal = HQ_EffectiveEndDate.IsValid ? Env.Time.GetUnlocoTimeFromUtc(Campaign.CampaignDateUNLOCO, HQ_EffectiveEndDate.ToDateTime()) : HQ_EffectiveEndDate;
		}

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			HQ_HV = campaign.PK;
		}

#endif
		#endregion

		#region Properties

		#region HQ_EffectiveStartDateLocal

		public ZDateTime HQ_EffectiveStartDateLocal
		{
			get { return hq_EffectiveStartDateLocal; }
			set
			{
				hq_EffectiveStartDateLocal = value.Date;
				HQ_EffectiveStartDate = value.IsValid && Campaign != null ? (ZDateTime)Env.Time.GetUtcFromUnlocoTime(Campaign.CampaignDateUNLOCO, value.Date.ToDateTime()) : value.Date;

				if (!IsValidationSuspended)
				{
					Validation.ValidateHQ_EffectiveEndDate();
				}
			}
		}
		ZDateTime hq_EffectiveStartDateLocal;

		public ZPropertyInfo HQ_EffectiveStartDateLocalInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.HQ_EffectiveStartDateLocal, x => HQ_EffectiveStartDateInfo); }
		}

		#endregion

		#region HQ_EffectiveEndDateLocal

		public ZDateTime HQ_EffectiveEndDateLocal
		{
			get { return hq_EffectiveEndDateLocal; }
			set
			{
				hq_EffectiveEndDateLocal = value.Date;
				HQ_EffectiveEndDate = value.IsValid && Campaign != null ? (ZDateTime)Env.Time.GetUtcFromUnlocoTime(Campaign.CampaignDateUNLOCO, value.Date.ToDateTime()) : value.Date;

				if (!IsValidationSuspended)
				{
					Validation.ValidateHQ_EffectiveStartDate();
				}
			}
		}
		ZDateTime hq_EffectiveEndDateLocal;

		public ZPropertyInfo HQ_EffectiveEndDateLocalInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.HQ_EffectiveEndDateLocal, x => HQ_EffectiveEndDateInfo); }
		}

		#endregion

		#endregion

		#region Related Business Objects

		#region Campaign

		[RelatedBusinessObject("Campaign")]
		public override ZGuid HQ_HV
		{
			get { return base.HQ_HV; }
			set { base.HQ_HV = value; }
		}

		public HRRecruitmentJobCampaign Campaign
		{
			get { return Factory.Load<HRRecruitmentJobCampaign>(HQ_HV); }
		}

		#endregion

		#endregion

	}
}
