using System.Collections;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class CustomsNumberViewStmNumsBusinessProviderHelper
	{
		public static CustomsNumberViewStmNumsBusinessProvider GetProvider(BusinessObjectFactory factory, string providerKey, ZGuid parentPk)
		{
			CustomsNumberViewStmNumsBusinessProvider provider = null;
			if (parentPk.IsValid)
			{
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "CustomsNumberViewStmNumsBusinessProvider_{0}_{1}", providerKey, parentPk);
				provider = factory.GetCachedValue(cacheKey, () =>
				{
					CustomsNumberViewStmNumsBusinessProvider result = null;
					if (!string.IsNullOrEmpty(providerKey))
					{
						var providers = ObjectFactory.Get<Hashtable>("CustomsNumberViewStmNumsBusinessProviders");
						var objectHandle = (ObjectHandle)providers[providerKey];
						if (objectHandle != null)
						{
							result = (CustomsNumberViewStmNumsBusinessProvider)objectHandle.GetObject(factory, parentPk);
						}
					}
					return result;
				});
			}
			return provider;
		}
	}
}
