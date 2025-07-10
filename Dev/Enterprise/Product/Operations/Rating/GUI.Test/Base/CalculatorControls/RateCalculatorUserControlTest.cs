using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	public class RateCalculatorUserControlTest : TestCaseWithFactory
	{
		public void TestExceptionIsRaisedWhenCalculatorIsNull()
		{
			using (var calculatorControl = new RateCalculatorUserControl())
			{
				AssertExceptionThrown<ArgumentNullException>(() => calculatorControl.BindTo = "Decimal1");
			}
		}

		[GuiTest]
		public void TestExceptionIsRaisedWhenNoMappingForProperty()
		{
			using (var calculatorControl = new UnitControl())
			{
				var costing = Factory.New<Costing>();
				var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.SCO, Core.Constants.RateMode.SEA, "", "");
				var rateLine = rateEntry.RateLines.AddNew();
				rateLine.TL_RateCalculator = HighestRateCalculator.Code;
				calculatorControl.ViewCalculatorForBinding = rateLine.ViewCalculatorForBinding;
				calculatorControl.BindTo = "Binding";
				AssertEquals("There is no mapping for property [Decimal1] in calculator Enterprise.Rating.GUI.UnitControl.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}
	}
}
