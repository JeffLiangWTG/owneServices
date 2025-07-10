using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	public abstract class BaseTrackingPageTest : WebControlTest
	{
		#region TestPageNameAndPath

		public void TestPageName()
		{
			Assert(!string.IsNullOrEmpty(TestPage.PageName));
			AssertEquals(GetExpectedPageName(), TestPage.PageName);
		}

		public void TestPageRelativePath()
		{
			Assert("Please Implement GetPageRelativePath", !string.IsNullOrEmpty(TestPage.PageRelativePath));
			AssertEndsWith("PageRelativePath should give the file name", ".aspx", TestPage.PageRelativePath);
			Assert("PageRelatiavePath should indicate the base subidrectory", IsInWebRootDirectory || TestPage.PageRelativePath.LastIndexOf('/') > 1);
		}

		protected abstract string GetExpectedPageName();
		protected virtual bool IsInWebRootDirectory { get { return false; } }

		#endregion

		public void TestGetNewControlReturnsZPage()
		{
			Control ctrl = GetNewControl();
			Assert("Control should descend from ZPage", Control.GetType().IsSubclassOf(typeof(ZPage)));
		}

		public void TestShowNavigation()
		{
			AssertEquals("Should be visible for logged in SiteUser", true, TestPage.ShowNavigation);
			TestPage.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			AssertEquals("Should be invisible for CurrentCompany.OrgProxy web users", false, TestPage.ShowNavigation);
		}

		public virtual void TestShowLoginStatus()
		{
			AssertEquals("Should be visible for logged in SiteUser", true, TestPage.ShowLoginStatusForTest);
			TestPage.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			AssertEquals("Should be invisible for CurrentCompany.OrgProxy web users", false, TestPage.ShowLoginStatusForTest);
		}

		public void TestFreightUtilitiesServicePath()
		{
			TestPage.RegisterFreightUtilitiesService();
			AssertNotEquals(0, TestPage.AJAX.ScriptManager.Services.Count);
			bool freightUtilitiesServiceRegistered = false;
			foreach (ServiceReference reference in TestPage.AJAX.ScriptManager.Services)
			{
				if (reference.Path.EndsWith("WebService/FreightUtilitiesService.asmx"))
				{
					Assert("Invalid path:" + reference.Path, !reference.Path.StartsWith("//"));
					freightUtilitiesServiceRegistered = true;
					break;
				}
			}
			Assert("Freight Utilities Service is not registered", freightUtilitiesServiceRegistered);
		}

		public void TestPageRequiresLogin()
		{
			AssertEquals("LoginPage", false, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.LoginPage)));
			AssertEquals("ForgotPasswordPage", false, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.ForgotPasswordPage)));
			AssertEquals("ResetPasswordPage", false, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.ResetPasswordPage)));
			AssertEquals("SetPasswordPage", false, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.SetPasswordPage)));
			AssertEquals("ErrorPage", false, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.ErrorPage)));
			AssertEquals("TermsAndConditionsPage", false, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.TermsAndConditionsPage)));
			AssertEquals("CustomsExchangeRatesPage", false, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.CustomsExchangeRatesPage)));

			AssertEquals("ShipmentsPage", true, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.ShipmentsPage)));

			AssertEquals("FlightSchedulesPage", false, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.FlightSchedulesPage)));
			AssertEquals("SailingSchedulesPage", false, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.SailingSchedulesPage)));
			AssertEquals("RoadSchedulesPage", false, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.RoadSchedulesPage)));
			AssertEquals("RailSchedulesPage", false, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.RailSchedulesPage)));
		}

		public void TestPageRequiresNavigation()
		{
			AssertEquals("LoginPage", false, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.LoginPage)));
			AssertEquals("ForgotPasswordPage", false, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.ForgotPasswordPage)));
			AssertEquals("ErrorPage", false, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.ErrorPage)));
			AssertEquals("TermsAndConditionsPage", false, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.TermsAndConditionsPage)));
			AssertEquals("CustomsExchangeRatesPage", false, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.CustomsExchangeRatesPage)));

			AssertEquals("ShipmentsPage", true, TestPage.PageRequiresLoginForTest(new Uri("http://localhost/" + TestPage.AppInstance.ShipmentsPage)));
		}

		public void TestShowChangePasswordLinkButtonReturnsFalse()
		{
			AssertEquals("ShowChangePasswordLinkButton should return False", false, TestPage.ShowChangePasswordLinkButtonForTest);
		}

		public void TestShowLogOffLinkButtonReturnsFalse()
		{
			AssertEquals("ShowLogOffLinkButton should return False", false, TestPage.ShowLogOffLinkButtonForTest);
		}

		public void TestTranslatability()
		{
			using (var mockRes = Res.UseMockData())
			{
				const string hao = "好";
				mockRes.SetResourceGetter(key => new ResourceStringData(key, hao));
				if (TestPage.MultipleProductsColumn != null)
				{
					Assert("MultipleProductsColumn.HeaderText is not translatable", TestPage.MultipleProductsColumn.HeaderText.Contains(hao) || string.IsNullOrEmpty(TestPage.MultipleProductsColumn.HeaderText));
					AssertNotContains("MultipleProductsColumn.BindTo should not be translatable", hao, TestPage.MultipleProductsColumn.BindTo);
				}
				else
				{
					Assert(true);
				}
			}
		}

		protected ZGuid GetRedirectReferencePK()
		{
			var queryString = HttpContext.Current.Response.RedirectLocation.Split('?').Skip(1).FirstOrDefault();

			return ZGuid.TryParse(HttpUtility.ParseQueryString(queryString)["Ref"], out var referencePK) ? referencePK : ZGuid.Empty;
		}

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			Helper = new ZWebTestHelper(Factory);
			Factory.Save();
			TestPage.SiteUser.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);
		}
		ZWebTestHelper Helper;

		protected BasePage TestPage
		{
			get
			{
				if (fTestPage == null)
				{
					fTestPage = GetNewControl() as BasePage;
				}
				return fTestPage;
			}
		}
		BasePage fTestPage;

		#endregion Setup

		#region TearDown

		protected override void TearDown()
		{
			Helper.RestoreEnvironmentSettings();
			base.TearDown();
		}

		#endregion TearDown
	}
}
