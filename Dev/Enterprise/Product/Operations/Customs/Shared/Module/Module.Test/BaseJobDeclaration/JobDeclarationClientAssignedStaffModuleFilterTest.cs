using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(JobDeclarationClientAssignedStaffModuleFilter))]
	sealed class JobDeclarationClientAssignedStaffModuleFilterTest : OrgClientAssignedStaffModuleFilterTest
	{
		protected override string GetExpectedClientTypeList()
		{
			return "IMP - Importer\r\n" + "SUP - Supplier\r\n" + "LOC - Local Client";
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobDeclarationClientAssignedStaffModuleFilter("Test");
		}
	}
}
