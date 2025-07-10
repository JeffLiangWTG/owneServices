using System.Collections.Generic;
using System.Xml;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Test.Transform
{
	[TestFixture]
	class UniversalXmlTransformerValidatorFixture
	{
		[TestCaseSource(nameof(TransformFullyTransform_TestCases_CondAndApp))]
		public TransformMode TransformFullyTransformCheck_CondAndApp(string schemaXml)
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(schemaXml);
			var validator = new UniversalXmlTransformerValidator();
			return validator.GetTransformMode(xmlDoc);
		}

		static IEnumerable<TestCaseData> TransformFullyTransform_TestCases_CondAndApp()
		{
			yield return new TestCaseData(OnlyRefCusTariff)
			{
				TestName = nameof(OnlyRefCusTariff),
				ExpectedResult = TransformMode.None,
			};
			yield return new TestCaseData(RefCusRateWithoutApplicability)
			{
				TestName = nameof(RefCusRateWithoutApplicability),
				ExpectedResult = TransformMode.None,
			};
			yield return new TestCaseData(RefCusConditionWithApplicability)
			{
				TestName = nameof(RefCusConditionWithApplicability),
				ExpectedResult = TransformMode.Transform_CondAndApp,
			};
			yield return new TestCaseData(RefCusRateWithApplicability)
			{
				TestName = nameof(RefCusRateWithApplicability),
				ExpectedResult = TransformMode.Transform_RateAndApp,
			};
			yield return new TestCaseData(RefCusRateWithApplicabilityAndRateUOM)
			{
				TestName = nameof(RefCusRateWithApplicabilityAndRateUOM),
				ExpectedResult = TransformMode.Transform_RateAndApp,
			};
			yield return new TestCaseData(RefCusRateWithApplicabilityAndExcludedTradeGroup)
			{
				TestName = nameof(RefCusRateWithApplicabilityAndExcludedTradeGroup),
				ExpectedResult = TransformMode.Transform_RateAndApp,
			};
			yield return new TestCaseData(NoKeyPresent)
			{
				TestName = nameof(NoKeyPresent),
				ExpectedResult = TransformMode.None,
			};
			yield return new TestCaseData(KeyProperties_OnlySimpleProperties)
			{
				TestName = nameof(KeyProperties_OnlySimpleProperties),
				ExpectedResult = TransformMode.None,
			};
			yield return new TestCaseData(RefCusExcludedTradeGroup_WithCondAndApp)
			{
				TestName = nameof(RefCusExcludedTradeGroup_WithCondAndApp),
				ExpectedResult = TransformMode.Transform_CondAndApp,
			};
			yield return new TestCaseData(KeyProperties_RelatedEntity)
			{
				TestName = nameof(KeyProperties_RelatedEntity),
				ExpectedResult = TransformMode.Transform_RateAndApp,
			};
			yield return new TestCaseData(RefCusApplicabilityBothInRateAndCondition)
			{
				TestName = nameof(RefCusApplicabilityBothInRateAndCondition),
				ExpectedResult = TransformMode.Transform_RateAndApp | TransformMode.Transform_CondAndApp,
			};
			yield return new TestCaseData(RefCusConditionWithoutApplicability)
			{
				TestName = nameof(RefCusConditionWithoutApplicability),
				ExpectedResult = TransformMode.None,
			};
			yield return new TestCaseData(RefCusCondtionWithApplicabilityAndValue)
			{
				TestName = nameof(RefCusCondtionWithApplicabilityAndValue),
				ExpectedResult = TransformMode.Transform_CondAndApp,
			};
			yield return new TestCaseData(RefCusConditionWithApplicabilityAndValueAndLanguage)
			{
				TestName = nameof(RefCusConditionWithApplicabilityAndValueAndLanguage),
				ExpectedResult = TransformMode.Transform_CondAndApp,
			};
			yield return new TestCaseData(NoKeyPersent_Condition_Applicability)
			{
				TestName = nameof(NoKeyPersent_Condition_Applicability),
				ExpectedResult = TransformMode.None,
			};
			yield return new TestCaseData(KeyProperties_OnlySimpleProperties_Condition_Applicability)
			{
				TestName = nameof(KeyProperties_OnlySimpleProperties_Condition_Applicability),
				ExpectedResult = TransformMode.None,
			};
			yield return new TestCaseData(KeyProperties_RelatedEntity_Condition_Applicability)
			{
				TestName = nameof(KeyProperties_RelatedEntity_Condition_Applicability),
				ExpectedResult = TransformMode.Transform_CondAndApp,
			};
			yield return new TestCaseData(DeletionXml)
			{
				TestName = nameof(DeletionXml),
				ExpectedResult = TransformMode.None,
			};
			yield return new TestCaseData(RateWithoutApp_AdditionalCodeWithApp)
			{
				TestName = nameof(RateWithoutApp_AdditionalCodeWithApp),
				ExpectedResult = TransformMode.None,
			};
			yield return new TestCaseData(RateWithApp_AdditionalCodeWithApp)
			{
				TestName = nameof(RateWithApp_AdditionalCodeWithApp),
				ExpectedResult = TransformMode.Transform_RateAndApp | TransformMode.Transform_KeepApp,
			};
			yield return new TestCaseData(RateWithApp_CondWithoutApp_AdditionalCodeWithApp)
			{
				TestName = nameof(RateWithApp_CondWithoutApp_AdditionalCodeWithApp),
				ExpectedResult = TransformMode.Transform_RateAndApp | TransformMode.Transform_KeepApp,
			};
			yield return new TestCaseData(RateWithApp_CondWithApp_AdditionalCodeWithApp)
			{
				TestName = nameof(RateWithApp_CondWithApp_AdditionalCodeWithApp),
				ExpectedResult = TransformMode.Transform_RateAndApp | TransformMode.Transform_CondAndApp | TransformMode.Transform_KeepApp,
			};
			yield return new TestCaseData(RateWithoutApp_CondWithoutApp_AdditionalCodeWithApp)
			{
				TestName = nameof(RateWithoutApp_CondWithoutApp_AdditionalCodeWithApp),
				ExpectedResult = TransformMode.None,
			};
			yield return new TestCaseData(RateWithoutApp_CondWithApp_AdditionalCodeWithApp)
			{
				TestName = nameof(RateWithoutApp_CondWithApp_AdditionalCodeWithApp),
				ExpectedResult = TransformMode.Transform_CondAndApp | TransformMode.Transform_KeepApp,
			};
		}

		const string OnlyRefCusTariff = @"
<Schema>
  <EntityType Name=""RefCusTariff"" Data=""true"">
  </EntityType>
</Schema>";

		const string RefCusRateWithoutApplicability = @"
<Schema>
  <EntityType Name=""RefCusRate"" Data=""true"">
  </EntityType>
  <EntityType Name=""RefCusApplicability"" Data=""true"">
  </EntityType>
</Schema>";

		const string RefCusConditionWithApplicability = @"
<Schema>
  <EntityType Name=""RefCusCondition"" Data=""true"">
	<Key>
	  <PropertyRef Name=""RefCusApplicability"" />
	</Key>
	<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
  </EntityType>
  <EntityType Name=""RefCusApplicability"" Data=""true"">
  </EntityType>
</Schema>";

		const string RefCusRateWithApplicability = @"
<Schema>
  <EntityType Name=""RefCusRate"" Data=""true"">
	<Key>
	  <PropertyRef Name=""RefCusApplicability"" />
	</Key>
	<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
  </EntityType>
  <EntityType Name=""RefCusApplicability"" Data=""true"">
  </EntityType>
</Schema>";

		const string RefCusRateWithApplicabilityAndRateUOM = @"
<Schema>
  <EntityType Name=""RefCusRate"" Data=""true"">
	<Key>
	  <PropertyRef Name=""RefCusApplicability"" />
	  <PropertyRef Name=""RefCusRateUOM"" />
	</Key>
	<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
	<Property Name=""RefCusRateUOM"" Type=""RefCusRateUOM"" />
  </EntityType>
  <EntityType Name=""RefCusApplicability"" Data=""true"">
  </EntityType>
  <EntityType Name=""RefCusRateUOM"" Data=""true"">
  </EntityType>
</Schema>";

		const string RefCusRateWithApplicabilityAndExcludedTradeGroup = @"
<Schema>
  <EntityType Name=""RefCusRate"" Data=""true"">
	<Key>
	  <PropertyRef Name=""RefCusApplicability"" />
	</Key>
	<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
  </EntityType>
  <EntityType Name=""RefCusApplicability"" Data=""true"">
	<Key>
	  <PropertyRef Name=""RefCusExcludedTradeGroup"" />
	</Key>
	<Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
  </EntityType>
  <EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
  </EntityType>
</Schema>";

		const string NoKeyPresent = @"
<Schema>
  <EntityType Name=""RefCusRate"" Data=""true"">
  </EntityType>
  <EntityType Name=""RefCusApplicability"" Data=""true"">
  </EntityType>
</Schema>";

		const string KeyProperties_OnlySimpleProperties = @"
<Schema>
  <EntityType Name=""RefCusRate"" Data=""true"">
	<Key>
	  <PropertyRef Name=""ZZ2_RateFormula"" />
	</Key>
	<Property Name=""ZZ2_RateFormula"" Type=""varchar"" />
  </EntityType>
  <EntityType Name=""RefCusApplicability"" Data=""true"">
  </EntityType>
</Schema>";

		const string RefCusExcludedTradeGroup_WithCondAndApp = @"
<Schema>
  <EntityType Name=""RefCusCondition"" Data=""true"">
	<Key>
	  <PropertyRef Name=""RefCusApplicability"" />
	</Key>
	<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
  </EntityType>
  <EntityType Name=""RefCusApplicability"" Data=""true"">
	<Key>
	  <PropertyRef Name=""RefCusExcludedTradeGroup"" />
	</Key>
	<Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
  </EntityType>
  <EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
  </EntityType>
</Schema>";

		const string KeyProperties_RelatedEntity = @"
<Schema>
  <EntityType Name=""RefCusRate"" Data=""true"">
	<Key>
	  <PropertyRef Name=""RefCusApplicability"" />
	</Key>
	<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
  </EntityType>
  <EntityType Name=""RefCusApplicability"" Data=""true"">
  </EntityType>
</Schema>";

		const string RefCusApplicabilityBothInRateAndCondition = @"<Schema>
  <EntityType Name=""RefCusRate"" Data=""true"">
	<Key>
	  <PropertyRef Name=""RefCusApplicability"" />
	</Key>
	<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
  </EntityType>
  <EntityType Name=""RefCusApplicability"" Data=""true"">
  </EntityType>
  <EntityType Name=""RefCusCondition"" Data=""true"">
	<Key>
	  <PropertyRef Name=""RefCusApplicability"" />
	</Key>
	<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
  </EntityType>
</Schema>";

		const string RefCusConditionWithoutApplicability = @"<Schema>
  <EntityType Name=""RefCusCondition"" Data=""true"">
  </EntityType>
  <EntityType Name=""RefCusApplicability"" Data=""true"">
  </EntityType>
</Schema>";

		const string RefCusCondtionWithApplicabilityAndValue = @"<Schema>
	<EntityType Name=""RefCusCondition"" Data=""true"">
	  <Key>
		<PropertyRef Name=""RefCusApplicability"" />
	  </Key>
	  <Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
	</EntityType>
	<EntityType Name=""RefCusApplicability"" Data=""true"">
	</EntityType>
	<EntityType Name=""RefCusConditionValue"" Data=""true"">
	</EntityType>
  </Schema>";

		const string RefCusConditionWithApplicabilityAndValueAndLanguage = @"<Schema>
	<EntityType Name=""RefCusCondition"" Data=""true"">
	  <Key>
		<PropertyRef Name=""RefCusApplicability"" />
	  </Key>
	  <Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
	</EntityType>
	<EntityType Name=""RefCusApplicability"" Data=""true"">
	</EntityType>
	<EntityType Name=""RefCusConditionValue"" Data=""true"">
	</EntityType>
	<EntityType Name=""RefCusConditionLanguage"" Data=""true"">
	</EntityType>
  </Schema>";

		const string NoKeyPersent_Condition_Applicability = @"<Schema>
  <EntityType Name=""RefCusCondition"" Data=""true"">
  </EntityType>
  <EntityType Name=""RefCusApplicability"" Data=""true"">
  </EntityType>
</Schema>";

		const string KeyProperties_OnlySimpleProperties_Condition_Applicability = @"<Schema>
	<EntityType Name=""RefCusCondition"" Data=""true"">
		<Key>
			<PropertyRef Name=""ZX1_Comment"" />
		  </Key>
		  <Property Name=""ZX1_Comment"" Type=""varchar"" />
	</EntityType>
	<EntityType Name=""RefCusApplicability"" Data=""true"">
	</EntityType>
  </Schema>";

		const string KeyProperties_RelatedEntity_Condition_Applicability = @"<Schema>
	<EntityType Name=""RefCusCondition"" Data=""true"">
	 <Key>
		<PropertyRef Name=""RefCusApplicability"" />
	</Key>
	<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
	</EntityType>
	<EntityType Name=""RefCusApplicability"" Data=""true"">
	</EntityType>
  </Schema>";

		const string DeletionXml = @"<UniversalReferenceData>
  <DataSource>TW Tariff</DataSource>
  <PublicationTime>2024-09-30T15:21:00</PublicationTime>
  <UpdateType>Full</UpdateType>
  <!--Internal Tags Start: Just for Schedulers in RefData Team, Not useful to Business Data-->
  <AppName>CargoWise.RefDbRepo.TaiwanReferenceData.dll</AppName>
  <AppProgramArgs>-I=WEB_TARIFF -F=..\..\UxmlFiles\TWTariff.xml -f</AppProgramArgs>
  <!--Internal Tags End-->
  <Schema>
  </Schema>
  </UniversalReferenceData>";

		const string RateWithoutApp_AdditionalCodeWithApp = @"
<Schema>
  <EntityType Name=""RefCusRate"" Data=""true"">
  </EntityType>
  <EntityType Name=""RefCusTariffAdditionalCode"" Data=""true"">
	<Key>
	  <PropertyRef Name=""RefCusApplicability"" />
	</Key>
	<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
  </EntityType>
  <EntityType Name=""RefCusApplicability"" Data=""true"">
  </EntityType>
</Schema>";

		const string RateWithApp_AdditionalCodeWithApp = @"
<Schema>
  <EntityType Name=""RefCusRate"" Data=""true"">
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
  <EntityType Name=""RefCusApplicability"" Data=""true"">
  </EntityType>
</Schema>";

		const string RateWithApp_CondWithoutApp_AdditionalCodeWithApp = @"
<Schema>
  <EntityType Name=""RefCusRate"" Data=""true"">
	<Key>
	  <PropertyRef Name=""RefCusApplicability"" />
	</Key>
	<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
  </EntityType>
  <EntityType Name=""RefCusCondition"" Data=""true"">
  </EntityType>
  <EntityType Name=""RefCusTariffAdditionalCode"" Data=""true"">
	<Key>
	  <PropertyRef Name=""RefCusApplicability"" />
	</Key>
	<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
  </EntityType>
  <EntityType Name=""RefCusApplicability"" Data=""true"">
  </EntityType>
</Schema>";

		const string RateWithApp_CondWithApp_AdditionalCodeWithApp = @"
<Schema>
  <EntityType Name=""RefCusRate"" Data=""true"">
	<Key>
	  <PropertyRef Name=""RefCusApplicability"" />
	</Key>
	<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
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
  <EntityType Name=""RefCusApplicability"" Data=""true"">
  </EntityType>
</Schema>";

		const string RateWithoutApp_CondWithoutApp_AdditionalCodeWithApp = @"
<Schema>
  <EntityType Name=""RefCusRate"" Data=""true"">
  </EntityType>
  <EntityType Name=""RefCusCondition"" Data=""true"">
  </EntityType>
  <EntityType Name=""RefCusTariffAdditionalCode"" Data=""true"">
	<Key>
	  <PropertyRef Name=""RefCusApplicability"" />
	</Key>
	<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
  </EntityType>
  <EntityType Name=""RefCusApplicability"" Data=""true"">
  </EntityType>
</Schema>";

		const string RateWithoutApp_CondWithApp_AdditionalCodeWithApp = @"
<Schema>
  <EntityType Name=""RefCusRate"" Data=""true"">
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
  <EntityType Name=""RefCusApplicability"" Data=""true"">
  </EntityType>
</Schema>";
	}
}
