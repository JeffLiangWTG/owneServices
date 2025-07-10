using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.eTail.Business;

public class DataObjectCacheFactoryService : IService
{
	internal DataObjectCacheFactoryService(BusinessObjectFactory factory)
	{
		cache = [];
	}

	public readonly Dictionary<string, IDisposable> cache;

	public string BuildKey(params string[] parameters)
	{
		var key = string.Join("-", parameters);
		return key;
	}

	public T GetValue<T>(string key, Func<T> getValueDelegate) where T : IDisposable
	{
		if (!cache.TryGetValue(key, out var value))
		{
			value = getValueDelegate();
			cache[key] = value;
		}

		return (T)value;
	}

	public static DataObjectCacheFactoryService GetOrCreateNewInstance(BusinessObjectFactory factory)
	{
		DataObjectCacheFactoryService dataObjectCacheFactoryService = factory.ServiceContainer.GetService<DataObjectCacheFactoryService>();
		if (dataObjectCacheFactoryService == null)
		{
			factory.ServiceContainer.AddService(new DataObjectCacheFactoryService(factory));
			dataObjectCacheFactoryService = factory.ServiceContainer.GetService<DataObjectCacheFactoryService>();
		}
		return dataObjectCacheFactoryService;
	}

	public static void DisposeInstance(BusinessObjectFactory factory)
	{
		DataObjectCacheFactoryService dataObjectCacheFactoryService = factory.ServiceContainer.GetService<DataObjectCacheFactoryService>();
		if (dataObjectCacheFactoryService != null)
		{
			factory.ServiceContainer.RemoveService<DataObjectCacheFactoryService>();
		}
	}
}
