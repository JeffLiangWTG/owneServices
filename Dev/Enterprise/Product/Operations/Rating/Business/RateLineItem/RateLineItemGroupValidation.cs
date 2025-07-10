using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// This class performs group validations on RateLineItems where we need to validate each rateLineItem with all other ratelineItems in the group.
	/// It is designed to utilize precalculated values used during each RateLineItem validation. So, we don't calculate value on every rateLineItem validation.
	/// </summary>
	class RateLineItemGroupValidation
	{
		public RateLineItemGroupValidation()
		{
			freightInclusiveCalculatorRateLineItemsCharges = new HashSet<ZGuid>();
			rateLinesQueue = new Queue<RateLine>();
			isProcessed = true;
		}

		readonly HashSet<ZGuid> freightInclusiveCalculatorRateLineItemsCharges;
		readonly Queue<RateLine> rateLinesQueue;
		bool isProcessed;

		public void AddItem(RateLine rateLine)
		{
			if (rateLine?.RateCalculatorType == CalculatorType.FreightInclusive)
			{
				rateLinesQueue.Enqueue(rateLine);
				isProcessed = false;
			}
		}

		public bool HasFRTCalculatorItemsHaveSameChargeCode(RateLine rateLine)
		{
			if (rateLine != null)
			{
				ProcessIfNotAlready();
				return freightInclusiveCalculatorRateLineItemsCharges.Contains(rateLine.TL_AC);
			}

			return false;
		}

		void ProcessIfNotAlready()
		{
			if (!isProcessed)
			{
				while (rateLinesQueue.Count > 0)
				{
					var rateLine = rateLinesQueue.Dequeue();
					foreach (RateLineItem item in rateLine.RateLineItems)
					{
						freightInclusiveCalculatorRateLineItemsCharges.Add(item.TM_AC);
					}
				}

				isProcessed = true;
			}
		}
	}
}
