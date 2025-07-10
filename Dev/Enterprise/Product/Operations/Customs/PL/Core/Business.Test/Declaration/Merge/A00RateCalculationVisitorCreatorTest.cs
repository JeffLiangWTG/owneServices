using System;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class A00RateCalculationVisitorCreatorTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null BaseCreator", "Value cannot be null.\r\nParameter name: baseCreator",
				() => new A00RateCalculationVisitorCreator(null));
		});
	}

	public void TestNewVisitor()
	{
		var baseVisitorMock = Mock.Of<IRateCalculationVisitorCreator>(
			m => m.NewVisitor(It.IsAny<IUniversalRateCalcData>(), It.IsAny<FormulaErrorListener>()) == Mock.Of<IRateCalculationVisitor>());
		IRateCalculationVisitorCreator creator = new A00RateCalculationVisitorCreator(baseVisitorMock);
		var visitor = creator.NewVisitor(Mock.Of<IUniversalRateCalcData>(), new FormulaErrorListener());
		AssertType<A00RateCalculationDecorator>(visitor);
	}
}
