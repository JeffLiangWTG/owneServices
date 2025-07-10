using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class GenericLandedCostingConfigTest : TestCase
	{
		public void TestXmlSerialize()
		{
			string xml = @"<GenericLandedCostingConfig CountryCode='CA'>
		<FormalEntryConfigs>
			<FormalEntryConfig EntryType='B3C' IsAdditionalEntryLine='Y' />
			<FormalEntryConfig />
		</FormalEntryConfigs>
	<LCEntryCustomsDisbursementCodeMappings>
		<LCEntryCustomsDisbursementCodeMapping EntryDisbursementCode ='DTY' LCDisbursementCode='TDT' Description='Duty' 
			CusEntryLineApplicablePropertyName='XXX' JobComInvoiceLineAmountPropertyName='YYY' HeaderLevelFee='True' />
		<LCEntryCustomsDisbursementCodeMapping />
	</LCEntryCustomsDisbursementCodeMappings>
	<LandedLineCostItemSettings>
		<LandedLineCostItemSetting CostType='TDT' Description='Total Duty' NumberOfDecimals='2' GridColumnWidth='80'
			DocumentColumnWidth='100' DocumentMacro='LandedCostHistories.LandedLineCostItems[""TDT""]' />
		<LandedLineCostItemSetting/>
	</LandedLineCostItemSettings>
</GenericLandedCostingConfig>";

			using (var strReader = new StringReader(xml))
			using (var reader = XmlReader.Create(strReader))
			{
				var config = new GenericLandedCostingConfig(XElement.Load(reader));

				AssertGenericLandedCostingConfig(config, "CA", 2, 2, 2);
				AssertFormalEntryConfigs(config.FormalEntryConfigs.First(), "B3C", true);
				AssertFormalEntryConfigs(config.FormalEntryConfigs.Last(), "", false);
				AssertLCEntryCustomsDisbursementCodeMapping(config.LCEntryCustomsDisbursementCodeMappings.First(), "DTY", "TDT", "Duty", "XXX", "YYY", true);
				AssertLCEntryCustomsDisbursementCodeMapping(config.LCEntryCustomsDisbursementCodeMappings.Last(), "", "", "", "", "", false);
				AssertLandedLineCostItemColumnSetting(config.LandedLineCostItemSettings.First(), "TDT", "Total Duty", 2, 80, 100, "LandedCostHistories.LandedLineCostItems[\"TDT\"]");
				AssertLandedLineCostItemColumnSetting(config.LandedLineCostItemSettings.Last(), "", "", 0, 0, 0, "");
			}
		}

		public static void AssertGenericLandedCostingConfig(GenericLandedCostingConfig config, ZString countryCode, int formalEntryConfigCount, int codeMappingCount, int columnSettingCount)
		{
			AssertEquals("CountryCode", countryCode, config.CountryCode);
			AssertEquals("FormalEntryConfigs.Count", formalEntryConfigCount, config.FormalEntryConfigs.Count());
			AssertEquals("LCEntryCustomsDisbursementCodeMappings.Count", codeMappingCount, config.LCEntryCustomsDisbursementCodeMappings.Count());
			AssertEquals("LandedLineCostItemSettings.Count", columnSettingCount, config.LandedLineCostItemSettings.Count());
		}

		public static void AssertFormalEntryConfigs(FormalEntryConfig setting, ZString entryType, bool isAdditionalEntryLine)
		{
			AssertEquals("EntryType", entryType, setting.EntryType);
			AssertEquals("IsAdditionalEntryLine", isAdditionalEntryLine, setting.IsAdditionalEntryLine);
		}

		public static void AssertLCEntryCustomsDisbursementCodeMapping(LCEntryCustomsDisbursementCodeMapping mapping,
			ZString entryDisbursementCode, ZString lcdisbursementCode, ZString description, ZString cusEntryLineApplicablePropertyName, ZString jobComInvoiceLineAmountPropertyName, ZBool headerLevelFee)
		{
			AssertEquals("LCEntryCustomsDisbursementCodeMapping.EntryDisbursementCode", entryDisbursementCode, mapping.EntryDisbursementCode);
			AssertEquals("LCEntryCustomsDisbursementCodeMapping.LCDisbursementCode", lcdisbursementCode, mapping.LCDisbursementCode);
			AssertEquals("LCEntryCustomsDisbursementCodeMapping.Description", description, mapping.Description);
			AssertEquals("LCEntryCustomsDisbursementCodeMapping.CusEntryLineApplicablePropertyName", cusEntryLineApplicablePropertyName, mapping.CusEntryLineApplicablePropertyName);
			AssertEquals("LCEntryCustomsDisbursementCodeMapping.JobComInvoiceLineAmountPropertyName", jobComInvoiceLineAmountPropertyName, mapping.JobComInvoiceLineAmountPropertyName);
			AssertEquals("LCEntryCustomsDisbursementCodeMapping.HeaderLevelFee", headerLevelFee, mapping.HeaderLevelFee);
		}

		public static void AssertLandedLineCostItemColumnSetting(LandedLineCostItemSetting setting,
			ZString costType, ZString caption, ZInt numberOfDecimals, ZInt gridColumnWidth, ZInt documentColumnWidth, ZString documentMacro)
		{
			AssertEquals("LandedLineCostItemSetting.CostType", costType, setting.CostType);
			AssertEquals("LandedLineCostItemSetting.Caption", caption, setting.Description);
			AssertEquals("LandedLineCostItemSetting.NumberOfDecimals", numberOfDecimals, setting.NumberOfDecimals);
			AssertEquals("LandedLineCostItemSetting.GridColumnWidth", gridColumnWidth, setting.GridColumnWidth);
			AssertEquals("LandedLineCostItemSetting.DocumentColumnWidth", documentColumnWidth, setting.DocumentColumnWidth);
			AssertEquals("LandedLineCostItemSetting.DocumentMacro", documentMacro, setting.DocumentMacro);
		}
	}
}
