using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusLiquidationLookupsTest : BusinessObjectLookupsTestCase
	{
		[TestDate(2021, 03, 17)]
		public void TestExtensionSuspensionCodeList()
		{
			var lookups = cusLiquidation.Lookups;
			var list1 = lookups.ExtensionSuspensionCodeList;
			var list2 = new CusLiquidationLookups(cusLiquidation).ExtensionSuspensionCodeList;
			AssertSame("Cached List.", list1, list2);
		}

		public void TestLookups()
		{
			AssertType<ExtensionSuspensionCodeList>("ExtensionSuspensionCodeList", cusLiquidation.Lookups.ExtensionSuspensionCodeList);
			AssertType<ChangeLiquidationReasonCodeList>("ChangeLiquidationReasonCodeList", cusLiquidation.Lookups.ChangeLiquidationReasonCodeList);
			AssertType<LiquidationTypeCodeList>("LiquidationTypeCodeList", cusLiquidation.Lookups.LiquidationTypeCodeList);
			AssertType<JobDeclarationCollection>("LiquidationTypeCodeList", cusLiquidation.Lookups.JobDeclarationList);
		}

		public void TestEntryTypeList()
		{
			var entryTypeList = cusLiquidation.Lookups.EntryTypeList;
			AssertSame(cusLiquidation.Lookups.EntryTypeList, entryTypeList);
			AssertEquals("01, 02, 03, 04, 05, 06, 07, 08, 09, 11, 12, 21, 22, 23, 24, 25, 26, 31, 32, 33, 34, 38, 47, 51, 52, 64, 65, 66, 86", entryTypeList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusLiquidation = Factory.New<CusLiquidation>();
		}

		CusLiquidation cusLiquidation;
	}
}
