using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruiter.Business
{
	public class HRGlbCompanyCampaignLookups : GlbCompanyCampaignLookups
	{
		public HRGlbCompanyCampaignLookups(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public HRGlbCompanyCampaignLookups(HRGlbCompanyCampaign parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList EmailSenderOptionList
		{
			get
			{
				var list = new EmailSenderOptionCodeDescriptionList();
				list.RemoveCode(EmailSenderOptionCodeDescriptionList.Codes.ORG);
				return list;
			}
		}

		public override ReadOnlyCodeDescriptionPairList ActiveMediaTypesList
		{
			get
			{
				return Factory.GetCachedValue("HRGlbCompanyCampaignLookups.ActiveMediaTypesList", () =>
				{
					return OrganisationsDataRegistry.Instance.HRCampaignCategory2List.Value.GetActiveCodeDescriptionPairList();
				});
			}
		}

		public override ReadOnlyCodeDescriptionPairList MediaTypesList
		{
			get
			{
				return Factory.GetCachedValue("HRGlbCompanyCampaignLookups.MediaTypesListForDisplay", () =>
				{
					return OrganisationsDataRegistry.Instance.HRCampaignCategory2List.Value.GetCodeDescriptionPairList();
				});
			}
		}

		public override string MediaTypeLabel
		{
			get
			{
				return OrganisationsDataRegistry.Instance.HRCampaignCategory2Label.Value.ToString();
			}
		}

		public override ReadOnlyCodeDescriptionPairList ActiveMediaCategoryList
		{
			get
			{
				return Factory.GetCachedValue("HRGlbCompanyCampaignLookups.ActiveMediaCategoryList", () =>
				{
					return OrganisationsDataRegistry.Instance.HRCampaignCategory1List.Value.GetActiveCodeDescriptionPairList();
				});
			}
		}

		public override ReadOnlyCodeDescriptionPairList MediaCategoryList
		{
			get
			{
				return Factory.GetCachedValue("HRGlbCompanyCampaignLookups.MediaCategoryList", () =>
				{
					return OrganisationsDataRegistry.Instance.HRCampaignCategory1List.Value.GetCodeDescriptionPairList();
				});
			}
		}

		public override string MediaCategoryLabel
		{
			get
			{
				return OrganisationsDataRegistry.Instance.HRCampaignCategory1Label.Value.ToString();
			}
		}

		public CodeDescriptionPairList HRContactDataSourceList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("HRGlbCompanyCampaignLookups.HRContactDataSourceList", () =>
				{
					return new HRContactDataSourceList(Parent);
				});
			}
		}

		public CodeDescriptionPairList HRSimulationContactDataSourceList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("HRGlbCompanyCampaignLookups.HRSimulationContactDataSourceList", () =>
				{
					var list = new HRContactDataSourceList(Parent);
					list.RemoveCode(Business.HRContactDataSourceList.Codes.CampaignTracking);
					return list;
				});
			}
		}
	}
}
