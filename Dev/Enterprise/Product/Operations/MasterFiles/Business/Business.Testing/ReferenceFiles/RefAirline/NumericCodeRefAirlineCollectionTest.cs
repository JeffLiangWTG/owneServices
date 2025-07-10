using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(NumericCodeRefAirlineCollection))]
	public class NumericCodeRefAirlineCollectionTest : ActiveBusinessObjectCollectionTestCase<NumericCodeRefAirlineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<NumericCodeRefAirline>();
		}

		public override void TestAddNew()
		{
			var initialCount = Collection.Count;

			var airline1 = Factory.NewWithValidTestData<NumericCodeRefAirline>();
			var airline2 = Factory.NewWithValidTestData<NumericCodeRefAirline>();

			AssertEquals("Collection count", initialCount + 2, Collection.Count);
			Assert("Contains new elements", Collection.Contains(airline1));
			Assert("Contains new elements", Collection.Contains(airline2));
		}

		public void TestAdditionalFilter()
		{
			var airline1 = Factory.NewWithValidTestData<NumericCodeRefAirline>();
			airline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "";

			var airline2 = Factory.NewWithValidTestData<NumericCodeRefAirline>();
			airline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "AAA";

			var collection = new NumericCodeRefAirlineCollection(Factory);

			AssertEquals("Should not contain airline without numeric code", false, collection.Contains(airline1));
			AssertEquals("Should contain airline with numeric code", true, collection.Contains(airline2));
		}
	}
}
