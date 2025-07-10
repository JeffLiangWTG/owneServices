using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public static class VOCValueInterfactExtensions
	{
		public static ZDecimal GetAmountDue(this IVOCAfterValues input)
		{
			var result = ZDecimal.Zero;
			if (input != null)
			{
				result = input.CustomsDutyNoS1P2B + input.S1P2BDuty + input.ValueAddedTax + input.ProvisionalPaymentAmount + input.PenaltyAmount;
			}
			return result;
		}

		public static ZDecimal GetAmountDue(this IVOCBeforeValues input)
		{
			var result = ZDecimal.Zero;
			if (input != null)
			{
				result = input.CustomsDutyNoS1P2B + input.S1P2BDuty + input.ValueAddedTax + input.ProvisionalPaymentAmount + input.PenaltyAmount;
			}
			return result;
		}
	}
}
