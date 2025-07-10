using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(DutyCalculatorStrategy))]
	class DutyCalculatorStrategyTest : DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
	{
		protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new DutyCalculatorStrategy((Declaration.JobDeclaration)Declaration);

		protected override ZDecimal ExpectedVATChargeAmountA => 9.3918m;

		protected override ZDecimal ExpectedVATBaseValueA => 42.69m;

		protected override IReadOnlyList<FeeAssertionObject> EntryLineAExpectedFees => new FeeAssertionObject[]
		{
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.VatRateCode, ChargeAmount = 9.3918m, BaseValue = 42.69m, Rate = 22.00m, MethodOfCalculation = "%", OverrideReason = "" },
		};

		protected override ZDecimal ExpectedVATChargeAmountB => 1.0736m;

		protected override ZDecimal ExpectedVATBaseValueB => 4.88m;

		protected override IReadOnlyList<FeeAssertionObject> EntryLineBExpectedFees => new FeeAssertionObject[]
		{
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.VatRateCode, ChargeAmount = 1.0736m, BaseValue = 4.88m, Rate = 22.00m, MethodOfCalculation = "%", OverrideReason = "" },
		};

		protected override ZString NonVATableDeductionChargeCode => EmptyChargeCode;

		protected override ZString VATableAdditionChargeCode => EmptyChargeCode;
	}
}
