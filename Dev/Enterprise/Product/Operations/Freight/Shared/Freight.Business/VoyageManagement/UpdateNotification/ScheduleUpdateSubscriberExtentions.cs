using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	internal static class ScheduleUpdateSubscriberExtentions
	{
		public static void ATDChanged(this IEnumerable<IScheduleUpdateSubscriber> subscribers, IScheduleUpdateServices services, VoyageOrigin origin, ZDateTime oldATD)
		{
			subscribers.RunSubscriberChangedAction(subscriber => subscriber.ATDChanged(services, origin, oldATD), ".ATDChanged()");
		}

		public static void ETDChanged(this IEnumerable<IScheduleUpdateSubscriber> subscribers, IScheduleUpdateServices services, VoyageOrigin origin, ZDateTime oldETD)
		{
			subscribers.RunSubscriberChangedAction(subscriber => subscriber.ETDChanged(services, origin, oldETD), ".ETDChanged()");
		}

		public static void ETAChanged(this IEnumerable<IScheduleUpdateSubscriber> subscribers, IScheduleUpdateServices services, VoyageDestination destination, ZDateTime oldETA)
		{
			subscribers.RunSubscriberChangedAction(subscriber => subscriber.ETAChanged(services, destination, oldETA), ".ETAChanged()");
		}

		static void RunSubscriberChangedAction(this IEnumerable<IScheduleUpdateSubscriber> subscribers,
			Action<IScheduleUpdateSubscriber> subscriberChangedAction,
			string methodName)
		{
			Argument.NotNull(subscribers, "subscribers");

			foreach (IScheduleUpdateSubscriber subscriber in subscribers)
			{
				if (subscriber == null)
				{
					continue;
				}

				try
				{
					subscriberChangedAction(subscriber);
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}

					ExceptionReporter.Instance.ReportException(subscriber.GetType().FullName + methodName, ex);
				}
			}
		}
	}
}
