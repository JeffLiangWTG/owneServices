using NUnit.Framework;

namespace Enterprise.Rating.Integration.Test
{
	internal class CalculationLogsWrapperTest : TestCase
	{
		public void TestSerialize()
		{
			CalculationLogsWrapper logsWrapper = new CalculationLogsWrapper();
			AssertEquals("Serialized as expected", EmptyXML, logsWrapper.Serialize());

			logsWrapper = GetPopulatedLogsArray();
			AssertEquals("Serialized as expected", PopulatedXML, logsWrapper.Serialize());
		}

		public void TestDeserialize()
		{
			CalculationLogsWrapper logsWrapper = CalculationLogsWrapper.Deserialize(EmptyXML);
			AssertEquals(false, logsWrapper.IsDisabled);
			AssertEquals(0, logsWrapper.Logs.Count);

			logsWrapper = CalculationLogsWrapper.Deserialize(PopulatedXML);
			AssertEquals(false, logsWrapper.IsDisabled);
			AssertEquals(2, logsWrapper.Logs.Count);

			CalculationLog log1 = logsWrapper.Logs[0];
			AssertEquals("FLT", log1.CalculatorCode);
			AssertEquals("CHR", log1.ChargeCode);
			AssertEquals("COM", log1.CommodityCode);
			AssertEquals(64m, log1.Weight);
			AssertEquals("KG", log1.Unit);
			AssertEquals("AUD", log1.Currency);
			AssertEquals("ULD", log1.RateMode);
			AssertEquals("LD-1", log1.ContainerCode);
			AssertEquals(10m, log1.BaseRate);
			AssertEquals(20m, log1.Minimum);
			AssertEquals(100m, log1.Maximum);
			AssertEquals(2, log1.Steps.Count);

			CalculationLogsWrapper expectedLogsArray = GetPopulatedLogsArray();
			AssertEquals(true, expectedLogsArray.Logs[0].Steps[0].Equals(log1.Steps[0]));
			AssertEquals(true, expectedLogsArray.Logs[0].Steps[1].Equals(log1.Steps[1]));

			CalculationLog log2 = logsWrapper.Logs[1];
			AssertEquals("CMB", log2.CalculatorCode);
			AssertEquals("FRT", log2.ChargeCode);
			AssertEquals("", log2.CommodityCode);
			AssertEquals("", log2.Currency);
			AssertEquals("AIR", log2.RateMode);
			AssertEquals("", log2.ContainerCode);
			AssertEquals(100m, log2.BaseRate);
			AssertEquals(0m, log2.Minimum);
			AssertEquals(0m, log2.Maximum);
			AssertEquals(1, log2.Steps.Count);
			AssertEquals(true, expectedLogsArray.Logs[1].Steps[0].Equals(log2.Steps[0]));

			logsWrapper = CalculationLogsWrapper.Deserialize("not a proper xml");
			AssertNull("Returns null when can't deserialize", logsWrapper);
		}

		public void TestFormatXML()
		{
			CalculationLogsWrapper logsWrapper = GetPopulatedLogsArray();
			AssertEquals("Formatted as expected", FormattedXML, logsWrapper.ToFormattedXML());
		}

		public void TestIsEmpty()
		{
			CalculationLogsWrapper logs = new CalculationLogsWrapper();
			AssertEquals(true, logs.IsEmpty);

			CalculationLog log = new CalculationLog();
			logs.Logs.Add(log);
			AssertEquals(false, logs.IsEmpty);

			logs.Logs.Clear();
			AssertEquals(true, logs.IsEmpty);
		}

		public void TestIsCosting()
		{
			CalculationLogsWrapper wrapper = new CalculationLogsWrapper();
			AssertEquals(false, wrapper.IsCosting);

			CalculationLog log = new CalculationLog();
			log.IsCosting = false;
			wrapper.Logs.Add(log);
			AssertEquals(false, wrapper.IsCosting);

			log.IsCosting = true;
			AssertEquals(true, wrapper.IsCosting);
		}

		#region Implementation

