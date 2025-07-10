using System.Collections;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class SupplierROOCalculator
	{
		public SupplierROOCalculator(Customs.Business.InvoiceHeaderActiveCollection invoices)
		{
			this.invoices = invoices;
		}

		public const decimal MinimumRandForDeclaration = 49020m;
		readonly Customs.Business.InvoiceHeaderActiveCollection invoices;

		public OrgHeader[] Execute()
		{
			Hashtable result = new Hashtable();
			for (int i = 0; i < invoices.Count; i++)
			{
				Customs.Business.BaseJobComInvoiceHeader invoice = invoices[i];
				if (invoice.Supplier != null)
				{
					object currentValue = result[invoice.Supplier];
					decimal oldAmount = currentValue != null ? (decimal)currentValue : 0m;
					Money invoiceTotal = new Money(invoice.JZ_InvoiceAmount, invoice.Invoice_Currency);
					result[invoice.Supplier] = oldAmount + invoice.CurrencyConverter.ConvertExact(invoiceTotal, JobDeclaration.GetLocalCurrency()).Amount;
				}
			}

			ArrayList tempArray = new ArrayList();
			foreach (DictionaryEntry entry in result)
			{
				if ((decimal)entry.Value >= MinimumRandForDeclaration)
				{
					tempArray.Add(entry.Key);
				}
			}
			return (OrgHeader[])tempArray.ToArray(typeof(OrgHeader));
		}
	}
}
