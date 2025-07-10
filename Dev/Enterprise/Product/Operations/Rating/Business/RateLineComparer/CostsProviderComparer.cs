using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class CostsProviderComparer : BaseRateLineComparer
	{
		public CostsProviderComparer(RatingCriteria criteria)
		{
			this.criteria = criteria;
		}

		readonly RatingCriteria criteria;

		public override int Compare(FastLine line1, FastLine line2)
		{
			var serviceCompare = ServiceAutoRater.CompareServiceContractors(criteria, line1, line2);
			if (serviceCompare.HasValue)
			{
				return serviceCompare.Value;
			}

			var tp1Ranking = criteria.Creditors.GetRank(line1.ChargeCode.AC_ChargeGroup, GetOrgWithSourceForLine(line1));
			var tp2Ranking = criteria.Creditors.GetRank(line2.ChargeCode.AC_ChargeGroup, GetOrgWithSourceForLine(line2));

			if (tp1Ranking.Intersect(tp2Ranking).Any())
			{
				return 0;
			}

			var tp1Rank = tp1Ranking.Any() ? tp1Ranking.Min() : int.MaxValue;
			var tp2Rank = tp2Ranking.Any() ? tp2Ranking.Min() : int.MaxValue;

			return tp2Rank.CompareTo(tp1Rank);
		}

		OrgWithSource GetOrgWithSourceForLine(FastLine line)
		{
			var header = line.ParentRateEntry.ParentRatingHeader;

			var creditor = criteria.Creditors[line.ChargeCode.AC_ChargeGroup].FirstOrDefault(x => x.Org.PK == header.TH_OH);
			return creditor != null
					? OrgWithSource.New(header.Header, creditor.Source)
					: null;
		}

		protected override string GetName()
		{
			return (NoResString)"Creditors"; // log message, subject to change, more for support people as of now
		}
	}
}
