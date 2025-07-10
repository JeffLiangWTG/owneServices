using System.Windows.Forms;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.GUI.Testing
{
	[TestedType(typeof(BulkConvertToStandAloneDeclarationForm))]
	public class BulkConvertToFormalDeclarationFormTest : ZFormBasherTest
	{
		public void TestGridRemoveActionIsNoRemovePossible()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();
			item.ULI_GoodsValue = 801;
			item.ULI_RX_NKCurrency = "USD";
			using (var form = new BulkConvertToStandAloneDeclarationForm(clearance))
			{
				form.Show();
				Application.DoEvents();
				var grid = form.Controls.Find("gridHouseBills", true)[0] as ZGrid;

				AssertEquals("Should not be able to remove/delete items on this grid", ZArchitecture.RemoveAction.NoRemovePossible, grid.RemoveAction);

				grid.ContextMenu.ShowPopupMenu();
				Assert("Should not be able to remove/delete items on this grid", !grid.DeleteMenuItem.Visible);
			}
		}

		public void TestSelectAllCheckbox()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();
			item.ULI_GoodsValue = 801;
			item.ULI_RX_NKCurrency = "USD";
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			var item2 = consignment2.CusUSLVItems.AddNew();
			item2.ULI_GoodsValue = 802;
			item2.ULI_RX_NKCurrency = "USD";
			Factory.Save();
			using (var form = new BulkConvertToStandAloneDeclarationForm(clearance))
			{
				form.Show();
				Application.DoEvents();
				var selectAllBox = form.Controls.Find("checkBoxSelectAll", true)[0] as ZCheckBox;

				Assert(!selectAllBox.Checked);
				consignment.ShouldConvertToStandaloneDeclaration = false;
				AssertEquals(CheckState.Unchecked, selectAllBox.CheckState);
				consignment2.ShouldConvertToStandaloneDeclaration = false;
				Assert(!selectAllBox.Checked);

				selectAllBox.Checked = true;
				Assert(consignment.ShouldConvertToStandaloneDeclaration);
				Assert(consignment2.ShouldConvertToStandaloneDeclaration);

				selectAllBox.Checked = false;
				Assert(!consignment.ShouldConvertToStandaloneDeclaration);
				Assert(!consignment2.ShouldConvertToStandaloneDeclaration);
			}
		}

		public void TestSendButtonReadonly()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();
			item.ULI_GoodsValue = 801;
			item.ULI_RX_NKCurrency = "USD";
			Factory.Save();
			using (var form = new BulkConvertToStandAloneDeclarationForm(clearance))
			{
				form.Show();
				Application.DoEvents();
				var buttonSend = form.AcceptButton as ZButton;
				Assert(!buttonSend.Enabled);

				var selectAllBox = form.Controls.Find("checkBoxSelectAll", true)[0] as ZCheckBox;
				selectAllBox.Checked = true;
				Assert(buttonSend.Enabled);

				Assert("pre-condition", buttonSend.Enabled);
				consignment.ShouldConvertToStandaloneDeclaration = false;
				Assert(!buttonSend.Enabled);
			}
		}

		public void TestSendButton()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var item = clearance.CusUSLVConsignments.AddNew().CusUSLVItems.AddNew();
			item.ULI_GoodsValue = 801;
			item.ULI_RX_NKCurrency = "USD";
			Factory.Save();
			using (var form = new BulkConvertToStandAloneDeclarationForm(clearance))
			{
				form.Show();
				Application.DoEvents();

				var selectAllBox = form.Controls.Find("checkBoxSelectAll", true)[0] as ZCheckBox;
				selectAllBox.Checked = true;

				form.AcceptButton.PerformClick();
				AssertEquals("1 Stand Alone Declarations queued for processing, check the individual consignment for status", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCancelButton()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			using (var form = new BulkConvertToStandAloneDeclarationForm(clearance))
			{
				form.Show();
				Application.DoEvents();
				Assert("pre-condition", form.Visible);
				AssertEquals("buttonCancel", ((ZButton)form.CancelButton).Name);
				form.CancelButton.PerformClick();
				Assert(!form.Visible);
			}
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			var result = base.ShouldIgnoreMissingBindingMember(control);
			result |= control.Name == "checkBoxSelectAll";
			return result;
		}

		protected override Form GetFormToBashCore() => new BulkConvertToStandAloneDeclarationForm(Factory.New<CusUSLVClearance>());
	}
}
