using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	[TestFixture]
	sealed class ApplicationConfigTests
	{
		[Test]
		public void TestNexDocReferenceDataUsername()
		{
			Assert.That(@"2b79000c00654c028cd1d49262abde9c", Is.EqualTo(ApplicationConfig.NexDocReferenceDataUsername));
		}

		[Test]
		public void TestNexDocReferenceDataPassword()
		{
			Assert.That(@"", Is.EqualTo(ApplicationConfig.NexDocReferenceDataPassword));
		}

		[Test]
		public void TestNexDocRESTReferenceDataEndPoint()
		{
			Assert.That(@"https://online.agriculture.gov.au/nexdoc-rs/api/public/v1/reference-data", Is.EqualTo(ApplicationConfig.NexDocRESTReferenceDataEndPoint));
		}

		[Test]
		public void TestNexDocRESTReferenceDataMaxAttempts()
		{
			Assert.That(@"5", Is.EqualTo(ApplicationConfig.NexDocRESTReferenceDataMaxAttempts));
		}

		[Test]
		public void TestNexDocRESTReferenceDataRetryDelay()
		{
			Assert.That(@"10000", Is.EqualTo(ApplicationConfig.NexDocRESTReferenceDataRetryDelay));
		}

		[Test]
		public void TestOutputDirectory()
		{
			Assert.That(@"..\..\UXmlFiles", Is.EqualTo(ApplicationConfig.OutputDirectory));
		}

		[Test]
		public void TestAUTariffsOutputPath()
		{
			Assert.That(@"..\..\UXmlFiles\AUTariffs\AU Tariffs.json", Is.EqualTo(ApplicationConfig.AUTariffsOutputPath));
		}

		[Test]
		public void TestExchangeRateMaxAttempts()
		{
			Assert.That(@"3", Is.EqualTo(ApplicationConfig.ExchangeRateMaxAttempts));
		}

		[Test]
		public void TestExchangeRateRetryDelay()
		{
			Assert.That(@"300000", Is.EqualTo(ApplicationConfig.ExchangeRateRetryDelay));
		}

		[Test]
		public void TestExchangeRateUrl()
		{
			Assert.That(@"https://www.ccf.customs.gov.au/reference/production/main/", Is.EqualTo(ApplicationConfig.ExchangeRateUrl));
		}

		[Test]
		public void TestExchangeRateFileNamePrefix()
		{
			Assert.That(@"XCHGRATE-P1-EDMAIN", Is.EqualTo(ApplicationConfig.ExchangeRateFileNamePrefix));
		}

		[Test]
		public void TestLCTThresholdsUrl()
		{
			Assert.That(@"https://www.ato.gov.au/tax-rates-and-codes/luxury-car-tax-rate-and-thresholds/", Is.EqualTo(ApplicationConfig.LCTThresholdsUrl));
		}

		[Test]
		public void TestAQISCommodityCodesFilePrefix()
		{
			Assert.That("AQSCMDTY-P1-EDMAIN", Is.EqualTo(ApplicationConfig.AQISCommodityCodesFilePrefix));
		}

		[Test]
		public void TestAHECCFilePrefix()
		{
			Assert.That("AHECCSS-P1-EEMAIN", Is.EqualTo(ApplicationConfig.AHECCFilePrefix));
		}

		[Test]
		public void TestAHECCPartialFilePrefix()
		{
			Assert.That("AHECCSS-P1-EDCHNG", Is.EqualTo(ApplicationConfig.AHECCPartialFilePrefix));
		}

		[Test]
		public void TestAHECCTestFilePrefix()
		{
			Assert.That("AHECCSS-Q1-EEMAIN", Is.EqualTo(ApplicationConfig.AHECCTestFilePrefix));
		}

		[Test]
		public void TestAHECCPartialTestFilePrefix()
		{
			Assert.That("AHECCSS-Q1-EDCHNG", Is.EqualTo(ApplicationConfig.AHECCPartialTestFilePrefix));
		}

		[Test]
		public void TestAQISCommodityStatisticalClassificationFilePrefix()
		{
			Assert.That("AQSCMSTC-P1-EDMAIN", Is.EqualTo(ApplicationConfig.AQISCommodityStatisticalClassificationFilePrefix));
		}

		[Test]
		public void TestAQISConcernCodesFilePrefix()
		{
			Assert.That("AQSCNCRN-P1-EDMAIN", Is.EqualTo(ApplicationConfig.AQISConcernCodesFilePrefix));
		}

		[Test]
		public void TestAQISDocumentTypesFilePrefix()
		{
			Assert.That("AQSDCMNT-P1-EDMAIN", Is.EqualTo(ApplicationConfig.AQISDocumentTypesFilePrefix));
		}

		[Test]
		public void TestAQISEntityCodesFilePrefix()
		{
			Assert.That("AQSENTIT-P1-EDMAIN", Is.EqualTo(ApplicationConfig.AQISEntityCodesFilePrefix));
		}

		[Test]
		public void TestAQISPremisesCodesFilePrefix()
		{
			Assert.That("AQSPREMS-P1-EDMAIN", Is.EqualTo(ApplicationConfig.AQISPremisesCodesFilePrefix));
		}

		[Test]
		public void TestAQISProcessingTypeFilePrefix()
		{
			Assert.That("AQSPROCS-P1-EDMAIN", Is.EqualTo(ApplicationConfig.AQISProcessingTypeFilePrefix));
		}

		[Test]
		public void TestAQISProducerCodesFilePrefix()
		{
			Assert.That("AQSPRDCR-P1-EDMAIN", Is.EqualTo(ApplicationConfig.AQISProducerCodesFilePrefix));
		}

		[Test]
		public void TestAQISProducerCodesPartialFilePrefix()
		{
			Assert.That("AQSPRDCR-P1-EDCHNG", Is.EqualTo(ApplicationConfig.AQISProducerCodesPartialFilePrefix));
		}
		
		[Test]
		public void TestBerthCodesFilePrefix()
		{
			Assert.That("BERTH-P1-EEMAIN", Is.EqualTo(ApplicationConfig.BerthCodesFilePrefix));
		}

		[Test]
		public void TestCharacteristicCodesFilePrefix()
		{
			Assert.That("CHRCTRST-P1-EDMAIN", Is.EqualTo(ApplicationConfig.CharacteristicCodesFilePrefix));
		}

		[Test]
		public void TestCMRSeaImpendingArrivalsFilePrefix()
		{
			Assert.That("SEAIMPAR-P1-EDMAIN", Is.EqualTo(ApplicationConfig.CMRSeaImpendingArrivalsFilePrefix));
		}

		[Test]
		public void TestCMRSeaImpendingArrivalsPartialFilePrefix()
		{
			Assert.That("SEAIMPAR-P1-EDCHNG", Is.EqualTo(ApplicationConfig.CMRSeaImpendingArrivalsPartialFilePrefix));
		}

		[Test]
		public void TestCMRSeaImpendingArrivalsTestFilePrefix()
		{
			Assert.That("SEAIMPAR-Q1-EDMAIN", Is.EqualTo(ApplicationConfig.CMRSeaImpendingArrivalsTestFilePrefix));
		}

		[Test]
		public void TestEstablishmentCodesFilePrefix()
		{
			Assert.That("ESTABMNT-P1-EEMAIN", Is.EqualTo(ApplicationConfig.EstablishmentCodesFilePrefix));
		}

		[Test]
		public void TestRefCusTradeGroupCountryFilePrefix()
		{
			Assert.That("PRSPCTRY-P1-EDMAIN", Is.EqualTo(ApplicationConfig.RefCusTradeGroupCountryFilePrefix));
		}

		[Test]
		public void TestRefCusTradeGroupFilePrefix()
		{
			Assert.That("PRSPSNAP-P1-EDMAIN", Is.EqualTo(ApplicationConfig.RefCusTradeGroupFilePrefix));
		}
		
		[Test]
		public void TestStatisticalClassificationPeriodSnapshotFilePrefix()
		{
			Assert.That("STCPSNAP-P1-EDMAIN", Is.EqualTo(ApplicationConfig.StatisticalClassificationPeriodSnapshotFilePrefix));
		}

		[Test]
		public void TestStatisticalClassificationPeriodSnapshotTestFilePrefix()
		{
			Assert.That("STCPSNAP-Q1-EDMAIN", Is.EqualTo(ApplicationConfig.StatisticalClassificationPeriodSnapshotTestFilePrefix));
		}

		[Test]
		public void TestStatisticalClassificationPeriodCharacteristicFilePrefix()
		{
			Assert.That("STCPCHAR-P1-EDMAIN", Is.EqualTo(ApplicationConfig.StatisticalClassificationPeriodCharacteristicFilePrefix));
		}

		[Test]
		public void TestStatisticalClassificationPeriodCharacteristicTestFilePrefix()
		{
			Assert.That("STCPCHAR-Q1-EDMAIN", Is.EqualTo(ApplicationConfig.StatisticalClassificationPeriodCharacteristicTestFilePrefix));
		}

		[Test]
		public void TestTariffClassificationCharacteristicFilePrefix()
		{
			Assert.That("TRFCCHAR-P1-EDMAIN", Is.EqualTo(ApplicationConfig.TariffClassificationCharacteristicFilePrefix));
		}

		[Test]
		public void TestTariffClassificationCharacteristicTestFilePrefix()
		{
			Assert.That("TRFCCHAR-Q1-EDMAIN", Is.EqualTo(ApplicationConfig.TariffClassificationCharacteristicTestFilePrefix));
		}

		[Test]
		public void TestAUNomenclatureForceDownloaded()
		{
			var configDict = new Dictionary<string, string>
			{
				{ "AUNomenclatureForceDownloaded", "true" }
			};

			ApplicationConfigTests.ChangeConfigTemprory4Test(configDict, () =>
			{
				Assert.IsTrue(ApplicationConfig.AUNomenclatureForceDownloaded, "Expected AUNomenclatureForceDownloaded to be true.");
			});

			configDict = new Dictionary<string, string>
			{
				{ "AUNomenclatureForceDownloaded", "false" }
			};

			ApplicationConfigTests.ChangeConfigTemprory4Test(configDict, () =>
			{
				Assert.IsFalse(ApplicationConfig.AUNomenclatureForceDownloaded, "Expected AUNomenclatureForceDownloaded to be false.");
			});
		}

		[TestCase("true", true)]
		[TestCase("false", false)]
		[TestCase("", false)]
		[TestCase("invalid", false)]
		public void TestParserIsActiveConfigsCorrectlyEvaluated(string configValue, bool expected)
		{
			var configValues = ParserIsActiveConfigs.Keys.ToDictionary(key => key, _ => configValue);
			ApplicationConfigTestHelper.SetApplicationConfigValues(configValues);

			Assert.Multiple(() =>
			{
				foreach (var(key, value) in ParserIsActiveConfigs)
				{
					Assert.AreEqual(expected, value.Invoke(), $"{key} expected to be evaluated correctly");
				}
			});

			ApplicationConfigTestHelper.ResetApplicationConfig();
		}

		[Test]
		public void TestParserIsActiveConfigsAreInactiveByDefault()
		{
			Assert.Multiple(() =>
			{
				foreach (var (key, value) in ParserIsActiveConfigs)
				{
					Assert.IsFalse(value.Invoke(), $"{key} should be inactive");
				}
			});
		}

		public static void ChangeConfigTemprory4Test(Dictionary<string, string> configDict, Action actionAfterConfigChanged)
		{
			var type = typeof(ApplicationConfig);
			var originValueDict = new Dictionary<string, object>();
			foreach (var pair in configDict)
			{
				originValueDict.Add(pair.Key, type.GetProperty(pair.Key).GetValue(null));
				ChangeConfigProperty(type, pair.Key, pair.Value);
			}
			try
			{
				actionAfterConfigChanged();
			}
			finally
			{
				foreach (var pair in originValueDict)
				{
					ChangeConfigProperty(type, pair.Key, pair.Value);
				}
			}
		}

		static void ChangeConfigProperty(Type configType, string key, object value)
		{
			var property = configType.GetRuntimeProperty(key);
			var setterMethod = configType.GetRuntimeMethods().FirstOrDefault(f => f.Name.Contains("set_" + key));

			if (property != null && setterMethod != null)
			{
				if (property.PropertyType == typeof(bool) && value is string stringValue && bool.TryParse(stringValue, out var boolValue))
				{
					setterMethod.Invoke(null, new object[] { boolValue });
				}
				else
				{
					setterMethod.Invoke(null, new object[] { value });
				}
			}
		}

		public static Dictionary<string, Func<bool>> ParserIsActiveConfigs => new() {
			{ nameof(ApplicationConfig.AQISCommodityCodesParserIsActive), () => ApplicationConfig.AQISCommodityCodesParserIsActive },
			{ nameof(ApplicationConfig.AQISCommodityStatisticalClassificationParserIsActive), () => ApplicationConfig.AQISCommodityStatisticalClassificationParserIsActive },
			{ nameof(ApplicationConfig.AQISConcernCodesParserIsActive), () => ApplicationConfig.AQISConcernCodesParserIsActive },
			{ nameof(ApplicationConfig.AQISDocumentTypesParserIsActive), () => ApplicationConfig.AQISDocumentTypesParserIsActive },
			{ nameof(ApplicationConfig.AQISEntityCodesParserIsActive), () => ApplicationConfig.AQISEntityCodesParserIsActive },
			{ nameof(ApplicationConfig.AQISPremisesCodesParserIsActive), () => ApplicationConfig.AQISPremisesCodesParserIsActive },
			{ nameof(ApplicationConfig.AQISPremisesCodesPartialParserIsActive), () => ApplicationConfig.AQISPremisesCodesPartialParserIsActive },
			{ nameof(ApplicationConfig.AQISProcessingTypeParserIsActive), () => ApplicationConfig.AQISProcessingTypeParserIsActive },
			{ nameof(ApplicationConfig.AQISProducerCodesParserIsActive), () => ApplicationConfig.AQISProducerCodesParserIsActive },
			{ nameof(ApplicationConfig.AQISProducerCodesPartialParserIsActive), () => ApplicationConfig.AQISProducerCodesPartialParserIsActive },
			{ nameof(ApplicationConfig.BerthCodesParserIsActive), () => ApplicationConfig.BerthCodesParserIsActive },
			{ nameof(ApplicationConfig.CharacteristicCodesParserIsActive), () => ApplicationConfig.CharacteristicCodesParserIsActive },
			{ nameof(ApplicationConfig.CMRSeaImpendingArrivalsParserIsActive), () => ApplicationConfig.CMRSeaImpendingArrivalsParserIsActive },
			{ nameof(ApplicationConfig.CMRSeaImpendingArrivalsPartialParserIsActive), () => ApplicationConfig.CMRSeaImpendingArrivalsPartialParserIsActive },
			{ nameof(ApplicationConfig.CMRSeaImpendingArrivalsTestParserIsActive), () => ApplicationConfig.CMRSeaImpendingArrivalsTestParserIsActive },
			{ nameof(ApplicationConfig.EstablishmentCodesParserIsActive), () => ApplicationConfig.EstablishmentCodesParserIsActive },
			{ nameof(ApplicationConfig.PreferenceRulesParserIsActive), () => ApplicationConfig.PreferenceRulesParserIsActive },
			{ nameof(ApplicationConfig.PreferenceRulesTestParserIsActive), () => ApplicationConfig.PreferenceRulesTestParserIsActive },
			{ nameof(ApplicationConfig.RefCusTradeGroupCountryParserIsActive), () => ApplicationConfig.RefCusTradeGroupCountryParserIsActive },
			{ nameof(ApplicationConfig.RefCusTradeGroupParserIsActive), () => ApplicationConfig.RefCusTradeGroupParserIsActive },
			{ nameof(ApplicationConfig.RefundReasonCodesParserIsActive), () => ApplicationConfig.RefundReasonCodesParserIsActive },
			{ nameof(ApplicationConfig.StatisticalClassificationPeriodCharacteristicParserIsActive), () => ApplicationConfig.StatisticalClassificationPeriodCharacteristicParserIsActive },
			{ nameof(ApplicationConfig.StatisticalClassificationPeriodSnapshotParserIsActive), () => ApplicationConfig.StatisticalClassificationPeriodSnapshotParserIsActive },
			{ nameof(ApplicationConfig.TariffClassificationCharacteristicParserIsActive), () => ApplicationConfig.TariffClassificationCharacteristicParserIsActive }
		};
	}
}
