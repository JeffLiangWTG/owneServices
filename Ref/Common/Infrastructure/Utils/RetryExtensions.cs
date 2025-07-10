using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public static class RetryExtensions
	{
		public static async Task<TResult> Retry<TResult, TException1, TException2>(Func<Task<TResult>> action, int maxRetries, Action handleEx, Func<TException1, bool> cond1, Func<TException2, bool> cond2, [CallerMemberName] string methodName = null)
			where TException1 : Exception
			where TException2 : Exception
		{
			var result = default(TResult);
			var retries = 0;
			while (retries < maxRetries)
			{
				Console.WriteLine($"{methodName} - attempt #{retries + 1}, maxAttempts: {maxRetries}");
				try
				{
					result = await action();
					break;
				}
				catch (TException1 ex) when (retries < maxRetries - 1 && cond1(ex))
				{
					retries++;
					handleEx();
				}
				catch (TException2 ex) when (retries < maxRetries - 1 && cond2(ex))
				{
					retries++;
					handleEx();
				}
			}
			return result;
		}
	}
}
