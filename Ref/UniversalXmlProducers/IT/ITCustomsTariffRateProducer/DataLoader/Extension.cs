using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader
{
	internal static class Extension
	{
		internal static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
		{
			T[] bucket = null;
			var count = 0;

			foreach (var item in source)
			{
				if (bucket == null)
				{
					bucket = new T[batchSize];
				}

				bucket[count++] = item;

				if (count != batchSize)
				{
					continue;
				}

				yield return bucket.Select(x => x);

				bucket = null;
				count = 0;
			}

			if (bucket != null && count > 0)
			{
				yield return bucket.Take(count);
			}
		}
	}
}
