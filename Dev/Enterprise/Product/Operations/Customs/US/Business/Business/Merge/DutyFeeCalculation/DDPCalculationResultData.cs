using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class DDPCalculationResultData
	{
		public DDPCalculationResultData()
		{
		}

		public DDPCalculationResultData(ZDecimal amount, FeeCalculationInternalData internalData)
		{
			Amount = amount;
			InternalData = internalData;
		}
		public decimal Amount { get; set; }
		public FeeCalculationInternalData InternalData { get; set; }

		public override bool Equals(object target)
		{
			if (target == null)
			{
				return false;
			}

			var targetResultData = target as DDPCalculationResultData;
			if (targetResultData == null)
			{
				return false;
			}

			var sourceInternalData = InternalData;
			if (sourceInternalData == null)
			{
				return false;
			}

			var targetInternalData = targetResultData.InternalData;
			if (targetInternalData == null)
			{
				return false;
			}

			return Amount == targetResultData.Amount && sourceInternalData.NoneCustomsValueAmount == targetResultData.InternalData.NoneCustomsValueAmount && sourceInternalData.PercentOfRate == targetInternalData.PercentOfRate;
		}

		public override int GetHashCode()
		{
			return Amount.GetHashCode() ^ InternalData.GetHashCode();
		}
	}
}
