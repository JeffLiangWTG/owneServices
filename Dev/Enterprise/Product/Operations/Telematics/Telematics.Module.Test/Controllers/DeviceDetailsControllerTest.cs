using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.GUI.Forms;
using Enterprise.Telematics.Module.Controllers;

namespace Enterprise.Telematics.Module.Test
{
	public class DeviceDetailsControllerTest : TestCaseWithFactory
	{
		public void TestSecurityCheckPointForNew()
		{
			var checkpoint = controller.GetCheckPointForNew(null);
			AssertEquals(Env.Security.None, checkpoint);
		}
		public void TestSecurityCheckPointForEdit()
		{
			var checkpoint = controller.GetCheckPointForEdit(null);
			AssertEquals(Env.Security.LDaaSDevicesEdit, checkpoint);
		}
		public void TestSecurityCheckPointForView()
		{
			var checkpoint = controller.GetCheckPointForView(null);
			AssertEquals(Env.Security.LDaaSDevicesView, checkpoint);
		}
		public void TestSecurityCheckPointForDelete()
		{
			var checkpoint = controller.GetCheckPointForDelete(null);
			AssertEquals(Env.Security.None, checkpoint);
		}

		public void TestGetFormReturnsGlbDeviceForm()
		{
			var device = Factory.NewWithValidTestData<GlbDevice>();
			Factory.Save();

			using (var form = controller.ShowEditForm(device))
			{
				AssertNotNull(form);
				AssertType<GlbDeviceForm>(form);
			}
		}

		public void TestTopLevelBusinessObjectIsGlbDevice()
		{
			AssertEquals(typeof(GlbDevice), controller.TypeOfTopLevelBusinessObject);
		}

		DeviceDetailsController controller;

		protected override void SetUp()
		{
			base.SetUp();

			controller = new DeviceDetailsController();
		}
	}
}
