using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Testing
{
	[TestedType(typeof(AccChargeCodeUniversalCodeMappingCollection))]
	sealed class AccChargeCodeUniversalCodeMappingCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddRemove()
		{
			AccChargeCodeUniversalCodeMappingCollection collection = (AccChargeCodeUniversalCodeMappingCollection)GetCollectionToTest();
			var mapping = collection.AddNew();
			AssertEquals("Add: AUP_AC", collection.Master.PK, mapping.AUP_AC);

			collection.RemoveAll();
			AssertEquals("Remove: AUP_AC", ZGuid.Empty, mapping.AUP_AC);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			return new AccChargeCodeUniversalCodeMappingCollection(chargeCode);
		}
	}
}
