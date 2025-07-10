using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceSubTypeAttributionRuleConfigurationCollection))]
	sealed class ComplianceSubTypeAttributionRuleConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ComplianceSubTypeAttributionRuleConfigurationCollection>
	{
		#region Implementation

		protected override ComplianceSubTypeAttributionRuleConfigurationCollection GetCollectionToTest()
		{
			return new ComplianceSubTypeAttributionRuleConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComplianceSubTypeAttributionRuleConfiguration();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
		#endregion
	}
}
