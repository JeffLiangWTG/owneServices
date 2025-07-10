using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USLicenseAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLPCOTransactionTypeList()
		{
			var lpcoTransactionTypeList = lookups.LPCOTransactionTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "2, 3, 1", lpcoTransactionTypeList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<LPCOTransactionTypeList>(), lpcoTransactionTypeList);
			});
		}

		public void TestDateQualifierList()
		{
			var dateQualifierList = lookups.DateQualifierList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "4, 3, 2, 1", dateQualifierList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<LPCODateQualifierList>(), dateQualifierList);
			});
		}

		public void TestLPCOTypeList()
		{
			var lpcoTypeList = lookups.LPCOTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "A01, A10, A11, A12, A13, A14, A15, A16, A17, A18, A19, A20, A21, A22, A23, FWD, FWF", lpcoTypeList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<LaceyActLPCOTypeList>(), lpcoTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var license = Factory.New<License>();
			var addInfo = new LicenseAddInfo(license.B7_AddInfoDataInfo);
			lookups = new USLicenseAddInfoLookups(addInfo);
		}
		USLicenseAddInfoLookups lookups;
	}
}
