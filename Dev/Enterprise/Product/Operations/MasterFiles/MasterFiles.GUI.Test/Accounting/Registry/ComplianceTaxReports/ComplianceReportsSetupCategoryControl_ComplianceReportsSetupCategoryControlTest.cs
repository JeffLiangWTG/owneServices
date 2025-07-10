using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ComplianceReportsSetupCategoryControl))]
	sealed class ComplianceReportsSetupCategoryControl_ComplianceReportsSetupCategoryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			ComplianceReportTypeCollection collection = new ComplianceReportTypeCollection("Test01");
			return collection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ComplianceReportsSetupCategoryControl)control).ReportCategoryGrid.ReadOnly && ((ComplianceReportsSetupCategoryControl)control).ReportTypeGrid.ReadOnly;
		}
	}
}
