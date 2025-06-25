using System.Reflection;
using NUnit.Framework;

namespace CargoWise.Billing.Tests.Common
{
	public static class ObjectComparer
	{
		public static void AssertPropertiesAreEqual(object expected, object actual)
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			var actualType = actual.GetType();
			foreach (var expectedProperty in expected.GetType().GetProperties(flags))
			{
				var actualProperty = actualType.GetProperty(expectedProperty.Name, flags);
				Assert.That(actualProperty, Is.Not.Null, "Property {0} exists.", expectedProperty.Name);
				var actualPropertyValue = actualProperty.GetValue(actual);
				var expectedPropertyValue = expectedProperty.GetValue(expected);
				Assert.That(actualPropertyValue, Is.EqualTo(expectedPropertyValue), expectedProperty.Name);
			}
		}
	}
}