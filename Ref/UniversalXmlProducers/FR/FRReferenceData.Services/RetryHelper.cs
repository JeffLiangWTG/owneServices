using System;
using System.Globalization;
using System.Threading;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public static class RetryHelper
	{
		public static void RetryWithDelay(Action retryAction, TimeSpan retryInterval, int maxRetryTimes)
		{
			RetryWithDelay<object>(() =>
				{
					retryAction();
					return null;
				}, retryInterval, maxRetryTimes);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Exception will be treated in a report.")]
		public static T RetryWithDelay<T>(Func<T> retryAction, TimeSpan retryInterval, int maxRetryTimes)
			where T : class
		{
			if (!UnitTestDetector.IsRunningTests.Value)
			{
				retryInterval = TimeSpan.FromMilliseconds(10);
			}
			var retryCounter = 0;
			while (retryCounter < maxRetryTimes)
			{
				try
				{
					return retryAction();
				}
				catch (Exception ex)
				{
					HandleException(ref retryCounter, retryInterval, maxRetryTimes, ex);
				}
			}
			return null;
		}

		static void HandleException(ref int retryCounter, in TimeSpan retryInterval, in int maxRetryTimes, in Exception ex)
		{
			retryCounter += 1;
			Console.WriteLine($"The {retryCounter}{GetOrdinal(retryCounter)} Attempt Failed. Exception Message: \"{ex.Message}\".");
			if (retryCounter == maxRetryTimes)
			{
				Console.WriteLine("\r\nMaximized retry times exceeded, program aborted, the last exception thrown.");
				throw ex;
			}
			else
			{
				Console.WriteLine("Another attempt would be conducted 1 minute later.");
				Thread.Sleep(retryInterval);
			}
		}

		static string GetOrdinal(int num)
		{
			var number = num.ToString(CultureInfo.InvariantCulture);
			if (number.EndsWith("11", StringComparison.InvariantCultureIgnoreCase))
				return "th";
			if (number.EndsWith("12", StringComparison.InvariantCultureIgnoreCase))
				return "th";
			if (number.EndsWith("13", StringComparison.InvariantCultureIgnoreCase))
				return "th";
			if (number.EndsWith("1", StringComparison.InvariantCultureIgnoreCase))
				return "st";
			if (number.EndsWith("2", StringComparison.InvariantCultureIgnoreCase))
				return "nd";
			if (number.EndsWith("3", StringComparison.InvariantCultureIgnoreCase))
				return "rd";
			return "th";
		}
	}
}
