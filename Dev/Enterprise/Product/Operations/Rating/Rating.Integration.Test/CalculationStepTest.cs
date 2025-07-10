using System.Globalization;
using System.Threading;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Rating.Integration.Test
{
	internal class CalculationStepTest : TestCase
	{
		public void TestConstants_CalculationType()
		{
			AssertEquals("UNT", CalculationStep.Constants.CalculationType.PerUnit);
			AssertEquals("PER", CalculationStep.Constants.CalculationType.Percentage);
		}

		public void TestSerialize()
		{
			var calculationStep = GetPopulatedStep();
			AssertEquals("Serialized as expected", XML, calculationStep.SerializeToString());
		}

		public void TestDeserialize()
		{
			var calculationStep = new CalculationStep();
			calculationStep.DeserializeFromString(XML);

			AssertEquals(CalculationStep.Constants.CalculationType.PerUnit, calculationStep.CalculationType);
			AssertEquals(false, calculationStep.IsSpecialCommodityRate);
			AssertEquals(10.24m, calculationStep.UnitCount);
			AssertEquals(20.48m, calculationStep.UnitPrice);
			AssertEquals("CN", calculationStep.Unit);
			AssertEquals(10m, calculationStep.Flat);
			AssertEquals(80m, calculationStep.Percentage);
			AssertEquals(40.96m, calculationStep.Result);

			var populatedStep = GetPopulatedStep();
			AssertEquals(true, calculationStep.Equals(populatedStep));
		}

		public void TestEquals()
		{
			var step1 = GetPopulatedStep();
			var step2 = GetPopulatedStep();
			AssertEquals(true, step1.Equals(step2));

			step2.CalculationType = "XXX";
			AssertEquals(false, step1.Equals(step2));

			step1.CalculationType = "XXX";
			AssertEquals(true, step1.Equals(step2));
		}

		public void TestCalculatedProperties()
		{
			AssertBooleanProperties("", false, false);
			AssertBooleanProperties("XXX", false, false);
			AssertBooleanProperties(CalculationStep.Constants.CalculationType.PerUnit, true, false);
			AssertBooleanProperties(CalculationStep.Constants.CalculationType.Percentage, false, true);
		}

		public void TestUseInvariantCultureToStoreNumber()
		{
			var originalCultureInfo = Thread.CurrentThread.CurrentCulture;

			try
			{
				var calculationStep = GetPopulatedStep();

				Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
				AssertEquals("the decimal point in French is a comma", "10,24", calculationStep.UnitCount.ToString());
				AssertEquals("Serialized as expected", XML, calculationStep.SerializeToString());
				TestDeserialize();

				var persianCulture = new CultureInfo("fa");
				Thread.CurrentThread.CurrentCulture = persianCulture;

				// Persian separator differs by .NET version: '/' in .NET 4.8, '٫' in .NET 8
				var separator = persianCulture.NumberFormat.NumberDecimalSeparator;

				AssertEquals($"the decimal point in Persian is '{separator}'", $"10{separator}24", calculationStep.UnitCount.ToString());
				AssertEquals("Serialized as expected", XML, calculationStep.SerializeToString());
				TestDeserialize();
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = originalCultureInfo;
			}
		}

		public void TestRemovalOfContainerNumber()
		{
			var calculationStep = new CalculationStep();
			calculationStep.DeserializeFromString(OldXML);

			AssertEquals(CalculationStep.Constants.CalculationType.PerUnit, calculationStep.CalculationType);
			AssertEquals(false, calculationStep.IsSpecialCommodityRate);
			AssertEquals(10.24m, calculationStep.UnitCount);
			AssertEquals(20.48m, calculationStep.UnitPrice);
			AssertEquals("CN", calculationStep.Unit);
			AssertEquals(10m, calculationStep.Flat);
			AssertEquals(80m, calculationStep.Percentage);
			AssertEquals(40.96m, calculationStep.Result);

			var serializedXML = calculationStep.SerializeToString();
			AssertEquals(false, serializedXML.Contains("<ContainerNumber>"));
		}

		void AssertBooleanProperties(ZString calculationType, bool perUnit, bool percentage)
		{
			var calculationStep = new CalculationStep();
			calculationStep.CalculationType = calculationType;

			CombineAssertions(delegate
			{
				AssertEquals(perUnit, calculationStep.IsPerUnit);
				AssertEquals(percentage, calculationStep.IsPercentage);
			});
		}

		#region Implementation

		CalculationStep GetPopulatedStep()
		{
			var result = new CalculationStep();
			result.CalculationType = CalculationStep.Constants.CalculationType.PerUnit;
			result.UnitCount = 10.24m;
			result.UnitPrice = 20.48m;
			result.Flat = 10m;
			result.Percentage = 80m;
			result.Result = 40.96m;
			result.Unit = "CN";

			return result;
		}

		const string XML =
		"<CalculationStep>" +
			"<CalculationType>UNT</CalculationType>" +
			"<IsSpecialCommodityRate>N</IsSpecialCommodityRate>" +
			"<UnitCount>10.24</UnitCount>" +
			"<UnitPrice>20.48</UnitPrice>" +
			"<Unit>CN</Unit>" +
			"<Flat>10</Flat>" +
			"<Percentage>80</Percentage>" +
			"<Result>40.96</Result>" +
		"</CalculationStep>";

		const string OldXML =
		"<CalculationStep>" +
			"<CalculationType>UNT</CalculationType>" +
			"<IsSpecialCommodityRate>N</IsSpecialCommodityRate>" +
			"<UnitCount>10.24</UnitCount>" +
			"<UnitPrice>20.48</UnitPrice>" +
			"<Unit>CN</Unit>" +
			"<Flat>10</Flat>" +
			"<Percentage>80</Percentage>" +
			"<Result>40.96</Result>" +
			"<ContainerNumber>CONT00001</ContainerNumber>" +
		"</CalculationStep>";

		#endregion
	}
}
