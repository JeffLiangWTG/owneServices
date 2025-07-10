//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbCompanyCampaignLinkValidation
//
//    This class should be used for overriding validation in AutoGlbCompanyCampaignLinkValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignLinkValidation : AutoGlbCompanyCampaignLinkValidation
	{
		public GlbCompanyCampaignLinkValidation(AutoGlbCompanyCampaignLink parent)
			: base(parent)
		{
		}

		public new GlbCompanyCampaignLink Parent { get { return (GlbCompanyCampaignLink)base.Parent; } }

		protected override void CheckGCL_Context()
		{
			MandatoryValidation.CheckEntered(Parent.GCL_ContextInfo);
			var parent = Parent;
			if (parent.HasTrackingMacro || parent.IsInDatabase)
			{
				foreach (var collection in ((IBusinessObjectInternals)parent).ParentCollections)
				{
					foreach (GlbCompanyCampaignLink other in collection)
					{
						if (other.PK != parent.PK &&
							other.HasTrackingMacro &&
							(other.GCL_G0_Campaign == parent.GCL_G0_Campaign || other.GCL_G0_Campaign.IsEmpty || parent.GCL_G0_Campaign.IsEmpty) &&
							other.GCL_Context.EqualsIgnoringCase(parent.GCL_Context))
						{
							parent.GCL_ContextInfo.AddError(Res.GetString("77473C11-4BD9-400E-B348-87212EF3E73F", "Duplicate value exists. Please choose a unique value for each tracked link."));
						}
					}
				}
			}
		}

		protected override void CheckGCL_URL()
		{
			base.CheckGCL_URL();
			MandatoryValidation.CheckEntered(Parent.GCL_URLInfo);

			if (!Parent.GCL_URL.IsEmpty)
			{
				Uri uriResult;
				Uri.TryCreate(Parent.GCL_URL, UriKind.Absolute, out uriResult);
				if (uriResult == null || !uriResult.IsWellFormedOriginalString() || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
				{
					Parent.GCL_URLInfo.AddError(Res.GetString("f2a3c2d4-3711-4a12-b1a6-70678e416bd8", @"Enter a valid URL starts with ""{0}"" or ""{1}"".", @"http://", @"https://"));
				}
			}
		}
	}
}
