using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	public abstract class DataProviderTestCase<T> : TestCaseWithFactory where T : class
	{
		T provider;

		protected T Provider => provider ?? (provider = GetProvider());

		public void TestAllProviderPropertiesHaveATestCase()
		{
			string[] existingTestMethods = (from methodInfo in GetType().GetMethods()
											where methodInfo.Name.StartsWith("Test")
											select ExtractMethodNameFromTestMethodName(methodInfo.Name)).Distinct().ToArray();
			Type providerType = typeof(T);
			List<string> list = new List<string>();
			IEnumerable<PropertyInfo> runtimeProperties = providerType.GetRuntimeProperties();
			IEnumerable<PropertyInfo> first = runtimeProperties.Where((PropertyInfo pi) => pi.GetGetMethod(nonPublic: true).IsPublic && pi.DeclaringType == providerType);
			IEnumerable<PropertyInfo> second = runtimeProperties.Where((PropertyInfo pi) => pi.GetGetMethod(nonPublic: true).IsPrivate && pi.Name.Contains("."));
			IEnumerable<PropertyInfo> propertyInfosToTest = first.Union(second);
			if (propertyInfosToTest.Any())
			{
				AssertionWithHtml.CombineAssertions(delegate
				{
					foreach (PropertyInfo item in propertyInfosToTest)
					{
						string text = ExtractNameFromPropertyInfo(item.Name);
						Assert("Property: " + text, existingTestMethods.Contains("Test" + text));
					}
				});
			}
			else
			{
				Assert("There are no differences to test from the abstract base class or explicit interface implementation", condition: true);
			}

			static string ExtractMethodNameFromTestMethodName(string name)
			{
				int num2 = name.IndexOf('_');
				return (num2 == -1) ? name : name.Substring(0, num2);
			}

			static string ExtractNameFromPropertyInfo(string name)
			{
				int num = name.LastIndexOf('.');
				return (num == -1) ? name : name.Substring(checked(num + 1));
			}
		}

		protected abstract T GetProvider();
	}
}
