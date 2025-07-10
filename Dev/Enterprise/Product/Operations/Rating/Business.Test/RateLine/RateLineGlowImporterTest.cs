using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.DataTransfer;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Rating.Business.Testing
{
	/// <summary>
	/// For all these calculator tests, we will:
	///		Given the calculator XYZ and valid values for it
	///		When importing the calculator XYZ into a RateLine with string data
	///		Then expect the calculator in the RateLine to be with the imported data present
	/// </summary>
	public class RateLineGlowImporterTest : RatingTestCase
	{
		public void TestImportCalculator_Agency()
		{
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.Agency, AgencyCalculator>(logger,
				(RatingCalculatorColumns.Agency.AgentBaseRate, "10"),
				(RatingCalculatorColumns.Agency.AgentMaximum, "14"),
				(RatingCalculatorColumns.Agency.AgentLineTypeRatePerAdditionalLine, "11"),
				(RatingCalculatorColumns.Agency.AgentLineTypeIncludedLines, "12"),
				(RatingCalculatorColumns.Agency.AgentLineTypeMaximumLines, "13"),
				(RatingCalculatorColumns.Agency.AgentFeeTypeRatePerAdditionalLine, "15.1234"),
				(RatingCalculatorColumns.Agency.AgentFeeTypeIncludedLines, "16"),
				(RatingCalculatorColumns.Agency.BaseRate, "20"),
				(RatingCalculatorColumns.Agency.Maximum, "24"),
				(RatingCalculatorColumns.Agency.LineTypeRatePerAdditionalLine, "21"),
				(RatingCalculatorColumns.Agency.LineTypeIncludedLines, "22"),
				(RatingCalculatorColumns.Agency.LineTypeMaximumLines, "23"),
				(RatingCalculatorColumns.Agency.FeeTypeRatePerAdditionalLine, "25.1234"),
				(RatingCalculatorColumns.Agency.FeeTypeIncludedLines, "26"),
				(RatingCalculatorColumns.Agency.HideFeeAndLineTypeOnQuotation, "1"),
				(RatingCalculatorColumns.Agency.HideTypeAndStyleOnQuotation, "1"),
				(RatingCalculatorColumns.Agency.LineType, "INE"),
				(RatingCalculatorColumns.Agency.FeeType, "SUP"),
				(RatingCalculatorColumns.Agency.MessageType, "EXP"),
				(RatingCalculatorColumns.Agency.MessageStyle, "SAC")
			);

			AssertEquals("AgencyRate should be 20", (ZDecimal)20, calc.AgencyRate);
			AssertEquals("PerAdditionalLine should be 21", (ZDecimal)21, calc.PerAdditionalLine);
			AssertEquals("IncludedLines should be 22", (ZInt)22, calc.IncludedLines);
			AssertEquals("MaximumLines should be 23", (ZInt)23, calc.MaximumLines);
			AssertEquals("Maximum should be 24", (ZDecimal)24, calc.Maximum);
			AssertEquals("AdditionalRate should be 25.1234", (ZDecimal)25.1234, calc.AdditionalRate);
			AssertEquals("IncludedHeaders should be 26", (ZInt)26, calc.IncludedHeaders);

			AssertEquals("AgencyLineType should be INE", "INE", calc.AgencyLineType);
			AssertEquals("AgencyFeeType should be SUP", "SUP", calc.AgencyFeeType);
			AssertEquals("HideFeeLineTypeOnQuote should be true", (ZBool)true, calc.HideFeeLineTypeOnQuote);
			AssertEquals("HideMessageTypeOnQuote should be true", (ZBool)true, calc.HideMessageTypeOnQuote);
			AssertEquals("MessageType should be EXP", "EXP", calc.MessageType);
			AssertEquals("MessageSubType should be SAC", "SAC", calc.MessageSubType);

			calc.Line.SetViewAgentRatesWithoutRefreshBinding(true);

			AssertEquals("AgencyRate should be 10 after switching to agent rate", (ZDecimal)10, calc.AgencyRate);
			AssertEquals("PerAdditionalLine should be 11 after switching to agent rate", (ZDecimal)11, calc.PerAdditionalLine);
			AssertEquals("IncludedLines should be 12 after switching to agent rate", (ZInt)12, calc.IncludedLines);
			AssertEquals("MaximumLines should be 13 after switching to agent rate", (ZInt)13, calc.MaximumLines);
			AssertEquals("Maximum should be 14 after switching to agent rate", (ZDecimal)14, calc.Maximum);
			AssertEquals("AdditionalRate should be 15.1234 after switching to agent rate", (ZDecimal)15.1234, calc.AdditionalRate);
			AssertEquals("IncludedHeaders should be 16 after switching to agent rate", (ZInt)16, calc.IncludedHeaders);

			AssertEquals("AgencyLineType should still be INE after switching to agent rate", "INE", calc.AgencyLineType);
			AssertEquals("AgencyFeeType should still be SUP after switching to agent rate", "SUP", calc.AgencyFeeType);
			AssertEquals("HideFeeLineTypeOnQuote should still be true after switching to agent rate", (ZBool)true, calc.HideFeeLineTypeOnQuote);
			AssertEquals("HideMessageTypeOnQuote should still be true after switching to agent rate", (ZBool)true, calc.HideMessageTypeOnQuote);
			AssertEquals("MessageType should still be EXP after switching to agent rate", "EXP", calc.MessageType);
			AssertEquals("MessageSubType should still be SAC after switching to agent rate", "SAC", calc.MessageSubType);
		}

		public void TestImportCalculator_Cartage()
		{
			var logger = (new Mock<INotifications>()).Object;
			var rateLine = CreateRateLine();
			rateLine.TL_WeightVolume = "CN"; // Needs to be CN so we can set the KG in the calculator

			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.Cartage, CartageCalculator>(logger, rateLine,
				(RatingCalculatorColumns.Cartage.UseInclusiveBreaks, "TRUE"),
				(RatingCalculatorColumns.Cartage.UseCumulativeBreaks, "N"),
				(RatingCalculatorColumns.Cartage.UseHigherBreakLowerRate, "N"),
				(RatingCalculatorColumns.Cartage.Operator, "-"),
				(RatingCalculatorColumns.Cartage.Break, "10"),
				(RatingCalculatorColumns.Cartage.BreakMinimum, "200"),
				(RatingCalculatorColumns.Cartage.Rate, "21"),
				(RatingCalculatorColumns.Cartage.AgentRate, "22"),
				(RatingCalculatorColumns.Cartage.Units, "KG"),
				(RatingCalculatorColumns.Cartage.FlatAmount, "555"),
				(RatingCalculatorColumns.Cartage.DropMode, "PSL")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.Cartage.UseInclusiveBreaks, "1"),
				(RatingCalculatorColumns.Cartage.UseCumulativeBreaks, "False"),
				(RatingCalculatorColumns.Cartage.UseHigherBreakLowerRate, "Y"),
				(RatingCalculatorColumns.Cartage.Operator, "+"),
				(RatingCalculatorColumns.Cartage.Break, "10"),
				(RatingCalculatorColumns.Cartage.BreakMinimum, "300"),
				(RatingCalculatorColumns.Cartage.Rate, "31"),
				(RatingCalculatorColumns.Cartage.AgentRate, "32"),
				(RatingCalculatorColumns.Cartage.FlatAmount, "556"),
				(RatingCalculatorColumns.Cartage.DropMode, "PSL")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.Cartage.Operator, "+"),
				(RatingCalculatorColumns.Cartage.Break, "100"),
				(RatingCalculatorColumns.Cartage.RestrictedReason, "Yawn"),
				(RatingCalculatorColumns.Cartage.IsRestricted, "Y")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.Cartage.Operator, "+"),
				(RatingCalculatorColumns.Cartage.Break, "200"),
				(RatingCalculatorColumns.Cartage.Rate, "200"),
				(RatingCalculatorColumns.Cartage.RestrictedReason, "Yawn2"),
				(RatingCalculatorColumns.Cartage.IsRestricted, "N")
			);
			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.Cartage, CartageCalculator>(logger, calc);

			AssertEquals("The last row will overwrite previous rows", (ZBool)true, calc.UseInclusiveBreaks);
			AssertEquals((ZBool)false, calc.IsAccumulated);
			AssertEquals("The last row will overwrite previous rows", (ZBool)true, calc.UseHigherChargeableLowerRateRule);
			AssertEquals("PSL", calc.EquipmentType);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("-", "+")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "-",
					Break = 10,
					BreakMinimum = 200,
					Rate = 21,
					AgentRate = 22,
					Units = "KG",
					FlatAmount = 555,
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 10,
					BreakMinimum = 300,
					Rate = 31,
					AgentRate = 32,
					FlatAmount = 556
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 100,
					RestrictedReason = "Yawn",
					IsRestricted = true
				},
				new BreakForTest()
				{
					Operator = "+",
					Rate = 200,
					Break = 200,
					RestrictedReason = "Yawn2",
				},
			};

			AssertContainsExactElementsInAnyOrder("BreakForTest results should match the expected collection", expected, results);
		}

		public void TestImportCalculator_CartageZoneDistance_WithTransportZones()
		{
			var logger = (new Mock<INotifications>()).Object;

			var zones = Helper.CreateRateTransportZoneSet(null, "AU", zoneNames: new ZString[] { "zone0", "zone1", "zone2" });
			Factory.Save();

			var rateLine = CreateRateLine(rateCategory: Category.ORG);
			rateLine.TL_WeightVolume = "CN"; // Needs to be CN so we can set the KG in the calculator

			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.CartageZoneDistance, CartageZoneDistanceCalculator>(logger, rateLine,
				(RatingCalculatorColumns.CartageZoneDistance.UseInclusiveBreaks, "TRUE"),
				(RatingCalculatorColumns.CartageZoneDistance.UseCumulativeBreaks, "N"),
				(RatingCalculatorColumns.CartageZoneDistance.UseHigherBreakLowerRate, "1"),
				(RatingCalculatorColumns.CartageZoneDistance.Operator, "-"),
				(RatingCalculatorColumns.CartageZoneDistance.Break, "10"),
				(RatingCalculatorColumns.CartageZoneDistance.BreakMinimum, "200"),
				(RatingCalculatorColumns.CartageZoneDistance.Rate, "21"),
				(RatingCalculatorColumns.CartageZoneDistance.AgentRate, "22"),
				(RatingCalculatorColumns.CartageZoneDistance.Units, "KG"),
				(RatingCalculatorColumns.CartageZoneDistance.FlatAmount, "555"),
				(RatingCalculatorColumns.CartageZoneDistance.DropMode, "PSL"),
				(RatingCalculatorColumns.CartageZoneDistance.AciZone, string.Empty),
				(RatingCalculatorColumns.CartageZoneDistance.UseAciZones, "false"),
				(RatingCalculatorColumns.CartageZoneDistance.DomesticZone__TZ_ZoneName, "zone0")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CartageZoneDistance.Operator, "+"),
				(RatingCalculatorColumns.CartageZoneDistance.Break, "10"),
				(RatingCalculatorColumns.CartageZoneDistance.RestrictedReason, "Help"),
				(RatingCalculatorColumns.CartageZoneDistance.IsRestricted, "Y"),
				(RatingCalculatorColumns.CartageZoneDistance.DomesticZone__TZ_ZoneName, "zone0")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CartageZoneDistance.Operator, "-"),
				(RatingCalculatorColumns.CartageZoneDistance.Break, "10"),
				(RatingCalculatorColumns.CartageZoneDistance.Rate, "10"),
				(RatingCalculatorColumns.CartageZoneDistance.DomesticZone__TZ_ZoneName, "zone1")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CartageZoneDistance.Operator, "+"),
				(RatingCalculatorColumns.CartageZoneDistance.Break, "10"),
				(RatingCalculatorColumns.CartageZoneDistance.Rate, "100"),
				(RatingCalculatorColumns.CartageZoneDistance.DomesticZone__TZ_ZoneName, "zone1")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CartageZoneDistance.Operator, "-"),
				(RatingCalculatorColumns.CartageZoneDistance.Break, "5"),
				(RatingCalculatorColumns.CartageZoneDistance.Rate, "10"),
				(RatingCalculatorColumns.CartageZoneDistance.DomesticZone__TZ_ZoneName, string.Empty) // General zone
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CartageZoneDistance.Operator, "+"),
				(RatingCalculatorColumns.CartageZoneDistance.Break, "5"),
				(RatingCalculatorColumns.CartageZoneDistance.Rate, "100"),
				(RatingCalculatorColumns.CartageZoneDistance.DomesticZone__TZ_ZoneName, string.Empty) // General zone
			);

			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.CartageZoneDistance, CartageZoneDistanceCalculator>(logger, calc);

			AssertEquals("calc.UseInclusiveBreaks should be true", (ZBool)true, calc.UseInclusiveBreaks);
			AssertEquals("calc.IsAccumulated should be false", (ZBool)false, calc.IsAccumulated);
			AssertEquals("calc.UseHigherChargeableLowerRateRule should be true", (ZBool)true, calc.UseHigherChargeableLowerRateRule);
			AssertEquals("calc.EquipmentType should be 'PSL'", "PSL", calc.EquipmentType);
			AssertEquals("calc.UseACIZones should be false", (ZBool)false, calc.UseACIZones);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("-", "+")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "-",
					Break = 10,
					BreakMinimum = 200,
					Rate = 21,
					AgentRate = 22,
					Units = "KG",
					FlatAmount = 555,
					TransportZone = zones.Zones[0].PK
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 10,
					RestrictedReason = "Help",
					IsRestricted = true,
					TransportZone = zones.Zones[0].PK
				},
				new BreakForTest()
				{
					Operator = "-",
					Break = 10,
					Rate = 10,
					TransportZone = zones.Zones[1].PK
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 10,
					Rate = 100,
					TransportZone = zones.Zones[1].PK
				},
				new BreakForTest()
				{
					Operator = "-",
					Break = 5,
					Rate = 10,
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 5,
					Rate = 100,
				}
			};

			AssertContainsExactElementsInAnyOrder("The RateLineItems should match the expected values", expected, results);
		}

		public void TestImportCalculator_CartageZoneDistance_WithTransportZonesNotFound()
		{
			var notifications = new List<INotification>();
			var loggerMock = new Mock<INotifications>(MockBehavior.Strict);
			loggerMock
				.Setup(x => x.Add(It.IsAny<INotification>()))
				.Callback<INotification>((x) => notifications.Add(x));
			var logger = loggerMock.Object;

			var zones = Helper.CreateRateTransportZoneSet(null, "AU", zoneNames: new ZString[] { "zone0", "zone1", "zone2" });
			Factory.Save();

			var rateLine = CreateRateLine();
			rateLine.TL_WeightVolume = "CN"; // Needs to be CN so we can set the KG in the calculator

			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.CartageZoneDistance, CartageZoneDistanceCalculator>(logger, rateLine,
				expectedSuccess: false,
				(RatingCalculatorColumns.CartageZoneDistance.Operator, "-"),
				(RatingCalculatorColumns.CartageZoneDistance.Break, "10"),
				(RatingCalculatorColumns.CartageZoneDistance.Rate, "21"),
				(RatingCalculatorColumns.CartageZoneDistance.Units, "KG"),
				(RatingCalculatorColumns.CartageZoneDistance.DropMode, "PSL"),
				(RatingCalculatorColumns.CartageZoneDistance.DomesticZone__TZ_ZoneName, "NonExistentZone")
			);

			var actual = notifications
				.Select(n => $"{n.Type.EnumValueName}|{n.Message}")
				.ToArray();

			var expected = new[]
			{
				$"{NotificationType.Error.EnumValueName}|Transport Zone with name 'NonExistentZone' not found.",
				$"{NotificationType.Error.EnumValueName}|RateLineItem importing has failed. The related incomplete RateLine has been deleted as a result"
			};

			AssertContainsExactElementsInAnyOrder(
				"The notifications should match the expected error messages",
				expected,
				actual
			);

			loggerMock.VerifyAll();
		}

		public void TestImportCalculator_CartageZoneDistance_WithACIZone()
		{
			CreateZone("2000", "Z1", "AUSYD", "SYDNEY");
			CreateZone("2100", "Z2", "AUSYD", "SYDNEY");
			Factory.Save();
			var logger = (new Mock<INotifications>()).Object;

			var rateLine = CreateRateLine();

			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.CartageZoneDistance, CartageZoneDistanceCalculator>(logger, rateLine,
				(RatingCalculatorColumns.CartageZoneDistance.Operator, "-"),
				(RatingCalculatorColumns.CartageZoneDistance.Break, "10"),
				(RatingCalculatorColumns.CartageZoneDistance.Rate, "10"),
				(RatingCalculatorColumns.CartageZoneDistance.AciZone, "Z1"),
				(RatingCalculatorColumns.CartageZoneDistance.UseAciZones, "Y")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CartageZoneDistance.Operator, "+"),
				(RatingCalculatorColumns.CartageZoneDistance.Break, "10"),
				(RatingCalculatorColumns.CartageZoneDistance.Rate, "100"),
				(RatingCalculatorColumns.CartageZoneDistance.AciZone, "Z1"),
				(RatingCalculatorColumns.CartageZoneDistance.UseAciZones, "Y")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CartageZoneDistance.Operator, "-"),
				(RatingCalculatorColumns.CartageZoneDistance.Break, "10"),
				(RatingCalculatorColumns.CartageZoneDistance.Rate, "10"),
				(RatingCalculatorColumns.CartageZoneDistance.AciZone, "Z2"),
				(RatingCalculatorColumns.CartageZoneDistance.UseAciZones, "Y")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CartageZoneDistance.Operator, "+"),
				(RatingCalculatorColumns.CartageZoneDistance.Break, "10"),
				(RatingCalculatorColumns.CartageZoneDistance.Rate, "100"),
				(RatingCalculatorColumns.CartageZoneDistance.AciZone, "Z2"),
				(RatingCalculatorColumns.CartageZoneDistance.UseAciZones, "Y")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CartageZoneDistance.Operator, "-"),
				(RatingCalculatorColumns.CartageZoneDistance.Break, "20"),
				(RatingCalculatorColumns.CartageZoneDistance.Rate, "20"),
				(RatingCalculatorColumns.CartageZoneDistance.AciZone, string.Empty), // general zone
				(RatingCalculatorColumns.CartageZoneDistance.UseAciZones, "Y")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CartageZoneDistance.Operator, "+"),
				(RatingCalculatorColumns.CartageZoneDistance.Break, "20"),
				(RatingCalculatorColumns.CartageZoneDistance.Rate, "22"),
				(RatingCalculatorColumns.CartageZoneDistance.AciZone, string.Empty), // general zone
				(RatingCalculatorColumns.CartageZoneDistance.UseAciZones, "Y")
			);

			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.CartageZoneDistance, CartageZoneDistanceCalculator>(logger, calc);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("-", "+")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "-",
					Break = 10,
					Rate = 10,
					AciZone = "Z1"
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 10,
					Rate = 100,
					AciZone = "Z1"
				},
				new BreakForTest()
				{
					Operator = "-",
					Break = 10,
					Rate = 10,
					AciZone = "Z2"
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 10,
					Rate = 100,
					AciZone = "Z2"
				},
				new BreakForTest()
				{
					Operator = "-",
					Break = 20,
					Rate = 20,
					AciZone = ""
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 20,
					Rate = 22,
					AciZone = ""
				}
			};

			AssertContainsExactElementsInAnyOrder(
				"Results should match the expected cartage zone distances with correct operators, breaks, rates, and ACI zones.",
				expected,
				results
			);

			AssertEquals("UseACIZones should be true.", (ZBool)true, calc.UseACIZones);
		}

		public void TestImportCalculator_BlankLines()
		{
			var logger = (new Mock<INotifications>()).Object;

			// Importing a line with defined but empty string values does nothing and causes no error
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.CostBased, CompanyTariffOrCostBasedCalculator>(logger,
				(RatingCalculatorColumns.CostBased.Operator, ""),
				(RatingCalculatorColumns.CostBased.Break, ""),
				(RatingCalculatorColumns.CostBased.PerUnitChange, ""),
				(RatingCalculatorColumns.CostBased.AgentPerUnitChange, ""),
				(RatingCalculatorColumns.CostBased.IsRestricted, "")
			);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("-", "+")
					.Select(i => new BreakForTest(i));

			AssertEquals("The results collection should be empty when importing rows with empty string values.", 0, results.Count());
		}

		public void TestImportCalculator_NoLines()
		{
			var logger = (new Mock<INotifications>()).Object;

			// importing a blank line does nothing and causes no error.
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.CostBased, CompanyTariffOrCostBasedCalculator>(logger, expectedSuccess: true);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("-", "+")
					.Select(i => new BreakForTest(i));

			AssertEquals("Importing a blank line should result in an empty collection.", 0, results.Count());
		}

		public void TestImportCalculator_CostBased_WithBreaks()
		{
			var logger = (new Mock<INotifications>()).Object;

			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.CostBased, CompanyTariffOrCostBasedCalculator>(logger,
				(RatingCalculatorColumns.CostBased.AgentPercentageChange, "1"),
				(RatingCalculatorColumns.CostBased.AgentBasePriceChange, "2"),
				(RatingCalculatorColumns.CostBased.AgentUnitPercentageChange, "3"),
				(RatingCalculatorColumns.CostBased.AgentUnitPriceChange, "4"),
				(RatingCalculatorColumns.CostBased.AgentMinimumChange, "5"),
				(RatingCalculatorColumns.CostBased.PercentageChange, "6"),
				(RatingCalculatorColumns.CostBased.BasePriceChange, "7"),
				(RatingCalculatorColumns.CostBased.UnitPercentageChange, "8"),
				(RatingCalculatorColumns.CostBased.UnitPriceChange, "9"),
				(RatingCalculatorColumns.CostBased.MinimumChange, "10"),
				(RatingCalculatorColumns.CostBased.CalculationOrder, "PER"),
				(RatingCalculatorColumns.CostBased.Operator, "-"),
				(RatingCalculatorColumns.CostBased.Break, "11"),
				(RatingCalculatorColumns.CostBased.PerUnitChange, "12"),
				(RatingCalculatorColumns.CostBased.AgentPerUnitChange, "13")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CostBased.Operator, ""),
				(RatingCalculatorColumns.CostBased.Break, ""),
				(RatingCalculatorColumns.CostBased.PerUnitChange, ""),
				(RatingCalculatorColumns.CostBased.AgentPerUnitChange, ""),
				(RatingCalculatorColumns.CostBased.IsRestricted, "")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CostBased.Operator, "+"),
				(RatingCalculatorColumns.CostBased.Break, "11"),
				(RatingCalculatorColumns.CostBased.PerUnitChange, "22"),
				(RatingCalculatorColumns.CostBased.AgentPerUnitChange, "23")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CostBased.Operator, "+"),
				(RatingCalculatorColumns.CostBased.Break, "20"),
				(RatingCalculatorColumns.CostBased.PerUnitChange, "0"),
				(RatingCalculatorColumns.CostBased.AgentPerUnitChange, "0"),
				(RatingCalculatorColumns.CostBased.RestrictedReason, "Panettone"),
				(RatingCalculatorColumns.CostBased.IsRestricted, "Y")
			);

			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.CostBased, CompanyTariffOrCostBasedCalculator>(logger, calc);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("-", "+")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "-",
					Break = 11,
					Rate = 12,
					AgentRate = 13
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 11,
					Rate = 22,
					AgentRate = 23
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 20,
					RestrictedReason = "Panettone",
					IsRestricted = true
				}
			};

			AssertContainsExactElementsInAnyOrder(
				"The breaks processed by the calculator should match the expected output.",
				expected,
				results
			);

			AssertEquals("Percent should be readonly with breaks applied.", (ZDecimal)0, calc.Percent);
			AssertEquals("BaseRate should reflect the first calculated base rate.", (ZDecimal)7, calc.BaseRate);
			AssertEquals("PerUnitPercent should match the calculated percentage.", (ZDecimal)8, calc.PerUnitPercent);
			AssertEquals("PerUnit should be readonly due to breaks.", (ZDecimal)0, calc.PerUnit);
			AssertEquals("Minimum should reflect the calculated minimum rate.", (ZDecimal)10, calc.Minimum);

			calc.Line.SetViewAgentRatesWithoutRefreshBinding(true);
			AssertEquals("Percent should remain readonly with breaks applied.", (ZDecimal)0, calc.Percent);
			AssertEquals("BaseRate should now reflect the agent's base rate.", (ZDecimal)2, calc.BaseRate);
			AssertEquals("PerUnitPercent should reflect the agent's percentage.", (ZDecimal)3, calc.PerUnitPercent);
			AssertEquals("PerUnit should remain readonly with breaks even after setting agent rates.", (ZDecimal)0, calc.PerUnit);
			AssertEquals("Minimum should match the agent's calculated minimum.", (ZDecimal)5, calc.Minimum);
		}

		public void TestImportCalculator_CostBased_WithBreaksAndReason()
		{
			var logger = (new Mock<INotifications>()).Object;

			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.CostBased, CompanyTariffOrCostBasedCalculator>(logger,
				(RatingCalculatorColumns.CostBased.PercentageChange, "20"),
				(RatingCalculatorColumns.CostBased.Operator, "+"),
				(RatingCalculatorColumns.CostBased.Break, "10"),
				(RatingCalculatorColumns.CostBased.PerUnitChange, "10"),
				(RatingCalculatorColumns.CostBased.RestrictedReason, "Carrot"),
				(RatingCalculatorColumns.CostBased.IsRestricted, "Y")
			);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("-", "+")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				// The first restricted reason should not be shown because the code makes the
				// CallForPricing check box readonly and unchecked. However, this is currently
				// not working and will be fixed in the future. (bug not in importing but rather
				// maybe some binding thing)
				new BreakForTest()
				{
					Operator = "+",
					Break = 10,
					Rate = 10,
					RestrictedReason = "Carrot"
				},
			};

			AssertContainsExactElementsInAnyOrder("The breaks and reasons should match the expected collection", expected, results);
		}

		public void TestImportCalculator_CostBased_NoBreaks()
		{
			var logger = (new Mock<INotifications>()).Object;

			var costing = Helper.NewCosting(null);
			var costingRateEntry = costing.AddRateEntry(Category.FCL, RateMode.SEA, "AU", removeLines: true);
			var costingRateLine = costingRateEntry.AddRateLine("CAF", CartageCalculator.Code, QuantityUnit.CN);
			costingRateLine.GetCalculator<CartageCalculator>().PerUnit = 10m;

			Factory.Save();

			var line = CreateRateLine("CAF");

			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.CostBased, CompanyTariffOrCostBasedCalculator>
			(
				logger,
				line,
				(RatingCalculatorColumns.CostBased.AgentPercentageChange, "1"),
				(RatingCalculatorColumns.CostBased.AgentBasePriceChange, "2"),
				(RatingCalculatorColumns.CostBased.AgentUnitPercentageChange, "3"),
				(RatingCalculatorColumns.CostBased.AgentUnitPriceChange, "4"),
				(RatingCalculatorColumns.CostBased.AgentMinimumChange, "5"),
				(RatingCalculatorColumns.CostBased.PercentageChange, "6"),
				(RatingCalculatorColumns.CostBased.BasePriceChange, "7"),
				(RatingCalculatorColumns.CostBased.UnitPercentageChange, "8"),
				(RatingCalculatorColumns.CostBased.UnitPriceChange, "9"),
				(RatingCalculatorColumns.CostBased.MinimumChange, "10"),
				(RatingCalculatorColumns.CostBased.Equipment, "ANY"),
				(RatingCalculatorColumns.CostBased.CalculationOrder, "PER")
			);

			var rateLineItems = calc
				.RateLineBizO
				.RateLineItems
				.Cast<RateLineItem>()
				.WithType("-", "+");

			AssertEquals("Rate line items should be empty.", 0, rateLineItems.Count());

			AssertEquals("Percentage change should match the expected value.", (ZDecimal)6, calc.Percent);
			AssertEquals("Base rate should match the expected value.", (ZDecimal)7, calc.BaseRate);
			AssertEquals("Per unit percentage should match the expected value.", (ZDecimal)8, calc.PerUnitPercent);
			AssertEquals("Per unit value should match the expected value.", (ZDecimal)9, calc.PerUnit);
			AssertEquals("Minimum value should match the expected value.", (ZDecimal)10, calc.Minimum);

			calc.Line.SetViewAgentRatesWithoutRefreshBinding(true);

			AssertEquals("Agent-related percentage change should match the expected value.", (ZDecimal)1, calc.Percent);
			AssertEquals("Agent-related base rate should match the expected value.", (ZDecimal)2, calc.BaseRate);
			AssertEquals("Agent-related per unit percentage should match the expected value.", (ZDecimal)3, calc.PerUnitPercent);
			AssertEquals("Agent-related per unit value should match the expected value.", (ZDecimal)4, calc.PerUnit);
			AssertEquals("Agent-related minimum value should match the expected value.", (ZDecimal)5, calc.Minimum);

			AssertEquals("Equipment type should match the expected value.", "ANY", calc.EquipmentType);
		}

		public void TestImportCalculator_CompanyTariffBased_NoBreaks()
		{
			var logger = (new Mock<INotifications>()).Object;

			var companyTariff = Helper.NewCompanyTariff();
			var companyTariffRateEntry = companyTariff.AddRateEntry(Category.FCL, RateMode.SEA, "AU", removeLines: true);
			var companyTariffRateLine = companyTariffRateEntry.AddRateLine("CAF", CartageCalculator.Code, QuantityUnit.CN);
			companyTariffRateLine.GetCalculator<CartageCalculator>().PerUnit = 10m;
			companyTariff.Factory.Save();

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var clientRateEntry = clientRate.AddRateEntry(Category.FCL, RateMode.SEA, "AU");
			var clientRateLine = clientRateEntry.AddRateLine("CAF", "_", "KG");

			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.CompanyTariffBased, CompanyTariffOrCostBasedCalculator>
			(
				logger,
				clientRateLine,
				(RatingCalculatorColumns.CompanyTariffBased.AgentPercentageChange, "1"),
				(RatingCalculatorColumns.CompanyTariffBased.AgentBasePriceChange, "2"),
				(RatingCalculatorColumns.CompanyTariffBased.AgentUnitPercentageChange, "3"),
				(RatingCalculatorColumns.CompanyTariffBased.AgentUnitPriceChange, "4"),
				(RatingCalculatorColumns.CompanyTariffBased.AgentMinimumChange, "5"),
				(RatingCalculatorColumns.CompanyTariffBased.PercentageChange, "6"),
				(RatingCalculatorColumns.CompanyTariffBased.BasePriceChange, "7"),
				(RatingCalculatorColumns.CompanyTariffBased.UnitPercentageChange, "8"),
				(RatingCalculatorColumns.CompanyTariffBased.UnitPriceChange, "9"),
				(RatingCalculatorColumns.CompanyTariffBased.MinimumChange, "10"),
				(RatingCalculatorColumns.CompanyTariffBased.Equipment, "ANY"),
				(RatingCalculatorColumns.CompanyTariffBased.CalculationOrder, "PER")
			);

			var rateLineItems = calc
				.RateLineBizO
				.RateLineItems
				.Cast<RateLineItem>()
				.WithType("-", "+");
			AssertEquals("Rate line items should be empty", 0, rateLineItems.Count());

			AssertEquals("Percentage change should be 6", (ZDecimal)6, calc.Percent);
			AssertEquals("Base rate should be 7", (ZDecimal)7, calc.BaseRate);
			AssertEquals("Per unit percentage should be 8", (ZDecimal)8, calc.PerUnitPercent);
			AssertEquals("Per unit value should be 9", (ZDecimal)9, calc.PerUnit);
			AssertEquals("Minimum value should be 10", (ZDecimal)10, calc.Minimum);

			calc.Line.SetViewAgentRatesWithoutRefreshBinding(true);
			AssertEquals("Percentage change should be 1", (ZDecimal)1, calc.Percent);
			AssertEquals("Base rate should be 2", (ZDecimal)2, calc.BaseRate);
			AssertEquals("Per unit percentage should be 3", (ZDecimal)3, calc.PerUnitPercent);
			AssertEquals("Per unit value should be 4", (ZDecimal)4, calc.PerUnit);
			AssertEquals("Minimum value should be 5", (ZDecimal)5, calc.Minimum);

			AssertEquals("Equipment type should be 'ANY'", "ANY", calc.EquipmentType);
		}

		public void TestImportCalculator_CompanyTariffBased_WithBreaks()
		{
			var logger = (new Mock<INotifications>()).Object;

			var companyTariff = Helper.NewCompanyTariff();
			var companyTariffRateEntry = companyTariff.AddRateEntry(Category.FCL, RateMode.SEA, "AU", removeLines: true);
			var companyTariffRateLine = companyTariffRateEntry.AddRateLine("CAF", CartageCalculator.Code, QuantityUnit.CN);
			companyTariffRateLine.GetCalculator<CartageCalculator>().PerUnit = 10m;
			companyTariff.Factory.Save();

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var clientRateEntry = clientRate.AddRateEntry(Category.FCL, RateMode.SEA, "AU");
			var clientRateLine = clientRateEntry.AddRateLine("CAF", "_", "KG");

			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.CompanyTariffBased, CompanyTariffOrCostBasedCalculator>
			(
				logger,
				clientRateLine,
				(RatingCalculatorColumns.CompanyTariffBased.AgentPercentageChange, "1"),
				(RatingCalculatorColumns.CompanyTariffBased.AgentBasePriceChange, "2"),
				(RatingCalculatorColumns.CompanyTariffBased.AgentUnitPercentageChange, "3"),
				(RatingCalculatorColumns.CompanyTariffBased.AgentUnitPriceChange, "4"),
				(RatingCalculatorColumns.CompanyTariffBased.AgentMinimumChange, "5"),
				(RatingCalculatorColumns.CompanyTariffBased.PercentageChange, "6"),
				(RatingCalculatorColumns.CompanyTariffBased.BasePriceChange, "7"),
				(RatingCalculatorColumns.CompanyTariffBased.UnitPercentageChange, "8"),
				(RatingCalculatorColumns.CompanyTariffBased.UnitPriceChange, "9"),
				(RatingCalculatorColumns.CompanyTariffBased.MinimumChange, "10"),
				(RatingCalculatorColumns.CompanyTariffBased.CalculationOrder, "PER"),
				(RatingCalculatorColumns.CompanyTariffBased.Operator, "-"),
				(RatingCalculatorColumns.CompanyTariffBased.Break, "11"),
				(RatingCalculatorColumns.CompanyTariffBased.PerUnitChange, "12"),
				(RatingCalculatorColumns.CompanyTariffBased.AgentPerUnitChange, "13"),
				(RatingCalculatorColumns.CompanyTariffBased.Equipment, "ANY"),
				(RatingCalculatorColumns.CompanyTariffBased.RestrictedReason, string.Empty),
				(RatingCalculatorColumns.CompanyTariffBased.IsRestricted, "N")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CompanyTariffBased.Operator, "+"),
				(RatingCalculatorColumns.CompanyTariffBased.Break, "11"),
				(RatingCalculatorColumns.CompanyTariffBased.PerUnitChange, "22"),
				(RatingCalculatorColumns.CompanyTariffBased.AgentPerUnitChange, "23"),
				(RatingCalculatorColumns.CompanyTariffBased.RestrictedReason, string.Empty)
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CompanyTariffBased.Operator, "+"),
				(RatingCalculatorColumns.CompanyTariffBased.Break, "20"),
				(RatingCalculatorColumns.CompanyTariffBased.PerUnitChange, "0"),
				(RatingCalculatorColumns.CompanyTariffBased.AgentPerUnitChange, "0"),
				(RatingCalculatorColumns.CompanyTariffBased.RestrictedReason, "Panettone"),
				(RatingCalculatorColumns.CompanyTariffBased.IsRestricted, "Y")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CompanyTariffBased.Operator, "+"),
				(RatingCalculatorColumns.CompanyTariffBased.Break, "22"),
				(RatingCalculatorColumns.CompanyTariffBased.PerUnitChange, "22"),
				(RatingCalculatorColumns.CompanyTariffBased.RestrictedReason, "Panettone2"),
				(RatingCalculatorColumns.CompanyTariffBased.IsRestricted, "N")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CompanyTariffBased.Operator, "+"),
				(RatingCalculatorColumns.CompanyTariffBased.Break, "23"),
				(RatingCalculatorColumns.CompanyTariffBased.PerUnitChange, "23"),
				(RatingCalculatorColumns.CompanyTariffBased.RestrictedReason, string.Empty),
				(RatingCalculatorColumns.CompanyTariffBased.IsRestricted, "Y")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CompanyTariffBased.Operator, "+"),
				(RatingCalculatorColumns.CompanyTariffBased.Break, "24"),
				(RatingCalculatorColumns.CompanyTariffBased.PerUnitChange, "24"),
				(RatingCalculatorColumns.CompanyTariffBased.IsRestricted, "Y")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CompanyTariffBased.Operator, "+"),
				(RatingCalculatorColumns.CompanyTariffBased.Break, "25"),
				(RatingCalculatorColumns.CompanyTariffBased.PerUnitChange, "25"),
				(RatingCalculatorColumns.CompanyTariffBased.IsRestricted, "N")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.CompanyTariffBased.Operator, "+"),
				(RatingCalculatorColumns.CompanyTariffBased.Break, "26"),
				(RatingCalculatorColumns.CompanyTariffBased.PerUnitChange, "26")
			);

			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.CompanyTariffBased, CompanyTariffOrCostBasedCalculator>(logger, calc);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("-", "+")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "-",
					Break = 11,
					Rate = 12,
					AgentRate = 13
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 11,
					Rate = 22,
					AgentRate = 23
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 20,
					RestrictedReason = "Panettone",
					IsRestricted = true
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 22,
					Rate = 22,
					RestrictedReason = "Panettone2",
					IsRestricted = false
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 23,
					RestrictedReason = string.Empty,
					IsRestricted = true
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 24,
					RestrictedReason = string.Empty,
					IsRestricted = true
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 25,
					Rate = 25,
					IsRestricted = false
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 26,
					Rate = 26,
					IsRestricted = false
				}
			};

			AssertContainsExactElementsInAnyOrder("The results should match the expected breaks", expected, results);

			AssertEquals("The equipment type should match the expected value", "ANY", calc.EquipmentType);

			AssertEquals("Percent should become 0 due to readonly state with breaks", (ZDecimal)0, calc.Percent);
			AssertEquals("BaseRate should match the expected value", (ZDecimal)7, calc.BaseRate);
			AssertEquals("PerUnitPercent should match the expected value", (ZDecimal)8, calc.PerUnitPercent);
			AssertEquals("PerUnit should become 0 due to readonly state with breaks", (ZDecimal)0, calc.PerUnit);
			AssertEquals("Minimum should match the expected value", (ZDecimal)10, calc.Minimum);

			calc.Line.SetViewAgentRatesWithoutRefreshBinding(true);

			AssertEquals("Percent should remain 0 due to readonly state with breaks", (ZDecimal)0, calc.Percent);
			AssertEquals("BaseRate should match the expected value after viewing agent rates", (ZDecimal)2, calc.BaseRate);
			AssertEquals("PerUnitPercent should match the expected value after viewing agent rates", (ZDecimal)3, calc.PerUnitPercent);
			AssertEquals("PerUnit should remain 0 due to readonly state with breaks", (ZDecimal)0, calc.PerUnit);
			AssertEquals("Minimum should match the expected value after viewing agent rates", (ZDecimal)5, calc.Minimum);
		}

		public void TestImportCalculator_Combined_WithBreaks()
		{
			var logger = (new Mock<INotifications>()).Object;

			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.Combined, CombinedCalculator>(logger,
				(RatingCalculatorColumns.Combined.UseInclusiveBreaks, "TRUE"),
				(RatingCalculatorColumns.Combined.UseCumulativeBreaks, "Y"),
				(RatingCalculatorColumns.Combined.UseHigherBreakLowerRate, "1"),
				(RatingCalculatorColumns.Combined.Operator, "-"),
				(RatingCalculatorColumns.Combined.Break, "10"),
				(RatingCalculatorColumns.Combined.Rate, "21"),
				(RatingCalculatorColumns.Combined.AgentRate, "22"),
				(RatingCalculatorColumns.Combined.FlatAmount, "111")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.Combined.Operator, "+"),
				(RatingCalculatorColumns.Combined.Break, "10"),
				(RatingCalculatorColumns.Combined.IsRestricted, "Y"),
				(RatingCalculatorColumns.Combined.RestrictedReason, "Batman")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.Combined.Operator, "+"),
				(RatingCalculatorColumns.Combined.Break, "100"),
				(RatingCalculatorColumns.Combined.Rate, "210"),
				(RatingCalculatorColumns.Combined.AgentRate, "220"),
				(RatingCalculatorColumns.Combined.FlatAmount, "222")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.Combined.Operator, "+"),
				(RatingCalculatorColumns.Combined.Break, "1000"),
				(RatingCalculatorColumns.Combined.IsRestricted, "Y"),
				(RatingCalculatorColumns.Combined.RestrictedReason, "Help")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.Combined.Operator, "MIN"),
				(RatingCalculatorColumns.Combined.Rate, "300"),
				(RatingCalculatorColumns.Combined.AgentRate, "301")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.Combined.Operator, "MAX"),
				(RatingCalculatorColumns.Combined.Rate, "600"),
				(RatingCalculatorColumns.Combined.AgentRate, "601")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.Combined.Operator, "BAS"),
				(RatingCalculatorColumns.Combined.Rate, "500"),
				(RatingCalculatorColumns.Combined.AgentRate, "501")
			);

			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.Combined, CombinedCalculator>(logger, calc);

			AssertEquals("The UseInclusiveBreaks property should be true", (ZBool)true, calc.UseInclusiveBreaks);
			AssertEquals("The IsAccumulated property should be true", (ZBool)true, calc.IsAccumulated);
			AssertEquals("The UseHigherChargeableLowerRateRule property should be true", (ZBool)true, calc.UseHigherChargeableLowerRateRule);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("-", "+", "BAS", "MIN", "MAX")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "-",
					Break = 10,
					Rate = 21,
					AgentRate = 22,
					FlatAmount = 111
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 10,
					IsRestricted = true,
					RestrictedReason = "Batman"
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 100,
					Rate = 210,
					AgentRate  = 220,
					FlatAmount = 222
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 1000,
					IsRestricted = true,
					RestrictedReason = "Help"
				},
				new BreakForTest()
				{
					Operator = "MIN",
					Rate = 300,
					AgentRate = 301
				},
				new BreakForTest()
				{
					Operator = "MAX",
					Rate = 600,
					AgentRate = 601
				},
				new BreakForTest()
				{
					Operator = "BAS",
					AgentRate = 501,
					Rate = 500,
				}
			};

			AssertContainsExactElementsInAnyOrder(
				"The calculated breaks should match the expected values",
				expected,
				results
			);
		}

		public void TestImportCalculator_Combined_WithPerUnit()
		{
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.Combined, CombinedCalculator>(logger,
				(RatingCalculatorColumns.Combined.UseInclusiveBreaks, "TRUE"),
				(RatingCalculatorColumns.Combined.UseCumulativeBreaks, "Y"),
				(RatingCalculatorColumns.Combined.UseHigherBreakLowerRate, "1"),
				(RatingCalculatorColumns.Combined.Operator, "UNT"),
				(RatingCalculatorColumns.Combined.Rate, "21"),
				(RatingCalculatorColumns.Combined.AgentRate, "22")
			);

			AssertEquals("UseInclusiveBreaks property does not match the expected value.", (ZBool)true, calc.UseInclusiveBreaks);
			AssertEquals("IsAccumulated property does not match the expected value.", (ZBool)true, calc.IsAccumulated);
			AssertEquals("UseHigherChargeableLowerRateRule property does not match the expected value.", (ZBool)true, calc.UseHigherChargeableLowerRateRule);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("UNT")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "UNT",
					Rate = 21,
					AgentRate = 22,
				},
			};

			AssertContainsExactElementsInAnyOrder(
				"RateLineItems do not match the expected collection.",
				expected,
				results
			);
		}

		public void TestImportCalculator_DisbursementInterest()
		{
			var logger = (new Mock<INotifications>()).Object;

			var baf = Helper.ChargeCodes["BAF"];
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.DisbursementInterest, DisbursementInterestCalculator>(logger,
				(RatingCalculatorColumns.DisbursementInterest.ApplyToType, "COD"),
				(RatingCalculatorColumns.DisbursementInterest.AccChargeCode__AC_Code, baf.AC_Code),

				(RatingCalculatorColumns.DisbursementInterest.Uplift, "1"),
				(RatingCalculatorColumns.DisbursementInterest.AgentUplift, "2"),

				(RatingCalculatorColumns.DisbursementInterest.AdjustmentDays, "3"),
				(RatingCalculatorColumns.DisbursementInterest.AgentAdjustmentDays, "4"),

				(RatingCalculatorColumns.DisbursementInterest.ApplyToOutstandingDaysOnly, "N"),
				(RatingCalculatorColumns.DisbursementInterest.IncludeGST, "Y")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.DisbursementInterest.ApplyToType, "FRT")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.DisbursementInterest.ApplyToType, "ALL")
			);

			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.DisbursementInterest, DisbursementInterestCalculator>(logger, calc);

			AssertEquals("Uplift should be equal to 1.", (ZDecimal)1, calc.Uplift);
			AssertEquals("AdjustmentDays should be equal to 3.", (ZDecimal)3, calc.AdjustmentDays);
			AssertEquals("IncludeGST should be true.", (ZBool)true, calc.IncludeGST);
			AssertEquals("OutstandingDays should be false.", (ZBool)false, calc.OutstandingDays);

			calc.Line.SetViewAgentRatesWithoutRefreshBinding(true);
			AssertEquals("Uplift should be updated to 2.", (ZDecimal)2, calc.Uplift);
			AssertEquals("AdjustmentDays should be updated to 4.", (ZDecimal)4, calc.AdjustmentDays);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("APP")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "APP",
					ApplyTo = "COD",
					ChargeCode = baf.PK,
				},
				new BreakForTest()
				{
					Operator = "APP",
					ApplyTo = "FRT"
				},
				new BreakForTest()
				{
					Operator = "APP",
					ApplyTo = "ALL"
				},
			};

			AssertContainsExactElementsInAnyOrder("The RateLineItems should match the expected collection.", expected, results);
		}

		public void TestImportCalculator_Equalization()
		{
			var logger = (new Mock<INotifications>()).Object;

			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.Equalization, EqualizationCalculator>(logger,
				(RatingCalculatorColumns.Equalization.UseInclusiveBreaks, "Y"),
				(RatingCalculatorColumns.Equalization.PivotBreakMinusOrPlus, "-"),
				(RatingCalculatorColumns.Equalization.PivotBreak, "66"),
				(RatingCalculatorColumns.Equalization.Rate, "30"),
				(RatingCalculatorColumns.Equalization.AgentRate, "31"),
				(RatingCalculatorColumns.Equalization.FlatAmount, "33")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.Equalization.PivotBreakMinusOrPlus, "+"),
				(RatingCalculatorColumns.Equalization.PivotBreak, "66"),
				(RatingCalculatorColumns.Equalization.Rate, "20"),
				(RatingCalculatorColumns.Equalization.AgentRate, "21"),
				(RatingCalculatorColumns.Equalization.FlatAmount, "23"),
				(RatingCalculatorColumns.Equalization.RestrictedReason, "Yabadabadoo"),
				(RatingCalculatorColumns.Equalization.IsRestricted, "Y")
			);

			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.Equalization, EqualizationCalculator>(logger, calc);

			AssertEquals("Inclusive Breaks flag should be set to true.", (ZBool)true, calc.UseInclusiveBreaks);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("+", "-")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "+",
					Break = 66,
					RestrictedReason = "Yabadabadoo",
					IsRestricted = true
				},
				new BreakForTest()
				{
					Operator = "-",
					Break = 66,
					FlatAmount = 33,
					Rate = 30,
					AgentRate = 31
				},
			};

			AssertContainsExactElementsInAnyOrder(
				"The results should match the expected breaks.",
				expected,
				results
			);
		}

		public void TestImportCalculator_ExcludeCompanyTariff()
		{
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.ExcludeCompanyTariffs, ExcludeCompanyTariffsCalculator>(logger);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.Select(i => new BreakForTest(i));

			AssertEquals("The results collection should be empty.", 0, results.Count());
		}

		public void TestImportCalculator_FirstPlusAdditional()
		{
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.FirstPlusAdditional, FirstPlusAdditionalCalculator>(logger,
				(RatingCalculatorColumns.FirstPlusAdditional.AdditionalItem, "111"),
				(RatingCalculatorColumns.FirstPlusAdditional.AgentAdditionalItem, "222"),
				(RatingCalculatorColumns.FirstPlusAdditional.FirstItem, "11"),
				(RatingCalculatorColumns.FirstPlusAdditional.AgentFirstItem, "22")
			);

			AssertEquals("The first item value should be 11.", (ZDecimal)11, calc.First);
			AssertEquals("The additional item value should be 111.", (ZDecimal)111, calc.Additional);

			calc.Line.SetViewAgentRatesWithoutRefreshBinding(true);

			AssertEquals("The first item value should be updated to 22.", (ZDecimal)22, calc.First);
			AssertEquals("The additional item value should be updated to 222.", (ZDecimal)222, calc.Additional);
		}

		public void TestImportCalculator_Flat_And_FreightInclusive()
		{
			var baf = Helper.ChargeCodes["BAF"];
			var logger = (new Mock<INotifications>()).Object;
			var headersFLT = GetColumnOrRelationshipNames(RatingCalculatorColumns.Flat.BasePrice, RatingCalculatorColumns.Flat.AgentBasePrice);
			var headersFRT = GetColumnOrRelationshipNames(RatingCalculatorColumns.FreightInclusive.Type, RatingCalculatorColumns.FreightInclusive.AccChargeCode__AC_Code);

			var org = Helper.NewOrgHeader();
			var costing = Helper.NewCosting(org);
			var entry = costing.AddRateEntry("FCL", "FCL", "AU");
			var rateLineFLT = entry.AddRateLine("BAF", "FLT", "KG");
			var rateLineFRT = entry.AddRateLine("CAF", "FRT", "KG");

			// Import FLT and FRT the first time.
			var calcFLT = ImportFirstCalculatorRow<FlatCalculator>(logger, rateLineFLT, true,
				(headersFLT[0], "100"),
				(headersFLT[1], "200"),
				(headersFRT[0], string.Empty),
				(headersFRT[1], string.Empty)
			);
			var calcFRT = ImportFirstCalculatorRow<FreightInclusiveCalculator>(logger, rateLineFRT, true,
				(headersFLT[0], string.Empty),
				(headersFLT[1], string.Empty),
				(headersFRT[0], "INC"),
				(headersFRT[1], baf.AC_Code)
			);

			// Re-import them again with bogus values, the RateLines should remain unchanged
			var calcUnmodifiedFRT = ImportFirstCalculatorRow<FreightInclusiveCalculator>(logger, rateLineFRT, true,
				(headersFLT[0], "100"),
				(headersFLT[1], "200"),
				(headersFRT[0], string.Empty),
				(headersFRT[1], string.Empty)
			);
			var calcUnmodifiedFLT = ImportFirstCalculatorRow<FlatCalculator>(logger, rateLineFLT, true,
				(headersFLT[0], string.Empty),
				(headersFLT[1], string.Empty),
				(headersFRT[0], "INC"),
				(headersFRT[1], baf.AC_Code)
			);

			AssertEquals("BaseRate of calcFLT should be 100 on initial import", (ZDecimal)100, calcFLT.BaseRate);
			calcFLT.Line.SetViewAgentRatesWithoutRefreshBinding(true);
			AssertEquals("BaseRate of calcFLT should be 200 after switching view binding", (ZDecimal)200, calcFLT.BaseRate);
			calcFLT.Line.SetViewAgentRatesWithoutRefreshBinding(false);

			AssertEquals("BaseRate of calcUnmodifiedFLT should remain 100 after re-import", (ZDecimal)100, calcUnmodifiedFLT.BaseRate);
			calcUnmodifiedFLT.Line.SetViewAgentRatesWithoutRefreshBinding(true);
			AssertEquals("BaseRate of calcUnmodifiedFLT should remain 200 after re-import and switching view binding", (ZDecimal)200, calcUnmodifiedFLT.BaseRate);

			AssertEquals("FreightCalcType of calcFRT should be 'INC'", (ZString)"INC", calcFRT.FreightCalcType);
			AssertEquals("ChargeCode of calcFRT should match BAF's primary key", baf.PK, calcFRT.ChargeCode);
			AssertEquals("FreightCalcType of calcUnmodifiedFRT should be 'INC' after re-import", (ZString)"INC", calcUnmodifiedFRT.FreightCalcType);
			AssertEquals("ChargeCode of calcUnmodifiedFRT should match BAF's primary key after re-import", baf.PK, calcUnmodifiedFRT.ChargeCode);
		}

		public void TestImportCalculator_CalculatorProperty_Error()
		{
			var notifications = new List<INotification>();
			var loggerMock = new Mock<INotifications>(MockBehavior.Strict);
			loggerMock
				.Setup(x => x.Add(It.IsAny<INotification>()))
				.Callback<INotification>((x) => notifications.Add(x));
			var logger = loggerMock.Object;

			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.Flat, FlatCalculator>(logger,
				expectedSuccess: false,

				(RatingCalculatorColumns.Flat.BasePrice, "XYZ"),
				(RatingCalculatorColumns.Flat.AgentBasePrice, "222")
			);

			var actualNotifications = notifications.Select(n => $"{n.Type.EnumValueName}|{n.Message}").ToArray();

			var expectedNotifications = new[]
			{
				"Error|Could not set values for a calculator of type 'FLT'. Values were '(BasePrice:XYZ) Column 0, (AgentBasePrice:222) Column 1'. Could not convert value 'XYZ'. Error in Row 0",
				"Error|RateLineItem importing has failed. The related incomplete RateLine has been deleted as a result"
			};

			AssertContainsExactElementsInAnyOrder(
				"Notifications should match the expected error messages",
				expectedNotifications,
				actualNotifications
			);

			Assert("The RateLineBizO should be deleted", calc.RateLineBizO.IsDeleted);

			loggerMock.VerifyAll();
		}

		public void TestImportCalculator_WhenImportingInvalidBooleanToFirstCalculatorRow_ThenErrorShouldBeNotified()
		{
			var notifications = new List<INotification>();
			var loggerMock = new Mock<INotifications>(MockBehavior.Strict);
			loggerMock
				.Setup(x => x.Add(It.IsAny<INotification>()))
				.Callback<INotification>((x) => notifications.Add(x));
			var logger = loggerMock.Object;

			ImportFirstCalculatorRow<RatingCalculatorColumns.Equalization, EqualizationCalculator>(logger, false,
				(RatingCalculatorColumns.Equalization.UseInclusiveBreaks, "3129823"),
				(RatingCalculatorColumns.Equalization.IsRestricted, null),
				(RatingCalculatorColumns.Equalization.PivotBreakMinusOrPlus, "-"),
				(RatingCalculatorColumns.Equalization.PivotBreak, "66"),
				(RatingCalculatorColumns.Equalization.Rate, "30"),
				(RatingCalculatorColumns.Equalization.AgentRate, "31"),
				(RatingCalculatorColumns.Equalization.FlatAmount, "33")
			);

			var actualNotifications = notifications
				.Select(n => $"{n.Type.EnumValueName}|{n.Message}")
				.ToArray();

			var expectedNotifications = new[]
			{
				"Error|Could not set values for a calculator of type 'VED'. Values were '(UseInclusiveBreaks:3129823) Column 0, (PivotBreakMinusOrPlus:-) Column 2, (PivotBreak:66) Column 3, (Rate:30) Column 4, (AgentRate:31) Column 5, (FlatAmount:33) Column 6'. Could not convert value '3129823'. Error in Row 0",
				"Error|RateLineItem importing has failed. The related incomplete RateLine has been deleted as a result"
			};

			AssertContainsExactElementsInAnyOrder(
				"The notifications should match the expected error messages",
				expectedNotifications,
				actualNotifications
			);

			loggerMock.VerifyAll();
		}

		public void TestImportCalculator_WhenImportingInvalidBooleanToSubsequentCalculatorRow_ThenErrorShouldBeNotified()
		{
			var notifications = new List<INotification>();
			var loggerMock = new Mock<INotifications>(MockBehavior.Strict);
			loggerMock
				.Setup(x => x.Add(It.IsAny<INotification>()))
				.Callback<INotification>((x) => notifications.Add(x));
			var logger = loggerMock.Object;

			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.ValueRange, ValueRangeCalculator>(
				logger,
				(RatingCalculatorColumns.ValueRange.ApplyTo, "VAL")
			);
			ImportSubsequentCalculatorRow(
				logger,
				calc,
				false,
				(RatingCalculatorColumns.ValueRange.Operator, "+"),
				(RatingCalculatorColumns.ValueRange.Break, "10"),
				(RatingCalculatorColumns.ValueRange.Rate, null),
				(RatingCalculatorColumns.ValueRange.AgentRate, null),
				(RatingCalculatorColumns.ValueRange.IsRestricted, "3129823"),
				(RatingCalculatorColumns.ValueRange.RestrictedReason, "Oh no!")
			);

			var actual = notifications
				.Select(n => $"{n.Type.EnumValueName}|{n.Message}")
				.ToArray();

			var expected = new[]
			{
				$"{NotificationType.Error.EnumValueName}|Could not set values for a calculator of type 'IXC'. Values were '(Operator:+) Column 0, (Break:10) Column 1, (IsRestricted:3129823) Column 4, (RestrictedReason:Oh no!) Column 5'. Could not convert value '3129823'. Error in Row 0",
				$"{NotificationType.Error.EnumValueName}|RateLineItem importing has failed. The related incomplete RateLine has been deleted as a result"
			};

			AssertContainsExactElementsInAnyOrder(
				"The notifications should match the expected error messages.",
				expected,
				actual
			);

			loggerMock.VerifyAll();
		}

		public void TestImportCalculator_Breaks_Error()
		{
			var notifications = new List<INotification>();
			var loggerMock = new Mock<INotifications>(MockBehavior.Strict);
			loggerMock
				.Setup(x => x.Add(It.IsAny<INotification>()))
				.Callback<INotification>((x) => notifications.Add(x));
			var logger = loggerMock.Object;

			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.Combined, CombinedCalculator>(logger,
				expectedSuccess: false,
				(RatingCalculatorColumns.Combined.Operator, "-"),
				(RatingCalculatorColumns.Combined.Rate, "pizza")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				expectedSuccess: false,
				(RatingCalculatorColumns.Combined.Operator, "+"),
				(RatingCalculatorColumns.Combined.Break, "10"),
				(RatingCalculatorColumns.Combined.IsRestricted, "Y"),
				(RatingCalculatorColumns.Combined.RestrictedReason, "Batman")
			);

			var actualNotifications = notifications
				.Select(n => $"{n.Type.EnumValueName}|{n.Message}")
				.ToArray();

			var expectedNotifications = new[]
			{
				"Error|Could not set values for a calculator of type 'CMB'. Values were '(Operator:-) Column 0, (Rate:pizza) Column 1'. Could not convert value 'pizza'. Error in Row 0",
				"Error|RateLineItem importing has failed. The related incomplete RateLine has been deleted as a result"
			};

			AssertContainsExactElementsInAnyOrder(
				"The notifications should match the expected error messages",
				expectedNotifications,
				actualNotifications
			);

			Assert("The RateLineBizO object should be marked as deleted", calc.RateLineBizO.IsDeleted);

			AssertEquals(
				"The total error count should be 1 due to accessing a deleted object",
				1,
				ErrorReporter.TotalErrorCount
			);

			ErrorReporter.Clear();

			loggerMock.VerifyAll();
		}

		public void TestImportCalculator_Flat()
		{
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.Flat, FlatCalculator>(logger,
				(RatingCalculatorColumns.Flat.BasePrice, "111"),
				(RatingCalculatorColumns.Flat.AgentBasePrice, "222")
			);

			AssertEquals("The base rate should match the initial base price", (ZDecimal)111, calc.BaseRate);

			calc.Line.SetViewAgentRatesWithoutRefreshBinding(true);
			AssertEquals("The base rate should update to match the agent base price", (ZDecimal)222, calc.BaseRate);
		}

		public void TestImportCalculator_FlatPlusPerUnit()
		{
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.FlatPlusPerUnit, FlatPlusPerUnitCalculator>(logger,
				(RatingCalculatorColumns.FlatPlusPerUnit.BasePrice, "111"),
				(RatingCalculatorColumns.FlatPlusPerUnit.AgentBasePrice, "222"),
				(RatingCalculatorColumns.FlatPlusPerUnit.PerUnitPrice, "11"),
				(RatingCalculatorColumns.FlatPlusPerUnit.AgentPerUnitPrice, "22")
			);

			AssertEquals("The calculated PerUnit should match the expected base value", (ZDecimal)11, calc.PerUnit);
			AssertEquals("The calculated BaseRate should match the expected base value", (ZDecimal)111, calc.BaseRate);

			calc.Line.SetViewAgentRatesWithoutRefreshBinding(true);
			AssertEquals("The calculated PerUnit should match the expected agent value", (ZDecimal)22, calc.PerUnit);
			AssertEquals("The calculated BaseRate should match the expected agent value", (ZDecimal)222, calc.BaseRate);
		}

		public void TestImportCalculator_FreightInclusive()
		{
			var baf = Helper.ChargeCodes["BAF"];
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.FreightInclusive, FreightInclusiveCalculator>(logger,
				(RatingCalculatorColumns.FreightInclusive.Type, "INC"),
				(RatingCalculatorColumns.FreightInclusive.AccChargeCode__AC_Code, baf.AC_Code)
			);

			AssertEquals("FreightCalcType should match the expected ZString value.", (ZString)"INC", calc.FreightCalcType);
			AssertEquals("ChargeCode should be equivalent to the primary key of baf.", baf.PK, calc.ChargeCode);
		}

		/// <summary>
		/// The below test checks that AccChargeCode relationships work.
		/// Although it uses the 'FreightInclusive' calculator, that's just
		/// a means to an end.
		/// </summary>
		public void TestImportCalculator_AccChargeCodeRelationship_NotFound()
		{
			var notifications = new List<INotification>();
			var loggerMock = new Mock<INotifications>(MockBehavior.Strict);
			loggerMock
				.Setup(x => x.Add(It.IsAny<INotification>()))
				.Callback<INotification>((x) => notifications.Add(x));
			var logger = loggerMock.Object;

			ImportFirstCalculatorRow<RatingCalculatorColumns.FreightInclusive, FreightInclusiveCalculator>(logger,
				expectedSuccess: false,
				(RatingCalculatorColumns.FreightInclusive.Type, "INC"),
				(RatingCalculatorColumns.FreightInclusive.AccChargeCode__AC_Code, "XYZ")
			);

			var actualNotifications = notifications
				.Select(n => $"{n.Type.EnumValueName}|{n.Message}")
				.ToArray();

			var expectedNotifications = new[]
			{
				$"{NotificationType.Error.EnumValueName}|Charge code with name 'XYZ' not found.",
				$"{NotificationType.Error.EnumValueName}|RateLineItem importing has failed. The related incomplete RateLine has been deleted as a result"
			};

			AssertContainsExactElementsInAnyOrder(
				"Notifications should match the expected errors",
				expectedNotifications,
				actualNotifications
			);

			loggerMock.VerifyAll();
		}

		/// <summary>
		/// The below test checks that AccChargeCode relationships work.
		/// Although it uses the 'FreightInclusive' calculator, that's just
		/// a means to an end.
		/// </summary>
		public void TestImportCalculator_AccChargeCodeRelationship_FoundGlobal()
		{
			// Creating a global charge code also creates a local one for the current company.
			// We make a global one and delete the local one.
			var globalEEE = Helper.ChargeCodes.CreateGlobalCharge("EEE");
			AssertEquals("A null AC_GC means it is a global charge code", ZGuid.Empty, globalEEE.AC_GC);
			Factory.Save();

			var localEEE = Helper.ChargeCodes["EEE"];
			AssertEquals(Env.CurrentCompanyPK, localEEE.AC_GC);
			AssertNotEquals(localEEE.PK, globalEEE.PK);
			localEEE.Delete();
			Factory.Save();

			var notifications = new List<INotification>();
			var loggerMock = new Mock<INotifications>(MockBehavior.Strict);
			loggerMock
				.Setup(x => x.Add(It.IsAny<INotification>()))
				.Callback<INotification>((x) => notifications.Add(x));
			var logger = loggerMock.Object;

			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.FreightInclusive, FreightInclusiveCalculator>(logger,
				(RatingCalculatorColumns.FreightInclusive.Type, "INC"),
				(RatingCalculatorColumns.FreightInclusive.AccChargeCode__AC_Code, "EEE")
			);

			AssertEquals((ZString)"INC", calc.FreightCalcType);
			AssertEquals("and it found the global one", globalEEE.PK, calc.ChargeCode);
			AssertEquals(0, notifications.Count());
		}

		/// <summary>
		/// The below test checks that AccChargeCode relationships work.
		/// Although it uses the 'FreightInclusive' calculator, that's just
		/// a means to an end.
		/// </summary>
		public void TestImportCalculator_AccChargeCodeRelationship_FoundLocal_GlobalExists()
		{
			var globalEEE = Helper.ChargeCodes.CreateGlobalCharge("EEE");
			AssertEquals("A null AC_GC means it is a global charge code", ZGuid.Empty, globalEEE.AC_GC);
			Factory.Save();

			var localEEE = Helper.ChargeCodes["EEE"];
			AssertEquals(Env.CurrentCompanyPK, localEEE.AC_GC);

			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.FreightInclusive, FreightInclusiveCalculator>(
				logger,
				(RatingCalculatorColumns.FreightInclusive.Type, "INC"),
				(RatingCalculatorColumns.FreightInclusive.AccChargeCode__AC_Code, "EEE")
			);

			AssertEquals((ZString)"INC", calc.FreightCalcType);
			AssertEquals("And it found the local one", localEEE.PK, calc.ChargeCode);
		}

		/// <summary>
		/// The below test checks that AccChargeCode relationships work.
		/// Although it uses the 'FreightInclusive' calculator, that's just
		/// a means to an end.
		/// </summary>
		public void TestImportCalculator_AccChargeCodeRelationship_FoundLocal_NoGlobalExists()
		{
			var eee = Helper.ChargeCodes.New("EEE", "EEE charge code", "FLT");
			AssertNotNull("eee.AC_GC", eee.AC_GC); // A non-null AC_GC means it is a local charge code
			Factory.Save();

			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.FreightInclusive, FreightInclusiveCalculator>(
				logger,
				(RatingCalculatorColumns.FreightInclusive.Type, "INC"),
				(RatingCalculatorColumns.FreightInclusive.AccChargeCode__AC_Code, "EEE")
			);

			AssertEquals((ZString)"INC", calc.FreightCalcType);
			AssertEquals("And it found the local one", eee.PK, calc.ChargeCode);
		}

		public void TestImportCalculator_HighestCharge()
		{
			var baf = Helper.ChargeCodes["BAF"];
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.HighestCharge, HighestChargeCalculator>(logger,
				(RatingCalculatorColumns.HighestCharge.MustMapToSomethingAnythingWillDo, null),
				(RatingCalculatorColumns.HighestCharge.AccChargeCode__AC_Code, baf.AC_Code)
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.HighestCharge.MustMapToSomethingAnythingWillDo, ""),
				(RatingCalculatorColumns.HighestCharge.AccChargeCode__AC_Code, baf.AC_Code));

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.HighestCharge.MustMapToSomethingAnythingWillDo, "Potato"),
				(RatingCalculatorColumns.HighestCharge.AccChargeCode__AC_Code, baf.AC_Code));

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("APP")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "APP",
					ApplyTo = "COD",
					ChargeCode = baf.PK
				},
				new BreakForTest()
				{
					Operator = "APP",
					ApplyTo = "COD",
					ChargeCode = baf.PK
				},
				new BreakForTest()
				{
					Operator = "APP",
					ApplyTo = "COD",
					ChargeCode = baf.PK
				},
			};

			AssertContainsExactElementsInAnyOrder(
				"Expected the calculated breaks to match the expected collection",
				expected,
				results
			);
		}

		public void TestImportCalculator_HighestRate()
		{
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.HighestRate, HighestRateCalculator>(logger,
				(RatingCalculatorColumns.HighestRate.Operator, "UNT"),
				(RatingCalculatorColumns.HighestRate.Rate, "10"),
				(RatingCalculatorColumns.HighestRate.AgentRate, "20"),
				(RatingCalculatorColumns.HighestRate.FlatAmount, "30"),
				(RatingCalculatorColumns.HighestRate.Multiple, "10"),
				(RatingCalculatorColumns.HighestRate.UnitAsFreighted, "HRM"),
				(RatingCalculatorColumns.HighestRate.Units, "KG")
			);
			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.HighestRate.Operator, "MIN"),
				(RatingCalculatorColumns.HighestRate.Rate, "100"),
				(RatingCalculatorColumns.HighestRate.AgentRate, "200"),
				(RatingCalculatorColumns.HighestRate.Multiple, "100")
			);

			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.HighestRate, HighestRateCalculator>(logger, calc);

			AssertEquals("RatePickRule should match 'HRM'", (ZString)"HRM", calc.RatePickRule);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("UNT", "MIN")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "UNT",
					Rate = 10,
					AgentRate = 20,
					FlatAmount = 30,
					UnitMultiple = 10,
					Units = "KG"
				},
				new BreakForTest()
				{
					Operator = "MIN",
					Rate = 100,
					AgentRate = 200,
					UnitMultiple = 100,
				},
			};

			AssertContainsExactElementsInAnyOrder(
				"Results should match the expected set of BreakForTest objects converted to strings for comparison",
				expected,
				results
			);
		}

		public void TestImportCalculator_HousebillReleaseType()
		{
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.HousebillReleaseType, HousebillReleaseTypeCalculator>(logger,
				(RatingCalculatorColumns.HousebillReleaseType.Code, "BSD"),
				(RatingCalculatorColumns.HousebillReleaseType.Amount, "111"),
				(RatingCalculatorColumns.HousebillReleaseType.AgentAmount, "222")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.HousebillReleaseType.Code, "NON"),
				(RatingCalculatorColumns.HousebillReleaseType.Amount, "333"),
				(RatingCalculatorColumns.HousebillReleaseType.AgentAmount, "444")
			);

			// Import a blank rowed. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.HousebillReleaseType, HousebillReleaseTypeCalculator>(logger, calc);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "BSD",
					Rate = 111,
					AgentRate = 222,
				},
				new BreakForTest()
				{
					Operator = "NON",
					Rate = 333,
					AgentRate = 444,
				},
			};

			AssertContainsExactElementsInAnyOrder(
				"The calculated results should match the expected collection",
				expected,
				results
			);
		}

		public void TestImportCalculator_Minimum()
		{
			var notifications = new List<INotification>();
			var loggerMock = new Mock<INotifications>(MockBehavior.Strict);
			loggerMock
				.Setup(x => x.Add(It.IsAny<INotification>()))
				.Callback<INotification>((x) => notifications.Add(x));
			var logger = loggerMock.Object;

			var calcPerJob = ImportFirstCalculatorRow<RatingCalculatorColumns.Minimum, MinimumCalculator>(logger,
				(RatingCalculatorColumns.Minimum.Value, "100"),
				(RatingCalculatorColumns.Minimum.AgentValue, "111"),
				(RatingCalculatorColumns.Minimum.IsPerJobMinimum, "Y"),
				(RatingCalculatorColumns.Minimum.IsPerChargeMinimum, "N")
			);

			var calcPerCharge = ImportFirstCalculatorRow<RatingCalculatorColumns.Minimum, MinimumCalculator>(logger,
				(RatingCalculatorColumns.Minimum.Value, "100"),
				(RatingCalculatorColumns.Minimum.AgentValue, "111"),
				(RatingCalculatorColumns.Minimum.IsPerJobMinimum, "N"),
				(RatingCalculatorColumns.Minimum.IsPerChargeMinimum, "Y")
			);

			// Cannot have both Job and Minimum set
			ImportFirstCalculatorRow<RatingCalculatorColumns.Minimum, MinimumCalculator>(logger,
				expectedSuccess: false,
				(RatingCalculatorColumns.Minimum.Value, "100"),
				(RatingCalculatorColumns.Minimum.AgentValue, "111"),
				(RatingCalculatorColumns.Minimum.IsPerJobMinimum, "Y"),
				(RatingCalculatorColumns.Minimum.IsPerChargeMinimum, "Y")
			);

			// Cannot have neither Job nor Minimum set.
			ImportFirstCalculatorRow<RatingCalculatorColumns.Minimum, MinimumCalculator>(logger,
				expectedSuccess: false,
				(RatingCalculatorColumns.Minimum.Value, "100"),
				(RatingCalculatorColumns.Minimum.AgentValue, "111"),
				(RatingCalculatorColumns.Minimum.IsPerJobMinimum, "n"),
				(RatingCalculatorColumns.Minimum.IsPerChargeMinimum, "n")
			);

			AssertEquals("calcPerJob.IsJobMinimum should be true.", (ZBool)true, calcPerJob.IsJobMinimum);
			AssertEquals("calcPerJob.IsChargeCodeMinimum should be false.", (ZBool)false, calcPerJob.IsChargeCodeMinimum);
			AssertEquals("calcPerJob.MinimumValue should be 100.", (ZDecimal)100, calcPerJob.MinimumValue);

			AssertEquals("calcPerCharge.IsJobMinimum should be false.", (ZBool)false, calcPerCharge.IsJobMinimum);
			AssertEquals("calcPerCharge.IsChargeCodeMinimum should be true.", (ZBool)true, calcPerCharge.IsChargeCodeMinimum);
			AssertEquals("calcPerCharge.MinimumValue should be 100.", (ZDecimal)100, calcPerCharge.MinimumValue);

			calcPerJob.Line.SetViewAgentRatesWithoutRefreshBinding(true);
			calcPerCharge.Line.SetViewAgentRatesWithoutRefreshBinding(true);
			AssertEquals("calcPerJob.MinimumValue should be updated to 111.", (ZDecimal)111, calcPerJob.MinimumValue);
			AssertEquals("calcPerCharge.MinimumValue should be updated to 111.", (ZDecimal)111, calcPerCharge.MinimumValue);

			var actualNotifications = notifications
				.Select(n => $"{n.Type.EnumValueName}|{n.Message}")
				.ToArray();

			var expectedNotifications = new[]
			{
				"Error|Calculator of type MIN contains invalid values for 'Is Job Minimum' and for 'Is Charge Minimum' as they are both set to the same value",
				"Error|RateLineItem importing has failed. The related incomplete RateLine has been deleted as a result",
				"Error|Calculator of type MIN contains invalid values for 'Is Job Minimum' and for 'Is Charge Minimum' as they are both set to the same value",
				"Error|RateLineItem importing has failed. The related incomplete RateLine has been deleted as a result"
			};

			AssertContainsExactElementsInAnyOrder(
				"The notifications should match the expected collection.",
				expectedNotifications,
				actualNotifications
			);

			loggerMock.VerifyAll();
		}

		public void TestImportCalculator_MinimumOrPerUnit()
		{
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.MinimumOrPerUnit, MinimumOrPerUnitCalculator>(logger,
				(RatingCalculatorColumns.MinimumOrPerUnit.AgentMinimumAmount, "400"),
				(RatingCalculatorColumns.MinimumOrPerUnit.AgentPerUnitAmount, "300"),
				(RatingCalculatorColumns.MinimumOrPerUnit.MinimumAmount, "200"),
				(RatingCalculatorColumns.MinimumOrPerUnit.PerUnitAmount, "100")
			);

			AssertEquals("The minimum amount should match the expected value.", (ZDecimal)200, calc.Minimum);
			AssertEquals("The per-unit amount should match the expected value.", (ZDecimal)100, calc.PerUnit);

			calc.Line.SetViewAgentRatesWithoutRefreshBinding(true);
			AssertEquals("The agent minimum amount should match the expected value after refresh.", (ZDecimal)400, calc.Minimum);
			AssertEquals("The agent per-unit amount should match the expected value after refresh.", (ZDecimal)300, calc.PerUnit);
		}

		public void TestImportCalculator_Note()
		{
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.Note, NoteCalculator>(logger,
				(RatingCalculatorColumns.Note.ShowOnBillingWithoutPrefix, "Y"),
				(RatingCalculatorColumns.Note.ItemDescription, "Cats"),
				(RatingCalculatorColumns.Note.AgentAmount, "200"),
				(RatingCalculatorColumns.Note.Amount, "100")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.Note.ItemDescription, "Dogs"),
				(RatingCalculatorColumns.Note.AgentAmount, "2000"),
				(RatingCalculatorColumns.Note.Amount, "1000")
			);

			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.Note, NoteCalculator>(logger, calc);

			AssertEquals(nameof(calc.ShowOnBillingWithoutPrefix), (ZBool)true, calc.ShowOnBillingWithoutPrefix);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType(string.Empty)
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Rate = 100,
					AgentRate = 200,
					Description = "Cats"
				},
				new BreakForTest()
				{
					Rate = 1000,
					AgentRate = 2000,
					Description = "Dogs"
				},
			};

			AssertContainsExactElementsInAnyOrder(
				"The results should match the expected rates, agent rates, and descriptions",
				expected,
				results
			);
		}

		public void TestImportCalculator_PackageCount()
		{
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.PackageCount, PackageCountCalculator>(logger,
				(RatingCalculatorColumns.PackageCount.AgentAdditionalPackage, "10"),
				(RatingCalculatorColumns.PackageCount.AgentBasicCharge, "20"),
				(RatingCalculatorColumns.PackageCount.AgentFirstPackage, "30"),
				(RatingCalculatorColumns.PackageCount.AgentRatePerKg, "40"),
				(RatingCalculatorColumns.PackageCount.AdditionalPackage, "100"),
				(RatingCalculatorColumns.PackageCount.BasicCharge, "200"),
				(RatingCalculatorColumns.PackageCount.FirstPackage, "300"),
				(RatingCalculatorColumns.PackageCount.RatePerKg, "400")
			);

			AssertEquals("FirstPackageRate should be 300.", (ZDecimal)300, calc.FirstPackageRate);
			AssertEquals("PerKG rate should be 400.", (ZDecimal)400, calc.PerKG);
			AssertEquals("BaseRate should be 200.", (ZDecimal)200, calc.BaseRate);
			AssertEquals("AdditionalPackageRate should be 100.", (ZDecimal)100, calc.AddtionalPackageRate);

			calc.Line.SetViewAgentRatesWithoutRefreshBinding(true);
			AssertEquals("BaseRate should be 20 after setting agent rates.", (ZDecimal)20, calc.BaseRate);
			AssertEquals("FirstPackageRate should be 30 after setting agent rates.", (ZDecimal)30, calc.FirstPackageRate);
			AssertEquals("PerKG rate should be 40 after setting agent rates.", (ZDecimal)40, calc.PerKG);
			AssertEquals("AdditionalPackageRate should be 10 after setting agent rates.", (ZDecimal)10, calc.AddtionalPackageRate);
		}

		public void TestImportCalculator_Percent()
		{
			var baf = Helper.ChargeCodes["BAF"];
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.Percentage, PercentageCalculator>(logger,
				(RatingCalculatorColumns.Percentage.AgentBasePrice, "11"),
				(RatingCalculatorColumns.Percentage.AgentMaximum, "21"),
				(RatingCalculatorColumns.Percentage.AgentMinimum, "31"),
				(RatingCalculatorColumns.Percentage.AgentPercentage, "51"),
				(RatingCalculatorColumns.Percentage.AgentPartThereofRate, "61"),
				(RatingCalculatorColumns.Percentage.AgentPartThereofValue, "71"),

				(RatingCalculatorColumns.Percentage.BasePrice, "10"),
				(RatingCalculatorColumns.Percentage.Maximum, "20"),
				(RatingCalculatorColumns.Percentage.Minimum, "30"),
				(RatingCalculatorColumns.Percentage.Percentage, "50"),
				(RatingCalculatorColumns.Percentage.PartThereofRate, "60"),
				(RatingCalculatorColumns.Percentage.PartThereofValue, "70"),

				(RatingCalculatorColumns.Percentage.IncludeGST, "Y"),
				(RatingCalculatorColumns.Percentage.UsePartThereof, "Y"),
				(RatingCalculatorColumns.Percentage.UseTakeHighestCharge, "Y"),

				(RatingCalculatorColumns.Percentage.AccChargeCode__AC_Code, baf.AC_Code),
				(RatingCalculatorColumns.Percentage.ApplyToType, "COD")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.Percentage.ApplyToType, "LOD")
			);

			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.Percentage, PercentageCalculator>(logger, calc);

			AssertEquals("IncludeGST should be true.", (ZBool)true, calc.IncludeGST);
			AssertEquals("GreaterCharge should be true.", (ZBool)true, calc.GreaterCharge);
			AssertEquals("IsPartThereof should be true.", (ZBool)true, calc.IsPartThereof);

			AssertEquals("BaseRate should match.", (ZDecimal)10, calc.BaseRate);
			AssertEquals("Maximum should match.", (ZDecimal)20, calc.Maximum);
			AssertEquals("Minimum should match.", (ZDecimal)30, calc.Minimum);
			AssertEquals("Percent should match.", (ZDecimal)50, calc.Percent);
			AssertEquals("Rate should match.", (ZDecimal)60, calc.Rate);
			AssertEquals("ValueOrPartThereOf should match.", (ZDecimal)70, calc.ValueOrPartThereOf);

			calc.Line.SetViewAgentRatesWithoutRefreshBinding(true);

			AssertEquals("Agent BaseRate should match.", (ZDecimal)11, calc.BaseRate);
			AssertEquals("Agent Maximum should match.", (ZDecimal)21, calc.Maximum);
			AssertEquals("Agent Minimum should match.", (ZDecimal)31, calc.Minimum);
			AssertEquals("Agent Percent should match.", (ZDecimal)51, calc.Percent);
			AssertEquals("Agent Rate should match.", (ZDecimal)61, calc.Rate);
			AssertEquals("Agent ValueOrPartThereOf should match.", (ZDecimal)71, calc.ValueOrPartThereOf);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("APP")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "APP",
					ApplyTo = "COD",
					ChargeCode = baf.PK
				},
				new BreakForTest()
				{
					Operator = "APP",
					ApplyTo = "LOD"
				},
			};

			AssertContainsExactElementsInAnyOrder(
				"The RateLineItems should match the expected collection.",
				expected,
				results
			);
		}
		public void TestImportCalculator_PercentageBreaks()
		{
			var baf = Helper.ChargeCodes["BAF"];
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.PercentageBreaks, PercentageBreaksCalculator>(logger,
				(RatingCalculatorColumns.PercentageBreaks.AccChargeCode__AC_Code, baf.AC_Code),
				(RatingCalculatorColumns.PercentageBreaks.ApplyToType, "COD"),

				(RatingCalculatorColumns.PercentageBreaks.UseInclusiveBreaks, "Y"),
				(RatingCalculatorColumns.PercentageBreaks.IncludeGST, "Y"),
				(RatingCalculatorColumns.PercentageBreaks.BreaksAreBasedOnValuesRatherThanMeasures, "Y"),

				(RatingCalculatorColumns.PercentageBreaks.Operator, "-"),
				(RatingCalculatorColumns.PercentageBreaks.Break, "10"),
				(RatingCalculatorColumns.PercentageBreaks.Rate, "20"),
				(RatingCalculatorColumns.PercentageBreaks.Percentage, "30"),
				(RatingCalculatorColumns.PercentageBreaks.AgentRate, "22"),
				(RatingCalculatorColumns.PercentageBreaks.FlatAmount, "55")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.PercentageBreaks.ApplyToType, "LOD"),

				(RatingCalculatorColumns.PercentageBreaks.Operator, "+"),
				(RatingCalculatorColumns.PercentageBreaks.Break, "10"),
				(RatingCalculatorColumns.PercentageBreaks.Percentage, "300"),
				(RatingCalculatorColumns.PercentageBreaks.IsRestricted, "Y"),
				(RatingCalculatorColumns.PercentageBreaks.RestrictedReason, "Beach"),
				(RatingCalculatorColumns.PercentageBreaks.FlatAmount, "555")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.PercentageBreaks.ApplyToType, "ALL")
			);

			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.PercentageBreaks.Operator, "+"),
				(RatingCalculatorColumns.PercentageBreaks.Break, "20"),
				(RatingCalculatorColumns.PercentageBreaks.Percentage, "400"),
				(RatingCalculatorColumns.PercentageBreaks.IsRestricted, "Y"),
				(RatingCalculatorColumns.PercentageBreaks.RestrictedReason, "Potato"),
				(RatingCalculatorColumns.PercentageBreaks.FlatAmount, "666")
			);

			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.PercentageBreaks, PercentageBreaksCalculator>(logger, calc);

			AssertEquals("IncludeGST should be true.", (ZBool)true, calc.IncludeGST);
			AssertEquals("UseInclusiveBreaks should be true.", (ZBool)true, calc.UseInclusiveBreaks);
			AssertEquals("UseBreaksBasedOnValues should be true.", (ZBool)true, calc.UseBreaksBasedOnValues);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("APP", "-", "+")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "APP",
					ApplyTo = "COD",
					ChargeCode = baf.PK
				},
				new BreakForTest()
				{
					Operator = "APP",
					ApplyTo = "LOD"
				},
				new BreakForTest()
				{
					Operator = "APP",
					ApplyTo = "ALL"
				},
				new BreakForTest()
				{
					Operator = "-",
					Break = 10,
					Rate = 20,
					Percentage = 30,
					AgentRate = 22,
					FlatAmount = 55
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 10,
					Percentage = 300,
					FlatAmount = 555,
					RestrictedReason = "Beach",
					IsRestricted = true
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 20,
					Percentage = 400,
					FlatAmount = 666,
					RestrictedReason = "Potato",
					IsRestricted = true
				},
			};

			AssertContainsExactElementsInAnyOrder("The resulting breaks should match the expected collection.", expected, results);
		}

		public void TestImportCalculator_ProfitShareRebate()
		{
			var baf = Helper.ChargeCodes["BAF"];
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.ProfitShareRebate, ProfitShareRebateCalculator>(logger,
				(RatingCalculatorColumns.ProfitShareRebate.AccChargeCode__AC_Code, baf.AC_Code),
				(RatingCalculatorColumns.ProfitShareRebate.ApplyToType, "COD"),

				(RatingCalculatorColumns.ProfitShareRebate.UseZeroWhenLoss, "Y"),

				(RatingCalculatorColumns.ProfitShareRebate.AgentBasePrice, "11"),
				(RatingCalculatorColumns.ProfitShareRebate.AgentMaximum, "21"),
				(RatingCalculatorColumns.ProfitShareRebate.AgentMinimum, "31"),
				(RatingCalculatorColumns.ProfitShareRebate.AgentPercentage, "41"),

				(RatingCalculatorColumns.ProfitShareRebate.BasePrice, "10"),
				(RatingCalculatorColumns.ProfitShareRebate.Maximum, "20"),
				(RatingCalculatorColumns.ProfitShareRebate.Minimum, "30"),
				(RatingCalculatorColumns.ProfitShareRebate.Percentage, "40")
			);
			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.ProfitShareRebate.ApplyToType, "LOD")
			);

			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.ProfitShareRebate, ProfitShareRebateCalculator>(logger, calc);

			AssertEquals("ZeroWhenLoss should be (ZBool) true.", (ZBool)true, calc.ZeroWhenLoss);

			AssertEquals("Maximum should be (ZDecimal) 20.", (ZDecimal)20, calc.Maximum);
			AssertEquals("Minimum should be (ZDecimal) 30.", (ZDecimal)30, calc.Minimum);
			AssertEquals("BaseRate should be (ZDecimal) 10.", (ZDecimal)10, calc.BaseRate);
			AssertEquals("Percent should be (ZDecimal) 40.", (ZDecimal)40, calc.Percent);

			calc.Line.SetViewAgentRatesWithoutRefreshBinding(true);
			AssertEquals("Maximum should be updated to (ZDecimal) 21.", (ZDecimal)21, calc.Maximum);
			AssertEquals("Minimum should be updated to (ZDecimal) 31.", (ZDecimal)31, calc.Minimum);
			AssertEquals("BaseRate should be updated to (ZDecimal) 11.", (ZDecimal)11, calc.BaseRate);
			AssertEquals("Percent should be updated to (ZDecimal) 41.", (ZDecimal)41, calc.Percent);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("APP")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "APP",
					ApplyTo = "COD",
					ChargeCode = baf.PK
				},
				new BreakForTest()
				{
					Operator = "APP",
					ApplyTo = "LOD"
				},
			};

			AssertContainsExactElementsInAnyOrder(
				"Results should match the expected collection of BreakForTest objects.",
				expected,
				results
			);
		}

		public void TestImportCalculator_SplitMonthBilling()
		{
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.SplitMonthBilling, SplitMonthBillingCalculator>(logger,
				(RatingCalculatorColumns.SplitMonthBilling.Operator, "-"),
				(RatingCalculatorColumns.SplitMonthBilling.Break, "10"),
				(RatingCalculatorColumns.SplitMonthBilling.Rate, "30"),
				(RatingCalculatorColumns.SplitMonthBilling.AgentRate, "31")
			);
			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.SplitMonthBilling.Operator, "+"),
				(RatingCalculatorColumns.SplitMonthBilling.Break, "10"),
				(RatingCalculatorColumns.SplitMonthBilling.Rate, "40"),
				(RatingCalculatorColumns.SplitMonthBilling.AgentRate, "41")
			);
			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.SplitMonthBilling.Operator, "+"),
				(RatingCalculatorColumns.SplitMonthBilling.Break, "20"),
				(RatingCalculatorColumns.SplitMonthBilling.IsRestricted, "Y"),
				(RatingCalculatorColumns.SplitMonthBilling.RestrictedReason, "Reason")
			);

			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.SplitMonthBilling, SplitMonthBillingCalculator>(logger, calc);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("-", "+")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "-",
					Break = 10,
					Rate = 30,
					AgentRate = 31
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 10,
					Rate = 40,
					AgentRate = 41
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 20,
					RestrictedReason = "Reason",
					IsRestricted = true
				}
			};

			AssertContainsExactElementsInAnyOrder(
				"The calculated breaks should match the expected collection.",
				expected,
				results
			);
		}

		public void TestImportCalculator_Time()
		{
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.Time, TimeCalculator>(logger,
				(RatingCalculatorColumns.Time.UseCumulativeBreaks, "Y"),
				(RatingCalculatorColumns.Time.UseHigherBreakLowerRate, "Y"),
				(RatingCalculatorColumns.Time.HolidaysToExclude, "WEH")
			);
			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.Time.Operator, "-"),
				(RatingCalculatorColumns.Time.Break, "10"),
				(RatingCalculatorColumns.Time.Units, "HR"),
				(RatingCalculatorColumns.Time.Rate, "20"),
				(RatingCalculatorColumns.Time.AgentRate, "30")
			);
			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.Time.Operator, "+"),
				(RatingCalculatorColumns.Time.Break, "10"),
				(RatingCalculatorColumns.Time.Units, "HR"),
				(RatingCalculatorColumns.Time.IsRestricted, "Y"),
				(RatingCalculatorColumns.Time.RestrictedReason, "Too slow")
			);

			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.Time, TimeCalculator>(logger, calc);

			AssertEquals("UseHigherChargeableLowerRateRule property should match the expected value.", (ZBool)true, calc.UseHigherChargeableLowerRateRule);
			AssertEquals("IsAccumulated property should match the expected value.", (ZBool)true, calc.IsAccumulated);
			AssertEquals("ExcludeHolidays property should match the expected value.", (ZString)"WEH", calc.ExcludeHolidays);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("-", "+")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "-",
					Break = 10,
					Units = "HR",
					Rate = 20,
					AgentRate = 30
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 10,
					Units = string.Empty, //Because only UNT and '-' allow units in TME
					IsRestricted = true,
					RestrictedReason = "Too slow"
				},
			};

			AssertContainsExactElementsInAnyOrder(
				"RateLineItems collection should match the expected result.",
				expected,
				results
			);
		}

		public void TestImportCalculator_Unit()
		{
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.Unit, UnitCalculator>(logger,
				(RatingCalculatorColumns.Unit.PerUnitPrice, "100"),
				(RatingCalculatorColumns.Unit.AgentPerUnitPrice, "200")
			);

			AssertEquals("The `PerUnit` value should match the `PerUnitPrice` from the input.", (ZDecimal)100, calc.PerUnit);

			calc.Line.SetViewAgentRatesWithoutRefreshBinding(true);

			AssertEquals("The `PerUnit` value should update to match the `AgentPerUnitPrice` from the input.", (ZDecimal)200, calc.PerUnit);
		}

		public void TestImportCalculator_ValueRange()
		{
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.ValueRange, ValueRangeCalculator>(logger,
				(RatingCalculatorColumns.ValueRange.ApplyTo, "VAL")
			);
			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.ValueRange.Operator, "-"),
				(RatingCalculatorColumns.ValueRange.Break, "10"),
				(RatingCalculatorColumns.ValueRange.Rate, "20"),
				(RatingCalculatorColumns.ValueRange.AgentRate, "30")
			);
			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.ValueRange.Operator, "+"),
				(RatingCalculatorColumns.ValueRange.Break, "10"),
				(RatingCalculatorColumns.ValueRange.IsRestricted, "Y"),
				(RatingCalculatorColumns.ValueRange.RestrictedReason, "Oh no!")
			);

			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.ValueRange, ValueRangeCalculator>(logger, calc);

			AssertEquals("Should set ApplyTo as VAL.", (ZString)"VAL", calc.ApplyTo);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.WithType("-", "+")
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "-",
					Break = 10,
					Rate = 20,
					AgentRate = 30
				},
				new BreakForTest()
				{
					Operator = "+",
					Break = 10,
					IsRestricted = true,
					RestrictedReason = "Oh no!"
				},
			};

			AssertContainsExactElementsInAnyOrder("Rate line items should match the expected breaks and rates.", expected, results);
		}

		public void TestImportCalculator_WarehouseLocationType()
		{
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.WarehouseLocationType, WarehouseLocationTypeCalculator>(logger,
				(RatingCalculatorColumns.WarehouseLocationType.Code, "PFC"),
				(RatingCalculatorColumns.WarehouseLocationType.Rate, "10"),
				(RatingCalculatorColumns.WarehouseLocationType.AgentRate, "11")
			);
			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.WarehouseLocationType.Code, "DPF"),
				(RatingCalculatorColumns.WarehouseLocationType.Rate, "20"),
				(RatingCalculatorColumns.WarehouseLocationType.AgentRate, "21")
			);

			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.WarehouseLocationType, WarehouseLocationTypeCalculator>(logger, calc);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "PFC",
					Rate = 10,
					AgentRate = 11
				},
				new BreakForTest()
				{
					Operator = "DPF",
					Rate = 20,
					AgentRate = 21
				},
			};

			AssertContainsExactElementsInAnyOrder(
				"The imported calculator values should match the expected results.",
				expected,
				results
			);
		}

		public void TestImportCalculator_WarehousePackCalculator()
		{
			var logger = (new Mock<INotifications>()).Object;
			var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.WarehousePack, WarehousePackCalculator>(logger,
				(RatingCalculatorColumns.WarehousePack.Code, "BAG"),
				(RatingCalculatorColumns.WarehousePack.Rate, "10"),
				(RatingCalculatorColumns.WarehousePack.AgentRate, "11")
			);
			ImportSubsequentCalculatorRow(logger, calc,
				(RatingCalculatorColumns.WarehousePack.Code, "BOX"),
				(RatingCalculatorColumns.WarehousePack.Rate, "20"),
				(RatingCalculatorColumns.WarehousePack.AgentRate, "21")
			);

			// Import a blank row. This should result in no additional rows recorded.
			ImportSubsequentCalculatorRow<RatingCalculatorColumns.WarehousePack, WarehousePackCalculator>(logger, calc);

			var results = calc
				.RateLineBizO
				.RateLineItems
					.Cast<RateLineItem>()
					.Select(i => new BreakForTest(i));

			var expected = new[]
			{
				new BreakForTest()
				{
					Operator = "BAG",
					Rate = 10,
					AgentRate = 11
				},
				new BreakForTest()
				{
					Operator = "BOX",
					Rate = 20,
					AgentRate = 21
				},
			};

			AssertContainsExactElementsInAnyOrder(
				"Results should match the expected BreakForTest collection",
				expected,
				results
			);
		}

		public void TestProductNumberIsConverted()
		{
			var rateLine = CreateRateLine(rateCategory: Category.WHS, rateMode: RateMode.ALL);
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_PartNum = "123";
			var productOrg = product.RelatedOrganisations.AddNew();
			productOrg.OU_OH = rateLine.Parent.Parent.TH_OH;
			Factory.Save();

			var importer = new RateLineGlowImporter();
			var logger = new Mock<INotifications>();

			AssertEquals("Initially the product number is empty", ZGuid.Empty, rateLine.TL_OP_ProductNumber);
			importer.ConvertCustomLine(rateLine, "123", logger.Object, 0, "Product_Number");
			AssertEquals("After conversion the product number is entered", product.PK, rateLine.TL_OP_ProductNumber);
			AssertNoErrors("There are no validation errors", rateLine.TL_OP_ProductNumberInfo);
			logger.Verify(x => x.Add(It.IsAny<INotification>()), Times.Never);
		}

		public void TestProductNumberNotFound_ShowLog()
		{
			var rateLine = CreateRateLine(rateCategory: Category.WHS, rateMode: RateMode.ALL);
			var importer = new RateLineGlowImporter();
			var logger = new Mock<INotifications>();

			AssertEquals("Initially the product number is empty", ZGuid.Empty, rateLine.TL_OP_ProductNumber);
			importer.ConvertCustomLine(rateLine, "123", logger.Object, 0, "Product_Number");
			AssertEquals("After conversion it's still empty as there's nothing found", ZGuid.Empty, rateLine.TL_OP_ProductNumber);
			AssertNoErrors("There are no validation errors", rateLine.TL_OP_ProductNumberInfo);
			logger.Verify(x => x.Add(It.Is<INotification>(x => x.Message == "Row 0, No Product exists with product number: 123")), Times.Once);
		}

		public void TestProductNumberNotApplicable_FailValidation()
		{
			var rateLine = CreateRateLine(rateCategory: Category.WHS, rateMode: RateMode.ALL);
			var importer = new RateLineGlowImporter();
			var logger = new Mock<INotifications>();
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_PartNum = "123";
			Assert("The product does not belong to the rate", !product.RelatedOrganisations.Any(x => x.PK == rateLine.Parent.Parent.TH_OH));

			Factory.Save();

			AssertEquals("Initially, the product number is empty", ZGuid.Empty, rateLine.TL_OP_ProductNumber);
			importer.ConvertCustomLine(rateLine, "123", logger.Object, 0, "Product_Number");
			AssertEquals("After conversion, the product number is entered", product.PK, rateLine.TL_OP_ProductNumber);
			AssertHasError("But there's a validation error", rateLine.TL_OP_ProductNumberInfo, "Enter a valid selection");
			logger.Verify(x => x.Add(It.IsAny<INotification>()), Times.Never);
		}

		/* Uncomment out for stress testing
				[StressTest]
				public void TestImportCalculator_OneRateLine_ManyRateLineItems()
				{
					// This test is mainly for performance profiling

					var logger = (new Mock<INotifications>()).Object;
					var currentBreak = 10;
					var currentRate = 20;
					var breaksRateLineItemsAdded = 2;

					var calc = ImportFirstCalculatorRow<RatingCalculatorColumns.Combined, CombinedCalculator>(logger,
						(RatingCalculatorColumns.Combined.Operator, "-"),
						(RatingCalculatorColumns.Combined.Break, currentBreak.ToString()),
						(RatingCalculatorColumns.Combined.Rate, currentRate.ToString())
					);

					currentRate++;
					ImportSubsequentCalculatorRow(logger, calc,
						(RatingCalculatorColumns.Combined.Operator, "+"),
						(RatingCalculatorColumns.Combined.Break, currentBreak.ToString()),
						(RatingCalculatorColumns.Combined.Rate, currentRate.ToString())
					);

					for (int i = 0; i < 10000; i++)
					{
						currentBreak++;
						currentRate++;
						breaksRateLineItemsAdded++;

						ImportSubsequentCalculatorRow(logger, calc,
							(RatingCalculatorColumns.Combined.Operator, "+"),
							(RatingCalculatorColumns.Combined.Break, currentBreak.ToString()),
							(RatingCalculatorColumns.Combined.Rate, currentRate.ToString())
						);
					}

					var results = calc
						.RateLineBizO
						.RateLineItems
							.Cast<RateLineItem>()
							.WithType("-", "+")
							.Select(i => new BreakForTest(i));

					// This test is not testing the accuracy of the results, rather just to check where the bottlenecks are.
					AssertEquals("The count of results should match the number of breaks rate line items added.", breaksRateLineItemsAdded, results.Count());
				}
		*/

		#region Helper functions

		RateLine CreateRateLine(string chargeCode = "FRT", string rateCategory = Category.FCL, string rateMode = RateMode.SEA)
		{
			var org = Helper.NewOrgHeader();
			var costing = Helper.NewCosting(org);
			var entry = costing.AddRateEntry(rateCategory, rateMode, "AU");
			var line = entry.AddRateLine(chargeCode, "_", "KG");

			return line;
		}

		/// <summary>
		/// Imports a single row into a calculator.
		/// succeeded or not
		/// </summary>
		/// <typeparam name="T">Must be one of the XYZCalculatorColumns enum</typeparam>
		T2 ImportFirstCalculatorRow<T, T2>(INotifications logger, params (T, string)[] input)
			where T : Enum
			where T2 : Calculator
		{
			return ImportFirstCalculatorRow<T, T2>(logger, true, input);
		}

		/// <summary>
		/// Imports a single row into a calculator with an expectation of whether the import
		/// succeeded or not
		/// </summary>
		/// <typeparam name="T">Must be one of the XYZCalculatorColumns enum</typeparam>
		T2 ImportFirstCalculatorRow<T, T2>(INotifications logger, bool expectedSuccess, params (T, string)[] input)
			where T : Enum
			where T2 : Calculator
		{
			var rateLine = CreateRateLine();

			return ImportFirstCalculatorRow<T, T2>(logger, rateLine, expectedSuccess, input);
		}

		/// <summary>
		/// Imports a single row into a calculator but allows an RateLine to be given for any extra
		/// configuration
		/// </summary>
		/// <typeparam name="T">Must be one of the XYZCalculatorColumns enum</typeparam>
		T2 ImportFirstCalculatorRow<T, T2>(INotifications logger, RateLine rateLine, params (T, string)[] inputs)
			where T : Enum
			where T2 : Calculator
		{
			return ImportFirstCalculatorRow<T, T2>(logger, rateLine, true, inputs);
		}

		/// <summary>
		/// Imports a single row into a calculator but allows an RateLine to be given for any extra
		/// configuration and also asserts the expectation of whether the import succeeded or not.
		/// </summary>
		/// <typeparam name="T">Must be one of the XYZCalculatorColumns enum</typeparam>
		T2 ImportFirstCalculatorRow<T, T2>(INotifications logger, RateLine rateLine, bool expectedSuccess, params (T, string)[] inputs)
			where T : Enum
			where T2 : Calculator
		{
			var importer = new RateLineGlowImporter();

			// Not calling using (DataImportIndicatorService.StartDataImport(rateLine.Factory))
			// on purpose since this is the equivalent of the call to
			// using (SupportDataImportingHelper.DataImporting(rateLine)) from GlowCollectionImporter.cs
			// when ImportData is about to be called from ImportChildLines.
			using (SupportDataImportingHelper.DataImporting(rateLine))
			{
				var calculatorCode = columnTypeToCalculatorCode[typeof(T).Name];
				rateLine.TL_RateCalculator = calculatorCode;

				var headers = GetColumnOrRelationshipNames(inputs.Select(t => t.Item1).ToArray());
				var values = GetPreviewLineDetails(inputs);
				var calculator = rateLine.GetCalculator<T2>();
				var result = importer.ImportChildlessChildren(rateLine, logger, 0, headers, values);
				AssertEquals("Importing result is unexpected", expectedSuccess, result);

				return calculator;
			}
		}

		T2 ImportFirstCalculatorRow<T2>(INotifications logger, RateLine rateLine, bool expectedSuccess, params (string, string)[] inputs)
			where T2 : Calculator
		{
			var importer = new RateLineGlowImporter();

			// Not calling using (DataImportIndicatorService.StartDataImport(rateLine.Factory))
			// on purpose since this is the equivalent of the call to
			// using (SupportDataImportingHelper.DataImporting(rateLine)) from GlowCollectionImporter.cs
			// when ImportData is about to be called from ImportChildLines.
			using (SupportDataImportingHelper.DataImporting(rateLine))
			{
				var headers = inputs.Select(t => t.Item1).ToArray();
				var values = GetPreviewLineDetails(inputs);

				var calculator = rateLine.GetCalculator<T2>();
				var result = importer.ImportChildlessChildren(rateLine, logger, 0, headers, values);
				AssertEquals("Importing result is unexpected", expectedSuccess, result);

				return calculator;
			}
		}

		/// <summary>
		/// Imports additional rows to a calculator
		/// </summary>
		/// <typeparam name="T">Must be one of the XYZCalculatorColumns enum</typeparam>
		T2 ImportSubsequentCalculatorRow<T, T2>(INotifications logger, T2 calculator, params (T, string)[] inputs)
			where T : Enum
			where T2 : Calculator
		{
			return ImportSubsequentCalculatorRow(logger, calculator, true, inputs);
		}

		/// <summary>
		/// Imports additional rows to a calculator
		/// </summary>
		/// <typeparam name="T">Must be one of the XYZCalculatorColumns enum</typeparam>
		T2 ImportSubsequentCalculatorRow<T, T2>(INotifications logger, T2 calculator, bool expectedSuccess, params (T, string)[] inputs)
		where T : Enum
		where T2 : Calculator
		{
			var rateLine = calculator.RateLineBizO;

			using (SupportDataImportingHelper.DataImporting(rateLine))
			{
				var headers = GetColumnOrRelationshipNames(inputs.Select(t => t.Item1).ToArray());
				var values = GetPreviewLineDetails(inputs);

				var result = Importer.ImportChildlessChildren(rateLine, logger, 0, headers, values);
				AssertEquals("Importing result is unexpected", expectedSuccess, result);
				Assert(rateLine.GetCalculator<T2>() == calculator);

				return calculator;
			}
		}

		ImportPreviewLineDetails[] GetPreviewLineDetails<T>((T, string)[] inputs) =>
			inputs.Select((t, index) => t.Item2 != null
			? new ImportPreviewLineDetails(t.Item2, index) : null).ToArray();

		string[] GetColumnOrRelationshipNames<T>(params T[] columns)
			where T : Enum
		{
			return columns
				.Select(c => RatingCalculatorColumns.GetColumnOrRelationshipName(c))
				.ToArray();
		}

		RefDomesticCartageZone CreateZone(ZString postCode, ZString zoneName, ZString loco, ZString city)
		{
			var unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, loco);
			var iata = unloco != null ? unloco.RL_IATA : ZString.Empty;

			var zone = Factory.New<RefDomesticCartageZone>();
			zone.F1_CityTownPostCode = postCode;
			zone.F1_Zone = zoneName;
			zone.F1_RL_NKLoco = loco;
			zone.F1_CityTown = city;
			zone.F1_PortCode = iata;

			return zone;
		}

		readonly RateLineGlowImporter Importer = new RateLineGlowImporter();

		static readonly Dictionary<string, string> columnTypeToCalculatorCode = new Dictionary<string, string>()
		{
			{ nameof(RatingCalculatorColumns.Agency), RatingCalculatorCodes.Agency },
			{ nameof(RatingCalculatorColumns.Cartage), RatingCalculatorCodes.Cartage },
			{ nameof(RatingCalculatorColumns.CartageZoneDistance), RatingCalculatorCodes.CartageZoneDistance },
			{ nameof(RatingCalculatorColumns.Combined), RatingCalculatorCodes.Combined },
			{ nameof(RatingCalculatorColumns.CompanyTariffBased), RatingCalculatorCodes.CompanyTariffBased },
			{ nameof(RatingCalculatorColumns.CostBased), RatingCalculatorCodes.CostBased },
			{ nameof(RatingCalculatorColumns.DisbursementInterest), RatingCalculatorCodes.DisbursementInterest },
			{ nameof(RatingCalculatorColumns.Equalization), RatingCalculatorCodes.Equalization },
			{ nameof(RatingCalculatorColumns.ExcludeCompanyTariffs), RatingCalculatorCodes.ExcludeCompanyTariffs },
			{ nameof(RatingCalculatorColumns.FirstPlusAdditional), RatingCalculatorCodes.FirstPlusAdditional },
			{ nameof(RatingCalculatorColumns.Flat), RatingCalculatorCodes.Flat },
			{ nameof(RatingCalculatorColumns.FlatPlusPerUnit), RatingCalculatorCodes.FlatPlusPerUnit },
			{ nameof(RatingCalculatorColumns.FreightInclusive), RatingCalculatorCodes.FreightInclusive },
			{ nameof(RatingCalculatorColumns.HighestCharge), RatingCalculatorCodes.HighestCharge },
			{ nameof(RatingCalculatorColumns.HighestRate), RatingCalculatorCodes.HighestRate },
			{ nameof(RatingCalculatorColumns.HousebillReleaseType), RatingCalculatorCodes.HousebillReleaseType },
			{ nameof(RatingCalculatorColumns.Minimum), RatingCalculatorCodes.Minimum },
			{ nameof(RatingCalculatorColumns.MinimumOrPerUnit), RatingCalculatorCodes.MinimumOrPerUnit },
			{ nameof(RatingCalculatorColumns.Note), RatingCalculatorCodes.Note },
			{ nameof(RatingCalculatorColumns.PackageCount), RatingCalculatorCodes.PackageCount },
			{ nameof(RatingCalculatorColumns.Percentage), RatingCalculatorCodes.Percentage },
			{ nameof(RatingCalculatorColumns.PercentageBreaks), RatingCalculatorCodes.PercentageBreaks },
			{ nameof(RatingCalculatorColumns.ProfitShareRebate), RatingCalculatorCodes.ProfitShareRebate },
			{ nameof(RatingCalculatorColumns.SplitMonthBilling), RatingCalculatorCodes.SplitMonthBilling },
			{ nameof(RatingCalculatorColumns.Time), RatingCalculatorCodes.Time },
			{ nameof(RatingCalculatorColumns.Unit), RatingCalculatorCodes.Unit },
			{ nameof(RatingCalculatorColumns.ValueRange), RatingCalculatorCodes.ValueRange },
			{ nameof(RatingCalculatorColumns.WarehouseLocationType), RatingCalculatorCodes.WarehouseLocationType },
			{ nameof(RatingCalculatorColumns.WarehousePack), RatingCalculatorCodes.WarehousePack },
		};

		#endregion
	}

	static class Extension
	{
		internal static IEnumerable<RateLineItem> WithType(this IEnumerable<RateLineItem> items, params string[] wantedTypes)
		{
			return items.Where(i => wantedTypes.Contains((string)i.TM_Type));
		}
	}

	class BreakForTest
	{
		internal BreakForTest()
		{
			isBeingConstructed = false;
		}

		internal BreakForTest(RateLineItem item)
		{
			Break = (decimal)item.TM_Break;

			Rate = (decimal)item.TM_Value;
			RelevantRate = (decimal)item.TM_RelevantValue;

			AgentRate = (decimal)item.TM_AgentDeclaredRate;
			IsRestricted = (bool)item.TM_CallForPricing;
			TransportZone = item.TM_TZ_DomesticZone;
			Operator = (string)item.TM_Type;
			FlatAmount = (decimal)item.TM_FlatAmount;
			Units = (string)item.TM_BreakWeightVolume;
			ChargeCode = item.TM_AC;
			UnitMultiple = (decimal)item.TM_UnitMultiple;

			// The following 3 lines are all stored in TM_BreakMinimum.
			// I'm leaving it as separate assignments to provide clarity.
			BreakMinimum = (decimal)item.TM_BreakMinimum;
			Percentage = (decimal)item.TM_BreakMinimum;

			AciZone = (string)item.TM_F1Zone;

			// The following 3 lines are all stored in TM_Text.
			// I'm leaving it as separate assignments to provide clarity.
			ApplyTo = (string)item.TM_Text;
			RestrictedReason = (string)item.TM_Text;
			Description = (string)item.TM_Text;

			isBeingConstructed = false;
		}

		public decimal Break { get; set; } = 0;

		public decimal BreakMinimum
		{
			get { return tmBreakMinimum; }
			set { tmBreakMinimum = value; }
		}

		public decimal Percentage
		{
			get { return tmBreakMinimum; }
			set { tmBreakMinimum = value; }
		}

		public decimal Rate
		{
			get
			{
				return rate;
			}
			set
			{
				rate = value;
				if (!isBeingConstructed)
				{
					RelevantRate = value;
				}
			}
		}

		public string AciZone { get; set; } = String.Empty;

		public decimal RelevantRate { get; private set; } = 0;

		public decimal AgentRate { get; set; } = 0;

		public bool IsRestricted { get; set; }

		public string RestrictedReason
		{
			get { return tmText; }
			set
			{
				tmText = value;
			}
		}

		public string Description
		{
			get { return tmText; }
			set { tmText = value; }
		}

		public string ApplyTo
		{
			get { return tmText; }
			set { tmText = value; }
		}

		public ZGuid TransportZone { get; set; } = ZGuid.Empty;

		public ZGuid ChargeCode { get; set; } = ZGuid.Empty;

		public decimal UnitMultiple { get; set; } = 1;

		public string Operator { get; set; } = string.Empty;

		public decimal FlatAmount { get; set; } = 0;

		public string Units { get; set; } = string.Empty;

		public override bool Equals(object obj)
		{
			if (obj is BreakForTest other)
			{
				return Break == other.Break &&
					   BreakMinimum == other.BreakMinimum &&
					   Percentage == other.Percentage &&
					   Rate == other.Rate &&
					   AciZone == other.AciZone &&
					   RelevantRate == other.RelevantRate &&
					   AgentRate == other.AgentRate &&
					   IsRestricted == other.IsRestricted &&
					   RestrictedReason == other.RestrictedReason &&
					   Description == other.Description &&
					   ApplyTo == other.ApplyTo &&
					   TransportZone == other.TransportZone &&
					   ChargeCode == other.ChargeCode &&
					   UnitMultiple == other.UnitMultiple &&
					   Operator == other.Operator &&
					   FlatAmount == other.FlatAmount &&
					   Units == other.Units;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Break.GetHashCode() ^ Rate.GetHashCode() ^ ChargeCode.GetHashCode();
		}

		decimal rate = 0;
		string tmText = string.Empty;
		decimal tmBreakMinimum = 0;
		readonly bool isBeingConstructed = true;
	}
}

