using System;
using System.Collections;
using CargoWise.Application;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class NameBasedProvider
	{
		public static TProvider Get<TProvider>(string providerFactoryKey, params object[] constructorArguments)
		{
			var providerFactoryGroupKey = typeof(TProvider).Name + (NoResString)"s";
			var providerFactoryGroup = ObjectFactory.Get<Hashtable>(providerFactoryGroupKey);
			var providerFactory = providerFactoryGroup.ContainsKey(providerFactoryKey)
				? (ObjectHandle)providerFactoryGroup[providerFactoryKey]
				: (ObjectHandle)providerFactoryGroup["Default"];
			var provider = providerFactory != null
				? (TProvider)providerFactory.GetObject(constructorArguments)
				: (TProvider)Activator.CreateInstance(typeof(TProvider), constructorArguments);
			return provider;
		}
	}
}
