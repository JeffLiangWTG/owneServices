using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	public abstract class BasePageWithAuthorisationTest : BaseTrackingPageTest
	{
		protected new BasePageWithAuthorisation TestPage
		{
			get { return base.TestPage as BasePageWithAuthorisation; }
		}

		public void TestModulePage()
		{
			TestPage.SetupModulePage();
			var moduleTestPage = TestPage as IModulePage;
			if (moduleTestPage != null)
			{
				Assert("Module pages do not show warnings.", !TestPage.NotificationFlags.DisplayWarnings);
			}
			Assert("ModulePage", true);
		}

		public void TestPageRestrictAuthorisedAccess()
		{
			if (SiteUserSecurityRight != null)
			{
				AssertPageRestrictsAuthorisedAccess(UseWebModule, SiteUserSecurityRight);
			}
			Assert("PageIncludesAccessControl", true);
		}

		public void TestSearchControlMaxRows()
		{
			SearchControl searchControl = null;

			foreach (Control control in TestPage.Controls)
			{
				searchControl = (control as SearchControl);

				if (searchControl != null)
				{
					break;
				}
			}

			if (searchControl == null)
			{
				Assert("The page does not contain any search controls.", true);
			}
			else
			{
				AssertEquals("The page size of the search control must be the value specified in the registry.",
					WebDataRegistry.Instance.PageSize, searchControl.PageSize);

				AssertEquals("The maximum number of rows of the search control must be the value specified in the registry.",
					WebDataRegistry.Instance.MaxFilteredRecords, searchControl.MaxRows);
			}
		}

		public void TestLicenceCheckPoints()
		{
			AssertNotNull(TestPage.LicenceCheckPoints);
			AssertArrayEqualsByElements(ExpectedLicenceCheckPoints.ToArray(), TestPage.LicenceCheckPoints.ToArray());
		}

		[HttpContextEnabledTest]
		public void TestOnUnloadRemovesAutoLoginQueryStringDataFromSession()
		{
			HttpContext.Current.Session[Global.AutoLoginQueryStringDataIndexer] = "some test data";

			TestPage.OnUnloadInternal(EventArgs.Empty);

			AssertNull(HttpContext.Current.Session[Global.AutoLoginQueryStringDataIndexer]);
		}

		[HttpContextEnabledTest]
		public void TestOnUnloadShipmentQuickViewUser()
		{
			TestPage.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			var method = typeof(Page).GetMethod("SetIntrinsics",
				BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				new Type[] { typeof(HttpContext) },
				null);
			method.Invoke(TestPage, new object[] { HttpContext.Current });

			var containerProperty = typeof(HttpSessionState).GetProperty("Container", BindingFlags.NonPublic | BindingFlags.Instance);
			var sessionStateContainer = (HttpSessionStateContainer)containerProperty.GetValue(HttpContext.Current.Session, null);

			AssertEquals("Pre-condition", DateTime.MinValue, TestPage.Response.Cookies.Get(FormsAuthentication.FormsCookieName).Expires);
			Assert("Pre-condition", TestPage.SiteUser.IsLoggedIn && TestPage.SiteUser.IsShipmentQuickViewUser);
			Assert("Pre-condition", !sessionStateContainer.IsAbandoned);

			TestPage.OnUnloadInternal(EventArgs.Empty);

			var authCookie = TestPage.Response.Cookies.Get(FormsAuthentication.FormsCookieName);
			Assert("FormsAuthentication cookie should be expired", DateTime.MinValue != authCookie.Expires && authCookie.Expires < ZDateTime.Now);
			Assert("Quick view user should be logged out", !TestPage.SiteUser.IsLoggedIn);
			Assert("Session should be abandoned", sessionStateContainer.IsAbandoned);
		}

		protected virtual IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints
		{
			get { return Array.Empty<ILicenceCheckpoint>(); }
		}

		void AssertPageRestrictsAuthorisedAccess(BooleanRegistryItem useModuleRI, WebSecurityRight siteUserSecurityRight)
		{
			var pageSecurity = GetSecurityContacts(TestPage.SiteUser.LoggedInUser, siteUserSecurityRight);
			AssertNotNull("Unable to locate SecurityRight for WebSecurityRight: " + siteUserSecurityRight.Code, pageSecurity);

			AssertEquals("UserSecurity should be granted", true, TestPage.SiteUser.LoggedInUser.SecurityRightsForBindingOnly.IsRightGranted(siteUserSecurityRight));

			pageSecurity.OZ_Granted = ZBool.False;
			AssertEquals("UserSecurity should be denied", false, TestPage.SiteUser.LoggedInUser.SecurityRightsForBindingOnly.IsRightGranted(siteUserSecurityRight));

			if (useModuleRI != null)
			{
				useModuleRI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}

			pageSecurity.OZ_Granted = ZBool.True;
			TestPage.SiteUser.LoggedInOrganisation.OH_IsWarehouseClient = true;
			TestPage.SiteUser.LoggedInOrganisation.OH_IsConsignor = true;

			TestPage.SiteUser.OnSecurityRightsChangedForTest();
			TestPage.SetupAuthorisedContentInternal();
			AssertEquals("AuthorisedContent should be visible", true, TestPage.AuthorisedContent.Visible);
			AssertEquals("UnauthorisedContent should be hidden", false, TestPage.UnauthorisedDiv.Visible);
			AssertAuthorisedContent(useModuleRI, pageSecurity);

			useModuleRI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			pageSecurity.OZ_Granted = ZBool.True;

			TestPage.SiteUser.OnSecurityRightsChangedForTest();
			TestPage.SetupAuthorisedContentInternal();
			AssertEquals("AuthorisedContent should be hidden", false, TestPage.AuthorisedContent.Visible);
			AssertEquals("UnauthorisedContent should be visible", true, TestPage.UnauthorisedDiv.Visible);
			AssertAuthorisedContent(useModuleRI, pageSecurity);

			useModuleRI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			pageSecurity.OZ_Granted = ZBool.False;
			TestPage.SiteUser.LoggedInOrganisation.OH_IsWarehouseClient = false;
			TestPage.SiteUser.LoggedInOrganisation.OH_IsConsignor = false;

			TestPage.SiteUser.OnSecurityRightsChangedForTest();
			TestPage.SetupAuthorisedContentInternal();
			AssertEquals("AuthorisedContent should be hidden", false, TestPage.AuthorisedContent.Visible);
			AssertEquals("UnauthorisedContent should be visible", true, TestPage.UnauthorisedDiv.Visible);
			AssertAuthorisedContent(useModuleRI, pageSecurity);

			useModuleRI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			pageSecurity.OZ_Granted = ZBool.False;
			TestPage.SiteUser.LoggedInOrganisation.OH_IsWarehouseClient = false;
			TestPage.SiteUser.LoggedInOrganisation.OH_IsConsignor = false;

			TestPage.SiteUser.OnSecurityRightsChangedForTest();
			TestPage.SetupAuthorisedContentInternal();
			AssertEquals("AuthorisedContent should be hidden", false, TestPage.AuthorisedContent.Visible);
			AssertEquals("UnauthorisedContent should be visible", true, TestPage.UnauthorisedDiv.Visible);
			AssertAuthorisedContent(useModuleRI, pageSecurity);
		}

		protected virtual void AssertAuthorisedContent(BooleanRegistryItem useModule, OrgSecurityContacts contactSecurity)
		{
		}

		protected OrgSecurityContacts GetSecurityContacts(OrgContact contact, WebSecurityRight securityRight)
		{
			OrgSecurityContacts result = null;
			foreach (OrgSecurityContacts security in contact.SecurityRightsForBindingOnly)
			{
				if (security.Security.OX_SecurityItemName == securityRight.Code)
				{
					result = security;
					break;
				}
			}
			AssertNotNull("Failed to find WebSecurityRight - " + securityRight.Code, result);
			return result;
		}

		protected void SetupSecurity(WebSecurityRight right, OrgHeader orgHeader, OrgContact contact, bool isGranted)
		{
			var orgRight = orgHeader.SecurityRights.AddNew();
			orgRight.OX_Granted = isGranted;
			orgRight.OX_SecurityItemName = right.Code;
			var userRight = contact.SecurityRightsForBindingOnly.AddNew();
			userRight.OZ_OX = orgRight.PK;
			userRight.OZ_Granted = isGranted;
		}

		#region Page Tab and Security

		protected abstract BooleanRegistryItem UseWebModule { get; }
		protected abstract WebSecurityRight SiteUserSecurityRight { get; }
		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			TestPage.AuthorisedContent = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
			TestPage.UnauthorisedDiv = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
			TestPage.UnauthorisedLabel = new ZTextLabel();
		}
	}
}
