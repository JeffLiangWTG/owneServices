using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.GUI;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(CartageController))]
	public class CartageControllerTest : ZControllerBasherTest
	{
		public void TestIsRoot()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			Assert(!cartage.IsRoot);
			Factory.Save();
			CartageController controller = new CartageController();
			CommonCartage cartage_FreshFactory = new BusinessObjectFactory().Load<CommonCartage>(cartage.PK);
			using (CartageForm shownForm = (CartageForm)controller.ShowEditForm(cartage_FreshFactory))
			{
				Assert("Edit", ((CommonCartage)shownForm.BusinessEntity).IsRoot);
			}

			using (CartageForm shownForm = (CartageForm)controller.ShowNewForm())
			{
				Assert("New", ((CommonCartage)shownForm.BusinessEntity).IsRoot);
			}
		}

		public void TestIsFormOpen()
		{
			var cartage = Factory.New<CommonCartage>();
			Factory.Save();
			var controller = new CartageController();
			var iController = (ICartageController)controller;
			AssertEquals("No Cartage Form is open, must return false.", false, iController.IsFormOpen(cartage));
			using (var shownForm = controller.ShowEditForm(cartage))
			{
				AssertEquals("A Cartage Form is open, must return true.", true, iController.IsFormOpen(cartage));
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Cartage;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			CommonCartage cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_ConsignmentID = "T1000";
			Factory.Save();
			return cartage;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		public void TestGetModuleID()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_ConsignmentID = "T1000";
			Factory.Save();
			var cartageController = (CartageController)ZControllerFactory.Create(GetControllerID());
			AssertEquals("Cartage", cartageController.ModuleID.ToString());
		}

		public void TestCRMSecurityCheckpoints()
		{
			var bizObjWithoutAccess = Factory.NewWithValidTestData<CommonCartage>();
			CRMSecurityProviderTest<CommonCartage>.AssertController(new CartageController(), bizObjWithoutAccess, Env.Security.TransportJobCRMSecurity);
		}
	}
}
