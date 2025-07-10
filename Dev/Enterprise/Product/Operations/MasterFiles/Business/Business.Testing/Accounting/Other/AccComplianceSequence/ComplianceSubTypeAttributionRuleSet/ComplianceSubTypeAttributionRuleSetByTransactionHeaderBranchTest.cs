using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch))]
	sealed class ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranchTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
