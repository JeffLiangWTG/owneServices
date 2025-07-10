using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class HRRecruitmentJobCampaignFindBoxListProvider : FindBoxListProvider
	{
		public HRRecruitmentJobCampaignFindBoxListProvider(BusinessObjectCollection collection)
			: base(collection)
		{ }

		public override bool AutoCompleteOnCommit
		{
			get { return false; }
		}

		public override (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
		{
			return (code, false);
		}

		protected override void AddCodeStartsWithFilter(ZQuery query, string code)
		{
			if (!string.IsNullOrWhiteSpace(code))
			{
				query.AddToFilter(GetCampaignQuery(code));
			}
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithRelationshipFilter(string code)
		{
			return GetJobCampaignFromCode(code);
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithCompleteFilter(string code)
		{
			return GetJobCampaignFromCode(code);
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithoutFilter(string code)
		{
			return GetJobCampaignFromCode(code);
		}

		IEnumerable<BusinessObject> GetJobCampaignFromCode(string code)
		{
			return List.Factory.Load<HRRecruitmentJobCampaign>(GetCampaignQuery(code));
		}

		public override ZGuid PrimaryKeyFromCode(string code)
		{
			BusinessObject bizO = BizObjFromCodeWithCompleteFilter(code);
			return bizO != null ? bizO.PK : ZGuid.Invalid;
		}

		ZQuery GetCampaignQuery(ZString code)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(HRRecruitmentJobCampaign));

			ZString adTitle = ZString.Empty;
			ZDateTime campaignStartDate = ZDateTime.Empty;
			ZString campaignUNLOCO = ZString.Empty;

			HRRecruitmentJobCampaign.GetInfoFromCampaignID(code, out adTitle, out campaignStartDate, out campaignUNLOCO);

			if (!adTitle.IsEmpty)
			{
				query.AddToFilter(HRRecruitmentJobCampaignSchema.HV_AdTitle, adTitle);
			}

			if (campaignStartDate.IsValid)
			{
				query.AddToFilter(HRRecruitmentJobCampaignSchema.HV_CampaignStartDate, SQLComparisonOperator.EqualToDatePartOnly, campaignStartDate);
			}
			else if (campaignStartDate.IsEmpty)
			{
				query.AddToFilter(HRRecruitmentJobCampaignSchema.HV_CampaignStartDate, null);
			}

			if (!campaignUNLOCO.IsEmpty)
			{
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgAddress), HRRecruitmentJobCampaignSchema.HV_OA_ClientAddress);
				subQuery.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, campaignUNLOCO);
				query.AddSubQuery(subQuery, JoinCondition.And);
			}
			else
			{
				query.AddToFilter(HRRecruitmentJobCampaignSchema.HV_OA_ClientAddress, null);
			}

			if (!campaignStartDate.IsValid && campaignUNLOCO.IsEmpty && adTitle.IsEmpty)
			{
				query.IsNoResultQuery = true;
			}

			return query;
		}
	}
}
