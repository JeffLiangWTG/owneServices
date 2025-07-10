using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class CTBCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<CompanyTariffOrCostBasedCalculator, Xsd.CTBCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.CTBCalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			#region Test (Specified = false)

			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator is an Agency Caluclator", typeof(CompanyTariffOrCostBasedCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());

			AssertEquals("Calculator 's Percent", 0m, CalculatorGenerated.Percent);
			AssertEquals("Calculator 's CalculationOrder", "UP", CalculatorGenerated.CalculationOrder);
			AssertEquals("Calculator 's EquipmentType", "T", CalculatorGenerated.EquipmentType);
			AssertEquals("Calculator 's Type", "T1", CalculatorGenerated.MessageType);
			AssertEquals("Calculator 's Style", "S1", CalculatorGenerated.MessageSubType);

			AssertEquals("Calculator RateLineItems Count", 3, CalculatorGenerated.RateLineItems.Count);

			AssertEquals("Calculator 's BaseRate", 0m, CalculatorGenerated.BaseRate);
			AssertEquals("Calculator 's Minimum", 0m, CalculatorGenerated.Minimum);
			AssertEquals("Calculator 's PerUnit", 0m, CalculatorGenerated.PerUnit);

			#endregion

			#region Test (Specified = true)

			calculatorXSD = CalculatorXSDForTest(true);
			calculatorXSD.SimpleRate.PerUnitSpecified = false;

			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator 's Percent", 0m, CalculatorGenerated.Percent);
			AssertEquals("Calculator 's CalculationOrder", "UP", CalculatorGenerated.CalculationOrder);
			AssertEquals("Calculator 's EquipmentType", "T", CalculatorGenerated.EquipmentType);
			AssertEquals("Calculator 's Type", "T1", CalculatorGenerated.MessageType);
			AssertEquals("Calculator 's Style", "S1", CalculatorGenerated.MessageSubType);

			AssertEquals("Calculator RateLineItems Count", 3, CalculatorGenerated.RateLineItems.Count);

			AssertEquals("Calculator 's BaseRate", 1.1m, CalculatorGenerated.BaseRate);
			AssertEquals("Calculator 's Minimum", 3.6m, CalculatorGenerated.Minimum);
			AssertEquals("Calculator 's PerUnit", 0m, CalculatorGenerated.PerUnit);

			calculatorXSD.SimpleRate.PerUnitSpecified = true;
			calculatorXSD.RateItems = new Xsd.RateItemWithOperatorAndBreakCollection();

			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator 's Percent", 12.3m, CalculatorGenerated.Percent);
			AssertEquals("Calculator 's CalculationOrder", "UP", CalculatorGenerated.CalculationOrder);
			AssertEquals("Calculator 's EquipmentType", "T", CalculatorGenerated.EquipmentType);
			AssertEquals("Calculator 's Type", "T1", CalculatorGenerated.MessageType);
			AssertEquals("Calculator 's Style", "S1", CalculatorGenerated.MessageSubType);

			AssertEquals("Calculator RateLineItems Count", 0, CalculatorGenerated.RateLineItems.Count);

			AssertEquals("Calculator 's BaseRate", 1.1m, CalculatorGenerated.BaseRate);
			AssertEquals("Calculator 's Minimum", 3.6m, CalculatorGenerated.Minimum);
			AssertEquals("Calculator 's PerUnit", 3.6m, CalculatorGenerated.PerUnit);

			#endregion
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Xsd.CTBCalculator calculatorXSD = new Xsd.CTBCalculator();

			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator 's Percent", 12.3m, calculatorXSD.Percent);
			AssertEquals("Calculator 's Percent Specified", true, calculatorXSD.PercentSpecified);
			AssertEquals("Calculator 's CalculationOrder", "UP", calculatorXSD.CalculationOrder);
			AssertEquals("Calculator 's EquipmentType", "T", calculatorXSD.EquipmentType);
			AssertEquals("Calculator 's Type", "T1", calculatorXSD.Type);
			AssertEquals("Calculator 's Style", "S1", calculatorXSD.Style);

			AssertEquals("Calculator RateLineItems Count", 0, calculatorXSD.RateItems.Count);

			AssertEquals("Calculator 's BaseRate", 1.1m, calculatorXSD.SimpleRate.BaseRate);
			AssertEquals("Calculator 's BaseRate Specified", true, calculatorXSD.SimpleRate.BaseRateSpecified);
			AssertEquals("Calculator 's Minimum", 3.6m, calculatorXSD.SimpleRate.Minimum);
			AssertEquals("Calculator 's Minimum Specified", true, calculatorXSD.SimpleRate.MinimumSpecified);
			AssertEquals("Calculator 's PerUnit", 3.6m, calculatorXSD.SimpleRate.PerUnit);
			AssertEquals("Calculator 's PerUnit Specified", true, calculatorXSD.SimpleRate.PerUnitSpecified);

			RateLineItem rateItem;

			rateItem = ((CompanyTariffOrCostBasedCalculator)RateLineForCalculatorTest.Calculator).RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("-");
			rateItem.TM_FlatAmount = 10.2m;
			rateItem.TM_Break = 5.3m;
			rateItem.TM_BreakMinimum = 4.2m;
			rateItem.TM_RelevantValue = 20.3m;
			rateItem.TM_BreakWeightVolume = "HR";

			rateItem = ((CompanyTariffOrCostBasedCalculator)RateLineForCalculatorTest.Calculator).RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_FlatAmount = 10.2m;
			rateItem.TM_Break = 5.3m;
			rateItem.TM_BreakMinimum = 4.2m;
			rateItem.TM_RelevantValue = 20.3m;
			rateItem.TM_BreakWeightVolume = "HR";

			rateItem = ((CompanyTariffOrCostBasedCalculator)RateLineForCalculatorTest.Calculator).RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_FlatAmount = 11.2m;
			rateItem.TM_Break = 6.3m;
			rateItem.TM_BreakMinimum = 5.2m;
			rateItem.TM_RelevantValue = 21.3m;
			rateItem.TM_BreakWeightVolume = "HR";

			((CompanyTariffOrCostBasedCalculator)RateLineForCalculatorTest.Calculator).PerUnit = 0m;
			calculatorXSD = new Xsd.CTBCalculator();
			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator 's Percent", 0m, calculatorXSD.Percent);
			AssertEquals("Calculator 's Percent Specified", false, calculatorXSD.PercentSpecified);
			AssertEquals("Calculator 's CalculationOrder", "UP", calculatorXSD.CalculationOrder);
			AssertEquals("Calculator 's EquipmentType", "T", calculatorXSD.EquipmentType);
			AssertEquals("Calculator 's Type", "T1", calculatorXSD.Type);
			AssertEquals("Calculator 's Style", "S1", calculatorXSD.Style);

			AssertEquals("Calculator RateLineItems Count", 3, calculatorXSD.RateItems.Count);

			AssertEquals("Calculator 's BaseRate", 1.1m, calculatorXSD.SimpleRate.BaseRate);
			AssertEquals("Calculator 's BaseRate Specified", true, calculatorXSD.SimpleRate.BaseRateSpecified);
			AssertEquals("Calculator 's Minimum", 3.6m, calculatorXSD.SimpleRate.Minimum);
			AssertEquals("Calculator 's Minimum Specified", true, calculatorXSD.SimpleRate.MinimumSpecified);
			AssertEquals("Calculator 's PerUnit", 0m, calculatorXSD.SimpleRate.PerUnit);
			AssertEquals("Calculator 's PerUnit Specified", false, calculatorXSD.SimpleRate.PerUnitSpecified);
		}

		#endregion

		#region Override Members

		protected override ZString CalculatorType
		{
			get { return CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(CTBCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<CompanyTariffOrCostBasedCalculator, Xsd.CTBCalculator> CalculatorGenerator
		{
			get { return new CTBCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.CTBCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.CTBCalculator();

			if (specified)
			{
				calculatorXSD.Percent = 12.3;
			}

			calculatorXSD.CalculationOrder = "UP";
			calculatorXSD.EquipmentType = "T";
			calculatorXSD.Type = "T1";
			calculatorXSD.Style = "S1";

			Xsd.RateItemWithOperatorAndBreak rateItemXSD;

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("-");
			rateItemXSD.Units = "HR";
			if (specified)
			{
				rateItemXSD.FlatAmount = 10.2m;
				rateItemXSD.BreakAmount = 5.3m;
				rateItemXSD.BreakMinimum = 4.2m;
				rateItemXSD.PerUnit = 20.3m;
			}

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("+");
			rateItemXSD.Units = "HR";
			if (specified)
			{
				rateItemXSD.FlatAmount = 10.2m;
				rateItemXSD.BreakAmount = 5.3m;
				rateItemXSD.BreakMinimum = 4.2m;
				rateItemXSD.PerUnit = 20.3m;
			}

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("+");
			rateItemXSD.Units = "HR";
			if (specified)
			{
				rateItemXSD.FlatAmount = 11.2m;
				rateItemXSD.BreakAmount = 6.3m;
				rateItemXSD.BreakMinimum = 5.2m;
				rateItemXSD.PerUnit = 21.3m;
			}

			if (specified)
			{
				calculatorXSD.SimpleRate.BaseRate = 1.1m;
				calculatorXSD.SimpleRate.Minimum = 3.6m;
				calculatorXSD.SimpleRate.PerUnit = 3.6m;
			}

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(CompanyTariffOrCostBasedCalculator calculator)
		{
			calculator.CalculationOrder = "UP";
			calculator.EquipmentType = "T";
			calculator.MessageType = "T1";
			calculator.MessageSubType = "S1";
			calculator.Percent = 12.3m;

			calculator.BaseRate = 1.1m;
			calculator.Minimum = 3.6m;
			calculator.PerUnit = 3.6m;
		}

		#endregion
	}
}
