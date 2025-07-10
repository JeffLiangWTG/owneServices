using Enterprise.Customs.Common;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class InvoiceHeaderUserControlTest : Customs.GUI.Testing.InvoiceHeaderUserControlAbstractTest
	{
		public void TestResetColumnsInInvoiceChargesGrid()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var declaration = (JobDeclaration)new Customs.Business.FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData;
			using (var form = new ZForm(declaration))
			{
				var control = GetNewInvoiceHeaderUserControl();
				control.Invoice = invoiceHeader;
				form.Controls.Add(control);
				control.SetDataBinding(form.BusinessEntity, "");
				form.Show();
				var invoiceChargesGrid = control.FindSingle<ZGrid>(c => c.Name == "InvoiceChargesGrid");
				AssertEquals(true, invoiceChargesGrid.GetColumnStyle(AutoJobComInvHeaderCharge.Schema.J7_ChargeDescription).IsVisible);
				AssertEquals(true, invoiceChargesGrid.GetColumnStyle(AutoJobComInvHeaderCharge.Schema.J7_ChargeDescription).IsVisible);
				AssertEquals(true, invoiceChargesGrid.GetColumnStyle(AutoJobComInvHeaderCharge.Schema.J7_Percentage).IsVisible);
				AssertNull(invoiceChargesGrid.Columns[JobComInvCharge.Schema.ChargeCodeDescription]);
			}
		}

		public void TestIsGSTApplicableColumnStyle()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var declaration = (JobDeclaration)new Customs.Business.FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData;
			using (var form = new ZForm(declaration))
			{
				var control = GetNewInvoiceHeaderUserControl();
				control.Invoice = invoiceHeader;
				form.Controls.Add(control);
				control.SetDataBinding(form.BusinessEntity, "");
				form.Show();
				var invoiceChargesGrid = control.FindSingle<ZGrid>(c => c.Name == "InvoiceChargesGrid");
				var columnStyle = invoiceChargesGrid.GetColumnStyle(AutoJobComInvHeaderCharge.Schema.J7_IsGSTApplicable);
				AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(157), columnStyle.Width);
				AssertEquals("Incl. in Total Inv. Amt. (16)", columnStyle.CaptionResourceString.ShortCaption);
				AssertEquals("Included in Declaration Total Invoice Amount (16)", columnStyle.CaptionResourceString.Caption);
				invoiceHeader.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				invoiceHeader.JZ_IncoTerm = "FOB";
				Assert(columnStyle.IsVisible);
				invoiceHeader.JZ_IncoTerm = "CIF";
				Assert(columnStyle.IsVisible);
				invoiceHeader.JZ_IncoTerm = "CFR";
				Assert(columnStyle.IsVisible);
				invoiceHeader.JZ_IncoTerm = "FAS";
				Assert(columnStyle.IsVisible);
				invoiceHeader.JZ_IncoTerm = "EXW";
				Assert(!columnStyle.IsVisible);
				invoiceHeader.JZ_IncoTerm = "C&I";
				Assert(columnStyle.IsVisible);
				invoiceHeader.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				Assert(!columnStyle.IsVisible);
			}
		}

		public void TestBindingMembers()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var declaration = (JobDeclaration)new Customs.Business.FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData;
			using (var form = new ZForm(declaration))
			{
				var control = GetNewInvoiceHeaderUserControl();
				control.Invoice = invoiceHeader;
				form.Controls.Add(control);
				control.SetDataBinding(form.BusinessEntity, "");
				form.Show();
				var bingdingSource = control.BindingSource;
				var marksAndNumbersLongTextBox = control.FindSingle<LongTextControl>(c => c.Name == "TW_MarksAndNumbersLongTextBox");
				AssertEquals("BindingMember", "Invoices.TW_MarksAndNumbers", bingdingSource.GetBindingMember(marksAndNumbersLongTextBox));
			}
		}

		public void TestIncoTermExplainButtonVisible()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var declaration = (JobDeclaration)new Customs.Business.FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData;
			using (var form = new ZForm(declaration))
			{
				var control = GetNewInvoiceHeaderUserControl();
				control.Invoice = invoiceHeader;
				form.Controls.Add(control);
				control.SetDataBinding(form.BusinessEntity, "");
				form.Show();
				var incoTermExplainButton = control.FindSingleOrDefault<ZButton>(c => c.Name == "IncoTermExplainButton");
				AssertEquals("Do not display IncoTermExplainButton in TW Customs.", false, incoTermExplainButton.Visible);
			}
		}

		public void TestSupplierDocumentaryAddressControl()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var declaration = (JobDeclaration)new Customs.Business.FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData;
			using (var form = new ZForm(declaration))
			{
				var control = GetNewInvoiceHeaderUserControl();
				control.Invoice = invoiceHeader;
				form.Controls.Add(control);
				control.SetDataBinding(form.BusinessEntity, "");
				form.Show();
				var supplierDocumentaryAddressControl = control.FindSingleOrDefault<TWJobDocAddressControl>(c => c.Name == "SupplierDocumentaryAddressControl");
				AssertEquals("SupplierDocumentaryAddressControl should be displayed.", true, supplierDocumentaryAddressControl.Visible);

				var supplierOrganisationControl = control.FindSingleOrDefault<ZOrganisationControl>(c => c.Name == "SupplierOrganisationControl");
				AssertEquals("SupplierOrganisationControl should not be displayed.", false, supplierOrganisationControl.Visible);
			}
		}

		public void TestBuyerDocumentaryAddressControl()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var declaration = (JobDeclaration)new Customs.Business.FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData;
			using (var form = new ZForm(declaration))
			{
				var control = GetNewInvoiceHeaderUserControl();
				control.Invoice = invoiceHeader;
				form.Controls.Add(control);
				control.SetDataBinding(form.BusinessEntity, "");
				form.Show();
				var buyerDocumentaryAddressControl = control.FindSingleOrDefault<TWJobDocAddressControl>(c => c.Name == "BuyerDocumentaryAddressControl");
				AssertEquals("BuyerDocumentaryAddressControl should be displayed.", true, buyerDocumentaryAddressControl.Visible);

				var importerOrganisationControl = control.FindSingleOrDefault<ZOrganisationControl>(c => c.Name == "ImporterOrganisationControl");
				AssertEquals("ImporterOrganisationControl should not be displayed.", false, importerOrganisationControl.Visible);
			}
		}

		public void TestJZ_LetterOfCreditNumberTextBox()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var declaration = (JobDeclaration)new Customs.Business.FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData;
			using (var form = new ZForm(declaration))
			{
				var control = GetNewInvoiceHeaderUserControl();
				control.Invoice = invoiceHeader;
				form.Controls.Add(control);
				control.SetDataBinding(form.BusinessEntity, "");
				form.Show();
				var bingdingSource = control.BindingSource;
				var letterOfCreditNumberTextBox = control.FindSingle<ZTextBox>(c => c.Name == "JZ_LetterOfCreditNumberTextBox");
				AssertEquals("BindingMember", "Invoices.JZ_LetterOfCreditNumber", bingdingSource.GetBindingMember(letterOfCreditNumberTextBox));
			}
		}

		public void TestJZ_LetterOfCreditDateDateEdit()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var declaration = (JobDeclaration)new Customs.Business.FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData;
			using (var form = new ZForm(declaration))
			{
				var control = GetNewInvoiceHeaderUserControl();
				control.Invoice = invoiceHeader;
				form.Controls.Add(control);
				control.SetDataBinding(form.BusinessEntity, "");
				form.Show();
				var bingdingSource = control.BindingSource;
				var letterOfCreditDateDateEdit = control.FindSingle<ZDateEdit>(c => c.Name == "JZ_LetterOfCreditDateDateEdit");
				AssertEquals("BindingMember", "Invoices.JZ_LetterOfCreditDate", bingdingSource.GetBindingMember(letterOfCreditDateDateEdit));
			}
		}

		protected override CommonInvoiceHeaderUserControl GetNewInvoiceHeaderUserControl() => new InvoiceHeaderUserControl();
	}
}
