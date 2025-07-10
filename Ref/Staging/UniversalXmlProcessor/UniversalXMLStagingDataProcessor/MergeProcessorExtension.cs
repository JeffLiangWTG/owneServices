using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public static class MergeProcessorExtension
	{
		public static T[][] SplitProcessingEntitiesIntoArray<T>(this IEnumerable<T> processingEntities, int mergeSize, int noMaxOfBatches)
		{
			Argument.NotNull(processingEntities, nameof(processingEntities));
			return processingEntities.Select((e, index) => new { e, index }).GroupBy(a => a.index / mergeSize)
						.Select(grp => grp.Select(g => g.e).ToArray()).Take(noMaxOfBatches).ToArray();
		}

		public static async Task<T1> Retry<T, T1>(IEnumerable<T> entities, Func<IEnumerable<T>, int, Task<T1>> funcToRetry, Func<T1, IEnumerable<T>> convertResult, int noOfRetries, int maximumRetries, int batchSize, Guid sourceDataPk, [CallerMemberName] string methodName = null)
		{
			Argument.NotNull(funcToRetry, nameof(funcToRetry));
			Argument.NotNull(entities, nameof(entities));

			Console.WriteLine($"Retrying in {methodName} - attempt #{noOfRetries}, batch Size: {batchSize}");
			var result = await funcToRetry(entities, batchSize);
			var retryEntities = convertResult(result);
			if (retryEntities.Any() && batchSize > 1 && noOfRetries < maximumRetries)
			{
				result = await Retry(retryEntities, funcToRetry, convertResult, noOfRetries + 1, maximumRetries, CalculateRetryMergeSize(batchSize, retryEntities.Count()), sourceDataPk);
			}
			return result;
		}

		public static int CalculateRetryMergeSize(int currentMergeSize, int entityCount)
		{
			return Math.Max(Math.Min(currentMergeSize / 3, entityCount / 10), 1);
		}

		public static int CalculateMaxInitialMergeSize(int maximumRetries)
		{
			Argument.GreaterThan(maximumRetries, 0, nameof(maximumRetries));
			if (maximumRetries == 1)
			{
				return 5;
			}
			else
			{
				return 3 * CalculateMaxInitialMergeSize(maximumRetries - 1) + 2;
			}
		}
	}
}
