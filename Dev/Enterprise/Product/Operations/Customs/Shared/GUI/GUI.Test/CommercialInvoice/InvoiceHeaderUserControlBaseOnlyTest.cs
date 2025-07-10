using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class InvoiceHeaderUserControlBaseOnlyTest : InvoiceHeaderUserControlAbstractTest
	{
		public void TestInvCustomFieldsUserControlIsBindToInvoice()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var declaration = (BaseJobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			using (var form = new ZForm(declaration))
			{
				var testControl = new TestUserControl();
				testControl.Invoice = invoice;
				form.Controls.Add(testControl);
				form.Show();
				var invCustomFieldsUserControl = testControl.FindSingle<InvoiceHeaderCustomFieldsUserControl>("InvCustomFieldsUserControl");
				IDataBoundControl dataBoundControl = invCustomFieldsUserControl;
				AssertEquals("DataMember", "Invoices", dataBoundControl.DataMember);
				AssertSame("DataSource", declaration, dataBoundControl.DataSource);
				AssertSame("CurrentDataItem", invoice, invCustomFieldsUserControl.CurrentDataItem);
			}
		}

		public void TestInvoiceChargesGrid_J7_IsIncludedInITOT_ToolTip()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var declaration = (BaseJobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			using (var form = new ZForm(declaration))
			{
				var testControl = new TestUserControl();
				testControl.Invoice = invoice;
				form.Controls.Add(testControl);
				form.Show();
				var invoiceChargesGrid = testControl.InvoiceChargesGrid;
				var checkBoxColumnStyleInfo = (ZCheckBoxColumnStyleInfo)invoiceChargesGrid.GetColumnStyle(BaseInvoiceCharge.Schema.J7_IsIncludedInITOT);
				AssertEquals("ToolTip", "If you tick this flag, it indicates that this charge is included in Lines. It won\'t reduce the ITOT you have to enter", checkBoxColumnStyleInfo.ToolTip);
			}
		}

		public void TestMessageTypeChangesCaptions()
		{
			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			var declaration = (BaseJobDeclaration)new FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData;
			using (var form = new ZForm(declaration))
			{
				var control = GetNewInvoiceHeaderUserControl();
				control.Invoice = invoiceHeader;
				form.Controls.Add(control);
				control.SetDataBinding(form.BusinessEntity, "");
				form.Show();
				invoiceHeader.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				AssertEquals("Column correct heading", "Add to FOB?", control.InvoiceChargesGrid.Columns[JobComInvHeaderChargeSchema.J7_IsDutiable.Name].ColumnStyle.HeaderText);
				AssertEquals("Column correct heading", "Add to CIF?", control.InvoiceChargesGrid.Columns[JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name].ColumnStyle.HeaderText);
				invoiceHeader.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				AssertEquals("Column correct heading", "Dutiable", control.InvoiceChargesGrid.Columns[JobComInvHeaderChargeSchema.J7_IsDutiable.Name].ColumnStyle.HeaderText);
				AssertEquals("Column correct heading", BaseCustomsSupplierHeaderUserControl.IsGSTApplicableCaption, control.InvoiceChargesGrid.Columns[JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name].ColumnStyle.HeaderText);
			}
		}

		public void TestChangeControlsVisibilityWhenMessageTypeChangesIsCalled()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			using (var form = new ZForm((BaseJobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData))
			{
				var testControl = new TestUserControl();
				testControl.Invoice = invoice;
				form.Controls.Add(testControl);
				AssertEquals("Not called yet", 0, testControl.ChangeControlsVisibilityWhenMessageTypeChangesCounter);
				form.Show();
				AssertEquals("Called initially when form opens", 1, testControl.ChangeControlsVisibilityWhenMessageTypeChangesCounter);
				invoice.JZ_MessageType = "ABC";
				AssertEquals("Called when message type changes in biz layer", 2, testControl.ChangeControlsVisibilityWhenMessageTypeChangesCounter);
				invoice.JZ_MessageType = "ABC";
				AssertEquals("Value hasn't changed", 2, testControl.ChangeControlsVisibilityWhenMessageTypeChangesCounter);
				invoice.JZ_InvoiceAmount = 34;
				AssertEquals("Value hasn't changed", 2, testControl.ChangeControlsVisibilityWhenMessageTypeChangesCounter);
				invoice.JZ_MessageType = "ZZZ";
				AssertEquals("Changed again", 3, testControl.ChangeControlsVisibilityWhenMessageTypeChangesCounter);
			}
		}

		protected override CommonInvoiceHeaderUserControl GetNewInvoiceHeaderUserControl() => new CommonInvoiceHeaderUserControl();

		sealed class TestUserControl : CommonInvoiceHeaderUserControl
		{
			public int ChangeControlsVisibilityWhenMessageTypeChangesCounter;
			protected override void ChangeControlsVisibilityWhenMessageTypeChanges()
			{
				ChangeControlsVisibilityWhenMessageTypeChangesCounter++;
				base.ChangeControlsVisibilityWhenMessageTypeChanges();
			}
		}
	}
}
