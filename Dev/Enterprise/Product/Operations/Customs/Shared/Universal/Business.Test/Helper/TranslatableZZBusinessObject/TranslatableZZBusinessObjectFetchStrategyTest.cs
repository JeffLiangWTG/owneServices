using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Universal.Testing
{
	class TranslatableZZBusinessObjectFetchStrategyTest : TestCaseWithFactory
	{
		public void TestAllITranslatableZZBusinessObjectsHaveFetchStrategies()
		{
			var currentAssembly = Assembly.Load("Enterprise.Customs.Universal");
			var allITranslatableZZBusinessObjects = currentAssembly.DefinedTypes.Where(ti => ti.IsClass && typeof(ITranslatableZZBusinessObject).IsAssignableFrom(ti.AsType()));
			CombineAssertions(() =>
			{
				foreach (var bizObjTypeInfo in allITranslatableZZBusinessObjects)
				{
					var bizObj = Factory.GetNull(bizObjTypeInfo.AsType());
					Assert($"{bizObjTypeInfo.FullName} implements the ITranslatableZZBusinessObject interface and therefore should have a FetchStrategy inheriting from TranslatableZZBusinessObjectFetchStrategy.", IsOfGenericType(bizObj.FetchStrategy.GetType(), typeof(TranslatableZZBusinessObjectFetchStrategy<>)));
				}
			});

			bool IsOfGenericType(Type type, Type genericType)
			{
				while (type.BaseType != null)
				{
					if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
					{
						return true;
					}

					type = type.BaseType;
				}

				return false;
			}
		}
	}
}
