using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class ParamValuesCollectionTest : TestCase
{
	public void TestConstructors() => CombineAssertions(() =>
	{
		const int testCapacity = 887;

		var paramValueCollection = new ParamValueCollection(testCapacity);
		AssertEquals("Simple capacity", testCapacity, paramValueCollection.Capacity);

		var testParamValues = new ParamValue[] { new ("Test Name 1", "Test value 1"), new ("Test Name 2", "Test value 2") };
		paramValueCollection = new ParamValueCollection(testParamValues.Cast<IParamValue>());
		AssertContainsExactElementsInExactOrder("collection", paramValueCollection, paramValueCollection);

		paramValueCollection = new ParamValueCollection(testCapacity, testParamValues.Cast<IParamValue>());
		AssertEquals("capacity with enumeration: capacity", testCapacity, paramValueCollection.Capacity);
		AssertContainsExactElementsInExactOrder("capacity with enumeration: collection", paramValueCollection, paramValueCollection);
	});

	public void TestAdd_ParamValue()
	{
		var paramValueCollection = new ParamValueCollection();
		var paramValue = new ParamValue("Test Name", "Test value");
		paramValueCollection.Add(paramValue);
		AssertContainsExactElementsInExactOrder([paramValue], paramValueCollection);
	}

	public void TestAdd_ParamValues()
	{
		var paramValueCollection = new ParamValueCollection();
		var paramValue = new ParamValues("Test Name", ["value1", "value2"]);
		paramValueCollection.Add(paramValue);
		AssertContainsExactElementsInExactOrder([paramValue], paramValueCollection);
	}
}
