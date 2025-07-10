using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgManagementGroupingModel))]
	sealed class OrgManagementGroupingModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.New<OrgHeader>();
			return new OrgManagementGroupingModel(org);
		}
	}
}
