using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public interface IVOCAfterValues
	{
		ZDecimal CIFValue { get; }
		ZDecimal CustomsValue { get; }
		ZDecimal CustomsDutyNoS1P2B { get; }
		ZDecimal S1P2BDuty { get; }
		ZDecimal ValueAddedTax { get; }
		ZDecimal ProvisionalPaymentAmount { get; }
		ZDecimal PenaltyAmount { get; }
	}
}
