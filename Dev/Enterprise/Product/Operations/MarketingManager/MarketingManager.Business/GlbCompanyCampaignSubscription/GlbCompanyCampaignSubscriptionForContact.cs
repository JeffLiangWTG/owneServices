using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSubscriptionForContact : GlbCompanyCampaignSubscription
	{
		public GlbCompanyCampaignSubscriptionForContact(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool CanDelete => base.CanDelete && GCS_OH.IsEmpty;

		protected override bool GCS_MediaCategory_ReadOnly => !GCS_OH.IsEmpty;
		protected override bool GCS_MediaType_ReadOnly => !GCS_OH.IsEmpty;
		protected override bool GCS_IsSubscribed_ReadOnly => !GCS_OH.IsEmpty;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return !GCS_OH.IsEmpty ? ResString.GetMultilingualString("f8dc79b9-3604-4820-924e-c7882b61923f", "Cannot delete - this is an Organization level Subscription.") : base.ReasonForNotAbleToDelete; }
		}

		[List("Lookups.MediaCategoryList")]
		public override ZString GCS_MediaCategory
		{
			get { return base.GCS_MediaCategory; }
			set
			{
				if (base.GCS_MediaCategory != value)
				{
					base.GCS_G0 = ZGuid.Empty;
				}
				base.GCS_MediaCategory = value;
			}
		}

		[List("Lookups.MediaTypeList")]
		public override ZString GCS_MediaType
		{
			get { return base.GCS_MediaType; }
			set
			{
				if (base.GCS_MediaType != value)
				{
					base.GCS_G0 = ZGuid.Empty;
				}
				base.GCS_MediaType = value;
			}
		}

		public override ZBool GCS_IsSubscribed
		{
			get { return base.GCS_IsSubscribed; }
			set
			{
				if (base.GCS_IsSubscribed != value)
				{
					base.GCS_G0 = ZGuid.Empty;
				}
				base.GCS_IsSubscribed = value;
			}
		}
	}
}
