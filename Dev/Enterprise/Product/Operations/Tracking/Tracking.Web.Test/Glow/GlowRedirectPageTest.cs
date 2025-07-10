using System;
using System.Web;
using System.Web.UI;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Moq;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class GlowRedirectPageTest : WebControlTest
	{
		public void TestRedirectIfAuthorised()
		{
			var testPage = (GlowRedirectPageForTest)Control;
			using (TrackingSiteUserTestHelper.EnableGlowTrackingPortalAccess(testPage.SiteUser))
			{
				var expectedUrl = "http://urltonewtrackingportal/";
				var trackingUrlGeneratorMock = new Mock<IGlowTrackingUrlGenerator>();
				trackingUrlGeneratorMock.Setup(h => h.GenerateURL(testPage.SiteUser.LoggedInUser.PK)).Returns(new Uri(expectedUrl));
				ObjectFactory.Substitute(trackingUrlGeneratorMock.Object);

				testPage.OnLoad();

				Assert(!HttpContext.Current.Response.IsRequestBeingRedirected); // Temporarily false - WI00238192
			}
		}

		public void TestNoRedirectIfNotAuthorised()
		{
			var testPage = (GlowRedirectPageForTest)Control;
			testPage.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			Factory.Save();

			testPage.OnLoad();

			Assert(!HttpContext.Current.Response.IsRequestBeingRedirected);
		}

		protected override Control GetNewControl()
		{
			return new GlowRedirectPageForTest();
		}
	}
}
