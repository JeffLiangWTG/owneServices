using System;
using System.Linq;
using System.Reflection;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceController))]
	sealed class CommercialInvoiceControllerConcreteTest : CommercialInvoiceControllerTest
	{
		public void TestSelectInvoiceNoNullObjectException()
		{
			var fwd = Factory.New<ForwardingShipment>();
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_JS = fwd.PK;
			var invoice = dec.Invoices.AddNew();
			Factory.Save();
			using (var form = Controller.ShowEditForm(invoice))
			{
				var brokerageControl = ((ZForm)form).FindSingle<BaseCustomsBrokerageUserControl>();
				var invoicesControl = brokerageControl.InvoicesTabPage.FindSingle<BaseCustomsSupplierHeaderUserControl>();
				var invoiceHeadersBoundGrid = invoicesControl.InvoiceHeadersBoundGrid;
				var selectedInvoice = (BaseJobComInvoiceHeader)invoiceHeadersBoundGrid.SelectedElements.SingleOrDefault();
				AssertEquals(invoice.PK, selectedInvoice.PK);
				invoiceHeadersBoundGrid.DataBindings.Clear();
				var selectInvoiceMethod = typeof(CommercialInvoiceController).GetMethod("SelectInvoice", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { form.GetType(), typeof(BaseJobComInvoiceHeader) }, null);
				AssertNoExceptionThrown(() =>
				{
					selectInvoiceMethod.Invoke(Controller, new object[] { form, null });
				});
			}
		}

		public void TestOpenDeclaration()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var invoice1 = dec.Invoices.AddNew();
			var invoice2 = dec.Invoices.AddNew();
			Factory.Save();
			using (var form1 = Controller.ShowEditForm(invoice1))
			{
				Assert(form1 is BaseJobDeclarationForm);
				AssertEquals(dec.PK, ((BaseJobDeclarationForm)form1).Declaration.PK);
				AssertInvoiceSelected(form1, invoice1);
				using (var form2 = Controller.ShowEditForm(invoice2))
				{
					AssertSame("Same parent form is open.", form1, form2);
					AssertInvoiceSelected(form2, invoice2);
				}
			}
		}

		public void TestOpenShipment()
		{
			var fwd = Factory.New<ForwardingShipment>();
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_JS = fwd.PK;
			var invoice1 = dec.Invoices.AddNew();
			var invoice2 = dec.Invoices.AddNew();
			Factory.Save();
			using (var form1 = Controller.ShowEditForm(invoice1))
			{
				Assert(form1 is Freight.Forwarding.GUI.ShipmentForm);
				Assert(form1.BusinessEntityForPersistingForm is ForwardingShipment);
				AssertInvoiceSelected(form1, invoice1);
				using (var form2 = Controller.ShowEditForm(invoice2))
				{
					AssertSame("Same parent form is open.", form1, form2);
					AssertInvoiceSelected(form2, invoice2);
				}
			}
		}

		public void TestDeleteDeclaration()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var invoice1 = dec.Invoices.AddNew();
			var invoice2 = dec.Invoices.AddNew();
			Factory.Save();
			using (var form1 = Controller.ShowDeleteForm(invoice1))
			{
				Assert(form1 is BaseJobDeclarationForm);
				var formInvoice = form1.BusinessEntityForPersistingForm.Factory.Load<BaseJobComInvoiceHeader>(invoice1.PK);
				AssertEquals("invoice is not deleted automatically", false, formInvoice.IsDeleted);
				AssertInvoiceSelected(form1, invoice1);
				using (var form2 = Controller.ShowDeleteForm(invoice2))
				{
					AssertSame("Same parent form is open.", form1, form2);
					AssertInvoiceSelected(form2, invoice2);
				}
			}
		}

		public void TestDeleteShipment()
		{
			var fwd = Factory.New<ForwardingShipment>();
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_JS = fwd.PK;
			var invoice1 = dec.Invoices.AddNew();
			var invoice2 = dec.Invoices.AddNew();
			Factory.Save();
			using (var form1 = Controller.ShowDeleteForm(invoice1))
			{
				Assert(form1 is Freight.Forwarding.GUI.ShipmentForm);
				var formInvoice = form1.BusinessEntityForPersistingForm.Factory.Load<BaseJobComInvoiceHeader>(invoice1.PK);
				AssertEquals("invoice is not deleted automatically", false, formInvoice.IsDeleted);
				AssertInvoiceSelected(form1, invoice1);
				using (var form2 = Controller.ShowDeleteForm(invoice2))
				{
					AssertSame("Same parent form is open.", form1, form2);
					AssertInvoiceSelected(form2, invoice2);
				}
			}
		}

		void AssertInvoiceSelected(IZForm form, BaseJobComInvoiceHeader invoice)
		{
			var brokerageControl = ((ZForm)form).FindSingle<BaseCustomsBrokerageUserControl>();
			AssertEquals(true, brokerageControl.Visible);
			var invoicesTabPage = brokerageControl.InvoicesTabPage;
			AssertEquals(invoicesTabPage, brokerageControl.MainTabControl.SelectedTab);
			var invoicesControl = invoicesTabPage.FindSingle<BaseCustomsSupplierHeaderUserControl>();
			var selectedInvoice = (BaseJobComInvoiceHeader)invoicesControl.InvoiceHeadersBoundGrid.SelectedElements.SingleOrDefault();
			AssertEquals(invoice.PK, selectedInvoice?.PK);
		}

		protected override BaseJobComInvoiceHeader GetNewInvoiceHeader()
		{
			return Factory.New<BaseJobComInvoiceHeader>();
		}

		protected override Type ExpectedFormType
		{
			get
			{
				return typeof(CommercialInvoiceForm);
			}
		}
	}
}
