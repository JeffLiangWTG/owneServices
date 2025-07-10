using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	public static class DocDataObjectTestUtility
	{
		public static void AssertDataObjectPropertyIsAlterable(NonPersistentBusinessObject dataObject, ZString expectedValue, ZString propertyName)
		{
			Assertion.AssertMultilineASCIIEquals($"Get {propertyName}", expectedValue, (ZString)dataObject[propertyName]);
			dataObject[propertyName] = "ABC123";
			Assertion.AssertEquals($"Set {propertyName}", "ABC123", dataObject[propertyName]);
			dataObject[propertyName] = expectedValue;
		}

		public static void AssertDataObjectPropertyIsAlterable(NonPersistentBusinessObject dataObject, ZDate expectedValue, ZString propertyName)
		{
			Assertion.AssertEquals($"Get {propertyName}", expectedValue, (ZDate)dataObject[propertyName]);
			var editedValue = ZDate.Today.AddDays(1);
			dataObject[propertyName] = editedValue;
			Assertion.AssertEquals($"Set {propertyName}", editedValue, dataObject[propertyName]);
			dataObject[propertyName] = expectedValue;
		}

		public static void AssertDataObjectPropertyIsAlterable(NonPersistentBusinessObject dataObject, ZInt expectedValue, ZString propertyName)
		{
			Assertion.AssertEquals($"Get {propertyName}", expectedValue, (ZInt)dataObject[propertyName]);
			var editedValue = new Random().Next();
			dataObject[propertyName] = editedValue;
			Assertion.AssertEquals($"Set {propertyName}", editedValue, dataObject[propertyName]);
			dataObject[propertyName] = expectedValue;
		}

		public static void AssertDataObjectPropertyIsAlterable(NonPersistentBusinessObject dataObject, ZBool expectedValue, ZString propertyName)
		{
			Assertion.AssertEquals($"Get {propertyName}", expectedValue, (ZBool)dataObject[propertyName]);
			var editedValue = true;
			dataObject[propertyName] = editedValue;
			Assertion.AssertEquals($"Set {propertyName}", editedValue, dataObject[propertyName]);
			dataObject[propertyName] = expectedValue;
		}
	}
}
