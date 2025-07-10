using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(GlbCompanyCredentialCollection))]
	sealed class GlbCompanyCredentialCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionNotContainsStaff()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "A12";

			var extPassword1 = Factory.New<GlbCompanyCredential>();
			extPassword1.GP_PasswordType = PasswordTypesList.Codes.EBD;
			extPassword1.GP_GC = company.PK;
			extPassword1.GP_MailBoxID = "123";
			var extPassword2 = Factory.New<GlbCompanyCredential>();
			extPassword2.GP_PasswordType = PasswordTypesList.Codes.EBD;
			extPassword2.GP_GC = company.PK;
			extPassword2.GP_GS = Factory.NewWithValidTestData<GlbStaff>().PK;
			extPassword2.GP_MailBoxID = "456";

			var collection = new GlbCompanyCredentialCollection(company);
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals("123", collection[0].GP_MailBoxID);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new GlbCompanyCredentialCollection(Factory.NewWithValidTestData<GlbCompany>());
	}
}
