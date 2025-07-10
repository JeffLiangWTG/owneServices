using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	class CMRParserProviderTests
	{
		[Test]
		public void TestRefDataParsersReturnsOnlyActiveParsers()
		{
			var parserConfigs = new Dictionary<string, string>
			{
				{ nameof(ApplicationConfig.AQISCommodityCodesParserIsActive), "true" },
				{ nameof(ApplicationConfig.AQISConcernCodesParserIsActive), "true" },
				{ nameof(ApplicationConfig.AQISDocumentTypesParserIsActive), "false" },
				{ nameof(ApplicationConfig.AQISEntityCodesParserIsActive), "false" },
				{ nameof(ApplicationConfig.AQISPremisesCodesParserIsActive), "true" },
				{ nameof(ApplicationConfig.AQISProcessingTypeParserIsActive), "false" },
				{ nameof(ApplicationConfig.AQISProducerCodesParserIsActive), "true" },
				{ nameof(ApplicationConfig.BerthCodesParserIsActive), "true" },
				{ nameof(ApplicationConfig.CharacteristicCodesParserIsActive), "false" },
				{ nameof(ApplicationConfig.CMRSeaImpendingArrivalsParserIsActive), "true" },
				{ nameof(ApplicationConfig.CMRSeaImpendingArrivalsTestParserIsActive), "false" },
				{ nameof(ApplicationConfig.EstablishmentCodesParserIsActive), "false" },
				{ nameof(ApplicationConfig.PreferenceRulesParserIsActive), "false" },
				{ nameof(ApplicationConfig.PreferenceRulesTestParserIsActive), "false" },
				{ nameof(ApplicationConfig.RefCusTradeGroupParserIsActive), "false" },
				{ nameof(ApplicationConfig.RefundReasonCodesParserIsActive), "false" }
			};

			var expectedActiveParsersTypes = new List<Type>
			{
				typeof(AQISCommodityCodesParser),
				typeof(AQISConcernCodesParser),
				typeof(AQISPremisesCodesParser),
				typeof(AQISProducerCodesParser),
				typeof(BerthCodesParser),
				typeof(CMRSeaImpendingArrivalsParser)
			};

			ApplicationConfigTestHelper.SetApplicationConfigValues(parserConfigs);

			var actualParsers = CMRParserProvider.RefDataParsers.ToList();

			foreach (var parserType in actualParsers.Select(actualParser => actualParser.GetType()))
			{
				Assert.Contains(
					parserType,
					expectedActiveParsersTypes,
					$"Unexpected parser type: {parserType.Name}. This parser should not be active based on the configuration."
				);
			}

			Assert.AreEqual(
				expectedActiveParsersTypes.Count,
				actualParsers.Count,
				$"Expected {expectedActiveParsersTypes.Count} active parsers but got {actualParsers.Count}."
			);

			ApplicationConfigTestHelper.ResetApplicationConfig();
		}

		[TestCase("true")]
		[TestCase("false")]
		public void TestRefDataPartialParsersReturnsOnlyActiveParsers(string config)
		{
			var partialParserConfigs = new Dictionary<string, string>
			{
				{ nameof(ApplicationConfig.AQISPremisesCodesPartialParserIsActive), config },
				{ nameof(ApplicationConfig.CMRSeaImpendingArrivalsPartialParserIsActive), config},
			};

			var expectedActiveParsersTypes = config == "true"
				? new List<Type> {
					typeof(AQISPremisesCodesPartialParser),
					typeof(CMRSeaImpendingArrivalsPartialParser)
				}
				: new List<Type>();

			ApplicationConfigTestHelper.SetApplicationConfigValues(partialParserConfigs);

			var actualParsers = CMRParserProvider.RefDataPartialParsers.ToList();

			Assert.AreEqual(
				expectedActiveParsersTypes.Count,
				actualParsers.Count,
				$"Expected {expectedActiveParsersTypes.Count} active parsers but got {actualParsers.Count}."
			);

			foreach (var parserType in actualParsers.Select(actualParser => actualParser.GetType()))
			{
				Assert.Contains(
					parserType,
					expectedActiveParsersTypes,
					$"Unexpected parser type: {parserType.Name}. This parser should not be active based on the configuration."
				);
			}
			ApplicationConfigTestHelper.ResetApplicationConfig();
		}
	}
}
