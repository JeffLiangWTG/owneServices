using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class A00RateCalculationDecoratorTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null BaseVisitor", "Value cannot be null.\r\nParameter name: baseVisitor",
				() => new A00RateCalculationDecorator(null, null));
			AssertExceptionThrown<ArgumentNullException>("Null RateCalcData", "Value cannot be null.\r\nParameter name: rateCalcData",
				() => new A00RateCalculationDecorator(Mock.Of<IRateCalculationVisitor>(), null));
		});
	}

	public void TestVisitFullFormulaExpression_OnlyPercentage()
	{
		var rateCalcDataMock = Mock.Of<IUniversalRateCalcData>();
		var exampleBaseVisitFullFormulaExpressionResult = new RateFormulaResult(0);
		exampleBaseVisitFullFormulaExpressionResult.AddIntermediateResult(300);
		var baseVisitorMock = Mock.Of<IRateCalculationVisitor>(x =>
			x.VisitFullFormulaExpression(It.IsAny<RateFormulaParser.ExpressionContext>()) == exampleBaseVisitFullFormulaExpressionResult);
		var rateCalculator = new A00RateCalculationDecorator(baseVisitorMock, rateCalcDataMock) as IRateCalculationVisitor;
		var result = rateCalculator.VisitFullFormulaExpression(It.IsAny<RateFormulaParser.ExpressionContext>());
		CombineAssertions(() =>
		{
			AssertEquals("Only one row", 1, result.IntermediateResults.Count);
			var intermediateResult = result.IntermediateResults.Single();
			AssertEquals("MethodOfCalculation should be percentage", Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage,
				intermediateResult.MethodOfCalculation);
		});
	}

	public void TestVisitFullFormulaExpression_PercentageAndUnit()
	{
		var rateCalcDataMock = Mock.Of<IUniversalRateCalcData>(x => x.ValueForDuty == 100);
		var exampleBaseVisitFullFormulaExpressionResult = new RateFormulaResult(0);
		exampleBaseVisitFullFormulaExpressionResult.AddIntermediateResult(300);
		exampleBaseVisitFullFormulaExpressionResult.AddIntermediateResult(60M, "DTN");
		var baseVisitorMock = Mock.Of<IRateCalculationVisitor>(x =>
			x.VisitFullFormulaExpression(It.IsAny<RateFormulaParser.ExpressionContext>()) == exampleBaseVisitFullFormulaExpressionResult);
		var rateCalculator = (IRateCalculationVisitor)new A00RateCalculationDecorator(baseVisitorMock, rateCalcDataMock);
		var result = rateCalculator.VisitFullFormulaExpression(It.IsAny<RateFormulaParser.ExpressionContext>());

		AssertEquals("Only one row", 1, result.IntermediateResults.Count);
		CombineAssertions(() =>
		{
			var intermediateResult = result.IntermediateResults.Single();
			AssertEquals("MethodOfCalculation should be empty", ZString.Empty, intermediateResult.MethodOfCalculation);
			AssertEquals("Amount should be summed", 360M, intermediateResult.Amount);
			AssertEquals("Base value same as ValueForDuty", 100M, intermediateResult.BaseValue);
		});
	}

	public void TestVisitFullFormulaExpression_WithMeursingExpression()
	{
		var rateCalcDataMock = Mock.Of<IUniversalRateCalcData>(x => x.ValueForDuty == 100);

		var exampleBaseVisitFullFormulaExpressionResult = new RateFormulaResult(0m);
		exampleBaseVisitFullFormulaExpressionResult.AddIntermediateResult(300);
		exampleBaseVisitFullFormulaExpressionResult.AddIntermediateResult(60M, "DTN");

		var agriculturalIntermediateResult = new DutyCalculationIntermediateResult(100, ZDecimal.Zero, 100, ZString.Empty)
		{
			OriginalMeursingExpression = "EA(1)"
		};
		exampleBaseVisitFullFormulaExpressionResult.AddIntermediateResultsRange(new[] { agriculturalIntermediateResult });

		var baseVisitorMock = Mock.Of<IRateCalculationVisitor>(x =>
			x.VisitFullFormulaExpression(It.IsAny<RateFormulaParser.ExpressionContext>()) == exampleBaseVisitFullFormulaExpressionResult);
		var rateCalculator = (IRateCalculationVisitor)new A00RateCalculationDecorator(baseVisitorMock, rateCalcDataMock);
		var result = rateCalculator.VisitFullFormulaExpression(It.IsAny<RateFormulaParser.ExpressionContext>());

		AssertEquals("Two rows", 2, result.IntermediateResults.Count);
		CombineAssertions(() =>
		{
			var nonMeursingResult = result.IntermediateResults.FirstOrDefault(x => x.OriginalMeursingExpression.IsEmpty);
			AssertEquals("MethodOfCalculation should be empty", ZString.Empty, nonMeursingResult.MethodOfCalculation);
			AssertEquals("Amount should be summed", 360M, nonMeursingResult.Amount);
			AssertEquals("Base value same as ValueForDuty", 100M, nonMeursingResult.BaseValue);

			var agriculturalResult = result.IntermediateResults.FirstOrDefault(x => !x.OriginalMeursingExpression.IsEmpty);
			AssertEquals("MethodOfCalculation should be empty", ZString.Empty, agriculturalResult.MethodOfCalculation);
			AssertEquals("Amount should be summed", 100M, agriculturalResult.Amount);
			AssertEquals("Base value same as ValueForDuty", 100M, agriculturalResult.BaseValue);
			AssertEquals("OriginalMeursingExpression", "EA(1)", agriculturalResult.OriginalMeursingExpression);
		});
	}
}
