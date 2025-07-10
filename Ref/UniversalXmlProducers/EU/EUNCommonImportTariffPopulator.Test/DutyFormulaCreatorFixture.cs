using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	public class DutyFormulaCreatorFixture
	{
		[Test]
		public void DutyExpressionId_01_Percentage_Zero()
		{
			var component1 = new measureComponent
			{
				dutyAmount = 0,
				dutyExpressionId = "01",
				dutyAmountSpecified = true
			};
			var creator = new DutyFormulaCreator();
			Assert.AreEqual("0", creator.Get(new[] { component1 }, string.Empty));
		}

		[Test]
		public void DutyExpressionId_01_Percentage()
		{
			var component1 = new measureComponent
			{
				dutyAmount = 36.1m,
				dutyExpressionId = "01",
				dutyAmountSpecified = true
			};
			var creator = new DutyFormulaCreator();
			Assert.AreEqual("VFD * 0.361", creator.Get(new[] { component1 }, string.Empty));
		}

		[Test]
		public void DutyExpressionId_01_Amount()
		{
			var component1 = new measureComponent
			{
				dutyAmount = 3490,
				dutyExpressionId = "01",
				measurementUnitCode = "MTQ",
				monetaryUnitCode = "EUR",
				dutyAmountSpecified = true,
			};
			var creator = new DutyFormulaCreator();
			Assert.AreEqual("3490 * [MTQ]", creator.Get(new[] { component1 }, string.Empty));
		}

		[Test]
		public void DutyExpressionId_04()
		{
			var component1 = new measureComponent
			{
				dutyAmount = 0.5m,
				measurementUnitCode = "KGM",
				measurementUnitQualifierCode = "P",
				monetaryUnitCode = "EUR",
				dutyExpressionId = "01",
				dutyAmountSpecified = true
			};
			var component2 = new measureComponent
			{
				dutyAmount = 10.5m,
				dutyExpressionId = "04",
				measurementUnitCode = "DTN",
				monetaryUnitCode = "EUR",
				dutyAmountSpecified = true
			};
			var creator = new DutyFormulaCreator();
			Assert.AreEqual("0.5 * [KGMP] + 10.5 * [DTN]", creator.Get(new[] { component1, component2 }, string.Empty));
		}

		[Test]
		public void DutyExpressionId_15()
		{
			var component1 = new measureComponent
			{
				dutyAmount = 2.5m,
				dutyExpressionId = "01",
				dutyAmountSpecified = true
			};
			var component2 = new measureComponent
			{
				dutyAmount = 1.0m,
				dutyExpressionId = "15",
				measurementUnitCode = "DTN",
				monetaryUnitCode = "EUR",
				dutyAmountSpecified = true
			};
			var creator = new DutyFormulaCreator();
			Assert.AreEqual("MAX(VFD * 0.025, 1.0 * [DTN])", creator.Get(new[] { component1, component2 }, string.Empty));
		}

		[Test]
		public void DutyExpressionId_17()
		{
			var component1 = new measureComponent
			{
				dutyAmount = 3.9m,
				dutyExpressionId = "01",
				dutyAmountSpecified = true
			};
			var component2 = new measureComponent
			{
				dutyAmount = 15m,
				dutyExpressionId = "17",
				measurementUnitCode = "DTN",
				monetaryUnitCode = "EUR",
				dutyAmountSpecified = true
			};
			var creator = new DutyFormulaCreator();
			Assert.AreEqual("MIN(VFD * 0.039, 15 * [DTN])", creator.Get(new[] { component1, component2 }, string.Empty));
		}

		[Test]
		public void DutyExpressionId_19()
		{
			var component1 = new measureComponent
			{
				dutyAmount = 4.3m,
				dutyExpressionId = "01",
				dutyAmountSpecified = true
			};
			var component2 = new measureComponent
			{
				dutyAmount = 20.53m,
				dutyExpressionId = "04",
				measurementUnitCode = "DTN",
				monetaryUnitCode = "EUR",
				dutyAmountSpecified = true
			};
			var component3 = new measureComponent
			{
				dutyAmount = 9.7m,
				dutyExpressionId = "17",
				dutyAmountSpecified = true
			};
			var component4 = new measureComponent
			{
				dutyAmount = 3.73m,
				dutyExpressionId = "19",
				measurementUnitCode = "DTN",
				monetaryUnitCode = "EUR",
				dutyAmountSpecified = true
			};
			var creator = new DutyFormulaCreator();
			Assert.AreEqual("MIN(VFD * 0.043 + 20.53 * [DTN], VFD * 0.097 + 3.73 * [DTN])", creator.Get(new[] { component1, component2, component3, component4 }, string.Empty));
		}

		[TestCase("14", "EAR")]
		[TestCase("12", "EA")]
		[TestCase("21", "ADSZ")]
		[TestCase("25", "ADSZR")]
		[TestCase("27", "ADFM")]
		[TestCase("29", "ADFMR")]
		public void DutyExpressionId_Agriculturecomponent(string expressionId, string expectedComponent)
		{
			var component1 = new measureComponent
			{
				dutyAmount = 5.3m,
				dutyExpressionId = "01",
				dutyAmountSpecified = true
			};
			var component2 = new measureComponent
			{
				dutyExpressionId = expressionId,
			};
			var creator = new DutyFormulaCreator();
			Assert.AreEqual($"VFD * 0.053 + #{expectedComponent}(4)#", creator.Get(new[] { component1, component2 }, "4"));
		}
	}
}
