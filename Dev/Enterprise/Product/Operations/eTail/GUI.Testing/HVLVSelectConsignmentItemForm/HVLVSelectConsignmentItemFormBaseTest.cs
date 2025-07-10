using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.eTail.GUI.Testing
{
	public abstract class HVLVSelectConsignmentItemFormBaseTest : ZFormBasherTest
	{
		public void TestOkButtonCaption()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			using (var form = GetSelectItemForm(consignment))
			{
				form.Show();

				var okButton = form.Controls.Find("ButtonOk", true)[0] as ZButton;
				AssertEquals(ExpectedOkButtonText, okButton.Text);
			}
		}

		public void TestCaptionResourceString()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			using (var form = GetSelectItemForm(consignment))
			{
				form.Show();

				AssertEquals(ExpectedCaptionResourceString, form.CaptionResourceString.Caption);
			}
		}

		public void TestSelectAll()
		{
			var consignment = Factory.New<HVLVConsignment>();
			CreateValidItemForTreeView(consignment);
			CreateValidItemForTreeView(consignment);
			CreateValidItemForTreeView(consignment);

			using (var form = GetSelectItemForm(consignment))
			{
				form.Show();
				var selectAllButton = form.Controls.Find("ButtonSelectAll", true)[0] as ZButton;
				selectAllButton.PerformClick();

				var consignmentTreeView = form.Controls.Find("ConsignmentItemTreeView", true)[0] as ZTreeView;

				CombineAssertions("All nodes should be checked", () =>
				{
					AssertEquals(true, consignmentTreeView.Nodes[0].Checked);
					AssertEquals(true, consignmentTreeView.Nodes[0].Nodes[0].Checked);
					AssertEquals(true, consignmentTreeView.Nodes[0].Nodes[1].Checked);
					AssertEquals(true, consignmentTreeView.Nodes[0].Nodes[2].Checked);
				});
			}
		}

		public void TestDeselectAll()
		{
			var consignment = Factory.New<HVLVConsignment>();
			CreateValidItemForTreeView(consignment);
			CreateValidItemForTreeView(consignment);
			CreateValidItemForTreeView(consignment);

			using (var form = GetSelectItemForm(consignment))
			{
				form.Show();

				var consignmentTreeView = form.Controls.Find("ConsignmentItemTreeView", true)[0] as ZTreeView;
				consignmentTreeView.Nodes[0].Nodes[1].Checked = true;
				consignmentTreeView.Nodes[0].Nodes[2].Checked = true;

				var deselectAllButton = form.Controls.Find("ButtonDeselectAll", true)[0] as ZButton;
				deselectAllButton.PerformClick();

				CombineAssertions("All nodes should not be checked", () =>
				{
					AssertEquals(false, consignmentTreeView.Nodes[0].Checked);
					AssertEquals(false, consignmentTreeView.Nodes[0].Nodes[0].Checked);
					AssertEquals(false, consignmentTreeView.Nodes[0].Nodes[1].Checked);
					AssertEquals(false, consignmentTreeView.Nodes[0].Nodes[2].Checked);
				});
			}
		}

		public void TestOkButton_WhenThreeItemsAreSelected()
		{
			var consignment = Factory.New<HVLVConsignment>();
			CreateValidItemForTreeView(consignment);
			CreateValidItemForTreeView(consignment);
			CreateValidItemForTreeView(consignment);

			using (var form = GetSelectItemForm(consignment))
			{
				form.Show();
				var consignmentTreeView = form.Controls.Find("ConsignmentItemTreeView", true)[0] as ZTreeView;
				consignmentTreeView.Nodes[0].Nodes[0].Checked = true;
				consignmentTreeView.Nodes[0].Nodes[1].Checked = true;
				consignmentTreeView.Nodes[0].Nodes[2].Checked = true;

				var bookButton = form.Controls.Find("ButtonOk", true)[0] as ZButton;
				bookButton.PerformClick();

				AssertEquals("Expected this prompt", "You have selected 3 HVLV Item(s). Do you wish to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOkButton_WhenNoItemsAreSelected()
		{
			var consignment = Factory.New<HVLVConsignment>();
			CreateValidItemForTreeView(consignment);
			CreateValidItemForTreeView(consignment);
			CreateValidItemForTreeView(consignment);

			using (var form = GetSelectItemForm(consignment))
			{
				form.Show();

				var bookButton = form.Controls.Find("ButtonOk", true)[0] as ZButton;
				bookButton.PerformClick();

				AssertEquals("Expected this merror message", "No HVLV Items were selected.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSelectedItems()
		{
			var consignment = Factory.New<HVLVConsignment>();

			CreateValidItemForTreeView(consignment);
			var item2 = CreateValidItemForTreeView(consignment);
			var item3 = CreateValidItemForTreeView(consignment);

			using (var form = GetSelectItemForm(consignment))
			{
				form.Show();

				var consignmentTreeView = form.Controls.Find("ConsignmentItemTreeView", true)[0] as ZTreeView;
				consignmentTreeView.Nodes[0].Nodes[1].Checked = true;
				consignmentTreeView.Nodes[0].Nodes[2].Checked = true;

				AssertContainsExactElementsInAnyOrder(new[] { item2, item3 }, form.SelectedItems);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var consignment = Factory.New<HVLVConsignment>();
			return GetSelectItemForm(consignment);
		}

		protected abstract HVLVSelectConsignmentItemForm GetSelectItemForm(HVLVConsignment consignment);

		protected virtual HVLVItem CreateValidItemForTreeView(HVLVConsignment parentConsignment) => parentConsignment.Items.AddNew();

		protected virtual ZString ExpectedOkButtonText => "Ok";

		protected virtual ZString ExpectedCaptionResourceString => "Select HVLV Items";
		#endregion
	}
}
