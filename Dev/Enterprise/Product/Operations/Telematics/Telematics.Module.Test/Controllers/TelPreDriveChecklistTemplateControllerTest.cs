using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.GUI.Forms;
using Enterprise.Telematics.Module.Controllers;

namespace Enterprise.Telematics.Module.Test.Controllers
{
	class TelPreDriveChecklistTemplateControllerTest : TestCaseWithFactory
	{
		public void TestSecurityCheckPointForNew()
		{
			var checkpoint = controller.GetCheckPointForNew(null);
			AssertEquals(Env.Security.None, checkpoint);
		}
		public void TestSecurityCheckPointForEdit()
		{
			var checkpoint = controller.GetCheckPointForEdit(null);
			AssertEquals(Env.Security.TelematicsPreDriveChecklistHeaderTemplateEdit, checkpoint);
		}
		public void TestSecurityCheckPointForView()
		{
			var checkpoint = controller.GetCheckPointForView(null);
			AssertEquals(Env.Security.TelematicsPreDriveChecklistHeaderTemplateView, checkpoint);
		}
		public void TestSecurityCheckPointForDelete()
		{
			var checkpoint = controller.GetCheckPointForDelete(null);
			AssertEquals(Env.Security.None, checkpoint);
		}

		public void TestGetFormReturnsTelPreDriveChecklistForm()
		{
			var header = Factory.NewWithValidTestData<TelPreDriveChecklistTemplateHeader>();
			header.TTH_Type = TelPreDriveChecklistHeaderTypes.Codes.PDR;
			Factory.Save();

			using (var form = controller.ShowEditForm(header))
			{
				AssertNotNull(form);
				AssertType<TelPreDriveChecklistTemplateForm>(form);
			}
		}

		public void TestTopLevelBusinessObjectIsGlbDevice()
		{
			AssertEquals(typeof(TelPreDriveChecklistTemplateHeader), controller.TypeOfTopLevelBusinessObject);
		}

		TelPreDriveChecklistTemplateController controller;

		protected override void SetUp()
		{
			base.SetUp();

			controller = new TelPreDriveChecklistTemplateController();
		}
	}
}
