using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeCodesForConsolidationCollection))]
	sealed class AccChargeCodesForConsolidationCollectionTest : ActiveBusinessObjectCollectionTestCase<AccChargeCodesForConsolidationCollection>
	{
		protected override AccChargeCodesForConsolidationCollection GetCollectionToTest()
		{
			return new AccChargeCodesForConsolidationCollection(Factory);
		}

		public void TestLoadsRelevantChargeCodes()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			const string aChargeCodeThatExistsInDemoCompany = "FRT";
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode,
				true, true, "NM1", aChargeCodeThatExistsInDemoCompany);

			var collection = GetCollectionToTest();

			Assert("Contains Linked Charge Code", collection.Contains(normalChargeCodeLinked));
			Assert("Contains Unlinked Charge Code", collection.Contains(normalChargeCode));
			AssertEquals("Does Not Contain Global Charge Code", false, collection.Contains(globalChargeCode));

			bool foundChargeCodeInDemo = false;
			foreach (AccChargeCode chargecode in collection)
			{
				foundChargeCodeInDemo = foundChargeCodeInDemo || GlbCompany.DemoCompanyCode == chargecode.Company.GC_Code;
			}
			Assert("Includes demo company charge code", foundChargeCodeInDemo);
		}
	}
}