		CalculationLogsWrapper GetPopulatedLogsArray()
		{
			CalculationLog log1 = new CalculationLog();
			log1.IsCosting = false;
			log1.CalculatorCode = "FLT";
			log1.ChargeCode = "CHR";
			log1.Currency = "AUD";
			log1.Weight = 64m;
			log1.Unit = "KG";
			log1.Chargeable = 128m;
			log1.ChargeableUnit = "LB";
			log1.CommodityCode = "COM";
			log1.RateMode = "ULD";
			log1.ContainerCode = "LD-1";
			log1.BaseRate = 10m;
			log1.Minimum = 20m;
			log1.Maximum = 100m;

			CalculationStep step11 = new CalculationStep();
			step11.CalculationType = "AAA";
			step11.UnitCount = 10.24m;
			step11.UnitPrice = 20.48m;
			step11.Unit = "";
			step11.Percentage = 80m;
			step11.Result = 40.96m;
			log1.Steps.Add(step11);

			CalculationStep step12 = new CalculationStep();
			step12.CalculationType = "BBB";
			step12.UnitCount = 1.28m;
			step12.UnitPrice = 2.56m;
			step12.Unit = "";
			step12.Flat = 4m;
			step12.Result = 8192m;
			log1.Steps.Add(step12);

			CalculationLog log2 = new CalculationLog();
			log2.IsCosting = true;
			log2.CalculatorCode = "CMB";
			log2.ChargeCode = "FRT";
			log2.RateMode = "AIR";
			log2.BaseRate = 100m;

			CalculationStep step21 = new CalculationStep();
			step21.CalculationType = "CCC";
			step21.UnitCount = 1234m;
			log2.Steps.Add(step21);

			CalculationLogsWrapper result = new CalculationLogsWrapper();
			result.Logs.Add(log1);
			result.Logs.Add(log2);

			return result;
		}

		const string EmptyXML =
		"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
		"<CalculationLogsWrapper>" +
			"<IsDisabled>N</IsDisabled>" +
			"<CalculationLogs />" +
		"</CalculationLogsWrapper>";

		const string PopulatedXML =
		"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
		"<CalculationLogsWrapper>" +
			"<IsDisabled>N</IsDisabled>" +
			"<CalculationLogs>" +
				"<CalculationLog>" +
					"<IsCosting>N</IsCosting>" +
					"<CalculatorCode>FLT</CalculatorCode>" +
					"<ChargeCode>CHR</ChargeCode>" +
					"<Currency>AUD</Currency>" +
					"<Weight>64</Weight>" +
					"<Unit>KG</Unit>" +
					"<Chargeable>128</Chargeable>" +
					"<ChargeableUnit>LB</ChargeableUnit>" +
					"<CommodityCode>COM</CommodityCode>" +
					"<RateMode>ULD</RateMode>" +
					"<ContainerCode>LD-1</ContainerCode>" +
					"<BaseRate>10</BaseRate>" +
					"<Minimum>20</Minimum>" +
					"<Maximum>100</Maximum>" +
					"<CalculationSteps>" +
						"<CalculationStep>" +
							"<CalculationType>AAA</CalculationType>" +
							"<IsSpecialCommodityRate>N</IsSpecialCommodityRate>" +
							"<UnitCount>10.24</UnitCount>" +
							"<UnitPrice>20.48</UnitPrice>" +
							"<Unit />" +
							"<Flat>0</Flat>" +
							"<Percentage>80</Percentage>" +
							"<Result>40.96</Result>" +
						"</CalculationStep>" +
						"<CalculationStep>" +
							"<CalculationType>BBB</CalculationType>" +
							"<IsSpecialCommodityRate>N</IsSpecialCommodityRate>" +
							"<UnitCount>1.28</UnitCount>" +
							"<UnitPrice>2.56</UnitPrice>" +
							"<Unit />" +
							"<Flat>4</Flat>" +
							"<Percentage>0</Percentage>" +
							"<Result>8192</Result>" +
						"</CalculationStep>" +
					"</CalculationSteps>" +
				"</CalculationLog>" +
				"<CalculationLog>" +
					"<IsCosting>Y</IsCosting>" +
					"<CalculatorCode>CMB</CalculatorCode>" +
					"<ChargeCode>FRT</ChargeCode>" +
					"<Currency />" +
					"<Weight>0</Weight>" +
					"<Unit />" +
					"<Chargeable>0</Chargeable>" +
					"<ChargeableUnit />" +
					"<CommodityCode />" +
					"<RateMode>AIR</RateMode>" +
					"<ContainerCode />" +
					"<BaseRate>100</BaseRate>" +
					"<Minimum>0</Minimum>" +
					"<Maximum>0</Maximum>" +
					"<CalculationSteps>" +
						"<CalculationStep>" +
							"<CalculationType>CCC</CalculationType>" +
							"<IsSpecialCommodityRate>N</IsSpecialCommodityRate>" +
							"<UnitCount>1234</UnitCount>" +
							"<UnitPrice>0</UnitPrice>" +
							"<Unit />" +
							"<Flat>0</Flat>" +
							"<Percentage>0</Percentage>" +
							"<Result>0</Result>" +
						"</CalculationStep>" +
					"</CalculationSteps>" +
				"</CalculationLog>" +
			"</CalculationLogs>" +
		"</CalculationLogsWrapper>";

