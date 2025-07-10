using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class RateTypeComparer : BaseRateLineComparer
	{
		public RateTypeComparer(RatingCriteria criteria)
		{
			this.criteria = criteria;
		}

		readonly RatingCriteria criteria;

		public override int Compare(FastLine line1, FastLine line2)
		{
			var entry1 = line1.ParentRateEntry;
			var entry2 = line2.ParentRateEntry;
			int result = 0;

			if (line1.IsCostRate()
				|| line2.IsCostRate()
				|| !IsClientSame(line1.Line, line2.Line)
				|| !line1.Line.IsFeeChargeSame(line2.Line))
			{
				return result;
			}

			if (IsEntry1FromClientRateAndEntry2FromCompanyTariffLevelOverride(entry1, entry2))
			{
				return -1;
			}
			else if (IsEntry1FromClientRateAndEntry2FromCompanyTariffLevelOverride(entry2, entry1))
			{
				return 1;
			}

			if (entry1.IsClientRateHavingSubsidiaryRelations() && entry2.IsClientRateHavingSubsidiaryRelations())
			{
				result = GetPrioritiseGroupClientRate(entry1, entry2);
			}
			else
			{
				result = RateTypePriority(entry1) - RateTypePriority(entry2);
			}

			return result;
		}

		bool IsEntry1FromClientRateAndEntry2FromCompanyTariffLevelOverride(IRateEntry entry1, IRateEntry entry2)
		{
			return entry1.IsClientRate() && entry2.IsCompanyTariff() && entry2.ParentRatingHeader.TH_GlobalRateLevel == criteria.TariffLevel;
		}

		int RateTypePriority(IRateEntry entry)
		{
			if (entry.IsQuote())
			{
				return 3;
			}

			if (entry.IsClientRate() && !entry.IsClientRateHavingSubsidiaryRelations())
			{
				return 2;
			}

			if (entry.IsClientRateHavingSubsidiaryRelations())
			{
				return 1;
			}

			if (entry.IsCompanyTariff())
			{
				return 0;
			}

			throw new NotSupportedException("Can only compare RateType on Quote, Client Rate or Company Tariff");
		}

		bool IsClientSame(IRateLine rateLine1, IRateLine rateLine2)
		{
			var entry1 = rateLine1.ParentRateEntry;
			var entry2 = rateLine2.ParentRateEntry;

			if (entry1.ParentRatingHeader != null && entry2.ParentRatingHeader != null)
			{
				if ((IsQuoteOrClientRate(entry1) && IsQuoteOrClientRate(entry2))
					&& !(entry1.IsClientRateHavingSubsidiaryRelations() || entry2.IsClientRateHavingSubsidiaryRelations()))
				{
					return entry1.ParentRatingHeader.Header != null
						&& entry2.ParentRatingHeader.Header != null
						&& entry1.ParentRatingHeader.Header.PK == entry2.ParentRatingHeader.Header.PK;
				}

				if ((IsQuoteOrClientRate(entry1) && entry2.IsClientRateHavingSubsidiaryRelations())
					|| (IsQuoteOrClientRate(entry2) && entry1.IsClientRateHavingSubsidiaryRelations()))
				{
					return rateLine2.IsApplicableToOrg(entry1.ParentRatingHeader.Header, criteria) || rateLine1.IsApplicableToOrg(entry2.ParentRatingHeader.Header, criteria);
				}

				if (IsQuoteOrClientRate(entry1) && entry2.IsCompanyTariff())
				{
					return rateLine2.IsApplicableToOrg(entry1.ParentRatingHeader.Header, criteria);
				}

				if (IsQuoteOrClientRate(entry2) && entry1.IsCompanyTariff())
				{
					return rateLine1.IsApplicableToOrg(entry2.ParentRatingHeader.Header, criteria);
				}
			}

			return false;
		}

		static bool IsQuoteOrClientRate(IRateEntry entry)
		{
			return entry.IsQuote() || entry.IsClientRate();
		}

		int GetPrioritiseGroupClientRate(IRateEntry entry1, IRateEntry entry2)
		{
			var result = 0;

			if (entry1.ParentRatingHeader != null
				&& entry2.ParentRatingHeader != null
				&& entry1.ParentRatingHeader.PK != entry2.ParentRatingHeader.PK)
			{
				if (RateLineExtensions.IsGroupRateApplicableSubsidiaryOrg(entry2, entry1.ParentRatingHeader.Header, criteria))
				{
					return 1;
				}

				if (RateLineExtensions.IsGroupRateApplicableSubsidiaryOrg(entry1, entry2.ParentRatingHeader.Header, criteria))
				{
					return -1;
				}
			}

			return result;
		}

		protected override string GetName()
		{
			return (NoResString)"Rate Type"; // log message, subject to change, more for support people as of now
		}
	}
}
