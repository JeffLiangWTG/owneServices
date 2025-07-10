using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCompanyCampaignFindBoxListProvider : FindBoxListProvider
	{
		public GlbCompanyCampaignFindBoxListProvider(BusinessObjectCollection list, bool isHRCampaignModule = false)
			: base(list)
		{
			IsHRCampaignModule = isHRCampaignModule;
		}

		readonly bool IsHRCampaignModule;

		protected override void AddCodeStartsWithFilter(ZQuery query, string code)
		{
			if (!string.IsNullOrWhiteSpace(code))
			{
				query.AddToFilter(GetCampaignQuery(code));
			}
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithRelationshipFilter(string code)
		{
			return GetCampaignByCode(code);
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithoutFilter(string code)
		{
			return GetCampaignByCode(code);
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithCompleteFilter(string code)
		{
			return GetCampaignByCode(code);
		}

		IEnumerable<BusinessObject> GetCampaignByCode(string code)
		{
			return IsHRCampaignModule ?
				(IEnumerable<BusinessObject>)List.Factory.Load<IHRGlbCompanyCampaign>(GetCampaignQuery(code)) :
				(IEnumerable<BusinessObject>)List.Factory.Load<IGlbCompanyCampaign>(GetCampaignQuery(code));
		}

		ZQuery GetCampaignQuery(ZString code)
		{
			var query = new ZQuery();
			query.AddToFilter(GlbCompanyCampaignSchema.G0_CampaignID, code);

			return query;
		}
	}
}
