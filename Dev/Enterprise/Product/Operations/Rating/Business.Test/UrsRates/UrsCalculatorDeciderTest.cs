using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Moq;
using Urs.Api.Integration.Interfaces;
using static Enterprise.Rating.Business.UrsConstants;

namespace Enterprise.Rating.Business.Test;

public class UrsCalculatorDeciderTest : TestCaseWithFactory
{
	public void TestUrsCalculatorDecider_WhenApplicabilityIsNotApplicable_ReturnsFlatCalculator()
	{
		var parent = new Mock<IRateLine>().Object;
		var logger = new Mock<ILogger>().Object;
		var ursRateEntry = new Mock<IUniversalRateEntryDto>();
		ursRateEntry.SetupGet(x => x.Applicable).Returns(RateApplicableCode.NotApplicable);
		ursRateEntry.SetupGet(x => x.Price).Returns(0m);
		ursRateEntry.SetupGet(x => x.BreakType).Returns(RateBreakTypeCode.Flat);
		ursRateEntry.SetupGet(x => x.BreakQuantity).Returns(1.0m);
		ursRateEntry.SetupGet(x => x.PricingQuantityUnit).Returns("None");
		ursRateEntry.SetupGet(x => x.MeasurementPrecision).Returns(1.0m);
		ursRateEntry.SetupGet(x => x.RoundingMode).Returns(0.01m);
		ursRateEntry.SetupGet(x => x.UseVolumetric).Returns(false);
		ursRateEntry.SetupGet(x => x.PricingQuantity).Returns(1.0m);

		var ursRateEntries = new List<IUniversalRateEntryDto> { ursRateEntry.Object };

		var result = UrsCalculatorDecider.GetCalculator(ursRateEntries, parent, logger, "SEA");

		AssertEquals(FlatCalculator.Code, result.Code);
		var expectedRateLineItems = new WiseLineItem(parent, Calculator.Items.Operator.BAS, string.Empty, 0, ZString.Empty, ZDecimal.Zero, 0, false);
		AssertEquals(1, result.RateLineItems.Count());
		var actualRateLineItem = result.RateLineItems.First();
		AssertEquals(expectedRateLineItems.TM_Type, actualRateLineItem.TM_Type);
		AssertEquals(expectedRateLineItems.TM_AC, actualRateLineItem.TM_AC);
		AssertEquals(expectedRateLineItems.TM_RelevantValue, actualRateLineItem.TM_RelevantValue);
	}
}
