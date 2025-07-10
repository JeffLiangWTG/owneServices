using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Tracking.Business.Testing
{
	public static class TrackingSiteUserTestHelper
	{
		public static IDisposable EnableGlowTrackingPortalAccess(TrackingSiteUser siteUser)
		{
			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "X";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "user@user.com";
			contact.SetHashedPassword("password");
			contact.OC_WebAccessEnabled = true;

			var contactSecurity = new List<OrgSecurityContacts>();
			var orgRight = org.SecurityRights.AddNew();
			orgRight.OX_Granted = true;
			orgRight.OX_SecurityItemName = WebSecurityRightsList.TrackingPortal.Code;
			var userRight = contact.SecurityRightsForBindingOnly.AddNew();
			userRight.OZ_OX = orgRight.PK;
			userRight.OZ_Granted = true;
			contactSecurity.Add(userRight);

			factory.Save();

			var originalShowValue = WebDataRegistry.Instance.ShowNewTrackingPortal.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var originalPortalsUrl = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			WebDataRegistry.Instance.ShowNewTrackingPortal.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glow");

			siteUser.Login(org.OH_Code, "user@user.com", "password");

			return new DisposableAction(() =>
			{
				WebDataRegistry.Instance.ShowNewTrackingPortal.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalShowValue);
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalPortalsUrl);
			});
		}
	}
}
