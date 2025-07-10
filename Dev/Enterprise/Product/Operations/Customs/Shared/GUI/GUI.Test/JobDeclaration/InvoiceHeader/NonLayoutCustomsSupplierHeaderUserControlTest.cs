using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class NonLayoutCustomsSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestLockingShipmentData_IsCancelled()
		{
			var jobDecBizObj = BaseJobDeclaration.New(Factory);
			jobDecBizObj.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDecBizObj.JE_IsCancelled = true;
			jobDecBizObj.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			using (var testForm = new DeclarationFormForTesting(jobDecBizObj))
			{
				testForm.Show();

				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = "IMP";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				var supplierHeaderControl = testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl as NonLayoutCustomsSupplierHeaderUserControl;

				var decUserControl = testForm.DeclarationUserControl as BaseCustomsDeclarationUserControl;
				AssertEquals("JZ_InvoiceNumberBoundTextBox ReadOnly", true, supplierHeaderControl.JZ_InvoiceNumberBoundTextBox.ReadOnly);
				AssertEquals("JZ_IncoTermBoundDropDownEdit ReadOnly", true, supplierHeaderControl.JZ_IncoTermBoundDropDownEdit.ReadOnly);
			}
		}

		public void TestLockingShipmentData_NotCancelled()
		{
			var jobDecBizObj = BaseJobDeclaration.New(Factory);
			jobDecBizObj.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDecBizObj.JE_IsCancelled = false;
			jobDecBizObj.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			using (var testForm = new DeclarationFormForTesting(jobDecBizObj))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;

				var supplierHeaderControl = testForm.CustomsBrokerageUserControl.SupplierHeaderUserControl as NonLayoutCustomsSupplierHeaderUserControl;
				AssertEquals("JZ_InvoiceNumberBoundTextBox ReadOnly", false, supplierHeaderControl.JZ_InvoiceNumberBoundTextBox.ReadOnly);
				AssertEquals("JZ_IncoTermBoundDropDownEdit ReadOnly", false, supplierHeaderControl.JZ_IncoTermBoundDropDownEdit.ReadOnly);
			}
		}

		sealed class DeclarationFormForTesting : BaseJobDeclarationForm
		{
			public DeclarationFormForTesting(BaseJobDeclaration jobDeclaration) : base(jobDeclaration) { }

			public BaseCustomsEntryUserControl DeclarationUserControl => CustomsBrokerageUserControl.DeclarationUserControlForTesting;

			public BaseCustomsSupplierHeaderUserControl SupplierUserControl => CustomsBrokerageUserControl.SupplierHeaderUserControl;

			protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new BrokerageUserControlForTesting();
		}

		sealed class BrokerageUserControlForTesting : BaseCustomsBrokerageUserControl
		{
			protected override BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl() => new NonLayoutCustomsSupplierHeaderUserControl();
		}
	}
}
