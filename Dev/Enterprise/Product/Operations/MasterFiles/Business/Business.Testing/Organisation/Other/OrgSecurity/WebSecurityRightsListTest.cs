using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.WebSecurityRight;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class WebSecurityRightsListTest : WebSecurityRightsListTestCase
	{
		public virtual void TestDeniedByDefaultRegistry()
		{
			using (OrganisationRegistry.Instance.WebSecurityRightsDeniedByDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var list = WebSecurityRightsList.New();
				WebSecurityRight securityRight;
				Assert(list.TryGetValue("Web Quoting", out securityRight));
				AssertEquals("granted by default, denied due to security right", false, securityRight.IsGrantedByDefault);
				Assert(list.TryGetValue("Web Publish Layouts", out securityRight));
				AssertEquals("denied by default", false, securityRight.IsGrantedByDefault);
			}

			using (OrganisationRegistry.Instance.WebSecurityRightsDeniedByDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var list = WebSecurityRightsList.New();
				WebSecurityRight securityRight;
				Assert(list.TryGetValue("Web Quoting", out securityRight));
				AssertEquals("granted by default", true, securityRight.IsGrantedByDefault);
				Assert(list.TryGetValue("Web Publish Layouts", out securityRight));
				AssertEquals("denied by default", false, securityRight.IsGrantedByDefault);
			}
		}

		public void TestETailWebSecurityRightsV2()
		{
			GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebSecurityRightsListForTest.RegisterThisSubTypeOverride();

			var list = WebSecurityRightsList.New();
			WebSecurityRight securityRight;
			CombineAssertions("Test new rights", () =>
			{
				Assert(list.TryGetValue("eCommerce Shipper Portal", out securityRight));
				AssertEquals("e-Commerce - Shipper Portal", securityRight.Description);
				Assert(list.TryGetValue("eCommerce Origin Depot Portal", out securityRight));
				AssertEquals("e-Commerce - Origin Depot Portal", securityRight.Description);
				Assert(list.TryGetValue("eCommerce Destination Depot Portal", out securityRight));
				AssertEquals("e-Commerce - Destination Depot Portal", securityRight.Description);
				Assert(list.TryGetValue("eCommerce View Carriers and Depots", out securityRight));
				AssertEquals("e-Commerce - Carriers & Depots (View)", securityRight.Description);
				Assert(list.TryGetValue("eCommerce Confirm Booking Header", out securityRight));
				AssertEquals("e-Commerce - Confirm HVLV Booking Header", securityRight.Description);
				Assert(list.TryGetValue("eCommerce Receive Booking Header", out securityRight));
				AssertEquals("e-Commerce - Receive HVLV Booking Header", securityRight.Description);
				Assert(list.TryGetValue("eCommerce Lodge Origin Load List", out securityRight));
				AssertEquals("e-Commerce - Lodge HVLV Origin Load List", securityRight.Description);
				Assert(list.TryGetValue("eCommerce Calculate Depot and LMC", out securityRight));
				AssertEquals("e-Commerce - Calculate Depot and Last Mile Carrier Details", securityRight.Description);
				Assert(list.TryGetValue("eCommerce Last Mile Carrier Booking", out securityRight));
				AssertEquals("e-Commerce - Last Mile Carrier Booking", securityRight.Description);
			});
		}

		public void TestUSAMSWebSecurityRights()
		{
			GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebSecurityRightsListForTest.RegisterThisSubTypeOverride();

			var list = WebSecurityRightsList.New();
			WebSecurityRight securityRight;
			CombineAssertions("Test new rights", () =>
			{
				Assert(list.TryGetValue("US AMS (Add/Edit)", out securityRight));
				AssertEquals("US AMS (Add/Edit)", securityRight.Description);
				Assert(list.TryGetValue("US AMS (Delete)", out securityRight));
				AssertEquals("US AMS (Delete)", securityRight.Description);
				Assert(list.TryGetValue("US AMS (Send)", out securityRight));
				AssertEquals("US AMS (Send)", securityRight.Description);
				Assert(list.TryGetValue("US AMS (View)", out securityRight));
				AssertEquals("US AMS (View)", securityRight.Description);
			});
		}

		public void TestOverridenNewDelegate()
		{
			WebSecurityRightsListForTest.RegisterThisSubTypeOverride();

			var list = WebSecurityRightsList.New();
			WebSecurityRight ignored;
			Assert(list.TryGetValue("Test right 1", out ignored));
			Assert(list.TryGetValue("Test right 2", out ignored));
			Assert("Base one should ALSO be included", list.TryGetValue("Web Invoicing and Statements", out ignored));
		}

		public void TestGlowWebSecurityShouldNotShowWhenEnableSecurityGroupsForContactsInGlow()
		{
			GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			WebSecurityRightsListForTest.ClearThisSubTypeOverride();
			var list = WebSecurityRightsList.New();
			WebSecurityRight ignored;

			AssertEquals("list should be type of WebSecurityRightsList", true, list.GetType() == typeof(WebSecurityRightsList));
			AssertEquals("should not show existing GlowWebSecurityRight when EnableSecurityGroupsForContactsInGLOW is true", false, list.TryGetValue(WebSecurityRightsList.ContainerYardShippingLinePortal.Code, out ignored));
			AssertEquals("should show existing EdiWebSecurityRight when EnableSecurityGroupsForContactsInGLOW is true", true, list.TryGetValue(WebSecurityRightsList.WebQuotes.Code, out ignored));
		}

		public void TestGlowWebSecurityShouldShowWhenDisableSecurityGroupsForContactsInGlow()
		{
			GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			WebSecurityRightsListForTest.ClearThisSubTypeOverride();
			var list = WebSecurityRightsList.New();
			WebSecurityRight ignored;

			AssertEquals("list should be type of WebSecurityRightsList", true, list.GetType() == typeof(WebSecurityRightsList));
			AssertEquals("should show existing GlowWebSecurityRight when EnableSecurityGroupsForContactsInGLOW is false", true, list.TryGetValue(WebSecurityRightsList.ContainerYardShippingLinePortal.Code, out ignored));
			AssertEquals("should show existing EdiWebSecurityRight when EnableSecurityGroupsForContactsInGLOW is false", true, list.TryGetValue(WebSecurityRightsList.WebQuotes.Code, out ignored));
		}

		public void TestGlowWebSecurityShouldNotBeAddedWhenEnableSecurityGroupsForContactsInGlow()
		{
			GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			WebSecurityRightsListForTest.RegisterThisSubTypeOverride();

			var list = WebSecurityRightsList.New();
			WebSecurityRight ignored;
			Assert(list.TryGetValue("Test right 1", out ignored));
			Assert(list.TryGetValue("Test right 2", out ignored));
			Assert("Base EdiWebSecurity should ALSO be included", list.TryGetValue("Web Invoicing and Statements", out ignored));

			AssertEquals(false, list.TryGetValue("Test right 3", out ignored));
			AssertEquals(false, list.TryGetValue("Test right 4", out ignored));
			AssertEquals("Base GlowWebSecurity should ALSO not be included", false, list.TryGetValue("Container Yard Shipping Line Portal", out ignored));
		}

		public void TestGlowWebSecurityShouldBeAddedWhenDisableSecurityGroupsForContactsInGlow()
		{
			GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			WebSecurityRightsListForTest.RegisterThisSubTypeOverride();

			var list = WebSecurityRightsList.New();
			WebSecurityRight ignored;
			Assert(list.TryGetValue("Test right 1", out ignored));
			Assert(list.TryGetValue("Test right 2", out ignored));
			Assert("Base EdiWebSecurity should ALSO be included", list.TryGetValue("Web Invoicing and Statements", out ignored));

			AssertEquals(true, list.TryGetValue("Test right 3", out ignored));
			AssertEquals(true, list.TryGetValue("Test right 4", out ignored));
			AssertEquals("Base GlowWebSecurity should ALSO be included", true, list.TryGetValue("Container Yard Shipping Line Portal", out ignored));
		}

		#region Implementation

		protected override IWebSecurityRightProvider GetNewList()
		{
			return WebSecurityRightsList.New();
		}

		class WebSecurityRightsListForTest : WebSecurityRightsList
		{
			public static void RegisterThisSubTypeOverride()
			{
				OverridableNewDelegate.Value = delegate
				{ return new WebSecurityRightsListForTest(); };
			}

			public static void ClearThisSubTypeOverride()
			{
				OverridableNewDelegate.Value = null;
			}

			public bool ShouldAddReflectedRightForTest(WebSecurityRight right)
			{
				return base.ShouldAddReflectedRight(right);
			}

			public static readonly WebSecurityRight TestRight1 = new WebSecurityRight("Test right 1", (NoResString)"", WebSecurityApplication.EdiWebTracker);
			public static readonly WebSecurityRight TestRight2 = new WebSecurityRight("Test right 2", (NoResString)"", WebSecurityApplication.EdiWebTracker);
			public static readonly WebSecurityRight TestRight3 = new WebSecurityRight("Test right 3", (NoResString)"", WebSecurityApplication.GlowWeb);
			public static readonly WebSecurityRight TestRight4 = new WebSecurityRight("Test right 4", (NoResString)"", WebSecurityApplication.GlowWeb);
		}

		#endregion
	}
}
