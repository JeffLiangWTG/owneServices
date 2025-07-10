using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public static class InvoiceLineFilterForDeclaration
	{
		public static ZQuery GetFilter(BaseJobDeclaration declaration)
		{
			//Do NOT use JobDeclaration.Invoices 
			//If this collection's Load() happens while JobDeclaration.Invoices is being loaded
			//only the invoice lines that belong to the invoices that have been loaded so far
			//are loaded. We want two Load() to be independent.

			var result = new ZQuery();

			var invoiceLineQuery = CreateLoadInvoiceLinesQuery(new BaseJobComInvoiceHeader.Loader(declaration.Factory).GetQuery(declaration), true);
			result.AddToFilter(invoiceLineQuery, JoinCondition.Or);

			if (declaration.SupportAdditionalInvoices)
			{
				var pivots = declaration.Factory.Load<InvoiceRelatedDeclarationGenPivot>(new InvoiceRelatedDeclarationGenPivot.Loader(declaration.Factory).GetQuery(declaration));
				var additionalInvoiceLineQuery = CreateLoadInvoiceLinesQuery(new ZQuery(JobComInvoiceHeaderSchema.PK, pivots.Select(p => p.XX_Relation1ID)), false);
				result.AddToFilter(additionalInvoiceLineQuery, JoinCondition.Or);
			}

			result.IsNoResultQuery = result.IsEmpty;

			return result;

			ZQuery CreateLoadInvoiceLinesQuery(ZQuery invoiceQuery, bool loadWithClusterKey)
			{
				var query = new ZQuery();
				var invoices = declaration.LoadInvoicesFromQuery(invoiceQuery);

				if (invoices.Any())
				{
					var invoicePKs = new List<ZGuid>();
					foreach (var invoice in invoices)
					{
						invoicePKs.Add(invoice.PK);
						invoice.AddFetchForLoadingInvoiceLines();
					}

					if (loadWithClusterKey)
					{
						query.AddToFilter(JobComInvoiceLineSchema.JI_ClusterKey, declaration.JE_ClusterKey);
					}
					query.AddToFilter(JobComInvoiceLineSchema.JI_JZ, invoicePKs);
				}
				return query;
			}
		}
	}
}
