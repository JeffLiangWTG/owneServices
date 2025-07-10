using System.Collections.Generic;
using System.Linq;

using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.Business.DIS
{
	class InvoiceValueProvider
	{
		public InvoiceValueProvider(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		public IEnumerable<ICommercialInvoiceDefault> Invoices
		{
			get
			{
				return
					from JobComInvoiceHeader invoice in declaration.Invoices
					select new InvoiceWrapper(invoice);
			}
		}

		class InvoiceWrapper : ICommercialInvoiceDefault
		{
			public InvoiceWrapper(JobComInvoiceHeader invoice)
			{
				this.invoice = invoice;
			}

			readonly JobComInvoiceHeader invoice;

			public IEnumerable<IDISInvoiceLineDefault> InvoiceLines
			{
				get { return new LinesValueProvider(invoice.JobComInvoiceLines.Cast<JobComInvoiceLine>()).Lines; }
			}

			public ZString InvoiceNumber
			{
				get { return invoice.JZ_InvoiceNumber; }
			}

			public ZString Description
			{
				get
				{
					var result = new ZStringBuilder();
					var supplier = invoice.Supplier_Effective;
					if (supplier != null)
					{
						result.Append("Supplier: " + supplier.OH_Code);
					}

					result.Append("Amount: " + invoice.JZ_InvoiceAmount.ToString(2) + " " + invoice.JZ_RX_NKInvoice_Currency);
					return result.ToStringWithDelimiterBetweenAppends(", ");
				}
			}
		}
	}
}
