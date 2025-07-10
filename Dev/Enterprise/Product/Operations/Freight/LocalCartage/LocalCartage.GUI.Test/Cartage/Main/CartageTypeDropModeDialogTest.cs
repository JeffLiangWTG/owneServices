using System.Windows.Forms;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	[TestedType(typeof(CartageTypeDropModeDialog))]
	public class CartageTypeDropModeDialogTest : ZFormBasherTest
	{
		public void TestButtons_AreSetupCorrectly()
		{
			Cartage.JJ_DropMode = "WUP";
			using (var form = GetNewForm())
			{
				form.Show();
				AssertEquals(true, form.KeepExistingButton.Visible);
				AssertEquals("Keep Existing Drop Mode 'WUP'", form.KeepExistingButton.Text);
				AssertEquals(true, form.UseAddressDropModeButton.Visible);
				AssertEquals(true, form.AddressButtonPanel.Visible);
				AssertEquals(26, form.AddressButtonPanel.Height);
				AssertEquals("Address", form.UseAddressDropModeButton.Text);
				AssertEquals(true, form.UseJobTypeDropModeButton.Visible);
				AssertEquals(true, form.JobTypeButtonPanel.Visible);
				AssertEquals(26, form.JobTypeButtonPanel.Height);
				AssertEquals("JobType", form.UseJobTypeDropModeButton.Text);
				AssertEquals(201, form.Height);
			}

			using (var form = GetNewForm(null, "Address\r\nDrop", null))
			{
				form.Show();
				AssertEquals(true, form.KeepExistingButton.Visible);
				AssertEquals("Keep Existing Drop Mode 'WUP'", form.KeepExistingButton.Text);
				AssertEquals(true, form.UseAddressDropModeButton.Visible);
				AssertEquals(true, form.AddressButtonPanel.Visible);
				AssertEquals(42, form.AddressButtonPanel.Height);
				AssertEquals("Address\r\nDrop", form.UseAddressDropModeButton.Text);
				AssertEquals(false, form.UseJobTypeDropModeButton.Visible);
				AssertEquals(false, form.JobTypeButtonPanel.Visible);
				AssertEquals("", form.UseJobTypeDropModeButton.Text);
				AssertEquals(191, form.Height);
			}

			using (var form = GetNewForm(null, "Address\r\nDrop", "JobType\r\nDrop"))
			{
				form.Show();
				AssertEquals(true, form.KeepExistingButton.Visible);
				AssertEquals("Keep Existing Drop Mode 'WUP'", form.KeepExistingButton.Text);
				AssertEquals(true, form.UseAddressDropModeButton.Visible);
				AssertEquals(true, form.AddressButtonPanel.Visible);
				AssertEquals(42, form.AddressButtonPanel.Height);
				AssertEquals("Address\r\nDrop", form.UseAddressDropModeButton.Text);
				AssertEquals(true, form.UseJobTypeDropModeButton.Visible);
				AssertEquals(true, form.JobTypeButtonPanel.Visible);
				AssertEquals(42, form.JobTypeButtonPanel.Height);
				AssertEquals("JobType\r\nDrop", form.UseJobTypeDropModeButton.Text);
				AssertEquals(233, form.Height);
			}
		}

		public void TestButtons_ReturnCorrectResult()
		{
			using (var form = GetNewForm())
			{
				AssertEquals(DropMode.None, form.Result);
				form.Show();
				form.KeepExistingButton.Visible = true;
				form.KeepExistingButton.PerformClick();
				AssertEquals(DropMode.None, form.Result);
				form.UseAddressDropModeButton.Visible = true;
				form.UseAddressDropModeButton.PerformClick();
				AssertEquals(DropMode.Address, form.Result);
				form.UseJobTypeDropModeButton.Visible = true;
				form.UseJobTypeDropModeButton.PerformClick();
				AssertEquals(DropMode.CartageType, form.Result);
			}
		}

		public void TestFormHeading()
		{
			AssertEquals("Form.DialogDescription", "Populate Cartage Drop Mode", CartageTypeDropModeDialog.DialogDescription);
		}

		public void TestMessage()
		{
			using (var form = GetNewForm())
			{
				AssertEquals("I am a good description", form.MessageLabel.Text);
			}
		}

		CartageTypeDropModeDialog GetNewForm(string message, string addressButtonText, string jobTypeButtonText)
		{
			return new CartageTypeDropModeDialog(Cartage, message, addressButtonText, jobTypeButtonText);
		}

		CartageTypeDropModeDialog GetNewForm()
		{
			return GetNewForm("I am a good description", "Address", "JobType");
		}

		CommonCartage Cartage
		{
			get
			{
				return cartage ?? (cartage = Factory.New<CommonCartage>());
			}
		}

		CommonCartage cartage;

		protected override Form GetFormToBashCore()
		{
			return GetNewForm(null, null, null);
		}
	}
}
