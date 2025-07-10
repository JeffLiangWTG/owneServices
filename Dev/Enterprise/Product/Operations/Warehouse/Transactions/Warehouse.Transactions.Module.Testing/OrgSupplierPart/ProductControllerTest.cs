using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(ProductController))]
	class ProductControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsConfigProduct;
		}

		protected OrgSupplierPartController GetOrgSupplierPartController()
		{
			return new ProductControllerForTest();
		}

		public void TestPluginTabPageCaption()
		{
			AssertEquals("Warehouse", GetOrgSupplierPartController().PluginTabPageCaption.Caption);
		}

		public void TestSecurityCheckpoints()
		{
			var controller = (ProductControllerForTest)GetOrgSupplierPartController();

			Env.Security.CustomsSupplierPart.IsAllowed = false;
			Env.Security.CustomsSupplierPartModify.IsAllowed = false;

			AssertEquals("For View", Env.Security.WhsConfigProductView, controller.GetCheckPointForView());
			AssertEquals("For Edit", Env.Security.WhsConfigProductEdit, controller.GetCheckPointForEdit());
			AssertEquals("For New", Env.Security.WhsConfigProductNew, controller.GetCheckPointForNew());
			AssertEquals("For Delete", Env.Security.WhsConfigProductDelete, controller.GetCheckPointForDelete());

			Env.Security.CustomsSupplierPart.IsAllowed = true;
			Env.Security.CustomsSupplierPartModify.IsAllowed = true;

			AssertEquals("For View", Env.Security.CustomsSupplierPart, controller.GetCheckPointForView());
			AssertEquals("For Edit", Env.Security.CustomsSupplierPartModify, controller.GetCheckPointForEdit());
			AssertEquals("For New", Env.Security.CustomsSupplierPartModify, controller.GetCheckPointForNew());
			AssertEquals("For Delete", Env.Security.CustomsSupplierPartModify, controller.GetCheckPointForDelete());
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var part = base.GetBusinessObjectWithoutValidationErrors() as OrgSupplierPart;
			part.OP_Desc = "Desc";
			return part;
		}
	}

	class ProductControllerForTest : ProductController
	{
		public SecurityCheckpoint GetCheckPointForView() => CheckPointForView;
		public SecurityCheckpoint GetCheckPointForNew() => CheckPointForNew;
		public SecurityCheckpoint GetCheckPointForEdit() => CheckPointForEdit;
		public SecurityCheckpoint GetCheckPointForDelete() => CheckPointForDelete;
	}
}
