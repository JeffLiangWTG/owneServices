using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Customs.Universal.RefCusRulingConfigCategories.Codes;
using static Enterprise.Customs.Universal.RefCusRulingConfigTypes.Codes;
using IBaseJobComInvoiceLine = Enterprise.Integration.Customs.IBaseJobComInvoiceLine;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRulingConfigCombinedCollection))]
	class CusRulingConfigCombinedCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(CusRulingConfigCombinedCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusRulingConfigCombinedCollection(Factory.New<ZZRefCusRulingCombined>());
		}

		public override void TestAddNew()
		{
			var cusRuling = Factory.New<ZZRefCusRulingCombined>();
			var config = cusRuling.Configurations.AddNew(DTY, Specific, 5, "CLT");
			AssertEquals("config.ZZY_Category", DTY, config.ZZY_Category);
			AssertEquals("config.ZZY_Type", Specific, config.ZZY_Type);
			AssertEquals("config.ZZY_Rate", 5.000M, config.ZZY_Rate);
			AssertEquals("config.ZZY_Value", "CLT", config.ZZY_Value);
			AssertEquals("cusRuling.Configurations.Count", 1, cusRuling.Configurations.Count);
			var config2 = cusRuling.Configurations.AddNew();
			AssertEquals("cusRuling.Configurations.Count", 2, cusRuling.Configurations.Count);
			AssertCollectionContains(config2, cusRuling.Configurations);
		}

		public void TestAllowNewCore()
		{
			var cusRuling = Factory.New<ZZRefCusRulingCombined>();
			Assert("cusRuling.AllowNewCore", cusRuling.Configurations.AllowNew);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			cusRuling = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "111111", RefCusRulingTypeList.Codes._2, ZDate.Today, ZDate.Today.AddYears(1));
			Assert("cusRuling.AllowNewCore", !cusRuling.Configurations.AllowNew);
			var invoiceLine = Factory.New<IBaseJobComInvoiceLine>();
			var collection = new CusRulingConfigCombinedCollection(invoiceLine);
			Assert("cusRuling.AllowNewCore", collection.AllowNew);
		}
	}
}
