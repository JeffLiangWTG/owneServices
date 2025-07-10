using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.RefDbRepo.AUReferenceData.Business.CMRReferenceData;
using CargoWise.RefDbRepo.AUReferenceData.Services;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public static class CMRParserProvider
	{
		static readonly Dictionary<Type, Func<ICMRDataParser>> refDataParserCreatorFuncs = new()
		{
			{ typeof(AQISCommodityCodesParser), () => new AQISCommodityCodesParser() },
			{ typeof(AQISConcernCodesParser), () => new AQISConcernCodesParser() },
			{ typeof(AQISDocumentTypesParser), () => new AQISDocumentTypesParser() },
			{ typeof(AQISEntityCodesParser), () => new AQISEntityCodesParser() },
			{ typeof(AQISPremisesCodesParser), () => new AQISPremisesCodesParser() },
			{ typeof(AQISProcessingTypeParser), () => new AQISProcessingTypeParser() },
			{ typeof(AQISProducerCodesParser), () => new AQISProducerCodesParser() },
			{ typeof(BerthCodesParser), () => new BerthCodesParser() },
			{ typeof(CharacteristicCodesParser), () => new CharacteristicCodesParser() },
			{ typeof(CMRSeaImpendingArrivalsParser), () => new CMRSeaImpendingArrivalsParser() },
			{ typeof(CMRSeaImpendingArrivalsTestParser), () => new CMRSeaImpendingArrivalsTestParser() },
			{ typeof(EstablishmentCodesParser), () => new EstablishmentCodesParser() },
			{ typeof(PreferenceRulesParser), () => new PreferenceRulesParser(new DateTimeProvider()) },
			{ typeof(PreferenceRulesTestParser), () => new PreferenceRulesTestParser(new DateTimeProvider()) },
			{ typeof(RefCusTradeGroupParser), () => new RefCusTradeGroupParser() },
			{ typeof(RefundReasonCodesParser), () => new RefundReasonCodesParser(new DateTimeProvider()) }
		};

		static readonly Dictionary<Type, Func<ICMRDataParser>> refDataPartialParserConstructors = new()
		{
			{ typeof(AQISPremisesCodesPartialParser), () => new AQISPremisesCodesPartialParser() },
			{ typeof(AQISProducerCodesPartialParser), () => new AQISProducerCodesPartialParser() },
			{ typeof(CMRSeaImpendingArrivalsPartialParser), () => new CMRSeaImpendingArrivalsPartialParser() }
		};

		public static IEnumerable<ICMRDataParser> RefDataParsers
		{
			get
			{
				foreach (var (type, creatorFunc) in refDataParserCreatorFuncs)
				{
					if (IsParserActive(type))
					{
						yield return creatorFunc();
					}
				}
			}
		}

		public static IEnumerable<ICMRDataParser> RefDataPartialParsers
		{
			get
			{
				foreach (var (type, creatorFunc) in refDataPartialParserConstructors)
				{
					if (IsParserActive(type))
					{
						yield return creatorFunc();
					}
				}
			}
		}

		public static IEnumerable<ITariffDataParser> CustomsTariffParsers
		{
			get
			{
				yield return new StatisticalClassificationPeriodSnapshotParser(new DateTimeProvider());
			}
		}

		public static IEnumerable<ITariffDataParser> TestParsers
		{
			get
			{
				yield return new StatisticalClassificationPeriodSnapshotTestParser(new DateTimeProvider());
			}
		}

		static bool IsParserActive(Type parserType)
		{
			return ApplicationConfig.ReadBool($"{parserType.Name}IsActive");
		}
	}
}
