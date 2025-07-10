using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefOrgPartCategoryController))]
	sealed class RefOrgPartCategoryControllerTest : ZControllerBasherTest
	{
		#region TestRefOrgPartCategoryControllerProperties

		public void TestRefOrgPartCategoryControllerProperties()
		{
			var controller = new RefOrgPartCategoryController();
			AssertEquals(ControllerIDs.RefOrgPartCategory, controller.ID);
			AssertEquals(typeof(OrgPartCategory), controller.TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.RefOrgPartCategory;

		#endregion
	}
}
