using System;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceSubTypeAttributionRuleSet))]
	sealed class ComplianceSubTypeAttributionRuleSetTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestRuleSetValidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Argentina))
			{
				var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				var ruleSet = new ComplianceSubTypeAttributionRuleSet(fallbackLevel, Factory, "7");

				AssertHasError(ruleSet.RuleSetCodeInfo, "Enter a valid selection.");

				ruleSet.RuleSetCode = ArgentinaComplianceInfo.RuleSetCodes.TipoABE;
				AssertNoErrors(ruleSet.RuleSetCodeInfo);

				ruleSet.RuleSetCode = string.Empty;
				AssertHasError(ruleSet.RuleSetCodeInfo, "Please enter a Rule Set Code.");

				ruleSet.RuleSetCode = ArgentinaComplianceInfo.RuleSetCodes.TipoABExcludedSupply;
				AssertNoErrors(ruleSet.RuleSetCodeInfo);

				ruleSet.RuleSetCode = ArgentinaComplianceInfo.RuleSetCodes.TipoMB;
				AssertNoErrors(ruleSet.RuleSetCodeInfo);

				ruleSet.RuleSetCode = ArgentinaComplianceInfo.RuleSetCodes.TipoMBExcludedSupply;
				AssertNoErrors(ruleSet.RuleSetCodeInfo);

				ruleSet.RuleSetCode = ArgentinaComplianceInfo.RuleSetCodes.TipoC;
				AssertNoErrors(ruleSet.RuleSetCodeInfo);

				ruleSet.RuleSetCode = ArgentinaComplianceInfo.RuleSetCodes.TipoCExcludedSupply;
				AssertNoErrors(ruleSet.RuleSetCodeInfo);
			}
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new ComplianceSubTypeAttributionRuleSet();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
