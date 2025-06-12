using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CargoWise.eServices.Billing.WcfService.IntegrationTests
{
	public class TestHelper
	{
		public static void DoWithRetry(Action action, string name, TimeSpan retryInterval, int maxAttemptCount = 5)
		{
			DoWithRetry<object>(() =>
			{
				action();
				return null;
			}, name, retryInterval, maxAttemptCount);
		}
		public static T DoWithRetry<T>(Func<T> action, string name, TimeSpan retryInterval, int maxAttemptCount = 5)
		{
			var exceptions = new List<Exception>();

			for (int attempted = 0; attempted < maxAttemptCount; attempted++)
			{
				try
				{
					if (attempted > 0)
					{
						Task.Delay(retryInterval).Wait(CancellationToken.None);
					}
					return action();
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}
			throw new AggregateException($"Executing {name} reached max retries with error(s)", exceptions);
		}
	}
}
