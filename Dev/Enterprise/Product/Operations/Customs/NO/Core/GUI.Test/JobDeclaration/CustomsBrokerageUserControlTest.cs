using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NO.GUI.Testing
{
	sealed class CustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestEntryInstructionsTabVisibleForCountry()
		{
			var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
			AssertEquals(true, brokerageControl.EntryInstructionsTabVisibleForCountry);
		}

		public void TestTabVisibleForInvoiceGroup_Export()
		{
			var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("'Inv. Group' tab for import should not be visible", false, brokerageControl.InvoiceGroupingTabPage.TabVisible);
		}

		public void TestTabVisibleForInvoiceGroup_Import()
		{
			var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("'Inv. Group' tab for export should not be visible", false, brokerageControl.InvoiceGroupingTabPage.TabVisible);
		}

		public void TestEntryInstructionUserControl()
		{
			using (var testForm = new JobDeclarationForm(declaration))
			{
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				brokerageControl.EntryInstructionDetailsTabPage.Bind();
				AssertType<EntryInstructionDetailsUserControl>(brokerageControl.CustomsEntryInstructionUserControl);
			}
		}

		public void TestCustomsEntryUserControl()
		{
			using (var testForm = new JobDeclarationForm(declaration))
			{
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				brokerageControl.MessagesTabPage.Bind();
				AssertType<MessageUserControl>(brokerageControl.MessageUserControl);
			}
		}

		public void TestInvoiceLineUserControl_Export()
		{
			declaration.JE_MessageType = Core.Constants.FreightShipmentDirection.Code.Export;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				brokerageControl.InvoiceLinesTabPage.Bind();
				AssertType<ExportInvoiceLineUserControl>(brokerageControl.InvoiceLinesUserControl);
			}
		}

		public void TestInvoiceLineUserControl_Import()
		{
			declaration.JE_MessageType = Core.Constants.FreightShipmentDirection.Code.Import;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				brokerageControl.InvoiceLinesTabPage.Bind();
				AssertType<ImportInvoiceLineUserControl>(brokerageControl.InvoiceLinesUserControl);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<Business.JobDeclaration>();
			form = new JobDeclarationForm(declaration);
		}
		Business.JobDeclaration declaration;
		JobDeclarationForm form;

		protected override void TearDown()
		{
			base.TearDown();
			form?.Dispose();
		}
	}
}
