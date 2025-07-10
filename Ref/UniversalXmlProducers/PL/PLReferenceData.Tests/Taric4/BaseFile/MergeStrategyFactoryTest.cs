using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4;

[TestFixture]
sealed class MergeStrategyFactoryTest
{
	[Test]
	[TestCaseSource(nameof(GetTestCases))]
	public Type TestCreateMergeStrategy(IsztarHistoryResponseIsztarHistoryItem historyItem)
	{
		var factory = new MergeStrategyFactory();
		var mergeStrategy = factory.CreateMergeStrategy(historyItem);
		return mergeStrategy?.GetType();
	}

	static IEnumerable<TestCaseData> GetTestCases()
	{
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findAdditionalCodeByDatesResponse() })
			.SetName(nameof(findAdditionalCodeByDatesResponse))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findAdditionalCodeTypeByDatesResponse() })
			.SetName(nameof(findAdditionalCodeTypeByDatesResponse))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findBaseRegulationByDatesResponseHistory() })
			.SetName(nameof(findBaseRegulationByDatesResponseHistory))
			.Returns(typeof(MergeStrategy<findBaseRegulationByDatesResponseHistory, baseRegulation>));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findCeilingByDatesResponseHistory() })
			.SetName(nameof(findCeilingByDatesResponseHistory))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findCertificateByDatesResponse() })
			.SetName(nameof(findCertificateByDatesResponse))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findCertificateTypeByDatesResponse() })
			.SetName(nameof(findCertificateTypeByDatesResponse))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findCompleteAbrogationRegulationByDatesResponseHistory() })
			.SetName(nameof(findCompleteAbrogationRegulationByDatesResponseHistory))
			.Returns(typeof(MergeStrategy<findCompleteAbrogationRegulationByDatesResponseHistory, completeAbrogationRegulation>));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findDutyExpressionByDatesResponse() })
			.SetName(nameof(findDutyExpressionByDatesResponse))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findExplicitAbrogationRegulationByDatesResponseHistory() })
			.SetName(nameof(findExplicitAbrogationRegulationByDatesResponseHistory))
			.Returns(typeof(MergeStrategy<findExplicitAbrogationRegulationByDatesResponseHistory, explicitAbrogationRegulation>));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findExportRefundNomenclatureByDatesResponse() })
			.SetName(nameof(findExportRefundNomenclatureByDatesResponse))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findFootnoteByDatesResponse() })
			.SetName(nameof(findFootnoteByDatesResponse))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findFootnoteTypeByDatesResponse() })
			.SetName(nameof(findFootnoteTypeByDatesResponse))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findFullTemporaryStopRegulationByDatesResponseHistory() })
			.SetName(nameof(findFullTemporaryStopRegulationByDatesResponseHistory))
			.Returns(typeof(MergeStrategy<findFullTemporaryStopRegulationByDatesResponseHistory, fullTemporaryStopRegulation>));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findGeographicalAreaByDatesResponse() })
			.SetName(nameof(findGeographicalAreaByDatesResponse))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findGoodsNomenclatureByDatesResponse() })
			.SetName(nameof(findGoodsNomenclatureByDatesResponse))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findGoodsNomenclatureGroupByDatesResponse() })
			.SetName(nameof(findGoodsNomenclatureGroupByDatesResponse))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findLanguageByDatesResponseHistory() })
			.SetName(nameof(findLanguageByDatesResponseHistory))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findMeasureActionByDatesResponse() })
			.SetName(nameof(findMeasureActionByDatesResponse))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findMeasureByDatesResponseHistory() })
			.SetName(nameof(findMeasureByDatesResponseHistory))
			.Returns(typeof(MergeStrategy<findMeasureByDatesResponseHistory, measure>));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findMeasureConditionCodeByDatesResponse() })
			.SetName(nameof(findMeasureConditionCodeByDatesResponse))
			.Returns(typeof(MergeStrategy<findMeasureConditionCodeByDatesResponse, measureConditionCode>));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findMeasureTypeByDatesResponse() })
			.SetName(nameof(findMeasureTypeByDatesResponse))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findMeasureTypeSeriesByDatesResponse() })
			.SetName(nameof(findMeasureTypeSeriesByDatesResponse))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findMeasurementUnitByDatesResponse() })
			.SetName(nameof(findMeasurementUnitByDatesResponse))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findMeasurementUnitQualifierByDatesResponse() })
			.SetName(nameof(findMeasurementUnitQualifierByDatesResponse))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findMeursingAdditionalCodeByDatesResponseHistory() })
			.SetName(nameof(findMeursingAdditionalCodeByDatesResponseHistory))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findMeursingTablePlanByDatesResponseHistory() })
			.SetName(nameof(findMeursingTablePlanByDatesResponseHistory))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findModificationRegulationByDatesResponseHistory() })
			.SetName(nameof(findModificationRegulationByDatesResponseHistory))
			.Returns(typeof(MergeStrategy<findModificationRegulationByDatesResponseHistory, modificationRegulation>));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findMonetaryExchangePeriodByDatesResponseHistory() })
			.SetName(nameof(findMonetaryExchangePeriodByDatesResponseHistory))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findMonetaryPlaceOfPublicationByDatesResponseHistory() })
			.SetName(nameof(findMonetaryPlaceOfPublicationByDatesResponseHistory))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findMonetaryUnitByDatesResponse() })
			.SetName(nameof(findMonetaryUnitByDatesResponse))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findProrogationRegulationByDatesResponseHistory() })
			.SetName(nameof(findProrogationRegulationByDatesResponseHistory))
			.Returns(typeof(MergeStrategy<findProrogationRegulationByDatesResponseHistory, prorogationRegulation>));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findPublicationSigleByDatesResponseHistory() })
			.SetName(nameof(findPublicationSigleByDatesResponseHistory))
			.Returns(typeof(MergeStrategy<findPublicationSigleByDatesResponseHistory, publicationSigle>));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findQuotaDefinitionByDatesResponseHistory() })
			.SetName(nameof(findQuotaDefinitionByDatesResponseHistory))
			.Returns(typeof(MergeStrategy<findQuotaDefinitionByDatesResponseHistory, quotaDefinition>));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findQuotaOrderNumberByDatesResponseHistory() })
			.SetName(nameof(findQuotaOrderNumberByDatesResponseHistory))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findRegulationGroupByDatesResponseHistory() })
			.SetName(nameof(findRegulationGroupByDatesResponseHistory))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findRegulationReplacementByDatesResponseHistory() })
			.SetName(nameof(findRegulationReplacementByDatesResponseHistory))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findRegulationRoleTypeByDatesResponseHistory() })
			.SetName(nameof(findRegulationRoleTypeByDatesResponseHistory))
			.Returns(typeof(NullMergeStrategy));
		yield return new TestCaseData(new IsztarHistoryResponseIsztarHistoryItem { Item = new findSensitiveGoodByDatesResponseHistory() })
			.SetName(nameof(findSensitiveGoodByDatesResponseHistory))
			.Returns(typeof(NullMergeStrategy));
	}
}
