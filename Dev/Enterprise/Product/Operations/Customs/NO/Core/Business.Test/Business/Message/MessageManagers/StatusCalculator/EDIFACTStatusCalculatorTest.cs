using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EDIFACTStatusCalculator))]
sealed class EDIFACTStatusCalculatorTest : EDIFACTMessageStatusCalculatorTestCase
{
	public override void TestCalculatedJobStatus()
	{
		Assert("To be implemented", true);
	}

	public override void TestMessageTypeDescription()
	{
		Assert("To be implemented", true);
	}

	protected override EDIFACTMessageStatusCalculator GetCalculator() => new EDIFACTStatusCalculator(ZString.Empty);
}
