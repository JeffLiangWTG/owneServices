using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public static class ScheduleUpdateSubscriberFactory
	{
		public static IScheduleUpdateSubscriber[] GetSubscribers(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			return factory.GetCachedValue("ScheduleUpdateSubscriberFactory.GetSubscribers", GetSubscribers);
		}

		public static IScheduleUpdateSubscriber[] GetSubscribers()
		{
			ArrayList list = ObjectFactory.Get<ArrayList>("ScheduleUpdateSubscribers");
			return (IScheduleUpdateSubscriber[])list.ToArray(typeof(IScheduleUpdateSubscriber));
		}
	}
}
