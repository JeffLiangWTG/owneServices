namespace Enterprise.Rating.Business
{
	public partial class BasePricingPageRateLineListComparer
	{
		sealed class LineSequencePricingPageRateLineListComparer : BasePricingPageRateLineListComparer
		{
			public override int Compare(PricingPageRateLineList l1, PricingPageRateLineList l2)
			{
				return CompareOrders(l1, l2);
			}

			public static int CompareOrders(PricingPageRateLineList l1, PricingPageRateLineList l2)
			{
				var result = CalculateMeanLineOrder(l1).CompareTo(CalculateMeanLineOrder(l2));

				if (result == 0)
				{
					result = CalculateMeanEntryOrder(l1).CompareTo(CalculateMeanEntryOrder(l2));
				}

				return result;
			}

			static decimal CalculateMeanLineOrder(PricingPageRateLineList set)
			{
				if (set.Count == 0)
				{
					return decimal.MaxValue;
				}
				else
				{
					decimal lineOrderSum = 0;

					for (var i = set.Count - 1; i >= 0; i--)
					{
						lineOrderSum += set[i].TL_LineOrder;
					}

					return lineOrderSum / set.Count;
				}
			}

			static decimal CalculateMeanEntryOrder(PricingPageRateLineList set)
			{
				if (set.Count == 0)
				{
					return decimal.MaxValue;
				}
				else
				{
					decimal lineOrderSum = 0;

					for (var i = set.Count - 1; i >= 0; i--)
					{
						lineOrderSum += set[i].Parent.TI_LineOrder;
					}

					return lineOrderSum / set.Count;
				}
			}
		}
	}
}
