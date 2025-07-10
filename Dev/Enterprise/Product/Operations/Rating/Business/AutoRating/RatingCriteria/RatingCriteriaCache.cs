using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class RatingCriteriaCache
	{
		public RatingCriteriaCache(RatingCriteria criteria)
		{
			fastLineProvider = new FastLineProvider(criteria);
			this.criteria = criteria;
		}

		readonly RatingCriteria criteria;

		public readonly Dictionary<ZGuid, Calculator> BaseCalculators = new Dictionary<ZGuid, Calculator>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public readonly Dictionary<ZGuid, List<IRateLine>> CostBasedCalculatorRelatedLines = new Dictionary<ZGuid, List<IRateLine>>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public readonly Dictionary<ZGuid, HashSet<ZGuid>> TransportProvidersAndContractorPKsCache = new Dictionary<ZGuid, HashSet<ZGuid>>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public readonly Dictionary<ZGuid, IEnumerable<ZString>> CostBasedCalculatorUnits = new Dictionary<ZGuid, IEnumerable<ZString>>();

		public FastLine GetOrCreateFastLine(IRateLine line) => fastLineProvider.GetOrCreate(line);
		readonly FastLineProvider fastLineProvider;

		public bool? IsCollect(bool isCost, string chargeGroup)
		{
			var map = isCost
				? (costIsCollect ??= new Dictionary<string, bool?>())
				: (sellIsCollect ??= new Dictionary<string, bool?>());
			if (!map.TryGetValue(chargeGroup, out var result))
			{
				var prepaidCollect = criteria.PaymentTerm.GetPrepaidCollect(isCost ? CostSell.Cost : CostSell.Revenue, chargeGroup);
				result = prepaidCollect.In(Core.Constants.PaymentType.Collect, Core.Constants.PaymentType.Prepaid)
					? prepaidCollect == Core.Constants.PaymentType.Collect
					: null;

				map[chargeGroup] = result;
			}
			return result;
		}
		Dictionary<string, bool?> costIsCollect;
		Dictionary<string, bool?> sellIsCollect;
	}

	public class FastLineProvider
	{
		public FastLineProvider(RatingCriteria criteria)
		{
			Criteria = criteria;
		}
		public RatingCriteria Criteria { get; private set; }

		/// <summary>
		/// Note: There can be multiple RateLines in memory for the same record in the database.
		/// They are linked to different RatingHeader records (via ParentRateEntry.ParentRatingHeader), with different company tariff levels.
		/// </summary>
		public FastLine GetOrCreate(IRateLine line)
		{
			if (!lookup.TryGetValue(line, out var result))
			{
				result = new FastLine(line, Criteria);
				lookup.Add(line, result);
			}
			return result;
		}

		readonly Dictionary<IRateLine, FastLine> lookup = new Dictionary<IRateLine, FastLine>();
	}
}
