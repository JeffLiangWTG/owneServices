using System;
using System.Reflection;

namespace Enterprise.Customs.Business.AccumulativeAmendment
{
	public static class IDProvider
	{
		public static PropertyInfo GetIDField(Type dataProviderType)
		{
			foreach (var propertyInfo in dataProviderType.GetProperties())
			{
				if (propertyInfo.GetCustomAttribute<IDAttribute>() != null)
				{
					return propertyInfo;
				}
			}
			return null;
		}
	}
}
