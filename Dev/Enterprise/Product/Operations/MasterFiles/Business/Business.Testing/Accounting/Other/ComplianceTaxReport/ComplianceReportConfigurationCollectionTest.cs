using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceReportConfigurationCollection))]
	sealed class ComplianceReportConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ComplianceReportConfigurationCollection>
	{
		#region Implementation

		protected override ComplianceReportConfigurationCollection GetCollectionToTest()
		{
			return new ComplianceReportConfigurationCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComplianceReportConfiguration(Factory);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
