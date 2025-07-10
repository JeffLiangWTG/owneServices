using System;
using System.Text;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class ScheduleUpdateSubscriberTest<T> : TestCaseWithFactory
			where T : IScheduleUpdateSubscriber, new()
	{
		public void TestIsRegistered()
		{
			Type expectedType = typeof(T);

			StringBuilder message = new StringBuilder();
			message.Append("The type ");
			message.Append(expectedType.FullName);
			message.AppendLine(" was not registered.");
			message.AppendLine();
			message.AppendLine("Found:");

			bool found = false;

			foreach (IScheduleUpdateSubscriber subscriber in ScheduleUpdateSubscriberFactory.GetSubscribers())
			{
				Type actualType = subscriber.GetType();

				if (actualType == expectedType)
				{
					found = true;
					break;
				}
				else
				{
					message.AppendLine(actualType.FullName);
				}
			}

			AssertEquals(message.ToString(), true, found);
		}
	}
}
