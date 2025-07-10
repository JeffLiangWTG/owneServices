using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class InvoiceGroupingControlTest : TestCaseWithFactory
	{
		[TestDate(2016, 09, 20)]
		public void TestBindAgainWhenSelectedNodeGetsChanged()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			var topGroup = testDec.JobComInvoiceGroupHeaders[0];

			var subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();
			subGroup.JZ_InvoiceNumber = "SubGroup";

			using (var testForm = new BaseJobDeclarationForm(testDec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration = testDec;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceGroupingTabPage;

				var control = testForm.CustomsBrokerageUserControl.InvoiceGroupUserControl;
				control.SetDataBinding(testDec, "");

				control.baseTreeViewUserControl1.TreeView.SelectedNode = (JobComInvoiceHeaderTreeNode)control.baseTreeViewUserControl1.TreeView.GetTreeNodeForBusinessObject(subGroup);
				AssertEquals("The invoice name field should show SubGroup. SwapGroup() should trigger FireListResetEvent()", "SubGroup", control.JZ_InvoiceNumberBoundGroupTextBox1.Text);

				control.baseTreeViewUserControl1.TreeView.SelectedNode = (JobComInvoiceHeaderTreeNode)control.baseTreeViewUserControl1.TreeView.GetTreeNodeForBusinessObject(topGroup);
				AssertEquals("The invoice name field should show All Invoices. SwapGroup() should trigger FireListResetEvent()", "All Invoices", control.JZ_InvoiceNumberBoundGroupTextBox1.Text);
			}
		}

		[TestDate(2016, 09, 20)]
		public void TestCoveringLavelVisibility()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			using (var testForm = new BaseJobDeclarationForm(testDec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration = testDec;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceGroupingTabPage;

				var control = testForm.CustomsBrokerageUserControl.InvoiceGroupUserControl;
				control.SetDataBinding(testDec, "");
				control.baseTreeViewUserControl1.TreeView.SelectedNode = (JobComInvoiceHeaderTreeNode)control.baseTreeViewUserControl1.TreeView.GetTreeNodeForBusinessObject(invoice);
				AssertEquals("Invoice is selected and covering label should be visible", true, control.CoveringLabel.Visible);
				AssertEquals("CoveringLabel brought to front", true, control.coveringLabelBroughtToFrontTestingOnly);

				control.baseTreeViewUserControl1.TreeView.SelectedNode = (JobComInvoiceHeaderTreeNode)control.baseTreeViewUserControl1.TreeView.GetTreeNodeForBusinessObject(testDec.JobComInvoiceGroupHeaders[0]);
				AssertEquals("Group Invoice is selected and covering label should be invisible", false, control.CoveringLabel.Visible);
			}
		}

		[ExpectNoExceptions()]
		public void TestControlInstantiation()
		{
			using (var control = new BaseInvoiceGroupingUserControl())
			{
			}
		}
	}
}
