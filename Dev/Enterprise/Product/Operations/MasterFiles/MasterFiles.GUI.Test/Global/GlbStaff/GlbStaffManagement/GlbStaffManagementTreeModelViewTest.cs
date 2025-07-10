using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbStaffManagementTreeModelView))]
	sealed class GlbStaffManagementTreeModelViewTest : NonPersistentBusinessObjectTestCase
	{
		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			var staff = Factory.New<GlbStaff>();
			var model = new GlbStaffManagementTreeModel(staff);
			return new GlbStaffManagementTreeModelView(model);
		}
	}
}
