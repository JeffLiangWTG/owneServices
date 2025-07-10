using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public static class SupplementaryCodeProviderFactory
	{
		public static TSupplementaryCodeProvider GetByCountryCodeOrDefault<TSupplementaryCodeProvider>(ZString countryCode, Func<TSupplementaryCodeProvider> defaultCodeProviderFactory)
			where TSupplementaryCodeProvider : BaseSupplementaryCodeProvider
		{
			TSupplementaryCodeProvider result = null;
			if (!countryCode.IsEmpty)
			{
				var types = ObjectFactory.Get<Hashtable>("SupplementaryCodeProviders");
				var objectHandle = (ObjectHandle)types[countryCode.ToString()];
				result = (TSupplementaryCodeProvider)objectHandle?.GetObject(countryCode);
			}

			return result ?? defaultCodeProviderFactory();
		}
	}
}
