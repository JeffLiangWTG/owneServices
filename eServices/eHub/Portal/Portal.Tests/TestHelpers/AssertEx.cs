using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.TestHelpers
{
	public static class AssertEx
	{
		public static void PropertyValuesAreEquals(object expected, object actual, bool ignoreCase = false)
		{
			Assert.IsNotNull(actual);

			PropertyInfo[] properties = expected.GetType().GetProperties();
			foreach (PropertyInfo expectedProp in properties)
			{
				object expectedValue = expectedProp.GetValue(expected, null);
				object actualValue; string message;
				var actualDictionary = actual as Dictionary<string, string>;
				if (actualDictionary != null)
				{
					actualValue = actualDictionary[expectedProp.Name];
					message = String.Format("Value of Dictionary<string,string>[\"{0}\"] does not match. Expected: {1} but was: {2}", expectedProp.Name, expectedValue, actualValue);
				}
				else
				{
					PropertyInfo actualProp = actual.GetType().GetProperty(expectedProp.Name);
					Assert.IsNotNull(actualProp, "Expected property {0}.{1} not found.", expectedProp.DeclaringType.Name, expectedProp.Name);
					actualValue = actualProp.GetValue(actual, null);
					message = String.Format("Property {0}.{1} does not match. Expected: {2} but was: {3}. Compare between {4} and {5}.", actualProp.DeclaringType.Name, actualProp.Name, expectedValue, actualValue, expected, actual);
				}

				if (expectedValue is string && actualValue is string)
					Assert.AreEqual(expectedValue as string, actualValue as string, ignoreCase, message);
				else
					Assert.AreEqual(expectedValue, actualValue, message);
			}
		}

		public static void JsonResultMatchesList(IList<object> expected, JsonResult actual, string dataProp, bool isAllowEmptyResult = false)
		{
			PropertyInfo prop = actual.Data.GetType().GetProperty(dataProp);
			Assert.IsNotNull(prop, "{0} property is null.", dataProp);
			var data = prop.GetValue(actual.Data, null) as IList;
			Assert.IsNotNull(data);
			if(!isAllowEmptyResult) Assert.IsTrue(data.Count > 0, "Expect actual data length > 0.");
			for (int i = 0; i < data.Count; i++)
			{
				Type t = expected[i].GetType();
				if (t.IsPrimitive || t.Equals(typeof(string)))
				{
					Assert.AreEqual(expected[i], data[i]);
				}
				else if (t.GetInterface(typeof(ICollection).FullName) != null)
				{
					CollectionAssert.AreEqual(expected[i] as ICollection, data[i] as ICollection);
				}
				else
				{
					AssertEx.PropertyValuesAreEquals(expected[i], data[i], false);
				}
			}
		}

		public static void JsonResultEqualList(IList<object> expected, JsonResult actual, string dataProp)
		{
			PropertyInfo prop = actual.Data.GetType().GetProperty(dataProp);
			Assert.IsNotNull(prop, "{0} property is null.", dataProp);
			var data = prop.GetValue(actual.Data, null) as IList;
			Assert.AreEqual(expected.Count, data.Count, "Expect match size of list");
			Assert.IsNotNull(data);
			for (int i = 0; i < data.Count; i++)
			{
				Type t = expected[i].GetType();
				if (t.IsPrimitive || t.Equals(typeof(string)))
				{
					Assert.AreEqual(expected[i], data[i]);
				}
				else if (t.GetInterface(typeof(ICollection).FullName) != null)
				{
					CollectionAssert.AreEqual(expected[i] as ICollection, data[i] as ICollection);
				}
				else
				{
					AssertEx.PropertyValuesAreEquals(expected[i], data[i], false);
				}
			}
		}
	}
}
