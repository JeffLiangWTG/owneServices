using System;
using System.Threading;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.Staging.Common
{
	public class HttpClientRetryHandler
	{
		readonly int maxRetryAttempts;
		readonly int retryIntervalMilliseconds;

		public HttpClientRetryHandler(int maxRetryAttempts = 3, int retryIntervalMilliseconds = 1000)
		{
			Argument.GreaterThan(maxRetryAttempts, 0, "max retry attempts");
			Argument.GreaterThan(retryIntervalMilliseconds, 0, "retry interval milliseconds");
			this.maxRetryAttempts = maxRetryAttempts;
			this.retryIntervalMilliseconds = retryIntervalMilliseconds;
		}

		public T Execute<T>(Uri uri, Func<T> retryFunc)
		{
			for (int retryCount = 1; retryCount <= maxRetryAttempts; retryCount++)
			{
				try
				{
					return retryFunc();
				}
				catch (Exception ex)
				{
					Console.WriteLine(
						$"The {retryCount} attempt to request {uri} failed. Exception message: {ex.Message}");
					if (retryCount == maxRetryAttempts)
					{
						Console.Error.WriteLine($"Failed after requesting {uri} {maxRetryAttempts} times.");
						throw;
					}
					Thread.Sleep(retryIntervalMilliseconds);
				}
			}

			return default;
		}
	}
}