		const string FormattedXML =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<CalculationLogsWrapper>
  <IsDisabled>N</IsDisabled>
  <CalculationLogs>
    <CalculationLog>
      <IsCosting>N</IsCosting>
      <CalculatorCode>FLT</CalculatorCode>
      <ChargeCode>CHR</ChargeCode>
      <Currency>AUD</Currency>
      <Weight>64</Weight>
      <Unit>KG</Unit>
      <Chargeable>128</Chargeable>
      <ChargeableUnit>LB</ChargeableUnit>
      <CommodityCode>COM</CommodityCode>
      <RateMode>ULD</RateMode>
      <ContainerCode>LD-1</ContainerCode>
      <BaseRate>10</BaseRate>
      <Minimum>20</Minimum>
      <Maximum>100</Maximum>
      <CalculationSteps>
        <CalculationStep>
          <CalculationType>AAA</CalculationType>
          <IsSpecialCommodityRate>N</IsSpecialCommodityRate>
          <UnitCount>10.24</UnitCount>
          <UnitPrice>20.48</UnitPrice>
          <Unit />
          <Flat>0</Flat>
          <Percentage>80</Percentage>
          <Result>40.96</Result>
        </CalculationStep>
        <CalculationStep>
          <CalculationType>BBB</CalculationType>
          <IsSpecialCommodityRate>N</IsSpecialCommodityRate>
          <UnitCount>1.28</UnitCount>
          <UnitPrice>2.56</UnitPrice>
          <Unit />
          <Flat>4</Flat>
          <Percentage>0</Percentage>
          <Result>8192</Result>
        </CalculationStep>
      </CalculationSteps>
    </CalculationLog>
    <CalculationLog>
      <IsCosting>Y</IsCosting>
      <CalculatorCode>CMB</CalculatorCode>
      <ChargeCode>FRT</ChargeCode>
      <Currency />
      <Weight>0</Weight>
      <Unit />
      <Chargeable>0</Chargeable>
      <ChargeableUnit />
      <CommodityCode />
      <RateMode>AIR</RateMode>
      <ContainerCode />
      <BaseRate>100</BaseRate>
      <Minimum>0</Minimum>
      <Maximum>0</Maximum>
      <CalculationSteps>
        <CalculationStep>
          <CalculationType>CCC</CalculationType>
          <IsSpecialCommodityRate>N</IsSpecialCommodityRate>
          <UnitCount>1234</UnitCount>
          <UnitPrice>0</UnitPrice>
          <Unit />
          <Flat>0</Flat>
          <Percentage>0</Percentage>
          <Result>0</Result>
        </CalculationStep>
      </CalculationSteps>
    </CalculationLog>
  </CalculationLogs>
</CalculationLogsWrapper>";

		#endregion
	}
}
