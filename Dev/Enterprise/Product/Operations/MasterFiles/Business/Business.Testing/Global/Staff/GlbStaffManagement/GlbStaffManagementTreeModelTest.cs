using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffManagementTreeModel))]
	sealed class GlbStaffManagementTreeModelTest : NonPersistentBusinessObjectTestCase
	{
		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var staff = Factory.New<GlbStaff>();
			return new GlbStaffManagementTreeModel(staff);
		}

		#endregion
	}
}
