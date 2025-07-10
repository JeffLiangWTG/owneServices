using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccGlobalChargeCodeCollection))]
	sealed class AccGlobalChargeCodeCollectionTest : ActiveBusinessObjectCollectionTestCase<AccGlobalChargeCodeCollection>
	{
		protected override AccGlobalChargeCodeCollection GetCollectionToTest()
		{
			return new AccGlobalChargeCodeCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return AccChargeCode.CreateGlobalChargeCode(Factory);
		}

		public void TestOnlyLoadsGlobalChargeCodes()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var collection = new AccGlobalChargeCodeCollection(new BusinessObjectFactory());

			Assert("Contains Global Charge Code", collection.Contains(globalChargeCode));
			AssertEquals("Does Not Contain Normal Charge Code", false, collection.Contains(normalChargeCode));
		}
	}
}
