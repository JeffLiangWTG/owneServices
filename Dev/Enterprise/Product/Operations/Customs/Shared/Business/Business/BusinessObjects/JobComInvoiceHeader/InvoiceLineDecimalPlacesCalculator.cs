using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	class InvoiceLineDecimalPlacesCalculator
	{
		public InvoiceLineDecimalPlacesCalculator(BaseJobComInvoiceHeader invoice)
		{
			this.invoice = invoice;
			this.factory = invoice.Factory;
		}

		readonly BaseJobComInvoiceHeader invoice;
		readonly BusinessObjectFactory factory;

		internal int GetInvoiceLineMaxDecimalPlaces(string fieldName)
		{
			int result;
			if (!InvoiceLineDecimalPlaces.TryGetValue(fieldName, out result) || result < 0)
			{
				result = 2;
			}
			return result;
		}

		Dictionary<string, int> InvoiceLineDecimalPlaces
		{
			get
			{
				if (invoiceLineDecimalPlacesCached == null)
				{
					invoiceLineDecimalPlacesCached = new CachedProperty<Dictionary<string, int>>(factory, delegate
					{
						Dictionary<string, int> result = new Dictionary<string, int>();
						int? customsQtyMetaData = null, invoiceQtyMetaData = null, netWeightMetaData = null, weightMetaData = null, volumeMetaData = null;
						int maxCustomsQtyDec = -1, maxInvoiceQtyDec = -1, maxNetWeightDec = -1, maxWeightDec = -1, maxVolumeDec = -1;

						foreach (BaseJobComInvoiceLine invoiceLine in invoice.JobComInvoiceLines)
						{
							SetDecimalPlacesMetaData(invoiceLine, JobComInvoiceLineSchema.JI_CustomsQuantity.Name, ref customsQtyMetaData);
							SetDecimalPlacesMetaData(invoiceLine, JobComInvoiceLineSchema.JI_InvoiceQuantity.Name, ref invoiceQtyMetaData);
							SetDecimalPlacesMetaData(invoiceLine, JobComInvoiceLineSchema.JI_NetWeight.Name, ref netWeightMetaData);
							SetDecimalPlacesMetaData(invoiceLine, JobComInvoiceLineSchema.JI_Weight.Name, ref weightMetaData);
							SetDecimalPlacesMetaData(invoiceLine, JobComInvoiceLineSchema.JI_Volume.Name, ref volumeMetaData);

							SetMaxDecimalPlaces(invoiceLine, JobComInvoiceLineSchema.JI_CustomsQuantity.Name, customsQtyMetaData.Value, ref maxCustomsQtyDec);
							SetMaxDecimalPlaces(invoiceLine, JobComInvoiceLineSchema.JI_InvoiceQuantity.Name, invoiceQtyMetaData.Value, ref maxInvoiceQtyDec);
							SetMaxDecimalPlaces(invoiceLine, JobComInvoiceLineSchema.JI_NetWeight.Name, netWeightMetaData.Value, ref maxNetWeightDec);
							SetMaxDecimalPlaces(invoiceLine, JobComInvoiceLineSchema.JI_Weight.Name, weightMetaData.Value, ref maxWeightDec);
							SetMaxDecimalPlaces(invoiceLine, JobComInvoiceLineSchema.JI_Volume.Name, volumeMetaData.Value, ref maxVolumeDec);
						}

						result.Add(JobComInvoiceLineSchema.JI_CustomsQuantity.Name, maxCustomsQtyDec);
						result.Add(JobComInvoiceLineSchema.JI_InvoiceQuantity.Name, maxInvoiceQtyDec);
						result.Add(JobComInvoiceLineSchema.JI_NetWeight.Name, maxNetWeightDec);
						result.Add(JobComInvoiceLineSchema.JI_Weight.Name, maxWeightDec);
						result.Add(JobComInvoiceLineSchema.JI_Volume.Name, maxVolumeDec);

						return result;
					});
				}
				return invoiceLineDecimalPlacesCached.Value;
			}
		}

		CachedProperty<Dictionary<string, int>> invoiceLineDecimalPlacesCached;

		void SetMaxDecimalPlaces(BaseJobComInvoiceLine invoiceLine, string invoiceLinePropertyName, int metaData, ref int maxDecimalPlaces)
		{
			var value = (ZDecimal)invoiceLine[invoiceLinePropertyName];
			var decimalPlaces = value.DecimalPlaces;

			var result = Math.Min(decimalPlaces, metaData);
			maxDecimalPlaces = Math.Max(maxDecimalPlaces, result);
		}

		void SetDecimalPlacesMetaData(BaseJobComInvoiceLine invoiceLine, string invoiceLinePropertyName, ref int? fieldDecimalPlaces)
		{
			if (!fieldDecimalPlaces.HasValue)
			{
				fieldDecimalPlaces = invoiceLine.GetDecimalPlacesMetaData(invoiceLinePropertyName);
			}
		}
	}
}
