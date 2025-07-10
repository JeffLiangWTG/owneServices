using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRulingFindBoxCollection))]
	class CusRulingFindBoxCollectionTest : ActiveBusinessObjectCollectionTestCase<CusRulingFindBoxCollection>
	{
		public void TestSetDefaultsForNewElement()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var collection = new CusRulingFindBoxCollection(Factory, "N", "C01", org1, null);
			var refCusRuling = collection.AddNew();
			AssertEquals(org1.MainAddress.PK, refCusRuling.ZZX_OA_AppliesTo);
			collection = new CusRulingFindBoxCollection(Factory, "N", "C01", null, new[] { org2.PK, org3.PK });
			refCusRuling = collection.AddNew();
			AssertEquals(org2.MainAddress.PK, refCusRuling.ZZX_OA_AppliesTo);
		}

		public void TestDefaultFilters()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var collection = new CusRulingFindBoxCollection(Factory, "N", "C01", org, null);
			var appliesToOrgFilter = collection.FilterBusinessObjectDefaults[Constants.ZZRefCusRulingFilters.AppliesToOrg + ":Property"];
			var rullingFilter = collection.FilterBusinessObjectDefaults[Constants.ZZRefCusRulingFilters.RulingNumber + ":Property"];
			AssertNotNull(appliesToOrgFilter);
			AssertNotNull(rullingFilter);
			AssertEquals(org.PK, appliesToOrgFilter.Value);
			AssertEquals("C01", rullingFilter.Value);
			collection = new CusRulingFindBoxCollection(Factory, "C02");
			appliesToOrgFilter = collection.FilterBusinessObjectDefaults[Constants.ZZRefCusRulingFilters.AppliesToOrg + ":Property"];
			rullingFilter = collection.FilterBusinessObjectDefaults[Constants.ZZRefCusRulingFilters.RulingNumber + ":Property"];
			AssertNotNull(appliesToOrgFilter);
			AssertNotNull(rullingFilter);
			AssertEquals(ZGuid.Empty, appliesToOrgFilter.Value);
			AssertEquals("C02", rullingFilter.Value);
		}

		public void TestAddNotificationWhenAdditionalFilterNotMet()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusRuling1 = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "C001", "2", ZDate.Today, ZDate.Today.AddDays(1));
			cusRuling1.ZZX_OA_AppliesTo = org1.MainAddress.PK;
			var cusRuling2 = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "C002", "2", ZDate.Today, ZDate.Today.AddDays(1));
			cusRuling2.ZZX_OA_AppliesTo = org2.MainAddress.PK;
			var cusRuling3 = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "C003", "2", ZDate.Today, ZDate.Today.AddDays(1));
			cusRuling3.ZZX_OA_AppliesTo = org3.MainAddress.PK;
			var cusRuling4 = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "C004", "2", ZDate.Today, ZDate.Today.AddDays(1));
			Factory.Save();
			var collection = new CusRulingFindBoxCollectionForTest(Factory, string.Empty);
			AssertNotification(collection, cusRuling1, false);
			AssertNotification(collection, cusRuling2, false);
			AssertNotification(collection, cusRuling3, false);
			AssertNotification(collection, cusRuling4, false);
			collection = new CusRulingFindBoxCollectionForTest(Factory, string.Empty, string.Empty, org1, new[] { org1.PK, org2.PK });
			AssertNotification(collection, cusRuling1, false);
			AssertNotification(collection, cusRuling2, false);
			AssertNotification(collection, cusRuling3, true);
			AssertNotification(collection, cusRuling4, false);
		}

		void AssertNotification(CusRulingFindBoxCollectionForTest collection, ZZRefCusRulingCombined cusRuling, bool hasError)
		{
			var errorText = "This ruling is associated with another organization.";
			var errors = new StringCollectionX();
			collection.AddNotificationWhenAdditionalFilterNotMet(errors, cusRuling);
			AssertEquals(hasError, errors.Contains(errorText));
		}

		public override void TestAdd()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public override void TestTypedget_Item()
		{
			Assert(true);
		}

		protected override CusRulingFindBoxCollection GetCollectionToTest()
		{
			return new CusRulingFindBoxCollection(Factory, "ABC");
		}
	}
}
