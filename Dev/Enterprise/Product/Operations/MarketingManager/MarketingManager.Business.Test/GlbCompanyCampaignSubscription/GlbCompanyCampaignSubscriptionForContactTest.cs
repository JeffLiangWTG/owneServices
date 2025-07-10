
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignSubscriptionForContact))]
	sealed class GlbCompanyCampaignSubscriptionForContactTest : GlbCompanyCampaignSubscriptionTest<GlbCompanyCampaignSubscriptionForContact>
	{
		public override void TestMediaCategory_ReadOnly()
		{
			base.TestMediaCategory_ReadOnly();

			var gccs = Factory.New<GlbCompanyCampaignSubscriptionForContact>();
			AssertEquals("Should be not readonly", false, gccs.GCS_MediaCategoryInfo.ReadOnly);
			gccs.GCS_OH = ZGuid.BrettsGuid;
			AssertEquals("Should be readonly", true, gccs.GCS_MediaCategoryInfo.ReadOnly);
		}

		public override void TestMediaCategoryWithAll_ReadOnly()
		{
			base.TestMediaCategoryWithAll_ReadOnly();

			var gccs = Factory.New<GlbCompanyCampaignSubscriptionForContact>();
			AssertEquals("Should be not readonly", false, gccs.GCS_MediaCategoryWithAllInfo.ReadOnly);
			gccs.GCS_OH = ZGuid.BrettsGuid;
			AssertEquals("Should be readonly", true, gccs.GCS_MediaCategoryWithAllInfo.ReadOnly);
		}

		public override void TestMediaType_ReadOnly()
		{
			base.TestMediaType_ReadOnly();

			var gccs = Factory.New<GlbCompanyCampaignSubscriptionForContact>();
			AssertEquals("Should be not readonly", false, gccs.GCS_MediaTypeInfo.ReadOnly);
			gccs.GCS_OH = ZGuid.BrettsGuid;
			AssertEquals("Should be readonly", true, gccs.GCS_MediaTypeInfo.ReadOnly);
		}

		public override void TestMediaTypeWithAll_ReadOnly()
		{
			base.TestMediaTypeWithAll_ReadOnly();

			var gccs = Factory.New<GlbCompanyCampaignSubscriptionForContact>();
			AssertEquals("Should be not readonly", false, gccs.GCS_MediaTypeWithAllInfo.ReadOnly);
			gccs.GCS_OH = ZGuid.BrettsGuid;
			AssertEquals("Should be readonly", true, gccs.GCS_MediaTypeWithAllInfo.ReadOnly);
		}

		public override void TestSubscriptionType_ReadOnly()
		{
			base.TestSubscriptionType_ReadOnly();

			var gccs = Factory.New<GlbCompanyCampaignSubscriptionForContact>();
			AssertEquals("Should not be readonly", false, gccs.GCS_IsSubscribedInfo.ReadOnly);
			gccs.GCS_OH = ZGuid.BrettsGuid;
			AssertEquals("Should be readonly", true, gccs.GCS_IsSubscribedInfo.ReadOnly);
		}

		public void TestBusinessObjectReadOnly()
		{
			var gccs = Factory.New<GlbCompanyCampaignSubscriptionForContact>();
			AssertEquals("Should not be readonly", false, gccs.ReadOnly);
			gccs.GCS_OH = ZGuid.BrettsGuid;
			AssertEquals("Should not be readonly", false, gccs.ReadOnly);
		}

		public void TestBusinessObjectCanDelete()
		{
			var gccs = Factory.New<GlbCompanyCampaignSubscriptionForContact>();
			AssertEquals("Able to delete", true, gccs.CanDelete);
			gccs.GCS_OH = ZGuid.BrettsGuid;
			AssertEquals("Unable to delete", false, gccs.CanDelete);
		}
	}
}
