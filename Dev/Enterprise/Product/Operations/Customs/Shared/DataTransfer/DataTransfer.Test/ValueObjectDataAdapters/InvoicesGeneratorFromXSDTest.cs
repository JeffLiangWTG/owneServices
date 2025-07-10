using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer.Testing
{
	public class InvoicesGeneratorFromXSDTest : TestCaseWithFactory
	{
		public void TestGroupInvoiceAdatper()
		{
			AssertEquals("Expected GroupInvoiceAdapter", ExpectedGroupInvoiceAdapterType, Generator.GroupInvoiceAdapter.GetType());
		}

		public void TestInvoiceAdapter()
		{
			AssertEquals("Expected InvoiceAdapter", ExpectedInvoiceAdapterType, Generator.InvoiceAdapter.GetType());
		}

		public virtual void TestImportInvoicesDetails()
		{
			Xsd.InvoiceHeaderCollection invoices = InvoicesXSD;
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			AssertEquals("Declaration doens't have any commercial invoices", 0, declaration.Invoices.Count);
			Generator.ImportInvoicesDetails(invoices, declaration, context);

			AssertEquals("Declaration has commercial invoices", 1, declaration.Invoices.Count);
			BaseJobComInvoiceHeader invoice = declaration.Invoices[0];
			AssertEquals("Invoice Number", "1", invoice.JZ_InvoiceNumber);
			AssertEquals("Invoice Amount", 100m, invoice.JZ_InvoiceAmount);
			AssertEquals("Invoice is linked to Housebill 1", "HOUSEBILL 1", invoice.Bill.CU_HouseBill);
			AssertEquals("Invoice is linked to Masterbill MB1", "MB1", invoice.Bill.CU_MasterBill);

			AssertGrpInvoices(declaration);
		}

		protected virtual void AssertGrpInvoices(BaseJobDeclaration declaration)
		{
			AssertEquals("Invoice Group Header", 1, declaration.JobComInvoiceGroupHeaders.Count);
			BaseJobComInvoiceGroupHeader grpHeader = declaration.JobComInvoiceGroupHeaders[0];
			AssertEquals("Invoice Number", "All Invoices", grpHeader.JZ_InvoiceNumber);
		}

		protected virtual Type ExpectedGroupInvoiceAdapterType => typeof(GroupInvoiceValueObjectDataAdapter);

		protected virtual Type ExpectedInvoiceAdapterType => typeof(InvoiceValueObjectDataAdapter);

		protected virtual BaseJobDeclaration GetDeclaration()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.JE_MasterBill = "MB1";
			jobDec.JE_HouseBill = "HOUSEBILL 1";

			Bill houseBill = jobDec.Bills.AddNew();
			houseBill.CU_HouseBill = "HOUSEBILL 1";
			houseBill.CU_MasterBill = "MB1";

			return jobDec;
		}

		protected BaseJobDeclaration declaration;

		InvoicesGeneratorFromXSD generator;
		protected virtual InvoicesGeneratorFromXSD Generator => generator ?? (generator = new InvoicesGeneratorFromXSD(declaration));

		protected override void SetUp()
		{
			base.SetUp();
			declaration = GetDeclaration();
		}

		protected Xsd.InvoiceHeaderCollection InvoicesXSD
		{
			get
			{
				Xsd.InvoiceHeaderCollection invoices = new Xsd.InvoiceHeaderCollection();
				Xsd.InvoiceHeader xmlInvoiceHeader = invoices.AddNew();

				xmlInvoiceHeader.InvoiceNumber = "1";
				xmlInvoiceHeader.PackingDetails.Housebill = "HOUSEBILL 1";
				xmlInvoiceHeader.PackingDetails.Masterbill = "MB1";
				xmlInvoiceHeader.InvoiceAmount.Value = 100m;

				Xsd.InvoiceHeader xmlInvoiceHeader2 = invoices.AddNew();
				xmlInvoiceHeader2.InvoiceNumber = "2";
				xmlInvoiceHeader2.IsGroupInvoice = Xsd.TrueFalse.@true;
				xmlInvoiceHeader2.IsGroupInvoiceSpecified = true;

				Xsd.InvoiceHeader xmlInvoiceHeader3 = invoices.AddNew();
				xmlInvoiceHeader3.InvoiceNumber = "3";
				xmlInvoiceHeader3.IsGroupInvoice = Xsd.TrueFalse.@true;
				xmlInvoiceHeader3.IsGroupInvoiceSpecified = true;

				return invoices;
			}
		}
	}
}
