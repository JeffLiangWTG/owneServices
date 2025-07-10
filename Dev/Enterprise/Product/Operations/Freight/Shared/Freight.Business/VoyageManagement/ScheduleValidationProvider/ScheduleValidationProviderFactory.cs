using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public static class ScheduleValidationProviderFactory
	{
		public static IScheduleValidationProvider[] GetProviders(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			return factory.GetCachedValue("ScheduleValidationProviderFactory.GetProviders", GetProviders);
		}

		public static IScheduleValidationProvider[] GetProviders()
		{
			ArrayList list = ObjectFactory.Get<ArrayList>("ScheduleValidationProviders");
			return (IScheduleValidationProvider[])list.ToArray(typeof(IScheduleValidationProvider));
		}
	}
}
