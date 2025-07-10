using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class FeeResult
	{
		public static FeeResult Empty
		{
			get
			{
				return new FeeResult(0m, false);
			}
		}

		public FeeResult(ZDecimal amount, decimal percentOfRate = default, decimal noneCustomsValueAmount = default)
			: this(amount, !amount.IsEmpty, percentOfRate)
		{
		}

		public FeeResult(ZDecimal amount, bool isRequired, decimal percentOfRate = default, decimal noneCustomsValueAmount = default)
		{
			Amount = amount;
			IsRequired = isRequired;
			PercentOfRate = percentOfRate;
			NoneCustomsValueAmount = noneCustomsValueAmount;
		}
		public readonly ZDecimal NoneCustomsValueAmount;
		public readonly ZDecimal PercentOfRate;
		public readonly ZDecimal Amount;
		public readonly bool IsRequired;
	}
}
