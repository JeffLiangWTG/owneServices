using System.Collections.Generic;
using System.Xml.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Test.Transform
{
	[TestFixture]
	class UniversalXmlTransformerFixture
	{
		[TestCaseSource(nameof(TransformTestCases_CondAndApp))]
		public void TransformSchemaFixture_CondAndApp(string schemaXml, string expectedResult)
		{
			var transformer = new UniversalXmlTransformer(new UniversalXmlTransformerValidator());
			var actualResult = transformer.Transform(schemaXml);
			Assert.That(XElement.Parse(actualResult).ToString(SaveOptions.DisableFormatting), Is.EqualTo(XElement.Parse(expectedResult).ToString(SaveOptions.DisableFormatting)));
		}

		static IEnumerable<TestCaseData> TransformTestCases_CondAndApp()
		{
			yield return new TestCaseData(RefCusRate_AllFields, RefCusRate_AllFields_ExpectedResult)
			{
				TestName = nameof(RefCusRate_AllFields)
			};
			yield return new TestCaseData(RefCusRateUOM_AllFields, RefCusRateUOM_AllFields_ExpectedResult)
			{
				TestName = nameof(RefCusRateUOM_AllFields)
			};
			yield return new TestCaseData(RefCusApplicability_AllFields, RefCusApplicability_AllFields_ExpectedResult)
			{
				TestName = nameof(RefCusApplicability_AllFields)
			};
			yield return new TestCaseData(RefCusExcludedTradeGroup_AllFields, RefCusExcludedTradeGroup_AllFields_ExpectedResult)
			{
				TestName = nameof(RefCusExcludedTradeGroup_AllFields)
			};
			yield return new TestCaseData(RefCusApplicability_UnderBothRateAndCondition, RefCusApplicability_UnderBothRateAndCondition_ExpectedResult)
			{
				TestName = nameof(RefCusApplicability_UnderBothRateAndCondition)
			};
			yield return new TestCaseData(RefCusRateWithUOM_ApplicabilityWithExcludedTradeGroup_RefCusApplicabilityUnderBothRateAndCondition, RefCusRateWithUOM_ApplicabilityWithExcludedTradeGroup_RefCusApplicabilityUnderBothRateAndCondition_ExpectedResult)
			{
				TestName = nameof(RefCusRateWithUOM_ApplicabilityWithExcludedTradeGroup_RefCusApplicabilityUnderBothRateAndCondition)
			};
			yield return new TestCaseData(RefCusTariffWithAllMappedEntities, RefCusTariffWithAllMappedEntities_ExpectedResult)
			{
				TestName = nameof(RefCusTariffWithAllMappedEntities)
			};
			yield return new TestCaseData(RefCusRateWithApplicability_CombinedEntity, RefCusRateWithApplicability_CombinedEntity_ExpectedResult)
			{
				TestName = nameof(RefCusRateWithApplicability_CombinedEntity)
			};
			yield return new TestCaseData(RefCusRateWithApplicability_DuplicateStartAndEndDate, RefCusRateWithApplicability_DuplicateStartAndEndDate_ExpectedResult)
			{
				TestName = nameof(RefCusRateWithApplicability_DuplicateStartAndEndDate)
			};
			yield return new TestCaseData(RefCusCondition_AllFields, RefCusCondition_AllFields_ExpectedResult)
			{
				TestName = nameof(RefCusCondition_AllFields)
			};
			yield return new TestCaseData(RefCusConditionValue_AllFields, RefCusConditionValue_AllFields_ExpectedResult)
			{
				TestName = nameof(RefCusConditionValue_AllFields)
			};
			yield return new TestCaseData(RefCusConditionLanguage_AllFields, RefCusConditionLanguage_AllFields_ExpectedResult)
			{
				TestName = nameof(RefCusConditionLanguage_AllFields)
			};
			yield return new TestCaseData(RefCusApplicability_UnderCondition_AllFields, RefCusApplicability_UnderCondition_AllFields_ExpectedResult)
			{
				TestName = nameof(RefCusApplicability_UnderCondition_AllFields)
			};
			yield return new TestCaseData(RefCusCondition_WithValueAndLanguage_RefCusApplicabilityUnderBothRateAndCondition, RefCusCondition_WithValueAndLanguage_RefCusApplicabilityUnderBothRateAndCondition_ExpectedResult)
			{
				TestName = nameof(RefCusCondition_WithValueAndLanguage_RefCusApplicabilityUnderBothRateAndCondition)
			};
			yield return new TestCaseData(RefCusConditionWithApplicability_DuplicateStartAndEndDate, RefCusConditionWithApplicability_DuplicateStartAndEndDate_ExpectedResult)
			{
				TestName = nameof(RefCusConditionWithApplicability_DuplicateStartAndEndDate)
			};
			yield return new TestCaseData(RefCusApplicability_UnderRate_Condition_AdditionalCode, RefCusApplicability_UnderRate_Condition_AdditionalCode_ExpectedResult)
			{
				TestName = nameof(RefCusApplicability_UnderRate_Condition_AdditionalCode)
			};
			yield return new TestCaseData(RefCusApplicability_UnderRate_AdditionalCode, RefCusApplicability_UnderRate_AdditionalCode_ExpectedResult)
			{
				TestName = nameof(RefCusApplicability_UnderRate_AdditionalCode)
			};
			yield return new TestCaseData(RefCusApplicability_UnderCondition_AdditionalCode, RefCusApplicability_UnderCondition_AdditionalCode_ExpectedResult)
			{
				TestName = nameof(RefCusApplicability_UnderCondition_AdditionalCode)
			};

			yield return new TestCaseData(RefCusRateWithoutApp_ChildEntities, RefCusRateWithoutApp_ChildEntities_ExpectedResult)
			{
				TestName = nameof(RefCusRateWithoutApp_ChildEntities)
			};
			yield return new TestCaseData(RefCusConditionWithoutApp_ChildEntities, RefCusConditionWithoutApp_ChildEntities_ExpectedResult)
			{
				TestName = nameof(RefCusConditionWithoutApp_ChildEntities)
			};
		}

		const string RefCusRate_AllFields = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRate"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZZ2_StartDate"" />
				<PropertyRef Name=""ZZ2_EndDate"" />
				<PropertyRef Name=""ZZ2_RateFormula"" />
				<PropertyRef Name=""ZZ2_SelectorFormula"" />
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
				<PropertyRef Name=""ZZ2_ZY1_ZZR_NKRateType"" />
				<PropertyRef Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZZ2_ZZS_NKPreference"" />
				<PropertyRef Name=""ZZ2_ZZS_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZZ2_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZZ2_RateFormulaDerivedFrom"" />
				<PropertyRef Name=""ZZ2_RX_NKCurrencyOverride"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZZ2_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZZ2_EndDate"" Type=""smalldatetime"" />
			<Property Name=""ZZ2_RateFormula"" Type=""varchar"" />
			<Property Name=""ZZ2_SelectorFormula"" Type=""varchar"" />
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" />
			<Property Name=""ZZ2_ZY1_ZZR_NKRateType"" Type=""varchar"" />
			<Property Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""ZZ2_ZZS_NKPreference"" Type=""varchar"" />
			<Property Name=""ZZ2_ZZS_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""ZZ2_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""ZZ2_RateFormulaDerivedFrom"" Type=""varchar"" />
			<Property Name=""ZZ2_RX_NKCurrencyOverride"" Type=""varchar"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability""></EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusRate_AllFields_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRateApplicability"">
			<Key>
				<PropertyRef Name=""S01_StartDate"" />
				<PropertyRef Name=""S01_EndDate"" />
				<PropertyRef Name=""S01_RateFormula"" />
				<PropertyRef Name=""S01_SelectorFormula"" />
				<PropertyRef Name=""S01_ZY1_NKRateCode"" />
				<PropertyRef Name=""S01_ZY1_ZZR_NKRateType"" />
				<PropertyRef Name=""S01_ZY1_ZZR_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""S01_ZZS_NKPreference"" />
				<PropertyRef Name=""S01_ZZS_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""S01_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""S01_RateFormulaDerivedFrom"" />
				<PropertyRef Name=""S01_RX_NKCurrencyOverride"" />
			</Key>
			<Property Name=""S01_StartDate"" Type=""smalldatetime"" />
			<Property Name=""S01_EndDate"" Type=""smalldatetime"" />
			<Property Name=""S01_RateFormula"" Type=""varchar"" />
			<Property Name=""S01_SelectorFormula"" Type=""varchar"" />
			<Property Name=""S01_ZY1_NKRateCode"" Type=""varchar"" />
			<Property Name=""S01_ZY1_ZZR_NKRateType"" Type=""varchar"" />
			<Property Name=""S01_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""S01_ZZS_NKPreference"" Type=""varchar"" />
			<Property Name=""S01_ZZS_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""S01_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""S01_RateFormulaDerivedFrom"" Type=""varchar"" />
			<Property Name=""S01_RX_NKCurrencyOverride"" Type=""varchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateWithoutApplicability"">
			<Key>
				<PropertyRef Name=""ZZ2_StartDate"" />
				<PropertyRef Name=""ZZ2_EndDate"" />
				<PropertyRef Name=""ZZ2_RateFormula"" />
				<PropertyRef Name=""ZZ2_SelectorFormula"" />
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
				<PropertyRef Name=""ZZ2_ZY1_ZZR_NKRateType"" />
				<PropertyRef Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZZ2_ZZS_NKPreference"" />
				<PropertyRef Name=""ZZ2_ZZS_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZZ2_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZZ2_RateFormulaDerivedFrom"" />
				<PropertyRef Name=""ZZ2_RX_NKCurrencyOverride"" />
			</Key>
			<Property Name=""ZZ2_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZZ2_EndDate"" Type=""smalldatetime"" />
			<Property Name=""ZZ2_RateFormula"" Type=""varchar"" />
			<Property Name=""ZZ2_SelectorFormula"" Type=""varchar"" />
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" />
			<Property Name=""ZZ2_ZY1_ZZR_NKRateType"" Type=""varchar"" />
			<Property Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""ZZ2_ZZS_NKPreference"" Type=""varchar"" />
			<Property Name=""ZZ2_ZZS_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""ZZ2_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""ZZ2_RateFormulaDerivedFrom"" Type=""varchar"" />
			<Property Name=""ZZ2_RX_NKCurrencyOverride"" Type=""varchar"" />
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusRate"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZZ2_StartDate"" />
				<PropertyRef Name=""ZZ2_EndDate"" />
				<PropertyRef Name=""ZZ2_RateFormula"" />
				<PropertyRef Name=""ZZ2_SelectorFormula"" />
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
				<PropertyRef Name=""ZZ2_ZY1_ZZR_NKRateType"" />
				<PropertyRef Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZZ2_ZZS_NKPreference"" />
				<PropertyRef Name=""ZZ2_ZZS_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZZ2_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZZ2_RateFormulaDerivedFrom"" />
				<PropertyRef Name=""ZZ2_RX_NKCurrencyOverride"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZZ2_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZZ2_EndDate"" Type=""smalldatetime"" />
			<Property Name=""ZZ2_RateFormula"" Type=""varchar"" />
			<Property Name=""ZZ2_SelectorFormula"" Type=""varchar"" />
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" />
			<Property Name=""ZZ2_ZY1_ZZR_NKRateType"" Type=""varchar"" />
			<Property Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""ZZ2_ZZS_NKPreference"" Type=""varchar"" />
			<Property Name=""ZZ2_ZZS_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""ZZ2_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""ZZ2_RateFormulaDerivedFrom"" Type=""varchar"" />
			<Property Name=""ZZ2_RX_NKCurrencyOverride"" Type=""varchar"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability""></EntityType>
	</OriginalSchema>
</UniversalReferenceData>";

		const string RefCusRateUOM_AllFields = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true""></EntityType>
		<EntityType Name=""RefCusRateUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZXG_UOM"" />
			</Key>
			<Property Name=""ZXG_UOM"" Type=""varchar"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusRateUOM_AllFields_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRateApplicability"" Data=""true"">
			<Key></Key>
		</EntityType>
		<EntityType Name=""RefCusRateApplicabilityUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""S02_UOM"" />
			</Key>
			<Property Name=""S02_UOM"" Type=""varchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateWithoutApplicability"" Data=""true"">
			<Key></Key>
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true""></EntityType>
		<EntityType Name=""RefCusRateUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZXG_UOM"" />
			</Key>
			<Property Name=""ZXG_UOM"" Type=""varchar"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>";

		const string RefCusApplicability_AllFields = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_StartDate"" />
				<PropertyRef Name=""ZZT_EndDate"" />
				<PropertyRef Name=""ZZT_ZZA_NKTradeGroup"" />
				<PropertyRef Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZZT_AdditionalCode"" />
				<PropertyRef Name=""ZZT_OrderNumber"" />
				<PropertyRef Name=""ZZT_ZZA_NKSecondTradeGroup"" />
				<PropertyRef Name=""ZZT_ZZA_ZZZ_NKSecondDataGrouping"" />
			</Key>
			<Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZZT_EndDate"" Type=""smalldatetime"" />
			<Property Name=""ZZT_ZZA_NKTradeGroup"" Type=""varchar"" />
			<Property Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
			<Property Name=""ZZT_OrderNumber"" Type=""nvarchar"" />
			<Property Name=""ZZT_ZZA_NKSecondTradeGroup"" Type=""nvarchar"" />
			<Property Name=""ZZT_ZZA_ZZZ_NKSecondDataGrouping"" Type=""nvarchar"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusApplicability_AllFields_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRateApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""S01_ZY1_NKRateCode"" />
				<PropertyRef Name=""S01_StartDate"" />
				<PropertyRef Name=""S01_EndDate"" />
				<PropertyRef Name=""S01_ZZA_NKTradeGroup"" />
				<PropertyRef Name=""S01_ZZA_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""S01_AdditionalCode"" />
				<PropertyRef Name=""S01_OrderNumber"" />
				<PropertyRef Name=""S01_ZZA_NKSecondTradeGroup"" />
				<PropertyRef Name=""S01_ZZA_ZZZ_NKSecondDataGrouping"" />
			</Key>
			<Property Name=""S01_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
			<Property Name=""S01_StartDate"" Type=""smalldatetime"" />
			<Property Name=""S01_EndDate"" Type=""smalldatetime"" />
			<Property Name=""S01_ZZA_NKTradeGroup"" Type=""varchar"" />
			<Property Name=""S01_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""S01_AdditionalCode"" Type=""nvarchar"" />
			<Property Name=""S01_OrderNumber"" Type=""nvarchar"" />
			<Property Name=""S01_ZZA_NKSecondTradeGroup"" Type=""nvarchar"" />
			<Property Name=""S01_ZZA_ZZZ_NKSecondDataGrouping"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateWithoutApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
			</Key>
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_StartDate"" />
				<PropertyRef Name=""ZZT_EndDate"" />
				<PropertyRef Name=""ZZT_ZZA_NKTradeGroup"" />
				<PropertyRef Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZZT_AdditionalCode"" />
				<PropertyRef Name=""ZZT_OrderNumber"" />
				<PropertyRef Name=""ZZT_ZZA_NKSecondTradeGroup"" />
				<PropertyRef Name=""ZZT_ZZA_ZZZ_NKSecondDataGrouping"" />
			</Key>
			<Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZZT_EndDate"" Type=""smalldatetime"" />
			<Property Name=""ZZT_ZZA_NKTradeGroup"" Type=""varchar"" />
			<Property Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
			<Property Name=""ZZT_OrderNumber"" Type=""nvarchar"" />
			<Property Name=""ZZT_ZZA_NKSecondTradeGroup"" Type=""nvarchar"" />
			<Property Name=""ZZT_ZZA_ZZZ_NKSecondDataGrouping"" Type=""nvarchar"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>";

		const string RefCusExcludedTradeGroup_AllFields = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true""></EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
				<PropertyRef Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" />
			</Key>
			<Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""varchar"" />
			<Property Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusExcludedTradeGroup_AllFields_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRateApplicability"" Data=""true"">
			<Key></Key>
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroupNew"" Data=""true"">
			<Key>
				<PropertyRef Name=""S03_ZZA_NKTradeGroup"" />
				<PropertyRef Name=""S03_ZZA_ZZZ_NKDataGrouping"" />
			</Key>
			<Property Name=""S03_ZZA_NKTradeGroup"" Type=""varchar"" />
			<Property Name=""S03_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateWithoutApplicability"" Data=""true"">
			<Key></Key>
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true""></EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
				<PropertyRef Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" />
			</Key>
			<Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""varchar"" />
			<Property Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>";

		const string RefCusApplicability_UnderBothRateAndCondition = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZX1_ZX2_NKConditionType"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusApplicability_UnderBothRateAndCondition_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRateApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""S01_ZY1_NKRateCode"" />
				<PropertyRef Name=""S01_AdditionalCode"" />
			</Key>
			<Property Name=""S01_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
			<Property Name=""S01_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""S07_ZX2_NKConditionType"" />
				<PropertyRef Name=""S07_AdditionalCode"" />
			</Key>
			<Property Name=""S07_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
			<Property Name=""S07_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateWithoutApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
			</Key>
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
		</EntityType>
		<EntityType Name=""RefCusConditionWithoutApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZX1_ZX2_NKConditionType"" />
			</Key>
			<Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZX1_ZX2_NKConditionType"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>
";

		const string RefCusRateWithUOM_ApplicabilityWithExcludedTradeGroup_RefCusApplicabilityUnderBothRateAndCondition = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""RefCusRateUOM"" />
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""RefCusRateUOM"" Type=""RefCusRateUOM"" />
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
				<PropertyRef Name=""RefCusExcludedTradeGroup"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
			<Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
		</EntityType>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZX1_ZX2_NKConditionType"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
		</EntityType>
		<EntityType Name=""RefCusRateUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZXG_UOM"" />
			</Key>
			<Property Name=""ZXG_UOM"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
			</Key>
			<Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""nvarchar"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusRateWithUOM_ApplicabilityWithExcludedTradeGroup_RefCusApplicabilityUnderBothRateAndCondition_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRateApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusRateApplicabilityUOM"" />
				<PropertyRef Name=""S01_ZY1_NKRateCode"" />
				<PropertyRef Name=""S01_AdditionalCode"" />
				<PropertyRef Name=""RefCusExcludedTradeGroupNew"" />
			</Key>
			<Property Name=""RefCusRateApplicabilityUOM"" Type=""RefCusRateApplicabilityUOM"" />
			<Property Name=""S01_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
			<Property Name=""S01_AdditionalCode"" Type=""nvarchar"" />
			<Property Name=""RefCusExcludedTradeGroupNew"" Type=""RefCusExcludedTradeGroupNew"" />
		</EntityType>
		<EntityType Name=""RefCusConditionApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""S07_ZX2_NKConditionType"" />
				<PropertyRef Name=""S07_AdditionalCode"" />
				<PropertyRef Name=""RefCusExcludedTradeGroupNew"" />
			</Key>
			<Property Name=""S07_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
			<Property Name=""S07_AdditionalCode"" Type=""nvarchar"" />
			<Property Name=""RefCusExcludedTradeGroupNew"" Type=""RefCusExcludedTradeGroupNew"" />
		</EntityType>
		<EntityType Name=""RefCusRateApplicabilityUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""S02_UOM"" />
			</Key>
			<Property Name=""S02_UOM"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroupNew"" Data=""true"">
			<Key>
				<PropertyRef Name=""S03_ZZA_NKTradeGroup"" />
			</Key>
			<Property Name=""S03_ZZA_NKTradeGroup"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateWithoutApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusRateUOM"" />
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
			</Key>
			<Property Name=""RefCusRateUOM"" Type=""RefCusRateUOM"" />
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
		</EntityType>
		<EntityType Name=""RefCusRateUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZXG_UOM"" />
			</Key>
			<Property Name=""ZXG_UOM"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionWithoutApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZX1_ZX2_NKConditionType"" />
			</Key>
			<Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""RefCusRateUOM"" />
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""RefCusRateUOM"" Type=""RefCusRateUOM"" />
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
				<PropertyRef Name=""RefCusExcludedTradeGroup"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
			<Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
		</EntityType>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZX1_ZX2_NKConditionType"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
		</EntityType>
		<EntityType Name=""RefCusRateUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZXG_UOM"" />
			</Key>
			<Property Name=""ZXG_UOM"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
			</Key>
			<Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""nvarchar"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>";

		const string RefCusTariffWithAllMappedEntities = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusTariff"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZ1_TariffCode"" />
			</Key>
			<Property Name=""RefCusRate"" Type=""RefCusRate"" />
			<Property Name=""RefCusCondition"" Type=""RefCusCondition"" />
			<Property Name=""ZZ1_TariffCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""RefCusRateUOM"" Type=""RefCusRateUOM"" />
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
			<Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
		</EntityType>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZX1_ZX2_NKConditionType"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
		</EntityType>
		<EntityType Name=""RefCusRateUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZXG_UOM"" />
			</Key>
			<Property Name=""ZXG_UOM"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
			</Key>
			<Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""nvarchar"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusTariffWithAllMappedEntities_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusTariff"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZ1_TariffCode"" />
			</Key>
			<Property Name=""RefCusRateWithoutApplicability"" Type=""RefCusRateWithoutApplicability"" />
			<Property Name=""RefCusRateApplicability"" Type=""RefCusRateApplicability"" />
			<Property Name=""RefCusConditionWithoutApplicability"" Type=""RefCusConditionWithoutApplicability"" />
			<Property Name=""RefCusConditionApplicability"" Type=""RefCusConditionApplicability"" />
			<Property Name=""ZZ1_TariffCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""S01_ZY1_NKRateCode"" />
				<PropertyRef Name=""S01_AdditionalCode"" />
			</Key>
			<Property Name=""RefCusRateApplicabilityUOM"" Type=""RefCusRateApplicabilityUOM"" />
			<Property Name=""S01_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
			<Property Name=""S01_AdditionalCode"" Type=""nvarchar"" />
			<Property Name=""RefCusExcludedTradeGroupNew"" Type=""RefCusExcludedTradeGroupNew"" />
		</EntityType>
		<EntityType Name=""RefCusConditionApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""S07_ZX2_NKConditionType"" />
				<PropertyRef Name=""S07_AdditionalCode"" />
			</Key>
			<Property Name=""S07_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
			<Property Name=""S07_AdditionalCode"" Type=""nvarchar"" />
			<Property Name=""RefCusExcludedTradeGroupNew"" Type=""RefCusExcludedTradeGroupNew"" />
		</EntityType>
		<EntityType Name=""RefCusRateApplicabilityUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""S02_UOM"" />
			</Key>
			<Property Name=""S02_UOM"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroupNew"" Data=""true"">
			<Key>
				<PropertyRef Name=""S03_ZZA_NKTradeGroup"" />
			</Key>
			<Property Name=""S03_ZZA_NKTradeGroup"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateWithoutApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
			</Key>
			<Property Name=""RefCusRateUOM"" Type=""RefCusRateUOM"" />
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
		</EntityType>
		<EntityType Name=""RefCusRateUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZXG_UOM"" />
			</Key>
			<Property Name=""ZXG_UOM"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionWithoutApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZX1_ZX2_NKConditionType"" />
			</Key>
			<Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusTariff"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZ1_TariffCode"" />
			</Key>
			<Property Name=""RefCusRate"" Type=""RefCusRate"" />
			<Property Name=""RefCusCondition"" Type=""RefCusCondition"" />
			<Property Name=""ZZ1_TariffCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""RefCusRateUOM"" Type=""RefCusRateUOM"" />
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
			<Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
		</EntityType>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZX1_ZX2_NKConditionType"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
		</EntityType>
		<EntityType Name=""RefCusRateUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZXG_UOM"" />
			</Key>
			<Property Name=""ZXG_UOM"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
			</Key>
			<Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""nvarchar"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>";

		const string RefCusRateWithApplicability_CombinedEntity = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZZ2_RateFormula"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZZ2_RateFormula"" Type=""varchar"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusRateWithApplicability_CombinedEntity_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRateApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""S01_RateFormula"" />
				<PropertyRef Name=""S01_AdditionalCode"" />
			</Key>
			<Property Name=""S01_RateFormula"" Type=""varchar"" />
			<Property Name=""S01_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateWithoutApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZ2_RateFormula"" />
			</Key>
			<Property Name=""ZZ2_RateFormula"" Type=""varchar"" />
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZZ2_RateFormula"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZZ2_RateFormula"" Type=""varchar"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>";

		const string RefCusRateWithApplicability_DuplicateStartAndEndDate = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZZ2_StartDate"" />
				<PropertyRef Name=""ZZ2_EndDate"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZZ2_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZZ2_EndDate"" Type=""smalldatetime"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_StartDate"" />
				<PropertyRef Name=""ZZT_EndDate"" />
			</Key>
			<Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZZT_EndDate"" Type=""smalldatetime"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusRateWithApplicability_DuplicateStartAndEndDate_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRateApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""S01_StartDate"" />
				<PropertyRef Name=""S01_EndDate"" />
			</Key>
			<Property Name=""S01_StartDate"" Type=""smalldatetime"" />
			<Property Name=""S01_EndDate"" Type=""smalldatetime"" />
		</EntityType>
		<EntityType Name=""RefCusRateWithoutApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZ2_StartDate"" />
				<PropertyRef Name=""ZZ2_EndDate"" />
			</Key>
			<Property Name=""ZZ2_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZZ2_EndDate"" Type=""smalldatetime"" />
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZZ2_StartDate"" />
				<PropertyRef Name=""ZZ2_EndDate"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZZ2_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZZ2_EndDate"" Type=""smalldatetime"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_StartDate"" />
				<PropertyRef Name=""ZZT_EndDate"" />
			</Key>
			<Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZZT_EndDate"" Type=""smalldatetime"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>";

		const string RefCusCondition_AllFields = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusCondition"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZX1_StartDate"" />
				<PropertyRef Name=""ZX1_EndDate"" />
				<PropertyRef Name=""ZX1_Source"" />
				<PropertyRef Name=""ZX1_Comment"" />
				<PropertyRef Name=""ZX1_IsImport"" />
				<PropertyRef Name=""ZX1_IsExport"" />
				<PropertyRef Name=""ZX1_ConditionValueTrueMeansStop"" />
				<PropertyRef Name=""ZX1_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZX1_LogicalANDWithinGroup"" />
				<PropertyRef Name=""ZX1_ZY7_NKConditionCode"" />
				<PropertyRef Name=""ZX1_AdditionalComment"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZX1_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZX1_EndDate"" Type=""smalldatetime"" />
			<Property Name=""ZX1_Source"" Type=""nvarchar"" />
			<Property Name=""ZX1_Comment"" Type=""nvarchar"" />
			<Property Name=""ZX1_IsImport"" Type=""bit"" />
			<Property Name=""ZX1_IsExport"" Type=""bit"" />
			<Property Name=""ZX1_ConditionValueTrueMeansStop"" Type=""bit"" />
			<Property Name=""ZX1_ZZZ_NKDataGrouping"" Type=""nvarchar"" />
			<Property Name=""ZX1_LogicalANDWithinGroup"" Type=""nvarchar"" />
			<Property Name=""ZX1_ZY7_NKConditionCode"" Type=""nvarchar"" />
			<Property Name=""ZX1_AdditionalComment"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability""></EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusCondition_AllFields_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusConditionApplicability"">
			<Key>
				<PropertyRef Name=""S07_StartDate"" />
				<PropertyRef Name=""S07_EndDate"" />
				<PropertyRef Name=""S07_Source"" />
				<PropertyRef Name=""S07_Comment"" />
				<PropertyRef Name=""S07_IsImport"" />
				<PropertyRef Name=""S07_IsExport"" />
				<PropertyRef Name=""S07_ConditionValueTrueMeansStop"" />
				<PropertyRef Name=""S07_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""S07_LogicalANDWithinGroup"" />
				<PropertyRef Name=""S07_ZY7_NKConditionCode"" />
				<PropertyRef Name=""S07_AdditionalComment"" />
			</Key>
			<Property Name=""S07_StartDate"" Type=""smalldatetime"" />
			<Property Name=""S07_EndDate"" Type=""smalldatetime"" />
			<Property Name=""S07_Source"" Type=""nvarchar"" />
			<Property Name=""S07_Comment"" Type=""nvarchar"" />
			<Property Name=""S07_IsImport"" Type=""bit"" />
			<Property Name=""S07_IsExport"" Type=""bit"" />
			<Property Name=""S07_ConditionValueTrueMeansStop"" Type=""bit"" />
			<Property Name=""S07_ZZZ_NKDataGrouping"" Type=""nvarchar"" />
			<Property Name=""S07_LogicalANDWithinGroup"" Type=""nvarchar"" />
			<Property Name=""S07_ZY7_NKConditionCode"" Type=""nvarchar"" />
			<Property Name=""S07_AdditionalComment"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionWithoutApplicability"">
			<Key>
				<PropertyRef Name=""ZX1_StartDate"" />
				<PropertyRef Name=""ZX1_EndDate"" />
				<PropertyRef Name=""ZX1_Source"" />
				<PropertyRef Name=""ZX1_Comment"" />
				<PropertyRef Name=""ZX1_IsImport"" />
				<PropertyRef Name=""ZX1_IsExport"" />
				<PropertyRef Name=""ZX1_ConditionValueTrueMeansStop"" />
				<PropertyRef Name=""ZX1_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZX1_LogicalANDWithinGroup"" />
				<PropertyRef Name=""ZX1_ZY7_NKConditionCode"" />
				<PropertyRef Name=""ZX1_AdditionalComment"" />
			</Key>
			<Property Name=""ZX1_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZX1_EndDate"" Type=""smalldatetime"" />
			<Property Name=""ZX1_Source"" Type=""nvarchar"" />
			<Property Name=""ZX1_Comment"" Type=""nvarchar"" />
			<Property Name=""ZX1_IsImport"" Type=""bit"" />
			<Property Name=""ZX1_IsExport"" Type=""bit"" />
			<Property Name=""ZX1_ConditionValueTrueMeansStop"" Type=""bit"" />
			<Property Name=""ZX1_ZZZ_NKDataGrouping"" Type=""nvarchar"" />
			<Property Name=""ZX1_LogicalANDWithinGroup"" Type=""nvarchar"" />
			<Property Name=""ZX1_ZY7_NKConditionCode"" Type=""nvarchar"" />
			<Property Name=""ZX1_AdditionalComment"" Type=""nvarchar"" />
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusCondition"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZX1_StartDate"" />
				<PropertyRef Name=""ZX1_EndDate"" />
				<PropertyRef Name=""ZX1_Source"" />
				<PropertyRef Name=""ZX1_Comment"" />
				<PropertyRef Name=""ZX1_IsImport"" />
				<PropertyRef Name=""ZX1_IsExport"" />
				<PropertyRef Name=""ZX1_ConditionValueTrueMeansStop"" />
				<PropertyRef Name=""ZX1_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZX1_LogicalANDWithinGroup"" />
				<PropertyRef Name=""ZX1_ZY7_NKConditionCode"" />
				<PropertyRef Name=""ZX1_AdditionalComment"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZX1_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZX1_EndDate"" Type=""smalldatetime"" />
			<Property Name=""ZX1_Source"" Type=""nvarchar"" />
			<Property Name=""ZX1_Comment"" Type=""nvarchar"" />
			<Property Name=""ZX1_IsImport"" Type=""bit"" />
			<Property Name=""ZX1_IsExport"" Type=""bit"" />
			<Property Name=""ZX1_ConditionValueTrueMeansStop"" Type=""bit"" />
			<Property Name=""ZX1_ZZZ_NKDataGrouping"" Type=""nvarchar"" />
			<Property Name=""ZX1_LogicalANDWithinGroup"" Type=""nvarchar"" />
			<Property Name=""ZX1_ZY7_NKConditionCode"" Type=""nvarchar"" />
			<Property Name=""ZX1_AdditionalComment"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability""></EntityType>
	</OriginalSchema>
</UniversalReferenceData>";

		const string RefCusConditionValue_AllFields = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusCondition"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""RefCusConditionValue"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""RefCusConditionValue"" Type=""RefCusConditionValue"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability""></EntityType>
		<EntityType Name=""RefCusConditionValue"">
			<Key>
				<PropertyRef Name=""ZX3_Value"" />
				<PropertyRef Name=""ZX3_LogicalORWithinGroup"" />
				<PropertyRef Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZX3_ZX4_NKValueType"" />
			</Key>
			<Property Name=""ZX3_Value"" Type=""nvarchar"" />
			<Property Name=""ZX3_LogicalORWithinGroup"" Type=""nvarchar"" />
			<Property Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" Type=""nvarchar"" />
			<Property Name=""ZX3_ZX4_NKValueType"" Type=""nvarchar"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusConditionValue_AllFields_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusConditionApplicability"">
			<Key>
				<PropertyRef Name=""RefCusConditionApplicabilityValue"" />
			</Key>
			<Property Name=""RefCusConditionApplicabilityValue"" Type=""RefCusConditionApplicabilityValue"" />
		</EntityType>
		<EntityType Name=""RefCusConditionApplicabilityValue"">
			<Key>
				<PropertyRef Name=""S08_Value"" />
				<PropertyRef Name=""S08_LogicalORWithinGroup"" />
				<PropertyRef Name=""S08_ZX4_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""S08_ZX4_NKValueType"" />
			</Key>
			<Property Name=""S08_Value"" Type=""nvarchar"" />
			<Property Name=""S08_LogicalORWithinGroup"" Type=""nvarchar"" />
			<Property Name=""S08_ZX4_ZZZ_NKDataGrouping"" Type=""nvarchar"" />
			<Property Name=""S08_ZX4_NKValueType"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionWithoutApplicability"">
			<Key>
				<PropertyRef Name=""RefCusConditionValue"" />
			</Key>
			<Property Name=""RefCusConditionValue"" Type=""RefCusConditionValue"" />
		</EntityType>
		<EntityType Name=""RefCusConditionValue"">
			<Key>
				<PropertyRef Name=""ZX3_Value"" />
				<PropertyRef Name=""ZX3_LogicalORWithinGroup"" />
				<PropertyRef Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZX3_ZX4_NKValueType"" />
			</Key>
			<Property Name=""ZX3_Value"" Type=""nvarchar"" />
			<Property Name=""ZX3_LogicalORWithinGroup"" Type=""nvarchar"" />
			<Property Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" Type=""nvarchar"" />
			<Property Name=""ZX3_ZX4_NKValueType"" Type=""nvarchar"" />
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusCondition"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""RefCusConditionValue"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""RefCusConditionValue"" Type=""RefCusConditionValue"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability""></EntityType>
		<EntityType Name=""RefCusConditionValue"">
			<Key>
				<PropertyRef Name=""ZX3_Value"" />
				<PropertyRef Name=""ZX3_LogicalORWithinGroup"" />
				<PropertyRef Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZX3_ZX4_NKValueType"" />
			</Key>
			<Property Name=""ZX3_Value"" Type=""nvarchar"" />
			<Property Name=""ZX3_LogicalORWithinGroup"" Type=""nvarchar"" />
			<Property Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" Type=""nvarchar"" />
			<Property Name=""ZX3_ZX4_NKValueType"" Type=""nvarchar"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>";

		const string RefCusConditionLanguage_AllFields = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusCondition"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""RefCusConditionLanguage"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""RefCusConditionLanguage"" Type=""RefCusConditionLanguage"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability""></EntityType>
		<EntityType Name=""RefCusConditionLanguage"">
			<Key>
				<PropertyRef Name=""ZXJ_ZX6_NKLanguage"" />
				<PropertyRef Name=""ZXJ_Comment"" />
				<PropertyRef Name=""ZXJ_Source"" />
				<PropertyRef Name=""ZXJ_AdditionalComment"" />
			</Key>
			<Property Name=""ZXJ_ZX6_NKLanguage"" Type=""nvarchar"" />
			<Property Name=""ZXJ_Comment"" Type=""nvarchar"" />
			<Property Name=""ZXJ_Source"" Type=""nvarchar"" />
			<Property Name=""ZXJ_AdditionalComment"" Type=""nvarchar"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusConditionLanguage_AllFields_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusConditionApplicability"">
			<Key>
				<PropertyRef Name=""RefCusConditionApplicabilityLanguage"" />
			</Key>
			<Property Name=""RefCusConditionApplicabilityLanguage"" Type=""RefCusConditionApplicabilityLanguage"" />
		</EntityType>
		<EntityType Name=""RefCusConditionApplicabilityLanguage"">
			<Key>
				<PropertyRef Name=""S09_ZX6_NKLanguage"" />
				<PropertyRef Name=""S09_Comment"" />
				<PropertyRef Name=""S09_Source"" />
				<PropertyRef Name=""S09_AdditionalComment"" />
			</Key>
			<Property Name=""S09_ZX6_NKLanguage"" Type=""nvarchar"" />
			<Property Name=""S09_Comment"" Type=""nvarchar"" />
			<Property Name=""S09_Source"" Type=""nvarchar"" />
			<Property Name=""S09_AdditionalComment"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionWithoutApplicability"">
			<Key>
				<PropertyRef Name=""RefCusConditionLanguage"" />
			</Key>
			<Property Name=""RefCusConditionLanguage"" Type=""RefCusConditionLanguage"" />
		</EntityType>
		<EntityType Name=""RefCusConditionLanguage"">
			<Key>
				<PropertyRef Name=""ZXJ_ZX6_NKLanguage"" />
				<PropertyRef Name=""ZXJ_Comment"" />
				<PropertyRef Name=""ZXJ_Source"" />
				<PropertyRef Name=""ZXJ_AdditionalComment"" />
			</Key>
			<Property Name=""ZXJ_ZX6_NKLanguage"" Type=""nvarchar"" />
			<Property Name=""ZXJ_Comment"" Type=""nvarchar"" />
			<Property Name=""ZXJ_Source"" Type=""nvarchar"" />
			<Property Name=""ZXJ_AdditionalComment"" Type=""nvarchar"" />
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusCondition"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""RefCusConditionLanguage"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""RefCusConditionLanguage"" Type=""RefCusConditionLanguage"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability""></EntityType>
		<EntityType Name=""RefCusConditionLanguage"">
			<Key>
				<PropertyRef Name=""ZXJ_ZX6_NKLanguage"" />
				<PropertyRef Name=""ZXJ_Comment"" />
				<PropertyRef Name=""ZXJ_Source"" />
				<PropertyRef Name=""ZXJ_AdditionalComment"" />
			</Key>
			<Property Name=""ZXJ_ZX6_NKLanguage"" Type=""nvarchar"" />
			<Property Name=""ZXJ_Comment"" Type=""nvarchar"" />
			<Property Name=""ZXJ_Source"" Type=""nvarchar"" />
			<Property Name=""ZXJ_AdditionalComment"" Type=""nvarchar"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>";

		const string RefCusApplicability_UnderCondition_AllFields = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_StartDate"" />
				<PropertyRef Name=""ZZT_EndDate"" />
				<PropertyRef Name=""ZZT_ZZA_NKTradeGroup"" />
				<PropertyRef Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZZT_AdditionalCode"" />
				<PropertyRef Name=""ZZT_OrderNumber"" />
				<PropertyRef Name=""ZZT_ZZA_NKSecondTradeGroup"" />
				<PropertyRef Name=""ZZT_ZZA_ZZZ_NKSecondDataGrouping"" />
			</Key>
			<Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZZT_EndDate"" Type=""smalldatetime"" />
			<Property Name=""ZZT_ZZA_NKTradeGroup"" Type=""varchar"" />
			<Property Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
			<Property Name=""ZZT_OrderNumber"" Type=""nvarchar"" />
			<Property Name=""ZZT_ZZA_NKSecondTradeGroup"" Type=""nvarchar"" />
			<Property Name=""ZZT_ZZA_ZZZ_NKSecondDataGrouping"" Type=""nvarchar"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusApplicability_UnderCondition_AllFields_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusConditionApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""S07_StartDate"" />
				<PropertyRef Name=""S07_EndDate"" />
				<PropertyRef Name=""S07_ZZA_NKTradeGroup"" />
				<PropertyRef Name=""S07_ZZA_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""S07_AdditionalCode"" />
				<PropertyRef Name=""S07_OrderNumber"" />
				<PropertyRef Name=""S07_ZZA_NKSecondTradeGroup"" />
				<PropertyRef Name=""S07_ZZA_ZZZ_NKSecondDataGrouping"" />
			</Key>
			<Property Name=""S07_StartDate"" Type=""smalldatetime"" />
			<Property Name=""S07_EndDate"" Type=""smalldatetime"" />
			<Property Name=""S07_ZZA_NKTradeGroup"" Type=""varchar"" />
			<Property Name=""S07_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""S07_AdditionalCode"" Type=""nvarchar"" />
			<Property Name=""S07_OrderNumber"" Type=""nvarchar"" />
			<Property Name=""S07_ZZA_NKSecondTradeGroup"" Type=""nvarchar"" />
			<Property Name=""S07_ZZA_ZZZ_NKSecondDataGrouping"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionWithoutApplicability"" Data=""true"">
			<Key></Key>
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_StartDate"" />
				<PropertyRef Name=""ZZT_EndDate"" />
				<PropertyRef Name=""ZZT_ZZA_NKTradeGroup"" />
				<PropertyRef Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZZT_AdditionalCode"" />
				<PropertyRef Name=""ZZT_OrderNumber"" />
				<PropertyRef Name=""ZZT_ZZA_NKSecondTradeGroup"" />
				<PropertyRef Name=""ZZT_ZZA_ZZZ_NKSecondDataGrouping"" />
			</Key>
			<Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZZT_EndDate"" Type=""smalldatetime"" />
			<Property Name=""ZZT_ZZA_NKTradeGroup"" Type=""varchar"" />
			<Property Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
			<Property Name=""ZZT_OrderNumber"" Type=""nvarchar"" />
			<Property Name=""ZZT_ZZA_NKSecondTradeGroup"" Type=""nvarchar"" />
			<Property Name=""ZZT_ZZA_ZZZ_NKSecondDataGrouping"" Type=""nvarchar"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>";

		const string RefCusCondition_WithValueAndLanguage_RefCusApplicabilityUnderBothRateAndCondition = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusCondition"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""RefCusConditionValue"" />
				<PropertyRef Name=""RefCusConditionLanguage"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""RefCusConditionValue"" Type=""RefCusConditionValue"" />
			<Property Name=""RefCusConditionLanguage"" Type=""RefCusConditionLanguage"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionValue"">
			<Key>
				<PropertyRef Name=""ZX3_Value"" />
				<PropertyRef Name=""ZX3_LogicalORWithinGroup"" />
				<PropertyRef Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZX3_ZX4_NKValueType"" />
			</Key>
			<Property Name=""ZX3_Value"" Type=""nvarchar"" />
			<Property Name=""ZX3_LogicalORWithinGroup"" Type=""nvarchar"" />
			<Property Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" Type=""nvarchar"" />
			<Property Name=""ZX3_ZX4_NKValueType"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionLanguage"">
			<Key>
				<PropertyRef Name=""ZXJ_ZX6_NKLanguage"" />
				<PropertyRef Name=""ZXJ_Comment"" />
				<PropertyRef Name=""ZXJ_Source"" />
				<PropertyRef Name=""ZXJ_AdditionalComment"" />
			</Key>
			<Property Name=""ZXJ_ZX6_NKLanguage"" Type=""nvarchar"" />
			<Property Name=""ZXJ_Comment"" Type=""nvarchar"" />
			<Property Name=""ZXJ_Source"" Type=""nvarchar"" />
			<Property Name=""ZXJ_AdditionalComment"" Type=""nvarchar"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusCondition_WithValueAndLanguage_RefCusApplicabilityUnderBothRateAndCondition_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRateApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""S01_AdditionalCode"" />
			</Key>
			<Property Name=""S01_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionApplicability"">
			<Key>
				<PropertyRef Name=""RefCusConditionApplicabilityValue"" />
				<PropertyRef Name=""RefCusConditionApplicabilityLanguage"" />
				<PropertyRef Name=""S07_AdditionalCode"" />
			</Key>
			<Property Name=""RefCusConditionApplicabilityValue"" Type=""RefCusConditionApplicabilityValue"" />
			<Property Name=""RefCusConditionApplicabilityLanguage"" Type=""RefCusConditionApplicabilityLanguage"" />
			<Property Name=""S07_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionApplicabilityValue"">
			<Key>
				<PropertyRef Name=""S08_Value"" />
				<PropertyRef Name=""S08_LogicalORWithinGroup"" />
				<PropertyRef Name=""S08_ZX4_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""S08_ZX4_NKValueType"" />
			</Key>
			<Property Name=""S08_Value"" Type=""nvarchar"" />
			<Property Name=""S08_LogicalORWithinGroup"" Type=""nvarchar"" />
			<Property Name=""S08_ZX4_ZZZ_NKDataGrouping"" Type=""nvarchar"" />
			<Property Name=""S08_ZX4_NKValueType"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionApplicabilityLanguage"">
			<Key>
				<PropertyRef Name=""S09_ZX6_NKLanguage"" />
				<PropertyRef Name=""S09_Comment"" />
				<PropertyRef Name=""S09_Source"" />
				<PropertyRef Name=""S09_AdditionalComment"" />
			</Key>
			<Property Name=""S09_ZX6_NKLanguage"" Type=""nvarchar"" />
			<Property Name=""S09_Comment"" Type=""nvarchar"" />
			<Property Name=""S09_Source"" Type=""nvarchar"" />
			<Property Name=""S09_AdditionalComment"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateWithoutApplicability"" Data=""true"">
			<Key></Key>
		</EntityType>
		<EntityType Name=""RefCusConditionWithoutApplicability"">
			<Key>
				<PropertyRef Name=""RefCusConditionValue"" />
				<PropertyRef Name=""RefCusConditionLanguage"" />
			</Key>
			<Property Name=""RefCusConditionValue"" Type=""RefCusConditionValue"" />
			<Property Name=""RefCusConditionLanguage"" Type=""RefCusConditionLanguage"" />
		</EntityType>
		<EntityType Name=""RefCusConditionValue"">
			<Key>
				<PropertyRef Name=""ZX3_Value"" />
				<PropertyRef Name=""ZX3_LogicalORWithinGroup"" />
				<PropertyRef Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZX3_ZX4_NKValueType"" />
			</Key>
			<Property Name=""ZX3_Value"" Type=""nvarchar"" />
			<Property Name=""ZX3_LogicalORWithinGroup"" Type=""nvarchar"" />
			<Property Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" Type=""nvarchar"" />
			<Property Name=""ZX3_ZX4_NKValueType"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionLanguage"">
			<Key>
				<PropertyRef Name=""ZXJ_ZX6_NKLanguage"" />
				<PropertyRef Name=""ZXJ_Comment"" />
				<PropertyRef Name=""ZXJ_Source"" />
				<PropertyRef Name=""ZXJ_AdditionalComment"" />
			</Key>
			<Property Name=""ZXJ_ZX6_NKLanguage"" Type=""nvarchar"" />
			<Property Name=""ZXJ_Comment"" Type=""nvarchar"" />
			<Property Name=""ZXJ_Source"" Type=""nvarchar"" />
			<Property Name=""ZXJ_AdditionalComment"" Type=""nvarchar"" />
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusCondition"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""RefCusConditionValue"" />
				<PropertyRef Name=""RefCusConditionLanguage"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""RefCusConditionValue"" Type=""RefCusConditionValue"" />
			<Property Name=""RefCusConditionLanguage"" Type=""RefCusConditionLanguage"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionValue"">
			<Key>
				<PropertyRef Name=""ZX3_Value"" />
				<PropertyRef Name=""ZX3_LogicalORWithinGroup"" />
				<PropertyRef Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZX3_ZX4_NKValueType"" />
			</Key>
			<Property Name=""ZX3_Value"" Type=""nvarchar"" />
			<Property Name=""ZX3_LogicalORWithinGroup"" Type=""nvarchar"" />
			<Property Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" Type=""nvarchar"" />
			<Property Name=""ZX3_ZX4_NKValueType"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionLanguage"">
			<Key>
				<PropertyRef Name=""ZXJ_ZX6_NKLanguage"" />
				<PropertyRef Name=""ZXJ_Comment"" />
				<PropertyRef Name=""ZXJ_Source"" />
				<PropertyRef Name=""ZXJ_AdditionalComment"" />
			</Key>
			<Property Name=""ZXJ_ZX6_NKLanguage"" Type=""nvarchar"" />
			<Property Name=""ZXJ_Comment"" Type=""nvarchar"" />
			<Property Name=""ZXJ_Source"" Type=""nvarchar"" />
			<Property Name=""ZXJ_AdditionalComment"" Type=""nvarchar"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>";

		const string RefCusConditionWithApplicability_DuplicateStartAndEndDate = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZX1_StartDate"" />
				<PropertyRef Name=""ZX1_EndDate"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZX1_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZX1_EndDate"" Type=""smalldatetime"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_StartDate"" />
				<PropertyRef Name=""ZZT_EndDate"" />
			</Key>
			<Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZZT_EndDate"" Type=""smalldatetime"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusConditionWithApplicability_DuplicateStartAndEndDate_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusConditionApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""S07_StartDate"" />
				<PropertyRef Name=""S07_EndDate"" />
			</Key>
			<Property Name=""S07_StartDate"" Type=""smalldatetime"" />
			<Property Name=""S07_EndDate"" Type=""smalldatetime"" />
		</EntityType>
		<EntityType Name=""RefCusConditionWithoutApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZX1_StartDate"" />
				<PropertyRef Name=""ZX1_EndDate"" />
			</Key>
			<Property Name=""ZX1_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZX1_EndDate"" Type=""smalldatetime"" />
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZX1_StartDate"" />
				<PropertyRef Name=""ZX1_EndDate"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""ZX1_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZX1_EndDate"" Type=""smalldatetime"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_StartDate"" />
				<PropertyRef Name=""ZZT_EndDate"" />
			</Key>
			<Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZZT_EndDate"" Type=""smalldatetime"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>";

		const string RefCusApplicability_UnderRate_Condition_AdditionalCode = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusTariffAdditionalCode"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusApplicability_UnderRate_Condition_AdditionalCode_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRateApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""S01_AdditionalCode"" />
			</Key>
			<Property Name=""S01_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""S07_AdditionalCode"" />
			</Key>
			<Property Name=""S07_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusTariffAdditionalCode"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusRateWithoutApplicability"" Data=""true"">
			<Key></Key>
		</EntityType>
		<EntityType Name=""RefCusConditionWithoutApplicability"" Data=""true"">
			<Key></Key>
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusTariffAdditionalCode"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>
";

		const string RefCusApplicability_UnderRate_AdditionalCode = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusTariffAdditionalCode"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusApplicability_UnderRate_AdditionalCode_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRateApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""S01_AdditionalCode"" />
			</Key>
			<Property Name=""S01_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusTariffAdditionalCode"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusRateWithoutApplicability"" Data=""true"">
			<Key></Key>
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusTariffAdditionalCode"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>
";

		const string RefCusApplicability_UnderCondition_AdditionalCode = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusTariffAdditionalCode"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusApplicability_UnderCondition_AdditionalCode_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""S07_AdditionalCode"" />
			</Key>
			<Property Name=""S07_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusTariffAdditionalCode"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusConditionWithoutApplicability"" Data=""true"">
			<Key></Key>
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusTariffAdditionalCode"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>
";

		const string RefCusRateWithoutApp_ChildEntities = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""RefCusRateUOM"" />
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""RefCusRateUOM"" Type=""RefCusRateUOM"" />
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZXG_UOM"" />
			</Key>
			<Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
			<Property Name=""RefCusRateUOMInvalid.INV_Name"" Type=""RefCusRateUOMInvalid"" />
			<Property Name=""ZXG_UOM"" Type=""varchar"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
			</Key>
			<Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateUOMInvalid"" Data=""true"">
			<Key>
				<PropertyRef Name=""INV_Name"" />
			</Key>
			<Property Name=""INV_Name"" Type=""varchar"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusRateWithoutApp_ChildEntities_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusRateApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusRateApplicabilityUOM"" />
				<PropertyRef Name=""S01_ZY1_NKRateCode"" />
				<PropertyRef Name=""S01_AdditionalCode"" />
			</Key>
			<Property Name=""RefCusRateApplicabilityUOM"" Type=""RefCusRateApplicabilityUOM"" />
			<Property Name=""S01_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
			<Property Name=""S01_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateApplicabilityUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""S02_UOM"" />
			</Key>
			<Property Name=""RefCusExcludedTradeGroupNew"" Type=""RefCusExcludedTradeGroupNew"" />
			<Property Name=""RefCusRateUOMInvalid.INV_Name"" Type=""RefCusRateUOMInvalid"" />
			<Property Name=""S02_UOM"" Type=""varchar"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroupNew"" Data=""true"">
			<Key>
				<PropertyRef Name=""S03_ZZA_NKTradeGroup"" />
			</Key>
			<Property Name=""S03_ZZA_NKTradeGroup"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateUOMInvalid"" Data=""true"">
			<Key>
				<PropertyRef Name=""INV_Name"" />
			</Key>
			<Property Name=""INV_Name"" Type=""varchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateWithoutApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusRateUOM"" />
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
			</Key>
			<Property Name=""RefCusRateUOM"" Type=""RefCusRateUOM"" />
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
		</EntityType>
		<EntityType Name=""RefCusRateUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZXG_UOM"" />
			</Key>
			<Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
			<Property Name=""RefCusRateUOMInvalid.INV_Name"" Type=""RefCusRateUOMInvalid"" />
			<Property Name=""ZXG_UOM"" Type=""varchar"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
			</Key>
			<Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""nvarchar"" />
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""RefCusRateUOM"" />
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""RefCusRateUOM"" Type=""RefCusRateUOM"" />
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateUOM"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZXG_UOM"" />
			</Key>
			<Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
			<Property Name=""RefCusRateUOMInvalid.INV_Name"" Type=""RefCusRateUOMInvalid"" />
			<Property Name=""ZXG_UOM"" Type=""varchar"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
			</Key>
			<Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusRateUOMInvalid"" Data=""true"">
			<Key>
				<PropertyRef Name=""INV_Name"" />
			</Key>
			<Property Name=""INV_Name"" Type=""varchar"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>";

		const string RefCusConditionWithoutApp_ChildEntities = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZX1_ZX2_NKConditionType"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""RefCusConditionValue"" Type=""RefCusConditionValue"" />
			<Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
		</EntityType>
		<EntityType Name=""RefCusConditionValue"">
			<Key>
				<PropertyRef Name=""RefCusExcludedTradeGroup"" />
				<PropertyRef Name=""ZX3_Value"" />
			</Key>
			<Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
			<Property Name=""RefCusConditionValueInvalid.INV_Name"" Type=""RefCusConditionValueInvalid"" />
			<Property Name=""ZX3_Value"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
			</Key>
			<Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionValueInvalid"" Data=""true"">
			<Key>
				<PropertyRef Name=""INV_Name"" />
			</Key>
			<Property Name=""INV_Name"" Type=""varchar"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>";
		const string RefCusConditionWithoutApp_ChildEntities_ExpectedResult = @"
<UniversalReferenceData>
	<Schema>
		<EntityType Name=""RefCusConditionApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""S07_ZX2_NKConditionType"" />
				<PropertyRef Name=""S07_AdditionalCode"" />
			</Key>
			<Property Name=""RefCusConditionApplicabilityValue"" Type=""RefCusConditionApplicabilityValue"" />
			<Property Name=""S07_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
			<Property Name=""S07_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionApplicabilityValue"">
			<Key>
				<PropertyRef Name=""RefCusExcludedTradeGroupNew"" />
				<PropertyRef Name=""S08_Value"" />
			</Key>
			<Property Name=""RefCusExcludedTradeGroupNew"" Type=""RefCusExcludedTradeGroupNew"" />
			<Property Name=""RefCusConditionValueInvalid.INV_Name"" Type=""RefCusConditionValueInvalid"" />
			<Property Name=""S08_Value"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroupNew"" Data=""true"">
			<Key>
				<PropertyRef Name=""S03_ZZA_NKTradeGroup"" />
			</Key>
			<Property Name=""S03_ZZA_NKTradeGroup"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionValueInvalid"" Data=""true"">
			<Key>
				<PropertyRef Name=""INV_Name"" />
			</Key>
			<Property Name=""INV_Name"" Type=""varchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionWithoutApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZX1_ZX2_NKConditionType"" />
			</Key>
			<Property Name=""RefCusConditionValue"" Type=""RefCusConditionValue"" />
			<Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
		</EntityType>
		<EntityType Name=""RefCusConditionValue"">
			<Key>
				<PropertyRef Name=""RefCusExcludedTradeGroup"" />
				<PropertyRef Name=""ZX3_Value"" />
			</Key>
			<Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
			<Property Name=""RefCusConditionValueInvalid.INV_Name"" Type=""RefCusConditionValueInvalid"" />
			<Property Name=""ZX3_Value"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
			</Key>
			<Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""nvarchar"" />
		</EntityType>
	</Schema>
	<OriginalSchema>
		<EntityType Name=""RefCusCondition"" Data=""true"">
			<Key>
				<PropertyRef Name=""RefCusApplicability"" />
				<PropertyRef Name=""ZX1_ZX2_NKConditionType"" />
			</Key>
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
			<Property Name=""RefCusConditionValue"" Type=""RefCusConditionValue"" />
			<Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
		</EntityType>
		<EntityType Name=""RefCusConditionValue"">
			<Key>
				<PropertyRef Name=""RefCusExcludedTradeGroup"" />
				<PropertyRef Name=""ZX3_Value"" />
			</Key>
			<Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
			<Property Name=""RefCusConditionValueInvalid.INV_Name"" Type=""RefCusConditionValueInvalid"" />
			<Property Name=""ZX3_Value"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
			</Key>
			<Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""nvarchar"" />
		</EntityType>
		<EntityType Name=""RefCusConditionValueInvalid"" Data=""true"">
			<Key>
				<PropertyRef Name=""INV_Name"" />
			</Key>
			<Property Name=""INV_Name"" Type=""varchar"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
		</EntityType>
	</OriginalSchema>
</UniversalReferenceData>";
	}
}
