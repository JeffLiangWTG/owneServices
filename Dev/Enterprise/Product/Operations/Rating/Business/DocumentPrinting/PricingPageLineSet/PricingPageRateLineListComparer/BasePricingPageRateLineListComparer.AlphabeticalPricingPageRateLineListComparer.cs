using System;

namespace Enterprise.Rating.Business
{
	public partial class BasePricingPageRateLineListComparer
	{
		sealed class AlphabeticalPricingPageRateLineListComparer : BasePricingPageRateLineListComparer
		{
			public override int Compare(PricingPageRateLineList x, PricingPageRateLineList y)
			{
				if (x.Count == 0)
				{
					return y.Count == 0 ? 0 : 1;
				}
				else if (y.Count == 0)
				{
					return -1;
				}
				else
				{
					var result = StringComparer.OrdinalIgnoreCase.Compare
					(
						x[0].GetRateDescOrRateDescLocal(),
						y[0].GetRateDescOrRateDescLocal()
					);

					if (result == 0)
					{
						result = LineSequencePricingPageRateLineListComparer.CompareOrders(x, y);
					}

					return result;
				}
			}
		}
	}
}
