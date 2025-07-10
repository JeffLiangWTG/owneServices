using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.Common
{
	public class DeduplicationExclusionBuilder<TBizo>
		where TBizo : BusinessObject, IDeduplicatable
	{
		readonly DeduplicationExclusion exclusion;

		public DeduplicationExclusionBuilder()
		{
			exclusion = new DeduplicationExclusion();
		}

		public DeduplicationExclusion Exclusion => exclusion;

		void AddToExclusions(TBizo[] bizos, ResourceString description)
		{
			foreach (var item in bizos)
			{
				exclusion[item.NaturalKey] = description;
			}
		}

		public void BuildExclusion(ZQuery query, ResourceString description)
		{
			var factory = new ReadOnlyBusinessObjectFactory();
			var results = factory.Load<TBizo>(query);

			AddToExclusions(results, description);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "WTG4001:Using Enumerable Extension Methods on a Queryable.", Justification = "Baseline")]
		public void BuildExclusion(IQueryable<TBizo> bizos, ResourceString description, Func<TBizo, bool> predicate)
		{
			var results = bizos.Where(predicate);

			AddToExclusions(results.ToArray(), description);
		}
	}
}
