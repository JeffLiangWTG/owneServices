using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeCodeCollectionForRegistry))]
	sealed class AccChargeCodeCollectionForRegistryTest : AccChargeCodeCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccChargeCodeCollectionForRegistry(Factory);
		}

		public void TestRelationshipFilter()
		{
			AccChargeCode testChargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			testChargeCode1.AC_Code = "TST";
			testChargeCode1.AC_GC = GlbCompany.CurrentCompany.PK;

			GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();
			AccChargeCode testChargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			testChargeCode2.AC_Code = "TST";
			testChargeCode2.AC_GC = newCompany.PK;

			Factory.Save();

			AccChargeCodeCollectionForRegistry testCollection = new AccChargeCodeCollectionForRegistry(Factory);
			testCollection.CompanyPK = newCompany.PK;
			testCollection.Load();

			int count = 0;
			foreach (var code in testCollection.Where(x => x.AC_Code == "TST"))
			{
				count++;
			}
			AssertEquals("Collection should contain only 1 record when company is specified", 1, count);
			Assert(testCollection.Contains(testChargeCode2));

			AccChargeCodeCollectionForRegistry testCollection2 = new AccChargeCodeCollectionForRegistry(Factory);
			testCollection2.CompanyPK = ZGuid.Empty;
			testCollection2.Load();
			count = 0;
			foreach (var code in testCollection2.Where(x => x.AC_Code == "TST"))
			{
				count++;
			}
			AssertEquals("Collection should contain 2 record when no company specified", 2, count);
			Assert(testCollection2.Contains(testChargeCode1));
			Assert(testCollection2.Contains(testChargeCode2));
		}
	}
}
