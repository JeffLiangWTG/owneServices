using System.Globalization;
using System.Runtime.CompilerServices;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSubscriptionLookups : AutoGlbCompanyCampaignSubscriptionLookups
	{
		public GlbCompanyCampaignSubscriptionLookups(AutoGlbCompanyCampaignSubscription parent)
			: base(parent)
		{
		}

		public GlbCompanyCampaignCollection Campaigns => ((GlbCompanyCampaignSubscription)Parent).GCS_IsHRCampaignSubscription ? new HRGlbCompanyCampaignCollection(Factory) : new GlbCompanyCampaignCollection(Factory);

		public ReadOnlyCodeDescriptionPairList MediaCategoryList => GetMediaCategoryList();

		public ReadOnlyCodeDescriptionPairList MediaCategoryWithAllList => GetMediaCategoryList(((GlbCompanyCampaignSubscription)Parent).ShouldHaveAllCampaignCategoryAndType);

		public ReadOnlyCodeDescriptionPairList MediaTypeList => GetMediaTypeList();

		public ReadOnlyCodeDescriptionPairList MediaTypeWithAllList => GetMediaTypeList(((GlbCompanyCampaignSubscription)Parent).ShouldHaveAllCampaignCategoryAndType);

		ReadOnlyCodeDescriptionPairList GetMediaTypeList(bool withAllValue = false, [CallerMemberName] string callerMember = null)
		{
			return GetCachedList(withAllValue, callerMember, ((GlbCompanyCampaignSubscription)Parent).GCS_IsHRCampaignSubscription ?
				OrganisationsDataRegistry.Instance.HRCampaignCategory2List.Value.GetCodeDescriptionPairList() :
				OrganisationsDataRegistry.Instance.CampaignCategory2List.Value.GetCodeDescriptionPairList(),
				ConstListValues.AllCampaignTypeDescription);
		}

		CodeDescriptionPairList GetMediaCategoryList(bool withAllValue = false, [CallerMemberName] string callerMember = null)
		{
			return GetCachedList(withAllValue, callerMember, ((GlbCompanyCampaignSubscription)Parent).GCS_IsHRCampaignSubscription ?
				OrganisationsDataRegistry.Instance.HRCampaignCategory1List.Value.GetCodeDescriptionPairList() :
				OrganisationsDataRegistry.Instance.CampaignCategory1List.Value.GetCodeDescriptionPairList(),
				ConstListValues.AllCampaignCategoryDescription);
		}

		CodeDescriptionPairList GetCachedList(bool withAllValue, string callerMember, CodeDescriptionPairList initialList, MultilingualString allValueDescription)
		{
			return Factory.GetCachedValue(GetCacheKey(((GlbCompanyCampaignSubscription)Parent).GCS_IsHRCampaignSubscription.ToString() + withAllValue.ToString() + callerMember), () =>
			{
				if (withAllValue)
				{
					initialList.AddPairIfNotExist(ConstListValues.AllCampaignCategoryAndTypeCode, allValueDescription);
				}
				initialList.Sort();
				return initialList;
			});
		}

		static string GetCacheKey(string callerMember)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", nameof(GlbCompanyCampaignSubscriptionLookups), callerMember);
		}

		public static class ConstListValues
		{
			public static MultilingualString AllCampaignCategoryDescription => ResString.GetMultilingualString("e7677386-66d3-4c34-bfe6-be2df8629e64", "All Categories");
			public static MultilingualString AllCampaignTypeDescription => ResString.GetMultilingualString("cbe3cd27-92db-44d6-bca3-0ceae2a1cd96", "All Types");
			public static ZString AllCampaignCategoryAndTypeCode { get; } = "ALL";
		}
	}
}
