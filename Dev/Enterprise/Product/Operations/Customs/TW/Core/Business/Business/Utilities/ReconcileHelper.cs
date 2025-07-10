using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public interface IReconcileCandidate
	{
		ZDecimal ReconciledCustomsValue { get; set; }

		ZDecimal PreReconciledCustomsValue { get; }

		ZInt SecondarySortingValue { get; }
	}

	public static class ReconcileHelper
	{
		public static void ReconcileValues(IEnumerable<IReconcileCandidate> candidates, ZInt discrepancy)
		{
			candidates.ToList().ForEach(x => x.ReconciledCustomsValue = x.PreReconciledCustomsValue);
			if (discrepancy != ZInt.Zero)
			{
				var isToAdd = discrepancy > ZDecimal.Zero;
				var candidatesToDistibute = isToAdd ? candidates : candidates.Where(x => x.PreReconciledCustomsValue != 1m);

				if (candidatesToDistibute.Any())
				{
					var lineCounts = candidatesToDistibute.Count();
					var toDistibute = discrepancy;
					if (Math.Abs(discrepancy) >= lineCounts)
					{
						var toAdjustForEveryItem = discrepancy / lineCounts;
						toDistibute = discrepancy % lineCounts;
						candidatesToDistibute.ToList().ForEach(x => x.ReconciledCustomsValue += toAdjustForEveryItem);
					}

					candidatesToDistibute.OrderByDescending(x => x.ReconciledCustomsValue).ThenBy(x => x.SecondarySortingValue).Take(Math.Abs(toDistibute)).ToList().ForEach(x => x.ReconciledCustomsValue += isToAdd ? 1m : -1m);
				}
			}
		}
	}
}
