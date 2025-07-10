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
	[TestedType(typeof(ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranchRegistryDataType))]
	sealed class ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranchRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranchRegistryDataType>
	{
		protected override ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranchRegistryDataType GetNewDataType()
		{
			return new ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranchRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ComplianceSubTypeAttributionRuleSetRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);

			var company = factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;

			var fallback = new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var ruleSet1 = new ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch(fallback, factory, ChinaComplianceInfo.RuleSetCodes.TXATXB);
			var ruleSet2 = new ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch(fallback, factory, ChinaComplianceInfo.RuleSetCodes.TXAETB);

			var xml1 = @"<ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch><RuleSetCode>1</RuleSetCode></ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch>";
			var xml2 = @"<ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch><RuleSetCode>2</RuleSetCode></ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(ruleSet1, xml1),
				new ValidSampleAndBinaryValueInDB(ruleSet2, xml2)
			};
		}
	}
}
