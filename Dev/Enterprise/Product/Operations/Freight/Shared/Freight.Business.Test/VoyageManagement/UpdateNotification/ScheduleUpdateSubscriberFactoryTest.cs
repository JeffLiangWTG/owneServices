using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ScheduleUpdateSubscriberFactoryTest : TestCaseWithFactory
	{
		public const string ObjectName = "ScheduleUpdateSubscribers";

		public delegate void DoETDChanged(IScheduleUpdateServices services, VoyageOrigin origin, ZDateTime oldETD);
		public delegate void DoETAChanged(IScheduleUpdateServices services, VoyageDestination destination, ZDateTime oldETA);

		public void TestGetSubscribersCached()
		{
			var subscriber1 = new Mock<IScheduleUpdateSubscriber>(MockBehavior.Strict);
			var subscriber2 = new Mock<IScheduleUpdateSubscriber>(MockBehavior.Strict);

			var list = new ArrayList { subscriber1.Object, subscriber2.Object };

			using (ObjectFactory.Substitute(ObjectName, list))
			{
				IScheduleUpdateSubscriber[] subscribers = ScheduleUpdateSubscriberFactory.GetSubscribers(Factory);

				AssertEquals("subscribers.Length", 2, subscribers.Length);
				AssertSame("subscribers[0]", subscriber1.Object, subscribers[0]);
				AssertSame("subscribers[1]", subscriber2.Object, subscribers[1]);

				AssertSame("result should be cached", subscribers, ScheduleUpdateSubscriberFactory.GetSubscribers(Factory));
			}
		}

		public void TestGetSubscribers()
		{
			var subscriber1 = new Mock<IScheduleUpdateSubscriber>(MockBehavior.Strict);
			var subscriber2 = new Mock<IScheduleUpdateSubscriber>(MockBehavior.Strict);

			var list = new ArrayList { subscriber1.Object, subscriber2.Object };

			using (ObjectFactory.Substitute(ObjectName, list))
			{
				IScheduleUpdateSubscriber[] subscribers = ScheduleUpdateSubscriberFactory.GetSubscribers();

				AssertEquals("subscribers.Length", 2, subscribers.Length);
				AssertSame("subscribers[0]", subscriber1.Object, subscribers[0]);
				AssertSame("subscribers[1]", subscriber2.Object, subscribers[1]);

				AssertEquals("don't cache", false, object.ReferenceEquals(subscribers, ScheduleUpdateSubscriberFactory.GetSubscribers()));
			}
		}
	}
}
