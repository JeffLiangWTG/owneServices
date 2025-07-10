using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	static class MultilingualBusinessObjectExtensions
	{
		public static void SetResourceString<TBizO>(this IMockResourceStringCache cache, TBizO context, Func<TBizO, ZPropertyInfo> propertyAccessor, string value)
			where TBizO : BusinessObject
		{
			var propertyInfo = propertyAccessor(context);
			var key = propertyInfo.CustomizableDataResourceStrings.Source.GetKey(context, (ZString)propertyInfo.Value);
			cache.Put(key, new ResourceStringData(key, value));
		}
	}
}
