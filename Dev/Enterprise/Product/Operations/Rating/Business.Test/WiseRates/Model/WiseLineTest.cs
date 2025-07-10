using CargoWise.Types;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Business.Test
{
	public class WiseLineTest : RatingTestCase
	{
		public void TestRateCalculatorType()
		{
			var charge = new Charge();
			var wiseLine = new WiseLine(Factory, charge);
			AssertEquals(CalculatorType.None, wiseLine.RateCalculatorType);
			AssertEquals(ZString.Empty, wiseLine.TL_RateCalculator);

			wiseLine.RateCalculatorType = CalculatorType.Unit;
			wiseLine.ChildRateLineItems = new WiseLineItem[] {
				new WiseLineItem(wiseLine, Calculator.Items.Operator.UNT, string.Empty, 5m, ZString.Empty, 0m, 0m, null) };
			AssertEquals("setting RateCalculatorType updates TL_RateCalculator", UnitCalculator.Code, wiseLine.TL_RateCalculator);
			AssertType<UnitCalculator>("setting RateCalculatorType resets Calculator", wiseLine.Calculator);

			wiseLine.RateCalculatorType = CalculatorType.Flat;
			wiseLine.ChildRateLineItems = new WiseLineItem[] {
				new WiseLineItem(wiseLine, Calculator.Items.Operator.BAS, string.Empty, 5m, ZString.Empty, 0m, 0m, null) };
			AssertEquals("setting RateCalculatorType updates TL_RateCalculator", FlatCalculator.Code, wiseLine.TL_RateCalculator);
			AssertType<FlatCalculator>("setting RateCalculatorType resets Calculator", wiseLine.Calculator);

			wiseLine.TL_RateCalculator = UnitCalculator.Code;
			wiseLine.ChildRateLineItems = new WiseLineItem[] {
				new WiseLineItem(wiseLine, Calculator.Items.Operator.UNT, string.Empty, 5m, ZString.Empty, 0m, 0m, null) };
			AssertEquals("setting TL_RateCalculator updates RateCalculatorType", CalculatorType.Unit, wiseLine.RateCalculatorType);
			AssertType<UnitCalculator>("setting TL_RateCalculator resets Calculator", wiseLine.Calculator);
		}
	}
}
