using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingLoginManager))]
	[HttpContextEnabledTest]
	sealed class LoginManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestQuickViewNumber()
		{
			TrackingLoginManager testLoginManager = (TrackingLoginManager)GetNewBusinessObject();
			testLoginManager.QuickViewNumber = ZString.Empty;
			AssertEquals("User name should have no errors", 0, testLoginManager.QuickViewNumberInfo.GetErrors().Count());

			testLoginManager.QuickViewNumber = "Abc";
			AssertEquals("User name should NOT have errors", 0, testLoginManager.QuickViewNumberInfo.GetErrors().Count());

			testLoginManager.QuickViewNumber = "abcdefghijklmnopqrstuvwxyz12345678901234";
			AssertEquals("User name should NOT have errors", 0, testLoginManager.QuickViewNumberInfo.GetErrors().Count());
		}

		public void TestContainerQuickViewNumber()
		{
			TrackingLoginManager testLoginManager = (TrackingLoginManager)GetNewBusinessObject();
			testLoginManager.ContainerQuickViewNumber = ZString.Empty;
			AssertEquals("User name should have no errors", 0, testLoginManager.ContainerQuickViewNumberInfo.GetErrors().Count());

			testLoginManager.ContainerQuickViewNumber = "Abc";
			AssertEquals("User name should NOT have errors", 0, testLoginManager.ContainerQuickViewNumberInfo.GetErrors().Count());

			AssertEquals(testLoginManager.ContainerQuickViewNumberInfo.MaxLength, JobContainerSchema.JC_ContainerNum.MaxLength);

			var expectedContainerNum = new string('A', JobContainerSchema.JC_ContainerNum.MaxLength);
			testLoginManager.ContainerQuickViewNumber = expectedContainerNum + "A";
			AssertEquals(expectedContainerNum, testLoginManager.ContainerQuickViewNumber);
		}

		public void TestCustomLoginPageQueryString()
		{
			TrackingLoginManager testLoginManager = (TrackingLoginManager)GetNewBusinessObject();
			AssertEquals("Precondition: CustomLoginPageQueryString should be Empty", 0, testLoginManager.CustomLoginPageQueryString.Count);

			testLoginManager.CompanyCode = "whitehouse";
			AssertEquals("CompanyCode", "whitehouse", testLoginManager.CustomLoginPageQueryString["CompanyCode"]);

			testLoginManager.UserName = "GeorgeBush@whitehouse.gov.us";
			AssertEquals("UserEmail", "GeorgeBush%40whitehouse.gov.us", testLoginManager.CustomLoginPageQueryString["UserEmail"]);

			AssertNull("Should be no password in Query String", testLoginManager.CustomLoginPageQueryString["Password"]);
			AssertEquals("Should be two elements", 2, testLoginManager.CustomLoginPageQueryString.Count);
			testLoginManager.Password = "who?";
			AssertNull("Password should not be passed via Url", testLoginManager.CustomLoginPageQueryString["Password"]);
			AssertEquals("Should be no new elements", 2, testLoginManager.CustomLoginPageQueryString.Count);

			testLoginManager.LoginErrorMsg = "unknown user";
			AssertEquals("LoginErrorMsg", "unknown%20user", testLoginManager.CustomLoginPageQueryString["LoginErrorMsg"]);

			testLoginManager.RememberMe = true;
			AssertEquals("RememberMe", "on", testLoginManager.CustomLoginPageQueryString["RememberMe"]);

			testLoginManager.QuickViewNumber = "666";
			AssertEquals("QuickViewNumber", "666", testLoginManager.CustomLoginPageQueryString["QuickViewNumber"]);

			testLoginManager.ClearSaved = true;
			AssertEquals("ClearSaved", "Y", testLoginManager.CustomLoginPageQueryString["ClearSaved"]);

			testLoginManager.QuickViewErrorMsg = "Bad Number";
			AssertEquals("QuickViewErrorMsg", "Bad%20Number", testLoginManager.CustomLoginPageQueryString["QuickViewErrorMsg"]);

			AssertEquals("Should be seven elements", 7, testLoginManager.CustomLoginPageQueryString.Count);

			testLoginManager.RememberMe = false;
			testLoginManager.ClearSaved = false;
			AssertEquals("Should be five elements left", 5, testLoginManager.CustomLoginPageQueryString.Count);
		}

		public void TestCustomShipmentQuickViewPageQueryString()
		{
			TrackingLoginManager testLoginManager = (TrackingLoginManager)GetNewBusinessObject();
			AssertEquals("Precondition: CustomQuickViewPageQueryString should be Empty", 0, testLoginManager.CustomQuickViewPageQueryString.Count);

			testLoginManager.CompanyCode = "whitehouse";
			AssertEquals("CustomQuickViewPageQueryString should be Empty", 0, testLoginManager.CustomQuickViewPageQueryString.Count);

			testLoginManager.UserName = "GeorgeBush@whitehouse.gov.us";
			AssertEquals("CustomQuickViewPageQueryString should be Empty", 0, testLoginManager.CustomQuickViewPageQueryString.Count);

			AssertNull("Should be no password in Query String", testLoginManager.CustomLoginPageQueryString["Password"]);
			AssertEquals("CustomQuickViewPageQueryString should be Empty", 0, testLoginManager.CustomQuickViewPageQueryString.Count);

			testLoginManager.Password = "who?";
			AssertEquals("CustomQuickViewPageQueryString should be Empty", 0, testLoginManager.CustomQuickViewPageQueryString.Count);

			testLoginManager.LoginErrorMsg = "unknown%20user";
			AssertEquals("CustomQuickViewPageQueryString should be Empty", 0, testLoginManager.CustomQuickViewPageQueryString.Count);

			testLoginManager.RememberMe = true;
			AssertEquals("CustomQuickViewPageQueryString should be Empty", 0, testLoginManager.CustomQuickViewPageQueryString.Count);

			testLoginManager.QuickViewNumber = "666";
			AssertEquals("QuickViewNumber", "666", testLoginManager.CustomQuickViewPageQueryString["QuickViewNumber"]);

			testLoginManager.QuickViewErrorMsg = "Bad Number";
			AssertEquals("QuickViewErrorMsg", "Bad%20Number", testLoginManager.CustomQuickViewPageQueryString["QuickViewErrorMsg"]);

			AssertEquals("Should be two elements", 2, testLoginManager.CustomQuickViewPageQueryString.Count);
		}
	}
}
