using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSubscriptionValidation : AutoGlbCompanyCampaignSubscriptionValidation
	{
		public GlbCompanyCampaignSubscriptionValidation(AutoGlbCompanyCampaignSubscription parent)
			: base(parent)
		{
		}

		protected override void CheckGCS_MediaCategory()
		{
			base.CheckGCS_MediaCategory();

			var parent = (GlbCompanyCampaignSubscription)Parent;
			if (!parent.IsInDatabase || parent.GCS_MediaCategoryWithAllInfo.HasChanges || !parent.Lookups.MediaCategoryWithAllList.ContainsCode(parent.GCS_MediaCategoryWithAll))
			{
				MandatoryValidation.CheckEntered(parent.GCS_MediaCategoryWithAllInfo);
				ListValidation.ErrorIfInvalidCode(parent.GCS_MediaCategoryWithAllInfo);
			}
			else
			{
				MandatoryValidation.WarnIfNotEntered(parent.GCS_MediaCategoryWithAllInfo);
				ListValidation.WarnIfInvalidCode(parent.GCS_MediaCategoryWithAllInfo, parent.Lookups.MediaCategoryWithAllList, ListValidation.InactiveCodeMessage);
			}
		}

		protected override void CheckGCS_MediaType()
		{
			base.CheckGCS_MediaType();

			var parent = (GlbCompanyCampaignSubscription)Parent;
			if (!parent.IsInDatabase || parent.GCS_MediaTypeWithAllInfo.HasChanges || !parent.Lookups.MediaTypeWithAllList.ContainsCode(parent.GCS_MediaTypeWithAll))
			{
				MandatoryValidation.CheckEntered(parent.GCS_MediaTypeWithAllInfo);
				ListValidation.ErrorIfInvalidCode(parent.GCS_MediaTypeWithAllInfo);
			}
			else
			{
				MandatoryValidation.WarnIfNotEntered(parent.GCS_MediaTypeWithAllInfo);
				ListValidation.WarnIfInvalidCode(parent.GCS_MediaTypeWithAllInfo, parent.Lookups.MediaTypeWithAllList, ListValidation.InactiveCodeMessage);
			}
		}

		protected override void CheckGCS_Email()
		{
			if (Parent.GCS_OH.IsEmpty || Parent.GCS_OH.IsDefault)
			{
				MandatoryValidation.CheckEntered(Parent.GCS_EmailInfo);
			}
			else
			{
				MandatoryValidation.CheckNotEntered(Parent.GCS_EmailInfo);
			}
		}

		protected override void CheckGCS_OHIsValidZGuid()
		{
			if (Parent.GCS_Email.IsEmpty)
			{
				base.CheckGCS_OHIsValidZGuid();
				MandatoryValidation.CheckEntered(Parent.GCS_OHInfo);
			}
			else
			{
				MandatoryValidation.CheckNotEntered(Parent.GCS_OHInfo);
			}
		}
	}
}
