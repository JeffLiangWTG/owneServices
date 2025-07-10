using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgWebURLDependentCollection))]
	sealed class OrgWebURLDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			return new OrgWebURLDependentCollection(org);
		}

		public void TestMainURL()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AssertNotNull(org.OrgWebURLs.MainURL);
			Assert(org.OrgWebURLs.MainURL.MainDefaultAdded);
			AssertEquals("", org.OrgWebURLs.MainURL.PU_URL);
		}

		public void TestSetFirstUrlAsMain()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Assert(org.OrgWebURLs.MainURL.PU_IsPrimary);

			OrgWebURL url2 = org.OrgWebURLs.AddNew();
			Assert(!url2.PU_IsPrimary);
		}

		public void TestMainURLDescriptionDoesNotCauseValidationErrorInChinese()
		{
			using (var mockRes = Res.UseMockData())
			{
				mockRes.SetResourceGetter(new ResourceStringGetter(delegate(string key)
				{ return new ResourceStringData(key, "字"); }));
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
				AssertNotNull(org.OrgWebURLs.MainURL);
				AssertNoErrors(org.OrgWebURLs.MainURL);
			}
		}

		#region TestAddNew

		public void TestAddNewWithOrgWebUrlType()
		{
			OrgWebURLDependentCollection collection = (OrgWebURLDependentCollection)GetCollectionToTest();
			OrgWebURL orgUrl = collection.AddNew(OrgWebUrlList.Codes.CartageTracking);
			AssertEquals(orgUrl.PU_Type, OrgWebUrlList.Codes.CartageTracking);
			AssertCollectionContains(orgUrl, collection);
		}

		#endregion

		#region FindByUrlType

		public void TestFindByUrlType()
		{
			OrgWebURLDependentCollection collection = (OrgWebURLDependentCollection)GetCollectionToTest();
			OrgWebURL mainUrl = collection.AddNew();
			OrgWebURL cartageUrl = collection.AddNew();
			mainUrl.PU_Type = OrgWebUrlList.Codes.MainWebsite;
			cartageUrl.PU_Type = OrgWebUrlList.Codes.CartageTracking;

			OrgWebURL[] mainUrlsFound = collection.FindByUrlType(OrgWebUrlList.Codes.MainWebsite);
			OrgWebURL[] cartageUrlsFound = collection.FindByUrlType(OrgWebUrlList.Codes.CartageTracking);

			AssertEquals(1, mainUrlsFound.Length);
			AssertEquals(1, cartageUrlsFound.Length);

			AssertEquals(OrgWebUrlList.Codes.MainWebsite, mainUrlsFound[0].PU_Type);
			AssertEquals(OrgWebUrlList.Codes.CartageTracking, cartageUrlsFound[0].PU_Type);
		}

		#endregion
	}
}
