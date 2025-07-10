using System;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.ImporterSecurityFiling;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class TrackingLoginPageTest : BaseTrackingPageTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.Login;
		}

		public void TestViewStateIsDisabled()
		{
			Login page = new Login();
			Assert("ViewState was disabled to avoid exception after logoff-login", !page.EnableViewState);
		}

		public void TestIsCompanyCodeRequired()
		{
			LoginManager loginMan = typeof(Login).InvokeMember("GetNewDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, TestPage, Array.Empty<object>()) as LoginManager;
			AssertNotNull("LoginManager", loginMan);
			AssertEquals("Should be set from registry", WebDataRegistry.Instance.WebTrackerLoginRequiresCompanyCode.Value, loginMan.IsCompanyCodeRequired);
		}

		public void TestPresetUserNameAndCompany()
		{
			TestPage.SiteUser.Logout();
			AssertPresetUserNameAndCompany(false);
			AssertPresetUserNameAndCompany(true);
		}

		void AssertPresetUserNameAndCompany(bool isCompanyRequiredRegistryValue)
		{
			using (WebDataRegistry.Instance.WebTrackerLoginRequiresCompanyCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isCompanyRequiredRegistryValue))
			{
				HttpContext.Current.Session.Abandon();
				HttpContext.Current.Request.QueryString.Clear();
				TestPage.OnLoadForTest();
				var loginMan = TestPage.GetNewDataSourceForTest();
				AssertEquals("Should not have preset user name", string.Empty, loginMan.UserName);
				AssertEquals("Should not have preset company", string.Empty, loginMan.CompanyCode);
				AssertEquals(isCompanyRequiredRegistryValue, loginMan.IsCompanyCodeRequired);

				var sessionLoginMan = new TrackingLoginManager();
				sessionLoginMan.UserName = "test@test.com";
				sessionLoginMan.CompanyCode = "TSTORG";
				var sessionReference = Guid.NewGuid().ToString();
				HttpContext.Current.Session.Add(sessionReference, sessionLoginMan);
				HttpContext.Current.Request.QueryString.Add(TrackingConstants.QueryStringKeys.RefKey, sessionReference);

				loginMan = TestPage.GetNewDataSourceForTest();
				AssertEquals("Should have preset user name", "test@test.com", loginMan.UserName);
				AssertEquals("Should have preset company", "TSTORG", loginMan.CompanyCode);
				Assert(loginMan.IsCompanyCodeRequired);

				sessionReference = Guid.NewGuid().ToString();
				HttpContext.Current.Request.QueryString[TrackingConstants.QueryStringKeys.RefKey] = sessionReference;
				TestPage.CompanyCodeTextBoxForTest.Text = "TSTORG";
				loginMan = TestPage.GetNewDataSourceForTest();
				Assert(loginMan.IsCompanyCodeRequired);

				TestPage.CompanyCodeTextBoxForTest.Text = string.Empty;
				loginMan = TestPage.GetNewDataSourceForTest();
				AssertEquals(isCompanyRequiredRegistryValue, loginMan.IsCompanyCodeRequired);
			}
		}

		public void TestDefaultTextBox()
		{
			TestPage.SiteUser.Logout();
			TestPage.OnLoadForTest();
			TestPage.CompanyCodeTextBoxForTest.Text = string.Empty;
			TestPage.LoginNameTextBoxForTest.Text = string.Empty;

			TestPage.LoginManForTest.IsCompanyCodeRequired = true;
			TestPage.OnLoadForTest(false);
			AssertEquals("CompanyCodeTextBox", TestPage.DefaultTextBox);

			TestPage.CompanyCodeTextBoxForTest.Text = "TSTORG";
			TestPage.OnLoadForTest(false);
			AssertEquals("LoginNameTextBox", TestPage.DefaultTextBox);

			TestPage.LoginNameTextBoxForTest.Text = "test@test.com";
			TestPage.OnLoadForTest(false);
			AssertEquals("PasswordTextBox", TestPage.DefaultTextBox);

			TestPage.CompanyCodeTextBoxForTest.Text = string.Empty;
			TestPage.LoginNameTextBoxForTest.Text = string.Empty;
			TestPage.LoginManForTest.IsCompanyCodeRequired = false;
			TestPage.OnLoadForTest(false);
			AssertEquals("LoginNameTextBox", TestPage.DefaultTextBox);

			TestPage.LoginNameTextBoxForTest.Text = "test@test.com";
			TestPage.OnLoadForTest(false);
			AssertEquals("PasswordTextBox", TestPage.DefaultTextBox);
		}

		public void TestIsDataSourceInSession()
		{
			var header = Factory.New<TrackingCusISFHeader>();
			var addressControl = new ZTextBox { BindTo = "CompanyCode" };
			TestPage.Controls.Add(addressControl);

			TestPage.Session[TestPage.DataSourceIndexer.ToString()] = header;

			AssertNoExceptionThrown(() => TestPage.OnLoadForTest()); //if IsDataSourceInSession = true, there would be an exception: Unabled to find 'CompanyCode' property on 'Enterprise.Tracking.Business.ImporterSecurityFiling.TrackingCusISFHeader' object.
		}

		public void TestLoginInstruction()
		{
			TestPage.SiteUser.Logout();
			TestPage.OnLoadForTest();
			Assert("Login Instruction should not be visible", !TestPage.LoginInstructionContentForTest.Visible);
			AssertEquals("Login Instruction should be empty", string.Empty, TestPage.LoginInstructionForTest);

			var registryValue = "Some Instruction";
			WebDataRegistry.Instance.WebTrackerLoginPageInstruction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			TestPage.OnLoadForTest();
			Assert("Login Instruction should be visible", TestPage.LoginInstructionContentForTest.Visible);
			AssertEquals("Login Instruction should be as expected", "Some Instruction", TestPage.LoginInstructionForTest);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Spanish))
			using (var mockSpn = Res.UseMockData())
			{
				string key = ((ResourceString)WebDataRegistry.Instance.WebTrackerLoginPageInstruction.Value).ResourceKey;
				mockSpn.Put(key, new ResourceStringData(key, "Algunos de instrucciones"));

				TestPage.OnLoadForTest();
				Assert("Login Instruction should be visible", TestPage.LoginInstructionContentForTest.Visible);
				AssertEquals("Login Instruction should be as expected", "Algunos de instrucciones", TestPage.LoginInstructionForTest);
			}
		}

		public void TestSigninBtn_ClickSpecialLogin()
		{
			TestPage.SiteUser.Logout();
			WebDataRegistry.Instance.WebServiceUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "hello");
			WebDataRegistry.Instance.WebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "there");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_Email = "fly@guy.com";
			contact.SetHashedPassword("1234");
			Factory.Save();

			TestPage.OnLoadForTest();
			TestPage.LoginManForTest.CompanyCode = contact.OrgCode;
			TestPage.LoginManForTest.UserName = WebDataRegistry.Instance.WebServiceUsername.Value;
			TestPage.LoginManForTest.Password = WebDataRegistry.Instance.WebServicePassword.Value;
			TestPage.SigninBtn_ClickForTest();
			AssertEquals(true, TestPage.SiteUser.IsLoggedIn);
			AssertContains("Should be redirected to default url", "/Default.aspx", TestPage.Response.RedirectLocation);
		}

		public void TestSigninBtn_ClickNormalLoginShouldRedirectThroughLoginRouter()
		{
			TestPage.SiteUser.Logout();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_Email = "fly@guy.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();

			TestPage.OnLoadForTest();
			TestPage.LoginManForTest.CompanyCode = contact.OrgCode;
			TestPage.LoginManForTest.UserName = contact.OC_Email;
			TestPage.LoginManForTest.Password = "1234";
			TestPage.SigninBtn_ClickForTest();
			AssertEquals(false, TestPage.SiteUser.IsLoggedIn);
			AssertStartsWith("Should be redirected via LoginRouter", "/webapp/Login/LoginComplete.aspx", TestPage.Response.RedirectLocation);
		}

		public void TestSigninBtn_Click()
		{
			TestPage.SiteUser.Logout();
			WebDataRegistry.Instance.WebTrackerLoginRequiresCompanyCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestPage.OnLoadForTest();
			TestPage.SigninBtn_ClickForTest();
			AssertEquals("Message should be shown when Company code or Username or Password is empty", "Company code or Username or Password is empty!", TestPage.LoginManForTest.LoginErrorMsg);
			TestPage.LoginManForTest.CompanyCode = "wisglosyd";
			TestPage.LoginManForTest.UserName = "George.Bush@whitehouse.usa";
			TestPage.LoginManForTest.Password = "wisglosyd";
			TestPage.SigninBtn_ClickForTest();
			AssertEquals("Login should fail", "Login Failed!", TestPage.LoginManForTest.LoginErrorMsg);
			AssertEquals("ShipmentQuickView() should not fire", ZString.Empty, TestPage.LoginManForTest.QuickViewErrorMsg);
			TestPage.LoginManForTest.QuickViewNumber = "SomeTestShipmentNumb";
			TestPage.SigninBtn_ClickForTest();
			AssertEquals("ShipmentQuickView() should fire", "Shipment not Found!", TestPage.LoginManForTest.QuickViewErrorMsg);
		}

		public void TestVisibilityOfViewers()
		{
			TestPage.SiteUser.Logout();
			WebDataRegistry.Instance.WebTrackerContainerQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.WebTrackerShipmentQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestPage.OnLoadForTest();
			Assert(!TestPage.ViewShipmentDetailsForTest.Visible);
			Assert(!TestPage.ViewContainerDetailsForTest.Visible);

			WebDataRegistry.Instance.WebTrackerContainerQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WebDataRegistry.Instance.WebTrackerShipmentQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestPage.OnLoadForTest();
			Assert(TestPage.ViewShipmentDetailsForTest.Visible);
			Assert(TestPage.ViewContainerDetailsForTest.Visible);

			WebDataRegistry.Instance.WebTrackerContainerQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.WebTrackerShipmentQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestPage.OnLoadForTest();
			Assert(TestPage.ViewShipmentDetailsForTest.Visible);
			Assert(!TestPage.ViewContainerDetailsForTest.Visible);

			WebDataRegistry.Instance.WebTrackerContainerQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WebDataRegistry.Instance.WebTrackerShipmentQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestPage.OnLoadForTest();
			Assert(TestPage.QuickViewDetailsForTest.Visible);
			Assert(TestPage.ViewContainerDetailsForTest.Visible);
			Assert(!TestPage.ViewShipmentDetailsForTest.Visible);
		}

		public void TestFindBtn_Click()
		{
			TestPage.SiteUser.Logout();
			WebDataRegistry.Instance.WebTrackerContainerQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WebDataRegistry.Instance.WebTrackerShipmentQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestPage.OnLoadForTest();
			TestPage.FindBtn_ClickForTest();
			AssertEquals("Please enter either Shipment Number or Container Number.", ZString.Empty, TestPage.LoginManForTest.LoginErrorMsg);

			TrackingShipment shipment = Factory.NewWithValidTestData<TrackingShipment>();
			shipment.JS_UniqueConsignRef = "S00001000";
			Factory.Save();

			TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();
			container.JC_ContainerNum = "TGHU216203";
			Factory.Save();

			TestPage.LoginManForTest.QuickViewNumber = "S00001000";
			TestPage.LoginManForTest.ContainerQuickViewNumber = "TGHU216203";

			TestPage.FindBtn_ClickForTest();
			AssertEquals("Container TGHU216203 is not registered against Shipment S00001000. Please try another search or try to search only by Shipment or only by Container number.", ZString.Empty, TestPage.LoginManForTest.LoginErrorMsg);

			TestPage.OnLoadForTest();
			TestPage.LoginManForTest.QuickViewNumber = "S00001001";
			TestPage.LoginManForTest.ContainerQuickViewNumber = "1234";
			TestPage.FindBtn_ClickForTest();
			AssertEquals("Shipment or container not Found!", ZString.Empty, TestPage.LoginManForTest.LoginErrorMsg);

			WebDataRegistry.Instance.WebTrackerContainerQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.WebTrackerShipmentQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestPage.OnLoadForTest();
			TestPage.LoginManForTest.QuickViewNumber = "S00001003";
			TestPage.FindBtn_ClickForTest();
			AssertEquals("Shipment not Found!", ZString.Empty, TestPage.LoginManForTest.LoginErrorMsg);

			WebDataRegistry.Instance.WebTrackerContainerQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WebDataRegistry.Instance.WebTrackerShipmentQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestPage.OnLoadForTest();
			TestPage.LoginManForTest.ContainerQuickViewNumber = "123456";
			TestPage.FindBtn_ClickForTest();
			AssertEquals("Container not Found!", ZString.Empty, TestPage.LoginManForTest.LoginErrorMsg);
		}

		public void TestOnLoad_RedirectIfSignedIn()
		{
			Assert("Precondition", TestPage.SiteUser.IsLoggedIn);
			TestPage.OnLoadForTest();

			AssertContains("Should be redirected to default url", "/Default.aspx", TestPage.Response.RedirectLocation);
		}

		public void TestOnLoad_LogOffRequest()
		{
			TestPage.RequestQueryStringForTest.Add("ClearSaved", "1");
			TestPage.AppInstance.ApplicationCookie.WriteCookie("meh");

			TestPage.OnLoadForTest();

			Assert(!TestPage.SiteUser.IsLoggedIn);
			Assert(!TestPage.AppInstance.ApplicationCookie.CookieExist());
		}

		public void TestOnLoad_LogOffShipmentQuickViewUser()
		{
			TestPage.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			Assert("Precondition", TestPage.SiteUser.IsShipmentQuickViewUser);

			TestPage.AppInstance.ApplicationCookie.WriteCookie("meh");

			TestPage.OnLoadForTest();

			Assert(!TestPage.SiteUser.IsLoggedIn);
			Assert(!TestPage.AppInstance.ApplicationCookie.CookieExist());

			TestPage.IsPostBack = true;
			TestPage.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			TestPage.OnLoadForTest();

			Assert("Should log off QuickViewUser during postback", !TestPage.SiteUser.IsLoggedIn);
		}

		public void TestOnLoad_PostbackLogOffRequest()
		{
			TestPage.IsPostBack = true;
			Assert("Precondition", TestPage.SiteUser.IsLoggedIn && !TestPage.SiteUser.IsShipmentQuickViewUser);
			TestPage.RequestQueryStringForTest.Add("ClearSaved", "1");

			TestPage.OnLoadForTest();

			Assert("Should not log off full user during postback", TestPage.SiteUser.IsLoggedIn);
		}

		public void TestContainerQuickViewNumberHasCorrectMaxLength()
		{
			using (WebDataRegistry.Instance.WebTrackerContainerQuickView.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestPage.OnLoadForTest();
				AssertEquals("Max length matches property info", TestPage.LoginManForTest.ContainerQuickViewNumberInfo.MaxLength, TestPage.ContainerNumberTextBoxForTest.MaxLength);
			}
		}

		public void TestNoCache()
		{
			Assert(!TestPage.CacheableForTest);
		}

		#region Implementation

		new LoginForTest TestPage
		{
			get { return base.TestPage as LoginForTest; }
		}

		protected override Control GetNewControl()
		{
			var testPage = new LoginForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics",
				BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				new Type[] { typeof(HttpContext) },
				null);
			method.Invoke(testPage, new object[] { HttpContext.Current });
			return testPage;
		}

		#endregion
	}
}
