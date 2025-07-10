using System.Web;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class TestNullDataSource : TestCaseWithFactory
	{
		public void TestRedirectWhenAutoLoginQueryStringDataInSession()
		{
			AssertEquals("pre-condition", true, page.SiteUser.IsLoggedIn);
			HttpContext.Current.Session[Global.AutoLoginQueryStringDataIndexer] = "some-test/data";

			page.OnLoadForTest();

			Assert(!page.SiteUser.IsLoggedIn);
			AssertEquals("/AutoLoginRequestHandler.axd?AutoLogin=some-test%2fdata", HttpContext.Current.Response.RedirectLocation);
		}

		public void TestNoRedirectWhenAutoLoginQueryStringDataNotInSession()
		{
			AssertEquals("pre-condition", true, page.SiteUser.IsLoggedIn);
			page.OnLoadForTest();

			Assert(page.SiteUser.IsLoggedIn);
			AssertNull(HttpContext.Current.Response.RedirectLocation);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_WebAccessEnabled = ZBool.True;
			contact.SetHashedPassword("1234");
			Factory.Save();

			page = new TestPageWithNullDataSourceForTest();
			page.SiteUser.LoginForTest(contact.OrgCode, contact.OC_Email, "1234");
		}

		TestPageWithNullDataSourceForTest page;
	}
}
