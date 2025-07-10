using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceReportConfigurationSettingCollection))]
	sealed class ComplianceReportConfigurationSettingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ComplianceReportConfigurationSettingCollection>
	{
		#region Implementation

		protected override ComplianceReportConfigurationSettingCollection GetCollectionToTest()
		{
			return new ComplianceReportConfigurationSettingCollection(Factory, "AU");
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComplianceReportConfigurationSetting(Collection.CurrentFallbackLevel, Factory);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new ComplianceReportConfigurationSettingCollection Collection => base.Collection;

		#endregion
	}
}
