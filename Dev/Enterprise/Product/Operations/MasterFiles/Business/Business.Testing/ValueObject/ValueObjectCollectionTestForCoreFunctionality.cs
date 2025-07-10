using CargoWise.EntityFramework.Testing;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class ValueObjectCollectionTestForCoreFunctionality : TestCaseWithFactory
	{
		public void TestIsSpecifiedIsFalseByDefault()
		{
			AssertEquals("IsSpecified should be false by default", false, new TestValueObjectCollection().IsSpecified);
		}

		public void TestIsSpecifiedWhenCountGreaterThan1()
		{
			TestValueObjectCollection collection = new TestValueObjectCollection();
			AssertEquals("IsSpecified should be false by default", false, collection.IsSpecified);
			collection.Add(new TestValueObject());
			AssertEquals("IsSpecified should be true when Count >= 1", true, collection.IsSpecified);
			collection.Clear();
			AssertEquals("IsSpecified should be false again when the count is zero again", false, collection.IsSpecified);
		}

		public void TestSettingIsSpecifiedManually()
		{
			TestValueObjectCollection collection = new TestValueObjectCollection();
			collection.IsSpecified = true;
			AssertEquals("IsSpecified set to true", true, collection.IsSpecified);

			collection.Add(new TestValueObject());
			collection.Clear();
			AssertEquals("IsSpecified set to true after collection add/clear", true, collection.IsSpecified);

			collection.Add(new TestValueObject());
			collection.IsSpecified = false;
			AssertEquals("IsSpecified set to false", false, collection.IsSpecified);
			AssertEquals("Collection count set to zero when IsSpecified set to false", 0, collection.Count);
		}

		public void TestEqualityOperatorOverloads()
		{
			TestValueObjectCollection specifiedCollection = new TestValueObjectCollection();
			specifiedCollection.IsSpecified = true;
			TestValueObjectCollection unspecifiedCollection = new TestValueObjectCollection();
			unspecifiedCollection.IsSpecified = false;

			AssertEqualsByOperator(
				"When IsSpecified=true, should equal null so that XmlSerializer doesn't create a node for it",
				true, unspecifiedCollection, null);
			AssertEqualsByOperator(
				"When IsSpecified=true, should equal null so that XmlSerializer doesn't create a node for it",
				true, null, unspecifiedCollection);
			AssertEqualsByOperator("When IsSpecified=true, should not equal null", false, specifiedCollection, null);
			AssertEqualsByOperator("When IsSpecified=true, should not equal null", false, null, specifiedCollection);

			AssertEqualsByOperator("IsSpecified=true == IsSpecified=true", true, unspecifiedCollection, unspecifiedCollection);
			AssertEqualsByOperator("IsSpecified=true == IsSpecified=true", true, specifiedCollection, specifiedCollection);

			AssertEqualsByOperator("IsSpecified=false == IsSpecified=false", false, specifiedCollection, unspecifiedCollection);
			AssertEqualsByOperator("IsSpecified=false == IsSpecified=false", false, unspecifiedCollection, specifiedCollection);
		}

		void AssertEqualsByOperator(string message, bool expectedValue, ValueObjectCollection lhs, ValueObjectCollection rhs)
		{
			AssertEquals(message + "; using operator==", expectedValue, lhs == rhs);
			AssertEquals(message + "; using operator!=", !expectedValue, lhs != rhs);
		}
	}
}
