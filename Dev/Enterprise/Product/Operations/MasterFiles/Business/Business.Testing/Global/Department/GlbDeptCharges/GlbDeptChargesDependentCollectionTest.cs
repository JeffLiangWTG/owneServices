using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbDeptChargesDependentCollection))]
	sealed class GlbDeptChargesDependentCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbDeptChargesDependentCollection>
	{
		#region Implementation

		protected override GlbDeptChargesDependentCollection GetCollectionToTest()
		{
			GlbDepartment department = Factory.New<GlbDepartment>();
			return department.DeptCharges;
		}

		#endregion
	}
}
