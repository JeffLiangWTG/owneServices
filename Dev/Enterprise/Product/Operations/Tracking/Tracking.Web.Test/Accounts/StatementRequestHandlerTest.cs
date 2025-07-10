using System.IO;
using System.Web;
using CargoWise.Application;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.Tracking.Web
{
	[TestedType(typeof(StatementRequestHandler))]
	sealed class StatementRequestHandlerTest : DataRequestHandlerTestCase<StatementRequestHelper>
	{
		public void TestLanguageNotChangeAfterRequest()
		{
			AssertNotEquals("Querystring should not be empty", 0, RequestHandler.QueryString.Count);
			using (var memoryStream = new MemoryStream())
			{
				var filter = new TestResponseFilter(HttpContext.Current.Response.Filter, memoryStream);
				HttpContext.Current.Response.Filter = filter;
				HttpContext.Current.Session["Language"] = "ZH-CN";
				HttpContext.Current.Session["SiteUser"] = new OrgContactWebUser();
				AssertNotEquals("Default language should not be ZH-CN.", "ZH-CN", ObjectFactory.Get<IResourceStrings>().CurrentLanguage);
				RequestHandler.ProcessRequest(HttpContext.Current);
				HttpContext.Current.Response.Flush();
				HttpContext.Current.Response.End();

				AssertEquals("Current Language should be the same as the session language.", "ZH-CN", ObjectFactory.Get<IResourceStrings>().CurrentLanguage);
			}
		}

		protected override DataRequestHandler<StatementRequestHelper> GetNewRequestHandler()
		{
			var loggedInOrg = ((OrgContactWebUser)ApplicationInstance.SiteUser).LoggedInOrganisation;
			var testCompany = Factory.New<GlbCompany>();
			testCompany.GC_OH_OrgProxy = loggedInOrg.PK;

			var testBranch = Factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_Code = "_GB";
			testBranch.GB_GC = testCompany.PK;

			testCompany.Branches.Add(testBranch);

			var testCompanyData = Factory.New<OrgCompanyData>();
			testCompanyData.OB_OH = loggedInOrg.PK;
			testCompanyData.OB_GC = testCompany.PK;
			testCompanyData.OB_GB_ControllingBranch = testBranch.PK;
			testCompanyData.OB_IsDebtor = true;

			var invoice1 = Factory.New<ARInvoice>();
			invoice1.AH_OH = loggedInOrg.PK;
			invoice1.AH_GB = testBranch.PK;
			TestObjectCreator.CreateInvoiceLine(invoice1, invoice1.TransactionCurrency, invoice1.AH_ExchangeRate, 100m, 0m, 0m);

			var invoice2 = Factory.New<ARInvoice>();
			invoice2.AH_OH = loggedInOrg.PK;
			invoice2.AH_GB = testBranch.PK;
			TestObjectCreator.CreateInvoiceLine(invoice2, invoice2.TransactionCurrency, invoice2.AH_ExchangeRate, 200m, 0m, 0m);

			var creditNote1 = Factory.New<ARCreditNote>();
			creditNote1.AH_OH = loggedInOrg.PK;
			creditNote1.AH_GB = testBranch.PK;
			TestObjectCreator.CreateInvoiceLine(creditNote1, creditNote1.TransactionCurrency, creditNote1.AH_ExchangeRate, -300m, 0m, 0m);

			var contact1 = loggedInOrg.Contacts.AddNew();
			contact1.OC_Email = "a@yahoo.com";
			contact1.SetHashedPassword("123");
			contact1.OC_WebAccessEnabled = true;

			Factory.Save();
			contact1.Factory.Save();

			var helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(loggedInOrg.OH_Code, contact1.OC_Email, contact1.PasswordForTesting);

			var testHandler = new StatementRequestHandler();
			testHandler.QueryString.Add(DataRequestHelper.DataKey, testCompany.PK.ToString());
			return testHandler;
		}

		protected override void TestCacheCore()
		{
			WebUser user = ApplicationInstance.SiteUser;

			byte[] retrievedData = RequestHandlerRetrieveFromCache(RequestHandler.PKs);
			AssertNull("The cache must be empty initially.", retrievedData);

			ZBlob testData = RequestHandler.GetBinaryData();
			AssertNotEquals("The test data must not be empty", ZBlob.Empty, testData);

			RequestHandlerStoreInCache(testData, RequestHandler.PKs);

			ApplicationInstance.SetSiteUser(new OrgContactWebUser());
			retrievedData = RequestHandlerRetrieveFromCache(RequestHandler.PKs);
			AssertNull("Should be empty as indexer was incorrect while retrieving", retrievedData);

			ApplicationInstance.SetSiteUser(user);
			retrievedData = RequestHandlerRetrieveFromCache(RequestHandler.PKs);
			AssertEquals("The object retrieved from the cache must equal the object added.", testData, retrievedData);

			ApplicationInstance.SetSiteUser(new OrgContactWebUser());
			RequestHandler.RemoveFromCache(RequestHandler.PKs);
			ApplicationInstance.SetSiteUser(user);
			retrievedData = RequestHandlerRetrieveFromCache(RequestHandler.PKs);
			AssertEquals("Should not be empty as indexer was incorrect while removing", testData, retrievedData);

			RequestHandler.RemoveFromCache(RequestHandler.PKs);
			retrievedData = RequestHandlerRetrieveFromCache(RequestHandler.PKs);
			AssertNull("The object must be removed from the cache successfully.", retrievedData);

			ApplicationInstance.SetSiteUser(new OrgContactWebUser());
			RequestHandlerStoreInCache(testData, RequestHandler.PKs);
			retrievedData = RequestHandlerRetrieveFromCache(RequestHandler.PKs);
			AssertEquals("The object retrieved from the cache must equal the object added.", testData, retrievedData);

			ApplicationInstance.SetSiteUser(user);
			retrievedData = RequestHandlerRetrieveFromCache(RequestHandler.PKs);
			AssertNull("Should be empty as indexer was incorrect while retrieving", retrievedData);
		}

		#region Implementation

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion
	}
}
