using System;
using System.Globalization;
using System.Threading;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Rating.Integration.Test
{
	internal class CalculationLogTest : TestCase
	{
		public void TestSerialize()
		{
			CalculationLog calculationLog = new CalculationLog();
			AssertEquals("Serialized as expected", EmptyXML, calculationLog.SerializeToString());

			calculationLog = GetPopulatedLog();
			AssertEquals("Serialized as expected", PopulatedXML, calculationLog.SerializeToString());
		}

		public void TestDeserialize()
		{
			CalculationLog calculationLog = new CalculationLog();
			calculationLog.DeserializeFromString(EmptyXML);

			AssertEquals(false, calculationLog.IsCosting);
			AssertEquals("", calculationLog.ChargeCode);
			AssertEquals("", calculationLog.Currency);
			AssertEquals(0m, calculationLog.Weight);
			AssertEquals("", calculationLog.Unit);
			AssertEquals(0m, calculationLog.Chargeable);
			AssertEquals("", calculationLog.ChargeableUnit);
			AssertEquals("", calculationLog.CommodityCode);
			AssertEquals("", calculationLog.RateMode);
			AssertEquals("", calculationLog.ContainerCode);
			AssertEquals(0m, calculationLog.BaseRate);
			AssertEquals(0m, calculationLog.Minimum);
			AssertEquals(0m, calculationLog.Maximum);
			AssertEquals(0, calculationLog.Steps.Count);

			calculationLog.DeserializeFromString(PopulatedXML);
			AssertEquals(true, calculationLog.IsCosting);
			AssertEquals("FLT", calculationLog.CalculatorCode);
			AssertEquals("CHR", calculationLog.ChargeCode);
			AssertEquals("AUD", calculationLog.Currency);
			AssertEquals(64.11m, calculationLog.Weight);
			AssertEquals("KG", calculationLog.Unit);
			AssertEquals(128.22m, calculationLog.Chargeable);
			AssertEquals("LB", calculationLog.ChargeableUnit);
			AssertEquals("COM", calculationLog.CommodityCode);
			AssertEquals("ULD", calculationLog.RateMode);
			AssertEquals("LD-1", calculationLog.ContainerCode);
			AssertEquals(10.33m, calculationLog.BaseRate);
			AssertEquals(20.44m, calculationLog.Minimum);
			AssertEquals(100.55m, calculationLog.Maximum);
			AssertEquals(2, calculationLog.Steps.Count);

			CalculationLog expectedLog = GetPopulatedLog();
			AssertEquals(true, expectedLog.Steps[0].Equals(calculationLog.Steps[0]));
			AssertEquals(true, expectedLog.Steps[1].Equals(calculationLog.Steps[1]));
		}

		public void TestAddCalculationSteps()
		{
			CalculationLog calculationLog = new CalculationLog();
			CalculationStep step = calculationLog.AddCalculationStep();
			AssertNotNull("Calculation step was added", step);
			AssertEquals(1, calculationLog.Steps.Count);

			calculationLog.Steps.Clear();
			step = calculationLog.AddPerUnitCalculation(32m, "KG", 64m);
			AssertEquals(1, calculationLog.Steps.Count);
			AssertEquals(true, step.IsPerUnit);
			AssertEquals(32m, step.UnitCount);
			AssertEquals(64m, step.UnitPrice);
			AssertEquals("KG", step.Unit);
			AssertEquals(32m * 64m, step.Result);

			calculationLog.Steps.Clear();
			step = calculationLog.AddPerContainerUnitCalculation(15m, 55m);
			AssertEquals(1, calculationLog.Steps.Count);
			AssertEquals(true, step.IsPerUnit);
			AssertEquals(15m, step.UnitCount);
			AssertEquals(55m, step.UnitPrice);
			AssertEquals("CN", step.Unit);
			AssertEquals(15m * 55m, step.Result);

			calculationLog.Steps.Clear();
			step = calculationLog.AddPercentageCalculation(100m, 16m);
			AssertEquals(1, calculationLog.Steps.Count);
			AssertEquals(true, step.IsPercentage);
			AssertEquals(100m, step.UnitCount);
			AssertEquals(16m, step.Percentage);
		}

		public void TestAddFlatAmountToLastCalculation()
		{
			CalculationLog calculationLog = new CalculationLog();

			CalculationStep step1 = calculationLog.AddPerUnitCalculation(10m, "CN", 1m);
			AssertEquals(10m, step1.Result);
			AssertEquals(0m, step1.Flat);

			CalculationStep step2 = calculationLog.AddPerUnitCalculation(10m, "CN", 2m);
			AssertEquals(20m, step2.Result);
			AssertEquals(0m, step2.Flat);

			AssertEquals("Precondition", 2, calculationLog.Steps.Count);

			CalculationStep flatStep = calculationLog.AddFlatAmountToLastCalculation(16m);
			AssertEquals("Added to the last per unit step", 2, calculationLog.Steps.Count);
			AssertEquals(20m + 16m, step2.Result);
			AssertEquals(16m, step2.Flat);

			calculationLog.Steps.Clear();
			step1 = calculationLog.AddPercentageCalculation(100m, 16m);
			AssertEquals(1, calculationLog.Steps.Count);

			flatStep = calculationLog.AddFlatAmountToLastCalculation(16m);
			AssertEquals("Created new step as last one wasn't per unit", 2, calculationLog.Steps.Count);
			AssertEquals(16m, flatStep.Result);
			AssertEquals(16m, flatStep.Flat);
		}

		public void TestBooleanCalculatedProperties()
		{
			CalculationLog calculationLog = new CalculationLog();
			AssertBooleanProperties(calculationLog, false, false, false);

			calculationLog.BaseRate = 1m;
			AssertBooleanProperties(calculationLog, true, false, false);

			calculationLog.Minimum = 2m;
			AssertBooleanProperties(calculationLog, true, true, false);

			calculationLog.Maximum = 3m;
			AssertBooleanProperties(calculationLog, true, true, true);
		}

		void AssertBooleanProperties(CalculationLog calculationLog, bool hasBaseRate, bool hasMinimum, bool hasMaximum)
		{
			CombineAssertions(delegate
			{
				AssertEquals(hasBaseRate, calculationLog.HasBaseRate);
				AssertEquals(hasMinimum, calculationLog.HasMinimum);
				AssertEquals(hasMaximum, calculationLog.HasMaximum);
			});
		}

		public void TestIsEmpty()
		{
			CalculationLog calculationLog = new CalculationLog();
			AssertEquals("Empty by default", true, calculationLog.IsEmpty);

			calculationLog.Steps.Add(new CalculationStep());
			AssertEquals("Not empty as has steps", false, calculationLog.IsEmpty);

			calculationLog.Steps.Clear();
			AssertEquals(true, calculationLog.IsEmpty);

			calculationLog.BaseRate = 10m;
			AssertEquals(false, calculationLog.IsEmpty);

			calculationLog.BaseRate = 0m;
			AssertEquals(true, calculationLog.IsEmpty);

			calculationLog.Maximum = 10m;
			AssertEquals(false, calculationLog.IsEmpty);

			calculationLog.Maximum = 0m;
			AssertEquals(true, calculationLog.IsEmpty);

			calculationLog.Minimum = 10m;
			AssertEquals(false, calculationLog.IsEmpty);

			calculationLog.Minimum = 0m;
			AssertEquals(true, calculationLog.IsEmpty);
		}

		public void TestResult()
		{
			CalculationLog calculationLog = new CalculationLog();
			AssertEquals("Sum of step's results", 0m, calculationLog.Result);

			calculationLog.AddCalculationStep().Result = 10m;
			calculationLog.AddCalculationStep().Result = 20m;
			calculationLog.AddCalculationStep().Result = 30m;
			AssertEquals("Sum of step's results", 60m, calculationLog.Result);
		}

		public void TestUseInvariantCultureToStoreNumber()
		{
			var originalCultureInfo = Thread.CurrentThread.CurrentCulture;

			try
			{
				CalculationLog calculationLog = new CalculationLog();

				calculationLog.DeserializeFromString(PopulatedXML);

				Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
				AssertEquals("the decimal point in French is a comma", "64,11", calculationLog.Weight.ToString());
				AssertEquals("the decimal point in French is a comma", "128,22", calculationLog.Chargeable.ToString());
				AssertEquals("the decimal point in French is a comma", "10,33", calculationLog.BaseRate.ToString());
				AssertEquals("the decimal point in French is a comma", "20,44", calculationLog.Minimum.ToString());
				AssertEquals("the decimal point in French is a comma", "100,55", calculationLog.Maximum.ToString());
				AssertEquals("Serialized as expected", PopulatedXML, calculationLog.SerializeToString());
				TestDeserialize();

				var persianCulture = new CultureInfo("fa");
				Thread.CurrentThread.CurrentCulture = persianCulture;

				// Persian separator differs by .NET version: '/' in .NET 4.8, '٫' in .NET 8
				var separator = persianCulture.NumberFormat.NumberDecimalSeparator;

				AssertEquals($"the decimal point in Persian is '{separator}'", $"64{separator}11", calculationLog.Weight.ToString());
				AssertEquals($"the decimal point in Persian is '{separator}'", $"128{separator}22", calculationLog.Chargeable.ToString());
				AssertEquals($"the decimal point in Persian is '{separator}'", $"10{separator}33", calculationLog.BaseRate.ToString());
				AssertEquals($"the decimal point in Persian is '{separator}'", $"20{separator}44", calculationLog.Minimum.ToString());
				AssertEquals($"the decimal point in Persian is '{separator}'", $"100{separator}55", calculationLog.Maximum.ToString());
				AssertEquals("Serialized as expected", PopulatedXML, calculationLog.SerializeToString());
				TestDeserialize();
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = originalCultureInfo;
			}
		}

		public void TestDeserializeDeprecatedBreakUnit()
		{
			CalculationLog calculationLog = new CalculationLog();

			calculationLog.DeserializeFromString(PopulatedXMLWithBreakUnit);
			AssertEquals(true, calculationLog.IsCosting);
			AssertEquals("FLT", calculationLog.CalculatorCode);
			AssertEquals("CHR", calculationLog.ChargeCode);
			AssertEquals("AUD", calculationLog.Currency);
			AssertEquals(64.11m, calculationLog.Weight);
			AssertEquals("KG", calculationLog.Unit);
			AssertEquals(128.22m, calculationLog.Chargeable);
			AssertEquals("LB", calculationLog.ChargeableUnit);
			AssertEquals("COM", calculationLog.CommodityCode);
			AssertEquals("ULD", calculationLog.RateMode);
			AssertEquals("LD-1", calculationLog.ContainerCode);
			AssertEquals(10.33m, calculationLog.BaseRate);
			AssertEquals(20.44m, calculationLog.Minimum);
			AssertEquals(100.55m, calculationLog.Maximum);
			AssertEquals(2, calculationLog.Steps.Count);

			CalculationLog expectedLog = GetPopulatedLog();
			AssertEquals(true, expectedLog.Steps[0].Equals(calculationLog.Steps[0]));
			AssertEquals(true, expectedLog.Steps[1].Equals(calculationLog.Steps[1]));

			var serializedXML = calculationLog.SerializeToString();
			AssertEquals(false, serializedXML.Contains("<BreakUnit>"));
		}

		#region Equals

		public void TestEquals_AllPropertiesAreEqual_ReturnTrue()
		{
			var log1 = GetPopulatedLog();
			var log2 = GetPopulatedLog();

			AssertEquals($"Are equal", true, log1.Equals(log2));
		}

		public void TestEquals_PropertyValueIsDifferent_ReturnFalse()
		{
			var properties = new[]
			{
				nameof(CalculationLog.IsCosting),
				nameof(CalculationLog.CalculatorCode),
				nameof(CalculationLog.ChargeCode),
				nameof(CalculationLog.Currency),
				nameof(CalculationLog.Weight),
				nameof(CalculationLog.Unit),
				nameof(CalculationLog.Chargeable),
				nameof(CalculationLog.ChargeableUnit),
				nameof(CalculationLog.CommodityCode),
				nameof(CalculationLog.RateMode),
				nameof(CalculationLog.BaseRate),
				nameof(CalculationLog.Minimum),
				nameof(CalculationLog.Maximum),
			};

			foreach (var name in properties)
			{
				var property = typeof(CalculationLog).GetProperty(name);
				var values = GetValues(property.PropertyType);

				var log1 = GetPopulatedLog();
				property.SetValue(log1, values.Item1);

				var log2 = GetPopulatedLog();
				property.SetValue(log2, values.Item2);

				AssertEquals($"Are equal when property '{name}' is different", false, log1.Equals(log2));
			}
		}

		public void TestEquals_StepPropertyValueIsDifferent_ReturnFalse()
		{
			var properties = new[]
			{
				nameof(CalculationStep.CalculationType),
				nameof(CalculationStep.IsSpecialCommodityRate),
				nameof(CalculationStep.UnitCount),
				nameof(CalculationStep.UnitPrice),
				nameof(CalculationStep.Flat),
				nameof(CalculationStep.Percentage),
			};

			foreach (var name in properties)
			{
				var property = typeof(CalculationStep).GetProperty(name);
				var values = GetValues(property.PropertyType);

				var log1 = GetPopulatedLog();
				property.SetValue(log1.Steps[0], values.Item1);

				var log2 = GetPopulatedLog();
				property.SetValue(log2.Steps[0], values.Item2);

				AssertEquals($"Are equal when property '{name}' is different", false, log1.Equals(log2));
			}
		}

		Tuple<object, object> GetValues(Type type)
		{
			if (type == typeof(ZBool))
			{
				return new Tuple<object, object>((ZBool)true, (ZBool)false);
			}
			else if (type == typeof(ZString))
			{
				return new Tuple<object, object>((ZString)"AAA", (ZString)"BBB");
			}
			else if (type == typeof(ZDecimal))
			{
				return new Tuple<object, object>((ZDecimal)10m, (ZDecimal)20m);
			}
			else
			{
				throw new NotSupportedException($"The type {type} is not supported");
			}
		}

		#endregion

		#region Implementation

		CalculationLog GetPopulatedLog()
		{
			CalculationLog result = new CalculationLog();
			result.IsCosting = true;
			result.CalculatorCode = "FLT";
			result.ChargeCode = "CHR";
			result.Currency = "AUD";
			result.Weight = 64.11m;
			result.Unit = "KG";
			result.Chargeable = 128.22m;
			result.ChargeableUnit = "LB";

			result.CommodityCode = "COM";
			result.RateMode = "ULD";
			result.ContainerCode = "LD-1";

			result.BaseRate = 10.33m;
			result.Minimum = 20.44m;
			result.Maximum = 100.55m;

			CalculationStep step1 = new CalculationStep();
			step1.CalculationType = "AAA";
			step1.UnitCount = 10.24m;
			step1.UnitPrice = 20.48m;
			step1.Unit = "KG";
			step1.Percentage = 80m;
			step1.Result = 40.96m;
			result.Steps.Add(step1);

			CalculationStep step2 = new CalculationStep();
			step2.CalculationType = "BBB";
			step2.IsSpecialCommodityRate = true;
			step2.UnitCount = 1.28m;
			step2.UnitPrice = 2.56m;
			step2.Unit = "KG";
			step2.Flat = 4m;
			step2.Result = 8192m;
			result.Steps.Add(step2);

			return result;
		}

		const string EmptyXML =
		"<CalculationLog>" +
			"<IsCosting>N</IsCosting>" +
			"<CalculatorCode />" +
			"<ChargeCode />" +
			"<Currency />" +
			"<Weight>0</Weight>" +
			"<Unit />" +
			"<Chargeable>0</Chargeable>" +
			"<ChargeableUnit />" +
			"<CommodityCode />" +
			"<RateMode />" +
			"<ContainerCode />" +
			"<BaseRate>0</BaseRate>" +
			"<Minimum>0</Minimum>" +
			"<Maximum>0</Maximum>" +
			"<CalculationSteps />" +
		"</CalculationLog>";

		const string PopulatedXML =
		"<CalculationLog>" +
			"<IsCosting>Y</IsCosting>" +
			"<CalculatorCode>FLT</CalculatorCode>" +
			"<ChargeCode>CHR</ChargeCode>" +
			"<Currency>AUD</Currency>" +
			"<Weight>64.11</Weight>" +
			"<Unit>KG</Unit>" +
			"<Chargeable>128.22</Chargeable>" +
			"<ChargeableUnit>LB</ChargeableUnit>" +
			"<CommodityCode>COM</CommodityCode>" +
			"<RateMode>ULD</RateMode>" +
			"<ContainerCode>LD-1</ContainerCode>" +
			"<BaseRate>10.33</BaseRate>" +
			"<Minimum>20.44</Minimum>" +
			"<Maximum>100.55</Maximum>" +
			"<CalculationSteps>" +
				"<CalculationStep>" +
					"<CalculationType>AAA</CalculationType>" +
					"<IsSpecialCommodityRate>N</IsSpecialCommodityRate>" +
					"<UnitCount>10.24</UnitCount>" +
					"<UnitPrice>20.48</UnitPrice>" +
					"<Unit>KG</Unit>" +
					"<Flat>0</Flat>" +
					"<Percentage>80</Percentage>" +
					"<Result>40.96</Result>" +
				"</CalculationStep>" +
				"<CalculationStep>" +
					"<CalculationType>BBB</CalculationType>" +
					"<IsSpecialCommodityRate>Y</IsSpecialCommodityRate>" +
					"<UnitCount>1.28</UnitCount>" +
					"<UnitPrice>2.56</UnitPrice>" +
					"<Unit>KG</Unit>" +
					"<Flat>4</Flat>" +
					"<Percentage>0</Percentage>" +
					"<Result>8192</Result>" +
				"</CalculationStep>" +
			"</CalculationSteps>" +
		"</CalculationLog>";

		const string PopulatedXMLWithBreakUnit =
		"<CalculationLog>" +
			"<IsCosting>Y</IsCosting>" +
			"<CalculatorCode>FLT</CalculatorCode>" +
			"<ChargeCode>CHR</ChargeCode>" +
			"<Currency>AUD</Currency>" +
			"<Weight>64.11</Weight>" +
			"<Unit>KG</Unit>" +
			"<BreakUnit>ZZ</BreakUnit>" +
			"<Chargeable>128.22</Chargeable>" +
			"<ChargeableUnit>LB</ChargeableUnit>" +
			"<CommodityCode>COM</CommodityCode>" +
			"<RateMode>ULD</RateMode>" +
			"<ContainerCode>LD-1</ContainerCode>" +
			"<BaseRate>10.33</BaseRate>" +
			"<Minimum>20.44</Minimum>" +
			"<Maximum>100.55</Maximum>" +
			"<CalculationSteps>" +
				"<CalculationStep>" +
					"<CalculationType>AAA</CalculationType>" +
					"<IsSpecialCommodityRate>N</IsSpecialCommodityRate>" +
					"<UnitCount>10.24</UnitCount>" +
					"<UnitPrice>20.48</UnitPrice>" +
					"<Unit>KG</Unit>" +
					"<Flat>0</Flat>" +
					"<Percentage>80</Percentage>" +
					"<Result>40.96</Result>" +
					"<ContainerNumber />" +
				"</CalculationStep>" +
				"<CalculationStep>" +
					"<CalculationType>BBB</CalculationType>" +
					"<IsSpecialCommodityRate>Y</IsSpecialCommodityRate>" +
					"<UnitCount>1.28</UnitCount>" +
					"<UnitPrice>2.56</UnitPrice>" +
					"<Unit>KG</Unit>" +
					"<Flat>4</Flat>" +
					"<Percentage>0</Percentage>" +
					"<Result>8192</Result>" +
					"<ContainerNumber />" +
				"</CalculationStep>" +
			"</CalculationSteps>" +
		"</CalculationLog>";

		#endregion
	}
}
