using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ISF.GUI.Testing
{
	sealed class ManufacturersUserControlTest : TestCaseWithFactory
	{
		public void TestOverrideCheckBox()
		{
			using (var form = new ZForm())
			using (var control = new ManufacturersUserControl())
			{
				var address = Factory.NewWithValidTestData<ISFDocAddress>();
				address.E2_AddressOverride = false;
				Assert("Test row should not be detached", ((INeedRow)address).Row.RowState != DataRowState.Detached);
				control.SetDataBinding(address, "");
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				var addressOverrideCheckBox = control.Controls.Find("OverrideAddressCheckbox", true)[0] as ZCheckBox;
				addressOverrideCheckBox.Checked = true;
				Assert(address.E2_AddressOverride);
				addressOverrideCheckBox.Checked = false;
				Assert(!address.E2_AddressOverride);
			}
		}
	}
}
