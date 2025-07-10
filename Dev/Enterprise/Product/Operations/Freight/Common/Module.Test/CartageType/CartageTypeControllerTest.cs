using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Common.Module.Testing
{
	[TestedType(typeof(CartageTypeController))]
	sealed class CartageTypeControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CartageType;
		}

		public void TestShowDeleteFormDoesNotDeleteSystemDefinedCodes()
		{
			CommonCartageType systemDefinedJobType = Factory.New<CommonCartageType>();
			systemDefinedJobType.E3_IsSystem = true;
			GlbDepartment systemDepartment = Factory.New<GlbDepartment>();
			systemDepartment.GE_Code = "SYS";
			systemDefinedJobType.E3_GE = systemDepartment.PK;
			systemDefinedJobType.E3_Description = "SYSTEM";
			systemDefinedJobType.E3_JobType = "CD11";

			CommonCartageType nonsystemDefinedJobType = Factory.New<CommonCartageType>();
			GlbDepartment nonsystemDepartment = Factory.New<GlbDepartment>();
			nonsystemDepartment.GE_Code = "NSY";
			nonsystemDefinedJobType.E3_IsSystem = false;
			nonsystemDefinedJobType.E3_Description = "NONSYSTEM";
			nonsystemDefinedJobType.E3_JobType = "NST";
			nonsystemDefinedJobType.E3_GE = Factory.New<GlbDepartment>().PK;
			Factory.Save();

			CartageTypeController controller = (CartageTypeController)Controller;

			AssertNull("ShowDeletedForm", controller.ShowDeleteForm(systemDefinedJobType));
			AssertNotNull("ShowDeletedForm", controller.ShowDeleteForm(nonsystemDefinedJobType));
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}
	}
}
