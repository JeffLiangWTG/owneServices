using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.Tracking.Web
{
	public class TrackingLoginRouter : LoginRouter
	{
		public TrackingLoginRouter(Uri originalUrl, string token) : base(originalUrl, token)
		{
		}

		public TrackingLoginRouter(Uri originalUrl, OrgContact contact) : base(originalUrl, contact)
		{
		}

		protected override Uri DefaultUrl => new Uri(WebDataRegistry.Instance.WebTrackerUrl.GetFallBackValueAtAllLevels(Contact?.CompanyPKForLogin ?? Guid.Empty, Guid.Empty, Guid.Empty));

		protected override ZGlobal GetNewGlobal()
		{
			return new Global();
		}
	}
}
