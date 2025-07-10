using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeCodesForGlobalCollection))]
	sealed class AccChargeCodesForGlobalCollectionTest : ActiveBusinessObjectCollectionTestCase<AccChargeCodesForGlobalCollection>
	{
		protected override AccChargeCodesForGlobalCollection GetCollectionToTest()
		{
			var globalChargeCode = Factory.New<AccChargeCode>();
			globalChargeCode.AC_Code = "123";
			globalChargeCode.AC_GC = ZGuid.Empty;
			return new AccChargeCodesForGlobalCollection(Factory, globalChargeCode.AC_Code);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorWithNullAccChargeCode()
		{
			new AccChargeCodesForGlobalCollection(Factory, null);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorWithNonGlobalChargeCode()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			AssertEquals("precondition, defaults to non-global", false, chargeCode.IsGlobal);
			new AccChargeCodesForGlobalCollection(Factory, chargeCode.AC_Code);
		}

		public void TestLoadsRelevantChargeCodes()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			const string aChargeCodeThatExistsInDemoCompany = "FRT";
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode,
				true, true, "NM1", aChargeCodeThatExistsInDemoCompany);

			var collection = new AccChargeCodesForGlobalCollection(new BusinessObjectFactory(), globalChargeCode.AC_Code);

			Assert("Contains Linked Charge Code", collection.Contains(normalChargeCodeLinked));
			AssertEquals("Does Not Contain Unlinked Charge Code", false, collection.Contains(normalChargeCode));
			AssertEquals("Does Not Contain Global Charge Code", false, collection.Contains(globalChargeCode));

			foreach (AccChargeCode chargecode in collection)
			{
				AssertNotEquals("No charge code in demo company", GlbCompany.DemoCompanyCode, chargecode.Company.GC_Code);
			}
		}
	}
}
