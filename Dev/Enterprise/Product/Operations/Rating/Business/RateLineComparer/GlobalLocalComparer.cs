using System.Globalization;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class GlobalLocalComparer : BaseRateLineComparer
	{
		public override int Compare(FastLine line1, FastLine line2)
		{
			var entry1 = line1.ParentRateEntry;
			var entry2 = line2.ParentRateEntry;

			if (entry1.IsIntercompanyTariff() || entry2.IsIntercompanyTariff())
			{
				if (entry1.IsCosting() || entry2.IsCosting())
				{ // IntercompanyTariff takes priority over Global/Local Costing
					return entry2.IsCosting().CompareTo(entry1.IsCosting());
				}

				return 0;
			}

			// WiseCost has lower priority than local rate
			int result = entry2.IsWiseCost().CompareTo(entry1.IsWiseCost());
			if (result == 0)
			{
				// For a global vs local rate, one always overrides the other
				result = entry2.IsGlobal().CompareTo(entry1.IsGlobal());
				if (result != 0)
				{
					if (IsApplicable(entry1) && IsApplicable(entry2) && RatingDataRegistry.Instance.GlobalSellRatesOverrideLocal.Value)
					{
						result = -result;
					}
				}
			}

			return result;
		}

		protected override string GetName()
		{
			return (NoResString)"Local Rates overriding Global Rates"; // log message, subject to change
		}

		protected override string GetReasonCore(FastLine overriddenLine, FastLine overriddenBy)
		{
			if (IsApplicable(overriddenLine) && IsApplicable(overriddenBy) && RatingDataRegistry.Instance.GlobalSellRatesOverrideLocal.Value)
			{
				return string.Format(CultureInfo.InvariantCulture, (NoResString)"Global Sell Rates to overriding Local Sell Rates ({0} Registry enabled)", RatingDataRegistry.Instance.GlobalSellRatesOverrideLocal.Caption); // log message, not useful to translate
			}
			else
			{
				return GetName();
			}
		}

		#region Implementation

		bool IsApplicable(IRateEntry rateEntry) => rateEntry.IsClientRate() || rateEntry.IsCompanyTariff();

		bool IsApplicable(FastLine rateLine) => IsApplicable(rateLine.ParentRateEntry);

		#endregion
	}
}
