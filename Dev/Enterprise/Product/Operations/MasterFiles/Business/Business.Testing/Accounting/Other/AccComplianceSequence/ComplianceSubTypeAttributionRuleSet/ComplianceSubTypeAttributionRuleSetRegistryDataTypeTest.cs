using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceSubTypeAttributionRuleSetRegistryDataType))]
	sealed class ComplianceSubTypeAttributionRuleSetRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ComplianceSubTypeAttributionRuleSetRegistryDataType>
	{
		protected override ComplianceSubTypeAttributionRuleSetRegistryDataType GetNewDataType()
		{
			return new ComplianceSubTypeAttributionRuleSetRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ComplianceSubTypeAttributionRuleSetRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Turkey);

			var company = factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;

			var fallback = new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var ruleSet1 = new ComplianceSubTypeAttributionRuleSet(fallback, factory, TurkeyComplianceInfo.RuleSetCodes.eFactura);
			var ruleSet2 = new ComplianceSubTypeAttributionRuleSet(fallback, factory, TurkeyComplianceInfo.RuleSetCodes.eArchive);

			var xml1 = @"<ComplianceSubTypeAttributionRuleSet><RuleSetCode>1</RuleSetCode></ComplianceSubTypeAttributionRuleSet>";
			var xml2 = @"<ComplianceSubTypeAttributionRuleSet><RuleSetCode>2</RuleSetCode></ComplianceSubTypeAttributionRuleSet>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(ruleSet1, xml1),
				new ValidSampleAndBinaryValueInDB(ruleSet2, xml2)
			};
		}
	}
}
