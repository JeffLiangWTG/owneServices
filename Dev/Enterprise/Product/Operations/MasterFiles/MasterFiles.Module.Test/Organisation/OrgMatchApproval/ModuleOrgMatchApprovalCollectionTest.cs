using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgMatchApprovalModule.ModuleOrgMatchApprovalCollection))]
	sealed class ModuleOrgMatchApprovalCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgMatchApprovalModule.ModuleOrgMatchApprovalCollection(Factory);
		}
	}
}
