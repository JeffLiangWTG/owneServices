using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceReportsSetupRegistryItem))]
	sealed class ComplianceReportsSetupCategoryRegistryItemTest : StronglyTypedRegistryItemTestCase<ComplianceReportTypeCollection>
	{
		protected override StronglyTypedRegistryItem<ComplianceReportTypeCollection, ComplianceReportTypeCollection> GetNewRegistryItem()
		{
			return new ComplianceReportsSetupRegistryItem("", null, null, null, new ComplianceReportTypeCollection(), RegistryOptions.IsOnlyForSupport);
		}
	}
}
