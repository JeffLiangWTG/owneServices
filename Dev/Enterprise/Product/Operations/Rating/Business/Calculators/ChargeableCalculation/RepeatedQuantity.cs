using System.Collections.Generic;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business
{
	public class RepeatedQuantity
	{
		public RepeatedQuantity(Quantity quantity, decimal repeatCount, int containerCount)
		{
			Quantity = quantity;
			RepeatCount = repeatCount;
			ContainerCount = containerCount;
		}

		/// <summary>
		/// Sum the given quantities and concatenate all references.
		/// Units are assumed to be the same.
		/// </summary>
		/// <param name="defaultUnit">unit to use for the result when the list has no non-zero quantites</param>
		public static Quantity Sum(IList<RepeatedQuantity> quantities, string defaultUnit)
		{
			if (quantities.Count == 1 && quantities[0].RepeatCount == 1)
			{
				return quantities[0].Quantity;
			}

			decimal chargeableAmountSum = 0m;
			var references = new List<string>(0);
			var unit = defaultUnit;
			foreach (var repeatedQuantity in quantities)
			{
				if (repeatedQuantity.Quantity.Amount != 0)
				{
					unit = repeatedQuantity.Quantity.Unit;
					chargeableAmountSum += repeatedQuantity.Quantity.Amount * repeatedQuantity.RepeatCount;
					if (!repeatedQuantity.Quantity.Reference.IsEmpty &&
						(references.Count == 0 || references[references.Count - 1] != repeatedQuantity.Quantity.Reference))
					{
						references.Add(repeatedQuantity.Quantity.Reference);
					}
				}
			}

			if (chargeableAmountSum != 0m)
			{
				return new Quantity(chargeableAmountSum, unit, reference: string.Join(", ", references));
			}
			else
			{
				return new Quantity(0, unit);
			}
		}

		public Quantity Quantity { get; }
		public decimal RepeatCount { get; }
		public int ContainerCount { get; }
	}
}
