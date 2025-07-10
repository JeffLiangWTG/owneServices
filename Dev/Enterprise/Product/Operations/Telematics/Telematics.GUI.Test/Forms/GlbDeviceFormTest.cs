using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.GUI.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Telematics.GUI.Test
{
	public class GlbDeviceFormTest : TestCaseWithFactory
	{
		public void TestFormCaption()
		{
			device.V3_HumanReadableIdentifier = "ABC001";
			AssertEquals("ABC001", form.FormCaption);
		}

		public void TestClearButtonClearsStaffParentID()
		{
			device.AssignedParentStaffID = Guid.NewGuid();
			ClickButtonNamed(ClearAssignmentButtonName);
			AssertEquals(ZGuid.Empty, device.AssignedParentStaffID);
		}

		public void TestClearButtonClearsEquipmentParentID()
		{
			device.AssignedParentEquipmentID = Guid.NewGuid();
			ClickButtonNamed(ClearAssignmentButtonName);
			AssertEquals(ZGuid.Empty, device.AssignedParentEquipmentID);
		}

		public void TestDoesNotAllowNew()
		{
			AssertEquals(false, ((IPostingButtonsProvider)form).AllowNew);
		}

		void ClickButtonNamed(string name)
		{
			var control = form.Controls.Find(name, searchAllChildren: true).SingleOrDefault() as ZButton;
			AssertNotNull(string.Format("ZButton named '{0}' should have been found.", name), control);

			control.PerformClick();
		}

		const string ClearAssignmentButtonName = "clearAssignmentButton";
		GlbDeviceForm form;
		GlbDevice device;

		protected override void SetUp()
		{
			base.SetUp();

			device = Factory.New<GlbDevice>();
			form = new GlbDeviceForm(device);
			form.Show();
		}

		protected override void TearDown()
		{
			form.Hide();
			form.Dispose();
			form = null;

			base.TearDown();
		}
	}
}
