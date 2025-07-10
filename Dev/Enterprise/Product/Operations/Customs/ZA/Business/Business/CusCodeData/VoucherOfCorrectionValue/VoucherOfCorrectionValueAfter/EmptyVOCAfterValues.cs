using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	class EmptyVOCAfterValues : IVOCAfterValues
	{
		public ZDecimal CIFValue => ZDecimal.Zero;

		public ZDecimal CustomsDutyNoS1P2B => ZDecimal.Zero;

		public ZDecimal CustomsValue => ZDecimal.Zero;

		public ZDecimal S1P2BDuty => ZDecimal.Zero;

		public ZDecimal ValueAddedTax => ZDecimal.Zero;

		public ZDecimal ProvisionalPaymentAmount => ZDecimal.Zero;

		public ZDecimal PenaltyAmount => ZDecimal.Zero;
	}
}
