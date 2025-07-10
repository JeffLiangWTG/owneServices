using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	class TWInvoiceLineDecimalPlacesCalculator
	{
		public TWInvoiceLineDecimalPlacesCalculator(JobComInvoiceHeader invoice)
		{
			this.invoice = invoice;
			this.factory = invoice.Factory;
		}

		readonly JobComInvoiceHeader invoice;
		readonly BusinessObjectFactory factory;

		internal int GetTWInvoiceLineMaxDecimalPlaces(string fieldName)
		{
			int result;
			if (!TWInvoiceLineDecimalPlaces.TryGetValue(fieldName, out result) || result < 0)
			{
				result = 2;
			}
			return result;
		}

		Dictionary<string, int> TWInvoiceLineDecimalPlaces => this.factory.GetValue(ref invoiceLineDecimalPlacesCached, delegate
					{
						Dictionary<string, int> result = new Dictionary<string, int>();
						int? documentaryQtyMetaData = null;
						int maxDocumentaryQtyDec = -1;

						foreach (JobComInvoiceLine invoiceLine in invoice.JobComInvoiceLines)
						{
							var addInfoChild = invoiceLine.AddInfoChild;
							SetDecimalPlacesMetaData(addInfoChild, JobTWComInvoiceLineSchema.TWL_DocumentaryQty.Name, ref documentaryQtyMetaData);
							SetMaxDecimalPlaces(addInfoChild, JobTWComInvoiceLineSchema.TWL_DocumentaryQty.Name, documentaryQtyMetaData.Value, ref maxDocumentaryQtyDec);
						}

						result.Add(JobTWComInvoiceLineSchema.TWL_DocumentaryQty.Name, maxDocumentaryQtyDec);
						return result;
					});

		CachedProperty<Dictionary<string, int>> invoiceLineDecimalPlacesCached;

		void SetMaxDecimalPlaces(JobTWComInvoiceLine tWInvoiceLine, string tWInvoiceLinePropertyName, int metaData, ref int maxDecimalPlaces)
		{
			var value = (ZDecimal)tWInvoiceLine[tWInvoiceLinePropertyName];
			var decimalPlaces = value.DecimalPlaces;

			var result = Math.Min(decimalPlaces, metaData);
			maxDecimalPlaces = Math.Max(maxDecimalPlaces, result);
		}

		void SetDecimalPlacesMetaData(JobTWComInvoiceLine tWInvoiceLine, string tWInvoiceLinePropertyName, ref int? fieldDecimalPlaces)
		{
			if (!fieldDecimalPlaces.HasValue)
			{
				fieldDecimalPlaces = tWInvoiceLine.GetDecimalPlacesMetaData(tWInvoiceLinePropertyName);
			}
		}
	}
}
