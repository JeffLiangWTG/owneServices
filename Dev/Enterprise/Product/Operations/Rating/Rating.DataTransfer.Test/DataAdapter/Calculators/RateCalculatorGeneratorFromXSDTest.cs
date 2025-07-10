using System;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture;
using static Enterprise.Core.Constants;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public abstract class RateCalculatorGeneratorFromXSDTest<TCalculator, TValueObject> : RatingTestCase
		where TCalculator : Calculator
		where TValueObject : Xsd.RateCalculator
	{
		#region Tests

		public void TestImportFromValueObject()
		{
			ImportFromValueObjectCore();
		}

		public void TestExportToValueObject()
		{
			ExportToValueObjectCore();
		}

		#region Import

		public void TestImportRateItemWithOperatorAndBreak()
		{
			Xsd.RateItemWithOperatorAndBreakCollection rateItemsXSD = new Xsd.RateItemWithOperatorAndBreakCollection();

			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("DST", "AIR", "", "AUSYD");
			var rateLine = rateEntry.AddRateLine("DCART", "", "M3", "AUD");
			rateLine.TL_RateCalculator = CombinedCalculator.Code;

			var provider = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, zoneNames: new ZString[] { "Zone" });
			var collection = new CartageZoneCollection(rateLine);
			collection.Load();
			var calculatorForTest = (CombinedCalculator)rateLine.Calculator;
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			#region Test Without RateItems

			calculatorForTest.RateLineItems.RemoveAndDeleteAll();
			RateCalculatorGenerator.ImportRateItemWithOperatorAndBreakForTest(calculatorForTest, rateItemsXSD, collection[1], context);

			AssertEquals("Calculator RateLineItems Count", 0, calculatorForTest.RateLineItems.Count);

			#endregion

			Xsd.RateItemWithOperatorAndBreak rateItemXSDPlus1 = rateItemsXSD.AddNew();
			rateItemXSDPlus1.Operator = new ZString("+");
			rateItemXSDPlus1.Units = "HR";

			Xsd.RateItemWithOperatorAndBreak rateItemXSDPlus2 = rateItemsXSD.AddNew();
			rateItemXSDPlus2.Operator = new ZString("+");
			rateItemXSDPlus2.Units = "HR";

			Xsd.RateItemWithOperatorAndBreak rateItemXSDMinus1 = rateItemsXSD.AddNew();
			rateItemXSDMinus1.Operator = new ZString("-");
			rateItemXSDMinus1.Units = "HR";

			#region Test With All Parameters (Specified =  false)

			calculatorForTest.RateLineItems.RemoveAndDeleteAll();
			RateCalculatorGenerator.ImportRateItemWithOperatorAndBreakForTest(calculatorForTest, rateItemsXSD, collection[1], context);

			AssertEquals("Calculator RateLineItems Count", 3, calculatorForTest.RateLineItems.Count);

			AssertEquals("Calculator's Operator", new ZString("-"), calculatorForTest.RateLineItems[0].TM_Type);
			AssertEquals("Calculator's FlatAmount", 0m, calculatorForTest.RateLineItems[0].TM_FlatAmount);
			AssertEquals("Calculator's BreakAmount", 0m, calculatorForTest.RateLineItems[0].TM_Break);
			AssertEquals("Calculator's BreakMinimum", 0m, calculatorForTest.RateLineItems[0].TM_BreakMinimum);
			AssertEquals("Calculator's PerUnit", 0m, calculatorForTest.RateLineItems[0].TM_RelevantValue);
			AssertEquals("Calculator's Units", "HR", calculatorForTest.RateLineItems[0].TM_BreakWeightVolume);
			AssertEquals("Calculator's ZoneName", "Zone", Factory.Load<RateTransportZone>(calculatorForTest.RateLineItems[0].TM_TZ_DomesticZone).TZ_ZoneName);

			AssertEquals("Calculator's Operator", new ZString("+"), calculatorForTest.RateLineItems[1].TM_Type);
			AssertEquals("Calculator's FlatAmount", 0m, calculatorForTest.RateLineItems[1].TM_FlatAmount);
			AssertEquals("Calculator's BreakAmount", 0m, calculatorForTest.RateLineItems[1].TM_Break);
			AssertEquals("Calculator's BreakMinimum", 0m, calculatorForTest.RateLineItems[1].TM_BreakMinimum);
			AssertEquals("Calculator's PerUnit", 0m, calculatorForTest.RateLineItems[1].TM_RelevantValue);
			AssertEquals("Calculator's Units", "HR", calculatorForTest.RateLineItems[1].TM_BreakWeightVolume);
			AssertEquals("Calculator's ZoneName", "Zone", Factory.Load<RateTransportZone>(calculatorForTest.RateLineItems[1].TM_TZ_DomesticZone).TZ_ZoneName);

			AssertEquals("Calculator's Operator", new ZString("+"), calculatorForTest.RateLineItems[2].TM_Type);
			AssertEquals("Calculator's FlatAmount", 0m, calculatorForTest.RateLineItems[2].TM_FlatAmount);
			AssertEquals("Calculator's BreakAmount", 0m, calculatorForTest.RateLineItems[2].TM_Break);
			AssertEquals("Calculator's BreakMinimum", 0m, calculatorForTest.RateLineItems[2].TM_BreakMinimum);
			AssertEquals("Calculator's PerUnit", 0m, calculatorForTest.RateLineItems[2].TM_RelevantValue);
			AssertEquals("Calculator's Units", "HR", calculatorForTest.RateLineItems[2].TM_BreakWeightVolume);
			AssertEquals("Calculator's ZoneName", "Zone", Factory.Load<RateTransportZone>(calculatorForTest.RateLineItems[2].TM_TZ_DomesticZone).TZ_ZoneName);

			#endregion

			#region Test With All Parameters (Specified =  true)

			rateItemXSDPlus1.FlatAmount = 10.2m;
			rateItemXSDPlus1.BreakAmount = 5.3m;
			rateItemXSDPlus1.BreakMinimum = 4.2m;
			rateItemXSDPlus1.PerUnit = 20.3m;

			rateItemXSDPlus2.FlatAmount = 11.2m;
			rateItemXSDPlus2.BreakAmount = 6.3m;
			rateItemXSDPlus2.BreakMinimum = 5.2m;
			rateItemXSDPlus2.PerUnit = 21.3m;

			rateItemXSDMinus1.FlatAmount = 10.2m;
			rateItemXSDMinus1.BreakAmount = 5.3m;
			rateItemXSDMinus1.BreakMinimum = 4.2m;
			rateItemXSDMinus1.PerUnit = 20.3m;

			calculatorForTest.RateLineItems.RemoveAndDeleteAll();
			RateCalculatorGenerator.ImportRateItemWithOperatorAndBreakForTest(calculatorForTest, rateItemsXSD, collection[1], context);

			AssertEquals("Calculator RateLineItems Count", 3, calculatorForTest.RateLineItems.Count);

			AssertEquals("Calculator's Operator", new ZString("-"), calculatorForTest.RateLineItems[0].TM_Type);
			AssertEquals("Calculator's FlatAmount", 10.2m, calculatorForTest.RateLineItems[0].TM_FlatAmount);
			AssertEquals("Calculator's BreakAmount", 5.3m, calculatorForTest.RateLineItems[0].TM_Break);
			AssertEquals("Calculator's BreakMinimum", 4.2m, calculatorForTest.RateLineItems[0].TM_BreakMinimum);
			AssertEquals("Calculator's PerUnit", 20.3m, calculatorForTest.RateLineItems[0].TM_RelevantValue);
			AssertEquals("Calculator's Units", "HR", calculatorForTest.RateLineItems[0].TM_BreakWeightVolume);
			AssertEquals("Calculator's ZoneName", "Zone", Factory.Load<RateTransportZone>(calculatorForTest.RateLineItems[0].TM_TZ_DomesticZone).TZ_ZoneName);

			AssertEquals("Calculator's Operator", new ZString("+"), calculatorForTest.RateLineItems[1].TM_Type);
			AssertEquals("Calculator's FlatAmount", 10.2m, calculatorForTest.RateLineItems[1].TM_FlatAmount);
			AssertEquals("Calculator's BreakAmount", 5.3m, calculatorForTest.RateLineItems[1].TM_Break);
			AssertEquals("Calculator's BreakMinimum", 4.2m, calculatorForTest.RateLineItems[1].TM_BreakMinimum);
			AssertEquals("Calculator's PerUnit", 20.3m, calculatorForTest.RateLineItems[1].TM_RelevantValue);
			AssertEquals("Calculator's Units", "HR", calculatorForTest.RateLineItems[1].TM_BreakWeightVolume);
			AssertEquals("Calculator's ZoneName", "Zone", Factory.Load<RateTransportZone>(calculatorForTest.RateLineItems[1].TM_TZ_DomesticZone).TZ_ZoneName);

			AssertEquals("Calculator's Operator", new ZString("+"), calculatorForTest.RateLineItems[2].TM_Type);
			AssertEquals("Calculator's FlatAmount", 11.2m, calculatorForTest.RateLineItems[2].TM_FlatAmount);
			AssertEquals("Calculator's BreakAmount", 6.3m, calculatorForTest.RateLineItems[2].TM_Break);
			AssertEquals("Calculator's BreakMinimum", 5.2m, calculatorForTest.RateLineItems[2].TM_BreakMinimum);
			AssertEquals("Calculator's PerUnit", 21.3m, calculatorForTest.RateLineItems[2].TM_RelevantValue);
			AssertEquals("Calculator's Units", "HR", calculatorForTest.RateLineItems[2].TM_BreakWeightVolume);
			AssertEquals("Calculator's ZoneName", "Zone", Factory.Load<RateTransportZone>(calculatorForTest.RateLineItems[2].TM_TZ_DomesticZone).TZ_ZoneName);

			#endregion

			#region Test With 2 minuses

			Xsd.RateItemWithOperatorAndBreak rateItemXSDMinus2 = rateItemsXSD.AddNew();
			rateItemXSDMinus2.Operator = new ZString("-");
			rateItemXSDMinus2.FlatAmount = 3.2m;
			rateItemXSDMinus2.BreakAmount = 5.3m;
			rateItemXSDMinus2.BreakMinimum = 6.2m;
			rateItemXSDMinus2.PerUnit = 11.3m;
			rateItemXSDMinus2.Units = "HR";

			calculatorForTest.RateLineItems.RemoveAndDeleteAll();
			RateCalculatorGenerator.ImportRateItemWithOperatorAndBreakForTest(calculatorForTest, rateItemsXSD, collection[1], context);

			Assert(Buffer.HasErrors);
			AssertContainsErrorMesg("You cannot have more than one '-' in same rate", Buffer);

			#endregion
		}

		public void TestImportRateItemWithDescAmtOrRate()
		{
			var rateItemsXSD = new Xsd.RateItemWithDescAmtOrRateCollection();

			Xsd.RateItemWithDescAmtOrRate rateItemXSD1 = rateItemsXSD.AddNew();
			rateItemXSD1.Code = "TRK";
			rateItemXSD1.Description = "Truck";
			rateItemXSD1.Units = "HR";

			Xsd.RateItemWithDescAmtOrRate rateItemXSD2 = rateItemsXSD.AddNew();
			rateItemXSD2.Code = "LDD";
			rateItemXSD2.Description = "45-foot Tautliner Drop Deck";
			rateItemXSD2.Units = "DY";

			Xsd.RateItemWithDescAmtOrRate rateItemXSD3 = rateItemsXSD.AddNew();
			rateItemXSD3.Description = "This is a very descriptive description that some person has very inconsiderately imported";
			rateItemXSD3.Code = "TRK";
			rateItemXSD1.Units = "HR";

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var line = rateEntry.RateLines.AddNew();
			line.TL_RateCalculator = EquipmentHireCalculator.Code;
			var calculatorForTest = (EquipmentHireCalculator)line.Calculator;

			Buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, Buffer);

			#region Test With All Parameters (Specified =  false)

			calculatorForTest.RateLineItems.RemoveAndDeleteAll();
			RateCalculatorGenerator.ImportRateItemWithDescAmtOrRateForTest(calculatorForTest, rateItemsXSD, false, context);

			AssertEquals("Calculator RateLineItems Count", 3, calculatorForTest.RateLineItems.Count);

			AssertEquals("Calculator's Code", "TRK", calculatorForTest.RateLineItems[0].TM_Type);
			AssertEquals("Calculator's Description", "Truck", calculatorForTest.RateLineItems[0].TM_Text);
			AssertEquals("Calculator's Amount", 0m, calculatorForTest.RateLineItems[0].TM_RelevantValue);
			AssertEquals("Calculator's Units", "HR", calculatorForTest.RateLineItems[0].TM_BreakWeightVolume);

			AssertEquals("Calculator's Code", "LDD", calculatorForTest.RateLineItems[1].TM_Type);
			AssertEquals("Calculator's Description", "45-foot Tautliner Drop Deck", calculatorForTest.RateLineItems[1].TM_Text);
			AssertEquals("Calculator's Amount", 0m, calculatorForTest.RateLineItems[1].TM_RelevantValue);
			AssertEquals("Calculator's Units", "DY", calculatorForTest.RateLineItems[1].TM_BreakWeightVolume);

			#endregion

			#region Test With All Parameters (Specified =  true)

			rateItemXSD1.Amount = 12.3m;
			rateItemXSD1.Rate = 5.2m;
			rateItemXSD2.Amount = 15.3m;
			rateItemXSD2.Rate = 7.2m;

			calculatorForTest.RateLineItems.RemoveAndDeleteAll();
			RateCalculatorGenerator.ImportRateItemWithDescAmtOrRateForTest(calculatorForTest, rateItemsXSD, false, context);

			AssertEquals("Calculator RateLineItems Count", 3, calculatorForTest.RateLineItems.Count);

			AssertEquals("Calculator's Code", "TRK", calculatorForTest.RateLineItems[0].TM_Type);
			AssertEquals("Calculator's Description", "Truck", calculatorForTest.RateLineItems[0].TM_Text);
			AssertEquals("Calculator's Amount", 12.3m, calculatorForTest.RateLineItems[0].TM_RelevantValue);
			AssertEquals("Calculator's Units", "HR", calculatorForTest.RateLineItems[0].TM_BreakWeightVolume);

			AssertEquals("Calculator's Code", "LDD", calculatorForTest.RateLineItems[1].TM_Type);
			AssertEquals("Calculator's Description", "45-foot Tautliner Drop Deck", calculatorForTest.RateLineItems[1].TM_Text);
			AssertEquals("Calculator's Amount", 15.3m, calculatorForTest.RateLineItems[1].TM_RelevantValue);
			AssertEquals("Calculator's Units", "DY", calculatorForTest.RateLineItems[1].TM_BreakWeightVolume);

			calculatorForTest.RateLineItems.RemoveAndDeleteAll();
			RateCalculatorGenerator.ImportRateItemWithDescAmtOrRateForTest(calculatorForTest, rateItemsXSD, true, context);

			AssertEquals("Calculator RateLineItems Count", 3, calculatorForTest.RateLineItems.Count);

			AssertEquals("Calculator's Code", "TRK", calculatorForTest.RateLineItems[0].TM_Type);
			AssertEquals("Calculator's Description", "Truck", calculatorForTest.RateLineItems[0].TM_Text);
			AssertEquals("Calculator's Amount", 5.2m, calculatorForTest.RateLineItems[0].TM_RelevantValue);
			AssertEquals("Calculator's Units", "HR", calculatorForTest.RateLineItems[0].TM_BreakWeightVolume);

			AssertEquals("Calculator's Code", "LDD", calculatorForTest.RateLineItems[1].TM_Type);
			AssertEquals("Calculator's Description", "45-foot Tautliner Drop Deck", calculatorForTest.RateLineItems[1].TM_Text);
			AssertEquals("Calculator's Amount", 7.2m, calculatorForTest.RateLineItems[1].TM_RelevantValue);
			AssertEquals("Calculator's Units", "DY", calculatorForTest.RateLineItems[1].TM_BreakWeightVolume);

			#endregion

			AssertEquals(context.LastNotificationMessage, string.Format("'{0}' exceeds the max length that is allowed for the field, Text. System has set '{1}' to the field instead.",
				rateItemXSD3.Description,
				rateItemXSD3.Description.Left(line.Calculator.RateLineItems.AddNew().TM_TextInfo.MaxLength)));
		}

		public void TestImportBaseOrMinimumRateOnCalculator()
		{
			var simpleRateXSD = new Xsd.CalculatorSimpleRate();

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var line = rateEntry.RateLines.AddNew();
			line.TL_RateCalculator = CombinedCalculator.Code;
			var calculatorForTest = (CombinedCalculator)line.Calculator;

			Buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, Buffer);

			#region Test With All Parameters (Specified =  false)

			RateCalculatorGenerator.ImportBaseOrMinimumRateOnCalculatorForTest(calculatorForTest, simpleRateXSD, context);

			AssertEquals("Calculator 's BaseRate", 0m, calculatorForTest.BaseRate);
			AssertEquals("Calculator 's Minimum", 0m, calculatorForTest.Minimum);

			#endregion

			#region Test With All Parameters (Specified =  true)

			simpleRateXSD.BaseRate = 1.1m;
			simpleRateXSD.Minimum = 3.6m;

			RateCalculatorGenerator.ImportBaseOrMinimumRateOnCalculatorForTest(calculatorForTest, simpleRateXSD, context);

			AssertEquals("Calculator 's BaseRate", 1.1m, calculatorForTest.BaseRate);
			AssertEquals("Calculator 's Minimum", 3.6m, calculatorForTest.Minimum);

			#endregion
		}

		public void TestImportMaximumRateOnCalculator()
		{
			var simpleRateXSD = new Xsd.CalculatorSimpleRate();
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var line = rateEntry.RateLines.AddNew();
			line.TL_RateCalculator = CombinedCalculator.Code;
			var calculatorForTest = (CombinedCalculator)line.Calculator;

			Buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, Buffer);

			#region Test With All Parameters (Specified =  false)

			RateCalculatorGenerator.ImportMaximumRateOnCalculatorForTest(calculatorForTest, simpleRateXSD, context);

			AssertEquals("Calculator 's Maximum", 0m, calculatorForTest.Maximum);

			#endregion

			#region Test With All Parameters (Specified =  true)

			simpleRateXSD.Maximum = 3.6m;

			RateCalculatorGenerator.ImportMaximumRateOnCalculatorForTest(calculatorForTest, simpleRateXSD, context);

			AssertEquals("Calculator 's Maximum", 3.6m, calculatorForTest.Maximum);

			#endregion
		}

		public void TestImportPerUnitRateOnCalculator()
		{
			var simpleRateXSD = new Xsd.CalculatorSimpleRate();
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var line = rateEntry.RateLines.AddNew();
			line.TL_RateCalculator = CombinedCalculator.Code;
			var calculatorForTest = (CombinedCalculator)line.Calculator;

			Buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, Buffer);

			#region Test With All Parameters (Specified =  false)

			RateCalculatorGenerator.ImportPerUnitRateOnCalculatorForTest(calculatorForTest, simpleRateXSD, context);

			AssertEquals("Calculator 's PerUnit", 0m, calculatorForTest.PerUnit);

			#endregion

			#region Test With All Parameters (Specified =  true)

			simpleRateXSD.PerUnit = 3.6m;

			RateCalculatorGenerator.ImportPerUnitRateOnCalculatorForTest(calculatorForTest, simpleRateXSD, context);

			AssertEquals("Calculator 's PerUnit", 3.6m, calculatorForTest.PerUnit);

			#endregion
		}

		#endregion

		#region Export

		public void TestExportRateItemWithOperatorAndBreak()
		{
			var rateItemsXSD = new Xsd.RateItemWithOperatorAndBreakCollection();

			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("DST", "AIR", "", "AUSYD");
			var rateLine = rateEntry.AddRateLine("FRT", "", "M3", "AUD");
			rateLine.TL_RateCalculator = CombinedCalculator.Code;

			var collection = new CartageZoneCollection(rateLine);
			collection.Load();
			var calculatorForTest = (CombinedCalculator)rateLine.Calculator;
			var notificationBuffer = new NotificationBuffer();

			#region Test Without RateItems

			rateItemsXSD = new Xsd.RateItemWithOperatorAndBreakCollection();
			RateCalculatorGenerator.ExportRateItemWithOperatorAndBreakForTest(calculatorForTest, rateItemsXSD, collection[0], notificationBuffer);

			AssertEquals("Calculator RateLineItems Count", 0, rateItemsXSD.Count);

			#endregion

			#region Test With Min Only

			rateItemsXSD = new Xsd.RateItemWithOperatorAndBreakCollection();

			RateLineItem rateItem;
			rateItem = calculatorForTest.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("MIN");
			rateItem.TM_RelevantValue = 100m;
			RateCalculatorGenerator.ExportRateItemWithOperatorAndBreakForTest(calculatorForTest, rateItemsXSD, collection[0], notificationBuffer);

			AssertEquals(false, notificationBuffer.HasErrors);

			calculatorForTest.RateLineItems.RemoveAndDelete(rateItem);

			#endregion

			rateItem = calculatorForTest.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_FlatAmount = 10.2m;
			rateItem.TM_Break = 5.3m;
			rateItem.TM_BreakMinimum = 4.2m;
			rateItem.TM_RelevantValue = 20.3m;
			rateItem.TM_BreakWeightVolume = "HR";

			rateItem = calculatorForTest.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_FlatAmount = 11.2m;
			rateItem.TM_Break = 6.3m;
			rateItem.TM_BreakMinimum = 5.2m;
			rateItem.TM_RelevantValue = 21.3m;
			rateItem.TM_BreakWeightVolume = "HR";

			rateItemsXSD = new Xsd.RateItemWithOperatorAndBreakCollection();
			RateCalculatorGenerator.ExportRateItemWithOperatorAndBreakForTest(calculatorForTest, rateItemsXSD, collection[0], notificationBuffer);

			Assert(!notificationBuffer.HasErrors);

			rateItem = calculatorForTest.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("-");
			rateItem.TM_FlatAmount = 10.2m;
			rateItem.TM_Break = 5.3m;
			rateItem.TM_BreakMinimum = 4.2m;
			rateItem.TM_RelevantValue = 20.3m;
			rateItem.TM_BreakWeightVolume = "HR";

			#region Test With All Parameters (check Specified)

			rateItemsXSD = new Xsd.RateItemWithOperatorAndBreakCollection();
			RateCalculatorGenerator.ExportRateItemWithOperatorAndBreakForTest(calculatorForTest, rateItemsXSD, collection[0], notificationBuffer);

			AssertEquals("Calculator RateLineItems Count", 3, rateItemsXSD.Count);

			AssertEquals("Calculator's Operator", new ZString("-"), rateItemsXSD[0].Operator);
			AssertEquals("Calculator's FlatAmount", 10.2m, rateItemsXSD[0].FlatAmount);
			AssertEquals("Calculator's FlatAmount Specified", true, rateItemsXSD[0].FlatAmountSpecified);
			AssertEquals("Calculator's BreakAmount", 5.3m, rateItemsXSD[0].BreakAmount);
			AssertEquals("Calculator's BreakAmount Specified", true, rateItemsXSD[0].BreakAmountSpecified);
			AssertEquals("Calculator's BreakMinimum", 4.2m, rateItemsXSD[0].BreakMinimum);
			AssertEquals("Calculator's BreakMinimum Specified", true, rateItemsXSD[0].BreakMinimumSpecified);
			AssertEquals("Calculator's PerUnit", 20.3m, rateItemsXSD[0].PerUnit);
			AssertEquals("Calculator's PerUnit Specified", true, rateItemsXSD[0].PerUnitSpecified);
			AssertEquals("Calculator's Units", "HR", rateItemsXSD[0].Units);

			AssertEquals("Calculator's Operator", new ZString("+"), rateItemsXSD[1].Operator);
			AssertEquals("Calculator's FlatAmount", 10.2m, rateItemsXSD[1].FlatAmount);
			AssertEquals("Calculator's FlatAmount Specified", true, rateItemsXSD[1].FlatAmountSpecified);
			AssertEquals("Calculator's BreakAmount", 5.3m, rateItemsXSD[1].BreakAmount);
			AssertEquals("Calculator's BreakAmount Specified", true, rateItemsXSD[1].BreakAmountSpecified);
			AssertEquals("Calculator's BreakMinimum", 4.2m, rateItemsXSD[1].BreakMinimum);
			AssertEquals("Calculator's BreakMinimum Specified", true, rateItemsXSD[1].BreakMinimumSpecified);
			AssertEquals("Calculator's PerUnit", 20.3m, rateItemsXSD[1].PerUnit);
			AssertEquals("Calculator's PerUnit Specified", true, rateItemsXSD[1].PerUnitSpecified);
			AssertEquals("Calculator's Units", "HR", rateItemsXSD[1].Units);

			AssertEquals("Calculator's Operator", new ZString("+"), rateItemsXSD[2].Operator);
			AssertEquals("Calculator's FlatAmount", 11.2m, rateItemsXSD[2].FlatAmount);
			AssertEquals("Calculator's FlatAmount Specified", true, rateItemsXSD[2].FlatAmountSpecified);
			AssertEquals("Calculator's BreakAmount", 6.3m, rateItemsXSD[2].BreakAmount);
			AssertEquals("Calculator's BreakAmount Specified", true, rateItemsXSD[2].BreakAmountSpecified);
			AssertEquals("Calculator's BreakMinimum", 5.2m, rateItemsXSD[2].BreakMinimum);
			AssertEquals("Calculator's BreakMinimum Specified", true, rateItemsXSD[2].BreakMinimumSpecified);
			AssertEquals("Calculator's PerUnit", 21.3m, rateItemsXSD[2].PerUnit);
			AssertEquals("Calculator's PerUnit Specified", true, rateItemsXSD[2].PerUnitSpecified);
			AssertEquals("Calculator's Units", "HR", rateItemsXSD[2].Units);

			#endregion

			#region Test With 2 minuses

			rateItem = calculatorForTest.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("-");
			rateItem.TM_FlatAmount = 3.2m;
			rateItem.TM_Break = 5.3m;
			rateItem.TM_BreakMinimum = 6.2m;
			rateItem.TM_RelevantValue = 11.3m;
			rateItem.TM_BreakWeightVolume = "HR";

			rateItemsXSD = new Xsd.RateItemWithOperatorAndBreakCollection();
			RateCalculatorGenerator.ExportRateItemWithOperatorAndBreakForTest(calculatorForTest, rateItemsXSD, collection[0], notificationBuffer);

			Assert(notificationBuffer.HasErrors);
			AssertContainsErrorMesg("You cannot have more than one '-' in same rate", notificationBuffer);

			#endregion
		}

		public void TestExportRateItemWithDescAmtOrRate()
		{
			var rateItemsXSD = new Xsd.RateItemWithDescAmtOrRateCollection();

			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("DST", "AIR", "", "AUSYD");
			var rateLine = rateEntry.AddRateLine("FRT", "", "M3", "AUD");
			rateLine.TL_RateCalculator = EquipmentHireCalculator.Code;
			var calculatorForTest = (EquipmentHireCalculator)rateLine.Calculator;
			var notificationBuffer = new NotificationBuffer();

			var rateItem = calculatorForTest.RateLineItems.AddNew();
			rateItem.TM_Type = "TRK";
			rateItem.TM_Text = "Truck";
			rateItem.TM_RelevantValue = 12.3m;
			rateItem.TM_BreakWeightVolume = "HR";

			rateItem = calculatorForTest.RateLineItems.AddNew();
			rateItem.TM_Type = "LDD";
			rateItem.TM_Text = "45-foot Tautliner Drop Deck";
			rateItem.TM_RelevantValue = 15.3m;
			rateItem.TM_BreakWeightVolume = "DY";

			#region Test With All Parameters (check Specified)

			rateItemsXSD = new Xsd.RateItemWithDescAmtOrRateCollection();
			RateCalculatorGenerator.ExportRateItemWithDescAmtOrRateForTest(calculatorForTest, rateItemsXSD, false, notificationBuffer);

			AssertEquals("Calculator RateLineItems Count", 2, rateItemsXSD.Count);

			AssertEquals("Calculator's Code", "TRK", rateItemsXSD[0].Code);
			AssertEquals("Calculator's Description", "Truck", rateItemsXSD[0].Description);
			AssertEquals("Calculator's Amount", 12.3m, rateItemsXSD[0].Amount);
			AssertEquals("Calculator's Amount Specified", true, rateItemsXSD[0].AmountSpecified);
			AssertEquals("Calculator's Rate", 0m, rateItemsXSD[0].Rate);
			AssertEquals("Calculator's Rate Specified", false, rateItemsXSD[0].RateSpecified);
			AssertEquals("Calculator's Units", "HR", rateItemsXSD[0].Units);

			AssertEquals("Calculator's Code", "LDD", rateItemsXSD[1].Code);
			AssertEquals("Calculator's Description", "45-foot Tautliner Drop Deck", rateItemsXSD[1].Description);
			AssertEquals("Calculator's Amount", 15.3m, rateItemsXSD[1].Amount);
			AssertEquals("Calculator's Amount Specified", true, rateItemsXSD[0].AmountSpecified);
			AssertEquals("Calculator's Rate", 0m, rateItemsXSD[1].Rate);
			AssertEquals("Calculator's Rate Specified", false, rateItemsXSD[0].RateSpecified);
			AssertEquals("Calculator's Units", "DY", rateItemsXSD[1].Units);

			rateItemsXSD = new Xsd.RateItemWithDescAmtOrRateCollection();
			RateCalculatorGenerator.ExportRateItemWithDescAmtOrRateForTest(calculatorForTest, rateItemsXSD, true, notificationBuffer);

			AssertEquals("Calculator RateLineItems Count", 2, rateItemsXSD.Count);

			AssertEquals("Calculator's Code", "TRK", rateItemsXSD[0].Code);
			AssertEquals("Calculator's Description", "Truck", rateItemsXSD[0].Description);
			AssertEquals("Calculator's Amount", 0m, rateItemsXSD[0].Amount);
			AssertEquals("Calculator's Amount Specified", false, rateItemsXSD[0].AmountSpecified);
			AssertEquals("Calculator's Rate", 12.3m, rateItemsXSD[0].Rate);
			AssertEquals("Calculator's Rate Specified", true, rateItemsXSD[0].RateSpecified);
			AssertEquals("Calculator's Units", "HR", rateItemsXSD[0].Units);

			AssertEquals("Calculator's Code", "LDD", rateItemsXSD[1].Code);
			AssertEquals("Calculator's Description", "45-foot Tautliner Drop Deck", rateItemsXSD[1].Description);
			AssertEquals("Calculator's Amount", 0m, rateItemsXSD[1].Amount);
			AssertEquals("Calculator's Amount Specified", false, rateItemsXSD[0].AmountSpecified);
			AssertEquals("Calculator's Rate", 15.3m, rateItemsXSD[1].Rate);
			AssertEquals("Calculator's Rate Specified", true, rateItemsXSD[0].RateSpecified);
			AssertEquals("Calculator's Units", "DY", rateItemsXSD[1].Units);

			#endregion
		}

		public void TestExportBaseOrMinimumRateOnCalculator()
		{
			var simpleRateXSD = new Xsd.CalculatorSimpleRate();
			var notificationBuffer = new NotificationBuffer();
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var line = rateEntry.RateLines.AddNew();
			line.TL_RateCalculator = CombinedCalculator.Code;
			var calculatorForTest = (CombinedCalculator)line.Calculator;

			calculatorForTest.BaseRate = 1.1m;
			calculatorForTest.Minimum = 3.6m;

			#region Test With All Parameters (check Specified)

			RateCalculatorGenerator.ExportBaseOrMinimumRateOnCalculatorForTest(calculatorForTest, simpleRateXSD, notificationBuffer);

			AssertEquals("Calculator's BaseRate", 1.1m, simpleRateXSD.BaseRate);
			AssertEquals("Calculator's BaseRate Specified", true, simpleRateXSD.BaseRateSpecified);
			AssertEquals("Calculator's Minimum", 3.6m, simpleRateXSD.Minimum);
			AssertEquals("Calculator's Minimum Specified", true, simpleRateXSD.MinimumSpecified);

			#endregion
		}

		public void TestExportMaximumRateOnCalculator()
		{
			var simpleRateXSD = new Xsd.CalculatorSimpleRate();
			var notificationBuffer = new NotificationBuffer();
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var line = rateEntry.RateLines.AddNew();
			line.TL_RateCalculator = CombinedCalculator.Code;
			var calculatorForTest = (CombinedCalculator)line.Calculator;

			calculatorForTest.Maximum = 3.6m;

			#region Test With All Parameters (check Specified)

			RateCalculatorGenerator.ExportMaximumRateOnCalculatorForTest(calculatorForTest, simpleRateXSD, notificationBuffer);

			AssertEquals("Calculator's Maximum", 3.6m, simpleRateXSD.Maximum);
			AssertEquals("Calculator's Maximum Specified", true, simpleRateXSD.MaximumSpecified);

			#endregion
		}

		public void TestExportPerUnitRateOnCalculator()
		{
			var simpleRateXSD = new Xsd.CalculatorSimpleRate();
			var notificationBuffer = new NotificationBuffer();
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var line = rateEntry.RateLines.AddNew();
			line.TL_RateCalculator = CombinedCalculator.Code;
			var calculatorForTest = (CombinedCalculator)line.Calculator;

			calculatorForTest.PerUnit = 3.6m;

			#region Test With All Parameters (check Specified)

			RateCalculatorGenerator.ExportPerUnitRateOnCalculatorForTest(calculatorForTest, simpleRateXSD, notificationBuffer);

			AssertEquals("Calculator's PerUnit", 3.6m, simpleRateXSD.PerUnit);
			AssertEquals("Calculator's PerUnit Specified", true, simpleRateXSD.PerUnitSpecified);

			#endregion
		}

		#endregion

		public void TestGeneratorType()
		{
			AssertEquals("Generator Type", ExpectedGeneratorType, CalculatorGenerator.GetType());
		}

		public void TestSchemaShouldnotBeNull()
		{
			AssertNotNull("Schema shouldn't be null", CalculatorGenerator.Schema);
		}

		#endregion

		#region Virtual Members

		protected virtual ZString ExpectedRateItemType
		{
			get { return ZString.Empty; }
		}

		protected virtual ZString ExpectedRateItemUnit
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Abstract Members

		protected abstract Type ExpectedGeneratorType { get; }
		protected abstract TValueObject CalculatorXSDForTest(bool specified);
		protected abstract void SetTestDataCalculatorForTest(TCalculator calculator);
		protected abstract void ImportFromValueObjectCore();
		protected abstract void ExportToValueObjectCore();
		protected abstract RateCalculatorGeneratorFromXSD<TCalculator, TValueObject> CalculatorGenerator { get; }
		protected abstract ZString CalculatorType { get; }

		#endregion

		#region Fields

		protected TValueObject calculatorXSD;
		protected NotificationBuffer Buffer;

		#endregion

		#region Assert Methods

		protected bool AssertContain(Xsd.RateItemWithOperatorAndBreak rateItemsXSD)
		{
			bool result = false;

			foreach (RateLineItem item in CalculatorGenerated.RateLineItems)
			{
				ZString rateOperator = rateItemsXSD.Operator.ToString();
				rateOperator = (rateOperator == "MINUS") ? new ZString("-") : rateOperator;
				rateOperator = (rateOperator == "PLUS") ? new ZString("+") : rateOperator;

				if (item.TM_FlatAmount == rateItemsXSD.FlatAmount
					&& item.TM_BreakMinimum == rateItemsXSD.BreakMinimum
					&& item.TM_RelevantValue == rateItemsXSD.PerUnit)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		#endregion

		#region Data For Test

		RateCalculatorGeneratorFromXSDForTest RateCalculatorGenerator
		{
			get
			{
				if (rateCalculatorGenerator == null)
				{
					rateCalculatorGenerator = new RateCalculatorGeneratorFromXSDForTest();
				}

				return rateCalculatorGenerator;
			}
		}
		RateCalculatorGeneratorFromXSDForTest rateCalculatorGenerator;

		class RateCalculatorGeneratorFromXSDForTest : RateCalculatorGeneratorFromXSD<Calculator, Xsd.RateCalculator>
		{
			#region Implementation

			public override XmlSchema Schema
			{
				get { return RatingXmlSchemaDefinitions.Instance.RateCalculatorSchema; }
			}

			protected override ZString CalculatorType
			{
				get { return ""; }
			}

			protected override void ImportFromValueObjectCore(RateLine rateLine, Calculator calculator, Xsd.RateCalculator calculatorXSD, IValueObjectImportContext context)
			{
			}

			protected override void ExportToValueObjectCore(RateLine rateLine, Calculator calculator, Xsd.RateCalculator calculatorXSD, INotifications notifications)
			{
			}

			#endregion

			#region Proxy Methods

			#region Import

			public void ImportRateItemWithOperatorAndBreakForTest(Calculator calculator, Xsd.RateItemWithOperatorAndBreakCollection rateItems, CartageZone zone, ValueObjectImportContext context)
			{
				base.ImportRateItemWithOperatorAndBreak(calculator, rateItems, zone, context);
			}

			public void ImportRateItemWithDescAmtOrRateForTest(Calculator calculator, Xsd.RateItemWithDescAmtOrRateCollection rateItemsXSD, bool takeValueFromRate, ValueObjectImportContext context)
			{
				base.ImportRateItemWithDescAmtOrRate(calculator, rateItemsXSD, takeValueFromRate, context);
			}

			public void ImportBaseOrMinimumRateOnCalculatorForTest(Calculator calculator, Xsd.CalculatorSimpleRate baseRate, ValueObjectImportContext context)
			{
				base.ImportBaseOrMinimumRateOnCalculator(calculator, baseRate, context);
			}

			public void ImportMaximumRateOnCalculatorForTest(Calculator calculator, Xsd.CalculatorSimpleRate baseRate, ValueObjectImportContext context)
			{
				base.ImportMaximumRateOnCalculator(calculator, baseRate, context);
			}

			public void ImportPerUnitRateOnCalculatorForTest(Calculator calculator, Xsd.CalculatorSimpleRate baseRate, ValueObjectImportContext context)
			{
				base.ImportPerUnitRateOnCalculator(calculator, baseRate, context);
			}

			#endregion

			#region Export

			public void ExportRateItemWithOperatorAndBreakForTest(Calculator calculator, Xsd.RateItemWithOperatorAndBreakCollection rateItems, CartageZone zone, INotifications notifications)
			{
				base.ExportRateItemWithOperatorAndBreak(calculator, rateItems, zone, notifications);
			}

			public void ExportRateItemWithDescAmtOrRateForTest(Calculator calculator, Xsd.RateItemWithDescAmtOrRateCollection rateItemsXSD, bool setValueToRate, INotifications notifications)
			{
				base.ExportRateItemWithDescAmtOrRate(calculator, rateItemsXSD, setValueToRate, notifications);
			}

			public void ExportBaseOrMinimumRateOnCalculatorForTest(Calculator calculator, Xsd.CalculatorSimpleRate baseRate, INotifications notifications)
			{
				base.ExportBaseOrMinimumRateOnCalculator(calculator, baseRate, notifications);
			}

			public void ExportMaximumRateOnCalculatorForTest(Calculator calculator, Xsd.CalculatorSimpleRate baseRate, INotifications notifications)
			{
				base.ExportMaximumRateOnCalculator(calculator, baseRate, notifications);
			}

			public void ExportPerUnitRateOnCalculatorForTest(Calculator calculator, Xsd.CalculatorSimpleRate baseRate, INotifications notifications)
			{
				base.ExportPerUnitRateOnCalculator(calculator, baseRate, notifications);
			}

			#endregion

			#endregion
		}

		protected RateLine RateLineForCalculatorXSDTest
		{
			get
			{
				if (rateLineForCalculatorXSDTest == null)
				{
					var tariff = Factory.New<CompanyTariff>();
					RateEntry entry = tariff.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
					rateLineForCalculatorXSDTest = entry.RateLines.AddNew();
					rateLineForCalculatorXSDTest.TL_AC = Env.Registry.FreightChargeCode;
					rateLineForCalculatorXSDTest.TL_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
					rateLineForCalculatorXSDTest.TL_WeightVolume = Core.Constants.Weight.Kilograms;
					rateLineForCalculatorXSDTest.TL_WeightVolumeMultiple = 2.34m;
					rateLineForCalculatorXSDTest.TL_RateCalculator = CalculatorType;
				}

				return rateLineForCalculatorXSDTest;
			}
		}
		RateLine rateLineForCalculatorXSDTest;

		protected RateLine RateLineForCalculatorTest
		{
			get
			{
				if (rateLineForCalculatorTest == null)
				{
					var tariff = Factory.New<CompanyTariff>();
					RateEntry entry = tariff.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
					rateLineForCalculatorTest = entry.RateLines.AddNew();
					rateLineForCalculatorTest.TL_AC = Env.Registry.FreightChargeCode;
					rateLineForCalculatorTest.TL_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
					rateLineForCalculatorTest.TL_WeightVolume = Core.Constants.Weight.Kilograms;
					rateLineForCalculatorTest.TL_WeightVolumeMultiple = 2.34m;
					rateLineForCalculatorTest.TL_RateCalculator = CalculatorType;

					SetTestDataCalculatorForTest((TCalculator)RateLineForCalculatorTest.Calculator);
				}

				return rateLineForCalculatorTest;
			}
		}
		RateLine rateLineForCalculatorTest;

		protected TCalculator CalculatorGenerated
		{
			get { return (TCalculator)RateLineForCalculatorXSDTest.Calculator; }
		}

		protected Xsd.RateItemWithOperatorAndBreak GetNewRateLineItem(ZString lineOperator, ZDecimal amount)
		{
			Xsd.RateItemWithOperatorAndBreak item = new Xsd.RateItemWithOperatorAndBreak();
			item.Operator = lineOperator;
			item.FlatAmount = amount;
			return item;
		}

		protected Xsd.RateItemWithOperatorAndBreakCollection RateLineItemsWithMissingMinus
		{
			get
			{
				Xsd.RateItemWithOperatorAndBreakCollection rateItems = new Xsd.RateItemWithOperatorAndBreakCollection();
				rateItems.Add(GetNewRateLineItem(new ZString("+"), 123m));
				rateItems.Add(GetNewRateLineItem(new ZString("+"), 13.34m));
				return rateItems;
			}
		}

		protected Xsd.RateItemWithOperatorAndBreakCollection RateLineItemsWithTooManyMinus
		{
			get
			{
				Xsd.RateItemWithOperatorAndBreakCollection rateItems = new Xsd.RateItemWithOperatorAndBreakCollection();
				rateItems.Add(GetNewRateLineItem(new ZString("-"), 123m));
				rateItems.Add(GetNewRateLineItem(new ZString("-"), 9.8m));
				rateItems.Add(GetNewRateLineItem(new ZString("+"), 13.34m));
				return rateItems;
			}
		}

		#endregion
	}
}
