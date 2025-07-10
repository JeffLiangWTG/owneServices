using System;
using System.Web;
using CargoWise.Application;
using Enterprise.Tracking.Business;

namespace Enterprise.Tracking.Web
{
	public partial class GlowRedirect : BasePageWithAuthorisation
	{
		protected override void OnLoad(EventArgs e)
		{
			if (CanAccessAuthorisedContent)
			{
				var glowURL = GlowTrackingUrlGenerator.GenerateURL(SiteUser.LoggedInUser.PK);
				if (glowURL != null)
				{
					HttpContext.Current.Response.Redirect(glowURL.AbsoluteUri);
				}
			}

			base.OnLoad(e);
		}

		protected override bool CanAccessAuthorisedContent => SiteUser.CanAccessGlowTrackingPortal;

		IGlowTrackingUrlGenerator GlowTrackingUrlGenerator => glowTrackingUrlGenerator ?? (glowTrackingUrlGenerator = ObjectFactory.Get<IGlowTrackingUrlGenerator>());
		IGlowTrackingUrlGenerator glowTrackingUrlGenerator;
	}
}
