using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.DEReferenceData.Services
{
	public static class RetryHelper
	{
		public async static Task<T> RetryWithDelayAsync<T>(Func<Task<T>> action, TimeSpan? retryInterval = null, int? maxRetryTimes = null)
			where T : class
		{
			TimeSpan retryIntervalValue;
			if (!retryInterval.HasValue)
			{
				if (UnitTestDetector.IsRunningTests.Value)
				{
					retryIntervalValue = TimeSpan.FromMilliseconds(10);
				}
				else
				{
					retryIntervalValue = TimeSpan.FromMilliseconds(ApplicationConfig.MinDelayInMilliSecondsBetweenAttemptsInCaseOfDownloadError);
				}
			}
			else
			{
				retryIntervalValue = retryInterval.Value;
			}

			var maxRetryTimesValue = maxRetryTimes.GetValueOrDefault(ApplicationConfig.MaxAttemptsCountInCaseOfDownloadError);

			var retryCounter = 0;
			while (true)
			{
				try
				{
					return await action();
				}
				catch (Exception ex)
				{
					retryCounter += 1;
					Console.WriteLine($"The request ({retryCounter}/{maxRetryTimesValue}) attempt Failed. Exception Message: \"{ex.Message}\".");
					if (retryCounter == maxRetryTimesValue)
					{
						Console.WriteLine($"\r\nReached maximum of {maxRetryTimesValue} attempts, program aborted, the last exception thrown.");
						throw;
					}
					else
					{
						Console.WriteLine($"Another attempt would be conducted in {retryIntervalValue.Minutes} min {retryIntervalValue.Seconds} sec.");
						Thread.Sleep(retryIntervalValue);
					}
				}
			}
		}
	}
}
