using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing;

public readonly record struct ExpectedFeeRecord(string ChargeType,
	ZDecimal ChargeAmount,
	ZDecimal BaseValue,
	ZDecimal Rate,
	ZString MethodOfCalculation,
	ZString OverrideReason)
{
	public string ChargeType { get; } = ChargeType;
	public ZDecimal ChargeAmount { get; } = ChargeAmount;
	public ZDecimal BaseValue { get; } = BaseValue;
	public ZDecimal Rate { get; } = Rate;
	public ZString MethodOfCalculation { get; } = MethodOfCalculation;
	public ZString OverrideReason { get; } = OverrideReason;
}
