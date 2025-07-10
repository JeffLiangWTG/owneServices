using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Data.Testing
{
	public class NZInvoicesGeneratorTest : InvoicesGeneratorFromXSDTest
	{
		protected override void AssertGrpInvoices(BaseJobDeclaration declaration)
		{
			AssertEquals("Invoice Group Header", 1, declaration.JobComInvoiceGroupHeaders.Count);
			AssertContainInvoice(declaration.JobComInvoiceGroupHeaders, "All Invoices");
			AssertEquals("Invoice Group sub Header", 1, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.Count);
			AssertContainInvoice(declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders, "");
		}

		void AssertContainInvoice(IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> invoices, ZString invoiceNumber)
		{
			bool invoiceExist = false;

			foreach (BaseJobComInvoiceGroupHeader invoice in invoices)
			{
				if (invoice.JZ_InvoiceNumber == invoiceNumber)
				{
					invoiceExist = true;
					break;
				}
			}

			Assert(invoiceExist);
		}

		protected override Type ExpectedInvoiceAdapterType
		{
			get { return typeof(NZInvoiceValueObjectDataAdapter); }
		}

		protected override InvoicesGeneratorFromXSD Generator
		{
			get
			{
				if (fGenerator == null)
				{
					fGenerator = new NZInvoicesGeneratorFromXSD(declaration);
				}

				return fGenerator;
			}
		}
		NZInvoicesGeneratorFromXSD fGenerator;

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			base.SetUp();
		}
	}
}
