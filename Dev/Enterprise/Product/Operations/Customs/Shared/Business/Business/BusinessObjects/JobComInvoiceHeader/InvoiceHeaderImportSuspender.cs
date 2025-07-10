using System;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class InvoiceHeaderImportSuspender : IDisposable
	{
		public InvoiceHeaderImportSuspender(BaseJobDeclaration jobDeclaration)
		{
			if (jobDeclaration != null)
			{
				invoiceSuspender = jobDeclaration.GetInvoiceNumberRenumberingSuspender();
				declaration = jobDeclaration;
			}
		}

		readonly IDisposable invoiceSuspender;
		readonly BaseJobDeclaration declaration;
		bool disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					invoiceSuspender?.Dispose();
					if (declaration != null
							&& declaration.Invoices.Cast<BaseJobComInvoiceHeader>().Any(header => header.JZ_InvoiceDisplaySequence == ZShort.Zero))
					{
						declaration.InvoiceNumberGenerator.ReCalculateAll();
					}
				}
				disposed = true;
			}
		}
	}
}

// Tested in InvoiceHeaderActiveCollection
