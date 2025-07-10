using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class DataProviderTestCase<T, TInterface> : DataProviderTestCase<T>
		where T : class, TInterface
	{
		protected TInterface IProvider => Provider;
	}

	public abstract class DataProviderTestCase<T> : TestCaseWithFactory
		where T : class
	{
		public void TestNoEnumerables()
		{
			var enumerableProperties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.PropertyType.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(x.PropertyType.GetGenericTypeDefinition())).ToArray();
			if (enumerableProperties.Length > 0)
			{
				Fail("Do not use an IEnumerable use IReadOnlyCollection. In our experience it is easy to forget about the performance impact deferred execution and caching could have on message builders. On average we seem to be better off not using deferred execution and mandating caching on all collections. For this reason we prefer the use of IReadOnlyCollection instead of IEnumerable for data providers."
					+ System.Environment.NewLine
					+ string.Join(System.Environment.NewLine, enumerableProperties.Select(x => x.DeclaringType.Name + "." + x.Name)));
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCollectionsAreNotCachedAsCachedValue()
		{
			var privateFields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);
			var cachedValueFields = privateFields
				.Where(y => y.FieldType.IsGenericType
					&& y.FieldType.GetGenericTypeDefinition() == typeof(CachedValue<>)
					&& y.FieldType.GetGenericArguments().Single() is Type type
					&& !type.IsValueType //This will exclude DateTime?, decimal?
					&& (type.IsGenericType || type.IsArray)); //This will include IsGenericType List<>, ReadOnlyCollection<> and IsArray int[], string[]
			if (cachedValueFields.Any())
			{
				Fail(System.Environment.NewLine + string.Join(System.Environment.NewLine, cachedValueFields.Select(x => $"{x.DeclaringType.Name}.{x.Name} should not have a collection (Array, List, IReadOnlyCollection or IReadOnlyList), as generic CachedValue Type. Use a local variable for caching instead.")));
			}
			else
			{
				Assert(true);
			}
		}

		public void TestReadOnlyCollectionsAreCached()
		{
			if (ReadOnlyCollectionProperties.Count > 0)
			{
				CombineAssertions(() =>
				{
					foreach (var propertyInfo in ReadOnlyCollectionProperties)
					{
						var propertyName = $"{propertyInfo.DeclaringType.Name}.{propertyInfo.Name}";
						var propertyValue = propertyInfo.GetValue(Provider);
						AssertSame($"{propertyName} should be cached so that it won't be evaluated multiple times.", propertyValue, propertyInfo.GetValue(Provider));
						AssertNotNull($"{propertyName} should not be null.", propertyValue);
					}
				});
			}
			else
			{
				Assert(true);
			}
		}

		public void TestPropertiesShouldBeCached()
		{
			var propertiesNeedToBeCached = GetPropertiesNeedToBeCached();
			if (propertiesNeedToBeCached.Any())
			{
				CombineAssertions(() =>
				{
					foreach (var expression in propertiesNeedToBeCached)
					{
						var member = (MemberExpression)expression.Body;
						var propertyInfo = (PropertyInfo)member.Member;
						AssertSame($"{propertyInfo.DeclaringType.Name}.{propertyInfo.Name} should be cached.", propertyInfo.GetValue(Provider), propertyInfo.GetValue(Provider));
					}
				});
			}
			else
			{
				Assert(true);
			}
		}

		public void TestAllProviderPropertiesHaveATestCase()
		{
			var existingTestMethods = GetType().GetMethods().Where(methodInfo => methodInfo.Name.StartsWith("Test")).Select(methodInfo => ExtractMethodNameFromTestMethodName(methodInfo.Name)).Distinct().ToArray();
			var providerType = typeof(T);
			var propertyNames = new List<string>();
			var providerRuntimeProperties = providerType.GetRuntimeProperties();
			var publicPropertyInfos = providerRuntimeProperties.Where(pi => pi.GetGetMethod(true).IsPublic && pi.DeclaringType == providerType);
			var explicitPropertyInfos = providerRuntimeProperties.Where(pi => pi.GetGetMethod(true).IsPrivate && pi.Name.Contains("."));
			var propertyInfosToTest = publicPropertyInfos.Union(explicitPropertyInfos);
			if (propertyInfosToTest.Any())
			{
				CombineAssertions(() =>
				{
					foreach (var propertyInfo in propertyInfosToTest)
					{
						var name = ExtractNameFromPropertyInfo(propertyInfo.Name);
						Assert($"Property: {name}", existingTestMethods.Contains("Test" + name));
					}
				});
			}
			else
			{
				Assert("There are no differences to test from the abstract base class or explicit interface implementation", true);
			}

			string ExtractMethodNameFromTestMethodName(string name)
			{
				var posUnderscore = name.IndexOf('_');
				return posUnderscore == -1 ? name : name.Substring(0, posUnderscore);
			}

			string ExtractNameFromPropertyInfo(string name)
			{
				var lastFullStop = name.LastIndexOf('.');
				return lastFullStop == -1 ? name : name.Substring(lastFullStop + 1);
			}
		}

		protected abstract T GetProvider();

		protected T Provider => provider ?? (provider = GetProvider());
		T provider;

		IReadOnlyCollection<PropertyInfo> ReadOnlyCollectionProperties => readOnlyCollectionProperties ?? (readOnlyCollectionProperties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.PropertyType.IsGenericType && typeof(IReadOnlyCollection<>).IsAssignableFrom(x.PropertyType.GetGenericTypeDefinition())).ToArray());
		PropertyInfo[] readOnlyCollectionProperties;

		protected virtual IEnumerable<Expression<Func<T, object>>> GetPropertiesNeedToBeCached() => Enumerable.Empty<Expression<Func<T, object>>>();
	}
}
